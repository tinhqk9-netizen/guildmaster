using System.Collections.Generic;

namespace GuildMaster.Runtime.Services
{
    public interface IDungeonService
    {
        void StartDungeon(string dungeonId, List<string> adventurerIds);
        void StopDungeon();
        void SaveDungeonState();
        void LoadDungeonState();
        bool IsDungeonActive();
        void AdvanceProgressOneStep();
    }
}
