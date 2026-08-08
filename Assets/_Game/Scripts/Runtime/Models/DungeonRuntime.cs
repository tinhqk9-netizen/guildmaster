using System;
using System.Collections.Generic;
using GuildMaster.Definitions;

namespace GuildMaster.Runtime.Models
{
    public enum DungeonState
    {
        Locked,
        Unlocked,
        Completed
    }

    public class DungeonRuntime
    {
        public string InstanceId { get; private set; }
        public DungeonDefinition Definition { get; private set; }

        public DungeonState State { get; set; }
        public int ClearCount { get; set; }
        public float BestTimeSeconds { get; set; }

        // S3-001A fields for active run
        public int Progress { get; set; }
        public int MaxProgress { get; set; }
        public int LocalDarkness { get; set; }
        public List<string> AdventurerInstanceIds { get; set; } = new List<string>();
        public List<GuildMaster.Runtime.Models.ItemRuntime> PendingDrops { get; set; } = new List<GuildMaster.Runtime.Models.ItemRuntime>();
        public List<GuildMaster.Runtime.Models.EnemyRuntime> Enemies { get; set; } = new List<GuildMaster.Runtime.Models.EnemyRuntime>();
        public List<GuildMaster.Runtime.Models.EnemyRuntime> Corpses { get; set; } = new List<GuildMaster.Runtime.Models.EnemyRuntime>();
        public string PetInstanceId { get; set; }
        
        public int ActionType { get; set; }
        public int ActionTurnsPassed { get; set; }
        public string SavedActingEntityId { get; set; }
        public int TurnsFighting { get; set; }

        /// <summary>
        /// Short, UI-facing description of the most recent room result (multi-enemy encounter,
        /// empty room, or search-room find) — not persisted, purely for the Dungeon tab's
        /// "encounter event" display (Task 2.6). Set by DungeonService.EnterRoom / RunSearchRoomReward.
        /// </summary>
        public string LastRoomEvent { get; set; } = string.Empty;

        /// <summary>
        /// Rolling text log of recent expedition events (encounters, loot, defeats) for the
        /// Dungeon tab's combat log display (Task 2.6). In-memory only, capped at
        /// <see cref="CombatLogCap"/> entries — not persisted across save/load.
        /// </summary>
        public List<string> CombatLog { get; } = new List<string>();

        private const int CombatLogCap = 20;

        public void AddLog(string entry)
        {
            if (string.IsNullOrEmpty(entry)) return;
            CombatLog.Add(entry);
            while (CombatLog.Count > CombatLogCap) CombatLog.RemoveAt(0);
        }

        public DungeonRuntime(string instanceId, DungeonDefinition definition)
        {
            InstanceId = instanceId;
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            
            State = DungeonState.Locked;
            ClearCount = 0;
            BestTimeSeconds = 0f;
        }
    }
}
