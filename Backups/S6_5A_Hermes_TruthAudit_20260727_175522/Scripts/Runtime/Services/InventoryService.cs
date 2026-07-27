using System;
using System.Collections.Generic;
using System.Linq;
using GuildMaster.Definitions;
using GuildMaster.Runtime.Core;
using GuildMaster.Database;
using GuildMaster.Runtime.Formulas;
using GuildMaster.Runtime.Models;
using GuildMaster.Runtime.Save;

namespace GuildMaster.Runtime.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly ISaveService _saveService;
        private readonly IFormulaService _formulaService;
        private readonly IItemService _itemService;
        private readonly GameDatabase _registry;

        private readonly List<ItemRuntime> _items;

        public InventoryService(ISaveService saveService, IFormulaService formulaService, IItemService itemService, GameDatabase registry)
        {
            _saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
            _formulaService = formulaService ?? throw new ArgumentNullException(nameof(formulaService));
            _itemService = itemService ?? throw new ArgumentNullException(nameof(itemService));
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));

            _items = new List<ItemRuntime>();
            LoadFromSave();
        }

        private void LoadFromSave()
        {
            var savedItems = _saveService.CurrentData.Items;
            foreach (var saveData in savedItems)
            {
                if (_registry.TryGet<ItemDefinition>(saveData.DefinitionId, out var def))
                {
                    var item = new ItemRuntime(saveData.InstanceId, def, saveData.StackCount)
                    {
                        IsLocked = saveData.IsLocked
                    };
                    _items.Add(item);
                }
            }
        }

        private void SyncToSave()
        {
            _saveService.CurrentData.Items.Clear();
            foreach (var item in _items)
            {
                _saveService.CurrentData.Items.Add(new ItemSaveData
                {
                    InstanceId = item.InstanceId,
                    DefinitionId = item.Definition.id,
                    StackCount = item.StackCount,
                    IsLocked = item.IsLocked
                });
            }
        }

        public int GetCapacity()
        {
            var data = _saveService.CurrentData;
            return _formulaService.StorageSpaces(data.LevelStorage, data.UpgradeStorage, data.GetPurchaseFlags());
        }

        public bool CanAddItem(string definitionId)
        {
            if (!_registry.TryGet<ItemDefinition>(definitionId, out var def)) return false;

            bool canStack = (def.Category == ItemCategory.Material || def.Category == ItemCategory.Consumable);
            
            // If we already have an item of this definition, and it can stack, we can add it
            if (canStack && _items.Any(i => i.Definition.id == definitionId))
            {
                return true;
            }

            // Otherwise, we need a free slot
            return _items.Count < GetCapacity();
        }

        public void AddItem(ItemRuntime item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            bool canStack = (item.Definition.Category == ItemCategory.Material || item.Definition.Category == ItemCategory.Consumable);

            if (canStack)
            {
                var existingItem = _items.FirstOrDefault(i => i.Definition.id == item.Definition.id);
                if (existingItem != null)
                {
                    existingItem.StackCount += item.StackCount;
                    SyncToSave();
                    return;
                }
            }
            
            if (_items.Count >= GetCapacity())
            {
                throw new InvalidOperationException("Inventory is full");
            }
            _items.Add(item);
            
            SyncToSave();
        }

        public bool RemoveItem(string instanceId, int amount)
        {
            if (amount <= 0) return false;

            var item = _items.FirstOrDefault(i => i.InstanceId == instanceId);
            if (item == null) return false;

            if (item.StackCount < amount) return false;

            item.StackCount -= amount;
            if (item.StackCount <= 0)
            {
                _items.Remove(item);
            }
            SyncToSave();
            return true;
        }

        public bool HasItem(string instanceId)
        {
            return _items.Any(i => i.InstanceId == instanceId);
        }

        public ItemRuntime GetItem(string instanceId)
        {
            return _items.FirstOrDefault(i => i.InstanceId == instanceId);
        }

        public IReadOnlyList<ItemRuntime> GetAllItems()
        {
            return _items.AsReadOnly();
        }

        public IReadOnlyList<ItemRuntime> GetItemsByCategory(ItemCategory category)
        {
            return _items.Where(i => i.Definition != null && i.Definition.Category == category).ToList().AsReadOnly();
        }

        public bool ToggleLockItem(string instanceId)
        {
            var item = GetItem(instanceId);
            if (item == null) return false;
            item.IsLocked = !item.IsLocked;
            SyncToSave();
            return true;
        }

        public bool UseConsumable(string instanceId, CharacterRuntime targetCharacter)
        {
            var item = GetItem(instanceId);
            if (item == null || item.Definition == null || item.Definition.Category != ItemCategory.Consumable) return false;

            // Apply consumable effect if targetCharacter provided
            if (targetCharacter != null)
            {
                // Potion / Consumable usage
                targetCharacter.CurrentHp = Math.Min(targetCharacter.Definition.BaseMaxHp, targetCharacter.CurrentHp + 50);
            }

            return RemoveItem(instanceId, 1);
        }

        // S3 Batch 2 - DefinitionId API Foundation
        public int GetQuantityByDefinitionId(string definitionId)
        {
            if (string.IsNullOrEmpty(definitionId)) return 0;
            return _items.Where(i => i.Definition != null && i.Definition.id == definitionId).Sum(i => i.StackCount);
        }

        public bool HasQuantityByDefinitionId(string definitionId, int amount)
        {
            if (string.IsNullOrEmpty(definitionId) || amount <= 0) return false;
            return GetQuantityByDefinitionId(definitionId) >= amount;
        }

        public bool ConsumeByDefinitionId(string definitionId, int amount)
        {
            if (string.IsNullOrEmpty(definitionId) || amount <= 0) return false;
            
            if (!HasQuantityByDefinitionId(definitionId, amount)) return false;

            int remainingToConsume = amount;
            var matchingItems = _items.Where(i => i.Definition != null && i.Definition.id == definitionId).ToList();

            foreach (var item in matchingItems)
            {
                if (remainingToConsume <= 0) break;

                if (item.StackCount <= remainingToConsume)
                {
                    remainingToConsume -= item.StackCount;
                    item.StackCount = 0;
                    _items.Remove(item);
                }
                else
                {
                    item.StackCount -= remainingToConsume;
                    remainingToConsume = 0;
                }
            }

            SyncToSave();
            return true;
        }
    }
}
