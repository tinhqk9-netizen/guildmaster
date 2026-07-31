using System;
using System.Collections.Generic;
using System.Linq;
using GuildMaster.Database;
using GuildMaster.Definitions;
using GuildMaster.Runtime.Save;
using UnityEngine;

namespace GuildMaster.Runtime.Services
{
    public interface IPetService
    {
        IReadOnlyList<PetSaveData> GetAllPets();
        PetSaveData CreatePet(string definitionId, string ownerCharacterId);
        void AddExp(string instanceId, long amount);
        bool LevelUp(string instanceId);
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

        public PetService(ISaveService saveService, GameDatabase database)
        {
            _saveService = saveService;
            _database = database;
        }

        public IReadOnlyList<PetSaveData> GetAllPets()
        {
            if (_saveService.CurrentData == null || _saveService.CurrentData.Pets == null)
                return Array.Empty<PetSaveData>().ToList().AsReadOnly();
            return _saveService.CurrentData.Pets.AsReadOnly();
        }

        public PetSaveData CreatePet(string definitionId, string ownerCharacterId)
        {
            if (!_database.TryGet<PetDefinition>(definitionId, out var def))
            {
                Debug.LogError($"[PetService] PetDefinition '{definitionId}' not found");
                return null;
            }

            var pet = new PetSaveData
            {
                DefinitionId = definitionId,
                InstanceId = Guid.NewGuid().ToString(),
                Level = 1,
                Exp = 0,
                EquippedToCharacterId = ownerCharacterId
            };

            if (_saveService.CurrentData != null && _saveService.CurrentData.Pets != null)
            {
                _saveService.CurrentData.Pets.Add(pet);
                _saveService.Save(out _);
            }
            return pet;
        }

        public void AddExp(string instanceId, long amount)
        {
            if (_saveService.CurrentData == null || _saveService.CurrentData.Pets == null) return;
            var pet = _saveService.CurrentData.Pets.FirstOrDefault(p => p.InstanceId == instanceId);
            if (pet == null) return;

            pet.Exp += amount;
            if (_database.TryGet<PetDefinition>(pet.DefinitionId, out var def))
            {
                int expNeeded = def.ExpToLevel;
                if (expNeeded <= 0) expNeeded = 100;
                while (pet.Exp >= expNeeded)
                {
                    pet.Exp -= expNeeded;
                    pet.Level++;
                    expNeeded = def.ExpToLevel;
                    if (expNeeded <= 0) expNeeded = 100;
                }
            }
            _saveService.Save(out _);
        }

        public bool LevelUp(string instanceId)
        {
            if (_saveService.CurrentData == null || _saveService.CurrentData.Pets == null) return false;
            var pet = _saveService.CurrentData.Pets.FirstOrDefault(p => p.InstanceId == instanceId);
            if (pet == null) return false;

            if (_database.TryGet<PetDefinition>(pet.DefinitionId, out var def))
            {
                int expNeeded = def.ExpToLevel;
                if (expNeeded <= 0) expNeeded = 100;
                if (pet.Exp < expNeeded) return false;

                pet.Exp -= expNeeded;
                pet.Level++;
                _saveService.Save(out _);
                return true;
            }
            return false;
        }

        public bool EquipToCharacter(string petInstanceId, string characterInstanceId)
        {
            if (_saveService.CurrentData == null || _saveService.CurrentData.Pets == null) return false;
            var pet = _saveService.CurrentData.Pets.FirstOrDefault(p => p.InstanceId == petInstanceId);
            if (pet == null) return false;

            pet.EquippedToCharacterId = characterInstanceId;
            _saveService.Save(out _);
            return true;
        }

        public bool UnequipFromCharacter(string petInstanceId)
        {
            if (_saveService.CurrentData == null || _saveService.CurrentData.Pets == null) return false;
            var pet = _saveService.CurrentData.Pets.FirstOrDefault(p => p.InstanceId == petInstanceId);
            if (pet == null) return false;

            pet.EquippedToCharacterId = null;
            _saveService.Save(out _);
            return true;
        }

        public IReadOnlyList<PetSaveData> GetCharacterPets(string characterInstanceId)
        {
            if (_saveService.CurrentData == null || _saveService.CurrentData.Pets == null)
                return Array.Empty<PetSaveData>().ToList().AsReadOnly();

            return _saveService.CurrentData.Pets
                .Where(p => p.EquippedToCharacterId == characterInstanceId)
                .ToList()
                .AsReadOnly();
        }

        public bool HasPetEquipped(string characterInstanceId)
        {
            if (_saveService.CurrentData == null || _saveService.CurrentData.Pets == null) return false;

            return _saveService.CurrentData.Pets
                .Any(p => p.EquippedToCharacterId == characterInstanceId);
        }

        public float GetAttackBonus(string characterInstanceId)
        {
            var pets = GetCharacterPets(characterInstanceId);
            float bonus = 0;
            foreach (var pet in pets)
            {
                if (_database.TryGet<PetDefinition>(pet.DefinitionId, out var def))
                    bonus += def.BaseAttack * pet.Level * def.AttackMultiplier;
            }
            return bonus;
        }

        public float GetDefenseBonus(string characterInstanceId)
        {
            var pets = GetCharacterPets(characterInstanceId);
            float bonus = 0;
            foreach (var pet in pets)
            {
                if (_database.TryGet<PetDefinition>(pet.DefinitionId, out var def))
                    bonus += def.BaseDefense * pet.Level * def.DefenseMultiplier;
            }
            return bonus;
        }

        public float GetHpBonus(string characterInstanceId)
        {
            var pets = GetCharacterPets(characterInstanceId);
            float bonus = 0;
            foreach (var pet in pets)
            {
                if (_database.TryGet<PetDefinition>(pet.DefinitionId, out var def))
                    bonus += def.BaseMaxHp * pet.Level * def.HpMultiplier;
            }
            return bonus;
        }

        public float GetSpeedBonus(string characterInstanceId)
        {
            var pets = GetCharacterPets(characterInstanceId);
            float bonus = 0;
            foreach (var pet in pets)
            {
                if (_database.TryGet<PetDefinition>(pet.DefinitionId, out var def))
                    bonus += def.BaseSpeed * pet.Level * def.SpeedMultiplier;
            }
            return bonus;
        }
    }
}
