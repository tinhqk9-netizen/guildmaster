using System.Collections.Generic;
using GuildMaster.Definitions;
using GuildMaster.Runtime.Models;

namespace GuildMaster.Runtime.Services
{
    public interface ICharacterService
    {
        CharacterRuntime CreateCharacter(string definitionId);
        int GetTotalStat(CharacterRuntime character, StatType statType);

        /// <summary>Phase 2A: percentage-scale combat modifiers (Counterattack, CriticalChance,
        /// CriticalDamage, FlatDodgeChance, HealingModifier, ImmunityToStatus). See
        /// CharacterService.GetCombatModifier for the Java source mapping.</summary>
        double GetCombatModifier(CharacterRuntime character, StatType statType);
        void GainExperience(CharacterRuntime character, int exp);
        bool LevelUp(CharacterRuntime character);
        IReadOnlyList<CharacterRuntime> GetAllCharacters();
        CharacterRuntime RecruitCharacter(GuildMaster.Runtime.Save.CharacterSaveData saveData);
        
        bool CanDismissCharacter(string instanceId, out string reason);
        bool DismissCharacter(string instanceId, out string errorReason);

        /// <summary>Real class change (promotion/ascension). See CharacterService.ChangeClass.</summary>
        bool ChangeClass(string instanceId, string newDefinitionId, bool setAscended);
    }
}
