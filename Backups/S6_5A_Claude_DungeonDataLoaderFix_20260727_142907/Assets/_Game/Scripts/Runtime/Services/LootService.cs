using System;
using System.Collections.Generic;
using System.Linq;
using GuildMaster.Runtime.Formulas;
using GuildMaster.Runtime.Models;

namespace GuildMaster.Runtime.Services
{
    /// <summary>
    /// Loot rolling and the temporary area chest, following <c>Area.loot()</c> and
    /// <c>Area.fullChest()</c>.
    ///
    /// Two decode behaviours worth calling out:
    ///  * Drop weights are per-mille on a FIXED 1000 scale. They are not normalised against
    ///    their own sum, so a table adding up to less than 1000 has a real chance of dropping
    ///    nothing — that gap is the miss chance. The previous implementation divided by the
    ///    table's own total, which made every kill drop something.
    ///  * The chest cap counts total stack size, not the number of distinct entries.
    ///
    /// Evidence: Reports/S6_5A/S6_5A_001E_Rule_Cleanup_Report.md,
    ///           Reports/S6_5A/S6_5A_001D_Remaining_Core_Rules_Report.md
    /// </summary>
    public class LootService : ILootService
    {
        private const int CHEST_CAP = 2000;
        private const int CHEST_CAP_MERCHANT_PACK = 3000;

        private readonly Random _random;

        public LootService() : this(new Random()) { }

        /// <summary>Seeded overload so tests can make drops deterministic.</summary>
        public LootService(Random random)
        {
            _random = random ?? new Random();
        }

        public ItemRuntime RollSingleDrop(List<DropTableEntry> dropTable)
        {
            if (dropTable == null || dropTable.Count == 0) return null;

            var weighted = dropTable
                .Where(e => e != null && e.Item != null && e.Weight > 0)
                .Select(e => new KeyValuePair<DropTableEntry, int>(e, e.Weight));

            DropTableEntry picked = DecodeMath.RollFromWeightedMap(weighted, _random.NextDouble());
            if (picked == null) return null;   // rolled into the miss gap

            int stack = picked.StackCount > 0 ? picked.StackCount : 1;
            return new ItemRuntime(Guid.NewGuid().ToString(), picked.Item, stack);
        }

        public List<ItemRuntime> RollLoot(List<DropTableEntry> dropTable, int count)
        {
            var results = new List<ItemRuntime>();
            if (dropTable == null || dropTable.Count == 0 || count <= 0) return results;

            for (int i = 0; i < count; i++)
            {
                ItemRuntime drop = RollSingleDrop(dropTable);
                if (drop != null) results.Add(drop);
            }

            return results;
        }

        public bool IsChestFull(List<ItemRuntime> pendingDrops, bool merchantPackPurchased = false)
        {
            if (pendingDrops == null) return false;

            int total = 0;
            foreach (ItemRuntime drop in pendingDrops)
            {
                if (drop != null) total += drop.StackCount;
            }

            int cap = merchantPackPurchased ? CHEST_CAP_MERCHANT_PACK : CHEST_CAP;
            return total >= cap;
        }

        public void CollectPendingLoot(List<ItemRuntime> pendingDrops, List<ItemRuntime> newLoot, bool merchantPackPurchased = false)
        {
            if (pendingDrops == null || newLoot == null) return;

            foreach (ItemRuntime loot in newLoot)
            {
                if (loot == null || loot.Definition == null) continue;

                // The original stops collecting once the chest is full rather than trimming
                // the incoming stack, so the check happens per drop.
                if (IsChestFull(pendingDrops, merchantPackPurchased)) return;

                ItemRuntime existing = pendingDrops
                    .FirstOrDefault(x => x != null && x.Definition != null && x.Definition.id == loot.Definition.id);

                if (existing != null) existing.StackCount += loot.StackCount;
                else pendingDrops.Add(loot);
            }
        }
    }
}
