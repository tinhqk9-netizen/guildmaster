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

        [Header("Recipe list")]
        [SerializeField] private RectTransform _listContent;
        [SerializeField] private GameObject _emptyState;
        [SerializeField] private Sprite _rowBorderSprite;

        private ServiceContainer _services;
        private System.Action _onClose;
        private System.Action _onCraftChanged;

        public void Setup(ServiceContainer services, System.Action onClose, System.Action onCraftChanged)
        {
            _services = services;
            _onClose = onClose;
            _onCraftChanged = onCraftChanged;

            if (_titleText != null) _titleText.text = "Recipes";
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
            if (_services?.Database == null || _listContent == null) return;

            var recipes = _services.Database.GetAll<RecipeDefinition>().ToList();

            for (int i = _listContent.childCount - 1; i >= 0; i--)
                Destroy(_listContent.GetChild(i).gameObject);

            if (_emptyState != null) _emptyState.SetActive(recipes.Count == 0);

            foreach (var recipe in recipes)
            {
                CreateRecipeRow(recipe);
            }
        }

        private void CreateRecipeRow(RecipeDefinition recipe)
        {
            var row = WorkshopRowBuilder.CreateRow(_listContent, _rowBorderSprite, recipe.OutputItemId, 1,
                BuildIngredientSummary(recipe), showProgress: false, progress: 0f,
                actionLabel: "Craft", onAction: () => OnCraftClicked(recipe.id));

            // The shared row builder always enables its action button — override interactability
            // here using the real CanCraft() gate instead of duplicating its validation logic.
            var button = row.GetComponentInChildren<Button>();
            if (button != null)
            {
                var result = _services.Craft.CanCraft(recipe.id);
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
