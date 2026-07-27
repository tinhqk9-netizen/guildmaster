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

            // Free recruit according to recovered rule TR-03
            newCharacter = _characterService.CreateCharacter(guestData.DefinitionId);
            return true;
        }

        public void GenerateVisitor()
        {
            var allDefs = _database.GetAll<AdventurerDefinition>().ToList();
            if (allDefs.Count == 0) return;

            var selectedDef = allDefs[_random.Next(allDefs.Count)];
            var guestData = new CharacterSaveData
            {
                InstanceId = Guid.NewGuid().ToString(),
                DefinitionId = selectedDef.id,
                Level = 1,
                Exp = 0,
                CurrentHp = selectedDef.BaseMaxHp
            };

            var data = _saveService.CurrentData;
            // Insert at beginning (recovered rule TR-06)
            data.TavernGuests.Insert(0, guestData);

            // Trim if exceeds tavern capacity
            int maxCap = GetTavernCapacity();
            while (data.TavernGuests.Count > maxCap)
            {
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
    }
}
