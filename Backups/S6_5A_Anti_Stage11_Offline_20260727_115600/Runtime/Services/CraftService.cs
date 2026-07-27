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

        private const int DEFAULT_CRAFT_DURATION_SECONDS = 10;

        public CraftService(
            GameDatabase database, 
            IInventoryService inventoryService, 
            ISaveService saveService, 
            IFormulaService formulaService = null)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            _saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
            _formulaService = formulaService ?? new FormulaService();
        }

        public int GetQueueCapacity()
        {
            var data = _saveService.CurrentData;
            return _formulaService.WorkshopQueue(data.LevelWorkshopQueue, data.UpgradeWorkshopQueue);
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

            var activeItem = queue[0];
            activeItem.SecondsPassed += deltaSeconds;

            if (activeItem.SecondsPassed >= DEFAULT_CRAFT_DURATION_SECONDS)
            {
                queue.RemoveAt(0);
                _saveService.CurrentData.CompletedWorkshopItems.Add(activeItem);
            }
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
            return true;
        }
    }
}
