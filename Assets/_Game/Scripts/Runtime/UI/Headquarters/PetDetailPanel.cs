using System.Collections.Generic;
using System.Linq;
using System;
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
    /// Phase 5E Pet Detail overlay — a child panel of <see cref="ShelterDialog"/> (never a second
    /// AppShellController popup). Shows only real, populated fields: PetSaveData (InstanceId,
    /// DefinitionId, Level, Exp, EquippedToCharacterId) are all real. PetDefinition fields
    /// (PetName, BaseAttack/Defense/MaxHp/Speed, multipliers, SkillDefinitionId,
    /// EvolutionDefinitionId/Level) are declared on the model but pets.json only ever supplies
    /// `id` — every other PetDefinition field is its C# default for all 21 pets in the current
    /// data (verified). Per "không hiển thị N/A giả", those are shown only when non-default, which
    /// in the current data means they never render — see report "Known limitations". No actions:
    /// no Favourite/Autofeed/Feed/Breeding API exists anywhere in IPetService.
    /// </summary>
    public sealed class PetDetailPanel : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] private Image _icon;
        [SerializeField] private Text _nameText;
        [SerializeField] private Text _infoText;
        [SerializeField] private Button _closeButton;

        private ServiceContainer _services;
        private System.Action _onClose;
        private System.Action _onChanged;
        private const string ActionPrefix = "PetAction_";
        private GameObject _releaseConfirmation;

        public void Setup(ServiceContainer services, System.Action onClose, System.Action onChanged = null)
        {
            _services = services;
            _onClose = onClose;
            _onChanged = onChanged;

            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveAllListeners();
                _closeButton.onClick.AddListener(() => _onClose?.Invoke());
            }
        }

        public void Show(PetSaveData pet)
        {
            if (pet == null) return;
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            Refresh(pet);
        }

        public void Hide()
        {
            if (_releaseConfirmation != null)
            {
                Destroy(_releaseConfirmation);
                _releaseConfirmation = null;
            }
            gameObject.SetActive(false);
        }

        private void Refresh(PetSaveData pet)
        {
            if (_icon != null)
            {
                string imageId = pet.DefinitionId;
                if (_services.Database.TryGet<PetDefinition>(pet.DefinitionId, out var petDefinition) &&
                    !string.IsNullOrEmpty(petDefinition.IdImage))
                    imageId = petDefinition.IdImage;
                Sprite sprite = LegacySpriteRegistry.GetPetSprite(imageId);
                _icon.sprite = sprite;
                _icon.color = sprite != null ? Color.white : new Color(1f, 1f, 1f, 0.25f);
            }

            if (_nameText != null) _nameText.text = PetFormat.FormatId(pet.DefinitionId);

            bool hasDef = _services.Database.TryGet<PetDefinition>(pet.DefinitionId, out var def);

            var lines = new List<string>
            {
                $"Level: {pet.Level}",
                $"Food: {pet.Food}/{_services.Pet.GetFoodToNextLevel(pet.InstanceId)}",
                $"Favourite: {(pet.Favourite ? "Yes" : "No")}",
                $"Assigned dungeon: {(string.IsNullOrEmpty(pet.AssignedDungeonId) ? "None" : PetFormat.FormatId(pet.AssignedDungeonId))}"
            };

            if (hasDef)
            {
                lines.Add($"Family: {PetFormat.FormatId(def.PetFamily)}  Tier: {def.PetTier}");
                lines.Add($"Abilities: {FormatAbilities(pet)}");
            }

            if (_services?.Pet != null)
            {
                double experienceBonus = _services.Pet.GetExperienceBonus(pet.InstanceId) * 100d;
                double dropBonus = _services.Pet.GetDropBonus(pet.InstanceId) * 100d;
                if (experienceBonus <= 0d && dropBonus <= 0d)
                {
                    lines.Add("No expedition bonus.");
                }
                else
                {
                    if (experienceBonus > 0d)
                    {
                        lines.Add("EXP BONUS");
                        lines.Add($"+{experienceBonus:0.##}% Dungeon EXP");
                    }
                    if (dropBonus > 0d)
                    {
                        lines.Add("DROP BONUS");
                        lines.Add($"+{dropBonus:0.##}% Additional Loot Chance");
                    }
                }
            }

            if (_infoText != null) _infoText.text = string.Join("\n", lines);

            BuildActions(pet);
        }

        private string FormatAbilities(PetSaveData pet)
        {
            return new[] { pet.Ability1, pet.Ability2, pet.Ability3, pet.Ability4 }
                .Where(a => !string.IsNullOrEmpty(a) && !string.Equals(a, "EMPTY", StringComparison.OrdinalIgnoreCase))
                .Select(PetFormat.FormatId)
                .DefaultIfEmpty("None")
                .Aggregate((a, b) => a + ", " + b);
        }

        private void BuildActions(PetSaveData pet)
        {
            Transform actionParent = GetComponentInChildren<VerticalLayoutGroup>(true)?.transform;
            if (actionParent == null) return;

            for (int i = actionParent.childCount - 1; i >= 0; i--)
            {
                if (actionParent.GetChild(i).name.StartsWith(ActionPrefix, StringComparison.Ordinal))
                    Destroy(actionParent.GetChild(i).gameObject);
            }

            AddAction(actionParent, "Favourite", pet.Favourite ? "UNFAVOURITE" : "FAVOURITE", () =>
            {
                _services.Pet.SetFavourite(pet.InstanceId, !pet.Favourite);
                Refresh(pet);
                _onChanged?.Invoke();
            });

            if (!string.IsNullOrEmpty(pet.AssignedDungeonId))
            {
                AddAction(actionParent, "Unassign", "UNASSIGN FROM DUNGEON", () =>
                {
                    if (_services.Pet.UnassignFromDungeon(pet.InstanceId))
                    {
                        Refresh(pet);
                        _onChanged?.Invoke();
                    }
                });
            }
            else
            {
                foreach (var dungeon in _services.Database.GetAll<DungeonDefinition>()
                    .Where(d => d != null && _services.Dungeon.IsDungeonUnlocked(d.id)))
                {
                    var selectedDungeon = dungeon;
                    AddAction(actionParent, "Assign_" + selectedDungeon.id, "ASSIGN: " + PetFormat.FormatId(selectedDungeon.id), () =>
                    {
                        if (_services.Pet.AssignToDungeon(pet.InstanceId, selectedDungeon.id))
                        {
                            Refresh(pet);
                            _onChanged?.Invoke();
                        }
                    });
                }
            }

            var food = (_services.Inventory?.GetAllItems() ?? new List<ItemRuntime>())
                .FirstOrDefault(item => item?.Definition != null &&
                    string.Equals(item.Definition.parentClass, "Food", StringComparison.OrdinalIgnoreCase) &&
                    item.Definition.FeedPower > 0 && item.StackCount > 0);
            if (food != null)
            {
                var selectedFood = food;
                AddAction(actionParent, "Feed_" + selectedFood.InstanceId,
                    $"FEED {PetFormat.FormatId(selectedFood.Definition.id)} x1 (+{selectedFood.Definition.FeedPower} food)", () =>
                    {
                        if (_services.Pet.FeedWithItem(pet.InstanceId, selectedFood.InstanceId, 1))
                        {
                            Refresh(pet);
                            _onChanged?.Invoke();
                        }
                    });
            }
            else
            {
                AddAction(actionParent, "FeedUnavailable", "FEED: NO FOOD AVAILABLE", null, false);
            }

            AddAction(actionParent, "Release", "RELEASE PET", () => ShowReleaseConfirmation(pet));
        }

        private void ShowReleaseConfirmation(PetSaveData pet)
        {
            if (pet == null || _releaseConfirmation != null) return;

            _releaseConfirmation = new GameObject("PetReleaseConfirmation", typeof(RectTransform), typeof(Image));
            _releaseConfirmation.transform.SetParent(transform, false);
            var overlayRect = (RectTransform)_releaseConfirmation.transform;
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;
            _releaseConfirmation.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.78f);

            var box = new GameObject("Dialog", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
            box.transform.SetParent(_releaseConfirmation.transform, false);
            var boxRect = (RectTransform)box.transform;
            boxRect.anchorMin = new Vector2(0.12f, 0.35f);
            boxRect.anchorMax = new Vector2(0.88f, 0.65f);
            boxRect.offsetMin = Vector2.zero;
            boxRect.offsetMax = Vector2.zero;
            box.GetComponent<Image>().color = LegacyUITheme.CardviewDarkBackground;
            var layout = box.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(24, 24, 20, 20);
            layout.spacing = 12f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;

            AddConfirmationText(box.transform, "RELEASE THIS PET?", 22, LegacyUITheme.BrassBorder, 48);
            AddConfirmationText(box.transform,
                $"Release {PetFormat.FormatId(pet.DefinitionId)} permanently?", 16, LegacyUITheme.DimWhite, 58);
            AddConfirmationButton(box.transform, "RELEASE", LegacyUITheme.Failure, () =>
            {
                if (_services.Pet.ReleasePet(pet.InstanceId))
                {
                    _onChanged?.Invoke();
                    _onClose?.Invoke();
                }
                else
                {
                    AddConfirmationText(box.transform, "Cannot release an active expedition companion.", 14,
                        LegacyUITheme.Failure, 42);
                }
            });
            AddConfirmationButton(box.transform, "CANCEL", LegacyUITheme.BrassBorder, () =>
            {
                Destroy(_releaseConfirmation);
                _releaseConfirmation = null;
            });
        }

        private static void AddConfirmationText(Transform parent, string value, int fontSize, Color color, float height)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(Text), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            go.GetComponent<LayoutElement>().preferredHeight = height;
            var text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.text = value;
            text.raycastTarget = false;
        }

        private static void AddConfirmationButton(Transform parent, string label, Color color, Action onClick)
        {
            var go = new GameObject("Button_" + label, typeof(RectTransform), typeof(Image), typeof(LayoutElement), typeof(Button));
            go.transform.SetParent(parent, false);
            var layout = go.GetComponent<LayoutElement>();
            layout.preferredHeight = 46f;
            layout.flexibleWidth = 1f;
            var image = go.GetComponent<Image>();
            image.color = color;
            var button = go.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => onClick?.Invoke());
            AddConfirmationText(go.transform, label, 16, LegacyUITheme.Black, 46);
        }

        private static void AddAction(Transform parent, string key, string label, Action onClick, bool interactable = true)
        {
            var go = new GameObject(ActionPrefix + key, typeof(RectTransform), typeof(Image), typeof(LayoutElement), typeof(Button));
            go.transform.SetParent(parent, false);
            var layout = go.GetComponent<LayoutElement>();
            layout.preferredHeight = 46f;
            layout.flexibleWidth = 1f;
            var image = go.GetComponent<Image>();
            image.color = LegacyUITheme.BrassBorder;
            var button = go.GetComponent<Button>();
            button.targetGraphic = image;
            button.interactable = interactable;
            if (!interactable) image.color = LegacyUITheme.GreyBorder;
            button.onClick.AddListener(() => onClick?.Invoke());

            var textGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(go.transform, false);
            var rect = textGo.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(12f, 0f);
            rect.offsetMax = new Vector2(-12f, 0f);
            var text = textGo.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 16;
            text.color = LegacyUITheme.Black;
            text.alignment = TextAnchor.MiddleCenter;
            text.text = label;
            text.raycastTarget = false;
        }
    }
}
