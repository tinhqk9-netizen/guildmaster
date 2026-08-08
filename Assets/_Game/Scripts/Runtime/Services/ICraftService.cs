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
        long GetCraftDurationSeconds(ItemActionSaveData item);
        bool CancelCraft(string instanceId);
        int GetMaxCraftable(string recipeId);
        bool ClaimCompletedCraft(string instanceId);
        int GetQueueCapacity();
        IReadOnlyList<ItemActionSaveData> GetQueue();
        IReadOnlyList<ItemActionSaveData> GetCompletedItems();

        // UI Upgrades
        bool UpgradeQueueCapacity();
        long GetUpgradeQueueCapacityPrice();
        int GetQueueCapacityLevel();
        bool UpgradeCraftSpeed();
        long GetUpgradeCraftSpeedPrice();
        int GetCraftSpeedLevel();
    }
}
