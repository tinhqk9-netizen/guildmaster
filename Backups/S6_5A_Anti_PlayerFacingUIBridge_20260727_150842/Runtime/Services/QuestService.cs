using System;
using System.Collections.Generic;
using System.Linq;
using GuildMaster.Database;
using GuildMaster.Definitions;
using GuildMaster.Runtime.Models;
using GuildMaster.Runtime.Save;

namespace GuildMaster.Runtime.Services
{
    public class QuestService : IQuestService
    {
        private readonly ISaveService _saveService;
        private readonly GameDatabase _registry;
        private readonly IDoctrineService _doctrineService;
        private readonly List<QuestRuntime> _activeQuests = new List<QuestRuntime>();

        public QuestService(
            ISaveService saveService, 
            GameDatabase registry, 
            IDoctrineService doctrineService = null)
        {
            _saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _doctrineService = doctrineService ?? new DoctrineService(_saveService, new Formulas.FormulaService());
            LoadQuests();
        }

        public IReadOnlyList<QuestRuntime> GetActiveQuests()
        {
            return _activeQuests.AsReadOnly();
        }

        private void LoadQuests()
        {
            _activeQuests.Clear();
            if (_saveService.CurrentData == null) return;

            foreach (var qData in _saveService.CurrentData.Quests)
            {
                var def = _registry.GetRequired<QuestDefinition>(qData.DefinitionId);
                var runtime = new QuestRuntime(qData.InstanceId, def)
                {
                    State = qData.State,
                    Progress = qData.Progress
                };
                _activeQuests.Add(runtime);
            }
        }

        private void SaveQuests()
        {
            if (_saveService.CurrentData == null) return;
            
            _saveService.CurrentData.Quests = _activeQuests.Select(q => new QuestSaveData
            {
                InstanceId = q.InstanceId,
                DefinitionId = q.Definition.id,
                State = q.State,
                Progress = q.Progress
            }).ToList();
        }

        public void Increment(string questInstanceId, long amount)
        {
            var quest = _activeQuests.FirstOrDefault(q => q.InstanceId == questInstanceId);
            if (quest == null || !quest.IsActive || amount <= 0 || quest.Progress >= quest.Definition.TargetProgress) return;

            long newProgress = quest.Progress + amount;
            quest.Progress = newProgress;
            
            if (newProgress >= quest.Definition.TargetProgress)
            {
                quest.State = QuestState.Completed;
            }

            quest.IsDirty = true;
            SaveQuests();
        }

        public void IncrementToValue(string questInstanceId, long newValue)
        {
            var quest = _activeQuests.FirstOrDefault(q => q.InstanceId == questInstanceId);
            if (quest == null || !quest.IsActive || newValue <= 0 || quest.Progress >= quest.Definition.TargetProgress) return;

            quest.Progress = newValue;
            
            if (newValue >= quest.Definition.TargetProgress)
            {
                quest.State = QuestState.Completed;
            }

            quest.IsDirty = true;
            SaveQuests();
        }

        public int GetRewardAmount(int rarity, bool isGems)
        {
            // Recovered Rule #2: rewardFromRarity
            switch (rarity)
            {
                case 1: return isGems ? 10 : 1;
                case 2: return isGems ? 20 : 2;
                case 3: return isGems ? 40 : 3;
                case 4: return isGems ? 100 : 5;
                default: return isGems ? 10 : 1;
            }
        }

        public bool ClaimReward(string questInstanceId, string targetDoctrineName = "war")
        {
            if (string.IsNullOrEmpty(questInstanceId)) return false;

            var quest = _activeQuests.FirstOrDefault(q => q.InstanceId == questInstanceId);
            if (quest == null || quest.State != QuestState.Completed) return false;

            int rarity = 1; // Default quest rarity
            bool isGems = false;

            int rewardAmount = GetRewardAmount(rarity, isGems);

            var data = _saveService.CurrentData;
            if (isGems)
            {
                data.Gems += rewardAmount;
            }
            else
            {
                _doctrineService.AddProgress(targetDoctrineName, rewardAmount);
            }

            _activeQuests.Remove(quest);
            data.QuestsCompleted++;
            SaveQuests();
            return true;
        }
    }
}
