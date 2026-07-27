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
        bool UpgradeQuarters();
        bool UpgradeTavernCapacity();
        bool UpgradeTavernTime();
    }
}
