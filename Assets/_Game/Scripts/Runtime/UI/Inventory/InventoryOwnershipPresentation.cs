using System.Collections.Generic;
using GuildMaster.Runtime.Models;

namespace GuildMaster.Runtime.UI.Inventory
{
    /// <summary>
    /// Presentation-only ownership counts. InventoryService remains the source of truth for
    /// available items; this helper explains available versus character-equipped instances.
    /// </summary>
    public readonly struct InventoryOwnershipCounts
    {
        public readonly int Available;
        public readonly int Equipped;

        public InventoryOwnershipCounts(int available, int equipped)
        {
            Available = available;
            Equipped = equipped;
        }
    }

    public static class InventoryOwnershipPresentation
    {
        public static InventoryOwnershipCounts ForDefinition(
            string definitionId,
            IReadOnlyList<ItemRuntime> availableItems,
            IReadOnlyList<CharacterRuntime> characters)
        {
            if (string.IsNullOrEmpty(definitionId)) return new InventoryOwnershipCounts(0, 0);

            var availableIds = new HashSet<string>();
            if (availableItems != null)
            {
                foreach (var item in availableItems)
                {
                    if (item?.Definition?.id == definitionId && !string.IsNullOrEmpty(item.InstanceId))
                        availableIds.Add(item.InstanceId);
                }
            }

            var equippedIds = new HashSet<string>();
            if (characters != null)
            {
                foreach (var character in characters)
                {
                    AddIfDefinitionMatches(character?.Weapon, definitionId, equippedIds);
                    AddIfDefinitionMatches(character?.Armor, definitionId, equippedIds);
                    AddIfDefinitionMatches(character?.Accessory, definitionId, equippedIds);
                }
            }

            return new InventoryOwnershipCounts(availableIds.Count, equippedIds.Count);
        }

        public static int CountEquippedInstances(IReadOnlyList<CharacterRuntime> characters)
        {
            var equippedIds = new HashSet<string>();
            if (characters == null) return 0;

            foreach (var character in characters)
            {
                AddInstanceId(character?.Weapon, equippedIds);
                AddInstanceId(character?.Armor, equippedIds);
                AddInstanceId(character?.Accessory, equippedIds);
            }

            return equippedIds.Count;
        }

        private static void AddIfDefinitionMatches(ItemRuntime item, string definitionId, HashSet<string> ids)
        {
            if (item?.Definition?.id == definitionId) AddInstanceId(item, ids);
        }

        private static void AddInstanceId(ItemRuntime item, HashSet<string> ids)
        {
            if (item != null && !string.IsNullOrEmpty(item.InstanceId)) ids.Add(item.InstanceId);
        }
    }
}
