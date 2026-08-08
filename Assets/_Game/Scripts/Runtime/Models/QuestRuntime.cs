using System;
using System.Collections.Generic;
using GuildMaster.Definitions;

namespace GuildMaster.Runtime.Models
{
    public enum QuestState
    {
        Locked,
        NotStarted,
        InProgress,
        Completed,
        RewardClaimed
    }

    public class QuestRuntime
    {
        public string InstanceId { get; private set; }
        public QuestDefinition Definition { get; private set; }

        public QuestState State { get; set; }
        public long Progress { get; set; }
        public int Rarity { get; set; }
        public long TargetProgress { get; set; }
        public bool IsDirty { get; set; }

        // A definition can appear in both the Legacy doctrine pool and the general/Kings
        // pool.  Keep the selected pool on the runtime instance instead of deriving the
        // reward from QuestDefinition.PoolType (which describes membership, not selection).
        public string RewardPoolType { get; set; }
        public string RewardDoctrineId { get; set; }

        public bool IsActive => State == QuestState.InProgress;

        public QuestRuntime(string instanceId, QuestDefinition definition, int rarity = 1, long targetProgress = 100)
        {
            InstanceId = instanceId;
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            
            State = QuestState.NotStarted;
            Progress = 0;
            Rarity = rarity;
            TargetProgress = targetProgress;
            RewardPoolType = definition.PoolType;
            RewardDoctrineId = definition.DoctrineId;
        }
    }
}
