using System.Collections.Generic;
using GuildMaster.Runtime.Models;

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
        void Tick();
        DungeonRuntime GetActiveDungeon();
    }
}
