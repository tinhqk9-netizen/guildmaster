using System;
using System.Linq;
using GuildMaster.Database;
using GuildMaster.Definitions;
using GuildMaster.Runtime.Models;
using GuildMaster.Runtime.Save;

namespace GuildMaster.Runtime.Services
{
    public class CraftService : ICraftService
    {
        private readonly GameDatabase _database;
        private readonly IInventoryService _inventoryService;
        private readonly ISaveService _saveService;

        public CraftService(GameDatabase database, IInventoryService inventoryService, ISaveService saveService)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            _saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
        }

        public CraftResult CanCraft(string recipeId)
        {
            if (string.IsNullOrEmpty(recipeId)) return CraftResult.Fail(CraftFailureReason.InvalidRecipeId);

            if (!_database.TryGet<RecipeDefinition>(recipeId, out var recipe))
            {
                return CraftResult.Fail(CraftFailureReason.RecipeNotFound);
            }

            if (recipe.manualRuleRequired)
            {
                return CraftResult.Fail(CraftFailureReason.ManualRuleRequired);
            }

            if (string.IsNullOrEmpty(recipe.OutputItemId))
            {
                return CraftResult.Fail(CraftFailureReason.InvalidOutputItem);
            }

            if (recipe.Ingredients == null || recipe.Ingredients.Count == 0)
            {
                return CraftResult.Fail(CraftFailureReason.InvalidIngredients);
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

            // Java evidence states Item.getInstance(name()) has a default stack of 1.
            // RecipeDefinition currently does not have an explicit OutputStack field in JSON.
            // Using safe assumption of 1 based on base Java item generation behavior for single crafts.
            int outputStack = 1;

            // Consume all ingredients only after passing validation
            foreach (var ingredient in recipe.Ingredients)
            {
                bool consumed = _inventoryService.ConsumeByDefinitionId(ingredient.ItemId, ingredient.Amount);
                if (!consumed)
                {
                    // This shouldn't happen due to pre-validation, but fallback just in case
                    return CraftResult.Fail(CraftFailureReason.MissingIngredients);
                }
            }

            var itemAction = new ItemActionSaveData
            {
                InstanceId = Guid.NewGuid().ToString(), // Generate a runtime tracking ID
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

            // Update time passed for the active item
            var activeItem = queue[0];
            activeItem.SecondsPassed += deltaSeconds;

            // Craft completion deferred because duration formula incomplete.
            // Cannot accurately move to CompletedWorkshopItems without LevelWorkshopTime properly wired in schema and formula.
        }
    }
}
