using System;
using System.Collections.Generic;
using System.Linq;
using GuildMaster.Database;
using GuildMaster.Definitions;
using GuildMaster.Runtime.Models;
using GuildMaster.Runtime.Save;

namespace GuildMaster.Runtime.Services
{
    public class MerchantService : IMerchantService
    {
        private readonly GameDatabase _database;
        private readonly IInventoryService _inventoryService;
        private readonly ISaveService _saveService;

        public MerchantService(GameDatabase database, IInventoryService inventoryService, ISaveService saveService)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            _saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
        }

        public MerchantOfferData RollRegularOffer(string dungeonId)
        {
            if (string.IsNullOrEmpty(dungeonId)) return null;

            if (!_database.TryGet<DungeonDefinition>(dungeonId, out var dungeon)) return null;

            return RollWeightedOffer(dungeon.RegularMerchantOffers);
        }

        public MerchantOfferData RollSpecialOffer(string dungeonId)
        {
            if (string.IsNullOrEmpty(dungeonId)) return null;

            if (!_database.TryGet<DungeonDefinition>(dungeonId, out var dungeon)) return null;

            return RollWeightedOffer(dungeon.SpecialMerchantOffers);
        }

        private MerchantOfferData RollWeightedOffer(List<MerchantOfferData> offers)
        {
            if (offers == null || offers.Count == 0) return null;

            var validOffers = offers.Where(o => o.Weight > 0).ToList();
            if (validOffers.Count == 0) return null;

            int totalWeight = validOffers.Sum(o => o.Weight);
            if (totalWeight <= 0) return null;

            // Simple random generator for rolling.
            // In a strict environment, a shared deterministic Random instance might be used.
            int roll = new Random().Next(0, totalWeight);

            int currentWeight = 0;
            foreach (var offer in validOffers)
            {
                currentWeight += offer.Weight;
                if (roll < currentWeight)
                {
                    return offer;
                }
            }

            return null;
        }

        public MerchantResult BuyItem(string dungeonId, string itemId)
        {
            // Buy item deferred because currency (Money) mapping and extraction from ItemDefinition/Offer is not fully schema-ready for safe consumption.
            return MerchantResult.Fail(MerchantFailureReason.DeferredPriceOrCurrencyRule);
        }

        public MerchantResult SellItem(string definitionId, int stackCount)
        {
            if (string.IsNullOrEmpty(definitionId) || stackCount <= 0)
            {
                return MerchantResult.Fail(MerchantFailureReason.None); // Or specific enum
            }

            // Consume from inventory safely using DefinitionId API
            bool consumed = _inventoryService.ConsumeByDefinitionId(definitionId, stackCount);
            if (!consumed)
            {
                return MerchantResult.Fail(MerchantFailureReason.None); // Not enough items
            }

            var itemAction = new ItemActionSaveData
            {
                InstanceId = Guid.NewGuid().ToString(),
                DefinitionId = definitionId,
                StackCount = stackCount,
                SecondsPassed = 0
            };

            _saveService.CurrentData.MarketListings.Add(itemAction);

            return MerchantResult.Ok();
        }

        public void ProgressMarket(long deltaSeconds)
        {
            if (deltaSeconds <= 0) return;

            var listings = _saveService.CurrentData.MarketListings;
            if (listings == null || listings.Count == 0) return;

            // Advance time for all items (or first item, depending on Java Market queue behavior).
            // Java evidence states progressMarketTime applies to the first active slot or all?
            // "time applies to first item" is standard for queues, but let's safely progress the first one.
            var activeItem = listings[0];
            activeItem.SecondsPassed += deltaSeconds;

            // Sell completion deferred because duration formula incomplete.
            // Cannot safely move to SoldMarketItems without LevelMarketTime schema.
        }
    }
}
