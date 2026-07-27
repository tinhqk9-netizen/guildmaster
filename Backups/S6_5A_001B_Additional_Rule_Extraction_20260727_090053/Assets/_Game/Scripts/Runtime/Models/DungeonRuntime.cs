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
        
        public int ActionType { get; set; }
        public int ActionTurnsPassed { get; set; }
        public string SavedActingEntityId { get; set; }
        public int TurnsFighting { get; set; }

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
