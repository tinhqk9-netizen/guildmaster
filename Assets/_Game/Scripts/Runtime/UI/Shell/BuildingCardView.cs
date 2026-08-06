using UnityEngine;
using UnityEngine.UI;

namespace GuildMaster.Runtime.UI.Shell
{
    /// <summary>
    /// Phase 4 (Headquarters Hub): one building card (Quarters/Tavern/Storage/Market/Workshop/
    /// Shelter). Pure view — <see cref="HeadquartersHubController"/> owns all data/click wiring.
    /// </summary>
    public class BuildingCardView : MonoBehaviour
    {
        [SerializeField] private Text _titleText;
        [SerializeField] private Text _statusText;
        [SerializeField] private Image _signIcon;
        [SerializeField] private GameObject _newIndicator;
        [SerializeField] private Button _button;

        public Button Button => _button;

        public void SetData(string title, string status, Sprite signSprite, bool showNew)
        {
            if (_titleText != null) _titleText.text = title;
            if (_statusText != null) _statusText.text = status;
            if (_signIcon != null) _signIcon.sprite = signSprite;
            if (_newIndicator != null) _newIndicator.SetActive(showNew);
        }
    }
}
