using System;
using System.Collections.Generic;
using UnityEngine;
using GuildMaster.Definitions;

namespace GuildMaster.Database
{
    /// <summary>
    /// Fills <see cref="ItemDefinition"/> stat fields (Constitution, Dexterity, Intelligence, Defense, MagicDefense, MaxHp)
    /// from the dynamic <c>"fields"</c> dictionary inside <c>items.json</c>.
    /// JsonUtility skips standard dictionaries during generic deserialization, so this class uses a typed DTO structure.
    /// </summary>
    public static class ItemFieldsLoader
    {
        [Serializable]
        private class ItemFileDto
        {
            public List<ItemDto> data;
        }

        [Serializable]
        private class ItemDto
        {
            public string id;
            public ItemFieldsDto fields;
        }

        [Serializable]
        private class ItemFieldsDto
        {
            public FieldValueDto constitution;
            public FieldValueDto dexterity;
            public FieldValueDto intelligence;
            public FieldValueDto defense;
            public FieldValueDto magicDefense;
            public FieldValueDto maxHp;
        }

        [Serializable]
        private class FieldValueDto
        {
            public int value;
        }

        public static int Apply(string rawJson, IEnumerable<ItemDefinition> definitions)
        {
            if (string.IsNullOrEmpty(rawJson) || definitions == null) return 0;

            ItemFileDto fileDto;
            try
            {
                fileDto = JsonUtility.FromJson<ItemFileDto>(rawJson);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ItemFieldsLoader] JsonUtility failed: {ex.Message}");
                return 0;
            }

            if (fileDto == null || fileDto.data == null) return 0;

            var map = new Dictionary<string, ItemFieldsDto>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in fileDto.data)
            {
                if (item != null && !string.IsNullOrEmpty(item.id) && item.fields != null)
                {
                    map[item.id] = item.fields;
                }
            }

            int enriched = 0;
            foreach (var def in definitions)
            {
                if (def == null || string.IsNullOrEmpty(def.id)) continue;

                if (map.TryGetValue(def.id, out var fields))
                {
                    if (fields.constitution != null) def.Constitution = fields.constitution.value;
                    if (fields.dexterity != null)    def.Dexterity    = fields.dexterity.value;
                    if (fields.intelligence != null) def.Intelligence = fields.intelligence.value;
                    if (fields.defense != null)      def.Defense      = fields.defense.value;
                    if (fields.magicDefense != null) def.MagicDefense = fields.magicDefense.value;
                    if (fields.maxHp != null)        def.MaxHp        = fields.maxHp.value;

                    if (def.Constitution != 0 || def.Dexterity != 0 || def.Intelligence != 0 ||
                        def.Defense != 0 || def.MagicDefense != 0 || def.MaxHp != 0)
                    {
                        enriched++;
                    }
                }
            }

            return enriched;
        }
    }
}
