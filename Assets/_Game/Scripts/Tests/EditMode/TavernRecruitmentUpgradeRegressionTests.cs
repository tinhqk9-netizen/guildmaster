using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using GuildMaster.Database;
using GuildMaster.Infrastructure.DataProviders;
using GuildMaster.Infrastructure.Serialization;
using GuildMaster.Runtime.Core;
using GuildMaster.Runtime.Save;
using GuildMaster.Runtime.Services;
using GuildMaster.Runtime.UI.Character;

namespace GuildMaster.Tests.EditMode
{
    [TestFixture]
    public class TavernRecruitmentUpgradeRegressionTests
    {
        private sealed class MockSaveService : ISaveService
        {
            public SaveData CurrentData { get; set; }
            public SaveLoadResult LastLoadStatus { get; private set; } = SaveLoadResult.FreshNewGame;
            public event Action OnSaveStarted;
            public event Action<bool> OnSaveCompleted;
            public bool HasSaveFile() => false;
            public bool Load(out Exception error) { error = null; return true; }
            public bool Save(out Exception error) { error = null; return true; }
            public void DeleteSave() { }
        }

        private GameDatabase _database;
        private GameObject _rosterHost;

        [SetUp]
        public void SetUp()
        {
            _database = new GameDatabase();
            new DatabaseBuilder(
                new EditorExternalGameDataProvider(),
                new UnityJsonSerializer(),
                _database).Build();
        }

        [TearDown]
        public void TearDown()
        {
            if (_rosterHost != null)
                UnityEngine.Object.DestroyImmediate(_rosterHost);
        }

        private ServiceContainer CreateServices(out MockSaveService save, SaveData data = null)
        {
            data ??= SaveData.CreateDefault();
            data.LevelQuarters = Math.Max(data.LevelQuarters, 5);
            data.LevelTavernCapacity = Math.Max(data.LevelTavernCapacity, 5);
            save = new MockSaveService { CurrentData = data };
            return new ServiceContainer(_database, save, null,
                new RuntimeFactory(new DefaultInstanceIdGenerator()));
        }

        [Test]
        public void RecruitHero_IsVisibleToAdventurersRosterSource()
        {
            var services = CreateServices(out _);
            ((TavernService)services.Tavern).GenerateVisitorForDeveloper("archer");
            Assert.IsTrue(services.Tavern.RecruitGuest(0, out var recruited));

            _rosterHost = new GameObject("AdventurersRosterTestHost");
            var roster = _rosterHost.AddComponent<AdventurersTabController>();
            roster.Setup(services);

            var content = _rosterHost.transform.Find(
                "Phase6AdventurersContent/RosterScroll/RosterContent");
            Assert.IsNotNull(content, "Adventurers roster content should exist.");
            Assert.IsNotNull(content.Find("AdventurerCard_" + recruited.InstanceId),
                "A recruited hero must be rendered by the Adventurers roster source.");
        }

        [Test]
        public void RecruitHero_PersistsAfterSaveLoad()
        {
            var services = CreateServices(out var save);
            ((TavernService)services.Tavern).GenerateVisitorForDeveloper("archer");
            Assert.IsTrue(services.Tavern.RecruitGuest(0, out var recruited));
            save.Save(out _);

            var reloaded = CreateServices(out _, save.CurrentData);
            var loaded = reloaded.Character.GetAllCharacters()
                .Single(c => c.InstanceId == recruited.InstanceId);

            Assert.IsNotNull(loaded);
            Assert.IsTrue(reloaded.Save.CurrentData.Characters
                .Any(c => c.InstanceId == recruited.InstanceId));
        }

        [Test]
        public void TavernCapacityUpgrade_IncreasesLimitAndConsumesGold()
        {
            var services = CreateServices(out var save);
            var tavern = services.Tavern;
            long cost = tavern.GetUpgradeTavernCapacityPrice();
            save.CurrentData.Money = cost + 123;
            int oldLevel = tavern.GetTavernCapacityLevel();
            int oldCapacity = tavern.GetTavernCapacity();

            Assert.IsTrue(tavern.UpgradeTavernCapacity());
            Assert.AreEqual(oldLevel + 1, tavern.GetTavernCapacityLevel());
            Assert.Greater(tavern.GetTavernCapacity(), oldCapacity);
            Assert.AreEqual(123, save.CurrentData.Money);
        }

        [Test]
        public void TavernSpeedUpgrade_DecreasesIntervalAndConsumesGold()
        {
            var services = CreateServices(out var save);
            var tavern = services.Tavern;
            long cost = tavern.GetUpgradeTavernTimePrice();
            save.CurrentData.Money = cost + 123;
            long oldInterval = tavern.GetVisitorIntervalSeconds();
            int oldLevel = tavern.GetTavernTimeLevel();

            Assert.IsTrue(tavern.UpgradeTavernTime());
            Assert.AreEqual(oldLevel + 1, tavern.GetTavernTimeLevel());
            Assert.Less(tavern.GetVisitorIntervalSeconds(), oldInterval);
            Assert.AreEqual(123, save.CurrentData.Money);
        }
    }
}
