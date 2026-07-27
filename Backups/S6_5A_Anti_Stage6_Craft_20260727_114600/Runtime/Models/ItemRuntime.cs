using System;
using GuildMaster.Definitions;

namespace GuildMaster.Runtime.Models
{
    public class ItemRuntime
    {
        public string InstanceId { get; private set; }
        public ItemDefinition Definition { get; private set; }

        public int StackCount { get; set; }
        public bool IsLocked { get; set; }

        public ItemRuntime(string instanceId, ItemDefinition definition, int stackCount = 1)
        {
            InstanceId = instanceId;
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            StackCount = stackCount;
        }
    }
}
