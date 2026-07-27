using System.Collections.Generic;
using GuildMaster.Definitions;
using GuildMaster.Runtime.Models;

namespace GuildMaster.Runtime.Services
{
    public interface IInventoryService
    {
        int GetCapacity();
        bool CanAddItem(string definitionId);
        void AddItem(ItemRuntime item);
        bool RemoveItem(string instanceId, int amount);
        bool HasItem(string instanceId);
        ItemRuntime GetItem(string instanceId);
        IReadOnlyList<ItemRuntime> GetAllItems();
        IReadOnlyList<ItemRuntime> GetItemsByCategory(ItemCategory category);
        bool ToggleLockItem(string instanceId);
        bool UseConsumable(string instanceId, CharacterRuntime targetCharacter);
        
        // DefinitionId API Foundation
        int GetQuantityByDefinitionId(string definitionId);
        bool HasQuantityByDefinitionId(string definitionId, int amount);
        bool ConsumeByDefinitionId(string definitionId, int amount);
    }
}
