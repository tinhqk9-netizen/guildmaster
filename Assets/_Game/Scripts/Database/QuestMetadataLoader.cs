using System;
using System.Collections.Generic;
using UnityEngine;
using GuildMaster.Definitions;

namespace GuildMaster.Database
{
    /// <summary>
    /// Fills <see cref="QuestDefinition.DefaultRarity"/> / <see cref="QuestDefinition.TargetProgressValues"/>
    /// from <c>quest_metadata.json</c>, which is NOT listed in manifest.json and was therefore
    /// never loaded before Phase 0 (quests.json, the manifest-driven "quests" category, only
    /// carries id/className). Read directly through the data provider, same technique already
    /// used by <see cref="ItemFieldsLoader"/> and <see cref="EnemyDropTableLoader"/> for data
    /// that JsonUtility can't bind generically (see phase0_schema_mapping.md §8).
    /// </summary>
    public static class QuestMetadataLoader
    {
        private const string FileName = "quest_metadata.json";

        [Serializable]
        private class QuestMetadataFileDto
        {
            public List<QuestMetadataEntryDto> entries;
        }

        [Serializable]
        private class QuestMetadataEntryDto
        {
            public string id;
            public string className;
            public int defaultRarity;
            public long[] targetProgressValues;
        }

        public static int Apply(GuildMaster.Infrastructure.DataProviders.IGameDataProvider dataProvider,
            IEnumerable<QuestDefinition> definitions)
        {
            if (dataProvider == null || definitions == null) return 0;
            if (!dataProvider.Exists(FileName)) return 0;

            string rawJson = dataProvider.ReadText(FileName);
            if (string.IsNullOrEmpty(rawJson)) return 0;

            QuestMetadataFileDto fileDto;
            try
            {
                fileDto = JsonUtility.FromJson<QuestMetadataFileDto>(rawJson);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[QuestMetadataLoader] JsonUtility failed: {ex.Message}");
                return 0;
            }

            if (fileDto == null || fileDto.entries == null) return 0;

            var map = new Dictionary<string, QuestMetadataEntryDto>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in fileDto.entries)
            {
                if (entry != null && !string.IsNullOrEmpty(entry.id))
                {
                    map[entry.id] = entry;
                }
            }

            int enriched = 0;
            foreach (var def in definitions)
            {
                if (def == null || string.IsNullOrEmpty(def.id)) continue;
                if (!map.TryGetValue(def.id, out var entry)) continue;

                def.DefaultRarity = entry.defaultRarity;
                def.TargetProgressValues = entry.targetProgressValues;
                enriched++;
            }

            return enriched;
        }
    }
}
