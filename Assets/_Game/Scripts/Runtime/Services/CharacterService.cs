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
        private readonly IDoctrineService _doctrineService;

        private readonly List<CharacterRuntime> _characters;

        private ItemRuntime ResolveEquippedWeapon(CharacterSaveData saveData, AdventurerDefinition definition)
        {
            if (saveData == null || string.IsNullOrEmpty(saveData.WeaponInstanceId)) return null;

            var existing = _inventoryService.GetItem(saveData.WeaponInstanceId);
            if (existing != null)
            {
                _inventoryService.AddEquippedItem(existing);
                return existing;
            }

            // Tavern visitors intentionally do not create an inventory item. On recruit
            // (or loading a save created by that flow), materialize the starter weapon
            // with the visitor's stable instance id as the character-owned item.
            if (definition == null || string.IsNullOrEmpty(definition.StarterWeaponId) ||
                !_registry.TryGet<ItemDefinition>(definition.StarterWeaponId, out var itemDefinition))
                return null;

            var starter = new ItemRuntime(saveData.WeaponInstanceId, itemDefinition, 1)
            {
                IsLocked = true
            };
            _inventoryService.AddEquippedItem(starter);
            return starter;
        }

        public CharacterService(
            ISaveService saveService,
            IFormulaService formulaService,
            GameDatabase registry,
            RuntimeFactory runtimeFactory,
            IInventoryService inventoryService,
            IPetService petService = null,
            IDoctrineService doctrineService = null)
        {
            _saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
            _formulaService = formulaService ?? throw new ArgumentNullException(nameof(formulaService));
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _runtimeFactory = runtimeFactory ?? throw new ArgumentNullException(nameof(runtimeFactory));
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            _petService = petService;
            _doctrineService = doctrineService;

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
                        TraitCommon = saveData.TraitCommon,
                        TraitRare = saveData.TraitRare,
                        Trait = saveData.TraitCommon, // legacy display alias, see CharacterRuntime.Trait
                        PotionsDrank = saveData.PotionsDrank,
                        ActiveSkillId = def.ActiveSkill,
                        PassiveSkillId = def.PassiveSkill
                    };

                    if (!string.IsNullOrEmpty(saveData.WeaponInstanceId))
                        charRuntime.Weapon = ResolveEquippedWeapon(saveData, def);
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
                    TraitCommon = c.TraitCommon,
                    TraitRare = c.TraitRare,
                    Trait = c.TraitCommon,
                    PotionsDrank = c.PotionsDrank,
                    IsHpInitialized = true
                });
            }
        }

        /// <summary>
        /// Real Legacy promotion/ascension: reassigns a character to a different
        /// AdventurerDefinition (class change), resetting Level/Experience to 1/0 exactly like
        /// Java's <c>Adventurer.getInstance(newClass, id, 1, 0, ...)</c> in
        /// DialogPromotionChoices/DialogEntityDetail.ascend(). Weapon/Armor/Accessory/traits/
        /// potions/doctrine are intentionally left untouched — Java carries them through
        /// unchanged on both promotion and ascension. Used by PromotionService only.
        /// </summary>
        public bool ChangeClass(string instanceId, string newDefinitionId, bool setAscended)
        {
            var character = _characters.FirstOrDefault(c => c.InstanceId == instanceId);
            if (character == null) return false;
            if (!_registry.TryGet<AdventurerDefinition>(newDefinitionId, out var newDef)) return false;

            character.Definition = newDef;
            character.DefinitionId = newDef.id;
            character.Level = 1;
            character.Experience = 0;
            character.ActiveSkillId = newDef.ActiveSkill;
            character.PassiveSkillId = newDef.PassiveSkill;
            if (setAscended) character.IsAscended = true;
            character.CurrentHp = GetTotalStat(character, StatType.MaxHp);

            SyncToSave();
            return true;
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
                TraitCommon = saveData.TraitCommon,
                TraitRare = saveData.TraitRare,
                Trait = saveData.TraitCommon,
                PotionsDrank = saveData.PotionsDrank,
                ActiveSkillId = def.ActiveSkill,
                PassiveSkillId = def.PassiveSkill
            };

            if (!string.IsNullOrEmpty(saveData.WeaponInstanceId))
                charRuntime.Weapon = ResolveEquippedWeapon(saveData, def);
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

            // Java's Adventurer.calculateTotalStat(int) body is not decompilable (stripped in
            // the APK). The +50% CON/INT/DEX/HP-not-DEF/MDEF multiplier for ascended() heroes is
            // a previously recovered rule (see S6_5A_Stage4_CharacterTests), kept as-is. The old
            // "promoMult" (AscensionLevel * 10%, or a PromotionDefinition.StatMultiplier that
            // never had any data source — promotions.json never existed) was fabricated: Phase 1
            // removed it because real class promotion now changes character.Definition entirely
            // (fresh BaseXXX from the new AdventurerDefinition), so no artificial multiplier is
            // needed on top of that.
            double mult = character.IsAscended ? 1.5 : 1.0;

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

                // --- Phase 2A: Equipment combat modifiers (Java Adventurer.getThreat() /
                // calculateTotalLifesteal() / calculateTotalRegeneration() / experienceMultiplier()
                // / darknessReduction() — all "base(from Entity, usually 0/1) + weapon + armor +
                // accessory [+ doctrine]" sums). Base stat here mirrors the Entity-level default
                // only; the equipment loop below adds the item contributions exactly like the
                // Constitution/Defense cases above.
                case StatType.Threat:
                    baseStat = 1; // Java: Entity.threat = 1 (Adventurer inherits, does not override the field)
                    break;
                case StatType.Lifesteal:
                case StatType.Regeneration:
                case StatType.BonusExperience:
                case StatType.DarknessReduction:
                    baseStat = 0;
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
                        case StatType.Threat: equipBonus += equip.Definition.Threat * factor; break;
                        case StatType.Lifesteal: equipBonus += equip.Definition.Lifesteal * factor; break;
                        case StatType.Regeneration: equipBonus += equip.Definition.Regeneration * factor; break;
                        case StatType.BonusExperience: equipBonus += equip.Definition.BonusExperience * factor; break;
                        case StatType.DarknessReduction: equipBonus += equip.Definition.DarknessReduction * factor; break;
                    }
                }
            }

            // Doctrine node bonuses feeding combat stats (Phase 2A). Only the ability types with a
            // confirmed, direct Java mapping are wired: MANIFEST_DANGER->bonusThreat (Fortitude),
            // SERVUS_SANGUINIS->bonusLifesteal (Affliction). See DoctrineCatalog.cs /
            // DoctrineOfFortitude.java / DoctrineOfAffliction.java.
            if (_doctrineService != null)
            {
                if (statType == StatType.Threat) equipBonus += _doctrineService.GetAggregateAbilityValue("MANIFEST_DANGER");
                if (statType == StatType.Lifesteal) equipBonus += _doctrineService.GetAggregateAbilityValue("SERVUS_SANGUINIS");
            }


            // Java: only the 3 common traits (+ their _PLUS premium variants) are visible
            // affecting CON/INT/DEX anywhere in Adventurer.java; none of the 14 rare traits touch
            // a base stat total (they hook combat mechanics instead — mana regen, crit, lifesteal,
            // dodge, etc., which are Phase 2/Combat concerns). So only TraitCommon feeds this.
            double traitMult = GetTraitMultiplier(character.TraitCommon, statType);

            int total = DecodeMath.Round((baseStat + equipBonus) * traitMult);

            // Java: Adventurer.getThreat() -> Math.max(1, threat + doctrine.bonusThreat())
            if (statType == StatType.Threat) total = Math.Max(1, total);

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

        /// <summary>
        /// Phase 2A: percentage-scale combat modifiers (Java doubles, e.g. 0.25 == 25%) that
        /// GetTotalStat's int-typed contract can't represent. Mirrors Adventurer.java's
        /// calculateCounterattackChance() / calculateCriticalChance() / calculateCriticalDamage()
        /// / calculateTotalFlatDodgeChance() / calculateHealingModifier() /
        /// calculateImmunityToStatus() — equipment sum + doctrine node bonus (*0.01, matching
        /// Java's percentage-point convention) where a confirmed node mapping exists. Rare-trait
        /// contributions (REACTIVE/RUTHLESS/NIMBLE/MINDFUL/EMPATHETIC) are NOT included — Phase 1
        /// explicitly deferred all rare-trait combat hooks to Phase 2/Combat and this task's scope
        /// is Equipment + the Combat pipeline, not a full trait-effect restore.
        /// </summary>
        public double GetCombatModifier(CharacterRuntime character, StatType statType)
        {
            if (character == null) return 0.0;

            double baseValue = 0.0;
            if (statType == StatType.CriticalDamage) baseValue = 1.5; // Java: Entity.criticalDamage default = 1.5

            double equipSum = 0.0;
            var equips = new[] { character.Weapon, character.Armor, character.Accessory };
            foreach (var equip in equips)
            {
                if (equip?.Definition == null) continue;
                switch (statType)
                {
                    case StatType.Counterattack: equipSum += equip.Definition.Counterattack; break;
                    case StatType.CriticalChance: equipSum += equip.Definition.CriticalChance; break;
                    case StatType.CriticalDamage: equipSum += equip.Definition.CriticalDamage; break;
                    case StatType.FlatDodgeChance: equipSum += equip.Definition.FlatDodgeChance; break;
                    case StatType.HealingModifier: equipSum += equip.Definition.HealingModifier; break;
                    case StatType.ImmunityToStatus: equipSum += equip.Definition.ImmunityToStatus; break;
                }
            }

            double doctrineBonus = 0.0;
            if (_doctrineService != null)
            {
                switch (statType)
                {
                    // War: CONDITIONED_REFLEXES -> bonusCounterattack() * 0.01
                    case StatType.Counterattack: doctrineBonus = _doctrineService.GetAggregateAbilityValue("CONDITIONED_REFLEXES") * 0.01; break;
                    // Ruin: EXPOSE_WEAKNESS -> bonusCritChance() * 0.01
                    case StatType.CriticalChance: doctrineBonus = _doctrineService.GetAggregateAbilityValue("EXPOSE_WEAKNESS") * 0.01; break;
                    // Ruin: EXPLOIT_WEAKNESS -> bonusCritDamage() * 0.01
                    case StatType.CriticalDamage: doctrineBonus = _doctrineService.GetAggregateAbilityValue("EXPLOIT_WEAKNESS") * 0.01; break;
                    // Illusion: EPHEMERAL_PRESENCE -> bonusDodgeChance() * 0.01
                    case StatType.FlatDodgeChance: doctrineBonus = _doctrineService.GetAggregateAbilityValue("EPHEMERAL_PRESENCE") * 0.01; break;
                    // Grace: SELFLESS_SPIRIT -> bonusHealingModifier() * 0.01
                    case StatType.HealingModifier: doctrineBonus = _doctrineService.GetAggregateAbilityValue("SELFLESS_SPIRIT") * 0.01; break;
                    // Control: IMPENETRABLE_WILLPOWER -> bonusStatusImmunity() * 0.01
                    case StatType.ImmunityToStatus: doctrineBonus = _doctrineService.GetAggregateAbilityValue("IMPENETRABLE_WILLPOWER") * 0.01; break;
                }
            }

            // Java: calculateCriticalChance() base is min(0.4, dex_or_int * 0.004), not raw equipment.
            if (statType == StatType.CriticalChance)
            {
                bool isMagic = character.Weapon?.Definition?.ItemType == "Staff";
                int scalingStat = isMagic ? GetTotalStat(character, StatType.Intelligence) : GetTotalStat(character, StatType.Dexterity);
                baseValue = Math.Min(0.4, scalingStat * 0.004);
            }

            return baseValue + equipSum + doctrineBonus;
        }

        // Java: it.paranoidsquirrels.idleguildmaster.storage.data.entities.adventurers.Trait —
        // 20-value enum: 3 common (BOOKWORM/BRUTE/FERAL) + their 3 "_PLUS" premium-common
        // variants + 14 rare. "STOUT" and "KEEN_EYED" (removed here in Phase 1) do not exist in
        // that enum at all — they were fabricated in a prior pass (flagged in
        // Docs/Backend_Audit/traits_audit.md). "NIMBLE" (also removed here) is real, but in Java
        // it only grants +0.08 flat dodge chance (Adventurer.calculateTotalFlatDodgeChance) — it
        // never touches Dexterity, so mapping it to a DEX multiplier was also wrong; flat dodge
        // chance is a Combat-system stat, deferred to Phase 2.
        // The exact _PLUS multiplier value is NOT visible anywhere in the decompiled source
        // (Adventurer.calculateTotalStat's body is stripped/undecompilable) — reusing the base
        // 1.15 for the _PLUS variant is the least speculative option (same stat, same direction,
        // no evidence of a different number) but is explicitly NOT a confirmed Java value.
        private double GetTraitMultiplier(string trait, StatType statType)
        {
            if (string.IsNullOrEmpty(trait)) return 1.0;
            switch (trait.ToUpperInvariant())
            {
                case "BRUTE":
                case "BRUTE_PLUS":
                    return statType == StatType.Constitution ? 1.15 : 1.0;
                case "BOOKWORM":
                case "BOOKWORM_PLUS":
                    return statType == StatType.Intelligence ? 1.15 : 1.0;
                case "FERAL":
                case "FERAL_PLUS":
                    return statType == StatType.Dexterity ? 1.15 : 1.0;
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
