// This file is subject to the terms and conditions defined
// in file 'LICENSE', which is part of this source code package.

using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace StarDepotDownloader
{
    class DownloadConfig
    {
        public int CellID { get; set; }
        public bool DownloadAllPlatforms { get; set; }
        public bool DownloadAllArchs { get; set; }
        public bool DownloadAllLanguages { get; set; }
        public bool DownloadManifestOnly { get; set; }
        public string InstallDirectory { get; set; }

        /// <summary>是否允许读取 stdin（密码 / Steam Guard 验证码）。JSON 模式下必须为 false。</summary>
        public bool Interactive { get; set; } = true;

        public bool UsingFileList { get; set; }
        public HashSet<string> FilesToDownload { get; set; }
        public List<Regex> FilesToDownloadRegex { get; set; }

        public string BetaPassword { get; set; }

        public bool VerifyAll { get; set; }

        public int MaxDownloads { get; set; }

        public bool RememberPassword { get; set; }

        // A Steam LoginID to allow multiple concurrent connections
        public uint? LoginID { get; set; }

        public bool UseQrCode { get; set; }
        public bool SkipAppConfirmation { get; set; }
    }
}
