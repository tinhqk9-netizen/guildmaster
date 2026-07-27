using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GuildMaster.Infrastructure.DataProviders
{
    public class EditorExternalGameDataProvider : IGameDataProvider
    {
        private readonly string _rootPath;

        public string ProviderName => "EditorExternalGameDataProvider";

        public EditorExternalGameDataProvider(string relativeOrAbsolutePath = null)
        {
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(relativeOrAbsolutePath))
            {
                _rootPath = Path.GetFullPath(relativeOrAbsolutePath);
            }
            else
            {
                // StreamingAssets is the single source of truth: it is what a player build
                // reads, and it is where enriched data (enemy stats, drop tables, dungeon
                // enemy lists) is written. Defaulting to the converter's staging folder meant
                // the editor silently ran on thinner data than the build did.
                string streamingRoot = Path.GetFullPath(Path.Combine(Application.streamingAssetsPath, "GameData"));

                if (File.Exists(Path.Combine(streamingRoot, "manifest.json")))
                {
                    _rootPath = streamingRoot;
                }
                else
                {
                    // Fall back to the converter output so a project without StreamingAssets
                    // data still boots — but say so, because the two can drift apart.
                    _rootPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../../Game Decode Converter/output/production_staging"));
                    Debug.LogWarning("[EditorExternalGameDataProvider] StreamingAssets/GameData has no manifest.json; " +
                                     $"falling back to converter staging at {_rootPath}");
                }
            }

            if (!Directory.Exists(_rootPath))
            {
                Debug.LogWarning($"[EditorExternalGameDataProvider] Root path does not exist: {_rootPath}");
            }
#else
            Debug.LogError("[EditorExternalGameDataProvider] This provider is not supported outside of the Unity Editor!");
            _rootPath = string.Empty;
#endif
        }

        public bool Exists(string relativePath)
        {
#if UNITY_EDITOR
            return File.Exists(Path.Combine(_rootPath, relativePath));
#else
            return false;
#endif
        }

        public string ReadText(string relativePath)
        {
#if UNITY_EDITOR
            string fullPath = Path.Combine(_rootPath, relativePath);
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"File not found in Editor provider: {fullPath}");
            }
            return File.ReadAllText(fullPath);
#else
            throw new NotSupportedException("EditorExternalGameDataProvider is not supported in builds.");
#endif
        }

        public IEnumerable<string> EnumerateFiles()
        {
#if UNITY_EDITOR
            if (Directory.Exists(_rootPath))
            {
                string[] files = Directory.GetFiles(_rootPath, "*.*", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    yield return file.Substring(_rootPath.Length + 1).Replace("\\", "/");
                }
            }
#else
            yield break;
#endif
        }
    }
}
