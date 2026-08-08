using System;
using System.Linq;
using GuildMaster.Definitions;
using GuildMaster.Runtime.Models;
using GuildMaster.Runtime.Services;
using GuildMaster.Runtime.UI.Inventory;
using GuildMaster.Runtime.UI.Legacy;
using UnityEngine;
using UnityEngine.UI;

namespace GuildMaster.Runtime.UI.Headquarters
{
    /// <summary>
    /// Phase 5C Storage — real Item Detail overlay (replaces the name+quantity-only placeholder).
    /// Reads every real, populated field on ItemDefinition/ItemRuntime and wires Equip/Unequip,
    /// Use, and Sell only where a clear backend API exists (IEquipmentService, IInventoryService,
    /// IMerchantService). Never mutates SaveData directly — every action goes through the real
    /// service call, and the panel only re-reads state afterward to refresh its own display.
    /// </summary>
    public sealed class StorageItemDetailPanel : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] private Image _icon;
        [SerializeField] private Text _nameText;
        [SerializeField] private Text _infoText;
        [SerializeField] private Text _hintText;
        [SerializeField] private Button _closeButton;

        [Header("Actions")]
        [SerializeField] private Button _unequipButton;
        [SerializeField] private Text _unequipButtonLabel;
        [SerializeField] private Button _useButton;
        [SerializeField] private Button _sellButton;

        private ServiceContainer _services;
        private Action _onClose;
        private Action _onItemChanged;
        private ItemRuntime _item;

        public void Setup(ServiceContainer services, Action onClose, Action onItemChanged)
        {
            _services = services;
            _onClose = onClose;
            _onItemChanged = onItemChanged;

            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveAllListeners();
                _closeButton.onClick.AddListener(() => _onClose?.Invoke());
            }
            if (_unequipButton != null)
            {
                _unequipButton.onClick.RemoveAllListeners();
                _unequipButton.onClick.AddListener(OnUnequipClicked);
            }
            if (_useButton != null)
            {
                _useButton.onClick.RemoveAllListeners();
                _useButton.onClick.AddListener(OnUseClicked);
            }
            if (_sellButton != null)
            {
                _sellButton.onClick.RemoveAllListeners();
                _sellButton.onClick.AddListener(OnSellClicked);
            }
        }

        public void Show(ItemRuntime item)
        {
            _item = item;
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            Refresh();
        }

        public void Hide()
        {
            _item = null;
            gameObject.SetActive(false);
        }

        private void Refresh()
        {
            if (_item?.Definition == null) { Hide(); return; }
            var def = _item.Definition;

            if (_icon != null)
            {
                Sprite sprite = LegacySpriteRegistry.GetItemSprite(def.id);
                _icon.sprite = sprite;
                _icon.color = sprite != null ? Color.white : new Color(1f, 1f, 1f, 0.25f);
            }

            if (_nameText != null) _nameText.text = FormatId(def.id);

            var owner = FindEquippedOwner(_item);
            bool isEquippable = def.Category == ItemCategory.Weapon || def.Category == ItemCategory.Armor || def.Category == ItemCategory.Accessory;

            var ownership = InventoryOwnershipPresentation.ForDefinition(
                def.id,
                _services.Inventory?.GetAllItems(),
                _services.Character?.GetAllCharacters());
            _infoText.text = BuildInfoText(_item, owner, ownership.Available, ownership.Equipped);

            // ── Unequip (only real, unambiguous equip action available without a character picker) ──
            if (_unequipButton != null)
            {
                bool canUnequip = owner.HasValue;
                _unequipButton.gameObject.SetActive(isEquippable);
                _unequipButton.interactable = canUnequip;
                if (_unequipButtonLabel != null) _unequipButtonLabel.text = "Unequip";
            }

            // ── Use (consumable) ──
            if (_useButton != null)
            {
                bool isConsumable = def.Category == ItemCategory.Consumable;
                _useButton.gameObject.SetActive(isConsumable);
                _useButton.interactable = isConsumable && !_item.IsLocked;
            }

            // ── Sell ──
            if (_sellButton != null)
            {
                bool canSell = !def.NotSellable && !_item.IsLocked;
                _sellButton.interactable = canSell;
            }

            if (_hintText != null) _hintText.text = BuildHintText(_item, owner, isEquippable);
        }

        private static string BuildInfoText(
            ItemRuntime item,
            (CharacterRuntime owner, EquipmentSlot slot)? equipped,
            int availableCount,
            int equippedCount)
        {
            var def = item.Definition;
            string typeLine = string.IsNullOrEmpty(def.ItemType) ? def.Category.ToString() : $"{def.Category} • {def.ItemType}";

            var lines = new System.Collections.Generic.List<string>
            {
                typeLine,
                $"Quantity: x{item.StackCount}",
                $"Available item: {availableCount > 0}",
                $"Available count: {availableCount}",
                $"Equipped count: {equippedCount}"
            };

            string stats = def.GetStatSummary();
            if (!string.IsNullOrEmpty(stats) && stats != def.Category.ToString() && stats != def.ItemType)
            {
                lines.Add($"Stats: {stats}");
            }

            // Price/Rarity are real fields on ItemDefinition but the current data pipeline never
            // assigns them (no JSON key maps to them — see phase_5c report "Known limitations").
            // Shown only when actually non-default, so a broken/empty field never masquerades as
            // real data ("Price: 0" for every single item in the game would be misleading).
            if (def.Price > 0) lines.Add($"Price: {def.Price}");
            if (def.Rarity != 0) lines.Add($"Rarity: {def.Rarity}");

            bool isEquippable = def.Category == ItemCategory.Weapon || def.Category == ItemCategory.Armor || def.Category == ItemCategory.Accessory;
            if (isEquippable)
            {
                lines.Add(equipped.HasValue
                    ? $"Equipped owner: {FormatId(equipped.Value.owner.DefinitionId)} ({equipped.Value.slot})"
                    : "Equipped owner: None");
            }

            if (item.IsLocked && !equipped.HasValue) lines.Add("🔒 Locked");

            return string.Join("\n", lines);
        }

        private static string BuildHintText(ItemRuntime item, (CharacterRuntime owner, EquipmentSlot slot)? equipped, bool isEquippable)
        {
            if (isEquippable && !equipped.HasValue)
                return "Equip this via the Characters screen.";
            if (item.Definition.NotSellable)
                return "This item cannot be sold.";
            if (item.IsLocked && !equipped.HasValue)
                return "Locked items cannot be sold. Unlock from the Inventory screen first.";
            return string.Empty;
        }

        // ── Actions ──────────────────────────────────────────────────────────────

        private void OnUnequipClicked()
        {
            if (_item == null || _services?.Equipment == null) return;
            var owner = FindEquippedOwner(_item);
            if (!owner.HasValue) return;

            bool success = _services.Equipment.Unequip(owner.Value.owner, owner.Value.slot);
            if (success) AfterAction();
        }

        private void OnUseClicked()
        {
            if (_item == null || _services?.Inventory == null) return;
            // No character-selection mechanism exists in the Storage/Item Detail context (that is
            // a UI-only concept owned by the legacy CharacterScreen), so this calls UseConsumable
            // with a null target — a real, supported call path (see InventoryService.UseConsumable:
            // the heal effect is skipped when targetCharacter is null, but the item is still consumed).
            bool success = _services.Inventory.UseConsumable(_item.InstanceId, null);
            if (success) AfterAction();
        }

        private void OnSellClicked()
        {
            if (_item?.Definition == null || _services?.Merchant == null) return;
            var result = _services.Merchant.SellItem(_item.Definition.id, _item.StackCount);
            if (result.Success) AfterAction();
        }

        /// <summary>
        /// Re-reads the item from the real backend after any action. If it still exists (partial
        /// stack remains, or the action didn't remove it), refresh the panel in place; if it's
        /// gone, close the overlay — never leave a stale/invalid item shown.
        /// </summary>
        private void AfterAction()
        {
            var refreshed = _services.Inventory.GetItem(_item.InstanceId);
            _onItemChanged?.Invoke();

            if (refreshed == null)
            {
                _onClose?.Invoke();
                return;
            }

            _item = refreshed;
            Refresh();
        }

        private (CharacterRuntime owner, EquipmentSlot slot)? FindEquippedOwner(ItemRuntime item)
        {
            if (_services?.Character == null || item == null) return null;
            foreach (var character in _services.Character.GetAllCharacters())
            {
                if (character.Weapon?.InstanceId == item.InstanceId) return (character, EquipmentSlot.Weapon);
                if (character.Armor?.InstanceId == item.InstanceId) return (character, EquipmentSlot.Armor);
                if (character.Accessory?.InstanceId == item.InstanceId) return (character, EquipmentSlot.Accessory);
            }
            return null;
        }

        private static string FormatId(string id)
        {
            if (string.IsNullOrEmpty(id)) return "Item";
            string normalized = id.Replace('_', ' ').Replace('-', ' ').Trim();
            return string.Join(" ", normalized.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(part => part.Length == 1 ? part.ToUpperInvariant() : char.ToUpperInvariant(part[0]) + part.Substring(1).ToLowerInvariant()));
        }
    }
}
