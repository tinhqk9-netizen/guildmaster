using GuildMaster.Definitions;
using GuildMaster.Runtime.Models;

namespace GuildMaster.Runtime.Services
{
    public interface IItemService
    {
        ItemRuntime CreateItem(string definitionId, int stackCount = 1);
        long GetItemPrice(ItemRuntime item);
    }
}
