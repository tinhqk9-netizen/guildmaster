using System.Collections.Generic;
using GuildMaster.Definitions;
using GuildMaster.Runtime.Models;

namespace GuildMaster.Runtime.Services
{
    public interface ILootService
    {
        /// <summary>
        /// Rolls one enemy's drop table once, following <c>Area.loot()</c>.
        /// Returns null when the roll lands past the table's filled weight — the decode's way
        /// of expressing "this kill dropped nothing".
        /// </summary>
        ItemRuntime RollSingleDrop(List<DropTableEntry> dropTable);

        List<ItemRuntime> RollLoot(List<DropTableEntry> dropTable, int count);

        /// <summary>Merges new drops into the temporary area chest, respecting its cap.</summary>
        void CollectPendingLoot(List<ItemRuntime> pendingDrops, List<ItemRuntime> newLoot, bool merchantPackPurchased = false);

        /// <summary>
        /// True once the chest is full. <c>Area.fullChest()</c> sums stack counts (not entry
        /// count) against 2000, or 3000 with the merchant pack.
        /// </summary>
        bool IsChestFull(List<ItemRuntime> pendingDrops, bool merchantPackPurchased = false);
    }

    public class DropTableEntry
    {
        public ItemDefinition Item { get; set; }
        public int Weight { get; set; }
        public int StackCount { get; set; } = 1;
    }
}
