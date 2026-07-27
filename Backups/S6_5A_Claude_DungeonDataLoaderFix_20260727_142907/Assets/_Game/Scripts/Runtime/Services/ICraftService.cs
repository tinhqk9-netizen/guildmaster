using System.Collections.Generic;
using GuildMaster.Runtime.Models;
using GuildMaster.Runtime.Save;

namespace GuildMaster.Runtime.Services
{
    public interface ICraftService
    {
        CraftResult CanCraft(string recipeId);
        CraftResult TryStartCraft(string recipeId);
        void ProgressWorkshop(long deltaSeconds);
        int GetMaxCraftable(string recipeId);
        bool ClaimCompletedCraft(string instanceId);
        int GetQueueCapacity();
        IReadOnlyList<ItemActionSaveData> GetQueue();
        IReadOnlyList<ItemActionSaveData> GetCompletedItems();
    }
}
