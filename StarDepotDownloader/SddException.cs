// This file is subject to the terms and conditions defined
// in file 'LICENSE', which is part of this source code package.

using System;

namespace StarDepotDownloader;

/// <summary>带机器可读错误码与进程退出码的失败。</summary>
class SddException(string code, string message, int exitCode) : Exception(message)
{
    public string Code { get; } = code;

    public int ExitCode { get; } = exitCode;
}

static class SddErrorCodes
{
    public const string InvalidArguments = "invalid_arguments";
    public const string AuthRequired = "auth_required";
    public const string AuthFailed = "auth_failed";
    public const string DepotUnavailable = "depot_unavailable";
    public const string DownloadFailed = "download_failed";
    public const string Canceled = "canceled";
    public const string Internal = "internal_error";
}
