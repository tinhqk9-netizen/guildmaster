using System.Collections.Generic;
using GuildMaster.Runtime.Models;
using GuildMaster.Runtime.Save;

namespace GuildMaster.Runtime.Services
{
    public interface ITavernService
    {
        int GetTavernCapacity();
        int GetQuartersCapacity();
        long GetVisitorIntervalSeconds();
        long GetNextVisitorTimerSeconds();
        IReadOnlyList<CharacterSaveData> GetGuests();
        bool CanRecruit();
        bool RecruitGuest(int index, out CharacterRuntime newCharacter);
        void ProgressVisitorTime(long deltaSeconds);
        void GenerateVisitor();

        /// <summary>
        /// Creates the production first-run Footman through the normal visitor/recruitment
        /// pipeline. This is called only by new-save initialization, not normal Tavern refresh.
        /// </summary>
        bool CreateInitialStartingHero(out CharacterRuntime newCharacter);

        /// <summary>
        /// Creates the production first-run visitor from the normal class pool excluding
        /// Footman. Subsequent visitors use GenerateVisitor().
        /// </summary>
        void GenerateInitialVisitor();

        bool UpgradeQuarters();
        bool UpgradeTavernCapacity();
        bool UpgradeTavernTime();

        // UI Helpers
        long GetUpgradeQuartersPrice();
        long GetUpgradeTavernCapacityPrice();
        long GetUpgradeTavernTimePrice();
        int GetQuartersLevel();
        int GetTavernCapacityLevel();
        int GetTavernTimeLevel();
    }
}
