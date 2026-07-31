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
        private readonly GameDatabase _database;
        private readonly Random _random = new Random();

        public TavernService(
            ISaveService saveService, 
            IFormulaService formulaService, 
            ICharacterService characterService, 
            GameDatabase database)
        {
            _saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
            _formulaService = formulaService ?? throw new ArgumentNullException(nameof(formulaService));
            _characterService = characterService ?? throw new ArgumentNullException(nameof(characterService));
            _database = database ?? throw new ArgumentNullException(nameof(database));
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
            guests.RemoveAt(index);

            newCharacter = _characterService.RecruitCharacter(guestData);
            return true;
        }

        private string RollClass()
        {
            double rand = _random.NextDouble();
            return rand < 0.25d ? "Footman" : rand < 0.5d ? "Rogue" : rand < 0.75d ? "Archer" : "Apprentice";
        }

        private string RollCommonTrait()
        {
            double rand = _random.NextDouble();
            if (rand < 0.13333333333333333d) return "BOOKWORM";
            if (rand < 0.26666666666666666d) return "BRUTE";
            if (rand < 0.4d) return "FERAL";
            return null;
        }

        private string RollRareTrait()
        {
            double rand = _random.NextDouble();
            if (rand < 0.014285714285714285d) return "EMPATHETIC";
            if (rand < 0.028571428571428574d) return "GIFTED";
            if (rand < 0.04285714285714286d) return "INTIMIDATING";
            if (rand < 0.05714285714285715d) return "FOCUSED";
            if (rand < 0.07142857142857144d) return "DRAGON_BLOOD";
            if (rand < 0.08571428571428572d) return "CURSED";
            if (rand < 0.1d) return "REACTIVE";
            return null;
        }



        public void GenerateVisitor()
        {
            var data = _saveService.CurrentData;
            string classId = "Footman";
            string trait = null;

            if (data.TutorialStep <= 1)
            {
                classId = "Footman";
            }
            else if (data.TutorialStep == 6)
            {
                classId = "LightDisciple";
                trait = "BOOKWORM";
            }
            else if (data.TutorialStep == 7)
            {
                classId = "Archer";
                trait = "FERAL";
                data.TutorialStep = 8;
                // AchievementsUtils.unlock(AchievementsUtils.ACHIEVEMENT_GUILD_MANAGEMENT_101);
            }
            else
            {
                classId = RollClass();
                trait = RollCommonTrait();
                if (trait == null) trait = RollRareTrait();
            }

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
                Trait = trait ?? string.Empty
            };

            string defaultWeapon = def.StarterWeaponId;
            if (!string.IsNullOrEmpty(defaultWeapon))
            {
                var weaponItem = new ItemSaveData
                {
                    InstanceId = Guid.NewGuid().ToString(),
                    DefinitionId = defaultWeapon,
                    StackCount = 1,
                    IsLocked = true
                };
                data.Items.Add(weaponItem);
                guestData.WeaponInstanceId = weaponItem.InstanceId;
            }

            // Insert at beginning (recovered rule TR-06)
            data.TavernGuests.Insert(0, guestData);

            // Trim if exceeds tavern capacity
            int maxCap = GetTavernCapacity();
            while (data.TavernGuests.Count > maxCap)
            {
                var removedGuest = data.TavernGuests[data.TavernGuests.Count - 1];
                if (!string.IsNullOrEmpty(removedGuest.WeaponInstanceId))
                {
                    data.Items.RemoveAll(x => x.InstanceId == removedGuest.WeaponInstanceId);
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
