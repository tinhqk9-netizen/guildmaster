using System;
using System.Collections.Generic;
using NUnit.Framework;
using GuildMaster.Database;
using GuildMaster.Definitions;
using GuildMaster.Infrastructure.DataProviders;
using GuildMaster.Infrastructure.Serialization;
using GuildMaster.Runtime.Models;
using GuildMaster.Runtime.Services;

namespace GuildMaster.Tests.EditMode
{
    [TestFixture]
    public class S6_5A_Stage6_CraftTests
    {
        private GameDatabase _database;
        private ServiceContainer _container;

        [SetUp]
        public void Setup()
        {
            _database = new GameDatabase();
            var provider = new EditorExternalGameDataProvider();
            var serializer = new UnityJsonSerializer();
            var builder = new DatabaseBuilder(provider, serializer, _database);
            builder.Build();

            _container = new ServiceContainer(_database);
        }

        [Test]
        public void CraftService_GetMaxCraftable_CalculatesBasedOnIngredients()
        {
            var craft = _container.Craft;
            var inv = _container.Inventory;

            var ironDef = new ItemDefinition { id = "mat_iron", Category = ItemCategory.Material };
            inv.AddItem(new ItemRuntime("inst_iron", ironDef, 10));

            var recipe = new RecipeDefinition
            {
                id = "recipe_sword",
                OutputItemId = "wp_sword",
                Ingredients = new List<IngredientData>
                {
                    new IngredientData { ItemId = "mat_iron", Amount = 3 }
                }
            };
            _database.Add(recipe);

            // 10 iron / 3 per craft = 3 craftable
            Assert.AreEqual(3, craft.GetMaxCraftable("recipe_sword"));
        }

        [Test]
        public void CraftService_TryStartCraftAndProgressAndClaim_FlowWorks()
        {
            var craft = _container.Craft;
            var inv = _container.Inventory;

            var ironDef = new ItemDefinition { id = "mat_iron", Category = ItemCategory.Material };
            var swordDef = new ItemDefinition { id = "wp_sword", Category = ItemCategory.Weapon };
            _database.Add(swordDef);

            inv.AddItem(new ItemRuntime("inst_iron", ironDef, 5));

            var recipe = new RecipeDefinition
            {
                id = "recipe_sword",
                OutputItemId = "wp_sword",
                Ingredients = new List<IngredientData>
                {
                    new IngredientData { ItemId = "mat_iron", Amount = 3 }
                }
            };
            _database.Add(recipe);

            var startResult = craft.TryStartCraft("recipe_sword");
            Assert.IsTrue(startResult.Success);
            Assert.AreEqual(2, inv.GetQuantityByDefinitionId("mat_iron")); // 5 - 3 = 2
            Assert.AreEqual(1, craft.GetQueue().Count);

            // Progress time to complete
            craft.ProgressWorkshop(10);
            Assert.AreEqual(0, craft.GetQueue().Count);
            Assert.AreEqual(1, craft.GetCompletedItems().Count);

            string completedInstanceId = craft.GetCompletedItems()[0].InstanceId;
            bool claimed = craft.ClaimCompletedCraft(completedInstanceId);

            Assert.IsTrue(claimed);
            Assert.AreEqual(0, craft.GetCompletedItems().Count);
            Assert.AreEqual(1, inv.GetQuantityByDefinitionId("wp_sword"));
        }
    }
}
