using System;
using System.Collections.Generic;
using System.Linq;
using GuildMaster.Runtime.Save;
using GuildMaster.Runtime.Services;
using GuildMaster.Runtime.UI.Legacy;
using UnityEngine;
using UnityEngine.UI;

namespace GuildMaster.Runtime.UI.Headquarters
{
    /// <summary>
    /// Phase 5B Tavern dialog. This view only reads Tavern/Save services and delegates
    /// recruitment to ITavernService; it never edits SaveData or currency itself.
    /// </summary>
    public sealed class TavernDialog : MonoBehaviour
    {
        [SerializeField] private Text _guestCountText;
        [SerializeField] private Text _quartersText;
        [SerializeField] private Text _timerText;
        [SerializeField] private RectTransform _guestContent;
        [SerializeField] private GameObject _emptyState;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Sprite _cardFrameSprite;
        [SerializeField] private Sprite _buttonSprite;

        private ServiceContainer _services;
        private Action _onClose;
        private Action _onStateChanged;

        public void Setup(ServiceContainer services, Action onClose, Action onStateChanged)
        {
            _services = services;
            _onClose = onClose;
            _onStateChanged = onStateChanged;

            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveAllListeners();
                _closeButton.onClick.AddListener(() => _onClose?.Invoke());
            }

            Refresh();
        }

        public void Refresh()
        {
            if (_services?.Tavern == null) return;

            var guests = _services.Tavern.GetGuests() ?? Array.Empty<CharacterSaveData>();
            int tavernCapacity = _services.Tavern.GetTavernCapacity();
            int quartersCapacity = _services.Tavern.GetQuartersCapacity();
            int ownedCharacters = _services.Character?.GetAllCharacters()?.Count ?? 0;

            if (_guestCountText != null)
                _guestCountText.text = $"Guests {guests.Count}/{tavernCapacity}";
            if (_quartersText != null)
                _quartersText.text = $"Quarters {ownedCharacters}/{quartersCapacity}";
            if (_timerText != null)
            {
                long seconds = _services.Tavern.GetNextVisitorTimerSeconds();
                _timerText.text = guests.Count >= tavernCapacity
                    ? "Tavern full"
                    : seconds > 0 ? $"Next visitor in {FormatTimer(seconds)}" : "Visitor arriving soon";
            }

            ClearGuestCards();
            bool hasGuests = guests.Count > 0;
            if (_emptyState != null) _emptyState.SetActive(!hasGuests);

            if (!hasGuests || _guestContent == null) return;
            for (int i = 0; i < guests.Count; i++)
                CreateGuestCard(guests[i], i, _services.Tavern.CanRecruit());

            // Runtime-created cards are added after the prefab's first layout pass. Force one
            // deterministic pass so the first frame shown by PopupRoot already contains the
            // portrait/details/button instead of waiting for a later canvas update.
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(_guestContent);
        }

        private void CreateGuestCard(CharacterSaveData guest, int index, bool canRecruit)
        {
            if (guest == null || _guestContent == null) return;

            var card = new GameObject($"Guest_{index}_{guest.DefinitionId}", typeof(RectTransform), typeof(Image), typeof(LayoutElement), typeof(HorizontalLayoutGroup));
            card.transform.SetParent(_guestContent, false);

            var cardImage = card.GetComponent<Image>();
            cardImage.sprite = _cardFrameSprite;
            cardImage.type = Image.Type.Sliced;
            cardImage.color = new Color(1f, 1f, 1f, 0.92f);

            var layout = card.GetComponent<LayoutElement>();
            layout.preferredHeight = 220f;
            layout.minHeight = 220f;

            var row = card.GetComponent<HorizontalLayoutGroup>();
            row.padding = new RectOffset(24, 24, 20, 20);
            row.spacing = 22f;
            row.childAlignment = TextAnchor.MiddleLeft;
            row.childControlWidth = true;
            row.childControlHeight = true;
            row.childForceExpandWidth = false;
            row.childForceExpandHeight = false;

            var portrait = new GameObject("Portrait", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            portrait.transform.SetParent(card.transform, false);
            var portraitImage = portrait.GetComponent<Image>();
            portraitImage.sprite = LegacySpriteRegistry.GetUnitSprite(NormalizeLegacyId(guest.DefinitionId));
            portraitImage.preserveAspect = true;
            portraitImage.color = portraitImage.sprite != null ? Color.white : LegacyUITheme.GreyBorder;
            var portraitLayout = portrait.GetComponent<LayoutElement>();
            portraitLayout.preferredWidth = 150f;
            portraitLayout.preferredHeight = 150f;

            var details = new GameObject("Details", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            details.transform.SetParent(card.transform, false);
            var detailsLayout = details.GetComponent<LayoutElement>();
            detailsLayout.flexibleWidth = 1f;
            detailsLayout.preferredHeight = 176f;
            var detailsGroup = details.GetComponent<VerticalLayoutGroup>();
            detailsGroup.spacing = 5f;
            detailsGroup.childAlignment = TextAnchor.UpperLeft;
            detailsGroup.childControlWidth = true;
            detailsGroup.childControlHeight = true;
            detailsGroup.childForceExpandHeight = false;

            string displayName = FormatId(guest.DefinitionId);
            AddLabel(details.transform, "Name", displayName, 30, LegacyUITheme.DimWhite, FontStyle.Bold, 40);
            AddLabel(details.transform, "Class", $"Class: {displayName}", 22, LegacyUITheme.BrassBorder, FontStyle.Normal, 32);
            AddLabel(details.transform, "Level", $"Level {guest.Level}", 22, LegacyUITheme.DimWhite, FontStyle.Normal, 32);
            AddLabel(details.transform, "Traits", $"Traits: {FormatTraits(guest.Trait)}", 20, LegacyUITheme.DimWhite, FontStyle.Normal, 32);
            AddLabel(details.transform, "Cost", "Recruit: Free", 20, LegacyUITheme.DimWhite, FontStyle.Normal, 32);

            var recruit = CreateButton(card.transform, "RecruitButton", "RECRUIT", 180f, 64f);
            recruit.interactable = canRecruit && !string.IsNullOrEmpty(guest.DefinitionId);
            recruit.onClick.AddListener(() => OnRecruitClicked(index));
        }

        private void OnRecruitClicked(int index)
        {
            if (_services?.Tavern == null) return;
            if (!_services.Tavern.CanRecruit())
            {
                Refresh();
                return;
            }

            var guests = _services.Tavern.GetGuests();
            if (guests == null || index < 0 || index >= guests.Count || guests[index] == null) return;

            if (_services.Tavern.RecruitGuest(index, out var newCharacter) && newCharacter != null)
            {
                Refresh();
                _onStateChanged?.Invoke();
            }
            else
            {
                Refresh();
            }
        }

        private void ClearGuestCards()
        {
            if (_guestContent == null) return;
            for (int i = _guestContent.childCount - 1; i >= 0; i--)
                Destroy(_guestContent.GetChild(i).gameObject);
        }

        private static Text AddLabel(Transform parent, string name, string value, int fontSize, Color color, FontStyle style, float height)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.color = color;
            text.fontStyle = style;
            text.alignment = TextAnchor.MiddleLeft;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.text = value;
            go.GetComponent<LayoutElement>().preferredHeight = height;
            return text;
        }

        private Button CreateButton(Transform parent, string name, string label, float width, float height)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(LayoutElement), typeof(Button));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.sprite = _buttonSprite;
            image.type = Image.Type.Sliced;
            var button = go.GetComponent<Button>();
            button.targetGraphic = image;
            var layout = go.GetComponent<LayoutElement>();
            layout.preferredWidth = width;
            layout.preferredHeight = height;

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(go.transform, false);
            var rect = textGo.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var text = textGo.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 24;
            text.fontStyle = FontStyle.Bold;
            text.color = LegacyUITheme.DimWhite;
            text.alignment = TextAnchor.MiddleCenter;
            text.text = label;
            return button;
        }

        private static string NormalizeLegacyId(string id)
        {
            if (string.IsNullOrEmpty(id)) return string.Empty;
            return id.Trim().ToLowerInvariant().Replace(' ', '_').Replace('-', '_');
        }

        private static string FormatId(string id)
        {
            if (string.IsNullOrEmpty(id)) return "Adventurer";
            string normalized = id.Replace('_', ' ').Replace('-', ' ').Trim();
            return string.Join(" ", normalized.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(part => part.Length == 1 ? part.ToUpperInvariant() : char.ToUpperInvariant(part[0]) + part.Substring(1).ToLowerInvariant()));
        }

        private static string FormatTraits(string traits)
        {
            if (string.IsNullOrEmpty(traits)) return "None";
            return FormatId(traits);
        }

        private static string FormatTimer(long seconds)
        {
            if (seconds <= 0) return "00:00";
            long hours = seconds / 3600;
            long minutes = (seconds % 3600) / 60;
            long remaining = seconds % 60;
            return hours > 0 ? $"{hours}h {minutes:00}m" : $"{minutes:00}:{remaining:00}";
        }
    }
}
