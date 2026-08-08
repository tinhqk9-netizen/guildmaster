using System;

namespace GuildMaster.Definitions
{
    [Serializable]
    public class QuestDefinition : DefinitionBase
    {
        // Phase 0: JsonUtility only binds public fields, never { get; set; } properties.
        // Converted from properties to fields — same names/types, no caller changes needed.
        public long TargetProgress;
        public string TrueClass;

        // Java: quest_metadata.json (previously not wired into the manifest at all).
        // defaultRarity is the rarity a fresh copy of this quest is generated at.
        public int DefaultRarity;
        // Per-rarity target progress table (index 0 = rarity 1, ... index 9 = rarity 10).
        public long[] TargetProgressValues;

        // Java: QuestsManager.java hardcodes pool membership as field-lists
        // (kingsQuests / afflictionQuests / warQuests / ...). No JSON source encodes this yet,
        // so these stay null until a future parser pass or hand-authored mapping fills them in.
        // Schema-only — see phase0_schema_mapping.md §8.
        public string PoolType;   // "Kings" or "Doctrine"
        public string DoctrineId; // e.g. "war", "affliction" — only set when PoolType == "Doctrine"
    }
}
