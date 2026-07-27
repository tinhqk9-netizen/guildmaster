using UnityEngine;
using UnityEngine.UI;
using GuildMaster.Runtime.UI.Core;
using GuildMaster.Runtime.Save;

namespace GuildMaster.Runtime.UI.Character
{
    public class CharacterScreen : UIScreen
    {
        private ISaveService _saveService;
        [SerializeField] private Text _characterText;

        public void Initialize(ISaveService saveService)
        {
            _saveService = saveService;
        }

        public override void Show()
        {
            base.Show();
            Refresh();
        }

        public void Refresh()
        {
            if (_saveService == null || _characterText == null) return;
            
            var data = _saveService.CurrentData;
            if (data == null || data.Characters == null || data.Characters.Count == 0)
            {
                _characterText.text = "No characters available.";
                return;
            }

            string content = "Characters:\n";
            foreach (var charData in data.Characters)
            {
                content += $"- [{charData.DefinitionId}] Lv.{charData.Level} HP:{charData.CurrentHp} Exp:{charData.Exp}\n";
                content += $"  Wpn:{charData.WeaponInstanceId} Arm:{charData.ArmorInstanceId} Acc:{charData.AccessoryInstanceId}\n";
            }
            _characterText.text = content;
        }
    }
}
