using System;
using NUnit.Framework;
using GuildMaster.Database;
using GuildMaster.Definitions;
using GuildMaster.Infrastructure.DataProviders;
using GuildMaster.Infrastructure.Serialization;
using GuildMaster.Runtime.Models;
using GuildMaster.Runtime.Save;
using GuildMaster.Runtime.Services;

namespace GuildMaster.Tests.EditMode
{
    [TestFixture]
    public class S6_5A_Stage7_MerchantTests
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
        public void MerchantService_BuyOfferGold_DeductsMoneyAndGrantsItem()
        {
            var merchant = _container.Merchant;
            var save = _container.Save;
            var inv = _container.Inventory;

            var potDef = new ItemDefinition { id = "pot_hp", Category = ItemCategory.Consumable };
            _database.Add(potDef);

            save.CurrentData.Money = 500;
            var offer = new MerchantOfferSaveData
            {
                DefinitionId = "pot_hp",
                StackCount = 2,
                Price = 300,
                IsGems = false
            };
            save.CurrentData.MerchantRegularStockItems.Add(offer);

            bool bought = merchant.BuyOffer(offer, false);

            Assert.IsTrue(bought);
            Assert.AreEqual(200, save.CurrentData.Money); // 500 - 300
            Assert.AreEqual(0, save.CurrentData.MerchantRegularStockItems.Count);
            Assert.AreEqual(2, inv.GetQuantityByDefinitionId("pot_hp"));
        }

        [Test]
        public void MerchantService_BuyOfferGold_FailsWhenInsufficientMoneyAndNoMutation()
        {
            var merchant = _container.Merchant;
            var save = _container.Save;

            save.CurrentData.Money = 100; // Not enough for 300
            var offer = new MerchantOfferSaveData
            {
                DefinitionId = "pot_hp",
                StackCount = 2,
                Price = 300,
                IsGems = false
            };
            save.CurrentData.MerchantRegularStockItems.Add(offer);

            bool bought = merchant.BuyOffer(offer, false);

            Assert.IsFalse(bought);
            Assert.AreEqual(100, save.CurrentData.Money); // Unchanged
            Assert.AreEqual(1, save.CurrentData.MerchantRegularStockItems.Count); // Offer still in stock
        }

        [Test]
        public void MerchantService_BuyOfferGems_DeductsGems()
        {
            var merchant = _container.Merchant;
            var save = _container.Save;
            var inv = _container.Inventory;

            var potDef = new ItemDefinition { id = "pot_mana", Category = ItemCategory.Consumable };
            _database.Add(potDef);

            save.CurrentData.Gems = 50;
            var offer = new MerchantOfferSaveData
            {
                DefinitionId = "pot_mana",
                StackCount = 1,
                Price = 20,
                IsGems = true
            };
            save.CurrentData.MerchantSpecialReserve.Add(offer);

            bool bought = merchant.BuyOffer(offer, true);

            Assert.IsTrue(bought);
            Assert.AreEqual(30, save.CurrentData.Gems); // 50 - 20
            Assert.AreEqual(0, save.CurrentData.MerchantSpecialReserve.Count);
            Assert.AreEqual(1, inv.GetQuantityByDefinitionId("pot_mana"));
        }

        [Test]
        public void MerchantService_SellMarketProgressAndClaim_FlowWorks()
        {
            var merchant = _container.Merchant;
            var save = _container.Save;
            var inv = _container.Inventory;

            var matDef = new ItemDefinition { id = "mat_stone", Category = ItemCategory.Material, SellPrice = 15 };
            _database.Add(matDef);

            inv.AddItem(new ItemRuntime("inst_stone", matDef, 10));
            save.CurrentData.Money = 0;

            var sellResult = merchant.SellItem("mat_stone", 4);
            Assert.IsTrue(sellResult.Success);
            Assert.AreEqual(6, inv.GetQuantityByDefinitionId("mat_stone")); // 10 - 4 = 6
            Assert.AreEqual(1, save.CurrentData.MarketListings.Count);

            // Progress time to sell
            merchant.ProgressMarket(20);
            Assert.AreEqual(0, save.CurrentData.MarketListings.Count);
            Assert.AreEqual(1, save.CurrentData.SoldMarketItems.Count);

            string soldInstId = save.CurrentData.SoldMarketItems[0].InstanceId;
            bool claimed = merchant.ClaimSoldItem(soldInstId);

            Assert.IsTrue(claimed);
            Assert.AreEqual(0, save.CurrentData.SoldMarketItems.Count);
            Assert.AreEqual(60, save.CurrentData.Money); // 4 * 15 = 60
        }
    }
}
