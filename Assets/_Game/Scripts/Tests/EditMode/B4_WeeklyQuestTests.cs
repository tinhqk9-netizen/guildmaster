using System;
using System.Linq;
using NUnit.Framework;
using GuildMaster.Database;
using GuildMaster.Definitions;
using GuildMaster.Runtime.Save;
using GuildMaster.Runtime.Models;
using GuildMaster.Runtime.Services;

namespace GuildMaster.Tests.EditMode
{
    public class B4_WeeklyQuestTests
    {
        private GameDatabase _database;
        private DoctrineService _doctrineService;
        private QuestService _questService;
        private SaveData _saveData;
        private MockSaveService _saveService;

        private class MockSaveService : ISaveService
        {
            public SaveData CurrentData { get; set; }
            public SaveLoadResult LastLoadStatus { get; private set; }
            public event Action OnSaveStarted;
            public event Action<bool> OnSaveCompleted;

            public bool HasSaveFile() => false;

            public bool Load(out Exception error)
            {
                error = null;
                return true;
            }

            public bool Save(out Exception error)
            {
                error = null;
                OnSaveStarted?.Invoke();
                OnSaveCompleted?.Invoke(true);
                return true;
            }

            public void DeleteSave()
            {
                CurrentData = new SaveData();
            }
        }

        [SetUp]
        public void Setup()
        {
            _saveData = new SaveData();
            _saveService = new MockSaveService();
            _saveService.CurrentData = _saveData;

            _database = new GameDatabase();
            foreach (var name in new string[] { "annihilator", "critical_hit", "heavy_armor", "hit_or_miss", "its_a_trap", "long_march", "lucky_roll", "medic", "protector", "smart_fighter", "student", "the_end", "warrior" })
            {
                var def = new QuestDefinition();
                def.id = name;
                _database.Add(def);
            }
            _doctrineService = new DoctrineService(_saveService, new GuildMaster.Runtime.Formulas.FormulaService());

            _questService = new QuestService(_saveService, _database, _doctrineService);
        }

        [Test]
        public void B4_BatchSize_And_ClearOldQuests()
        {
            // Arrange
            _saveData.Quests.Add(new QuestSaveData { InstanceId = "old_1", DefinitionId = "old_quest" });
            _saveData.LastWeekTriggered = 1000;
            // Reload service to read save data
            _questService = new QuestService(_saveService, _database, _doctrineService);
            
            // Act
            long currentUnix = 1000 + 604800; // Exact 1 week
            bool triggered = _questService.CheckAndTriggerWeeklyQuests(currentUnix);

            // Assert
            Assert.IsTrue(triggered, "Should trigger weekly quests.");
            Assert.AreEqual(5, _saveData.Quests.Count, "Batch size should be exactly 5.");
            Assert.IsFalse(_saveData.Quests.Any(q => q.InstanceId == "old_1"), "Old quest should be cleared.");
        }

        [Test]
        public void B4_SafePool_And_RarityRolled()
        {
            // Act
            long currentUnix = 1000000;
            _questService.CheckAndTriggerWeeklyQuests(currentUnix);

            var activeQuests = _questService.GetActiveQuests();

            // Assert
            Assert.AreEqual(5, activeQuests.Count, "Should generate exactly 5 quests.");
            
            // Check duplicates
            var uniqueIds = activeQuests.Select(q => q.Definition.id).Distinct().ToList();
            Assert.AreEqual(5, uniqueIds.Count, "Generated quests must not have duplicate IDs.");

            // Check if they are valid Rarity
            foreach (var q in activeQuests)
            {
                Assert.IsTrue(q.Rarity >= 1 && q.Rarity <= 4, "Rarity must be between 1 and 4.");
            }
        }

        [Test]
        public void B4_NoSpam_OfflineCatchup()
        {
            // Arrange
            _saveData.LastWeekTriggered = 1000;
            
            // Act
            long currentUnix = 1000 + (604800 * 5); // 5 weeks offline
            bool triggered = _questService.CheckAndTriggerWeeklyQuests(currentUnix);

            // Assert
            Assert.IsTrue(triggered, "Should trigger once.");
            Assert.AreEqual(5, _saveData.Quests.Count, "Should only generate 5 quests, not 25.");
            Assert.AreEqual(currentUnix, _saveData.LastWeekTriggered, "Timestamp should update to current catchup time.");
        }

        // We can't mock SaveService easily because it's a concrete class with file IO,
        // but the manual test with try/catch ensures we rollback memory.
    }
}
