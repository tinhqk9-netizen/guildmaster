using System;
using System.Collections.Generic;
using System.Linq;
using GuildMaster.Database;
using GuildMaster.Definitions;
using GuildMaster.Runtime.Formulas;
using GuildMaster.Runtime.Models;
using GuildMaster.Runtime.Save;

namespace GuildMaster.Runtime.Services
{
    public class CraftService : ICraftService
    {
        private readonly GameDatabase _database;
        private readonly IInventoryService _inventoryService;
        private readonly ISaveService _saveService;
        private readonly IFormulaService _formulaService;
        private readonly IQuestService _questService;

        public CraftService(
            GameDatabase database, 
            IInventoryService inventoryService, 
            ISaveService saveService, 
            IFormulaService formulaService = null,
            IQuestService questService = null)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            _saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
            _formulaService = formulaService ?? new FormulaService();
            _questService = questService;

            NormalizePersistedWorkshopItems();
        }

        public int GetQueueCapacity()
        {
            var data = _saveService.CurrentData;
            return _formulaService.WorkshopQueue(data.LevelWorkshopQueue, data.UpgradeWorkshopQueue, data.GetPurchaseFlags());
        }

        public IReadOnlyList<ItemActionSaveData> GetQueue()
        {
            return _saveService.CurrentData.WorkshopQueue.AsReadOnly();
        }

        public IReadOnlyList<ItemActionSaveData> GetCompletedItems()
        {
            return _saveService.CurrentData.CompletedWorkshopItems.AsReadOnly();
        }

        public int GetMaxCraftable(string recipeId)
        {
            if (string.IsNullOrEmpty(recipeId)) return 0;
            if (!_database.TryGet<RecipeDefinition>(recipeId, out var recipe)) return 0;
            if (recipe.Ingredients == null || recipe.Ingredients.Count == 0) return 0;

            int maxPossible = int.MaxValue;
            foreach (var ing in recipe.Ingredients)
            {
                if (string.IsNullOrEmpty(ing.ItemId) || ing.Amount <= 0) return 0;
                int owned = _inventoryService.GetQuantityByDefinitionId(ing.ItemId);
                int craftableWithThis = owned / ing.Amount;
                if (craftableWithThis < maxPossible)
                {
                    maxPossible = craftableWithThis;
                }
            }

            return maxPossible == int.MaxValue ? 0 : maxPossible;
        }

        public CraftResult CanCraft(string recipeId)
        {
            if (string.IsNullOrEmpty(recipeId)) return CraftResult.Fail(CraftFailureReason.InvalidRecipeId);

            if (!_database.TryGet<RecipeDefinition>(recipeId, out var recipe))
            {
                return CraftResult.Fail(CraftFailureReason.RecipeNotFound);
            }

            if (string.IsNullOrEmpty(recipe.OutputItemId))
            {
                return CraftResult.Fail(CraftFailureReason.InvalidOutputItem);
            }

            if (!_database.TryGet<ItemDefinition>(recipe.OutputItemId, out _))
            {
                return CraftResult.Fail(CraftFailureReason.InvalidOutputItem);
            }

            if (recipe.Ingredients == null || recipe.Ingredients.Count == 0)
            {
                return CraftResult.Fail(CraftFailureReason.InvalidIngredients);
            }

            if (_saveService.CurrentData.WorkshopQueue.Count >= GetQueueCapacity())
            {
                return CraftResult.Fail(CraftFailureReason.QueueFull);
            }

            foreach (var ingredient in recipe.Ingredients)
            {
                if (string.IsNullOrEmpty(ingredient.ItemId) || ingredient.Amount <= 0)
                {
                    return CraftResult.Fail(CraftFailureReason.InvalidIngredients);
                }

                if (!_inventoryService.HasQuantityByDefinitionId(ingredient.ItemId, ingredient.Amount))
                {
                    return CraftResult.Fail(CraftFailureReason.MissingIngredients);
                }
            }

            return CraftResult.Ok();
        }

        public CraftResult TryStartCraft(string recipeId)
        {
            var validateResult = CanCraft(recipeId);
            if (!validateResult.Success)
            {
                return validateResult;
            }

            _database.TryGet<RecipeDefinition>(recipeId, out var recipe);

            int outputStack = 1;

            // Consume all ingredients
            foreach (var ingredient in recipe.Ingredients)
            {
                bool consumed = _inventoryService.ConsumeByDefinitionId(ingredient.ItemId, ingredient.Amount);
                if (!consumed)
                {
                    return CraftResult.Fail(CraftFailureReason.MissingIngredients);
                }
            }

            var itemAction = new ItemActionSaveData
            {
                InstanceId = Guid.NewGuid().ToString(),
                DefinitionId = recipe.OutputItemId,
                StackCount = outputStack,
                SecondsPassed = 0
            };

            _saveService.CurrentData.WorkshopQueue.Add(itemAction);
            return CraftResult.Ok();
        }

        public void ProgressWorkshop(long deltaSeconds)
        {
            if (deltaSeconds <= 0) return;

            var queue = _saveService.CurrentData.WorkshopQueue;
            if (queue == null || queue.Count == 0) return;

            long remaining = deltaSeconds;
            while (remaining > 0 && queue.Count > 0)
            {
                var activeItem = queue[0];
                long duration = GetCraftDurationSeconds(activeItem);
                if (duration <= 0)
                {
                    UnityEngine.Debug.LogError(
                        $"[CraftService] Cannot progress craft '{activeItem?.InstanceId}': " +
                        $"output item '{activeItem?.DefinitionId}' is not a valid ItemDefinition.");
                    break;
                }
                long required = Math.Max(1L, (duration + 1L) - activeItem.SecondsPassed);
                long applied = Math.Min(remaining, required);
                activeItem.SecondsPassed += applied;
                remaining -= applied;

                // Java completes when secondsPassed becomes strictly greater than
                // secondsToCraft (Utils.progressWorkshopTime).
                if (activeItem.SecondsPassed > duration)
                {
                    queue.RemoveAt(0);
                    _saveService.CurrentData.CompletedWorkshopItems.Add(activeItem);
                }
            }
        }

        public long GetCraftDurationSeconds(ItemActionSaveData item)
        {
            if (item == null || string.IsNullOrEmpty(item.DefinitionId)) return 0;
            if (!_database.TryGet<ItemDefinition>(item.DefinitionId, out var itemDef)) return 0;

            return _formulaService.GetSecondsToCraft(
                itemDef.Price,
                Math.Max(1, item.StackCount),
                _saveService.CurrentData.LevelWorkshopTime,
                _saveService.CurrentData.UpgradeWorkshopTime,
                _saveService.CurrentData.GetPurchaseFlags());
        }

        public bool CancelCraft(string instanceId)
        {
            if (string.IsNullOrEmpty(instanceId)) return false;

            var queue = _saveService.CurrentData.WorkshopQueue;
            var item = queue.FirstOrDefault(x => x.InstanceId == instanceId);
            if (item == null) return false;

            // Legacy refunds the recipe ingredients. Recipes are keyed by output item
            // in the Java ItemAction path, so resolve the matching recipe from the data
            // registry rather than inventing a second payload in SaveData.
            var recipe = _database.GetAll<RecipeDefinition>()?.FirstOrDefault(r => r.OutputItemId == item.DefinitionId);
            if (recipe == null || recipe.Ingredients == null) return false;

            foreach (var ingredient in recipe.Ingredients)
            {
                if (ingredient == null || string.IsNullOrEmpty(ingredient.ItemId) || ingredient.Amount <= 0) continue;
                if (_database.TryGet<ItemDefinition>(ingredient.ItemId, out var ingredientDef))
                {
                    _inventoryService.AddItem(new ItemRuntime(
                        Guid.NewGuid().ToString(), ingredientDef, ingredient.Amount * Math.Max(1, item.StackCount)));
                }
            }

            queue.Remove(item);
            return true;
        }

        public bool ClaimCompletedCraft(string instanceId)
        {
            if (string.IsNullOrEmpty(instanceId)) return false;

            var completed = _saveService.CurrentData.CompletedWorkshopItems;
            var item = completed.FirstOrDefault(x => x.InstanceId == instanceId);
            if (item == null) return false;

            if (!_database.TryGet<ItemDefinition>(item.DefinitionId, out var itemDef)) return false;

            var itemRuntime = new ItemRuntime(Guid.NewGuid().ToString(), itemDef, item.StackCount);
            if (!_inventoryService.CanAddItem(item.DefinitionId)) return false;

            _inventoryService.AddItem(itemRuntime);
            completed.Remove(item);
            // Legacy DialogWorkshop increments MasterCrafter by 1% of the crafted
            // item's price when the completed item is collected.
            long questProgress = (long)(itemDef.Price * 0.01d);
            if (questProgress > 0) _questService?.IncrementDefinition("master_crafter", questProgress);
            _saveService.Save(out _);
            return true;
        }

        public bool UpgradeQueueCapacity()
        {
            var data = _saveService.CurrentData;
            if (data.LevelWorkshopQueue >= 10) return false;
            long price = _formulaService.GetWorkshopQueuePrice(data.LevelWorkshopQueue);
            if (data.Money >= price)
            {
                data.Money -= price;
                data.LevelWorkshopQueue++;
                _saveService.Save(out _);
                return true;
            }
            return false;
        }

        public long GetUpgradeQueueCapacityPrice() => _formulaService.GetWorkshopQueuePrice(_saveService.CurrentData.LevelWorkshopQueue);
        public int GetQueueCapacityLevel() => _saveService.CurrentData.LevelWorkshopQueue;

        public bool UpgradeCraftSpeed()
        {
            var data = _saveService.CurrentData;
            if (data.LevelWorkshopTime >= 25) return false;

            long price = _formulaService.GetWorkshopTimePrice(data.LevelWorkshopTime);
            if (data.Money < price) return false;

            data.Money -= price;
            data.LevelWorkshopTime++;
            _saveService.Save(out _);
            return true;
        }

        private void NormalizePersistedWorkshopItems()
        {
            var data = _saveService.CurrentData;
            if (data == null) return;

            var resolver = new CanonicalItemIdResolver(_database.GetAll<ItemDefinition>());
            bool changed = false;
            NormalizeActions(data.WorkshopQueue, resolver, ref changed);
            NormalizeActions(data.CompletedWorkshopItems, resolver, ref changed);

            if (changed)
                _saveService.Save(out _);
        }

        private static void NormalizeActions(
            List<ItemActionSaveData> actions,
            CanonicalItemIdResolver resolver,
            ref bool changed)
        {
            if (actions == null) return;

            foreach (var action in actions)
            {
                if (action == null) continue;

                if (resolver.TryResolve(action.DefinitionId, out var canonicalId, out var failure))
                {
                    if (!string.Equals(action.DefinitionId, canonicalId, StringComparison.Ordinal))
                    {
                        action.DefinitionId = canonicalId;
                        changed = true;
                    }
                }
                else
                {
                    UnityEngine.Debug.LogError(
                        $"[CraftService] Persisted workshop item '{action.InstanceId}' has unresolved " +
                        $"definition id '{action.DefinitionId}': {failure}.");
                }
            }
        }

        public long GetUpgradeCraftSpeedPrice() => _formulaService.GetWorkshopTimePrice(_saveService.CurrentData.LevelWorkshopTime);
        public int GetCraftSpeedLevel() => _saveService.CurrentData.LevelWorkshopTime;
    }
}
