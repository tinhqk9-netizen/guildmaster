using System;
using GuildMaster.Definitions;
using GuildMaster.Runtime.Core;
using GuildMaster.Database;
using GuildMaster.Runtime.Models;

namespace GuildMaster.Runtime.Services
{
    public class ItemService : IItemService
    {
        private readonly RuntimeFactory _runtimeFactory;
        private readonly GameDatabase _registry;

        public ItemService(RuntimeFactory runtimeFactory, GameDatabase registry)
        {
            _runtimeFactory = runtimeFactory ?? throw new ArgumentNullException(nameof(runtimeFactory));
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        }

        public ItemRuntime CreateItem(string definitionId, int stackCount = 1)
        {
            if (!_registry.TryGet<ItemDefinition>(definitionId, out var definition))
            {
                throw new ArgumentException($"ItemDefinition not found for id: {definitionId}");
            }

            return _runtimeFactory.CreateItem(definition, stackCount);
        }

        public long GetItemPrice(ItemRuntime item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            return item.Definition.Price;
        }
    }
}
