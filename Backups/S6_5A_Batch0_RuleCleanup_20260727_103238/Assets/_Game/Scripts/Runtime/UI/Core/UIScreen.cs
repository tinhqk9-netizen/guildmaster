using UnityEngine;

namespace GuildMaster.Runtime.UI.Core
{
    public class UIScreen : MonoBehaviour
    {
        public UIScreenId ScreenId;
        public bool IsPopup;

        public virtual void Show()
        {
            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
