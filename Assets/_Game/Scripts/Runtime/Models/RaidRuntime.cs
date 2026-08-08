using System.Collections.Generic;
using GuildMaster.Definitions;

namespace GuildMaster.Runtime.Models
{
    public sealed class RaidRuntime
    {
        public RaidDefinition Definition { get; }
        public int RoomIndex { get; set; }
        public int LegacyProgress { get; set; } = 1;
        public string EventKey { get; set; }
        public int EventProgress { get; set; }
        public string EventOutcome { get; set; }
        public bool IsComplete { get; set; }
        public bool IsFailed { get; set; }
        public List<CharacterRuntime> Party { get; } = new List<CharacterRuntime>();
        public List<EnemyRuntime> Enemies { get; } = new List<EnemyRuntime>();
        public List<ItemRuntime> PendingRewards { get; } = new List<ItemRuntime>();
        public List<string> Log { get; } = new List<string>();

        public RaidRuntime(RaidDefinition definition)
        {
            Definition = definition;
        }

        public bool HasActiveEvent => !string.IsNullOrEmpty(EventKey);

        public void AddLog(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            Log.Add(message);
            while (Log.Count > 40) Log.RemoveAt(0);
        }
    }
}
