using UnityEngine;
using UnityEngine.UI;
using GuildMaster.Runtime.UI.Core;
using GuildMaster.Runtime.Services;

namespace GuildMaster.Runtime.UI.Character
{
    public class CharacterScreen : UIScreen
    {
        private ICharacterService _characterService;
        [SerializeField] private Text _characterText;

        public void Initialize(ICharacterService characterService)
        {
            _characterService = characterService;
        }

        public override void Show()
        {
            base.Show();
            Refresh();
        }

        public void Refresh()
        {
            if (_characterService == null || _characterText == null) return;

            var characters = _characterService.GetAllCharacters();
            if (characters == null || characters.Count == 0)
            {
                _characterText.text = "No characters available.";
                return;
            }

            string content = "Characters:\n";
            foreach (var c in characters)
            {
                string defId = c.Definition != null ? c.Definition.id : "Unknown";
                content += $"- [{defId}] Lv.{c.Level} HP:{c.CurrentHp} Exp:{c.Experience}\n";
                content += $"  Wpn:{c.Weapon?.InstanceId} Arm:{c.Armor?.InstanceId} Acc:{c.Accessory?.InstanceId}\n";
            }
            _characterText.text = content;
        }
    }
}
