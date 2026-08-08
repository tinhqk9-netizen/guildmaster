using System.Linq;
using System.Text;
using GuildMaster.Definitions;
using GuildMaster.Runtime.Models;
using GuildMaster.Runtime.Services;
using GuildMaster.Runtime.UI.Legacy;
using UnityEngine;
using UnityEngine.UI;

namespace GuildMaster.Runtime.UI.Headquarters
{
    /// <summary>
    /// Phase 5D recipe selection overlay — a child panel of <see cref="WorkshopDialog"/> (never a
    /// second AppShellController popup). Reads every real recipe via GameDatabase.GetAll&lt;RecipeDefinition&gt;()
    /// (same call CraftScreen (S6) already uses) and every ingredient count via
    /// IInventoryService.GetQuantityByDefinitionId. Craft button state is gated directly by
    /// ICraftService.CanCraft — the real validation logic is never re-implemented here.
    /// No quantity/batch picker: ICraftService.TryStartCraft always crafts a single output
    /// (CraftService hard-codes outputStack = 1), so a batch selector would have nothing real to
    /// drive. No filter/sort: not required by this phase's scope, kept minimal like Storage.
    /// </summary>
    public sealed class WorkshopRecipePanel : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] private Text _titleText;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Toggle _craftableOnlyToggle;

        [Header("Recipe list")]
        [SerializeField] private RectTransform _listContent;
        [SerializeField] private GameObject _emptyState;
        [SerializeField] private Sprite _rowBorderSprite;

        private ServiceContainer _services;
        private System.Action _onClose;
        private System.Action _onCraftChanged;
        private Text _craftableOnlyLabel;

        public void Setup(ServiceContainer services, System.Action onClose, System.Action onCraftChanged)
        {
            _services = services;
            _onClose = onClose;
            _onCraftChanged = onCraftChanged;

            if (_titleText != null) _titleText.text = "Recipes";
            EnsureCraftableOnlyToggle();
            if (_craftableOnlyToggle != null)
            {
                _craftableOnlyToggle.onValueChanged.RemoveAllListeners();
                _craftableOnlyToggle.onValueChanged.AddListener(OnCraftableOnlyChanged);
                UpdateCraftableOnlyLabel(_craftableOnlyToggle.isOn);
            }
            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveAllListeners();
                _closeButton.onClick.AddListener(() => _onClose?.Invoke());
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            Refresh();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void Refresh()
        {
            if (_services?.Database == null || _services.Craft == null || _listContent == null) return;

            var recipes = _services.Database.GetAll<RecipeDefinition>()
                .Where(recipe => recipe != null)
                .ToList();
            var results = recipes.ToDictionary(recipe => recipe.id, recipe => _services.Craft.CanCraft(recipe.id));
            var available = recipes.Where(recipe => results[recipe.id].Success).ToList();
            var unavailable = recipes.Where(recipe => !results[recipe.id].Success).ToList();

            for (int i = _listContent.childCount - 1; i >= 0; i--)
                Destroy(_listContent.GetChild(i).gameObject);

            if (_emptyState != null) _emptyState.SetActive(recipes.Count == 0);

            bool craftableOnly = _craftableOnlyToggle != null && _craftableOnlyToggle.isOn;
            if (craftableOnly)
            {
                if (available.Count > 0) AddSectionHeader("CRAFTABLE ONLY");
                foreach (var recipe in available)
                    CreateRecipeRow(recipe, results[recipe.id]);
            }
            else
            {
                if (available.Count > 0)
                {
                    AddSectionHeader("AVAILABLE RECIPES");
                    foreach (var recipe in available)
                        CreateRecipeRow(recipe, results[recipe.id]);
                }

                if (unavailable.Count > 0)
                {
                    AddSectionHeader("UNAVAILABLE RECIPES");
                    foreach (var recipe in unavailable)
                        CreateRecipeRow(recipe, results[recipe.id]);
                }
            }

            if (craftableOnly && available.Count == 0)
                AddSectionHeader("No craftable recipes");
        }

        private void OnCraftableOnlyChanged(bool enabled)
        {
            UpdateCraftableOnlyLabel(enabled);
            Refresh();
        }

        private void EnsureCraftableOnlyToggle()
        {
            if (_craftableOnlyToggle != null)
            {
                _craftableOnlyLabel = _craftableOnlyToggle.transform.Find("Label")?.GetComponent<Text>();
                return;
            }

            Transform host = _titleText != null && _titleText.transform.parent != null
                ? _titleText.transform.parent
                : transform;
            Transform existing = host.Find("CraftableOnlyToggle");
            if (existing != null)
            {
                _craftableOnlyToggle = existing.GetComponent<Toggle>();
                _craftableOnlyLabel = existing.Find("Label")?.GetComponent<Text>();
                return;
            }

            var toggleObject = new GameObject("CraftableOnlyToggle", typeof(RectTransform), typeof(Image), typeof(Toggle), typeof(LayoutElement));
            toggleObject.transform.SetParent(host, false);
            var toggleImage = toggleObject.GetComponent<Image>();
            toggleImage.color = LegacyUITheme.CardviewDarkBackground;
            toggleImage.raycastTarget = true;

            _craftableOnlyToggle = toggleObject.GetComponent<Toggle>();
            _craftableOnlyToggle.targetGraphic = toggleImage;
            var layout = toggleObject.GetComponent<LayoutElement>();
            layout.preferredHeight = 48f;
            layout.flexibleWidth = 1f;

            var labelObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(toggleObject.transform, false);
            var labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(18f, 0f);
            labelRect.offsetMax = new Vector2(-18f, 0f);
            _craftableOnlyLabel = labelObject.GetComponent<Text>();
            _craftableOnlyLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _craftableOnlyLabel.fontSize = 24;
            _craftableOnlyLabel.color = LegacyUITheme.DimWhite;
            _craftableOnlyLabel.alignment = TextAnchor.MiddleLeft;
            _craftableOnlyLabel.raycastTarget = false;
            UpdateCraftableOnlyLabel(false);
        }

        private void UpdateCraftableOnlyLabel(bool enabled)
        {
            if (_craftableOnlyLabel != null)
                _craftableOnlyLabel.text = enabled ? "[✓] Show Craftable Only" : "[ ] Show Craftable Only";
        }

        private void CreateRecipeRow(RecipeDefinition recipe, CraftResult result)
        {
            var row = WorkshopRowBuilder.CreateRow(_listContent, _rowBorderSprite, recipe.OutputItemId, 1,
                BuildRecipeStatus(recipe, result), showProgress: false, progress: 0f,
                actionLabel: "Craft", onAction: () => OnCraftClicked(recipe.id));

            // The shared row builder always enables its action button — override interactability
            // here using the real CanCraft() gate instead of duplicating its validation logic.
            var button = row.GetComponentInChildren<Button>();
            if (button != null)
            {
                button.interactable = result.Success;
                var label = button.GetComponentInChildren<Text>();
                if (label != null) label.text = result.Success ? "Craft" : ShortFailureLabel(result.FailureReason);
            }
        }

        private void OnCraftClicked(string recipeId)
        {
            if (_services?.Craft == null) return;
            var result = _services.Craft.TryStartCraft(recipeId);
            if (result.Success)
            {
                Refresh();
                _onCraftChanged?.Invoke();
            }
        }

        private string BuildIngredientSummary(RecipeDefinition recipe)
        {
            if (recipe.Ingredients == null || recipe.Ingredients.Count == 0) return "No ingredients defined.";

            var sb = new StringBuilder();
            for (int i = 0; i < recipe.Ingredients.Count; i++)
            {
                var ing = recipe.Ingredients[i];
                int owned = _services.Inventory.GetQuantityByDefinitionId(ing.ItemId);
                bool enough = owned >= ing.Amount;
                if (i > 0) sb.Append("  ");
                sb.Append(WorkshopRowBuilder.FormatId(ing.ItemId));
                sb.Append(": ");
                sb.Append(enough ? $"{owned}/{ing.Amount}" : $"<color=#c83232>{owned}/{ing.Amount}</color>");
            }
            return sb.ToString();
        }

        private string BuildRecipeStatus(RecipeDefinition recipe, CraftResult result)
        {
            if (result.Success) return $"✓ Craftable\n{BuildIngredientSummary(recipe)}";
            if (result.FailureReason == CraftFailureReason.MissingIngredients)
                return $"Missing materials\n{BuildIngredientSummary(recipe)}";
            return $"Unavailable: {ShortFailureLabel(result.FailureReason)}\n{BuildIngredientSummary(recipe)}";
        }

        private void AddSectionHeader(string label)
        {
            var go = new GameObject($"Section_{label.Replace(' ', '_')}", typeof(RectTransform), typeof(Text), typeof(LayoutElement));
            go.transform.SetParent(_listContent, false);
            var text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 28;
            text.fontStyle = FontStyle.Bold;
            text.color = LegacyUITheme.AscendedUnit;
            text.alignment = TextAnchor.MiddleLeft;
            text.text = label;
            go.GetComponent<LayoutElement>().preferredHeight = 46f;
        }

        private static string ShortFailureLabel(CraftFailureReason reason)
        {
            switch (reason)
            {
                case Models.CraftFailureReason.QueueFull: return "Queue Full";
                case Models.CraftFailureReason.MissingIngredients: return "Missing";
                default: return "Unavailable";
            }
        }
    }
}
