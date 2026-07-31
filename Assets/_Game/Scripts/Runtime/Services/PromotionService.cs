using System;
using System.Collections.Generic;
using System.Linq;
using GuildMaster.Database;
using GuildMaster.Definitions;
using GuildMaster.Runtime.Save;
using UnityEngine;

namespace GuildMaster.Runtime.Services
{
    public interface IPromotionService
    {
        IReadOnlyList<PromotionDefinition> GetAvailablePromotions(CharacterSaveData character);
        bool CanPromote(CharacterSaveData character, string promotionId);
        bool Promote(CharacterSaveData character, string promotionId);
        int GetPromotionCount(string characterInstanceId);
    }

    public class PromotionService : IPromotionService
    {
        private readonly ISaveService _saveService;
        private readonly GameDatabase _database;
        private readonly IInventoryService _inventoryService;
        private readonly ICharacterService _characterService;

        public PromotionService(ISaveService saveService, GameDatabase database, IInventoryService inventoryService, ICharacterService characterService)
        {
            _saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            _characterService = characterService ?? throw new ArgumentNullException(nameof(characterService));
        }

        public IReadOnlyList<PromotionDefinition> GetAvailablePromotions(CharacterSaveData character)
        {
            if (character == null) return Array.Empty<PromotionDefinition>();

            var allPromotions = _database.GetAll<PromotionDefinition>();
            var available = new List<PromotionDefinition>();

            // Find promotions that are >= current promotion count (e.g., 0→1, 1→2)
            int currentCount = character.AscensionLevel; // Reuse AscensionLevel as promotion count
            foreach (var promo in allPromotions)
            {
                if (promo.TierIndex == currentCount + 1 && character.Level >= promo.RequiredLevel)
                {
                    available.Add(promo);
                }
            }
            return available.AsReadOnly();
        }

        public bool CanPromote(CharacterSaveData character, string promotionId)
        {
            if (character == null || string.IsNullOrEmpty(promotionId)) return false;

            var promo = _database.GetRequired<PromotionDefinition>(promotionId);
            if (promo == null) return false;

            int currentCount = character.AscensionLevel;
            if (promo.TierIndex != currentCount + 1) return false;
            if (character.Level < promo.RequiredLevel) return false;

            // Check item requirement
            if (!string.IsNullOrEmpty(promo.RequiredItemId))
            {
                var item = _inventoryService.GetAllItems().FirstOrDefault(i => i.Definition.id == promo.RequiredItemId);
                if (item == null || item.StackCount < promo.RequiredItemCount) return false;
            }

            return true;
        }

        public bool Promote(CharacterSaveData character, string promotionId)
        {
            if (!CanPromote(character, promotionId)) return false;

            var promo = _database.GetRequired<PromotionDefinition>(promotionId);

            // Consume required item
            if (!string.IsNullOrEmpty(promo.RequiredItemId))
            {
                var item = _inventoryService.GetAllItems().FirstOrDefault(i => i.Definition.id == promo.RequiredItemId);
                if (item != null)
                    _inventoryService.RemoveItem(item.InstanceId, promo.RequiredItemCount);
            }

            character.AscensionLevel++;
            character.Level = 1;
            character.Exp = 0;

            // Sync to runtime character in memory if exists
            var runtime = _characterService.GetAllCharacters().FirstOrDefault(c => c.InstanceId == character.InstanceId);
            if (runtime != null)
            {
                runtime.Level = 1;
                runtime.Experience = 0;
                runtime.AscensionLevel = character.AscensionLevel;
                runtime.IsAscended = true;
            }

            _saveService.Save(out _);
            Debug.Log($"[PromotionService] Character '{character.InstanceId}' promoted to tier {character.AscensionLevel}");
            return true;
        }

        public int GetPromotionCount(string characterInstanceId)
        {
            var character = _saveService.CurrentData.Characters?.FirstOrDefault(c => c.InstanceId == characterInstanceId);
            return character?.AscensionLevel ?? 0;
        }
    }
}
