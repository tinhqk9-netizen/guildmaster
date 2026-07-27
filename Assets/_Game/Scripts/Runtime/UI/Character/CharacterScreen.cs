using UnityEngine;
using UnityEngine.UI;
using GuildMaster.Runtime.UI.Core;
using GuildMaster.Runtime.Services;

namespace GuildMaster.Runtime.UI.Character
{
    public class CharacterScreen : UIScreen
    {
        private ICharacterService _characterService;
        private IEquipmentService _equipmentService;
        private IInventoryService _inventoryService;
        [SerializeField] private Text _characterText;

        public void Initialize(ServiceContainer services)
        {
            _characterService = services.Character;
            _equipmentService = services.Equipment;
            _inventoryService = services.Inventory;
        }

        public override void Show()
        {
            base.Show();
            Refresh();
        }

        public void Refresh()
        {
            if (_characterService == null || _characterText == null) return;

            var characters = _characterService.GetAllCharacters();
            if (characters == null || characters.Count == 0)
            {
                _characterText.text = "No characters available.";
                return;
            }

            string content = "Characters:\n";
            int idx = 0;
            foreach (var c in characters)
            {
                string defId = c.Definition != null ? c.Definition.id : "Unknown";
                content += $"[{idx}] [{defId}] Lv.{c.Level} HP:{c.CurrentHp} Exp:{c.Experience}\n";
                content += $"  Stats - CON:{_characterService.GetTotalStat(c, GuildMaster.Definitions.StatType.Constitution)} DEX:{_characterService.GetTotalStat(c, GuildMaster.Definitions.StatType.Dexterity)} INT:{_characterService.GetTotalStat(c, GuildMaster.Definitions.StatType.Intelligence)}\n";
                content += $"  Wpn:{c.Weapon?.InstanceId} Arm:{c.Armor?.InstanceId} Acc:{c.Accessory?.InstanceId}\n";
                idx++;
            }
            _characterText.text = content;
        }

        public void OnClickEquipFirstItemToFirstCharacter()
        {
            if (_characterService == null || _equipmentService == null || _inventoryService == null) return;
            var chars = _characterService.GetAllCharacters();
            var items = _inventoryService.GetAllItems();
            if (chars.Count > 0 && items.Count > 0)
            {
                var target = chars[0];
                foreach (var item in items)
                {
                    if (item.Definition is GuildMaster.Definitions.ItemDefinition itemDef)
                    {
                        if (_equipmentService.Equip(target, item.InstanceId, GuildMaster.Definitions.EquipmentSlot.Weapon)) break;
                        if (_equipmentService.Equip(target, item.InstanceId, GuildMaster.Definitions.EquipmentSlot.Armor)) break;
                        if (_equipmentService.Equip(target, item.InstanceId, GuildMaster.Definitions.EquipmentSlot.Accessory)) break;
                    }
                }
            }
            Refresh();
        }

        public void OnClickUnequipFirstCharacter()
        {
            if (_characterService == null || _equipmentService == null) return;
            var chars = _characterService.GetAllCharacters();
            if (chars.Count > 0)
            {
                var target = chars[0];
                _equipmentService.Unequip(target, GuildMaster.Definitions.EquipmentSlot.Weapon);
                _equipmentService.Unequip(target, GuildMaster.Definitions.EquipmentSlot.Armor);
                _equipmentService.Unequip(target, GuildMaster.Definitions.EquipmentSlot.Accessory);
            }
            Refresh();
        }
    }
}
