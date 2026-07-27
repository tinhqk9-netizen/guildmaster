using System;
using System.Collections.Generic;
using GuildMaster.Database;
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
            return _formulaService.GetTavernCapacity(data.LevelTavernCapacity, data.UpgradeTavernCapacity);
        }

        public int GetQuartersCapacity()
        {
            var data = _saveService.CurrentData;
            return _formulaService.GetQuartersCapacity(data.LevelQuarters, data.UpgradeQuarters, data.GetPurchaseFlags());
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

        public void ProgressVisitorTime(long deltaSeconds)
        {
            if (deltaSeconds <= 0) return;

            var data = _saveService.CurrentData;
            long intervalMs = _formulaService.GetTavernVisitorInterval(data.LevelTavernTime, data.UpgradeTavernTime);
            long intervalSeconds = Math.Max(1, intervalMs / 1000);

            data.NextTavernVisit -= deltaSeconds;
            if (data.NextTavernVisit <= 0)
            {
                data.NextTavernVisit = intervalSeconds;
            }
        }
    }
}
