using UnityEngine;
using UnityEngine.UI;

namespace GuildMaster.Runtime.UI.Foundation
{
    public class WelcomeModal : MonoBehaviour
    {
        [SerializeField] private Button _startJourneyButton;

        public void Initialize()
        {
            _startJourneyButton.onClick.RemoveAllListeners();
            _startJourneyButton.onClick.AddListener(CloseModal);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        private void CloseModal()
        {
            gameObject.SetActive(false);
        }
    }
}
