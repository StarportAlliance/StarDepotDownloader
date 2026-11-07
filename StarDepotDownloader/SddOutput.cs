// This file is subject to the terms and conditions defined
// in file 'LICENSE', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace StarDepotDownloader;

/// <summary>机器可读事件输出。JSON 模式下 stdout 只写 NDJSON，人类可读文本全部走 stderr。</summary>
static class SddOutput
{
    private const long ProgressIntervalMs = 250;

    private static readonly object Sync = new();
    private static TextWriter _stdout = Console.Out;
    private static long _lastProgressAt;

    public static bool JsonMode { get; private set; }

    public static void Initialize(bool jsonMode)
    {
        JsonMode = jsonMode;
        if (!jsonMode)
        {
            return;
        }

        _stdout = Console.Out;
        Console.SetOut(Console.Error);
    }

    public static void Start(
        uint appId,
        IEnumerable<(uint DepotId, ulong ManifestId)> depots,
        ulong totalBytes
    )
    {
        if (!JsonMode)
        {
            return;
        }

        Emit("start", writer =>
        {
            writer.WriteNumber("app", appId);
            writer.WriteNumber("totalBytes", totalBytes);
            writer.WriteStartArray("depots");
            foreach (var (depotId, manifestId) in depots)
            {
                writer.WriteStartObject();
                writer.WriteNumber("depot", depotId);
                writer.WriteString("manifest", manifestId.ToString());
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        });
    }

    public static void Progress(ulong downloaded, ulong total)
    {
        if (!JsonMode)
        {
            return;
        }

        long now = Environment.TickCount64;
        if (downloaded < total && now - _lastProgressAt < ProgressIntervalMs)
        {
            return;
        }

        _lastProgressAt = now;
        double percent = total == 0 ? 0 : Math.Min(100.0, downloaded * 100.0 / total);

        Emit("progress", writer =>
        {
            writer.WriteNumber("downloaded", downloaded);
            writer.WriteNumber("total", total);
            writer.WriteNumber("percent", Math.Round(percent, 2));
        });
    }

    public static void Auth(string account)
    {
        Emit("auth", writer =>
        {
            if (account is null)
            {
                writer.WriteNull("account");
            }
            else
            {
                writer.WriteString("account", account);
            }
        });
    }

    public static void Qr(IReadOnlyList<BitArray> matrix)
    {
        Emit("qr", writer =>
        {
            writer.WriteNumber("size", matrix.Count);
            writer.WriteStartArray("rows");
            foreach (var row in matrix)
            {
                StringBuilder builder = new(row.Length);
                for (int i = 0; i < row.Length; i++)
                {
                    builder.Append(row[i] ? '1' : '0');
                }
                writer.WriteStringValue(builder.ToString());
            }
            writer.WriteEndArray();
        });
    }

    public static void Status(bool hasCredentials, string account)
    {
        Emit("status", writer =>
        {
            writer.WriteBoolean("hasCredentials", hasCredentials);
            if (account is null)
            {
                writer.WriteNull("account");
            }
            else
            {
                writer.WriteString("account", account);
            }
        });
    }

    public static void Error(string code, string message)
    {
        Emit("error", writer =>
        {
            writer.WriteString("code", code);
            writer.WriteString("message", message);
        });
    }

    public static void Done(ulong bytes)
    {
        Emit("done", writer => writer.WriteNumber("bytes", bytes));
    }

    private static void Emit(string name, Action<Utf8JsonWriter> body)
    {
        if (!JsonMode)
        {
            return;
        }

        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartObject();
            writer.WriteString("event", name);
            body(writer);
            writer.WriteEndObject();
        }

        lock (Sync)
        {
            _stdout.WriteLine(Encoding.UTF8.GetString(buffer.ToArray()));
            _stdout.Flush();
        }
    }
}
