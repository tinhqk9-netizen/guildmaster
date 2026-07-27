using UnityEngine;

namespace GuildMaster.Runtime.UI.Core
{
    public class SafeArea : MonoBehaviour
    {
        private RectTransform _panel;
        private Rect _lastSafeArea = new Rect(0, 0, 0, 0);

        void Awake()
        {
            _panel = GetComponent<RectTransform>();
            ApplySafeArea();
        }

        void Update()
        {
            if (_lastSafeArea != Screen.safeArea)
            {
                ApplySafeArea();
            }
        }

        void ApplySafeArea()
        {
            _lastSafeArea = Screen.safeArea;
            var anchorMin = _lastSafeArea.position;
            var anchorMax = _lastSafeArea.position + _lastSafeArea.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;
            
            if (_panel != null)
            {
                _panel.anchorMin = anchorMin;
                _panel.anchorMax = anchorMax;
            }
        }
    }
}
