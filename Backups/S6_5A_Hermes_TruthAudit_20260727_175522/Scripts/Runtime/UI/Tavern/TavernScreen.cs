using UnityEngine;
using UnityEngine.UI;
using GuildMaster.Runtime.UI.Core;
using GuildMaster.Runtime.Services;

namespace GuildMaster.Runtime.UI.Tavern
{
    public class TavernScreen : UIScreen
    {
        private ITavernService _tavernService;
        [SerializeField] private Text _statusText;

        public void Initialize(ServiceContainer services)
        {
            _tavernService = services.Tavern;
        }

        public override void Show()
        {
            base.Show();
            Refresh();
        }

        public void Refresh()
        {
            if (_tavernService == null || _statusText == null) return;

            int guestsCount = _tavernService.GetGuests().Count;
            int tavernCap = _tavernService.GetTavernCapacity();
            int quartersCap = _tavernService.GetQuartersCapacity();
            long timer = _tavernService.GetNextVisitorTimerSeconds();

            string content = $"--- TAVERN ---\n";
            content += $"Guests: {guestsCount}/{tavernCap}\n";
            content += $"Quarters Capacity: {quartersCap}\n";
            content += $"Next Visitor in: {timer}s\n\n";

            int idx = 0;
            foreach (var guest in _tavernService.GetGuests())
            {
                content += $"[{idx}] Definition: {guest.DefinitionId} (Level {guest.Level})\n";
                idx++;
            }

            _statusText.text = content;
        }

        public void OnClickRecruit(int index)
        {
            if (_tavernService == null) return;
            if (_tavernService.RecruitGuest(index, out var _))
            {
                Refresh();
            }
        }

        public void OnClickRecruitFirst()
        {
            OnClickRecruit(0);
        }

        public void OnClickSpawnGuest()
        {
            _tavernService?.ProgressVisitorTime(3600); // 1 hour progress to guarantee spawn if not capped
            Refresh();
        }
    }
}
