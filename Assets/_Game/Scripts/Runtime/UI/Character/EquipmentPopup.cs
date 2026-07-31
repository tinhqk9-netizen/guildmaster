using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GuildMaster.Runtime.UI.Core;
using GuildMaster.Runtime.Services;
using GuildMaster.Runtime.Models;
using GuildMaster.Definitions;
using System.Linq;

namespace GuildMaster.Runtime.UI.Character
{
    public class EquipmentPopup : UIScreen
    {
        private IInventoryService _inventoryService;
        private IEquipmentService _equipmentService;
        
        [SerializeField] private RectTransform _itemContainer;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Text _titleText;
        [SerializeField] private Text _feedbackText;
        [SerializeField] private Button _equipButton;

        private CharacterRuntime _targetCharacter;
        private EquipmentSlot _targetSlot;
        private ItemRuntime _selectedItem;

        public void Initialize(ServiceContainer services)
        {
            _inventoryService = services.Inventory;
            _equipmentService = services.Equipment;
            
            if (_closeButton != null)
                _closeButton.onClick.AddListener(Hide);
                
            if (_equipButton != null)
                _equipButton.onClick.AddListener(OnClickEquip);
        }

        public void ShowForSlot(CharacterRuntime character, EquipmentSlot slot)
        {
            _targetCharacter = character;
            _targetSlot = slot;
            _selectedItem = null;
            
            if (_titleText != null)
                _titleText.text = $"Select {slot}";
                
            if (_feedbackText != null)
                _feedbackText.text = "";

            Show();
            RefreshList();
        }

        private void RefreshList()
        {
            if (_itemContainer == null) return;
            UICardFactory.ClearContainer(_itemContainer);

            if (_inventoryService == null || _targetCharacter == null) return;

            var allItems = _inventoryService.GetAllItems();
            var availableItems = allItems.Where(i => 
                !i.IsLocked && 
                i.Definition != null && 
                i.Definition.Category.ToString() == _targetSlot.ToString()
            ).ToList();

            if (availableItems.Count == 0)
            {
                UICardFactory.CreateDivider(_itemContainer, "No items available for this slot.");
                if (_equipButton != null) _equipButton.interactable = false;
                return;
            }

            for (int i = 0; i < availableItems.Count; i++)
            {
                var item = availableItems[i];
                bool isSelected = (_selectedItem == item);
                
                UICardFactory.CreateCard(
                    _itemContainer, 
                    item.Definition.id, 
                    item.Definition != null ? item.Definition.GetStatSummary() : $"Qty: {item.StackCount}",
                    isSelected, 
                    true, 
                    () => {
                        _selectedItem = item;
                        RefreshList();
                    }
                );
            }
            
            if (_equipButton != null)
                _equipButton.interactable = (_selectedItem != null);
        }

        private void OnClickEquip()
        {
            if (_targetCharacter == null || _selectedItem == null || _equipmentService == null) return;

            bool success = _equipmentService.Equip(_targetCharacter, _selectedItem.InstanceId, _targetSlot);
            if (success)
            {
                if (_feedbackText != null)
                {
                    _feedbackText.text = "Equipped!";
                    _feedbackText.color = UITemporaryTheme.SuccessColor;
                }
                Hide();
            }
            else
            {
                if (_feedbackText != null)
                {
                    _feedbackText.text = "Failed to equip.";
                    _feedbackText.color = UITemporaryTheme.FailureColor;
                }
            }
        }
    }
}
