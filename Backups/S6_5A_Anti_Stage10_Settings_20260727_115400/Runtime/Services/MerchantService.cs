using System;
using System.Collections.Generic;
using System.Linq;
using GuildMaster.Database;
using GuildMaster.Definitions;
using GuildMaster.Runtime.Formulas;
using GuildMaster.Runtime.Models;
using GuildMaster.Runtime.Save;

namespace GuildMaster.Runtime.Services
{
    public class MerchantService : IMerchantService
    {
        private readonly GameDatabase _database;
        private readonly IInventoryService _inventoryService;
        private readonly ISaveService _saveService;

        private const int DEFAULT_SELL_TIME_SECONDS = 20;

        public MerchantService(GameDatabase database, IInventoryService inventoryService, ISaveService saveService)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            _saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
        }

        public IReadOnlyList<MerchantOfferSaveData> GetRegularStock()
        {
            return _saveService.CurrentData.MerchantRegularStockItems.AsReadOnly();
        }

        public IReadOnlyList<MerchantOfferSaveData> GetSpecialStock()
        {
            return _saveService.CurrentData.MerchantSpecialReserve.AsReadOnly();
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

        public bool BuyOffer(MerchantOfferSaveData offer, bool isSpecial)
        {
            if (offer == null || string.IsNullOrEmpty(offer.DefinitionId)) return false;

            var data = _saveService.CurrentData;

            // 1. Inventory capacity check
            if (!_inventoryService.CanAddItem(offer.DefinitionId))
            {
                return false;
            }

            // 2. Currency check based on offer.IsGems (Recovered Rule #1)
            if (!offer.IsGems)
            {
                if (data.Money < offer.Price) return false;
            }
            else
            {
                if (data.Gems < offer.Price) return false;
            }

            // 3. Deduct currency
            if (!offer.IsGems) data.Money -= offer.Price;
            else data.Gems -= offer.Price;

            // 4. Remove offer from stock
            if (!isSpecial) data.MerchantRegularStockItems.Remove(offer);
            else data.MerchantSpecialReserve.Remove(offer);

            // 5. Grant item to inventory
            if (_database.TryGet<ItemDefinition>(offer.DefinitionId, out var itemDef))
            {
                var itemRuntime = new ItemRuntime(Guid.NewGuid().ToString(), itemDef, offer.StackCount);
                _inventoryService.AddItem(itemRuntime);
            }

            return true;
        }

        public MerchantResult BuyItem(string dungeonId, string itemId)
        {
            return MerchantResult.Fail(MerchantFailureReason.DeferredPriceOrCurrencyRule);
        }

        public MerchantResult SellItem(string definitionId, int stackCount)
        {
            if (string.IsNullOrEmpty(definitionId) || stackCount <= 0)
            {
                return MerchantResult.Fail(MerchantFailureReason.None);
            }

            bool consumed = _inventoryService.ConsumeByDefinitionId(definitionId, stackCount);
            if (!consumed)
            {
                return MerchantResult.Fail(MerchantFailureReason.None);
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

            var activeItem = listings[0];
            activeItem.SecondsPassed += deltaSeconds;

            if (activeItem.SecondsPassed >= DEFAULT_SELL_TIME_SECONDS)
            {
                listings.RemoveAt(0);
                _saveService.CurrentData.SoldMarketItems.Add(activeItem);
            }
        }

        public bool ClaimSoldItem(string instanceId)
        {
            if (string.IsNullOrEmpty(instanceId)) return false;

            var sold = _saveService.CurrentData.SoldMarketItems;
            var item = sold.FirstOrDefault(x => x.InstanceId == instanceId);
            if (item == null) return false;

            long itemPrice = 100; // Base default price
            if (_database.TryGet<ItemDefinition>(item.DefinitionId, out var itemDef))
            {
                itemPrice = itemDef.SellPrice > 0 ? itemDef.SellPrice : 100;
            }

            long totalEarned = DecodeMath.TruncatePrice(itemPrice * item.StackCount);
            _saveService.CurrentData.Money += totalEarned;

            sold.Remove(item);
            return true;
        }
    }
}
