using System;
using System.Collections.Generic;
using UnityEngine;
using GuildMaster.Infrastructure.DataProviders;
using GuildMaster.Infrastructure.Serialization;

namespace GuildMaster.Services
{
    [Serializable]
    public class AssetEntry
    {
        public string id;
        public string type;
        public string path;
        public string originalFile;
        public int width;
        public int height;
        public string status;
    }

    [Serializable]
    public class AssetManifestFile
    {
        public List<AssetEntry> data;
    }

    public interface IAssetManifestService
    {
        bool TryGetAssetInfo(string id, out AssetEntry entry);
    }

    public class AssetManifestService : IAssetManifestService
    {
        private readonly Dictionary<string, AssetEntry> _assets = new Dictionary<string, AssetEntry>(StringComparer.OrdinalIgnoreCase);

        public AssetManifestService(IGameDataProvider dataProvider, IJsonSerializer serializer)
        {
            if (dataProvider == null) throw new ArgumentNullException(nameof(dataProvider));
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));

            string filename = "assets_manifest.json";
            if (!dataProvider.Exists(filename))
            {
                Debug.LogWarning($"[AssetManifestService] {filename} not found in provider.");
                return;
            }

            try
            {
                string json = dataProvider.ReadText(filename);
                // JSON is a top-level array, wrap it in a dummy object so JsonUtility can parse it
                string wrappedJson = "{ \"data\": " + json + " }";
                var file = serializer.Deserialize<AssetManifestFile>(wrappedJson);
                if (file != null && file.data != null)
                {
                    foreach (var entry in file.data)
                    {
                        if (!string.IsNullOrEmpty(entry.id))
                        {
                            _assets[entry.id] = entry;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AssetManifestService] Failed to load {filename}: {ex.Message}");
            }
        }

        public bool TryGetAssetInfo(string id, out AssetEntry entry)
        {
            if (string.IsNullOrEmpty(id))
            {
                entry = null;
                return false;
            }
            return _assets.TryGetValue(id, out entry);
        }
    }
}
