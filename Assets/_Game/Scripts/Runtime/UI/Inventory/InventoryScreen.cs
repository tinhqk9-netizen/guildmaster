using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using GuildMaster.Runtime.UI.Core;
using GuildMaster.Runtime.UI.Character;
using GuildMaster.Runtime.Services;
using GuildMaster.Runtime.Models;
using GuildMaster.Definitions;

namespace GuildMaster.Runtime.UI.Inventory
{
    public enum InventoryTab { All, Weapons, Armor, Consumables, Materials }

    /// <summary>
    /// Inventory screen — shows owned items filtered by category tab,
    /// capacity bar, detail panel with type/lock/stack info,
    /// and contextual actions (Use/Lock/Sell) enabled only when valid.
    /// Sources: DialogStorage.java, DialogItemDetail.java, DialogSell.java
    /// </summary>
    public class InventoryScreen : UIScreen
    {
        private IInventoryService _inventoryService;
        private ICharacterService _characterService;
        private CharacterScreen   _characterScreen;

        // ── Serialized references (wired by Apply tool) ──────────────────────────
        [SerializeField] private RectTransform _cardContainer;
        [SerializeField] private Text          _summaryText;
        [SerializeField] private Text          _detailText;
        [SerializeField] private Button        _useButton;
        [SerializeField] private Button        _lockButton;
        [SerializeField] private Text          _lockButtonLabel;
        [SerializeField] private Button        _sellButton;
        [SerializeField] private Text          _feedbackText;
        [SerializeField] private Button        _prevButton;
        [SerializeField] private Button        _nextButton;

        // ── Tab bar buttons ──────────────────────────────────────────────────────
        [SerializeField] private Button _tabAllBtn;
        [SerializeField] private Button _tabWeaponsBtn;
        [SerializeField] private Button _tabArmorBtn;
        [SerializeField] private Button _tabConsumablesBtn;
        [SerializeField] private Button _tabMaterialsBtn;

        private InventoryTab _activeTab = InventoryTab.All;
        private int          _selectedIndex;

        public void Initialize(ServiceContainer services, CharacterScreen characterScreen = null)
        {
            _inventoryService = services.Inventory;
            _characterService = services.Character;
            _characterScreen  = characterScreen;

            if (_tabAllBtn         != null) _tabAllBtn.onClick.AddListener(OnClickTabAll);
            if (_tabWeaponsBtn     != null) _tabWeaponsBtn.onClick.AddListener(OnClickTabWeapons);
            if (_tabArmorBtn       != null) _tabArmorBtn.onClick.AddListener(OnClickTabArmor);
            if (_tabConsumablesBtn != null) _tabConsumablesBtn.onClick.AddListener(OnClickTabConsumables);
            if (_tabMaterialsBtn   != null) _tabMaterialsBtn.onClick.AddListener(OnClickTabMaterials);
        }

        public override void Show()
        {
            base.Show();
            _activeTab     = InventoryTab.All;
            _selectedIndex = 0;
            Refresh();
        }

        // ── Tab clicks ───────────────────────────────────────────────────────────
        public void OnClickTabAll()         { _activeTab = InventoryTab.All;         _selectedIndex = 0; Refresh(); }
        public void OnClickTabWeapons()     { _activeTab = InventoryTab.Weapons;     _selectedIndex = 0; Refresh(); }
        public void OnClickTabArmor()       { _activeTab = InventoryTab.Armor;       _selectedIndex = 0; Refresh(); }
        public void OnClickTabConsumables() { _activeTab = InventoryTab.Consumables; _selectedIndex = 0; Refresh(); }
        public void OnClickTabMaterials()   { _activeTab = InventoryTab.Materials;   _selectedIndex = 0; Refresh(); }

        /// <summary>Exposes the player's selected item for Character/Merchant cross-screen actions.</summary>
        public ItemRuntime GetSelectedItem()
        {
            var items = GetFilteredItems();
            if (items == null || items.Count == 0) return null;
            _selectedIndex = Mathf.Clamp(_selectedIndex, 0, items.Count - 1);
            return items[_selectedIndex];
        }

        public void Refresh()
        {
            if (_inventoryService == null) return;

            UpdateTabButtons();

            var allItems      = _inventoryService.GetAllItems();
            int capacity      = _inventoryService.GetCapacity();
            var filteredItems = GetFilteredItems();
            var characters    = _characterService?.GetAllCharacters();
            int equippedCount = InventoryOwnershipPresentation.CountEquippedInstances(characters);

            // Summary: "Items: 5 / 20  •  Equipped: 2"
            if (_summaryText != null)
                _summaryText.text = $"Available: {allItems.Count} / {capacity} • Equipped: {equippedCount}";

            BuildCards(filteredItems, allItems, characters);

            if (filteredItems.Count == 0)
            {
                _selectedIndex = -1;
                if (_detailText != null)
                    _detailText.text = _activeTab == InventoryTab.All
                        ? "Inventory is empty. Go on Dungeon Expeditions or Craft items to earn loot!"
                        : $"No {_activeTab} items found.";
                SetAllActionsDisabled();
                return;
            }

            _selectedIndex = Mathf.Clamp(_selectedIndex, 0, filteredItems.Count - 1);
            var sel = filteredItems[_selectedIndex];
            RefreshDetail(sel);
            RefreshActions(sel);
        }

        // ── Tab filtering ────────────────────────────────────────────────────────

        private IReadOnlyList<ItemRuntime> GetFilteredItems()
        {
            if (_inventoryService == null) return new List<ItemRuntime>();
            switch (_activeTab)
            {
                case InventoryTab.Weapons:
                    return _inventoryService.GetItemsByCategory(ItemCategory.Weapon);
                case InventoryTab.Armor:
                    // Armor + Accessory both map to the Armor tab
                    var armors = _inventoryService.GetItemsByCategory(ItemCategory.Armor)
                        .Concat(_inventoryService.GetItemsByCategory(ItemCategory.Accessory)).ToList();
                    return armors;
                case InventoryTab.Consumables:
                    return _inventoryService.GetItemsByCategory(ItemCategory.Consumable);
                case InventoryTab.Materials:
                    return _inventoryService.GetItemsByCategory(ItemCategory.Material);
                default:
                    return _inventoryService.GetAllItems();
            }
        }

        // No longer needed — using ItemCategory.Armor directly from backend.

        // ── Tab visuals ──────────────────────────────────────────────────────────

        private void UpdateTabButtons()
        {
            SetTabActive(_tabAllBtn,         _activeTab == InventoryTab.All);
            SetTabActive(_tabWeaponsBtn,     _activeTab == InventoryTab.Weapons);
            SetTabActive(_tabArmorBtn,       _activeTab == InventoryTab.Armor);
            SetTabActive(_tabConsumablesBtn, _activeTab == InventoryTab.Consumables);
            SetTabActive(_tabMaterialsBtn,   _activeTab == InventoryTab.Materials);
        }

        private static void SetTabActive(Button btn, bool active)
        {
            if (btn == null) return;
            var img = btn.GetComponent<Image>();
            if (img != null) img.color = active ? UITemporaryTheme.TabActive : UITemporaryTheme.TabInactive;
        }

        // ── Card list ────────────────────────────────────────────────────────────

        private void BuildCards(
            IReadOnlyList<ItemRuntime> items,
            IReadOnlyList<ItemRuntime> allAvailableItems,
            IReadOnlyList<CharacterRuntime> characters)
        {
            if (_cardContainer == null) return;
            UICardFactory.ClearContainer(_cardContainer);

            if (items == null || items.Count == 0)
            {
                string tabName = _activeTab == InventoryTab.All ? "INVENTORY IS EMPTY" : $"NO {_activeTab.ToString().ToUpper()} ITEMS";
                UICardFactory.CreateDivider(_cardContainer, tabName);
                UICardFactory.CreateCard(_cardContainer, "📦 Nothing Here",
                    _activeTab == InventoryTab.All
                        ? "Go on Dungeon Expeditions or Craft items to earn loot!"
                        : $"No items in this category. Switch tabs or earn more loot!",
                    false, false, null, preferredHeight: 65);
                return;
            }

            string dividerLabel = _activeTab == InventoryTab.All
                ? $"ALL ITEMS  ({items.Count})"
                : $"{_activeTab.ToString().ToUpper()}  ({items.Count})";
            UICardFactory.CreateDivider(_cardContainer, dividerLabel);

            for (int i = 0; i < items.Count; i++)
            {
                int captured = i;
                var item     = items[i];
                string defId = item.Definition != null ? item.Definition.id : "Unknown";
                var ownership = InventoryOwnershipPresentation.ForDefinition(defId, allAvailableItems, characters);

                // Keep ownership counts explicit when several instances share one definition.
                string sub = $"Available {ownership.Available} • Equipped {ownership.Equipped}";
                if (item.StackCount > 1) sub += $"x{item.StackCount}  ";
                if (item.IsLocked) sub += "🔒 Locked  ";
                if (IsEquipped(item)) sub += "⚔️ Equipped";

                UICardFactory.CreateCard(_cardContainer, defId, sub.Trim(),
                    i == _selectedIndex, true, () => SelectIndex(captured),
                    preferredHeight: 65);
            }
        }

        private static string GetCategoryLabel(ItemRuntime item)
        {
            if (item?.Definition == null) return "Item";
            if (item.Definition is ItemDefinition id)
            {
                switch (id.Category)
                {
                    case ItemCategory.Weapon:     return "Weapon";
                    case ItemCategory.Armor:      return "Armor";
                    case ItemCategory.Accessory:  return "Accessory";
                    case ItemCategory.Consumable: return "Consumable";
                    case ItemCategory.Material:   return "Material";
                    default:                      return "Item";
                }
            }
            return "Item";
        }

        private bool IsEquipped(ItemRuntime item)
        {
            if (item == null || _characterService == null) return false;
            foreach (var character in _characterService.GetAllCharacters())
            {
                if (character?.Weapon?.InstanceId == item.InstanceId ||
                    character?.Armor?.InstanceId == item.InstanceId ||
                    character?.Accessory?.InstanceId == item.InstanceId)
                    return true;
            }
            return false;
        }

        // ── Detail panel ─────────────────────────────────────────────────────────

        private void RefreshDetail(ItemRuntime item)
        {
            if (_detailText == null || item == null) return;

            string defId       = item.Definition != null ? item.Definition.id : "Unknown";
            bool isConsumable  = item.Definition is ItemDefinition id && id.Consumable;
            string typeLabel   = GetCategoryLabel(item);
            string lockStatus  = item.IsLocked ? "🔒 Locked (cannot sell)" : "🔓 Unlocked";
            var ownership = InventoryOwnershipPresentation.ForDefinition(
                defId,
                _inventoryService.GetAllItems(),
                _characterService?.GetAllCharacters());
            bool hasCharacter  = _characterScreen?.GetSelectedCharacter() != null;
            string useHint     = isConsumable
                ? (hasCharacter ? "→ Select 'Use' to consume on selected hero." : "→ Go to Characters tab and select a hero first.")
                : "→ Equip this via the Characters screen.";

            _detailText.text =
                $"📦 {defId}\n" +
                $"Type: {typeLabel}\n" +
                $"Quantity: x{item.StackCount}\n" +
                $"Ownership: {(IsEquipped(item) ? "Equipped" : "Available item")}\n" +
                $"Available count: {ownership.Available}\n" +
                $"Equipped count: {ownership.Equipped}\n" +
                $"Status: {lockStatus}\n" +
                $"\n{useHint}";
        }

        // ── Action buttons ───────────────────────────────────────────────────────

        private void RefreshActions(ItemRuntime item)
        {
            if (item == null) { SetAllActionsDisabled(); return; }

            bool isConsumable = item.Definition is ItemDefinition id && id.Consumable;
            bool hasCharacter = _characterScreen?.GetSelectedCharacter() != null;

            if (_useButton   != null) _useButton.interactable   = isConsumable && hasCharacter;
            if (_lockButton  != null) _lockButton.interactable   = true;
            if (_lockButtonLabel != null) _lockButtonLabel.text = item.IsLocked ? "Unlock" : "Lock";
            if (_sellButton  != null) _sellButton.interactable   = !item.IsLocked;
        }

        private void SetAllActionsDisabled()
        {
            if (_useButton   != null) _useButton.interactable   = false;
            if (_lockButton  != null) _lockButton.interactable   = false;
            if (_sellButton  != null) _sellButton.interactable   = false;
        }

        // ── Selection ────────────────────────────────────────────────────────────

        /// <summary>Click-to-select — also callable from tests.</summary>
        public void SelectIndex(int index)
        {
            var items = GetFilteredItems();
            if (items == null || items.Count == 0) return;
            _selectedIndex = Mathf.Clamp(index, 0, items.Count - 1);
            Refresh();
        }

        public void OnClickSelectNext()     => CycleSelection(1);
        public void OnClickSelectPrevious() => CycleSelection(-1);

        private void CycleSelection(int dir)
        {
            var items = GetFilteredItems();
            if (items == null || items.Count == 0) return;
            _selectedIndex = ((_selectedIndex + dir) % items.Count + items.Count) % items.Count;
            Refresh();
        }

        // ── Button handlers ──────────────────────────────────────────────────────

        public void OnClickToggleLockSelected()
        {
            var item = GetSelectedItem();
            if (item == null) return;
            bool wasLocked = item.IsLocked;
            _inventoryService.ToggleLockItem(item.InstanceId);
            ShowFeedback(wasLocked ? "Item unlocked." : "Item locked.", true);
            Refresh();
        }

        public void OnClickUseSelectedConsumable()
        {
            var item   = GetSelectedItem();
            var target = _characterScreen?.GetSelectedCharacter();
            if (item?.Definition is ItemDefinition itemDef && itemDef.Consumable && target != null)
            {
                _inventoryService.UseConsumable(item.InstanceId, target);
                ShowFeedback($"Used {itemDef.id} on {target.Definition?.id ?? "hero"}.", true);
            }
            else if (target == null)
            {
                ShowFeedback("Select a character in the Characters screen first!", false);
            }
            else
            {
                ShowFeedback("This item is not consumable.", false);
            }
            Refresh();
        }

        public void OnClickSellSelected()
        {
            var item = GetSelectedItem();
            if (item == null) { ShowFeedback("No item selected.", false); return; }
            if (item.IsLocked) { ShowFeedback("Cannot sell a locked item. Unlock it first!", false); return; }
            // Sell is handled via MerchantScreen which reads GetSelectedItem().
            ShowFeedback("Go to Merchant > Sell tab to sell this item.", false);
        }

        private void ShowFeedback(string msg, bool success)
        {
            if (_feedbackText == null) return;
            _feedbackText.text  = msg;
            _feedbackText.color = success ? UITemporaryTheme.SuccessColor : UITemporaryTheme.WarningColor;
        }
    }
}
