using UnityEngine;
using UnityEngine.UI;
using GuildMaster.Runtime.UI.Core;
using GuildMaster.Runtime.Services;

namespace GuildMaster.Runtime.UI.Dungeon
{
    public class DungeonScreen : UIScreen
    {
        private IDungeonService _dungeonService;
        [SerializeField] private Text _statusText;

        private ICharacterService _characterService;
        private GuildMaster.Database.GameDatabase _database;

        public void Initialize(ServiceContainer services)
        {
            _dungeonService = services.Dungeon;
            _characterService = services.Character;
            _database = services.Database;
        }

        public override void Show()
        {
            base.Show();
            Refresh();
        }

        public void Refresh()
        {
            if (_dungeonService == null || _statusText == null) return;

            bool isActive = _dungeonService.IsDungeonActive();
            var activeDungeon = _dungeonService.GetActiveDungeon();

            string content = $"--- DUNGEON & COMBAT ---\n";
            content += $"Status: {(isActive ? "EXPLORING" : "IDLE")}\n";

            if (isActive && activeDungeon != null)
            {
                content += $"Dungeon ID: {activeDungeon.Definition?.id}\n";
                content += $"Progress: {activeDungeon.Progress}\n";
                content += $"Action State: {activeDungeon.ActionType} (Turn {activeDungeon.ActionTurnsPassed})\n";
            }

            _statusText.text = content;
        }

        public void OnClickStartFirst()
        {
            if (_dungeonService == null || _characterService == null || _database == null) return;
            var dungeons = _database.GetAll<GuildMaster.Definitions.DungeonDefinition>();
            var chars = _characterService.GetAllCharacters();
            
            if (dungeons != null && dungeons.Count > 0 && chars.Count > 0)
            {
                var party = new System.Collections.Generic.List<string> { chars[0].InstanceId };
                var enumerator = dungeons.GetEnumerator();
                if (enumerator.MoveNext())
                {
                    _dungeonService.StartDungeon(enumerator.Current.id, party);
                }
            }
            Refresh();
        }

        public void OnClickTick1()
        {
            _dungeonService?.Tick();
            Refresh();
        }

        public void OnClickTick10()
        {
            for (int i = 0; i < 10; i++)
            {
                _dungeonService?.Tick();
            }
            Refresh();
        }

        public void OnClickCollectLoot()
        {
            _dungeonService?.CollectDrops();
            Refresh();
        }
    }
}
