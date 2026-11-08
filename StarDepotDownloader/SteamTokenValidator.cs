// This file is subject to the terms and conditions defined
// in file 'LICENSE', which is part of this source code package.

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ProtoBuf;
using SteamKit2;

namespace StarDepotDownloader
{
    enum SteamTokenValidationStatus
    {
        Accepted,
        Rejected,
        Unavailable,
    }

    sealed class SteamTokenValidationResult
    {
        public SteamTokenValidationStatus Status { get; init; }

        public EResult Result { get; init; }

        public string RenewedToken { get; init; }

        public string Error { get; init; }
    }

    static class SteamTokenValidator
    {
        const string Endpoint = "https://api.steampowered.com/IAuthenticationService/GenerateAccessTokenForApp/v1/";
        const int RenewalAllowed = 1;

        // 只有这些结果才代表 token 被服务端否定；其余非 OK 一律按暂时不可用处理，避免误删凭据
        static readonly EResult[] RejectedResults =
        [
            EResult.Expired,
            EResult.Revoked,
            EResult.AccessDenied,
            EResult.InvalidPassword,
            EResult.InvalidSignature,
        ];

        public static bool TryReadClaims(string token, out ulong steamId, out DateTimeOffset? expiresAt)
        {
            steamId = 0;
            expiresAt = null;

            string[] parts = token.Split('.');
            if (parts.Length != 3)
                return false;

            string payload = parts[1].Replace('-', '+').Replace('_', '/');
            payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');

            try
            {
                using var document = JsonDocument.Parse(Convert.FromBase64String(payload));
                var root = document.RootElement;

                if (!root.TryGetProperty("sub", out var sub))
                    return false;

                if (!ulong.TryParse(sub.ValueKind == JsonValueKind.String ? sub.GetString() : sub.ToString(), out steamId))
                    return false;

                if (root.TryGetProperty("exp", out var exp) && exp.TryGetInt64(out var seconds))
                    expiresAt = DateTimeOffset.FromUnixTimeSeconds(seconds);

                return true;
            }
            catch (FormatException)
            {
                return false;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        public static async Task<SteamTokenValidationResult> ValidateAsync(ulong steamId, string token)
        {
            var request = new GenerateForAppRequest
            {
                RefreshToken = token,
                SteamId = steamId,
                RenewalType = RenewalAllowed,
            };

            using var payload = new MemoryStream();
            ProtobufModel.Instance.Serialize(payload, request);

            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(Convert.ToBase64String(payload.ToArray())), "input_protobuf_encoded");

            using var http = HttpClientFactory.CreateHttpClient();
            http.Timeout = TimeSpan.FromSeconds(10);

            try
            {
                using var response = await http.PostAsync(Endpoint, content).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                    return Unavailable($"WebAPI returned HTTP {(int)response.StatusCode}.");

                if (!response.Headers.TryGetValues("x-eresult", out var values) || !int.TryParse(values.FirstOrDefault(), out var resultValue))
                    return Unavailable("WebAPI response is missing a valid x-eresult header.");

                var result = (EResult)resultValue;
                if (result != EResult.OK)
                {
                    string message = null;
                    if (response.Headers.TryGetValues("x-error_message", out var messages))
                        message = messages.FirstOrDefault();

                    // 服务端可能只回错误头而不回错误正文
                    string error = message ?? $"EResult {(int)result} ({result})";
                    return new SteamTokenValidationResult
                    {
                        Status = RejectedResults.Contains(result) ? SteamTokenValidationStatus.Rejected : SteamTokenValidationStatus.Unavailable,
                        Result = result,
                        Error = error,
                    };
                }

                byte[] body = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                string renewed = null;
                if (body.Length > 0)
                {
                    using var stream = new MemoryStream(body);
                    renewed = ProtobufModel.Instance.Deserialize<GenerateForAppResponse>(stream).RefreshToken;
                }

                return new SteamTokenValidationResult
                {
                    Status = SteamTokenValidationStatus.Accepted,
                    Result = result,
                    RenewedToken = renewed,
                };
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or IOException)
            {
                return Unavailable(ex.Message);
            }
        }

        static SteamTokenValidationResult Unavailable(string error) =>
            new() { Status = SteamTokenValidationStatus.Unavailable, Error = error };
    }

    [ProtoContract]
    public sealed class GenerateForAppRequest
    {
        [ProtoMember(1)]
        public string RefreshToken { get; set; }

        [ProtoMember(2, DataFormat = DataFormat.FixedSize)]
        public ulong SteamId { get; set; }

        [ProtoMember(3)]
        public int RenewalType { get; set; }
    }

    [ProtoContract]
    public sealed class GenerateForAppResponse
    {
        [ProtoMember(1)]
        public string AccessToken { get; set; }

        [ProtoMember(2)]
        public string RefreshToken { get; set; }
    }
}
