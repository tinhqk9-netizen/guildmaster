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
                case "affliction": return data.LevelAffliction;
                case "control": return data.LevelControl;
                case "fortitude": return data.LevelFortitude;
                case "grace": return data.LevelGrace;
                case "illusion": return data.LevelIllusion;
                case "knowledge": return data.LevelKnowledge;
                case "ruin": return data.LevelRuin;
                case "war": return data.LevelWar;
                default: return 0;
            }
        }

        public int GetProgress(string doctrineName)
        {
            if (string.IsNullOrEmpty(doctrineName)) return 0;
            var data = _saveService.CurrentData;
            switch (doctrineName.ToLowerInvariant())
            {
                case "affliction": return data.ProgressAffliction;
                case "control": return data.ProgressControl;
                case "fortitude": return data.ProgressFortitude;
                case "grace": return data.ProgressGrace;
                case "illusion": return data.ProgressIllusion;
                case "knowledge": return data.ProgressKnowledge;
                case "ruin": return data.ProgressRuin;
                case "war": return data.ProgressWar;
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
                case "affliction": data.LevelAffliction = newLevel; data.ProgressAffliction = newProgress; break;
                case "control": data.LevelControl = newLevel; data.ProgressControl = newProgress; break;
                case "fortitude": data.LevelFortitude = newLevel; data.ProgressFortitude = newProgress; break;
                case "grace": data.LevelGrace = newLevel; data.ProgressGrace = newProgress; break;
                case "illusion": data.LevelIllusion = newLevel; data.ProgressIllusion = newProgress; break;
                case "knowledge": data.LevelKnowledge = newLevel; data.ProgressKnowledge = newProgress; break;
                case "ruin": data.LevelRuin = newLevel; data.ProgressRuin = newProgress; break;
                case "war": data.LevelWar = newLevel; data.ProgressWar = newProgress; break;
            }
        }

        public bool IsMaxed()
        {
            return _saveService.CurrentData.DoctrineMaxed;
        }
    }
}
