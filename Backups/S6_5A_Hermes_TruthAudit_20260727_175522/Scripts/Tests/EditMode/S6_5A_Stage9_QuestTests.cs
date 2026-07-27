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
    public class S6_5A_Stage9_QuestTests
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
        public void QuestService_RewardFromRarity_ReturnsExactRecoveredValues()
        {
            var questService = _container.Quest;

            // Rarity 1: 1 LP / 10 Gems
            Assert.AreEqual(1, questService.GetRewardAmount(1, false));
            Assert.AreEqual(10, questService.GetRewardAmount(1, true));

            // Rarity 2: 2 LP / 20 Gems
            Assert.AreEqual(2, questService.GetRewardAmount(2, false));
            Assert.AreEqual(20, questService.GetRewardAmount(2, true));

            // Rarity 3: 3 LP / 40 Gems
            Assert.AreEqual(3, questService.GetRewardAmount(3, false));
            Assert.AreEqual(40, questService.GetRewardAmount(3, true));

            // Rarity 4: 5 LP / 100 Gems
            Assert.AreEqual(5, questService.GetRewardAmount(4, false));
            Assert.AreEqual(100, questService.GetRewardAmount(4, true));
        }

        [Test]
        public void QuestService_ClaimReward_AwardsProgressToDoctrineAndCompletesQuest()
        {
            var questService = _container.Quest;
            var save = _container.Save;

            var questDef = new QuestDefinition { id = "quest_kill_10", TargetProgress = 10 };
            _database.Add(questDef);

            var qSave = new QuestSaveData
            {
                DefinitionId = "quest_kill_10",
                InstanceId = "q_inst_1",
                State = QuestState.Completed,
                Progress = 10
            };
            save.CurrentData.Quests.Add(qSave);

            // Re-load quests into service
            var freshQuestService = new QuestService(save, _database, _container.Doctrine);

            int warProgressBefore = _container.Doctrine.GetProgress("war");
            bool claimed = freshQuestService.ClaimReward("q_inst_1", "war");

            Assert.IsTrue(claimed);
            Assert.AreEqual(warProgressBefore + 1, _container.Doctrine.GetProgress("war")); // Rarity 1 default -> +1 progress
            Assert.AreEqual(1, save.CurrentData.QuestsCompleted);
        }
    }
}
