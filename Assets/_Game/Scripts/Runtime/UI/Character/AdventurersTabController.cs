using System;
using GuildMaster.Runtime.Models;
using GuildMaster.Runtime.Services;
using GuildMaster.Runtime.UI.Core;
using GuildMaster.Runtime.UI.Legacy;
using UnityEngine;
using UnityEngine.UI;

namespace GuildMaster.Runtime.UI.Character
{
    /// <summary>
    /// Safe Phase 6 entry point. It does no work in Awake/OnEnable; AppShell calls Setup only
    /// after the existing Headquarters shell has initialized successfully.
    /// </summary>
    public sealed class AdventurersTabController : MonoBehaviour
    {
        private const string GeneratedRootName = "Phase6AdventurersContent";
        private ServiceContainer _services;
        private RectTransform _list;
        private CharacterDetailPanel _detailPanel;
        private string _selectedInstanceId;
        private bool _isSetup;

        public void Setup(ServiceContainer services)
        {
            if (_isSetup && ReferenceEquals(_services, services))
            {
                Refresh();
                return;
            }

            if (services?.Character == null)
            {
                Debug.LogWarning("[Phase6] Adventurers setup skipped: Character service unavailable.");
                return;
            }

            try
            {
                _services = services;
                var placeholder = GetComponent<GuildMaster.Runtime.UI.Shell.TabPlaceholderView>();
                if (placeholder != null) Destroy(placeholder);
                BuildRosterRoot();
                _detailPanel = gameObject.GetComponent<CharacterDetailPanel>() ?? gameObject.AddComponent<CharacterDetailPanel>();
                _detailPanel.Initialize(services, this);
                _isSetup = true;
                Refresh();
            }
            catch (Exception ex)
            {
                _isSetup = false;
                _list = null;
                Debug.LogError($"[Phase6] Adventurers setup failed; placeholder retained. {ex}");
            }
        }

        public void Refresh()
        {
            if (!_isSetup || _list == null || _services?.Character == null) return;
            UICardFactory.ClearContainer(_list);

            var characters = _services.Character.GetAllCharacters();
            int capacity = _services.Tavern != null
                ? _services.Tavern.GetQuartersCapacity()
                : characters.Count;
            CreateRosterHeader(characters.Count, capacity);

            if (characters.Count == 0)
            {
                CreateLegacyText(_list, "Empty", "No adventurers recruited yet.", 16, LegacyUITheme.DimWhite, 48, TextAnchor.MiddleCenter);
                return;
            }

            foreach (var character in characters)
            {
                if (character == null) continue;
                try
                {
                    CreateLegacyAdventurerCard(character);
                }
                catch (Exception ex)
                {
                    // One malformed definition/sprite must not remove the rest of the roster.
                    Debug.LogWarning($"[Phase6] Adventurer card skipped detail for {character.InstanceId}: {ex.Message}");
                    CreateLegacyText(_list, "Fallback_" + character.InstanceId, FormatId(character.Definition?.id ?? character.InstanceId), 16, LegacyUITheme.DimWhite, 60, TextAnchor.MiddleLeft);
                }
            }
        }

        public string SelectedInstanceId => _selectedInstanceId;

        private void SelectCharacter(CharacterRuntime character)
        {
            _selectedInstanceId = character?.InstanceId;
            Refresh();
            Debug.Log($"[Phase6] Adventurer selected: {_selectedInstanceId}");
        }

        private void OpenCharacter(CharacterRuntime character)
        {
            if (character == null) return;
            _selectedInstanceId = character.InstanceId;
            _detailPanel?.Open(character);
        }

        public void ReturnToRoster()
        {
            if (_detailPanel != null) _detailPanel.Close();
            Refresh();
        }

        private void BuildRosterRoot()
        {
            var existing = transform.Find(GeneratedRootName);
            if (existing != null) Destroy(existing.gameObject);

            var root = new GameObject(GeneratedRootName, typeof(RectTransform), typeof(Image));
            var rootRect = root.GetComponent<RectTransform>();
            rootRect.SetParent(transform, false);
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = new Vector2(18, 18);
            rootRect.offsetMax = new Vector2(-18, -18);
            root.GetComponent<Image>().color = LegacyUITheme.CardviewDarkBackground;

            var scrollObject = new GameObject("RosterScroll", typeof(RectTransform), typeof(ScrollRect), typeof(RectMask2D));
            var scrollRect = scrollObject.GetComponent<RectTransform>();
            scrollRect.SetParent(rootRect, false);
            scrollRect.anchorMin = Vector2.zero; scrollRect.anchorMax = Vector2.one;
            scrollRect.offsetMin = new Vector2(12, 12); scrollRect.offsetMax = new Vector2(-12, -12);

            var contentObject = new GameObject("RosterContent", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            _list = contentObject.GetComponent<RectTransform>();
            _list.SetParent(scrollRect, false);
            _list.anchorMin = new Vector2(0, 1); _list.anchorMax = new Vector2(1, 1);
            _list.pivot = new Vector2(.5f, 1); _list.sizeDelta = Vector2.zero;
            var layout = contentObject.GetComponent<VerticalLayoutGroup>();
            layout.spacing = LegacyUITheme.DP(8); layout.padding = new RectOffset(LegacyUITheme.DP(4), LegacyUITheme.DP(4), LegacyUITheme.DP(4), LegacyUITheme.DP(4));
            layout.childControlWidth = true; layout.childControlHeight = true; layout.childForceExpandWidth = true; layout.childForceExpandHeight = false;
            var fitter = contentObject.GetComponent<ContentSizeFitter>(); fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var scroll = scrollObject.GetComponent<ScrollRect>(); scroll.horizontal = false; scroll.vertical = true; scroll.viewport = scrollRect; scroll.content = _list;
        }

        private void CreateRosterHeader(int count, int capacity)
        {
            var header = new GameObject("AdventurersHeader", typeof(RectTransform), typeof(Image), typeof(Outline));
            var rt = header.GetComponent<RectTransform>(); rt.SetParent(_list, false);
            var le = header.AddComponent<LayoutElement>(); le.preferredHeight = LegacyUITheme.DP(52); le.flexibleWidth = 1;
            header.GetComponent<Image>().color = LegacyUITheme.StandardBackground;
            var outline = header.GetComponent<Outline>(); outline.effectColor = LegacyUITheme.GreyBorder; outline.effectDistance = new Vector2(2, -2);
            var group = header.AddComponent<HorizontalLayoutGroup>(); group.padding = new RectOffset(LegacyUITheme.DP(12), LegacyUITheme.DP(12), LegacyUITheme.DP(4), LegacyUITheme.DP(4)); group.childControlWidth = true; group.childControlHeight = true; group.childForceExpandWidth = false; group.spacing = LegacyUITheme.DP(8); group.childAlignment = TextAnchor.MiddleLeft;
            var icon = CreateLegacyImage(header.transform, "TabIcon", LegacyUITheme.DP(34)); icon.sprite = LegacySpriteRegistry.GetDrawerIconSprite("drawer_icon_recall_adventurers"); icon.preserveAspect = true;
            CreateLegacyText(header.transform, "Title", "ADVENTURERS", 20, LegacyUITheme.DimWhite, LegacyUITheme.DP(34), TextAnchor.MiddleLeft, true);
            CreateLegacyText(header.transform, "Count", $"{count}/{capacity}", 16, LegacyUITheme.BrassBorder, LegacyUITheme.DP(34), TextAnchor.MiddleRight);
        }

        private void CreateLegacyAdventurerCard(CharacterRuntime character)
        {
            bool selected = character.InstanceId == _selectedInstanceId;
            int maxHp = Math.Max(1, SafeMaxHp(character));
            int party = _services.Party?.GetPartyIndexOf(character.InstanceId) ?? -1;
            string partyText = party >= 0 ? $"★ Party {party + 1}" : "Unassigned";
            string name = FormatId(character.Definition?.id ?? character.InstanceId);
            string classText = $"{FormatId(character.Definition?.id)}  ·  Lv.{character.Level}";

            var go = new GameObject("AdventurerCard_" + character.InstanceId, typeof(RectTransform), typeof(Image), typeof(Outline), typeof(Button));
            var rt = go.GetComponent<RectTransform>(); rt.SetParent(_list, false);
            var layoutElement = go.AddComponent<LayoutElement>(); layoutElement.preferredHeight = LegacyUITheme.DP(76); layoutElement.minHeight = LegacyUITheme.DP(76); layoutElement.flexibleWidth = 1;
            var background = go.GetComponent<Image>(); background.color = selected ? LegacyUITheme.AscendedBackground : LegacyUITheme.StandardBackground;
            var border = go.GetComponent<Outline>(); border.effectColor = selected ? LegacyUITheme.BrassBorder : LegacyUITheme.DimWhite;
            border.effectDistance = new Vector2(selected ? 3 : 2, selected ? -3 : -2);
            var row = go.AddComponent<HorizontalLayoutGroup>(); row.padding = new RectOffset(LegacyUITheme.DP(4), LegacyUITheme.DP(8), LegacyUITheme.DP(4), LegacyUITheme.DP(4)); row.spacing = LegacyUITheme.DP(8); row.childControlWidth = true; row.childControlHeight = true; row.childForceExpandWidth = false; row.childForceExpandHeight = true; row.childAlignment = TextAnchor.MiddleLeft;

            var portrait = CreateLegacyImage(row.transform, "Portrait", LegacyUITheme.DP(60));
            portrait.sprite = LegacySpriteRegistry.GetUnitSprite(character.Definition?.id); portrait.preserveAspect = true; portrait.color = portrait.sprite == null ? LegacyUITheme.GreyBorder : Color.white;
            AddLevelBadge(portrait.transform, character.Level);

            var textColumn = new GameObject("Text", typeof(RectTransform)); textColumn.transform.SetParent(row.transform, false);
            var textLayout = textColumn.AddComponent<LayoutElement>(); textLayout.flexibleWidth = 1; textLayout.minWidth = LegacyUITheme.DP(80);
            var textGroup = textColumn.AddComponent<VerticalLayoutGroup>(); textGroup.spacing = 1; textGroup.childControlWidth = true; textGroup.childControlHeight = true; textGroup.childForceExpandWidth = true; textGroup.childForceExpandHeight = false; textGroup.childAlignment = TextAnchor.MiddleLeft;
            CreateLegacyText(textColumn.transform, "Name", name, 16, selected ? LegacyUITheme.AscendedUnit : LegacyUITheme.DimWhite, 22, TextAnchor.MiddleLeft);
            CreateLegacyText(textColumn.transform, "ClassLevel", classText, 13, LegacyUITheme.DimWhite, 18, TextAnchor.MiddleLeft);
            CreateHpBar(textColumn.transform, character.CurrentHp, maxHp);
            string trait = string.IsNullOrEmpty(character.Trait) ? partyText : $"{character.Trait}  ·  {partyText}";
            CreateLegacyText(textColumn.transform, "TraitParty", trait, 11, LegacyUITheme.BrassBorder, 16, TextAnchor.MiddleLeft);

            CreateEquipmentSlot(row.transform, "Weapon", character.Weapon);
            CreateEquipmentSlot(row.transform, "Armor", character.Armor);
            CreateEquipmentSlot(row.transform, "Accessory", character.Accessory);

            var button = go.GetComponent<Button>(); button.targetGraphic = background; button.interactable = true; button.onClick.AddListener(() => OpenCharacter(character));
        }

        private static void CreateEquipmentSlot(Transform parent, string name, ItemRuntime item)
        {
            var image = CreateLegacyImage(parent, name, LegacyUITheme.DP(44));
            image.sprite = item == null ? LegacySpriteRegistry.GetItemSprite("empty_equipment") : LegacySpriteRegistry.GetItemSprite(item.Definition?.id);
            image.color = image.sprite == null ? new Color(LegacyUITheme.DimWhite.r, LegacyUITheme.DimWhite.g, LegacyUITheme.DimWhite.b, .22f) : Color.white;
            image.preserveAspect = true;
            var outline = image.gameObject.AddComponent<Outline>(); outline.effectColor = LegacyUITheme.DimWhite; outline.effectDistance = new Vector2(1, -1);
        }

        private static void CreateHpBar(Transform parent, int current, int max)
        {
            var root = new GameObject("HP", typeof(RectTransform), typeof(Image)); root.transform.SetParent(parent, false);
            var le = root.AddComponent<LayoutElement>(); le.preferredHeight = 8; le.flexibleWidth = 1;
            root.GetComponent<Image>().color = new Color(0, 0, 0, .75f);
            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image)); var fillRt = fill.GetComponent<RectTransform>(); fillRt.SetParent(root.transform, false); fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = new Vector2(Mathf.Clamp01((float)current / max), 1); fillRt.offsetMin = Vector2.zero; fillRt.offsetMax = Vector2.zero; fill.GetComponent<Image>().color = new Color(1f, .35f, .35f, 1f);
        }

        private static void AddLevelBadge(Transform portrait, int level)
        {
            var badge = new GameObject("Level", typeof(RectTransform), typeof(Image)); var rt = badge.GetComponent<RectTransform>(); rt.SetParent(portrait, false); rt.anchorMin = new Vector2(1, 0); rt.anchorMax = new Vector2(1, 0); rt.pivot = new Vector2(1, 0); rt.sizeDelta = new Vector2(42, 20); rt.anchoredPosition = new Vector2(2, 2); badge.GetComponent<Image>().color = LegacyUITheme.CardviewDarkBackground;
            CreateLegacyText(badge.transform, "Text", level.ToString(), 11, LegacyUITheme.DimWhite, 20, TextAnchor.MiddleCenter);
        }

        private static Image CreateLegacyImage(Transform parent, string name, float size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent, false); var le = go.AddComponent<LayoutElement>(); le.preferredWidth = size; le.preferredHeight = size; le.minWidth = size; le.minHeight = size; le.flexibleWidth = 0; le.flexibleHeight = 0; var image = go.GetComponent<Image>(); image.color = LegacyUITheme.StandardBackground; image.raycastTarget = false; return image;
        }

        private static Text CreateLegacyText(Transform parent, string name, string value, int size, Color color, float height, TextAnchor alignment, bool flexible = false)
        {
            var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false); var le = go.AddComponent<LayoutElement>(); le.preferredHeight = height; le.flexibleWidth = flexible ? 1 : 0; var text = go.AddComponent<Text>(); text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = size; text.fontStyle = size >= 16 ? FontStyle.Bold : FontStyle.Normal; text.color = color; text.text = value ?? ""; text.alignment = alignment; text.horizontalOverflow = HorizontalWrapMode.Overflow; text.verticalOverflow = VerticalWrapMode.Truncate; text.raycastTarget = false; return text;
        }

        private int SafeMaxHp(CharacterRuntime character)
        {
            try { return _services.Character.GetTotalStat(character, GuildMaster.Definitions.StatType.MaxHp); }
            catch (Exception ex) { Debug.LogWarning($"[Phase6] Max HP unavailable for {character.InstanceId}: {ex.Message}"); return character.CurrentHp; }
        }

        private static string FormatId(string id) => string.IsNullOrEmpty(id) ? "Unknown" : id.Replace('_', ' ').Replace('-', ' ');
    }
}
