using System;
using NUnit.Framework;
using GuildMaster.Database;
using GuildMaster.Infrastructure.DataProviders;
using GuildMaster.Infrastructure.Serialization;
using GuildMaster.Runtime.Formulas;
using GuildMaster.Runtime.Save;
using GuildMaster.Runtime.Services;

namespace GuildMaster.Tests.EditMode
{
    [TestFixture]
    public class S6_5A_Stage2_ServiceWiringTests
    {
        private GameDatabase _database;

        [SetUp]
        public void Setup()
        {
            _database = new GameDatabase();
            var provider = new EditorExternalGameDataProvider();
            var serializer = new UnityJsonSerializer();
            var builder = new DatabaseBuilder(provider, serializer, _database);
            builder.Build();
        }

        [Test]
        public void ServiceContainer_Initialization_AllServicesNotNull()
        {
            var container = new ServiceContainer(_database);

            Assert.IsNotNull(container.Database);
            Assert.IsNotNull(container.Formula);
            Assert.IsNotNull(container.Save);
            Assert.IsNotNull(container.Factory);
            Assert.IsNotNull(container.Item);
            Assert.IsNotNull(container.Inventory);
            Assert.IsNotNull(container.Character);
            Assert.IsNotNull(container.Equipment);
            Assert.IsNotNull(container.Skill);
            Assert.IsNotNull(container.StatusEffect);
            Assert.IsNotNull(container.Craft);
            Assert.IsNotNull(container.Merchant);
            Assert.IsNotNull(container.Dungeon);
            Assert.IsNotNull(container.Combat);
            Assert.IsNotNull(container.TargetSelection);
            Assert.IsNotNull(container.Loot);
            Assert.IsNotNull(container.Quest);
            Assert.IsNotNull(container.Doctrine);
            Assert.IsNotNull(container.Tavern);
            Assert.IsNotNull(container.Settings);
            Assert.IsNotNull(container.OfflineProgress);
        }

        [Test]
        public void ServiceContainer_SharedSaveService_MutationsReflectedAcrossServices()
        {
            var saveService = new SaveService();
            saveService.Load(out _);
            var container = new ServiceContainer(_database, saveService);

            // Mutate via Settings
            container.Settings.SetToggle("music", false);
            Assert.IsFalse(saveService.CurrentData.SettingsMusic);

            // Mutate via Doctrine
            container.Doctrine.AddProgress("war", 2);
            Assert.AreEqual(2, container.Doctrine.GetProgress("war"));
            Assert.AreEqual(2, saveService.CurrentData.WarProgress);
        }

        [Test]
        public void DoctrineService_ProgressAndLevelUp_WorksCorrectly()
        {
            var container = new ServiceContainer(_database);
            
            // TotalStarsToNextLp(0) = 4
            Assert.AreEqual(0, container.Doctrine.GetLevel("war"));
            Assert.AreEqual(0, container.Doctrine.GetProgress("war"));

            container.Doctrine.AddProgress("war", 2);
            Assert.AreEqual(0, container.Doctrine.GetLevel("war"));
            Assert.AreEqual(2, container.Doctrine.GetProgress("war"));

            // Add 3 more (total 5 progress -> level up to 1 with 1 rollover progress)
            container.Doctrine.AddProgress("war", 3);
            Assert.AreEqual(1, container.Doctrine.GetLevel("war"));
            Assert.AreEqual(1, container.Doctrine.GetProgress("war"));
        }

        [Test]
        public void TavernService_Capacities_CalculatedFromSaveAndFormula()
        {
            var container = new ServiceContainer(_database);

            int tavernCap = container.Tavern.GetTavernCapacity();
            int quartersCap = container.Tavern.GetQuartersCapacity();

            Assert.Greater(tavernCap, 0);
            Assert.Greater(quartersCap, 0);
            Assert.IsTrue(container.Tavern.CanRecruit());
        }

        [Test]
        public void SettingsService_TogglesAndLanguage_PersistedInSave()
        {
            var container = new ServiceContainer(_database);

            container.Settings.SetToggle("sound", false);
            Assert.IsFalse(container.Settings.GetToggle("sound"));

            container.Settings.SetLanguage("vi");
            Assert.AreEqual("vi", container.Settings.GetLanguage());
        }
    }
}
