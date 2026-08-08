using System;
using System.Linq;
using GuildMaster.Database;
using GuildMaster.Definitions;
using GuildMaster.Runtime.Formulas;
using GuildMaster.Runtime.Save;

namespace GuildMaster.Runtime.Services
{
    public class DoctrineService : IDoctrineService
    {
        private readonly ISaveService _saveService;
        private readonly IFormulaService _formulaService;
        private readonly GameDatabase _database;

        public DoctrineService(ISaveService saveService, IFormulaService formulaService, GameDatabase database = null)
        {
            _saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
            _formulaService = formulaService ?? throw new ArgumentNullException(nameof(formulaService));
            _database = database;
        }

        public int GetLevel(string doctrineName)
        {
            if (string.IsNullOrEmpty(doctrineName)) return 0;
            var data = _saveService.CurrentData;
            switch (doctrineName.ToLowerInvariant())
            {
                case "affliction": return data.AfflictionLevel;
                case "control": return data.ControlLevel;
                case "fortitude": return data.FortitudeLevel;
                case "grace": return data.GraceLevel;
                case "illusion": return data.IllusionLevel;
                case "knowledge": return data.KnowledgeLevel;
                case "ruin": return data.RuinLevel;
                case "war": return data.WarLevel;
                default: return 0;
            }
        }

        public int GetProgress(string doctrineName)
        {
            if (string.IsNullOrEmpty(doctrineName)) return 0;
            var data = _saveService.CurrentData;
            switch (doctrineName.ToLowerInvariant())
            {
                case "affliction": return data.AfflictionProgress;
                case "control": return data.ControlProgress;
                case "fortitude": return data.FortitudeProgress;
                case "grace": return data.GraceProgress;
                case "illusion": return data.IllusionProgress;
                case "knowledge": return data.KnowledgeProgress;
                case "ruin": return data.RuinProgress;
                case "war": return data.WarProgress;
                default: return 0;
            }
        }

        public void AddProgress(string doctrineName, int amount)
        {
            if (string.IsNullOrEmpty(doctrineName) || amount <= 0) return;
            var data = _saveService.CurrentData;
            int level = GetLevel(doctrineName);
            int progress = GetProgress(doctrineName);

            int newLevel = level;
            int currentProgress = progress;
            int remainingAmount = amount;

            while (remainingAmount > 0)
            {
                int starsNeeded = _formulaService.TotalStarsToNextLp(newLevel);
                if (starsNeeded <= 0)
                {
                    break;
                }

                int neededToNext = starsNeeded - currentProgress;
                if (neededToNext <= 0)
                {
                    newLevel++;
                    currentProgress = 0;
                    continue;
                }

                if (neededToNext > remainingAmount)
                {
                    currentProgress += remainingAmount;
                    remainingAmount = 0;
                }
                else
                {
                    newLevel++;
                    remainingAmount -= neededToNext;
                    currentProgress = 0;
                }
            }

            int newProgress = currentProgress;

            switch (doctrineName.ToLowerInvariant())
            {
                case "affliction": data.AfflictionLevel = newLevel; data.AfflictionProgress = newProgress; break;
                case "control": data.ControlLevel = newLevel; data.ControlProgress = newProgress; break;
                case "fortitude": data.FortitudeLevel = newLevel; data.FortitudeProgress = newProgress; break;
                case "grace": data.GraceLevel = newLevel; data.GraceProgress = newProgress; break;
                case "illusion": data.IllusionLevel = newLevel; data.IllusionProgress = newProgress; break;
                case "knowledge": data.KnowledgeLevel = newLevel; data.KnowledgeProgress = newProgress; break;
                case "ruin": data.RuinLevel = newLevel; data.RuinProgress = newProgress; break;
                case "war": data.WarLevel = newLevel; data.WarProgress = newProgress; break;
            }
        }

        public bool IsMaxed()
        {
            return _saveService.CurrentData.DoctrineMaxed;
        }

        // ---------------------------------------------------------------------------------
        // Phase 2A: per-node progression (Java: Doctrine.l1..l6).
        // ---------------------------------------------------------------------------------

        private DoctrineNodeDefinition FindNodeDef(string doctrineId, string nodeId)
        {
            if (_database == null || string.IsNullOrEmpty(doctrineId) || string.IsNullOrEmpty(nodeId)) return null;
            if (!_database.TryGet<DoctrineDefinition>(doctrineId.ToLowerInvariant(), out var doctrineDef)) return null;
            return doctrineDef.Nodes?.FirstOrDefault(n => n.NodeId == nodeId);
        }

        private DoctrineNodeSaveData FindOrCreateNodeSave(string doctrineId, string nodeId)
        {
            var data = _saveService.CurrentData;
            if (data.DoctrineNodes == null) data.DoctrineNodes = new System.Collections.Generic.List<DoctrineNodeSaveData>();

            var node = data.DoctrineNodes.FirstOrDefault(n =>
                string.Equals(n.DoctrineId, doctrineId, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(n.NodeId, nodeId, StringComparison.OrdinalIgnoreCase));

            if (node == null)
            {
                node = new DoctrineNodeSaveData { DoctrineId = doctrineId, NodeId = nodeId, Level = 0 };
                data.DoctrineNodes.Add(node);
            }
            return node;
        }

        public int GetNodeLevel(string doctrineId, string nodeId)
        {
            if (string.IsNullOrEmpty(doctrineId) || string.IsNullOrEmpty(nodeId)) return 0;
            var data = _saveService.CurrentData;
            if (data.DoctrineNodes == null) return 0;

            var node = data.DoctrineNodes.FirstOrDefault(n =>
                string.Equals(n.DoctrineId, doctrineId, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(n.NodeId, nodeId, StringComparison.OrdinalIgnoreCase));
            return node?.Level ?? 0;
        }

        public bool CanUpgradeNode(string doctrineId, string nodeId)
        {
            var def = FindNodeDef(doctrineId, nodeId);
            if (def == null) return false;
            int current = GetNodeLevel(doctrineId, nodeId);
            return current < def.MaxLevel;
        }

        public bool UpgradeNode(string doctrineId, string nodeId)
        {
            if (!CanUpgradeNode(doctrineId, nodeId)) return false;
            var node = FindOrCreateNodeSave(doctrineId, nodeId);
            node.Level++;
            return true;
        }

        public int GetNodeEffectValue(string doctrineId, string nodeId)
        {
            var def = FindNodeDef(doctrineId, nodeId);
            if (def == null) return 0;
            int level = GetNodeLevel(doctrineId, nodeId);
            return (int)(level * def.IncreasePerLevel);
        }

        public int GetAggregateAbilityValue(string abilityTypeId)
        {
            if (_database == null || string.IsNullOrEmpty(abilityTypeId)) return 0;
            int total = 0;
            foreach (var doctrineDef in _database.GetAll<DoctrineDefinition>())
            {
                if (doctrineDef.Nodes == null) continue;
                foreach (var node in doctrineDef.Nodes)
                {
                    if (node.AbilityType == abilityTypeId)
                    {
                        total += GetNodeEffectValue(doctrineDef.id, node.NodeId);
                    }
                }
            }
            return total;
        }
    }
}
