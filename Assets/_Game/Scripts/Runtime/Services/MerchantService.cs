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
        private readonly IFormulaService _formulaService;
        private readonly IQuestService _questService;
        private readonly Random _random = new Random();

        private const long DaySeconds = 24 * 60 * 60;
        private const long WeekSeconds = 7 * DaySeconds;
        private const int MaxMarketListingsLevel = 10;
        private const int MaxMarketTimeLevel = 25;

        public MerchantService(
            GameDatabase database,
            IInventoryService inventoryService,
            ISaveService saveService,
            IFormulaService formulaService = null,
            IQuestService questService = null)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            _saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
            _formulaService = formulaService ?? new FormulaService();
            _questService = questService;
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

            int roll = _random.Next(0, totalWeight);

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
            if (string.IsNullOrEmpty(itemId))
                return MerchantResult.Fail(MerchantFailureReason.InvalidItem);

            var regular = _saveService.CurrentData.MerchantRegularStockItems
                .FirstOrDefault(offer => offer.DefinitionId == itemId);
            if (regular != null)
                return BuyOffer(regular, false)
                    ? MerchantResult.Ok()
                    : MerchantResult.Fail(MerchantFailureReason.DeferredPriceOrCurrencyRule);

            var special = _saveService.CurrentData.MerchantSpecialReserve
                .FirstOrDefault(offer => offer.DefinitionId == itemId);
            if (special != null)
                return BuyOffer(special, true)
                    ? MerchantResult.Ok()
                    : MerchantResult.Fail(MerchantFailureReason.DeferredPriceOrCurrencyRule);

            return MerchantResult.Fail(MerchantFailureReason.NoOffersAvailable);
        }

        public MerchantResult SellItem(string definitionId, int stackCount)
        {
            if (string.IsNullOrEmpty(definitionId) || stackCount <= 0)
            {
                return MerchantResult.Fail(MerchantFailureReason.InvalidItem);
            }

            var data = _saveService.CurrentData;
            if (data.MarketListings.Count >= GetMarketListingsCapacity())
            {
                return MerchantResult.Fail(MerchantFailureReason.MarketFull);
            }

            var matchingItems = _inventoryService.GetAllItems()
                .Where(item => item.Definition != null && item.Definition.id == definitionId)
                .ToList();
            if (matchingItems.Count == 0) return MerchantResult.Fail(MerchantFailureReason.InvalidItem);
            if (matchingItems.Any(item => item.Definition.NotSellable))
                return MerchantResult.Fail(MerchantFailureReason.NotSellable);
            if (!matchingItems.Any(item => !item.IsLocked) ||
                matchingItems.Where(item => !item.IsLocked).Sum(item => item.StackCount) < stackCount)
            {
                return matchingItems.All(item => item.IsLocked)
                    ? MerchantResult.Fail(MerchantFailureReason.ItemLocked)
                    : MerchantResult.Fail(MerchantFailureReason.InvalidItem);
            }

            bool consumed = _inventoryService.ConsumeByDefinitionId(definitionId, stackCount);
            if (!consumed)
            {
                return MerchantResult.Fail(MerchantFailureReason.InvalidItem);
            }

            var itemAction = new ItemActionSaveData
            {
                InstanceId = Guid.NewGuid().ToString(),
                DefinitionId = definitionId,
                StackCount = stackCount,
                SecondsPassed = 0
            };

            data.MarketListings.Add(itemAction);
            // Legacy DialogItemDetail increments Paleontologist immediately when
            // the player sells a stack, before the market timer completes.
            _questService?.IncrementDefinition("paleontologist", stackCount);

            return MerchantResult.Ok();
        }

        public void ProgressMarket(long deltaSeconds)
        {
            if (deltaSeconds <= 0) return;

            var listings = _saveService.CurrentData.MarketListings;
            if (listings == null || listings.Count == 0) return;

            long remaining = deltaSeconds;
            while (remaining > 0 && listings.Count > 0)
            {
                var activeItem = listings[0];
                long duration = GetSellDurationSeconds(activeItem);
                long required = Math.Max(1L, (duration + 1L) - activeItem.SecondsPassed);
                long applied = Math.Min(remaining, required);
                activeItem.SecondsPassed += applied;
                remaining -= applied;

                if (activeItem.SecondsPassed > duration)
                {
                    listings.RemoveAt(0);
                    _saveService.CurrentData.SoldMarketItems.Add(activeItem);
                }
            }
        }

        public long GetSellDurationSeconds(ItemActionSaveData item)
        {
            if (item == null || string.IsNullOrEmpty(item.DefinitionId)) return 0;
            if (!_database.TryGet<ItemDefinition>(item.DefinitionId, out var itemDef)) return 0;

            return _formulaService.GetSecondsToSell(
                itemDef.Price,
                Math.Max(1, item.StackCount),
                _saveService.CurrentData.LevelMarketTime,
                _saveService.CurrentData.UpgradeMarketTime,
                _saveService.CurrentData.GetPurchaseFlags());
        }

        public bool ClaimSoldItem(string instanceId)
        {
            if (string.IsNullOrEmpty(instanceId)) return false;

            var sold = _saveService.CurrentData.SoldMarketItems;
            var item = sold.FirstOrDefault(x => x.InstanceId == instanceId);
            if (item == null) return false;

            if (!_database.TryGet<ItemDefinition>(item.DefinitionId, out var itemDef)) return false;
            long itemPrice = DecodeMath.TruncatePrice(itemDef.Price);

            long totalEarned = DecodeMath.TruncatePrice(itemPrice * item.StackCount);
            _saveService.CurrentData.Money += totalEarned;

            sold.Remove(item);
            return true;
        }

        public bool CancelListing(string instanceId)
        {
            if (string.IsNullOrEmpty(instanceId)) return false;

            var data = _saveService.CurrentData;
            var listing = data.MarketListings.FirstOrDefault(x => x.InstanceId == instanceId);
            if (listing == null) return false;
            if (!_database.TryGet<ItemDefinition>(listing.DefinitionId, out var itemDef)) return false;

            _inventoryService.AddItem(new ItemRuntime(
                Guid.NewGuid().ToString(), itemDef, Math.Max(1, listing.StackCount)));
            data.MarketListings.Remove(listing);
            _saveService.Save(out _);
            return true;
        }

        public void ProcessScheduledRefreshes(long currentUnix)
        {
            if (currentUnix <= 0) return;
            var data = _saveService.CurrentData;

            if (data.LastWeekTriggered <= 0 || currentUnix - data.LastWeekTriggered > WeekSeconds)
            {
                RefreshSpecialStock();
                data.LastWeekTriggered = currentUnix;
            }

            if (data.Last24Triggered <= 0 || currentUnix - data.Last24Triggered > DaySeconds)
            {
                RefreshRegularStock();
                data.Last24Triggered = currentUnix;
            }
        }

        private void RefreshRegularStock()
        {
            var data = _saveService.CurrentData;
            data.MerchantRegularStockItems.Clear();

            var unlocked = GetUnlockedDungeons();
            foreach (var dungeon in unlocked.Skip(Math.Max(0, unlocked.Count - 4)))
            {
                var offer = RollWeightedOffer(dungeon.RegularMerchantOffers);
                AddRegularOffer(data, offer);
            }

            data.NewMerchantRegularItems = true;
        }

        private void RefreshSpecialStock()
        {
            var data = _saveService.CurrentData;
            data.MerchantSpecialReserve.Clear();

            var unlocked = GetUnlockedDungeons();
            if (unlocked.Count > 0)
            {
                var offer = RollWeightedOffer(unlocked[unlocked.Count - 1].SpecialMerchantOffers);
                if (offer != null && _database.TryGet<ItemDefinition>(offer.ItemId, out _))
                {
                    data.MerchantSpecialReserve.Add(new MerchantOfferSaveData
                    {
                        DefinitionId = offer.ItemId,
                        StackCount = Math.Max(1, offer.StackCount),
                        Price = 50L + (5L * unlocked.Count),
                        IsGems = true
                    });
                }
            }

            data.NewMerchantSpecialItems = true;
        }

        private void AddRegularOffer(SaveData data, MerchantOfferData offer)
        {
            if (offer == null || string.IsNullOrEmpty(offer.ItemId)) return;
            if (!_database.TryGet<ItemDefinition>(offer.ItemId, out var itemDef)) return;

            int stack = Math.Max(1, offer.StackCount);
            data.MerchantRegularStockItems.Add(new MerchantOfferSaveData
            {
                DefinitionId = offer.ItemId,
                StackCount = stack,
                Price = DecodeMath.TruncatePrice(itemDef.Price) * stack * 10L,
                IsGems = false
            });
        }

        private List<DungeonDefinition> GetUnlockedDungeons()
        {
            var data = _saveService.CurrentData;
            return (_database.GetAll<DungeonDefinition>() ?? new List<DungeonDefinition>())
                .Where(d => d != null && (string.IsNullOrEmpty(d.RequiredClearDungeonId) ||
                    data.Dungeons.Any(c => c.DefinitionId == d.RequiredClearDungeonId && c.MaxProgress >= d.RequiredClearProgress)))
                .ToList();
        }

        public bool UpgradeMarketListings()
        {
            var data = _saveService.CurrentData;
            if (data.LevelMarketListings >= MaxMarketListingsLevel) return false;
            long price = GetUpgradeMarketListingsPrice();
            if (data.Money < price) return false;
            data.Money -= price;
            data.LevelMarketListings++;
            _saveService.Save(out _);
            return true;
        }

        public bool UpgradeMarketTime()
        {
            var data = _saveService.CurrentData;
            if (data.LevelMarketTime >= MaxMarketTimeLevel) return false;
            long price = GetUpgradeMarketTimePrice();
            if (data.Money < price) return false;
            data.Money -= price;
            data.LevelMarketTime++;
            _saveService.Save(out _);
            return true;
        }

        public long GetUpgradeMarketListingsPrice() => _formulaService.GetMarketListingsPrice(_saveService.CurrentData.LevelMarketListings);
        public long GetUpgradeMarketTimePrice() => _formulaService.GetMarketTimePrice(_saveService.CurrentData.LevelMarketTime);
        public int GetMarketListingsCapacity() => _formulaService.MarketListings(_saveService.CurrentData.LevelMarketListings, _saveService.CurrentData.UpgradeMarketQueue, _saveService.CurrentData.GetPurchaseFlags());
        public int GetMarketListingsLevel() => _saveService.CurrentData.LevelMarketListings;
        public int GetMarketTimeLevel() => _saveService.CurrentData.LevelMarketTime;

        public IReadOnlyList<ItemActionSaveData> GetMarketListings() => _saveService.CurrentData.MarketListings;
        public IReadOnlyList<ItemActionSaveData> GetSoldMarketItems() => _saveService.CurrentData.SoldMarketItems;
    }
}
