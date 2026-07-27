using System;
using System.Collections.Generic;
using UnityEngine;
using GuildMaster.Infrastructure.DataProviders;
using GuildMaster.Infrastructure.Serialization;

namespace GuildMaster.Services
{
    [Serializable]
    public class LocalizationEntry
    {
        public string key;
        public string text;
    }

    [Serializable]
    public class LocalizationFile
    {
        public List<LocalizationEntry> data;
    }

    public interface ILocalizationService
    {
        string GetText(string key);
    }

    public class LocalizationService : ILocalizationService
    {
        private readonly Dictionary<string, string> _texts = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public LocalizationService(IGameDataProvider dataProvider, IJsonSerializer serializer)
        {
            if (dataProvider == null) throw new ArgumentNullException(nameof(dataProvider));
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));

            string filename = "localization.json";
            if (!dataProvider.Exists(filename))
            {
                Debug.LogWarning($"[LocalizationService] {filename} not found in provider.");
                return;
            }

            try
            {
                string json = dataProvider.ReadText(filename);
                // JSON is a top-level array, wrap it in a dummy object so JsonUtility can parse it
                string wrappedJson = "{ \"data\": " + json + " }";
                var file = serializer.Deserialize<LocalizationFile>(wrappedJson);
                if (file != null && file.data != null)
                {
                    foreach (var entry in file.data)
                    {
                        if (!string.IsNullOrEmpty(entry.key))
                        {
                            _texts[entry.key] = entry.text;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LocalizationService] Failed to load {filename}: {ex.Message}");
            }
        }

        public string GetText(string key)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;
            if (_texts.TryGetValue(key, out var text))
            {
                return text;
            }
            return $"[{key}]"; // Default fallback
        }
    }
}
