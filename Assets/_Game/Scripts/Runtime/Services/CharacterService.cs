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
        private readonly IPetService _petService;

        private readonly List<CharacterRuntime> _characters;

        public CharacterService(
            ISaveService saveService, 
            IFormulaService formulaService, 
            GameDatabase registry, 
            RuntimeFactory runtimeFactory, 
            IInventoryService inventoryService,
            IPetService petService = null)
        {
            _saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
            _formulaService = formulaService ?? throw new ArgumentNullException(nameof(formulaService));
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _runtimeFactory = runtimeFactory ?? throw new ArgumentNullException(nameof(runtimeFactory));
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            _petService = petService;

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
                        IsAscended = saveData.IsAscended,
                        AscensionLevel = saveData.AscensionLevel,
                        Trait = saveData.Trait,
                        PotionsDrank = saveData.PotionsDrank
                    };

                    if (!string.IsNullOrEmpty(saveData.WeaponInstanceId))
                        charRuntime.Weapon = _inventoryService.GetItem(saveData.WeaponInstanceId);
                    if (!string.IsNullOrEmpty(saveData.ArmorInstanceId))
                        charRuntime.Armor = _inventoryService.GetItem(saveData.ArmorInstanceId);
                    if (!string.IsNullOrEmpty(saveData.AccessoryInstanceId))
                        charRuntime.Accessory = _inventoryService.GetItem(saveData.AccessoryInstanceId);

                    if (!saveData.IsHpInitialized)
                    {
                        int maxHp = GetTotalStat(charRuntime, StatType.MaxHp);
                        charRuntime.CurrentHp = maxHp;
                        saveData.IsHpInitialized = true;
                        saveData.CurrentHp = maxHp;
                    }
                    else
                    {
                        charRuntime.CurrentHp = (int)saveData.CurrentHp;
                    }

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
                    AccessoryInstanceId = c.Accessory?.InstanceId,
                    IsAscended = c.IsAscended,
                    AscensionLevel = c.AscensionLevel,
                    Trait = c.Trait,
                    PotionsDrank = c.PotionsDrank,
                    IsHpInitialized = true
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
            character.CurrentHp = GetTotalStat(character, StatType.MaxHp);
            _characters.Add(character);
            SyncToSave();
            return character;
        }

        public CharacterRuntime RecruitCharacter(CharacterSaveData saveData)
        {
            if (!_registry.TryGet<AdventurerDefinition>(saveData.DefinitionId, out var def))
            {
                throw new ArgumentException($"AdventurerDefinition not found: {saveData.DefinitionId}");
            }

            var charRuntime = new CharacterRuntime(saveData.InstanceId, def)
            {
                Level = saveData.Level,
                Experience = saveData.Exp,
                IsAscended = saveData.IsAscended,
                AscensionLevel = saveData.AscensionLevel,
                Trait = saveData.Trait,
                PotionsDrank = saveData.PotionsDrank
            };

            if (!string.IsNullOrEmpty(saveData.WeaponInstanceId))
                charRuntime.Weapon = _inventoryService.GetItem(saveData.WeaponInstanceId);
            if (!string.IsNullOrEmpty(saveData.ArmorInstanceId))
                charRuntime.Armor = _inventoryService.GetItem(saveData.ArmorInstanceId);
            if (!string.IsNullOrEmpty(saveData.AccessoryInstanceId))
                charRuntime.Accessory = _inventoryService.GetItem(saveData.AccessoryInstanceId);

            if (!saveData.IsHpInitialized)
            {
                int maxHp = GetTotalStat(charRuntime, StatType.MaxHp);
                charRuntime.CurrentHp = maxHp;
                saveData.IsHpInitialized = true;
                saveData.CurrentHp = maxHp;
            }
            else
            {
                charRuntime.CurrentHp = (int)saveData.CurrentHp;
            }

            _characters.Add(charRuntime);
            _saveService.CurrentData.Characters.Add(saveData);
            return charRuntime;
        }

        public int GetTotalStat(CharacterRuntime character, StatType statType)
        {
            if (character == null || character.Definition == null) return 0;

            double promoMult = 1.0;
            try
            {
                var promo = _registry.GetAll<PromotionDefinition>()?.FirstOrDefault(p => p.TierIndex == character.AscensionLevel);
                promoMult = promo != null ? promo.StatMultiplier : (1.0 + character.AscensionLevel * 0.1);
            }
            catch
            {
                promoMult = 1.0 + character.AscensionLevel * 0.1;
            }

            double legacyMult = character.IsAscended ? 1.5 : 1.0;
            double mult = legacyMult * promoMult;

            int warLevel = 0, fortitudeLevel = 0, ruinLevel = 0, graceLevel = 0, illusionLevel = 0, knowledgeLevel = 0;
            if (_saveService?.CurrentData != null)
            {
                warLevel = _saveService.CurrentData.WarLevel;
                fortitudeLevel = _saveService.CurrentData.FortitudeLevel;
                ruinLevel = _saveService.CurrentData.RuinLevel;
                graceLevel = _saveService.CurrentData.GraceLevel;
                illusionLevel = _saveService.CurrentData.IllusionLevel;
                knowledgeLevel = _saveService.CurrentData.KnowledgeLevel;
            }

            int doctrineCON = warLevel * 2;
            int doctrineDEX = warLevel * 2;
            int doctrineINT = knowledgeLevel * 2;
            int doctrineHP = fortitudeLevel * 15 + ruinLevel * 25;
            int doctrineDEF = fortitudeLevel * 3;
            int doctrineMDEF = illusionLevel * 3;
            bool doubleAccessory = graceLevel >= 2;

            int baseStat = 0;

            // Potion index mapping (Recovered Rule #1): CON->0, INT->2, DEX->1, HP->3(*5), DEF->4, MDEF->5
            int[] potions = character.PotionsDrank ?? new int[6];

            switch (statType)
            {
                case StatType.Constitution:
                    baseStat = (int)(character.Definition.BaseConstitution * mult) + (potions.Length > 0 ? potions[0] : 0) + doctrineCON;
                    break;
                case StatType.Intelligence:
                    baseStat = (int)(character.Definition.BaseIntelligence * mult) + (potions.Length > 2 ? potions[2] : 0) + doctrineINT;
                    break;
                case StatType.Dexterity:
                    baseStat = (int)(character.Definition.BaseDexterity * mult) + (potions.Length > 1 ? potions[1] : 0) + doctrineDEX;
                    break;
                case StatType.MaxHp:
                    baseStat = (int)((character.Definition.BaseMaxHp + character.Level - 1) * mult) + (potions.Length > 3 ? potions[3] * 5 : 0) + doctrineHP;
                    break;
                case StatType.Defense:
                    baseStat = character.Definition.BaseDefense + (potions.Length > 4 ? potions[4] : 0) + doctrineDEF; // DEF does NOT multiply by mult!
                    break;
                case StatType.MagicDefense:
                    baseStat = character.Definition.BaseMagicDefense + (potions.Length > 5 ? potions[5] : 0) + doctrineMDEF; // MDEF does NOT multiply by mult!
                    break;
            }

            // Equipment bonus
            int equipBonus = 0;
            var equips = new[] { character.Weapon, character.Armor, character.Accessory };
            foreach (var equip in equips)
            {
                if (equip != null && equip.Definition != null)
                {
                    int factor = (doubleAccessory && equip == character.Accessory) ? 2 : 1;
                    switch (statType)
                    {
                        case StatType.MaxHp: equipBonus += equip.Definition.MaxHp * factor; break;
                        case StatType.Constitution: equipBonus += equip.Definition.Constitution * factor; break;
                        case StatType.Intelligence: equipBonus += equip.Definition.Intelligence * factor; break;
                        case StatType.Dexterity: equipBonus += equip.Definition.Dexterity * factor; break;
                        case StatType.Defense: equipBonus += equip.Definition.Defense * factor; break;
                        case StatType.MagicDefense: equipBonus += equip.Definition.MagicDefense * factor; break;
                    }
                }
            }

            double traitMult = GetTraitMultiplier(character.Trait, statType);

            int total = DecodeMath.Round((baseStat + equipBonus) * traitMult);

            if (_petService != null && character != null)
            {
                if (statType == StatType.MaxHp)
                {
                    total += (int)_petService.GetHpBonus(character.InstanceId);
                }
                else if (statType == StatType.Defense)
                {
                    total += (int)_petService.GetDefenseBonus(character.InstanceId);
                }
                else if (statType == StatType.Dexterity)
                {
                    total += (int)_petService.GetSpeedBonus(character.InstanceId);
                }
            }

            return total;
        }

        private double GetTraitMultiplier(string trait, StatType statType)
        {
            if (string.IsNullOrEmpty(trait)) return 1.0;
            switch (trait.ToUpperInvariant())
            {
                case "BRUTE":
                case "STOUT":
                    return statType == StatType.Constitution ? 1.15 : 1.0;
                case "BOOKWORM":
                    return statType == StatType.Intelligence ? 1.15 : 1.0;
                case "FERAL":
                case "NIMBLE":
                    return statType == StatType.Dexterity ? 1.15 : 1.0;
                case "KEEN_EYED":
                    if (statType == StatType.Dexterity) return 1.10;
                    if (statType == StatType.Intelligence) return 1.05;
                    return 1.0;
                default:
                    return 1.0;
            }
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
                // Exact Java Parity: Java explicitly resets experience to 0 and discards remainder
                character.Experience = 0;
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

        public bool CanDismissCharacter(string instanceId, out string reason)
        {
            var character = _characters.FirstOrDefault(c => c.InstanceId == instanceId);
            if (character == null)
            {
                reason = "Character not found.";
                return false;
            }

            var saveData = _saveService.CurrentData;
            if (saveData != null)
            {
                if (saveData.CurrentParty != null && saveData.CurrentParty.Contains(instanceId))
                {
                    reason = "Cannot dismiss a character while they are in the active party.";
                    return false;
                }

                if (saveData.ActiveDungeon != null && saveData.ActiveDungeon.AdventurerInstanceIds != null && saveData.ActiveDungeon.AdventurerInstanceIds.Contains(instanceId))
                {
                    reason = "Cannot dismiss a character while they are exploring a dungeon.";
                    return false;
                }

                if (saveData.ActiveExpeditions != null)
                {
                    foreach (var exp in saveData.ActiveExpeditions)
                    {
                        if (exp?.Dungeon?.AdventurerInstanceIds != null && exp.Dungeon.AdventurerInstanceIds.Contains(instanceId))
                        {
                            reason = "Cannot dismiss a character while they are exploring a dungeon.";
                            return false;
                        }
                    }
                }
            }

            reason = string.Empty;
            return true;
        }

        public bool DismissCharacter(string instanceId, out string errorReason)
        {
            if (!CanDismissCharacter(instanceId, out errorReason))
            {
                return false;
            }

            var character = _characters.FirstOrDefault(c => c.InstanceId == instanceId);
            if (character == null)
            {
                errorReason = "Character not found.";
                return false;
            }

            // Unlock items safely
            if (character.Weapon != null) character.Weapon.IsLocked = false;
            if (character.Armor != null) character.Armor.IsLocked = false;
            if (character.Accessory != null) character.Accessory.IsLocked = false;

            // Remove from SaveData
            var saveData = _saveService.CurrentData;
            var saveIndex = saveData.Characters.FindIndex(c => c.InstanceId == instanceId);
            if (saveIndex >= 0)
            {
                saveData.Characters.RemoveAt(saveIndex);
            }

            // Remove from runtime
            _characters.Remove(character);
            
            errorReason = string.Empty;
            return true;
        }
    }
}
