using System;
using System.Linq;
using NUnit.Framework;
using GuildMaster.Database;
using GuildMaster.Definitions;
using GuildMaster.Infrastructure.DataProviders;
using GuildMaster.Infrastructure.Serialization;
using GuildMaster.Runtime.Save;
using GuildMaster.Runtime.Services;
using GuildMaster.Tools.Phase1Verification;

namespace GuildMaster.Tests
{
    [TestFixture]
    public class Phase1VerificationToolTests
    {
        private class MockSaveService : ISaveService
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
        private ServiceContainer _container;

        [SetUp]
        public void SetUp()
        {
            _database = new GameDatabase();
            var provider = new EditorExternalGameDataProvider();
            var serializer = new UnityJsonSerializer();
            var builder = new DatabaseBuilder(provider, serializer, _database);
            builder.Build();

            var save = new MockSaveService
            {
                CurrentData = new SaveData()
            };
            _container = new ServiceContainer(_database, save);
        }

        [Test]
        public void SpawnBasicHero_CreatesValidTestHero()
        {
            var saveData = Phase1VerificationHelper.SpawnBasicHero(_container);
            Assert.IsNotNull(saveData, "SaveData should be generated.");
            Assert.IsTrue(saveData.InstanceId.StartsWith(Phase1VerificationHelper.TestIdPrefix), "InstanceId should start with DEV_TEST_.");
            Assert.AreEqual("footman", saveData.DefinitionId);
            
            var hero = _container.Character.GetAllCharacters().FirstOrDefault(c => c.InstanceId == saveData.InstanceId);
            Assert.IsNotNull(hero, "Hero should exist in CharacterService.");
        }

        [Test]
        public void SpawnDoubleTraitHero_PreservesBothTraits()
        {
            var saveData = Phase1VerificationHelper.SpawnDoubleTraitHero(_container);
            Assert.IsNotNull(saveData);
            Assert.AreEqual("FERAL", saveData.TraitCommon);
            Assert.AreEqual("DRAGON_BLOOD", saveData.TraitRare);

            var hero = _container.Character.GetAllCharacters().FirstOrDefault(c => c.InstanceId == saveData.InstanceId);
            Assert.IsNotNull(hero);
            Assert.AreEqual("FERAL", hero.TraitCommon);
            Assert.AreEqual("DRAGON_BLOOD", hero.TraitRare);
        }

        [Test]
        public void SpawnPromotionTestHero_SetsMaxLevelAndChoices()
        {
            var saveData = Phase1VerificationHelper.SpawnPromotionTestHero(_container);
            Assert.IsNotNull(saveData);
            Assert.AreEqual("apprentice", saveData.DefinitionId);
            _container.Database.TryGet<AdventurerDefinition>("apprentice", out var apprenticeDef);
            Assert.AreEqual(apprenticeDef.MaxLevel, saveData.Level, "Spawned hero level should match apprentice's real MaxLevel from data.");

            bool canPromote = _container.Promotion.CanPromote(saveData);
            Assert.IsTrue(canPromote, "Apprentice at MaxLevel should be eligible for promotion.");

            var choices = _container.Promotion.GetPromotionChoices(saveData);
            Assert.Greater(choices.Count, 0, "Apprentice should have promotion choices.");
        }

        [Test]
        public void SpawnShowcaseHero_GeneratesLevel15HeroWithTraits()
        {
            var saveData = Phase1VerificationHelper.SpawnShowcaseHero(_container);
            Assert.IsNotNull(saveData);
            Assert.AreEqual(15, saveData.Level);
            Assert.AreEqual("BOOKWORM", saveData.TraitCommon);
            Assert.AreEqual("GIFTED", saveData.TraitRare);
        }

        [Test]
        public void ClearTestData_RemovesOnlyTestHeroes()
        {
            // Spawn normal character
            var normalHero = _container.Character.CreateCharacter("footman");
            normalHero.InstanceId = "NORMAL_HERO_123";

            // Spawn test heroes
            Phase1VerificationHelper.SpawnBasicHero(_container);
            Phase1VerificationHelper.SpawnDoubleTraitHero(_container);
            Phase1VerificationHelper.SpawnPromotionTestHero(_container);

            Assert.AreEqual(4, _container.Character.GetAllCharacters().Count);

            // Clear test data
            int cleared = Phase1VerificationHelper.ClearTestData(_container);
            Assert.AreEqual(3, cleared, "Should clear exactly 3 test heroes.");

            var remaining = _container.Character.GetAllCharacters();
            Assert.AreEqual(1, remaining.Count, "Only normal hero should remain.");
            Assert.AreEqual("NORMAL_HERO_123", remaining[0].InstanceId);
        }
    }
}
