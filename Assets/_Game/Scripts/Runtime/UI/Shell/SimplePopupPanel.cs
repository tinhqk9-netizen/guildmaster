using UnityEngine;
using UnityEngine.UI;

namespace GuildMaster.Runtime.UI.Shell
{
    /// <summary>
    /// Phase 4: minimal popup panel used for every building card in this phase — just a title and
    /// a Close button. Real dialog content (Tavern/Storage/Workshop/Shelter) is out of Phase 4
    /// scope; this exists so click → popup → close is provable end-to-end.
    /// </summary>
    public class SimplePopupPanel : MonoBehaviour
    {
        [SerializeField] private Text _titleText;
        [SerializeField] private Button _closeButton;

        /// <summary>Identifies which building this popup instance represents, for the singleton guard.</summary>
        public string FeatureId { get; private set; }

        public void Setup(string featureId, string title, System.Action onClose)
        {
            FeatureId = featureId;
            if (_titleText != null) _titleText.text = title;

            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveAllListeners();
                _closeButton.onClick.AddListener(() => onClose?.Invoke());
            }
        }
    }
}
