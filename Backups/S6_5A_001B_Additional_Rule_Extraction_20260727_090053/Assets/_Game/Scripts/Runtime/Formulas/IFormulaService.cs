using System;
using UnityEngine;

namespace GuildMaster.Runtime.Formulas
{
    public interface IFormulaService
    {
        // Ported logic
        int ExperienceToNextLevel(int currentLevel, bool isAdventurer);
        int FoodToNextLevel(int currentLevel);
        
        long GetQuartersPrice(int level);
        long GetTavernCapacityPrice(int level);
        long GetStorageCapacityPrice(int level);
        int GetStorageSpaces(int levelStorage, int upgradeStorage, int additionalBonus = 0);

        // Missing formulas that need manual porting
        void CalculateDamage_ManualPortRequired();
    }
}
