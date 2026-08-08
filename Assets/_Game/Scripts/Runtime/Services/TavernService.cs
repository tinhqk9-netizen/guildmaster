using System;
using System.Collections.Generic;
using System.Linq;
using GuildMaster.Database;
using GuildMaster.Definitions;
using GuildMaster.Runtime.Core;
using GuildMaster.Runtime.Formulas;
using GuildMaster.Runtime.Models;
using GuildMaster.Runtime.Save;

namespace GuildMaster.Runtime.Services
{
    public class TavernService : ITavernService
    {
        private readonly ISaveService _saveService;
        private readonly IFormulaService _formulaService;
        private readonly ICharacterService _characterService;
        private readonly IInventoryService _inventoryService;
        private readonly GameDatabase _database;
        private readonly ITraitService _traitService;
        private readonly Random _random = new Random();
        private const string StartingHeroClassId = "footman";
        private static readonly string[] VisitorClassPool = { "footman", "rogue", "archer", "apprentice" };

        public TavernService(
            ISaveService saveService,
            IFormulaService formulaService,
            ICharacterService characterService,
            IInventoryService inventoryService,
            GameDatabase database,
            ITraitService traitService = null)
        {
            _saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
            _formulaService = formulaService ?? throw new ArgumentNullException(nameof(formulaService));
            _characterService = characterService ?? throw new ArgumentNullException(nameof(characterService));
            _inventoryService = inventoryService;
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _traitService = traitService ?? new TraitService(database);
        }

        public int GetTavernCapacity()
        {
            var data = _saveService.CurrentData;
            return _formulaService.GetTavernCapacity(data.LevelTavernCapacity, data.UpgradeTavernCapacity, data.GetPurchaseFlags());
        }

        public int GetQuartersCapacity()
        {
            var data = _saveService.CurrentData;
            return _formulaService.GetQuartersCapacity(data.LevelQuarters, data.UpgradeQuarters, data.GetPurchaseFlags());
        }

        public long GetVisitorIntervalSeconds()
        {
            var data = _saveService.CurrentData;
            long intervalMs = _formulaService.GetTavernVisitorInterval(data.LevelTavernTime, data.UpgradeTavernTime);
            return Math.Max(1, intervalMs / 1000);
        }

        public long GetNextVisitorTimerSeconds()
        {
            return Math.Max(0, _saveService.CurrentData.NextTavernVisit);
        }

        public IReadOnlyList<CharacterSaveData> GetGuests()
        {
            return _saveService.CurrentData.TavernGuests.AsReadOnly();
        }

        public bool CanRecruit()
        {
            int currentOwned = _characterService.GetAllCharacters().Count;
            return GetQuartersCapacity() > currentOwned;
        }

        public bool RecruitGuest(int index, out CharacterRuntime newCharacter)
        {
            newCharacter = null;
            var guests = _saveService.CurrentData.TavernGuests;
            if (index < 0 || index >= guests.Count) return false;

            if (!CanRecruit()) return false;

            var guestData = guests[index];
            if (guestData == null || !_database.TryGet<AdventurerDefinition>(guestData.DefinitionId, out _)) return false;
            guests.RemoveAt(index);

            try
            {
                newCharacter = _characterService.RecruitCharacter(guestData);
                if (newCharacter == null)
                {
                    guests.Insert(index, guestData);
                    return false;
                }

                // Recruitment is a real save boundary in Legacy. The visitor and the new
                // character must survive closing the dialog or a subsequent reload.
                _saveService.Save(out _);
                return true;
            }
            catch
            {
                // Do not silently lose a visitor if CharacterService rejects malformed data.
                guests.Insert(index, guestData);
                throw;
            }
        }

        private string RollClass()
        {
            return VisitorClassPool[_random.Next(VisitorClassPool.Length)];
        }

        // Phase 1: trait rolling moved to TraitService (Docs/Backend_Audit/phase1_audit_report.md).
        // Java's Utils.generateVisitor() rolls traitCommon AND traitRare INDEPENDENTLY for every
        // randomly-generated guest (Adventurer.getInstance(..., rollCommonTrait(), rollRareTrait(),
        // ...)) — a guest can have both at once, or either, or neither. The old code here treated
        // them as mutually exclusive (one Trait field, common OR rare) which was wrong.

        public void GenerateVisitor()
        {
            GenerateVisitorInternal(null);
        }

        public bool CreateInitialStartingHero(out CharacterRuntime newCharacter)
        {
            newCharacter = null;
            GenerateVisitorInternal(StartingHeroClassId);
            return RecruitGuest(0, out newCharacter) && newCharacter != null;
        }

        public void GenerateInitialVisitor()
        {
            var eligibleClasses = VisitorClassPool
                .Where(id => !string.Equals(id, StartingHeroClassId, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            if (eligibleClasses.Length == 0)
                throw new InvalidOperationException("No eligible non-Footman Tavern class exists for the first visitor.");

            // One uniform roll across Rogue, Archer and Apprentice. This method is only used
            // during fresh-save initialization; normal refreshes remain unchanged.
            GenerateVisitorInternal(eligibleClasses[_random.Next(eligibleClasses.Length)]);
        }

#if UNITY_EDITOR
        /// <summary>
        /// Developer-only deterministic class override. It still uses the normal visitor
        /// trait, starter-weapon identity, capacity and save flow. Production Tavern generation
        /// never calls this method.
        /// </summary>
        public void GenerateVisitorForDeveloper(string classId)
        {
            if (string.IsNullOrEmpty(classId) ||
                !VisitorClassPool.Contains(classId, StringComparer.OrdinalIgnoreCase))
                throw new ArgumentException($"Developer visitor class '{classId}' is not in the normal Tavern class pool.", nameof(classId));

            GenerateVisitorInternal(classId);
        }

        public IReadOnlyList<string> GetDeveloperVisitorClassPool()
        {
            return Array.AsReadOnly(VisitorClassPool);
        }
#endif

        private void GenerateVisitorInternal(string forcedClassId)
        {
            var data = _saveService.CurrentData;
            string classId = string.IsNullOrEmpty(forcedClassId) ? RollClass() : forcedClassId;
            string traitCommon = _traitService.RollCommonTrait();
            string traitRare = _traitService.RollRareTrait();

            if (!_database.TryGet<AdventurerDefinition>(classId, out var def))
            {
                // Fallback if ID case mismatch
                var allDefs = _database.GetAll<AdventurerDefinition>().ToList();
                if (allDefs.Count == 0) return;
                def = allDefs.FirstOrDefault(x => x.id.Equals(classId, StringComparison.OrdinalIgnoreCase)) ?? allDefs[0];
            }

            var guestData = new CharacterSaveData
            {
                InstanceId = Guid.NewGuid().ToString(),
                DefinitionId = def.id,
                Level = 1,
                Exp = 0,
                CurrentHp = def.BaseMaxHp,
                TraitCommon = traitCommon ?? string.Empty,
                TraitRare = traitRare ?? string.Empty,
                Trait = traitCommon ?? string.Empty
            };

            string defaultWeapon = def.StarterWeaponId;
            if (!string.IsNullOrEmpty(defaultWeapon) && _database.TryGet<ItemDefinition>(defaultWeapon, out _))
            {
                // Legacy visitors carry the starter-weapon identity, but the item is
                // not owned by the player until recruitment. Creating an ItemRuntime
                // or touching InventoryService here leaks equipment for un-recruited
                // and expired visitors.
                guestData.WeaponInstanceId = Guid.NewGuid().ToString();
            }

            // Insert at beginning (recovered rule TR-06)
            data.TavernGuests.Insert(0, guestData);

            // Trim if exceeds tavern capacity
            int maxCap = GetTavernCapacity();
            while (data.TavernGuests.Count > maxCap)
            {
                var removedGuest = data.TavernGuests[data.TavernGuests.Count - 1];
                if (!string.IsNullOrEmpty(removedGuest.WeaponInstanceId) && _inventoryService != null)
                {
                    var item = _inventoryService.GetItem(removedGuest.WeaponInstanceId);
                    // Clean up only pre-fix leaked visitor items. A current visitor has
                    // no runtime item until recruitment, so this branch is normally a no-op.
                    if (item != null && _inventoryService.GetAllItems().Any(i => i.InstanceId == item.InstanceId))
                    {
                        _inventoryService.RemoveItem(removedGuest.WeaponInstanceId, item.StackCount);
                    }
                }
                data.TavernGuests.RemoveAt(data.TavernGuests.Count - 1);
            }
        }

        public void ProgressVisitorTime(long deltaSeconds)
        {
            if (deltaSeconds <= 0) return;

            var data = _saveService.CurrentData;
            long intervalSeconds = GetVisitorIntervalSeconds();

            if (data.NextTavernVisit <= 0)
            {
                data.NextTavernVisit = intervalSeconds;
            }

            data.NextTavernVisit -= deltaSeconds;
            while (data.NextTavernVisit <= 0)
            {
                GenerateVisitor();
                data.NextTavernVisit += intervalSeconds;
            }
        }

        public bool UpgradeQuarters()
        {
            var data = _saveService.CurrentData;
            long price = _formulaService.GetQuartersPrice(data.LevelQuarters);
            if (data.Money >= price)
            {
                data.Money -= price;
                data.LevelQuarters++;
                return true;
            }
            return false;
        }

        public bool UpgradeTavernCapacity()
        {
            var data = _saveService.CurrentData;
            long price = _formulaService.GetTavernCapacityPrice(data.LevelTavernCapacity);
            if (data.Money >= price)
            {
                data.Money -= price;
                data.LevelTavernCapacity++;
                return true;
            }
            return false;
        }

        public bool UpgradeTavernTime()
        {
            var data = _saveService.CurrentData;
            long price = _formulaService.GetTavernTimePrice(data.LevelTavernTime);
            if (data.Money >= price)
            {
                data.Money -= price;
                data.LevelTavernTime++;
                return true;
            }
            return false;
        }

        public long GetUpgradeQuartersPrice() => _formulaService.GetQuartersPrice(_saveService.CurrentData.LevelQuarters);
        public long GetUpgradeTavernCapacityPrice() => _formulaService.GetTavernCapacityPrice(_saveService.CurrentData.LevelTavernCapacity);
        public long GetUpgradeTavernTimePrice() => _formulaService.GetTavernTimePrice(_saveService.CurrentData.LevelTavernTime);
        public int GetQuartersLevel() => _saveService.CurrentData.LevelQuarters;
        public int GetTavernCapacityLevel() => _saveService.CurrentData.LevelTavernCapacity;
        public int GetTavernTimeLevel() => _saveService.CurrentData.LevelTavernTime;
    }
}
