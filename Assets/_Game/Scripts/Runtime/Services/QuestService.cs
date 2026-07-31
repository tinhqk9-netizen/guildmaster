using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GuildMaster.Database;
using GuildMaster.Definitions;
using GuildMaster.Runtime.Models;
using GuildMaster.Runtime.Save;

namespace GuildMaster.Runtime.Services
{
    [Serializable]
    public class QuestFlatMetadataEntry
    {
        public string id;
        public string className;
        public int defaultRarity;
        public List<long> targetProgressValues;
    }

    [Serializable]
    public class QuestFlatMetadataList
    {
        public List<QuestFlatMetadataEntry> entries;
    }

    public class QuestService : IQuestService
    {
        private readonly ISaveService _saveService;
        private readonly GameDatabase _registry;
        private readonly IDoctrineService _doctrineService;
        private readonly List<QuestRuntime> _activeQuests = new List<QuestRuntime>();
        private readonly Dictionary<string, QuestFlatMetadataEntry> _metadataMap = new Dictionary<string, QuestFlatMetadataEntry>(StringComparer.OrdinalIgnoreCase);

        public QuestService(
            ISaveService saveService, 
            GameDatabase registry, 
            IDoctrineService doctrineService = null)
        {
            _saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _doctrineService = doctrineService ?? new DoctrineService(_saveService, new Formulas.FormulaService());
            
            LoadMetadata();
            LoadQuests();
        }

        private void LoadMetadata()
        {
            _metadataMap.Clear();
            try
            {
                string path = Path.Combine(UnityEngine.Application.streamingAssetsPath, "GameData", "quest_metadata.json");
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    var list = UnityEngine.JsonUtility.FromJson<QuestFlatMetadataList>(json);
                    if (list != null && list.entries != null)
                    {
                        foreach (var entry in list.entries)
                        {
                            if (!string.IsNullOrEmpty(entry.id))
                            {
                                _metadataMap[entry.id] = entry;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[QuestService] Failed to load quest metadata: {ex.Message}");
            }
        }

        public long GetTargetProgress(string questId, int rarity)
        {
            if (_metadataMap.TryGetValue(questId, out var entry))
            {
                int idx = Math.Clamp(rarity - 1, 0, 9);
                if (entry.targetProgressValues != null && entry.targetProgressValues.Count > idx)
                {
                    return entry.targetProgressValues[idx];
                }
            }
            return rarity * 100;
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
                if (_registry.TryGet<QuestDefinition>(qData.DefinitionId, out var def))
                {
                    int rarity = qData.Rarity > 0 ? qData.Rarity : 1;
                    long targetProg = qData.TargetProgress > 0 ? qData.TargetProgress : GetTargetProgress(qData.DefinitionId, rarity);
                    var runtime = new QuestRuntime(qData.InstanceId, def, rarity, targetProg)
                    {
                        State = qData.State,
                        Progress = qData.Progress
                    };
                    _activeQuests.Add(runtime);
                }
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
                Progress = q.Progress,
                Rarity = q.Rarity,
                TargetProgress = q.TargetProgress
            }).ToList();
        }

        public void Increment(string questInstanceId, long amount)
        {
            var quest = _activeQuests.FirstOrDefault(q => q.InstanceId == questInstanceId);
            if (quest == null || !quest.IsActive || amount <= 0 || quest.Progress >= quest.TargetProgress) return;

            long newProgress = quest.Progress + amount;
            quest.Progress = newProgress;
            
            if (newProgress >= quest.TargetProgress)
            {
                quest.State = QuestState.Completed;
            }

            quest.IsDirty = true;
            SaveQuests();
        }

        public void IncrementToValue(string questInstanceId, long newValue)
        {
            var quest = _activeQuests.FirstOrDefault(q => q.InstanceId == questInstanceId);
            if (quest == null || !quest.IsActive || newValue <= 0 || quest.Progress >= quest.TargetProgress) return;

            quest.Progress = newValue;
            
            if (newValue >= quest.TargetProgress)
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

            int rarity = quest.Rarity;
            bool isGems = (rarity >= 4);

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

        public void IncrementDefinition(string definitionId, long amount)
        {
            if (string.IsNullOrEmpty(definitionId) || amount <= 0) return;
            var targets = _activeQuests.Where(q => q.Definition.id.Equals(definitionId, StringComparison.OrdinalIgnoreCase)).ToList();
            foreach (var q in targets)
            {
                Increment(q.InstanceId, amount);
            }
        }
    }
}
