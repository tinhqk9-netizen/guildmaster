using System;
using System.Linq;
using GuildMaster.Definitions;
using GuildMaster.Runtime.Models;
using GuildMaster.Runtime.Save;

namespace GuildMaster.Runtime.Services
{
    public class EquipmentService : IEquipmentService
    {
        private readonly IInventoryService _inventoryService;
        private readonly ISaveService _saveService;

        public EquipmentService(IInventoryService inventoryService, ISaveService saveService)
        {
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            _saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
        }

        public bool CanEquip(CharacterRuntime character, ItemRuntime item, EquipmentSlot slot)
        {
            if (character == null || item == null) return false;

            // Check if item matches the slot category
            if (slot == EquipmentSlot.Weapon && item.Definition.Category != ItemCategory.Weapon) return false;
            if (slot == EquipmentSlot.Armor && item.Definition.Category != ItemCategory.Armor) return false;
            if (slot == EquipmentSlot.Accessory && item.Definition.Category != ItemCategory.Accessory) return false;

            // Check class restrictions from definition
            if (slot == EquipmentSlot.Weapon)
            {
                if (character.Definition.WeaponType != "Generic" && item.Definition.ItemType != character.Definition.WeaponType)
                {
                    return false;
                }
            }
            else if (slot == EquipmentSlot.Armor)
            {
                if (item.Definition.ItemType != character.Definition.ArmorType)
                {
                    return false;
                }
            }

            return true;
        }

        public bool Equip(CharacterRuntime character, string itemInstanceId, EquipmentSlot slot)
        {
            if (character == null || string.IsNullOrEmpty(itemInstanceId)) return false;

            var item = _inventoryService.GetItem(itemInstanceId);
            if (item == null) return false;

            // TODO: manualRuleRequired for class restriction check fully
            if (!CanEquip(character, item, slot)) return false;

            // Unequip current item in that slot if exists (which doesn't add to inventory since it was never removed)
            Unequip(character, slot);

            // TODO: manualRuleRequired. Did not find evidence of removing from inventory upon equip. 
            // So we just hold the reference and lock it.
            item.IsLocked = true;
            
            switch (slot)
            {
                case EquipmentSlot.Weapon:
                    character.Weapon = item;
                    break;
                case EquipmentSlot.Armor:
                    character.Armor = item;
                    break;
                case EquipmentSlot.Accessory:
                    character.Accessory = item;
                    break;
            }

            SyncSave(character);
            return true;
        }

        public bool Unequip(CharacterRuntime character, EquipmentSlot slot)
        {
            if (character == null) return false;

            ItemRuntime currentItem = null;
            switch (slot)
            {
                case EquipmentSlot.Weapon:
                    currentItem = character.Weapon;
                    character.Weapon = null;
                    break;
                case EquipmentSlot.Armor:
                    currentItem = character.Armor;
                    character.Armor = null;
                    break;
                case EquipmentSlot.Accessory:
                    currentItem = character.Accessory;
                    character.Accessory = null;
                    break;
            }

            if (currentItem != null)
            {
                // TODO: manualRuleRequired. We did not remove it from inventory on equip, so we don't add it back.
                // Just unlock it.
                currentItem.IsLocked = false;
                SyncSave(character);
                return true;
            }

            return false;
        }

        private void SyncSave(CharacterRuntime character)
        {
            var charSave = _saveService.CurrentData.Characters.FirstOrDefault(c => c.InstanceId == character.InstanceId);
            if (charSave != null)
            {
                charSave.WeaponInstanceId = character.Weapon?.InstanceId;
                charSave.ArmorInstanceId = character.Armor?.InstanceId;
                charSave.AccessoryInstanceId = character.Accessory?.InstanceId;
            }
        }
    }
}
