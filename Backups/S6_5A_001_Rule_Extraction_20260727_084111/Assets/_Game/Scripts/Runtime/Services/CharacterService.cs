using System;
using System.Collections.Generic;
using System.Linq;
using GuildMaster.Definitions;
using GuildMaster.Runtime.Core;
using GuildMaster.Database;
using GuildMaster.Runtime.Formulas;
using GuildMaster.Runtime.Models;
using GuildMaster.Runtime.Save;

namespace GuildMaster.Runtime.Services
{
    public class CharacterService : ICharacterService
    {
        private readonly ISaveService _saveService;
        private readonly IFormulaService _formulaService;
        private readonly GameDatabase _registry;
        private readonly RuntimeFactory _runtimeFactory;
        private readonly IInventoryService _inventoryService;

        private readonly List<CharacterRuntime> _characters;

        public CharacterService(ISaveService saveService, IFormulaService formulaService, GameDatabase registry, RuntimeFactory runtimeFactory, IInventoryService inventoryService)
        {
            _saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
            _formulaService = formulaService ?? throw new ArgumentNullException(nameof(formulaService));
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _runtimeFactory = runtimeFactory ?? throw new ArgumentNullException(nameof(runtimeFactory));
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));

            _characters = new List<CharacterRuntime>();
            LoadFromSave();
        }

        private void LoadFromSave()
        {
            var savedChars = _saveService.CurrentData.Characters;
            foreach (var saveData in savedChars)
            {
                if (_registry.TryGet<AdventurerDefinition>(saveData.DefinitionId, out var def))
                {
                    var charRuntime = new CharacterRuntime(saveData.InstanceId, def)
                    {
                        Level = saveData.Level,
                        Experience = saveData.Exp,
                        CurrentHp = (int)saveData.CurrentHp
                    };

                    // Reconstruct equipment from inventory logic if they are in inventory or saved individually?
                    // In the original, the item is stored in the Character. 
                    // So we must retrieve the ItemRuntime from somewhere. Wait! If the equipment is on the Character, it's NOT in the inventory!
                    // So we should reconstruct it by using an ItemService or recreating the ItemRuntime from Save?
                    // Ah, the items on a character need their own ItemSaveData if we want to preserve their state!
                    // Wait, our SaveData.Items is a global list? No, our character equipment stores the InstanceId.
                    // This implies the ItemSaveData must still exist somewhere. Either in SaveData.Items (which is Inventory) or SaveData.EquippedItems.
                    // Let's assume for now they are created fresh if they don't have unique state, or we just keep their InstanceId but where is their Stack?
                    // We'll leave equipment loading as a TODO/ManualRuleRequired for S2-005 when SaveSystem is fully wired for Equipment.
                    
                    _characters.Add(charRuntime);
                }
            }
        }

        private void SyncToSave()
        {
            _saveService.CurrentData.Characters.Clear();
            foreach (var c in _characters)
            {
                _saveService.CurrentData.Characters.Add(new CharacterSaveData
                {
                    InstanceId = c.InstanceId,
                    DefinitionId = c.Definition.id,
                    Level = c.Level,
                    Exp = c.Experience,
                    CurrentHp = c.CurrentHp,
                    WeaponInstanceId = c.Weapon?.InstanceId,
                    ArmorInstanceId = c.Armor?.InstanceId,
                    AccessoryInstanceId = c.Accessory?.InstanceId
                });
            }
        }

        public CharacterRuntime CreateCharacter(string definitionId)
        {
            if (!_registry.TryGet<AdventurerDefinition>(definitionId, out var def))
            {
                throw new ArgumentException($"AdventurerDefinition not found: {definitionId}");
            }

            var character = _runtimeFactory.CreateCharacter(def);
            _characters.Add(character);
            SyncToSave();
            return character;
        }

        public int GetTotalStat(CharacterRuntime character, StatType statType)
        {
            if (character == null) return 0;
            
            // TODO: manualRuleRequired for level stat multiplier
            float levelMultiplier = 1.0f; // Reset to 1.0f as we don't have the exact formula yet
            int baseStat = 0;
            
            switch (statType)
            {
                case StatType.MaxHp: baseStat = (int)(character.Definition.BaseMaxHp * levelMultiplier); break;
                case StatType.Constitution: baseStat = (int)(character.Definition.BaseConstitution * levelMultiplier); break;
                case StatType.Intelligence: baseStat = (int)(character.Definition.BaseIntelligence * levelMultiplier); break;
                case StatType.Dexterity: baseStat = (int)(character.Definition.BaseDexterity * levelMultiplier); break;
                case StatType.Defense: baseStat = (int)(character.Definition.BaseDefense * levelMultiplier); break;
                case StatType.MagicDefense: baseStat = (int)(character.Definition.BaseMagicDefense * levelMultiplier); break;
            }

            // Add equipment bonus
            int equipBonus = 0;
            var equips = new[] { character.Weapon, character.Armor, character.Accessory };
            foreach (var equip in equips)
            {
                if (equip != null && equip.Definition != null)
                {
                    switch (statType)
                    {
                        case StatType.MaxHp: equipBonus += equip.Definition.MaxHp; break;
                        case StatType.Constitution: equipBonus += equip.Definition.Constitution; break;
                        case StatType.Intelligence: equipBonus += equip.Definition.Intelligence; break;
                        case StatType.Dexterity: equipBonus += equip.Definition.Dexterity; break;
                        case StatType.Defense: equipBonus += equip.Definition.Defense; break;
                        case StatType.MagicDefense: equipBonus += equip.Definition.MagicDefense; break;
                    }
                }
            }

            return baseStat + equipBonus;
        }

        public void GainExperience(CharacterRuntime character, int exp)
        {
            if (character == null || exp <= 0) return;
            if (character.Level >= character.Definition.MaxLevel) return;

            character.Experience += exp;
            
            while (LevelUp(character)) { }
            SyncToSave();
        }

        public bool LevelUp(CharacterRuntime character)
        {
            if (character.Level >= character.Definition.MaxLevel) return false;

            int requiredExp = _formulaService.ExperienceToNextLevel(character.Level, true);
            if (character.Experience >= requiredExp)
            {
                character.Experience -= requiredExp;
                character.Level++;
                
                // Heal on level up
                character.CurrentHp = GetTotalStat(character, StatType.MaxHp);
                return true;
            }
            return false;
        }

        public IReadOnlyList<CharacterRuntime> GetAllCharacters()
        {
            return _characters.AsReadOnly();
        }
    }
}
