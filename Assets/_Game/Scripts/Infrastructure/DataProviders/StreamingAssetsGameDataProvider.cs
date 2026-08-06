using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace GuildMaster.Infrastructure.DataProviders
{
    public class StreamingAssetsGameDataProvider : IGameDataProvider
    {
        private readonly string _rootPath;

        /// <summary>
        /// On Android, StreamingAssets is packed inside the APK and cannot be read via
        /// System.IO. We must use UnityWebRequest. For other platforms, direct File I/O
        /// works fine.
        /// </summary>
        public string ProviderName => "StreamingAssetsGameDataProvider";

        // Known game data file names (Android cannot enumerate inside APK).
        private static readonly string[] KnownFiles = new[]
        {
            "adventurers.json",
            "dungeons.json",
            "enemies.json",
            "items.json",
            "pets.json",
            "quests.json",
            "skills.json",
            "statuses.json",
            "traits.json",
            "upgrades.json"
        };

        public StreamingAssetsGameDataProvider()
        {
            _rootPath = Path.Combine(Application.streamingAssetsPath, "GameData");
        }

        public bool Exists(string relativePath)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            // On Android we cannot check File.Exists inside the APK.
            // Try loading via UnityWebRequest — if it succeeds the file exists.
            string url = Path.Combine(Application.streamingAssetsPath, "GameData", relativePath);
            using (var req = UnityWebRequest.Get(url))
            {
                req.SendWebRequest();
                while (!req.isDone) { } // Sync wait (acceptable during boot)
                return req.result == UnityWebRequest.Result.Success;
            }
#else
            return File.Exists(Path.Combine(_rootPath, relativePath));
#endif
        }

        public string ReadText(string relativePath)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            string url = Path.Combine(Application.streamingAssetsPath, "GameData", relativePath);
            using (var req = UnityWebRequest.Get(url))
            {
                req.SendWebRequest();
                while (!req.isDone) { } // Sync wait (acceptable during boot)
                if (req.result != UnityWebRequest.Result.Success)
                {
                    throw new FileNotFoundException(
                        $"Failed to load from StreamingAssets on Android: {url} — {req.error}");
                }
                return req.downloadHandler.text;
            }
#else
            string fullPath = Path.Combine(_rootPath, relativePath);
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"File not found in StreamingAssets provider: {fullPath}");
            }
            return File.ReadAllText(fullPath);
#endif
        }

        public IEnumerable<string> EnumerateFiles()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            // Android APK does not support directory enumeration.
            // Return the known list of game data files that exist.
            foreach (string fileName in KnownFiles)
            {
                if (Exists(fileName))
                {
                    yield return fileName;
                }
            }
#else
            if (Directory.Exists(_rootPath))
            {
                string[] files = Directory.GetFiles(_rootPath, "*.*", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    yield return file.Substring(_rootPath.Length + 1).Replace("\\", "/");
                }
            }
#endif
        }
    }
}
