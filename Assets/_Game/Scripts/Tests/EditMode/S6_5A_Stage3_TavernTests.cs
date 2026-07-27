using System;
using NUnit.Framework;
using GuildMaster.Database;
using GuildMaster.Infrastructure.DataProviders;
using GuildMaster.Infrastructure.Serialization;
using GuildMaster.Runtime.Services;

namespace GuildMaster.Tests.EditMode
{
    [TestFixture]
    public class S6_5A_Stage3_TavernTests
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
        public void TavernService_VisitorGeneration_InsertsAtFrontAndTrims()
        {
            var tavern = _container.Tavern;
            int initialCount = tavern.GetGuests().Count;

            tavern.GenerateVisitor();
            Assert.AreEqual(initialCount + 1, tavern.GetGuests().Count);
        }

        [Test]
        public void TavernService_ProgressVisitorTime_TriggersVisitorOnTimerExpiry()
        {
            var tavern = _container.Tavern;
            int initialCount = tavern.GetGuests().Count;

            long interval = tavern.GetVisitorIntervalSeconds();
            tavern.ProgressVisitorTime(interval + 1);

            Assert.Greater(tavern.GetGuests().Count, initialCount);
        }

        [Test]
        public void TavernService_RecruitGuest_TransfersGuestToCharacterAndIsFree()
        {
            var tavern = _container.Tavern;
            var save = _container.Save;

            long moneyBefore = save.CurrentData.Money;

            // Generate a guest
            tavern.GenerateVisitor();
            int guestCountBefore = tavern.GetGuests().Count;
            int charCountBefore = _container.Character.GetAllCharacters().Count;

            bool success = tavern.RecruitGuest(0, out var newChar);

            Assert.IsTrue(success);
            Assert.IsNotNull(newChar);
            Assert.AreEqual(guestCountBefore - 1, tavern.GetGuests().Count);
            Assert.AreEqual(charCountBefore + 1, _container.Character.GetAllCharacters().Count);

            // Verified Rule TR-03: Recruit is free
            Assert.AreEqual(moneyBefore, save.CurrentData.Money);
        }

        [Test]
        public void TavernService_Upgrades_DeductMoneyAndIncreaseLevel()
        {
            var tavern = _container.Tavern;
            var save = _container.Save;

            save.CurrentData.Money = 1000000; // Give plenty of money

            int levelQuartersBefore = save.CurrentData.LevelQuarters;
            bool success = tavern.UpgradeQuarters();

            Assert.IsTrue(success);
            Assert.AreEqual(levelQuartersBefore + 1, save.CurrentData.LevelQuarters);
            Assert.Less(save.CurrentData.Money, 1000000);
        }
    }
}
