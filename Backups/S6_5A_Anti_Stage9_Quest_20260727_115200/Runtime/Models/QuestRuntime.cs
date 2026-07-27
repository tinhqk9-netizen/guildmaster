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
        public bool IsDirty { get; set; }

        public bool IsActive => State == QuestState.InProgress;

        public QuestRuntime(string instanceId, QuestDefinition definition)
        {
            InstanceId = instanceId;
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            
            State = QuestState.NotStarted;
            Progress = 0;
        }
    }
}
