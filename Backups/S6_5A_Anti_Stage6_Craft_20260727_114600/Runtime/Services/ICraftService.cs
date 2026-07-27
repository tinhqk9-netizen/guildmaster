using GuildMaster.Runtime.Models;

namespace GuildMaster.Runtime.Services
{
    public interface ICraftService
    {
        CraftResult CanCraft(string recipeId);
        CraftResult TryStartCraft(string recipeId);
        void ProgressWorkshop(long deltaSeconds);
    }
}
