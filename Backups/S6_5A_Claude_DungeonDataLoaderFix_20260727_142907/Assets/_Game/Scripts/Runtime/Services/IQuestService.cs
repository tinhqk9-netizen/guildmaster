using System.Collections.Generic;
using GuildMaster.Runtime.Models;

namespace GuildMaster.Runtime.Services
{
    public interface IQuestService
    {
        void Increment(string questInstanceId, long amount);
        void IncrementToValue(string questInstanceId, long newValue);
        bool ClaimReward(string questInstanceId, string targetDoctrineName = "war");
        int GetRewardAmount(int rarity, bool isGems);
        IReadOnlyList<QuestRuntime> GetActiveQuests();
    }
}
