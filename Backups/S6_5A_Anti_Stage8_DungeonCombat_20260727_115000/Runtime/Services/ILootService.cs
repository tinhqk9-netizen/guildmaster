using System.Collections.Generic;
using GuildMaster.Definitions;
using GuildMaster.Runtime.Models;

namespace GuildMaster.Runtime.Services
{
    public interface ILootService
    {
        List<ItemRuntime> RollLoot(List<DropTableEntry> dropTable, int count);
        void CollectPendingLoot(List<ItemRuntime> pendingDrops, List<ItemRuntime> newLoot);
    }

    public class DropTableEntry
    {
        public ItemDefinition Item { get; set; }
        public int Weight { get; set; }
    }
}
