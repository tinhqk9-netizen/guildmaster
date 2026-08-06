using System;
using System.Collections.Generic;
using System.Linq;
using GuildMaster.Definitions;
using GuildMaster.Runtime.Models;
using GuildMaster.Runtime.Save;
using GuildMaster.Runtime.Services;
using GuildMaster.Runtime.UI.Core;
using GuildMaster.Runtime.UI.Legacy;
using UnityEngine;
using UnityEngine.UI;

namespace GuildMaster.Runtime.UI.Character
{
    /// <summary>
    /// Phase 6 internal character flow. It owns only a generated child overlay under
    /// Tab_Adventurers; it never calls AppShell popup navigation or mutates SaveData directly.
    /// </summary>
    public sealed class CharacterDetailPanel : MonoBehaviour
    {
        private ServiceContainer _services;
        private AdventurersTabController _roster;
        private CharacterRuntime _character;
        private GameObject _overlay;
        private RectTransform _body;

        public void Initialize(ServiceContainer services, AdventurersTabController roster)
        {
            _services = services;
            _roster = roster;
        }

        public void Open(CharacterRuntime character)
        {
            if (character == null || _services?.Character == null) return;
            _character = character;
            CloseOverlayOnly();
            _overlay = CreateOverlay("CharacterDetailOverlay");
            _body = CreateScrollBody(_overlay.transform);
            BuildDetail();
        }

        public void Close()
        {
            CloseOverlayOnly();
            _character = null;
        }

        private void BuildDetail()
        {
            var c = _character;
            AddText(_body, "Title", "ADVENTURER DETAIL", 28, UITemporaryTheme.TextHighlight, 52);
            var identity = AddRow(_body, "Identity", 90);
            var portrait = CreateImage(identity.transform, "Portrait", 72);
            portrait.sprite = LegacySpriteRegistry.GetUnitSprite(c.Definition?.id);
            portrait.preserveAspect = true;
            portrait.color = portrait.sprite == null ? UITemporaryTheme.PlaceholderIcon : Color.white;
            AddText(identity.transform, "Name", Format(c.Definition?.id ?? c.InstanceId) + $"\nLv.{c.Level}  Rank {c.AscensionLevel}", 20, UITemporaryTheme.TextPrimary, 72, true);

            AddText(_body, "Progress", $"EXP: {c.Experience}    HP: {c.CurrentHp}/{SafeStat(c, StatType.MaxHp)}", 17, LegacyUITheme.DimWhite, 32);
            CreateProgressBar(_body, "HP", c.CurrentHp, Math.Max(1, SafeStat(c, StatType.MaxHp)), new Color(1f, .35f, .35f, 1f));
            AddText(_body, "Class", $"Class/Job: {Format(c.Definition?.id)}    Trait: {Format(c.Trait)}", 17, UITemporaryTheme.TextSecondary, 32);

            AddText(_body, "StatsHeader", "COMBAT STATS", 19, UITemporaryTheme.TextHighlight, 34);
            AddStat("Constitution", SafeStat(c, StatType.Constitution));
            AddStat("Dexterity", SafeStat(c, StatType.Dexterity));
            AddStat("Intelligence", SafeStat(c, StatType.Intelligence));
            AddStat("Defense", SafeStat(c, StatType.Defense));
            AddStat("Magic Defense", SafeStat(c, StatType.MagicDefense));
            AddStat("Immunity", SafeStat(c, StatType.ImmunityToStatus));

            AddText(_body, "SkillsHeader", "SKILLS / EQUIPMENT", 19, UITemporaryTheme.TextHighlight, 34);
            AddText(_body, "Skills", $"Active: {Format(c.Definition?.ActiveSkill)}\nPassive: {Format(c.Definition?.PassiveSkill)}", 16, UITemporaryTheme.TextSecondary, 44);
            AddEquipmentSummary(c);
            var pets = _services.Pet?.GetCharacterPets(c.InstanceId);
            AddText(_body, "Pet", $"Pet: {(pets != null && pets.Count > 0 ? string.Join(", ", pets.Select(p => Format(p.DefinitionId))) : "None equipped")}", 16, UITemporaryTheme.TextSecondary, 30);

            AddButton(_body, "Equipment", "Equipment", () => ShowEquipment(), true);
            AddButton(_body, "Promotion", "Promotion", () => ShowPromotion(), true);
            AddButton(_body, "Doctrine", "Doctrine", () => ShowDoctrine(), true);
            AddButton(_body, "Back", "Back to Adventurers", () => _roster?.ReturnToRoster(), true);
        }

        private void ShowEquipment()
        {
            CloseOverlayOnly();
            _overlay = CreateOverlay("EquipmentOverlay");
            _body = CreateScrollBody(_overlay.transform);
            AddText(_body, "Title", "EQUIPMENT", 28, UITemporaryTheme.TextHighlight, 52);
            AddText(_body, "Summary", Format(_character.Definition?.id) + $"  Lv.{_character.Level}", 18, UITemporaryTheme.TextSecondary, 32);
            BuildEquipmentSlot(EquipmentSlot.Weapon, _character.Weapon);
            BuildEquipmentSlot(EquipmentSlot.Armor, _character.Armor);
            BuildEquipmentSlot(EquipmentSlot.Accessory, _character.Accessory);
            AddButton(_body, "Back", "Back to Detail", () => Open(_character), true);
        }

        private void BuildEquipmentSlot(EquipmentSlot slot, ItemRuntime equipped)
        {
            var current = AddRow(_body, slot.ToString().ToUpperInvariant(), 66);
            var currentIcon = CreateImage(current.transform, "Icon", 52);
            currentIcon.sprite = equipped == null ? LegacySpriteRegistry.GetItemSprite("empty_equipment") : LegacySpriteRegistry.GetItemSprite(equipped.Definition?.id);
            currentIcon.preserveAspect = true;
            if (equipped == null && currentIcon.sprite == null) AddEmptySlotLabel(currentIcon.transform);
            AddText(current.transform, "Name", slot + ": " + (equipped == null ? "Empty" : Format(equipped.Definition?.id)), 17, LegacyUITheme.DimWhite, 52, true);
            if (equipped != null)
            {
                AddText(_body, slot + "Stats", equipped.Definition?.GetStatSummary() ?? "No stats exposed", 15, UITemporaryTheme.TextSecondary, 28);
                AddButton(_body, "Unequip_" + slot, "Unequip " + slot, () =>
                {
                    if (_services.Equipment.Unequip(_character, slot))
                    {
                        _services.Save.Save(out _);
                        ShowEquipment();
                    }
                }, true);
            }

            var compatible = (_services.Inventory?.GetAllItems() ?? Array.Empty<ItemRuntime>())
                .Where(i => i != null && !i.IsLocked && _services.Equipment.CanEquip(_character, i, slot))
                .ToList();
            if (compatible.Count == 0)
            {
                AddText(_body, "Empty_" + slot, "No compatible unlocked items.", 14, UITemporaryTheme.TextSecondary, 28);
                return;
            }

            AddText(_body, "Picker_" + slot, "Compatible items", 15, UITemporaryTheme.TextSecondary, 28);
            foreach (var item in compatible)
            {
                var captured = item;
                CreateLegacyItemCard(_body, captured, "Equip", () =>
                {
                    if (_services.Equipment.Equip(_character, captured.InstanceId, slot))
                    {
                        _services.Save.Save(out _);
                        ShowEquipment();
                    }
                });
            }
        }

        private void ShowPromotion()
        {
            CloseOverlayOnly();
            _overlay = CreateOverlay("PromotionOverlay");
            _body = CreateScrollBody(_overlay.transform);
            AddText(_body, "Title", "PROMOTION", 28, UITemporaryTheme.TextHighlight, 52);
            AddText(_body, "Current", $"Current rank: {Mathf.Max(0, _character.AscensionLevel)}    Level: {_character.Level}", 18, UITemporaryTheme.TextPrimary, 36);

            var saveCharacter = _services.Save.CurrentData?.Characters?.FirstOrDefault(x => x.InstanceId == _character.InstanceId);
            var promotions = saveCharacter == null ? Array.Empty<PromotionDefinition>() : _services.Promotion.GetAvailablePromotions(saveCharacter).ToArray();
            if (promotions.Length == 0)
            {
                AddText(_body, "Locked", "No eligible promotion returned by PromotionService.", 16, UITemporaryTheme.TextSecondary, 40);
                AddButton(_body, "PromotionUnavailable", "Promotion unavailable", null, false);
            }
            else
            {
                foreach (var promo in promotions)
                {
                    var captured = promo;
                    bool can = _services.Promotion.CanPromote(saveCharacter, promo.id);
                    int owned = string.IsNullOrEmpty(promo.RequiredItemId) ? 0 : _services.Inventory.GetQuantityByDefinitionId(promo.RequiredItemId);
                    string req = string.IsNullOrEmpty(promo.RequiredItemId) ? "No item requirement" : $"{Format(promo.RequiredItemId)}: {owned}/{promo.RequiredItemCount}";
                    AddText(_body, "Next_" + promo.id, $"Next: {Format(promo.TierName)} (Tier {promo.TierIndex})\nRequired level: {promo.RequiredLevel}", 17, LegacyUITheme.DimWhite, 52);
                    if (!string.IsNullOrEmpty(promo.RequiredItemId))
                    {
                        var requirement = AddRow(_body, "Requirement_" + promo.id, 54);
                        var requirementIcon = CreateImage(requirement.transform, "Icon", 42);
                        requirementIcon.sprite = LegacySpriteRegistry.GetItemSprite(promo.RequiredItemId);
                        requirementIcon.preserveAspect = true;
                        AddText(requirement.transform, "Text", req, 15, can ? LegacyUITheme.DimWhite : LegacyUITheme.Failure, 42, true);
                    }
                    else AddText(_body, "Requirement_" + promo.id, req, 15, LegacyUITheme.DimWhite, 28);
                    AddButton(_body, "Promote_" + promo.id, can ? "Promote" : "Promotion unavailable", () =>
                    {
                        if (_services.Promotion.Promote(saveCharacter, captured.id))
                        {
                            _character = _services.Character.GetAllCharacters().FirstOrDefault(x => x.InstanceId == _character.InstanceId) ?? _character;
                            _roster.Refresh();
                            ShowPromotion();
                        }
                    }, can);
                }
            }
            AddButton(_body, "Back", "Back to Detail", () => Open(_character), true);
        }

        private void ShowDoctrine()
        {
            CloseOverlayOnly();
            _overlay = CreateOverlay("DoctrineOverlay");
            _body = CreateScrollBody(_overlay.transform);
            AddText(_body, "Title", "DOCTRINE", 28, UITemporaryTheme.TextHighlight, 52);
            AddText(_body, "ReadOnly", "Read-only: current backend exposes doctrine progress but no character assign or upgrade API.", 15, UITemporaryTheme.TextSecondary, 46);
            string[] names = { "affliction", "control", "fortitude", "grace", "illusion", "knowledge", "ruin", "war" };
            foreach (var name in names)
            {
                int level = _services.Doctrine.GetLevel(name);
                int progress = _services.Doctrine.GetProgress(name);
                var row = AddRow(_body, "Doctrine_" + name, 54);
                var icon = CreateImage(row.transform, "Icon", 42);
                icon.sprite = GetDoctrineLegacySprite(name);
                icon.preserveAspect = true;
                AddText(row.transform, "Text", Format(name) + $"  Lv.{level}  Progress: {progress}", 16, UITemporaryTheme.TextPrimary, 42, true);
                int target = Math.Max(progress + 1, _services.Formula.TotalStarsToNextLp(level));
                CreateProgressBar(_body, "Progress_" + name, progress, target, LegacyUITheme.BrassBorder);
            }
            AddText(_body, "Max", "Account maxed: " + (_services.Doctrine.IsMaxed() ? "Yes" : "No"), 15, UITemporaryTheme.TextSecondary, 30);
            AddButton(_body, "Back", "Back to Detail", () => Open(_character), true);
        }

        private void AddEquipmentSummary(CharacterRuntime c)
        {
            AddText(_body, "Equipment", $"Weapon: {ItemName(c.Weapon)}\nArmor: {ItemName(c.Armor)}\nAccessory: {ItemName(c.Accessory)}", 16, UITemporaryTheme.TextSecondary, 58);
        }

        private void AddStat(string label, int value) => UICardFactory.CreateStatRow(_body, label, value.ToString());

        private int SafeStat(CharacterRuntime c, StatType stat)
        {
            try { return _services.Character.GetTotalStat(c, stat); }
            catch (Exception ex) { Debug.LogWarning("[Phase6] Stat unavailable: " + ex.Message); return 0; }
        }

        private static string ItemName(ItemRuntime item) => item == null ? "Empty" : Format(item.Definition?.id ?? item.InstanceId);

        private static Sprite GetDoctrineLegacySprite(string doctrineId)
        {
            // The imported legacy files use the original "doctrine_of_*" names.
            // Knowledge retains the original decompile typo: "doctrine_of_knowlegde".
            string legacyName = doctrineId == "knowledge"
                ? "doctrine_of_knowlegde"
                : "doctrine_of_" + doctrineId;
            return LegacySpriteRegistry.GetSprite(legacyName);
        }

        private GameObject CreateOverlay(string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(transform, false);
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = new Color(0, 0, 0, .55f);
            var frame = new GameObject("DialogFrame", typeof(RectTransform), typeof(Image), typeof(Outline));
            var frameRt = frame.GetComponent<RectTransform>(); frameRt.SetParent(rt, false); frameRt.anchorMin = new Vector2(.04f, .035f); frameRt.anchorMax = new Vector2(.96f, .965f); frameRt.offsetMin = Vector2.zero; frameRt.offsetMax = Vector2.zero;
            frame.GetComponent<Image>().color = LegacyUITheme.CardviewDarkBackground;
            var outline = frame.GetComponent<Outline>(); outline.effectColor = LegacyUITheme.Black; outline.effectDistance = new Vector2(6, -6);
            return go;
        }

        private RectTransform CreateScrollBody(Transform parent)
        {
            var frame = parent.Find("DialogFrame");
            if (frame != null) parent = frame;
            var scrollGo = new GameObject("Scroll", typeof(RectTransform), typeof(ScrollRect), typeof(RectMask2D));
            var scroll = scrollGo.GetComponent<RectTransform>(); scroll.SetParent(parent, false);
            scroll.anchorMin = Vector2.zero; scroll.anchorMax = Vector2.one;
            float inset = LegacyUITheme.DP(12);
            scroll.offsetMin = new Vector2(inset, inset); scroll.offsetMax = new Vector2(-inset, -inset);
            var bodyGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            var body = bodyGo.GetComponent<RectTransform>(); body.SetParent(scroll, false);
            body.anchorMin = new Vector2(0, 1); body.anchorMax = new Vector2(1, 1); body.pivot = new Vector2(.5f, 1); body.sizeDelta = Vector2.zero;
            var group = bodyGo.GetComponent<VerticalLayoutGroup>(); group.spacing = LegacyUITheme.DP(8); int padding = LegacyUITheme.DP(8); group.padding = new RectOffset(padding, padding, padding, padding); group.childControlWidth = true; group.childControlHeight = true; group.childForceExpandWidth = true; group.childForceExpandHeight = false;
            bodyGo.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var sr = scrollGo.GetComponent<ScrollRect>(); sr.horizontal = false; sr.vertical = true; sr.viewport = scroll; sr.content = body;
            return body;
        }

        private static GameObject AddRow(Transform parent, string name, float height)
        {
            var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false);
            var le = go.AddComponent<LayoutElement>(); le.preferredHeight = height; le.flexibleWidth = 1;
            var layout = go.AddComponent<HorizontalLayoutGroup>(); layout.spacing = 10; layout.padding = new RectOffset(8, 8, 6, 6); layout.childControlWidth = true; layout.childControlHeight = true; layout.childForceExpandWidth = false; layout.childForceExpandHeight = true;
            go.AddComponent<Image>().color = LegacyUITheme.StandardBackground;
            var outline = go.AddComponent<Outline>(); outline.effectColor = LegacyUITheme.GreyBorder; outline.effectDistance = new Vector2(1, -1);
            return go;
        }

        private static Image CreateImage(Transform parent, string name, float size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent, false);
            var le = go.AddComponent<LayoutElement>(); le.preferredWidth = size; le.preferredHeight = size; le.flexibleWidth = 0; le.flexibleHeight = 0;
            var image = go.GetComponent<Image>(); image.color = UITemporaryTheme.PlaceholderIcon; image.raycastTarget = false; return image;
        }

        private static void AddEmptySlotLabel(Transform parent)
        {
            var go = new GameObject("EmptySlot", typeof(RectTransform)); go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>(); rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var text = go.AddComponent<Text>(); text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = 9; text.fontStyle = FontStyle.Bold; text.color = LegacyUITheme.DimWhite; text.text = "EMPTY"; text.alignment = TextAnchor.MiddleCenter; text.raycastTarget = false;
        }

        private static Text AddText(Transform parent, string name, string value, int size, Color color, float height, bool flexible = false)
            => AddText(parent, name, value, size, color, height, TextAnchor.MiddleLeft, flexible);

        private static Text AddText(Transform parent, string name, string value, int size, Color color, float height, TextAnchor alignment, bool flexible = false)
        {
            var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false);
            var le = go.AddComponent<LayoutElement>(); le.preferredHeight = height; le.flexibleWidth = 1; if (flexible) le.flexibleHeight = 1;
            var text = go.AddComponent<Text>(); text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = size; text.color = color; text.text = value ?? ""; text.alignment = alignment; text.horizontalOverflow = HorizontalWrapMode.Wrap; text.verticalOverflow = VerticalWrapMode.Truncate; text.raycastTarget = false; return text;
        }

        private static Button AddButton(RectTransform parent, string name, string label, Action click, bool interactable)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Outline), typeof(Button)); go.transform.SetParent(parent, false);
            var le = go.AddComponent<LayoutElement>(); le.preferredHeight = 52; le.flexibleWidth = 1;
            var image = go.GetComponent<Image>(); image.color = interactable ? LegacyUITheme.StandardBackground : LegacyUITheme.StandardBackgroundUnavailable;
            var outline = go.GetComponent<Outline>(); outline.effectColor = interactable ? LegacyUITheme.BrassBorder : LegacyUITheme.Failure; outline.effectDistance = new Vector2(2, -2);
            AddText(go.transform, "Label", label, 16, interactable ? LegacyUITheme.DimWhite : LegacyUITheme.Failure, 52, TextAnchor.MiddleCenter, true);
            var button = go.GetComponent<Button>(); button.targetGraphic = image; button.interactable = interactable; if (interactable && click != null) button.onClick.AddListener(() => click()); return button;
        }

        private static void CreateLegacyItemCard(RectTransform parent, ItemRuntime item, string action, Action click)
        {
            var go = new GameObject("Item_" + item.InstanceId, typeof(RectTransform), typeof(Image), typeof(Outline), typeof(Button)); go.transform.SetParent(parent, false);
            var le = go.AddComponent<LayoutElement>(); le.preferredHeight = 68; le.flexibleWidth = 1;
            var bg = go.GetComponent<Image>(); bg.color = LegacyUITheme.StandardBackground;
            var outline = go.GetComponent<Outline>(); outline.effectColor = LegacyUITheme.DimWhite; outline.effectDistance = new Vector2(1, -1);
            var group = go.AddComponent<HorizontalLayoutGroup>(); group.padding = new RectOffset(8, 8, 6, 6); group.spacing = 8; group.childControlWidth = true; group.childControlHeight = true; group.childForceExpandWidth = false; group.childForceExpandHeight = true;
            var icon = CreateImage(go.transform, "Icon", 52); icon.sprite = LegacySpriteRegistry.GetItemSprite(item.Definition?.id); icon.preserveAspect = true;
            AddText(go.transform, "Name", Format(item.Definition?.id) + $"\n{item.Definition?.GetStatSummary()}  x{item.StackCount}", 14, LegacyUITheme.DimWhite, 52, TextAnchor.MiddleLeft, true);
            AddText(go.transform, "Action", action, 13, LegacyUITheme.BrassBorder, 52, TextAnchor.MiddleCenter);
            var button = go.GetComponent<Button>(); button.targetGraphic = bg; button.interactable = true; button.onClick.AddListener(() => click());
        }

        private static void CreateProgressBar(RectTransform parent, string name, int current, int max, Color fillColor)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent, false); var le = go.AddComponent<LayoutElement>(); le.preferredHeight = 10; le.flexibleWidth = 1; go.GetComponent<Image>().color = new Color(0, 0, 0, .75f);
            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image)); var rt = fill.GetComponent<RectTransform>(); rt.SetParent(go.transform, false); rt.anchorMin = Vector2.zero; rt.anchorMax = new Vector2(Mathf.Clamp01((float)current / Math.Max(1, max)), 1); rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero; fill.GetComponent<Image>().color = fillColor;
        }

        private void CloseOverlayOnly()
        {
            if (_overlay != null) Destroy(_overlay);
            _overlay = null; _body = null;
        }

        private static string Format(string value) => string.IsNullOrEmpty(value) ? "Unknown" : value.Replace('_', ' ').Replace('-', ' ');
    }
}
