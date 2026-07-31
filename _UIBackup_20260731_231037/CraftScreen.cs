using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using GuildMaster.Runtime.UI.Core;
using GuildMaster.Runtime.Services;
using GuildMaster.Runtime.Models;
using GuildMaster.Definitions;
using GuildMaster.Database;
using GuildMaster.Runtime.Save;

namespace GuildMaster.Runtime.UI.Craft
{
    /// <summary>
    /// Craft / Workshop screen — 3 tabs: Recipes | Queue | Completed.
    /// Includes search filter bar for recipes, real-time inventory quantity checks,
    /// and Workshop queue capacity upgrades.
    /// </summary>
    public class CraftScreen : UIScreen
    {
        private ICraftService     _craftService;
        private IInventoryService _inventoryService;
        private GameDatabase      _database;
        private ISaveService      _saveService;

        // ── Tab state ────────────────────────────────────────────────────────────
        private enum CraftTab { Recipes, Queue, Completed }
        private CraftTab _activeTab = CraftTab.Recipes;

        // ── Serialized references ────────────────────────────────────────────────
        [SerializeField] private RectTransform _cardContainer;
        [SerializeField] private Text          _summaryText;
        [SerializeField] private Text          _detailText;
        [SerializeField] private Button        _tabRecipesBtn;
        [SerializeField] private Button        _tabQueueBtn;
        [SerializeField] private Button        _tabCompletedBtn;
        [SerializeField] private Button        _craftButton;
        [SerializeField] private Button        _claimButton;
        [SerializeField] private Button        _upgradeQueueButton;
        [SerializeField] private Text          _feedbackText;

        // Search references
        [SerializeField] private InputField _searchInputField;
        [SerializeField] private GameObject _searchBarContainer;
        private string _searchQuery = "";

        private int _selectedRecipeIndex;
        private int _selectedCompletedIndex;
        private int _selectedQueueIndex;

        public void Initialize(ServiceContainer services)
        {
            _craftService     = services.Craft;
            _inventoryService = services.Inventory;
            _database         = services.Database;
            _saveService      = services.Save;
        }

        public override void Show()
        {
            base.Show();
            _activeTab = CraftTab.Recipes;
            
            if (_searchInputField != null)
            {
                _searchInputField.onValueChanged.RemoveAllListeners();
                _searchInputField.onValueChanged.AddListener(OnSearchQueryChanged);
            }

            Refresh();
        }

        public void OnSearchQueryChanged(string query)
        {
            _searchQuery = query != null ? query.Trim() : "";
            _selectedRecipeIndex = 0;
            Refresh();
        }

        private List<RecipeDefinition> GetRecipes() =>
            _database != null
                ? new List<RecipeDefinition>(_database.GetAll<RecipeDefinition>())
                : new List<RecipeDefinition>();

        private long GetPlayerMoney() => _saveService?.CurrentData?.Money ?? 0;

        public void Refresh()
        {
            if (_craftService == null) return;

            var queue     = _craftService.GetQueue();
            var completed = _craftService.GetCompletedItems();
            int queueCap  = _craftService.GetQueueCapacity();

            if (_summaryText != null)
                _summaryText.text = $"Queue: {queue.Count}/{queueCap}  |  Completed: {completed.Count}";

            UpdateTabButtons();

            // Toggle Search Bar visibility — only show search bar on Recipes tab
            if (_searchBarContainer != null)
            {
                _searchBarContainer.SetActive(_activeTab == CraftTab.Recipes);
            }
            else if (_searchInputField != null)
            {
                _searchInputField.gameObject.SetActive(_activeTab == CraftTab.Recipes);
            }

            switch (_activeTab)
            {
                case CraftTab.Recipes:   ShowRecipesTab(); break;
                case CraftTab.Queue:     ShowQueueTab();   break;
                case CraftTab.Completed: ShowCompletedTab(); break;
            }

            RefreshUpgradeQueueButton();
        }

        // ── Tab switching ────────────────────────────────────────────────────────

        public void OnClickTabRecipes()   { _activeTab = CraftTab.Recipes;   Refresh(); }
        public void OnClickTabQueue()     { _activeTab = CraftTab.Queue;     Refresh(); }
        public void OnClickTabCompleted() { _activeTab = CraftTab.Completed; Refresh(); }

        private void UpdateTabButtons()
        {
            SetTabActive(_tabRecipesBtn,   _activeTab == CraftTab.Recipes);
            SetTabActive(_tabQueueBtn,     _activeTab == CraftTab.Queue);
            SetTabActive(_tabCompletedBtn, _activeTab == CraftTab.Completed);
        }

        private static void SetTabActive(Button btn, bool active)
        {
            if (btn == null) return;
            var img = btn.GetComponent<Image>();
            if (img != null)
                img.color = active ? UITemporaryTheme.TabActive : UITemporaryTheme.TabInactive;
        }

        private void RefreshUpgradeQueueButton()
        {
            if (_upgradeQueueButton == null) return;

            if (_activeTab == CraftTab.Queue)
            {
                _upgradeQueueButton.gameObject.SetActive(true);
                long money = GetPlayerMoney();
                long price = _craftService.GetUpgradeQueueCapacityPrice();
                int level  = _craftService.GetQueueCapacityLevel();
                _upgradeQueueButton.interactable = (money >= price);
                var btnTxt = _upgradeQueueButton.GetComponentInChildren<Text>();
                if (btnTxt != null) btnTxt.text = $"Upgrade Queue\nLv.{level} (Cost: {price}g)";
            }
            else
            {
                _upgradeQueueButton.gameObject.SetActive(false);
            }
        }

        // ── Item / Recipe Name formatting ────────────────────────────────────────

        private string FormatItemName(string id)
        {
            if (string.IsNullOrEmpty(id)) return "Item";
            string clean = id.Replace("recipe_", "").Replace("_", " ");
            clean = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(clean);

            if (id.Contains("potion") || id.Contains("elixir")) return $"🧪 {clean}";
            if (id.Contains("sword") || id.Contains("blade") || id.Contains("cutlass")) return $"⚔️ {clean}";
            if (id.Contains("armor") || id.Contains("jacket")) return $"🛡️ {clean}";
            if (id.Contains("boot")) return $"🥾 {clean}";
            if (id.Contains("glove")) return $"🧤 {clean}";
            if (id.Contains("amulet") || id.Contains("ring")) return $"📿 {clean}";
            if (id.Contains("ingot") || id.Contains("goo") || id.Contains("compendium")) return $"📘 {clean}";

            return $"🛠️ {clean}";
        }

        // ── Recipes tab ──────────────────────────────────────────────────────────

        private void ShowRecipesTab()
        {
            if (_craftButton != null) _craftButton.gameObject.SetActive(true);
            if (_claimButton != null) _claimButton.gameObject.SetActive(false);

            var allRecipes = GetRecipes();

            List<RecipeDefinition> filteredRecipes = allRecipes;
            if (!string.IsNullOrEmpty(_searchQuery))
            {
                filteredRecipes = allRecipes.Where(r =>
                    (r.id != null && r.id.IndexOf(_searchQuery, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (r.OutputItemId != null && r.OutputItemId.IndexOf(_searchQuery, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    FormatItemName(r.OutputItemId).IndexOf(_searchQuery, StringComparison.OrdinalIgnoreCase) >= 0
                ).ToList();
            }

            if (_cardContainer != null)
            {
                UICardFactory.ClearContainer(_cardContainer);
                if (filteredRecipes.Count == 0)
                {
                    string emptyMsg = string.IsNullOrEmpty(_searchQuery)
                        ? "No recipes defined"
                        : $"🔍 No recipes found for \"{_searchQuery}\"";
                    UICardFactory.CreateDivider(_cardContainer, emptyMsg);
                }
                else
                {
                    for (int i = 0; i < filteredRecipes.Count; i++)
                    {
                        int captured = i;
                        var r        = filteredRecipes[i];
                        bool sel     = (i == _selectedRecipeIndex);

                        string title    = FormatItemName(r.OutputItemId);
                        string subtitle = $"{r.OutputItemId} x1";

                        UICardFactory.CreateCard(_cardContainer, title, subtitle,
                            sel, true, () => SelectRecipeIndex(captured), preferredHeight: 70);
                    }
                }
            }

            if (filteredRecipes.Count == 0)
                _selectedRecipeIndex = -1;
            else if (_selectedRecipeIndex >= filteredRecipes.Count)
                _selectedRecipeIndex = filteredRecipes.Count - 1;

            RefreshRecipeDetail(filteredRecipes);
        }

        public void SelectRecipeIndex(int index)
        {
            var recipes = GetFilteredRecipes();
            if (recipes.Count == 0) { _selectedRecipeIndex = -1; Refresh(); return; }
            _selectedRecipeIndex = Mathf.Clamp(index, 0, recipes.Count - 1);
            Refresh();
        }

        private List<RecipeDefinition> GetFilteredRecipes()
        {
            var all = GetRecipes();
            if (string.IsNullOrEmpty(_searchQuery)) return all;
            return all.Where(r =>
                (r.id != null && r.id.IndexOf(_searchQuery, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (r.OutputItemId != null && r.OutputItemId.IndexOf(_searchQuery, StringComparison.OrdinalIgnoreCase) >= 0) ||
                FormatItemName(r.OutputItemId).IndexOf(_searchQuery, StringComparison.OrdinalIgnoreCase) >= 0
            ).ToList();
        }

        private void RefreshRecipeDetail(IReadOnlyList<RecipeDefinition> recipes)
        {
            if (_detailText == null) return;

            if (recipes.Count == 0 || _selectedRecipeIndex < 0)
            {
                _detailText.text = "Select a recipe above to view details & ingredients.";
                if (_craftButton != null) _craftButton.interactable = false;
                return;
            }

            var r = recipes[_selectedRecipeIndex];
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"<b>Recipe:</b> {FormatItemName(r.OutputItemId)}");
            sb.AppendLine($"<b>Output:</b> {r.OutputItemId} x1");
            sb.AppendLine("\n<b>Ingredients Required:</b>");

            bool canCraft = true;
            if (r.Ingredients != null)
            {
                foreach (var ing in r.Ingredients)
                {
                    int owned = GetItemOwnedQuantity(ing.ItemId);
                    bool enough = owned >= ing.Amount;
                    if (!enough) canCraft = false;

                    string statusIcon = enough ? "<color=#5CD973>✓</color>" : "<color=#FF6B6B>✗</color>";
                    string qtyText    = enough
                        ? $"<color=#5CD973>{owned}/{ing.Amount}</color>"
                        : $"<color=#FF6B6B>{owned}/{ing.Amount} (Missing {ing.Amount - owned})</color>";

                    sb.AppendLine($"  {statusIcon} {FormatItemName(ing.ItemId)}: {qtyText}");
                }
            }

            _detailText.text = sb.ToString();

            if (_craftButton != null)
            {
                var queue = _craftService?.GetQueue();
                int limit = _craftService?.GetQueueCapacity() ?? 0;
                bool queueHasSpace = (queue != null && queue.Count < limit);
                _craftButton.interactable = canCraft && queueHasSpace;

                var txt = _craftButton.GetComponentInChildren<Text>();
                if (txt != null)
                {
                    txt.text = !canCraft ? "Missing Items ⚠️" : (!queueHasSpace ? "Queue Full ⚠️" : "Craft 🛠️");
                }
            }
        }

        private int GetItemOwnedQuantity(string itemId)
        {
            if (_inventoryService != null)
            {
                return _inventoryService.GetQuantityByDefinitionId(itemId);
            }
            return 0;
        }

        // ── Queue tab ────────────────────────────────────────────────────────────

        private void ShowQueueTab()
        {
            if (_craftButton != null) _craftButton.gameObject.SetActive(false);
            if (_claimButton != null) _claimButton.gameObject.SetActive(false);

            var queue = _craftService?.GetQueue() ?? new List<ItemActionSaveData>().AsReadOnly();

            if (_cardContainer != null)
            {
                UICardFactory.ClearContainer(_cardContainer);
                if (queue.Count == 0)
                {
                    UICardFactory.CreateDivider(_cardContainer, "Queue is empty. Select a recipe from Tab 1 to craft.");
                }
                else
                {
                    for (int i = 0; i < queue.Count; i++)
                    {
                        int captured = i;
                        var item     = queue[i];
                        bool sel     = (i == _selectedQueueIndex);

                        long remaining = Math.Max(0, 10 - item.SecondsPassed);
                        string status  = (i == 0) ? $"Crafting... ({remaining}s)" : "Waiting...";

                        UICardFactory.CreateCard(_cardContainer,
                            FormatItemName(item.DefinitionId),
                            status,
                            sel, true, () => SelectQueueIndex(captured), preferredHeight: 65);
                    }
                }
            }

            if (queue.Count == 0)
                _selectedQueueIndex = -1;
            else if (_selectedQueueIndex >= queue.Count)
                _selectedQueueIndex = queue.Count - 1;

            RefreshQueueDetail(queue);
        }

        public void SelectQueueIndex(int index)
        {
            var queue = _craftService?.GetQueue() ?? new List<ItemActionSaveData>().AsReadOnly();
            if (queue.Count == 0) { _selectedQueueIndex = -1; Refresh(); return; }
            _selectedQueueIndex = Mathf.Clamp(index, 0, queue.Count - 1);
            Refresh();
        }

        private void RefreshQueueDetail(IReadOnlyList<ItemActionSaveData> queue)
        {
            if (_detailText == null) return;

            if (queue.Count == 0 || _selectedQueueIndex < 0)
            {
                _detailText.text = "Crafting queue is currently empty.";
                return;
            }

            var item = queue[_selectedQueueIndex];
            long remaining = Math.Max(0, 10 - item.SecondsPassed);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"<b>Crafting:</b> {FormatItemName(item.DefinitionId)}");
            sb.AppendLine($"<b>Quantity:</b> x{item.StackCount}");
            sb.AppendLine($"<b>Queue Position:</b> #{_selectedQueueIndex + 1}");
            sb.AppendLine($"<b>Time Remaining:</b> {remaining} seconds");
            _detailText.text = sb.ToString();
        }

        // ── Completed tab ────────────────────────────────────────────────────────

        private void ShowCompletedTab()
        {
            if (_craftButton != null) _craftButton.gameObject.SetActive(false);
            if (_claimButton != null) _claimButton.gameObject.SetActive(true);

            var completed = _craftService?.GetCompletedItems() ?? new List<ItemActionSaveData>().AsReadOnly();

            if (_cardContainer != null)
            {
                UICardFactory.ClearContainer(_cardContainer);
                if (completed.Count == 0)
                {
                    UICardFactory.CreateDivider(_cardContainer, "No completed items yet.");
                }
                else
                {
                    for (int i = 0; i < completed.Count; i++)
                    {
                        int captured = i;
                        var item     = completed[i];
                        bool sel     = (i == _selectedCompletedIndex);

                        UICardFactory.CreateCard(_cardContainer,
                            FormatItemName(item.DefinitionId),
                            $"Ready to claim x{item.StackCount}",
                            sel, true, () => SelectCompletedIndex(captured), preferredHeight: 65);
                    }
                }
            }

            if (completed.Count == 0)
                _selectedCompletedIndex = -1;
            else if (_selectedCompletedIndex >= completed.Count)
                _selectedCompletedIndex = completed.Count - 1;

            RefreshCompletedDetail(completed);
        }

        public void SelectCompletedIndex(int index)
        {
            var completed = _craftService?.GetCompletedItems() ?? new List<ItemActionSaveData>().AsReadOnly();
            if (completed.Count == 0) { _selectedCompletedIndex = -1; Refresh(); return; }
            _selectedCompletedIndex = Mathf.Clamp(index, 0, completed.Count - 1);
            Refresh();
        }

        private void RefreshCompletedDetail(IReadOnlyList<ItemActionSaveData> completed)
        {
            if (_detailText == null) return;

            if (completed.Count == 0 || _selectedCompletedIndex < 0)
            {
                _detailText.text = "No completed items.";
                if (_claimButton != null) _claimButton.interactable = false;
                return;
            }

            var item = completed[_selectedCompletedIndex];
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"<b>Completed Item:</b> {FormatItemName(item.DefinitionId)}");
            sb.AppendLine($"<b>Quantity:</b> x{item.StackCount}");
            sb.AppendLine("Click 'Claim Item 🎁' below to add item to inventory.");
            _detailText.text = sb.ToString();

            if (_claimButton != null)
            {
                _claimButton.interactable = true;
                var txt = _claimButton.GetComponentInChildren<Text>();
                if (txt != null) txt.text = "Claim Item 🎁";
            }
        }

        // ── Actions ───────────────────────────────────────────────────────────────

        public void OnClickCraftSelected()
        {
            var recipes = GetFilteredRecipes();
            if (recipes.Count == 0 || _selectedRecipeIndex < 0 || _selectedRecipeIndex >= recipes.Count) return;

            var r      = recipes[_selectedRecipeIndex];
            var result = _craftService?.TryStartCraft(r.id);

            if (result != null && result.Success)
            {
                ShowFeedback($"🛠️ Added {FormatItemName(r.OutputItemId)} to crafting queue!", true);
            }
            else
            {
                string reason = result != null ? result.FailureReason.ToString() : "Unknown";
                ShowFeedback($"Cannot craft: {reason}", false);
            }

            Refresh();
        }

        public void OnClickClaimSelected()
        {
            var completed = _craftService?.GetCompletedItems();
            if (completed == null || completed.Count == 0 || _selectedCompletedIndex < 0 || _selectedCompletedIndex >= completed.Count) return;

            var item = completed[_selectedCompletedIndex];
            bool ok  = _craftService?.ClaimCompletedCraft(item.InstanceId) ?? false;

            if (ok)
            {
                ShowFeedback($"🎁 Claimed {FormatItemName(item.DefinitionId)} into inventory!", true);
            }
            else
            {
                ShowFeedback("Inventory is full or item cannot be claimed.", false);
            }

            Refresh();
        }

        public void OnClickUpgradeQueue()
        {
            bool ok = _craftService?.UpgradeQueueCapacity() ?? false;
            if (ok)
            {
                ShowFeedback("🎉 Workshop queue capacity upgraded!", true);
            }
            else
            {
                ShowFeedback("Not enough gold to upgrade queue capacity.", false);
            }

            Refresh();
        }

        private void ShowFeedback(string msg, bool success)
        {
            if (_feedbackText == null) return;
            _feedbackText.text  = msg;
            _feedbackText.color = success ? UITemporaryTheme.SuccessColor : UITemporaryTheme.FailureColor;
        }
    }
}
