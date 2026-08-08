using System;
using System.Collections.Generic;
using System.Linq;
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
    public class Phase3_EconomyCoreTests
    {
        private sealed class CountingSaveService : ISaveService
        {
            public SaveData CurrentData { get; private set; } = SaveData.CreateDefault();
            public SaveLoadResult LastLoadStatus { get; private set; } = SaveLoadResult.FreshNewGame;
            public int SaveCallCount { get; private set; }
            public event Action OnSaveStarted;
            public event Action<bool> OnSaveCompleted;

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
        private ServiceContainer _services;
        private CountingSaveService _save;

        [SetUp]
        public void Setup()
        {
            _database = new GameDatabase();
            var provider = new EditorExternalGameDataProvider();
            new DatabaseBuilder(provider, new UnityJsonSerializer(), _database).Build();
            _save = new CountingSaveService();
            _services = new ServiceContainer(_database, _save);
        }

        private ItemDefinition AddItem(string id, long price, ItemCategory category = ItemCategory.Material)
        {
            var item = new ItemDefinition { id = id, Price = price, Category = category };
            _database.Add(item);
            return item;
        }

        [Test]
        public void Workshop_FormulaProcessesMultipleQueueItemsAndUsesSpeedUpgrade()
        {
            var ingredient = AddItem("phase3_ingot", 1);
            var output = AddItem("phase3_sword", 3, ItemCategory.Weapon);
            _database.Add(new RecipeDefinition
            {
                id = "phase3_recipe",
                OutputItemId = output.id,
                Ingredients = new List<IngredientData> { new IngredientData { ItemId = ingredient.id, Amount = 1 } }
            });

            _save.CurrentData.LevelWorkshopQueue = 1; // two base slots
            _services.Inventory.AddItem(new ItemRuntime("phase3_ingots", ingredient, 2));
            Assert.IsTrue(_services.Craft.TryStartCraft("phase3_recipe").Success);
            Assert.IsTrue(_services.Craft.TryStartCraft("phase3_recipe").Success);

            var firstDuration = _services.Craft.GetCraftDurationSeconds(_services.Craft.GetQueue()[0]);
            var secondDuration = _services.Craft.GetCraftDurationSeconds(_services.Craft.GetQueue()[1]);
            Assert.Greater(firstDuration, 0);
            Assert.AreEqual(firstDuration, secondDuration);

            _services.Craft.ProgressWorkshop((firstDuration + 1) + (secondDuration + 1));

            Assert.AreEqual(0, _services.Craft.GetQueue().Count);
            Assert.AreEqual(2, _services.Craft.GetCompletedItems().Count);
        }

        [Test]
        public void Workshop_CancelRefundsIngredientsAndSpeedUpgradeUsesFormulaPrice()
        {
            var ingredient = AddItem("phase3_cancel_ingot", 1);
            var output = AddItem("phase3_cancel_output", 2, ItemCategory.Weapon);
            _database.Add(new RecipeDefinition
            {
                id = "phase3_cancel_recipe",
                OutputItemId = output.id,
                Ingredients = new List<IngredientData> { new IngredientData { ItemId = ingredient.id, Amount = 2 } }
            });

            _save.CurrentData.LevelWorkshopQueue = 1;
            _services.Inventory.AddItem(new ItemRuntime("phase3_cancel_stack", ingredient, 2));
            Assert.IsTrue(_services.Craft.TryStartCraft("phase3_cancel_recipe").Success);
            string instanceId = _services.Craft.GetQueue()[0].InstanceId;
            Assert.AreEqual(0, _services.Inventory.GetQuantityByDefinitionId(ingredient.id));

            Assert.IsTrue(_services.Craft.CancelCraft(instanceId));
            Assert.AreEqual(2, _services.Inventory.GetQuantityByDefinitionId(ingredient.id));
            Assert.AreEqual(0, _services.Craft.GetQueue().Count);

            long price = _services.Craft.GetUpgradeCraftSpeedPrice();
            _save.CurrentData.Money = price;
            Assert.IsTrue(_services.Craft.UpgradeCraftSpeed());
            Assert.AreEqual(1, _services.Craft.GetCraftSpeedLevel());
            Assert.AreEqual(0, _save.CurrentData.Money);
        }

        [Test]
        public void Offline_12HoursDrainsCraftAndMarketQueuesWithoutTimestampLoss()
        {
            var craftItem = AddItem("phase3_offline_craft", 1, ItemCategory.Material);
            var sellItem = AddItem("phase3_offline_sell", 1, ItemCategory.Material);
            _save.CurrentData.LastAccess = 1_000_000L;
            _save.CurrentData.Metadata.SaveTimeUnix = 1_000_000L;
            _services.Inventory.AddItem(new ItemRuntime("phase3_sell_stack", sellItem, 2));

            _save.CurrentData.WorkshopQueue.Add(new ItemActionSaveData
            {
                InstanceId = "craft_a", DefinitionId = craftItem.id, StackCount = 1
            });
            _save.CurrentData.WorkshopQueue.Add(new ItemActionSaveData
            {
                InstanceId = "craft_b", DefinitionId = craftItem.id, StackCount = 1
            });
            Assert.IsTrue(_services.Merchant.SellItem(sellItem.id, 2).Success);

            var result = _services.OfflineProgress.ApplyOfflineProgress(1_000_000L + 86400L);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(12 * 3600, result.DeltaSeconds);
            Assert.AreEqual(0, _services.Craft.GetQueue().Count);
            Assert.AreEqual(2, _services.Craft.GetCompletedItems().Count);
            Assert.AreEqual(0, _services.Merchant.GetMarketListings().Count);
            Assert.AreEqual(1, _services.Merchant.GetSoldMarketItems().Count);
            Assert.AreEqual(1_000_000L + 86400L, _save.CurrentData.LastAccess);
            Assert.AreEqual(1_000_000L + 86400L, _save.CurrentData.Metadata.SaveTimeUnix);
            Assert.AreEqual(1, _save.SaveCallCount); // dungeon FastForward persists once, no per-second saves
        }

        [Test]
        public void Market_RefreshesRegularStockAfterOfflineDayAndSupportsUpgrade()
        {
            var item = AddItem("phase3_market_item", 12);
            _database.Add(new DungeonDefinition
            {
                id = "phase3_market_dungeon",
                RegularMerchantOffers = new List<MerchantOfferData>
                {
                    new MerchantOfferData { ItemId = item.id, StackCount = 2, Weight = 100 }
                },
                SpecialMerchantOffers = new List<MerchantOfferData>()
            });

            long now = 2_000_000L;
            _save.CurrentData.Last24Triggered = now - (2 * 24 * 3600L);
            _save.CurrentData.LastWeekTriggered = now;
            _services.Merchant.ProcessScheduledRefreshes(now);

            Assert.IsTrue(_services.Merchant.GetRegularStock().Any(x => x.DefinitionId == item.id));
            var offer = _services.Merchant.GetRegularStock().First(x => x.DefinitionId == item.id);
            Assert.AreEqual(240L, offer.Price);
            Assert.IsTrue(_save.CurrentData.NewMerchantRegularItems);

            long capacityPrice = _services.Merchant.GetUpgradeMarketListingsPrice();
            _save.CurrentData.Money = capacityPrice;
            Assert.IsTrue(_services.Merchant.UpgradeMarketListings());
            Assert.AreEqual(1, _services.Merchant.GetMarketListingsLevel());

            long speedPrice = _services.Merchant.GetUpgradeMarketTimePrice();
            _save.CurrentData.Money = speedPrice;
            Assert.IsTrue(_services.Merchant.UpgradeMarketTime());
            Assert.AreEqual(1, _services.Merchant.GetMarketTimeLevel());
        }

        [Test]
        public void Storage_UpgradeUsesLegacyPriceIncreasesCapacityAndPersists()
        {
            int initialLevel = _services.Inventory.GetStorageLevel();
            int initialCapacity = _services.Inventory.GetCapacity();
            long price = _services.Inventory.GetUpgradeStorageCapacityPrice();
            _save.CurrentData.Money = price;

            Assert.IsTrue(_services.Inventory.UpgradeStorageCapacity());
            Assert.AreEqual(initialLevel + 1, _services.Inventory.GetStorageLevel());
            Assert.AreEqual(initialCapacity + 1, _services.Inventory.GetCapacity());
            Assert.AreEqual(0L, _save.CurrentData.Money);

            var restored = JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(_save.CurrentData));
            restored.NormalizeAfterLoad();
            Assert.AreEqual(_save.CurrentData.LevelStorage, restored.LevelStorage);
            Assert.AreEqual(_save.CurrentData.UpgradeStorage, restored.UpgradeStorage);
        }

        [Test]
        public void Economy_QueueAndMarketUpgradesSubtractCurrencyAndPersistLevels()
        {
            long queuePrice = _services.Craft.GetUpgradeQueueCapacityPrice();
            _save.CurrentData.Money = queuePrice;
            Assert.IsTrue(_services.Craft.UpgradeQueueCapacity());
            Assert.AreEqual(1, _services.Craft.GetQueueCapacityLevel());
            Assert.AreEqual(0L, _save.CurrentData.Money);

            long marketListingsPrice = _services.Merchant.GetUpgradeMarketListingsPrice();
            _save.CurrentData.Money = marketListingsPrice;
            Assert.IsTrue(_services.Merchant.UpgradeMarketListings());
            Assert.AreEqual(1, _services.Merchant.GetMarketListingsLevel());
            Assert.AreEqual(0L, _save.CurrentData.Money);

            long marketTimePrice = _services.Merchant.GetUpgradeMarketTimePrice();
            _save.CurrentData.Money = marketTimePrice;
            Assert.IsTrue(_services.Merchant.UpgradeMarketTime());
            Assert.AreEqual(1, _services.Merchant.GetMarketTimeLevel());
            Assert.AreEqual(0L, _save.CurrentData.Money);

            var restored = JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(_save.CurrentData));
            restored.NormalizeAfterLoad();
            Assert.AreEqual(_save.CurrentData.LevelWorkshopQueue, restored.LevelWorkshopQueue);
            Assert.AreEqual(_save.CurrentData.LevelMarketListings, restored.LevelMarketListings);
            Assert.AreEqual(_save.CurrentData.LevelMarketTime, restored.LevelMarketTime);
        }

        [Test]
        public void Market_CancelListing_RestoresItemRemovesListingAndPersists()
        {
            var item = AddItem("phase3_cancel_market_item", 12);
            _services.Inventory.AddItem(new ItemRuntime("phase3_cancel_market_inventory", item, 2));
            Assert.IsTrue(_services.Merchant.SellItem(item.id, 2).Success);
            Assert.AreEqual(0, _services.Inventory.GetQuantityByDefinitionId(item.id));
            Assert.AreEqual(1, _services.Merchant.GetMarketListings().Count);

            var listing = _services.Merchant.GetMarketListings()[0];
            int savesBeforeCancel = _save.SaveCallCount;
            Assert.IsTrue(_services.Merchant.CancelListing(listing.InstanceId));

            Assert.AreEqual(0, _services.Merchant.GetMarketListings().Count);
            Assert.AreEqual(2, _services.Inventory.GetQuantityByDefinitionId(item.id));
            Assert.AreEqual(savesBeforeCancel + 1, _save.SaveCallCount);
        }
    }
}
