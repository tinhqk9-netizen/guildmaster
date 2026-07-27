using UnityEngine;
using UnityEngine.UI;
using GuildMaster.Runtime.UI.Core;
using GuildMaster.Runtime.Services;

namespace GuildMaster.Runtime.UI.Inventory
{
    public class InventoryScreen : UIScreen
    {
        private IInventoryService _inventoryService;
        private ICharacterService _characterService;
        [SerializeField] private Text _inventoryText;

        public void Initialize(ServiceContainer services)
        {
            _inventoryService = services.Inventory;
            _characterService = services.Character;
        }

        public override void Show()
        {
            base.Show();
            Refresh();
        }

        public void Refresh()
        {
            if (_inventoryService == null || _inventoryText == null) return;
            
            var items = _inventoryService.GetAllItems();
            if (items == null || items.Count == 0)
            {
                _inventoryText.text = "Inventory is empty.";
                return;
            }

            string content = "Inventory:\n";
            foreach (var item in items)
            {
                string defId = item.Definition != null ? item.Definition.id : "Unknown";
                content += $"- {defId} x{item.StackCount} (Lock:{item.IsLocked})\n";
            }
            _inventoryText.text = content;
        }

        public void OnClickToggleLockFirst()
        {
            var items = _inventoryService?.GetAllItems();
            if (items != null && items.Count > 0)
            {
                _inventoryService.ToggleLockItem(items[0].InstanceId);
                Refresh();
            }
        }

        public void OnClickUseFirstConsumable()
        {
            var items = _inventoryService?.GetAllItems();
            var chars = _characterService?.GetAllCharacters();
            if (items != null && chars != null && chars.Count > 0)
            {
                foreach (var item in items)
                {
                    if (item.Definition is GuildMaster.Definitions.ItemDefinition itemDef && itemDef.Consumable)
                    {
                        if (_inventoryService.UseConsumable(item.InstanceId, chars[0])) break;
                    }
                }
                Refresh();
            }
        }
    }
}
