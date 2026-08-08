using System;
using System.Collections.Generic;
using NUnit.Framework;
using GuildMaster.Database;
using GuildMaster.Definitions;
using GuildMaster.Infrastructure.DataProviders;
using GuildMaster.Infrastructure.Serialization;
using GuildMaster.Runtime.Models;
using GuildMaster.Runtime.Save;
using GuildMaster.Runtime.Services;
using UnityEngine;

namespace GuildMaster.Tests.EditMode
{
    [TestFixture]
    public sealed class Phase3_WorkshopCraftDataResolutionTests
    {
        private sealed class CountingSaveService : ISaveService
        {
            public SaveData CurrentData { get; private set; }
            public SaveLoadResult LastLoadStatus { get; private set; } = SaveLoadResult.FreshNewGame;
            public int SaveCallCount { get; private set; }
            public event Action OnSaveStarted;
            public event Action<bool> OnSaveCompleted;

            public CountingSaveService(SaveData data = null)
            {
                CurrentData = data ?? SaveData.CreateDefault();
            }

            public bool HasSaveFile() => false;
            public bool Load(out Exception error) { error = null; return true; }

            public bool Save(out Exception error)
            {
                error = null;
                SaveCallCount++;
                OnSaveStarted?.Invoke();
                OnSaveCompleted?.Invoke(true);
                return true;
            }

            public void DeleteSave() => CurrentData = SaveData.CreateDefault();
        }

        private GameDatabase _database;
        private DatabaseBuildReport _buildReport;

        [SetUp]
        public void SetUp()
        {
            _database = new GameDatabase();
            var provider = new EditorExternalGameDataProvider();
            _buildReport = new DatabaseBuilder(provider, new UnityJsonSerializer(), _database).Build();
        }

        private ServiceContainer CreateServices(SaveData data, out CountingSaveService save)
        {
            save = new CountingSaveService(data);
            return new ServiceContainer(_database, save);
        }

        private ServiceContainer CreateServices(out CountingSaveService save)
        {
            return CreateServices(null, out save);
        }

        private ItemDefinition AddItem(string id, long price, ItemCategory category = ItemCategory.Material)
        {
            var item = new ItemDefinition { id = id, Price = price, Category = category };
            _database.Add(item);
            return item;
        }

        private RecipeDefinition AddRecipe(string id, string outputId, string ingredientId, int amount)
        {
            var recipe = new RecipeDefinition
            {
                id = id,
                OutputItemId = outputId,
                Ingredients = new List<IngredientData>
                {
                    new IngredientData { ItemId = ingredientId, Amount = amount }
                }
            };
            _database.Add(recipe);
            return recipe;
        }

        [Test]
        public void AllRecipes_OutputResolvesToItemDefinition()
        {
            Assert.AreEqual(321, _buildReport.loadedRecordsByCategory["recipes_before_item_id_resolution"]);
            Assert.AreEqual(321, _buildReport.loadedRecordsByCategory["recipe_outputs_resolved"]);
            Assert.AreEqual(0, _buildReport.loadedRecordsByCategory["recipe_invalid_removed"]);

            foreach (var recipe in _database.GetAll<RecipeDefinition>())
            {
                Assert.IsNotNull(recipe.OutputItemId, recipe.id);
                Assert.IsTrue(_database.TryGet<ItemDefinition>(recipe.OutputItemId, out _), recipe.id);
            }
        }

        [Test]
        public void Craft_InvalidOutputCannotStart()
        {
            var ingredient = AddItem("phase3_invalid_output_material", 1);
            AddRecipe("phase3_invalid_output_recipe", "phase3_missing_output", ingredient.id, 2);
            var services = CreateServices(out var save);
            services.Inventory.AddItem(new ItemRuntime("phase3_invalid_output_stack", ingredient, 2));

            var result = services.Craft.TryStartCraft("phase3_invalid_output_recipe");

            Assert.IsFalse(result.Success);
            Assert.AreEqual(CraftFailureReason.InvalidOutputItem, result.FailureReason);
            Assert.AreEqual(2, services.Inventory.GetQuantityByDefinitionId(ingredient.id));
            Assert.AreEqual(0, services.Craft.GetQueue().Count);
            Assert.AreEqual(0, save.SaveCallCount);
        }

        [Test]
        public void Craft_ValidRecipeHasPositiveDuration()
        {
            var ingredient = AddItem("phase3_positive_duration_material", 1);
            var output = AddItem("phase3_positive_duration_output", 3, ItemCategory.Weapon);
            AddRecipe("phase3_positive_duration_recipe", output.id, ingredient.id, 1);
            var services = CreateServices(out _);
            services.Inventory.AddItem(new ItemRuntime("phase3_positive_duration_stack", ingredient, 1));

            Assert.IsTrue(services.Craft.TryStartCraft("phase3_positive_duration_recipe").Success);
            Assert.Greater(services.Craft.GetCraftDurationSeconds(services.Craft.GetQueue()[0]), 0);
        }

        [Test]
        public void Craft_CompleteAddsItemToInventory()
        {
            var ingredient = AddItem("phase3_claim_material", 1);
            var output = AddItem("phase3_claim_output", 3, ItemCategory.Weapon);
            AddRecipe("phase3_claim_recipe", output.id, ingredient.id, 1);
            var services = CreateServices(out _);
            services.Inventory.AddItem(new ItemRuntime("phase3_claim_stack", ingredient, 1));

            Assert.IsTrue(services.Craft.TryStartCraft("phase3_claim_recipe").Success);
            long duration = services.Craft.GetCraftDurationSeconds(services.Craft.GetQueue()[0]);
            string instanceId = services.Craft.GetQueue()[0].InstanceId;
            services.Craft.ProgressWorkshop(duration + 1);

            Assert.AreEqual(1, services.Craft.GetCompletedItems().Count);
            Assert.IsTrue(services.Craft.ClaimCompletedCraft(instanceId));
            Assert.AreEqual(0, services.Craft.GetCompletedItems().Count);
            Assert.AreEqual(1, services.Inventory.GetQuantityByDefinitionId(output.id));
        }

        [Test]
        public void Craft_CancelStillRefundsMaterial()
        {
            var ingredient = AddItem("phase3_cancel_material", 1);
            var output = AddItem("phase3_cancel_output", 3, ItemCategory.Weapon);
            AddRecipe("phase3_cancel_recipe_data_fix", output.id, ingredient.id, 2);
            var services = CreateServices(out _);
            services.Inventory.AddItem(new ItemRuntime("phase3_cancel_stack_data_fix", ingredient, 2));

            Assert.IsTrue(services.Craft.TryStartCraft("phase3_cancel_recipe_data_fix").Success);
            string instanceId = services.Craft.GetQueue()[0].InstanceId;
            Assert.AreEqual(0, services.Inventory.GetQuantityByDefinitionId(ingredient.id));

            Assert.IsTrue(services.Craft.CancelCraft(instanceId));
            Assert.AreEqual(2, services.Inventory.GetQuantityByDefinitionId(ingredient.id));
            Assert.AreEqual(0, services.Craft.GetQueue().Count);
        }

        [Test]
        public void Save_LoadKeepsWorkshopQueue()
        {
            var data = SaveData.CreateDefault();
            data.WorkshopQueue.Add(new ItemActionSaveData
            {
                DefinitionId = "woodenbuckler",
                InstanceId = "phase3_alias_queue",
                StackCount = 1,
                SecondsPassed = 17
            });

            var services = CreateServices(data, out var save);
            Assert.AreEqual("wooden_buckler", services.Craft.GetQueue()[0].DefinitionId);
            Assert.AreEqual(17, services.Craft.GetQueue()[0].SecondsPassed);
            Assert.AreEqual(1, save.SaveCallCount);

            string json = JsonUtility.ToJson(save.CurrentData);
            var loaded = JsonUtility.FromJson<SaveData>(json);
            loaded.NormalizeAfterLoad();

            Assert.AreEqual(1, loaded.WorkshopQueue.Count);
            Assert.AreEqual("wooden_buckler", loaded.WorkshopQueue[0].DefinitionId);
            Assert.AreEqual("phase3_alias_queue", loaded.WorkshopQueue[0].InstanceId);
            Assert.AreEqual(17, loaded.WorkshopQueue[0].SecondsPassed);
        }
    }
}
