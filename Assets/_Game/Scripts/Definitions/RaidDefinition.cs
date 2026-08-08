using System;
using System.Collections.Generic;

namespace GuildMaster.Definitions
{
    [Serializable]
    public class RaidRoomData
    {
        public List<string> EnemySourceClasses = new List<string>();
        public string EventKey;
        public bool IsBossRoom;
        // Java Area.progress at which this room is entered. -1 means descriptive-only data.
        public int LegacyProgress = -1;
    }

    [Serializable]
    public class RaidEncounterData
    {
        public int LegacyProgress;
        public bool IsBossRoom;
        public string UniqueRewardItemId;
        public List<string> EnemyIds = new List<string>();
    }

    [Serializable]
    public class RaidDefinition : DefinitionBase
    {
        public string RequiredClearDungeonId;
        public int RequiredClearProgress;

        /// <summary>
        /// Enemies this raid can spawn. Java: rollEnemies() (hardcoded, same as regular
        /// dungeons). Was completely absent from raids.json/RaidDefinition before Phase 0 —
        /// see Docs/Backend_Audit/dungeon_encounter_data_audit.md §6.
        /// </summary>
        public List<string> EnemyIds;

        /// <summary>Same weighted-group schema as DungeonDefinition.EncounterGroups.</summary>
        public List<EncounterGroupData> EncounterGroups;

        public double EmptyRoomWeight;
        public List<RaidRoomData> Rooms = new List<RaidRoomData>();
        public List<RaidEncounterData> LegacyEncounters = new List<RaidEncounterData>();
        public string UniqueRewardItemId;

        // Restored Java Area metadata. These are populated by RaidContentCatalog from the
        // decoded raid classes; absent values retain the previous Unity defaults.
        public int LegacyPartySize;
        public int LegacyMaxProgress;
        public int LegacyDarkness;
        public bool IsEventDriven;
        public List<string> LegacyEventKeys = new List<string>();
    }
}
