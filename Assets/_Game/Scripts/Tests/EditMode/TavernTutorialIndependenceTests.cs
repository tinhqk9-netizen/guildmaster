using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using GuildMaster.Database;
using GuildMaster.Infrastructure.DataProviders;
using GuildMaster.Infrastructure.Serialization;
using GuildMaster.Runtime.Core;
using GuildMaster.Runtime.Save;
using GuildMaster.Runtime.Services;

namespace GuildMaster.Tests.EditMode
{
    [TestFixture]
    public class TavernTutorialIndependenceTests
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

        [SetUp]
        public void SetUp()
        {
            _database = new GameDatabase();
            new DatabaseBuilder(
                new EditorExternalGameDataProvider(),
                new UnityJsonSerializer(),
                _database).Build();
        }

        private ServiceContainer CreateServices(int tutorialStep, int tavernCapacity = 50)
        {
            var data = SaveData.CreateDefault();
            data.TutorialStep = tutorialStep;
            data.LevelTavernCapacity = tavernCapacity;
            data.LevelQuarters = tavernCapacity;
            return new ServiceContainer(
                _database,
                new MockSaveService { CurrentData = data },
                null,
                new RuntimeFactory(new DefaultInstanceIdGenerator()));
        }

        [Test]
        public void TavernGeneration_DoesNotUseTutorialStepForSpecialClassesOrMutation()
        {
            var expectedPool = new HashSet<string>(
                ((TavernService)CreateServices(0).Tavern).GetDeveloperVisitorClassPool(),
                StringComparer.OrdinalIgnoreCase);

            foreach (int tutorialStep in new[] { 0, 6, 7, 8 })
            {
                var services = CreateServices(tutorialStep);
                services.Tavern.GenerateVisitor();
                var guest = services.Tavern.GetGuests().Single();

                Assert.IsTrue(expectedPool.Contains(guest.DefinitionId),
                    $"TutorialStep {tutorialStep} generated a class outside normal Tavern pool.");
                Assert.AreEqual(tutorialStep, services.Save.CurrentData.TutorialStep,
                    "Tavern generation must not mutate tutorial progression.");
            }
        }

        [Test]
        public void TutorialStepZero_DoesNotForceFootmanGeneration()
        {
            var services = CreateServices(0);
            var generatedClasses = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < 40; i++)
            {
                services.Tavern.GenerateVisitor();
                generatedClasses.Add(services.Tavern.GetGuests()[0].DefinitionId);
            }

            Assert.IsTrue(generatedClasses.Any(id => !string.Equals(id, "footman", StringComparison.OrdinalIgnoreCase)),
                "TutorialStep 0 must not force every visitor to Footman.");
        }

        [Test]
        public void TutorialStepSeven_DoesNotForceArcherOrLegacyTrait()
        {
            var services = CreateServices(7);
            var generatedClasses = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < 40; i++)
            {
                services.Tavern.GenerateVisitor();
                var guest = services.Tavern.GetGuests()[0];
                generatedClasses.Add(guest.DefinitionId);
            }

            Assert.IsTrue(generatedClasses.Any(id => !string.Equals(id, "archer", StringComparison.OrdinalIgnoreCase)),
                "TutorialStep 7 must not force every visitor to Archer.");
            Assert.AreEqual(7, services.Save.CurrentData.TutorialStep);
        }
    }
}
