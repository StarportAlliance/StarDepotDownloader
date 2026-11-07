// This file is subject to the terms and conditions defined
// in file 'LICENSE', which is part of this source code package.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.IO.IsolatedStorage;
using ProtoBuf;

namespace StarDepotDownloader
{
    [ProtoContract]
    public class AccountSettingsStore
    {
        // Member 1 was a Dictionary<string, byte[]> for SentryData.

        [ProtoMember(2, IsRequired = false)]
        public ConcurrentDictionary<string, int> ContentServerPenalty { get; private set; }

        // Member 3 was a Dictionary<string, string> for LoginKeys.

        [ProtoMember(4, IsRequired = false)]
        public Dictionary<string, string> LoginTokens { get; private set; }

        [ProtoMember(5, IsRequired = false)]
        public Dictionary<string, string> GuardData { get; private set; }

        string FileName;

        AccountSettingsStore()
        {
            ContentServerPenalty = new ConcurrentDictionary<string, int>();
            LoginTokens = new(StringComparer.OrdinalIgnoreCase);
            GuardData = new(StringComparer.OrdinalIgnoreCase);
        }

        static bool Loaded
        {
            get { return Instance != null; }
        }

        public static AccountSettingsStore Instance;
        static readonly IsolatedStorageFile IsolatedStorage = IsolatedStorageFile.GetUserStoreForAssembly();

        // 绝对路径走普通文件，其余沿用旧版 IsolatedStorage，避免破坏 CLI 历史行为
        static bool UsesDirectFile(string filename) => Path.IsPathRooted(filename);

        public static void LoadFromFile(string filename)
        {
            if (Loaded)
                throw new Exception("Config already loaded");

            if (Exists(filename))
            {
                try
                {
                    using var fs = OpenRead(filename);
                    using var ds = new DeflateStream(fs, CompressionMode.Decompress);
                    Instance = ProtobufModel.Instance.Deserialize<AccountSettingsStore>(ds);
                }
                catch (IOException ex)
                {
                    Console.WriteLine("Failed to load account settings: {0}", ex.Message);
                    Instance = new AccountSettingsStore();
                }
            }
            else
            {
                Instance = new AccountSettingsStore();
            }

            Instance.FileName = filename;
        }

        public static void Save()
        {
            if (!Loaded)
                throw new Exception("Saved config before loading");

            try
            {
                using var fs = OpenWrite(Instance.FileName);
                using var ds = new DeflateStream(fs, CompressionMode.Compress);
                ProtobufModel.Instance.Serialize(ds, Instance);
            }
            catch (IOException ex)
            {
                Console.WriteLine("Failed to save account settings: {0}", ex.Message);
            }
        }

        static bool Exists(string filename) =>
            UsesDirectFile(filename) ? File.Exists(filename) : IsolatedStorage.FileExists(filename);

        static Stream OpenRead(string filename) =>
            UsesDirectFile(filename)
                ? File.Open(filename, FileMode.Open, FileAccess.Read)
                : IsolatedStorage.OpenFile(filename, FileMode.Open, FileAccess.Read);

        static Stream OpenWrite(string filename)
        {
            if (!UsesDirectFile(filename))
            {
                return IsolatedStorage.OpenFile(filename, FileMode.Create, FileAccess.Write);
            }

            string directory = Path.GetDirectoryName(filename);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            return File.Open(filename, FileMode.Create, FileAccess.Write);
        }
    }
}
