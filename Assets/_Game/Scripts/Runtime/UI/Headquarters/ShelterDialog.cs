using System.Collections.Generic;
using System.Linq;
using GuildMaster.Definitions;
using GuildMaster.Runtime.Models;
using GuildMaster.Runtime.Save;
using GuildMaster.Runtime.Services;
using GuildMaster.Runtime.UI.Legacy;
using UnityEngine;
using UnityEngine.UI;

namespace GuildMaster.Runtime.UI.Headquarters
{
    /// <summary>
    /// Phase 5E Shelter dialog. Reads IPetService only; never mutates SaveData itself. No
    /// capacity denominator is shown — IPetService exposes no capacity getter (FormulaService.ShelterCapacity
    /// exists but is never wired to any service method, mirroring Workshop's unwired time-upgrade
    /// formula — see report "Known limitations"). Count-only, matching the existing Headquarters
    /// card convention for Shelter. No Favourite/Autofeed indicator: neither field exists anywhere
    /// on PetSaveData/PetDefinition (see report audit).
    /// </summary>
    public sealed class ShelterDialog : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] private Text _titleText;
        [SerializeField] private Text _countText;
        [SerializeField] private Button _closeButton;

        [Header("Grid")]
        [SerializeField] private RectTransform _gridContent;
        [SerializeField] private GameObject _emptyState;
        [SerializeField] private Sprite _slotBorderSprite;

        [Header("Pet Detail (child overlay — not an AppShell popup)")]
        [SerializeField] private PetDetailPanel _detailPanel;

        private ServiceContainer _services;
        private System.Action _onClose;
        private System.Action _onStateChanged;

        public void Setup(ServiceContainer services, System.Action onClose, System.Action onStateChanged)
        {
            _services = services;
            _onClose = onClose;
            _onStateChanged = onStateChanged;

            if (_titleText != null) _titleText.text = "Shelter";

            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveAllListeners();
                _closeButton.onClick.AddListener(() => _onClose?.Invoke());
            }

            if (_detailPanel != null)
            {
                _detailPanel.Setup(_services, onClose: () => _detailPanel.Hide(), onChanged: Refresh);
                _detailPanel.Hide();
            }

            Refresh();
        }

        public void Refresh()
        {
            if (_services?.Pet == null) return;

            var pets = _services.Pet.GetAllPets();
            var eggs = GetPetEggs();
            if (_countText != null) _countText.text = $"{pets.Count} pet(s)  {eggs.Count} egg(s)";

            BuildGrid(pets, eggs);

            if (_emptyState != null) _emptyState.SetActive(pets.Count == 0 && eggs.Count == 0);
        }

        private List<ItemRuntime> GetPetEggs()
        {
            return (_services.Inventory?.GetAllItems() ?? new List<ItemRuntime>())
                .Where(item => item?.Definition != null &&
                               item.Definition.id.EndsWith("_egg", System.StringComparison.OrdinalIgnoreCase) &&
                               !string.Equals(item.Definition.id, "frozen_egg", System.StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        private void BuildGrid(IReadOnlyList<PetSaveData> pets, IReadOnlyList<ItemRuntime> eggs)
        {
            if (_gridContent == null) return;
            for (int i = _gridContent.childCount - 1; i >= 0; i--)
                Destroy(_gridContent.GetChild(i).gameObject);

            foreach (var pet in pets)
            {
                CreateSlot(pet);
            }

            foreach (var egg in eggs)
            {
                CreateEggSlot(egg);
            }
        }

        private void CreateSlot(PetSaveData pet)
        {
            var slot = new GameObject($"Slot_{pet.InstanceId}", typeof(RectTransform), typeof(Image), typeof(Button));
            slot.transform.SetParent(_gridContent, false);

            var border = slot.GetComponent<Image>();
            border.sprite = _slotBorderSprite;
            border.type = _slotBorderSprite != null ? Image.Type.Sliced : Image.Type.Simple;
            border.color = _slotBorderSprite != null ? Color.white : new Color(1f, 1f, 1f, 0.08f);

            var icon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            icon.transform.SetParent(slot.transform, false);
            var iconRect = (RectTransform)icon.transform;
            iconRect.anchorMin = new Vector2(0.14f, 0.32f);
            iconRect.anchorMax = new Vector2(0.86f, 0.94f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;
            var iconImage = icon.GetComponent<Image>();
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;
            string imageId = pet.DefinitionId;
            if (_services.Database.TryGet<PetDefinition>(pet.DefinitionId, out var petDefinition) &&
                !string.IsNullOrEmpty(petDefinition.IdImage))
                imageId = petDefinition.IdImage;
            Sprite iconSprite = LegacySpriteRegistry.GetPetSprite(imageId);
            iconImage.sprite = iconSprite;
            iconImage.color = iconSprite != null ? Color.white : new Color(1f, 1f, 1f, 0.25f);

            var nameGo = new GameObject("Name", typeof(RectTransform), typeof(Text));
            nameGo.transform.SetParent(slot.transform, false);
            var nameRect = (RectTransform)nameGo.transform;
            nameRect.anchorMin = new Vector2(0f, 0.12f);
            nameRect.anchorMax = new Vector2(1f, 0.30f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;
            var nameText = nameGo.GetComponent<Text>();
            nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            nameText.fontSize = 16;
            nameText.fontStyle = FontStyle.Bold;
            nameText.color = LegacyUITheme.DimWhite;
            nameText.alignment = TextAnchor.MiddleCenter;
            nameText.horizontalOverflow = HorizontalWrapMode.Wrap;
            nameText.text = (pet.Favourite ? "★ " : string.Empty) + PetFormat.FormatId(pet.DefinitionId);
            nameText.raycastTarget = false;

            var lvlGo = new GameObject("Level", typeof(RectTransform), typeof(Text));
            lvlGo.transform.SetParent(slot.transform, false);
            var lvlRect = (RectTransform)lvlGo.transform;
            lvlRect.anchorMin = new Vector2(0f, 0f);
            lvlRect.anchorMax = new Vector2(1f, 0.12f);
            lvlRect.offsetMin = Vector2.zero;
            lvlRect.offsetMax = Vector2.zero;
            var lvlText = lvlGo.GetComponent<Text>();
            lvlText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            lvlText.fontSize = 15;
            lvlText.color = LegacyUITheme.BrassBorder;
            lvlText.alignment = TextAnchor.MiddleCenter;
            lvlText.text = $"Lv.{pet.Level}  {(string.IsNullOrEmpty(pet.AssignedDungeonId) ? "Unassigned" : "Assigned")}";
            lvlText.raycastTarget = false;

            var button = slot.GetComponent<Button>();
            button.targetGraphic = border;
            button.onClick.AddListener(() => _detailPanel?.Show(pet));
        }

        private void CreateEggSlot(ItemRuntime egg)
        {
            if (egg?.Definition == null) return;
            var slot = new GameObject($"Egg_{egg.InstanceId}", typeof(RectTransform), typeof(Image), typeof(Button));
            slot.transform.SetParent(_gridContent, false);

            var border = slot.GetComponent<Image>();
            border.sprite = _slotBorderSprite;
            border.type = _slotBorderSprite != null ? Image.Type.Sliced : Image.Type.Simple;
            border.color = LegacyUITheme.BrassBorder;

            var icon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            icon.transform.SetParent(slot.transform, false);
            var iconRect = (RectTransform)icon.transform;
            iconRect.anchorMin = new Vector2(0.14f, 0.32f);
            iconRect.anchorMax = new Vector2(0.86f, 0.94f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;
            var iconImage = icon.GetComponent<Image>();
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;
            var sprite = ResolveEggSprite(egg.Definition);
            iconImage.sprite = sprite;
            iconImage.color = sprite != null ? Color.white : new Color(1f, 1f, 1f, 0.25f);

            var labelGo = new GameObject("Name", typeof(RectTransform), typeof(Text));
            labelGo.transform.SetParent(slot.transform, false);
            var labelRect = (RectTransform)labelGo.transform;
            labelRect.anchorMin = new Vector2(0f, 0.08f);
            labelRect.anchorMax = new Vector2(1f, 0.30f);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            var label = labelGo.GetComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 14;
            label.fontStyle = FontStyle.Bold;
            label.color = LegacyUITheme.BrassBorder;
            label.alignment = TextAnchor.MiddleCenter;
            label.text = $"HATCH x{egg.StackCount}";
            label.raycastTarget = false;

            var button = slot.GetComponent<Button>();
            button.targetGraphic = border;
            button.onClick.AddListener(() =>
            {
                if (_services.Pet.HatchEgg(egg.Definition.id) != null)
                {
                    _onStateChanged?.Invoke();
                    Refresh();
                }
            });
        }

        /// <summary>
        /// Uses the same generic resolver as the runtime egg slot. Keeping this boundary public
        /// lets EditMode tests verify exactly what Shelter will assign to Image.sprite without
        /// constructing a Canvas or invoking hatch logic.
        /// </summary>
        public static Sprite ResolveEggSprite(ItemDefinition definition)
        {
            if (definition == null) return null;
            string imageId = !string.IsNullOrEmpty(definition.IdImage) ? definition.IdImage : definition.id;
            return LegacySpriteRegistry.GetEggSprite(imageId);
        }
    }

    internal static class PetFormat
    {
        public static string FormatId(string id)
        {
            if (string.IsNullOrEmpty(id)) return "Pet";
            string normalized = id.Replace('_', ' ').Replace('-', ' ').Trim();
            var parts = normalized.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            var sb = new System.Text.StringBuilder();
            foreach (var part in parts)
            {
                if (sb.Length > 0) sb.Append(' ');
                sb.Append(part.Length == 1 ? part.ToUpperInvariant() : char.ToUpperInvariant(part[0]) + part.Substring(1).ToLowerInvariant());
            }
            return sb.ToString();
        }
    }
}
