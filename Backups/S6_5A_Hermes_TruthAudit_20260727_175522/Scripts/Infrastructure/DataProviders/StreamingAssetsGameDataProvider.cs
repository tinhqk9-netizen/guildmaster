using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GuildMaster.Infrastructure.DataProviders
{
    public class StreamingAssetsGameDataProvider : IGameDataProvider
    {
        private readonly string _rootPath;

        public string ProviderName => "StreamingAssetsGameDataProvider";

        public StreamingAssetsGameDataProvider()
        {
            _rootPath = Path.Combine(Application.streamingAssetsPath, "GameData");
        }

        public bool Exists(string relativePath)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            // Not supported synchronously on Android via File.Exists
            return false;
#else
            return File.Exists(Path.Combine(_rootPath, relativePath));
#endif
        }

        public string ReadText(string relativePath)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            throw new NotSupportedException("Task S1-002: AndroidWebRequest flow for StreamingAssets not yet implemented.");
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
            throw new NotSupportedException("Task S1-002: EnumerateFiles on Android StreamingAssets not yet implemented.");
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
