using System;
using System.Collections.Generic;
using System.Linq;
using GuildMaster.Database;
using GuildMaster.Definitions;
using GuildMaster.Runtime.Save;

namespace GuildMaster.Runtime.Services
{
    public interface IPetService
    {
        IReadOnlyList<PetSaveData> GetAllPets();
        PetSaveData CreatePet(string definitionId, string ownerCharacterId = null);
        PetSaveData HatchEgg(string eggDefinitionId);
        bool AssignToDungeon(string petInstanceId, string dungeonId);
        bool UnassignFromDungeon(string petInstanceId);
        IReadOnlyList<PetSaveData> GetDungeonPets(string dungeonId);
        double GetDropBonus(string petInstanceId);
        double GetExperienceBonus(string petInstanceId);
        void SetFavourite(string petInstanceId, bool favourite);
        int GetFoodToNextLevel(string petInstanceId);
        int Feed(string petInstanceId, int foodAmount);
        bool FeedWithItem(string petInstanceId, string itemInstanceId, int amount);
        bool ReleasePet(string petInstanceId);
        void AddExp(string instanceId, long amount);
        bool LevelUp(string instanceId);

        // Compatibility methods retained for existing UI/model callers. Legacy pets are not
        // hero equipment, so these methods no longer grant fabricated combat stat bonuses.
        bool EquipToCharacter(string petInstanceId, string characterInstanceId);
        bool UnequipFromCharacter(string petInstanceId);
        IReadOnlyList<PetSaveData> GetCharacterPets(string characterInstanceId);
        bool HasPetEquipped(string characterInstanceId);
        float GetAttackBonus(string characterInstanceId);
        float GetDefenseBonus(string characterInstanceId);
        float GetHpBonus(string characterInstanceId);
        float GetSpeedBonus(string characterInstanceId);
    }

    public class PetService : IPetService
    {
        private readonly ISaveService _saveService;
        private readonly GameDatabase _database;
        private readonly IInventoryService _inventoryService;
        private readonly System.Random _random = new System.Random();

        public PetService(ISaveService saveService, GameDatabase database, IInventoryService inventoryService = null)
        {
            _saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _inventoryService = inventoryService;
            _saveService.CurrentData?.NormalizeAfterLoad();
        }

        public IReadOnlyList<PetSaveData> GetAllPets()
        {
            return _saveService.CurrentData?.Pets?.AsReadOnly() ?? Array.Empty<PetSaveData>().ToList().AsReadOnly();
        }

        public PetSaveData HatchEgg(string eggDefinitionId)
        {
            if (_inventoryService == null || string.IsNullOrEmpty(eggDefinitionId)) return null;
            if (!_database.TryGet<ItemDefinition>(eggDefinitionId, out var egg) || !IsPetEgg(egg)) return null;

            string family = EggToFamily(eggDefinitionId);
            var candidates = _database.GetAll<PetDefinition>()
                .Where(p => p != null && string.Equals(p.PetFamily, family, StringComparison.OrdinalIgnoreCase)).ToList();
            if (candidates.Count == 0) return null;

            // Resolve the Legacy family/tier before consuming the egg. A malformed or
            // incomplete database must never destroy the player's egg without producing a pet.
            if (!_inventoryService.ConsumeByDefinitionId(eggDefinitionId, 1)) return null;

            // Java: 75% common, 20% uncommon, 5% rare. Tier is the value restored from the
            // Java class' abilityNumber (2/3/4 -> 1/2/3).
            double roll = _random.NextDouble();
            int tier = roll < 0.75d ? 1 : (roll < 0.95d ? 2 : 3);
            var definition = candidates.FirstOrDefault(p => p.PetTier == tier) ?? candidates.OrderBy(p => p.PetTier).First();
            return CreatePetInternal(definition);
        }

        public PetSaveData CreatePet(string definitionId, string ownerCharacterId = null)
        {
            if (!_database.TryGet<PetDefinition>(definitionId, out var definition)) return null;
            return CreatePetInternal(definition);
        }

        private PetSaveData CreatePetInternal(PetDefinition definition)
        {
            var pet = new PetSaveData
            {
                DefinitionId = definition.id,
                InstanceId = Guid.NewGuid().ToString(),
                Level = 1,
                Exp = 0,
                Food = 0,
                Favourite = false,
                Ability1 = RollGuaranteedAbility(definition.PetFamily),
                Ability2 = "EMPTY",
                Ability3 = "EMPTY",
                Ability4 = "EMPTY"
            };
            var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { pet.Ability1 };
            pet.Ability2 = RollAbility(used);
            used.Add(pet.Ability2);
            if (definition.AbilityNumber > 2) { pet.Ability3 = RollAbility(used); used.Add(pet.Ability3); }
            if (definition.AbilityNumber > 3) pet.Ability4 = RollAbility(used);
            _saveService.CurrentData?.Pets.Add(pet);
            _saveService.Save(out _);
            return pet;
        }

        public bool AssignToDungeon(string petInstanceId, string dungeonId)
        {
            var pet = Find(petInstanceId);
            if (pet == null || string.IsNullOrEmpty(dungeonId) ||
                !_database.TryGet<GuildMaster.Definitions.DungeonDefinition>(dungeonId, out _)) return false;
            if (!string.IsNullOrEmpty(pet.AssignedDungeonId) &&
                !string.Equals(pet.AssignedDungeonId, dungeonId, StringComparison.OrdinalIgnoreCase)) return false;
            pet.AssignedDungeonId = dungeonId;
            pet.EquippedToCharacterId = null;
            _saveService.Save(out _);
            return true;
        }

        public bool UnassignFromDungeon(string petInstanceId)
        {
            var pet = Find(petInstanceId);
            if (pet == null) return false;
            pet.AssignedDungeonId = null;
            _saveService.Save(out _);
            return true;
        }

        public IReadOnlyList<PetSaveData> GetDungeonPets(string dungeonId)
        {
            return GetAllPets().Where(p => string.Equals(p.AssignedDungeonId, dungeonId, StringComparison.OrdinalIgnoreCase)).ToList().AsReadOnly();
        }

        public double GetDropBonus(string petInstanceId)
        {
            var pet = Find(petInstanceId);
            return pet == null || !HasAbility(pet, "DROPS") ? 0d : pet.Level * 0.3d / 100d;
        }

        public double GetExperienceBonus(string petInstanceId)
        {
            var pet = Find(petInstanceId);
            return pet == null || !HasAbility(pet, "EXPERIENCE") ? 0d : pet.Level * 0.4d / 100d;
        }

        public void SetFavourite(string petInstanceId, bool favourite)
        {
            var pet = Find(petInstanceId);
            if (pet == null) return;
            pet.Favourite = favourite;
            _saveService.Save(out _);
        }

        public int GetFoodToNextLevel(string petInstanceId)
        {
            var pet = Find(petInstanceId);
            return pet == null ? 0 : FoodToNextLevel(pet.Level);
        }

        public int Feed(string petInstanceId, int foodAmount)
        {
            var pet = Find(petInstanceId);
            if (pet == null || foodAmount <= 0) return 0;
            int oldLevel = pet.Level;
            pet.Food += foodAmount;
            while (pet.Food >= FoodToNextLevel(pet.Level))
            {
                pet.Food -= FoodToNextLevel(pet.Level);
                pet.Level++;
            }
            _saveService.Save(out _);
            return pet.Level - oldLevel;
        }

        public bool FeedWithItem(string petInstanceId, string itemInstanceId, int amount)
        {
            if (amount <= 0 || _inventoryService == null) return false;
            var item = _inventoryService.GetItem(itemInstanceId);
            if (item?.Definition == null || !string.Equals(item.Definition.parentClass, "Food", StringComparison.OrdinalIgnoreCase) || item.Definition.FeedPower <= 0 || item.StackCount < amount)
                return false;
            int food = item.Definition.FeedPower * amount;
            if (!_inventoryService.RemoveItem(itemInstanceId, amount)) return false;
            Feed(petInstanceId, food);
            return true;
        }

        public bool ReleasePet(string petInstanceId)
        {
            if (string.IsNullOrEmpty(petInstanceId) || _saveService.CurrentData == null) return false;

            bool activeCompanion = _saveService.CurrentData.ActiveDungeon != null &&
                                   string.Equals(_saveService.CurrentData.ActiveDungeon.PetInstanceId,
                                       petInstanceId, StringComparison.Ordinal);
            if (!activeCompanion && _saveService.CurrentData.ActiveExpeditions != null)
            {
                activeCompanion = _saveService.CurrentData.ActiveExpeditions.Any(expedition =>
                    string.Equals(expedition?.Dungeon?.PetInstanceId, petInstanceId, StringComparison.Ordinal));
            }

            // Do not leave an active expedition pointing at a deleted pet. The UI keeps the
            // detail open when this guard rejects the release, so the player can stop the run
            // first and try again without corrupting save state.
            if (activeCompanion) return false;

            int removed = _saveService.CurrentData.Pets.RemoveAll(pet =>
                pet != null && string.Equals(pet.InstanceId, petInstanceId, StringComparison.Ordinal));
            if (removed == 0) return false;

            _saveService.Save(out _);
            return true;
        }

        public void AddExp(string instanceId, long amount)
        {
            if (amount <= 0) return;
            Feed(instanceId, amount > int.MaxValue ? int.MaxValue : (int)amount);
        }

        public bool LevelUp(string instanceId)
        {
            var pet = Find(instanceId);
            if (pet == null || pet.Food < FoodToNextLevel(pet.Level)) return false;
            Feed(instanceId, FoodToNextLevel(pet.Level));
            return true;
        }

        public bool EquipToCharacter(string petInstanceId, string characterInstanceId) => false;
        public bool UnequipFromCharacter(string petInstanceId) => false;
        public IReadOnlyList<PetSaveData> GetCharacterPets(string characterInstanceId) => Array.Empty<PetSaveData>();
        public bool HasPetEquipped(string characterInstanceId) => false;
        public float GetAttackBonus(string characterInstanceId) => 0f;
        public float GetDefenseBonus(string characterInstanceId) => 0f;
        public float GetHpBonus(string characterInstanceId) => 0f;
        public float GetSpeedBonus(string characterInstanceId) => 0f;

        private PetSaveData Find(string instanceId) => GetAllPets().FirstOrDefault(p => p.InstanceId == instanceId);

        private static bool IsPetEgg(ItemDefinition item)
        {
            return item != null && !string.IsNullOrEmpty(item.id) &&
                   item.id.EndsWith("_egg", StringComparison.OrdinalIgnoreCase) &&
                   item.id != "frozen_egg";
        }

        private static string EggToFamily(string eggId)
        {
            return eggId.Substring(0, eggId.Length - "_egg".Length);
        }

        private string RollGuaranteedAbility(string family)
        {
            var choices = family.ToLowerInvariant() switch
            {
                "avian" => new[] { "DECOY" },
                "construct" => new[] { "MAGIC", "BRIGHT" },
                "esoteric" => new[] { "EXPERIENCE", "DROPS" },
                "insect" => new[] { "FIGHTER", "BARRIER" },
                "reptile" => new[] { "SAVAGE", "OPPORTUNIST" },
                "wild" => new[] { "LIFESTEAL", "COUNTERATTACK" },
                "wooden" => new[] { "REGENERATION", "HEALER" },
                _ => new[] { "EMPTY" }
            };
            return choices[_random.Next(choices.Length)];
        }

        private string RollAbility(HashSet<string> used)
        {
            var abilities = new[] { "FIGHTER", "HEALER", "DECOY", "OPPORTUNIST", "MAGIC", "SAVAGE", "BRIGHT", "EXPERIENCE", "DROPS", "COUNTERATTACK", "LIFESTEAL", "REGENERATION", "BARRIER" };
            string result;
            do { result = abilities[_random.Next(abilities.Length)]; } while (used.Contains(result));
            return result;
        }

        private static int FoodToNextLevel(int level)
        {
            // Java Formulas.foodToNextLevel is an integer progression; this boundary keeps the
            // current formula service out of the pet data model until its exact method is exposed.
            return Math.Max(1, (int)(Math.Pow(1.085d, level) * 30.0d));
        }

        private static bool HasAbility(PetSaveData pet, string ability)
        {
            return string.Equals(pet.Ability1, ability, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(pet.Ability2, ability, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(pet.Ability3, ability, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(pet.Ability4, ability, StringComparison.OrdinalIgnoreCase);
        }
    }
}
