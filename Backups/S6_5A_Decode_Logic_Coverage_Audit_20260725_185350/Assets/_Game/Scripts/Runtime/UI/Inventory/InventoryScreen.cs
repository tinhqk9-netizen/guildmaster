using UnityEngine;
using UnityEngine.UI;
using GuildMaster.Runtime.UI.Core;
using GuildMaster.Runtime.Services;

namespace GuildMaster.Runtime.UI.Inventory
{
    public class InventoryScreen : UIScreen
    {
        private IInventoryService _inventoryService;
        [SerializeField] private Text _inventoryText; // Simple text-based list for placeholder

        public void Initialize(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
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
                content += $"- {defId} x{item.StackCount}\n";
            }
            _inventoryText.text = content;
        }
    }
}
