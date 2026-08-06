using UnityEngine;
using UnityEngine.UI;

namespace GuildMaster.Runtime.UI.Shell
{
    /// <summary>
    /// Phase 3 (App Shell): trivial placeholder for a main tab's content area — just displays the
    /// tab name. Feature content (Headquarters buildings, Adventurers list, Dungeons list, Raids
    /// list) is explicitly out of scope for Phase 3 and will replace this in a later phase.
    /// Uses UnityEngine.UI.Text (not TextMeshPro) to match the rest of the codebase — no existing
    /// UI screen in this project references TMPro, and the Runtime asmdef does not include it.
    /// </summary>
    public class TabPlaceholderView : MonoBehaviour
    {
        [SerializeField] private Text _label;

        public void SetLabel(string tabName)
        {
            if (_label != null)
            {
                _label.text = $"{tabName}\n(placeholder — Phase 4+)";
            }
        }
    }
}
