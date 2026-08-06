using System;
using System.Collections.Generic;
using GuildMaster.Runtime.Models;
using GuildMaster.Runtime.Services;
using GuildMaster.Runtime.UI.Legacy;
using UnityEngine;
using UnityEngine.UI;

namespace GuildMaster.Runtime.UI.Headquarters
{
    /// <summary>
    /// Phase 5C Storage dialog (simplified — no filter/sort). Reads IInventoryService only;
    /// never writes SaveData, never mutates item instances itself (the real Item Detail overlay
    /// delegates every mutation to the real backend services). Shows the full GetAllItems() list
    /// in a 5-column grid. No storage-upgrade action exists here: IInventoryService exposes no
    /// upgrade method and no cost formula was found in FormulaService, so this dialog is
    /// capacity-display-only (see phase_5c report, "Known limitations").
    /// </summary>
    public sealed class StorageDialog : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] private Text _titleText;
        [SerializeField] private Text _capacityText;
        [SerializeField] private Button _closeButton;

        [Header("Grid")]
        [SerializeField] private RectTransform _gridContent;
        [SerializeField] private Sprite _itemSlotBorderSprite;

        [Header("Item Detail (real overlay — see StorageItemDetailPanel)")]
        [SerializeField] private StorageItemDetailPanel _itemDetailPanel;

        private ServiceContainer _services;
        private Action _onClose;
        private Action _onStateChanged;

        public void Setup(ServiceContainer services, Action onClose, Action onStateChanged)
        {
            _services = services;
            _onClose = onClose;
            _onStateChanged = onStateChanged;

            if (_titleText != null) _titleText.text = "Storage";

            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveAllListeners();
                _closeButton.onClick.AddListener(() => _onClose?.Invoke());
            }

            if (_itemDetailPanel != null)
            {
                _itemDetailPanel.Setup(_services, onClose: CloseItemDetail, onItemChanged: OnItemChanged);
                _itemDetailPanel.Hide();
            }

            Refresh();
        }

        public void Refresh()
        {
            if (_services?.Inventory == null) return;

            var allItems = _services.Inventory.GetAllItems();
            int capacity = _services.Inventory.GetCapacity();
            if (_capacityText != null) _capacityText.text = $"{allItems.Count} / {capacity}";

            BuildGrid(allItems);
        }

        /// <summary>Called by the Item Detail panel after a successful Equip/Use/Sell action.</summary>
        private void OnItemChanged()
        {
            Refresh();
            _onStateChanged?.Invoke();
        }

        // ── Grid ─────────────────────────────────────────────────────────────────

        private void BuildGrid(IReadOnlyList<ItemRuntime> items)
        {
            if (_gridContent == null) return;
            for (int i = _gridContent.childCount - 1; i >= 0; i--)
                Destroy(_gridContent.GetChild(i).gameObject);

            foreach (var item in items)
            {
                CreateSlot(item);
            }
        }

        private void CreateSlot(ItemRuntime item)
        {
            var slot = new GameObject($"Slot_{item.Definition?.id ?? "unknown"}",
                typeof(RectTransform), typeof(Image), typeof(Button));
            slot.transform.SetParent(_gridContent, false);

            var border = slot.GetComponent<Image>();
            border.sprite = _itemSlotBorderSprite;
            border.type = _itemSlotBorderSprite != null ? Image.Type.Sliced : Image.Type.Simple;
            border.color = _itemSlotBorderSprite != null ? Color.white : new Color(1f, 1f, 1f, 0.08f);

            var icon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            icon.transform.SetParent(slot.transform, false);
            var iconRect = (RectTransform)icon.transform;
            iconRect.anchorMin = new Vector2(0.12f, 0.16f);
            iconRect.anchorMax = new Vector2(0.88f, 0.92f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;
            var iconImage = icon.GetComponent<Image>();
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;
            Sprite iconSprite = item.Definition != null ? LegacySpriteRegistry.GetItemSprite(item.Definition.id) : null;
            iconImage.sprite = iconSprite;
            iconImage.color = iconSprite != null ? Color.white : new Color(1f, 1f, 1f, 0.25f);

            if (item.StackCount > 1)
            {
                var qty = new GameObject("Quantity", typeof(RectTransform), typeof(Text));
                qty.transform.SetParent(slot.transform, false);
                var qtyRect = (RectTransform)qty.transform;
                qtyRect.anchorMin = new Vector2(0f, 0f);
                qtyRect.anchorMax = new Vector2(1f, 0.28f);
                qtyRect.offsetMin = Vector2.zero;
                qtyRect.offsetMax = Vector2.zero;
                var qtyText = qty.GetComponent<Text>();
                qtyText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                qtyText.fontSize = 22;
                qtyText.fontStyle = FontStyle.Bold;
                qtyText.color = LegacyUITheme.DimWhite;
                qtyText.alignment = TextAnchor.LowerRight;
                qtyText.text = $"x{item.StackCount}";
                qtyText.raycastTarget = false;
            }

            var button = slot.GetComponent<Button>();
            button.targetGraphic = border;
            button.onClick.AddListener(() => _itemDetailPanel?.Show(item));
        }

        // ── Item detail (child overlay, no AppShell popup involved) ────────────────

        private void CloseItemDetail()
        {
            _itemDetailPanel?.Hide();
        }
    }
}
