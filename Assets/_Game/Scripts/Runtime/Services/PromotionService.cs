using System;
using System.Collections.Generic;
using System.Linq;
using GuildMaster.Database;
using GuildMaster.Definitions;
using GuildMaster.Runtime.Save;
using UnityEngine;

namespace GuildMaster.Runtime.Services
{
    /// <summary>
    /// Restores the real Legacy promotion/ascension flow (Docs/Backend_Audit/phase1_audit_report.md).
    ///
    /// Java ground truth (DialogEntityDetail.dialogAdventurerPromotion/promote/ascend,
    /// DialogPromotionChoices.java):
    ///   - A hero can promote once character.Level reaches its CURRENT class's MaxLevel.
    ///   - Promotion choices are exactly AdventurerDefinition.NextClasses (declarative per class,
    ///     e.g. Apprentice -> LightDisciple or Adept). No item is required.
    ///   - Promoting changes the class (DefinitionId) outright and resets Level=1, Experience=0
    ///     (Adventurer.getInstance(newClass, id, 1, 0, ...)). Weapon/Armor/Accessory/traits/
    ///     potions/doctrine carry over unchanged.
    ///   - A class with an EMPTY NextClasses list (e.g. Balrog, MaxLevel 45) is a final tier.
    ///     Reaching MaxLevel there triggers "ascend" instead of "promote": the hero resets back
    ///     to its BASE class (Utils.getBaseClass — derived from weapon type: bow->Archer,
    ///     dagger->Rogue, staff->Apprentice, else->Footman), Level=1, Experience=0, and gains the
    ///     permanent `ascended` flag (IsAscended) which grants a recovered +50% CON/INT/DEX/HP
    ///     stat bonus (see CharacterService.GetTotalStat) and unlocks Doctrine
    ///     (Adventurer.canPickDoctrine()). Ascension is repeatable (the hero can climb the tree
    ///     and ascend again; the flag simply stays true).
    /// </summary>
    public interface IPromotionService
    {
        /// <summary>Real promotion choices (full next-class definitions) for this character, or empty if not eligible.</summary>
        IReadOnlyList<AdventurerDefinition> GetPromotionChoices(CharacterSaveData character);
        bool CanPromote(CharacterSaveData character);
        bool CanPromoteTo(CharacterSaveData character, string targetDefinitionId);
        bool Promote(CharacterSaveData character, string targetDefinitionId);

        /// <summary>True once the character is at a final-tier class (no NextClasses) at MaxLevel.</summary>
        bool CanAscend(CharacterSaveData character);
        bool Ascend(CharacterSaveData character);
    }

    public class PromotionService : IPromotionService
    {
        private readonly ISaveService _saveService;
        private readonly GameDatabase _database;
        private readonly ICharacterService _characterService;

        public PromotionService(ISaveService saveService, GameDatabase database, IInventoryService inventoryService, ICharacterService characterService)
        {
            _saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
            _database = database ?? throw new ArgumentNullException(nameof(database));
            // inventoryService is intentionally unused: Legacy's standard promotion tree consumes
            // no item (see class doc). Kept as a constructor parameter for DI-container compatibility.
            _characterService = characterService ?? throw new ArgumentNullException(nameof(characterService));
        }

        private AdventurerDefinition GetCurrentDefinition(CharacterSaveData character)
        {
            if (character == null) return null;
            _database.TryGet<AdventurerDefinition>(character.DefinitionId, out var def);
            return def;
        }

        public IReadOnlyList<AdventurerDefinition> GetPromotionChoices(CharacterSaveData character)
        {
            var current = GetCurrentDefinition(character);
            if (current == null) return Array.Empty<AdventurerDefinition>();
            if (character.Level < current.MaxLevel) return Array.Empty<AdventurerDefinition>();
            if (current.NextClasses == null || current.NextClasses.Length == 0) return Array.Empty<AdventurerDefinition>();

            var all = _database.GetAll<AdventurerDefinition>();
            var choices = new List<AdventurerDefinition>();
            foreach (var nextClassName in current.NextClasses)
            {
                // NextClasses stores the raw Java class name (e.g. "LightDisciple"), matching
                // DefinitionBase.className exactly — see phase1 extraction in adventurers.json.
                var match = all.FirstOrDefault(d => d.className == nextClassName);
                if (match != null) choices.Add(match);
            }
            return choices.AsReadOnly();
        }

        public bool CanPromote(CharacterSaveData character) => GetPromotionChoices(character).Count > 0;

        public bool CanPromoteTo(CharacterSaveData character, string targetDefinitionId)
        {
            if (string.IsNullOrEmpty(targetDefinitionId)) return false;
            return GetPromotionChoices(character).Any(d => d.id == targetDefinitionId);
        }

        public bool Promote(CharacterSaveData character, string targetDefinitionId)
        {
            if (character == null || !CanPromoteTo(character, targetDefinitionId)) return false;

            bool ok = _characterService.ChangeClass(character.InstanceId, targetDefinitionId, setAscended: false);
            if (!ok) return false;

            _saveService.Save(out _);
            Debug.Log($"[PromotionService] Character '{character.InstanceId}' promoted to '{targetDefinitionId}'.");
            return true;
        }

        public bool CanAscend(CharacterSaveData character)
        {
            var current = GetCurrentDefinition(character);
            if (current == null) return false;
            bool isFinalTier = current.NextClasses == null || current.NextClasses.Length == 0;
            return isFinalTier && character.Level >= current.MaxLevel;
        }

        public bool Ascend(CharacterSaveData character)
        {
            if (character == null || !CanAscend(character)) return false;

            var current = GetCurrentDefinition(character);
            string baseClassName = GetBaseClassName(current.WeaponType);
            var baseDef = _database.GetAll<AdventurerDefinition>().FirstOrDefault(d => d.className == baseClassName);
            if (baseDef == null) return false;

            bool ok = _characterService.ChangeClass(character.InstanceId, baseDef.id, setAscended: true);
            if (!ok) return false;

            _saveService.Save(out _);
            Debug.Log($"[PromotionService] Character '{character.InstanceId}' ascended, reset to base class '{baseDef.id}'.");
            return true;
        }

        // Java: Utils.getBaseClass(Adventurer) — it.paranoidsquirrels.idleguildmaster.Utils.java.
        private static string GetBaseClassName(string weaponType)
        {
            switch (weaponType)
            {
                case "Bow": return "Archer";
                case "Dagger": return "Rogue";
                case "Staff": return "Apprentice";
                default: return "Footman";
            }
        }
    }
}
