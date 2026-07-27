using System;
using System.Collections.Generic;
using System.Linq;
using GuildMaster.Runtime.Models;

namespace GuildMaster.Runtime.Services
{
    public class LootService : ILootService
    {
        private const int MAX_PENDING_CHEST_CAPACITY = 2000;
        private const int MAX_STACK_SIZE = 99999;
        private readonly Random _random = new Random();

        public List<ItemRuntime> RollLoot(List<DropTableEntry> dropTable, int count)
        {
            var results = new List<ItemRuntime>();
            if (dropTable == null || dropTable.Count == 0 || count <= 0) return results;

            int totalWeight = dropTable.Sum(x => x.Weight);
            if (totalWeight <= 0) return results;

            for (int i = 0; i < count; i++)
            {
                int roll = _random.Next(totalWeight);
                int currentWeight = 0;
                foreach (var entry in dropTable)
                {
                    currentWeight += entry.Weight;
                    if (roll < currentWeight)
                    {
                        if (entry.Item != null)
                        {
                            results.Add(new ItemRuntime(Guid.NewGuid().ToString(), entry.Item, 1));
                        }
                        break;
                    }
                }
            }
            return results;
        }

        public void CollectPendingLoot(List<ItemRuntime> pendingDrops, List<ItemRuntime> newLoot)
        {
            if (pendingDrops == null || newLoot == null) return;

            foreach (var loot in newLoot)
            {
                // Find existing stack of same definition
                var existing = pendingDrops.FirstOrDefault(x => x.Definition.id == loot.Definition.id);
                
                if (existing != null)
                {
                    if (existing.StackCount + loot.StackCount <= MAX_STACK_SIZE)
                    {
                        existing.StackCount += loot.StackCount;
                    }
                    else
                    {
                        int remainder = (existing.StackCount + loot.StackCount) - MAX_STACK_SIZE;
                        existing.StackCount = MAX_STACK_SIZE;
                        
                        // Check chest limit before adding a new stack
                        if (pendingDrops.Count < MAX_PENDING_CHEST_CAPACITY)
                        {
                            pendingDrops.Add(new ItemRuntime(Guid.NewGuid().ToString(), loot.Definition, remainder));
                        }
                    }
                }
                else
                {
                    if (pendingDrops.Count < MAX_PENDING_CHEST_CAPACITY)
                    {
                        pendingDrops.Add(loot);
                    }
                }
            }
        }
    }
}
