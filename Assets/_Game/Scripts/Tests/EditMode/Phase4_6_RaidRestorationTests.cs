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

namespace GuildMaster.Tests.EditMode
{
    [TestFixture]
    public sealed class Phase4_6_RaidRestorationTests
    {
        private sealed class InMemorySaveService : ISaveService
        {
            public SaveData CurrentData { get; private set; } = SaveData.CreateDefault();
            public SaveLoadResult LastLoadStatus { get; private set; } = SaveLoadResult.FreshNewGame;
            public event Action OnSaveStarted;
            public event Action<bool> OnSaveCompleted;

            public bool HasSaveFile() => false;

            public bool Load(out Exception error)
            {
                error = null;
                CurrentData.NormalizeAfterLoad();
                return true;
            }

            public bool Save(out Exception error)
            {
                error = null;
                OnSaveStarted?.Invoke();
                OnSaveCompleted?.Invoke(true);
                return true;
            }

            public void DeleteSave() => CurrentData = SaveData.CreateDefault();
        }

        private GameDatabase _database;
        private InMemorySaveService _save;
        private ServiceContainer _services;

        [SetUp]
        public void SetUp()
        {
            _database = new GameDatabase();
            var builder = new DatabaseBuilder(new EditorExternalGameDataProvider(), new UnityJsonSerializer(), _database);
            var report = builder.Build();
            Assert.IsEmpty(report.errors, "Database build must be clean before raid tests.");
            _save = new InMemorySaveService();
            _services = new ServiceContainer(_database, _save);
        }

        [Test]
        public void AllTwelveRaids_LoadWithLegacyMetadataAndResolvableEncounters()
        {
            var expectedIds = new[]
            {
                "ancient_grave_digging", "celestial_mothership", "divine_archeology", "imperial_rescue",
                "kaunis", "sleeping_planet", "the_cultist_rebels", "the_dire_descent",
                "the_dreadful_ascent", "the_lost_expedition", "the_slime_pond", "the_tower"
            };
            var raids = _database.GetAll<RaidDefinition>().ToList();

            CollectionAssert.AreEquivalent(expectedIds, raids.Select(raid => raid.id));
            Assert.That(raids, Has.Count.EqualTo(12));
            foreach (var raid in raids)
            {
                Assert.Greater(raid.LegacyMaxProgress, 0, raid.id + " must have a Legacy max progress.");
                Assert.That(raid.LegacyEventKeys, Is.Not.Null);
                Assert.That(raid.LegacyEncounters, Is.Not.Null);
                Assert.That(raid.LegacyEncounters.Count > 0 || raid.IsEventDriven,
                    Is.True, raid.id + " has neither fixed encounter data nor an event-driven branch.");
                foreach (var encounter in raid.LegacyEncounters)
                {
                    Assert.That(encounter.EnemyIds, Is.Not.Null);
                    foreach (string enemyId in encounter.EnemyIds)
                        Assert.That(_database.TryGet<EnemyDefinition>(enemyId, out _), Is.True,
                            raid.id + " has an unresolved encounter enemy " + enemyId);
                }
            }

            var divine = raids.Single(raid => raid.id == "divine_archeology");
            Assert.AreEqual("eyes_of_the_swordsman", divine.LegacyEncounters.Single(e => e.LegacyProgress == 9).UniqueRewardItemId);
            Assert.AreEqual("divine_zygote", divine.LegacyEncounters.Single(e => e.LegacyProgress == 12).UniqueRewardItemId);
            var missingUniqueItems = raids.SelectMany(raid => raid.LegacyEncounters)
                .Where(encounter => !string.IsNullOrEmpty(encounter.UniqueRewardItemId))
                .Where(encounter => !_database.TryGet<ItemDefinition>(encounter.UniqueRewardItemId, out _))
                .Select(encounter => encounter.UniqueRewardItemId)
                .ToList();
            Assert.IsEmpty(missingUniqueItems, "Missing unique reward definitions: " + string.Join(", ", missingUniqueItems));
        }

        [Test]
        public void UnlockRules_UseDecodedDungeonProgress()
        {
            foreach (var raid in _database.GetAll<RaidDefinition>())
            {
                if (string.IsNullOrEmpty(raid.RequiredClearDungeonId))
                {
                    Assert.IsTrue(_services.Raid.IsUnlocked(raid), raid.id + " should be open without a gate.");
                    continue;
                }

                _save.CurrentData.Dungeons.Add(new DungeonSaveData
                {
                    DefinitionId = raid.RequiredClearDungeonId,
                    MaxProgress = Math.Max(0, raid.RequiredClearProgress - 1)
                });
                Assert.IsFalse(_services.Raid.IsUnlocked(raid), raid.id + " unlocked below its gate.");
                _save.CurrentData.Dungeons[0].MaxProgress = raid.RequiredClearProgress;
                Assert.IsTrue(_services.Raid.IsUnlocked(raid), raid.id + " stayed locked at its gate.");
                _save.CurrentData.Dungeons.Clear();
            }
        }

        [Test]
        public void StartCultistRaid_TriggersPersistedLegacyEventWithoutNullReference()
        {
            UnlockRequiredDungeon("frostbite_peaks", 150);
            AddCurrentPartyHero();

            Assert.IsTrue(_services.Raid.StartRaid("the_cultist_rebels", null, out var error), error);
            Assert.IsNotNull(_services.Raid.ActiveRaid);
            Assert.AreEqual("halls_exploration", _services.Raid.ActiveRaid.EventKey);
            Assert.GreaterOrEqual(_services.Raid.ActiveRaid.EventProgress, 1);
            Assert.IsNotNull(_save.CurrentData.ActiveRaid);
            Assert.AreEqual("halls_exploration", _save.CurrentData.ActiveRaid.EventKey);
        }

        [Test]
        public void StartTowerRaid_ResolvesDecodedBossEncounter()
        {
            AddCurrentPartyHero();

            Assert.IsTrue(_services.Raid.StartRaid("the_tower", null, out var error), error);
            var active = _services.Raid.ActiveRaid;
            Assert.IsNotNull(active);
            Assert.AreEqual(8, active.LegacyProgress);
            Assert.That(active.Enemies, Has.Count.EqualTo(1));
            Assert.AreEqual("Lazarus", active.Enemies[0].Definition.className);
            StringAssert.Contains("BOSS", string.Join("\n", active.Log));
        }

        [Test]
        public void StartKaunisRaid_ResolvesRegularMultiEnemyEncounter()
        {
            AddCurrentPartyHero();

            Assert.IsTrue(_services.Raid.StartRaid("kaunis", null, out var error), error);
            Assert.IsNotNull(_services.Raid.ActiveRaid);
            Assert.AreEqual(1, _services.Raid.ActiveRaid.LegacyProgress);
            Assert.That(_services.Raid.ActiveRaid.Enemies, Has.Count.EqualTo(3));
            Assert.IsFalse(_services.Raid.ActiveRaid.HasActiveEvent);
        }

        [Test]
        public void ActiveRaid_RoundTripsRoomEventPartyAndEnemyState()
        {
            AddCurrentPartyHero();
            Assert.IsTrue(_services.Raid.StartRaid("the_tower", null, out var error), error);
            var before = _services.Raid.ActiveRaid;
            before.EventKey = "test_event_state";
            before.EventProgress = 3;
            before.EventOutcome = "Persisted outcome";
            before.Enemies[0].CurrentHp = Math.Max(1, before.Enemies[0].CurrentHp - 7);

            // The service's public actions save state. Abandon is intentionally not used here.
            Assert.IsTrue(_services.Raid.FightCurrentRoom(out _).ToString().Length > 0);
            // Re-apply the state check to the saved payload after a normal service save.
            Assert.IsNotNull(_save.CurrentData.ActiveRaid);
            var reloaded = new ServiceContainer(_database, _save);

            Assert.IsNotNull(reloaded.Raid.ActiveRaid);
            Assert.AreEqual(_save.CurrentData.ActiveRaid.LegacyProgress, reloaded.Raid.ActiveRaid.LegacyProgress);
            Assert.AreEqual(_save.CurrentData.ActiveRaid.EventKey, reloaded.Raid.ActiveRaid.EventKey);
            Assert.AreEqual(_save.CurrentData.ActiveRaid.EventProgress, reloaded.Raid.ActiveRaid.EventProgress);
            Assert.AreEqual(_save.CurrentData.ActiveRaid.Enemies.Count, reloaded.Raid.ActiveRaid.Enemies.Count);
            Assert.AreEqual(_save.CurrentData.ActiveRaid.Party.Count, reloaded.Raid.ActiveRaid.Party.Count);
        }

        private void UnlockRequiredDungeon(string dungeonId, int progress)
        {
            _save.CurrentData.Dungeons.Add(new DungeonSaveData
            {
                DefinitionId = dungeonId,
                MaxProgress = progress
            });
        }

        private CharacterRuntime AddCurrentPartyHero()
        {
            var definition = _database.GetAll<AdventurerDefinition>().First();
            var hero = _services.Character.CreateCharacter(definition.id);
            _save.CurrentData.CurrentParty.Add(hero.InstanceId);
            return hero;
        }
    }
}
