using System;
using GuildMaster.Runtime.Formulas;
using GuildMaster.Runtime.Save;

namespace GuildMaster.Runtime.Services
{
    public class DoctrineService : IDoctrineService
    {
        private readonly ISaveService _saveService;
        private readonly IFormulaService _formulaService;

        public DoctrineService(ISaveService saveService, IFormulaService formulaService)
        {
            _saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
            _formulaService = formulaService ?? throw new ArgumentNullException(nameof(formulaService));
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
            int needed = _formulaService.TotalStarsToNextLp(level) - progress;

            int newLevel = level;
            int newProgress = progress;

            if (needed > amount)
            {
                newProgress = progress + amount;
            }
            else
            {
                newLevel = level + 1;
                newProgress = amount - needed;
            }

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
    }
}
