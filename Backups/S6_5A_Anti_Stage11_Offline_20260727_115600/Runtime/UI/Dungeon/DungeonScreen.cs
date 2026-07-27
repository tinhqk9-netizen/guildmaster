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

        public void Initialize(IDungeonService dungeonService)
        {
            _dungeonService = dungeonService;
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
    }
}
