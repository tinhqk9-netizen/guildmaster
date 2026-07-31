using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GuildMaster.Runtime.Boot
{
    /// <summary>
    /// Minimal Boot -> Main scene transition for real builds (Standalone/APK), where
    /// Boot.unity is the first scene in Build Settings. Editor Play Mode on Main.unity
    /// directly does not go through this. No composition/service wiring here — that
    /// stays in UIRuntimeBootstrap (S5) inside Main.unity.
    /// </summary>
    public class BootSceneLoader : MonoBehaviour
    {
        [SerializeField] private string _mainSceneName = "Main";

        private void Start()
        {
            SetupFallbackUI();

            // Use LoadSceneAsync so the Boot scene can render at least one frame 
            // of the Canvas (Logo/Loading Text) before freezing to load Main.
            SceneManager.LoadSceneAsync(_mainSceneName);
        }

        private void SetupFallbackUI()
        {
            if (Object.FindFirstObjectByType<Canvas>() != null)
                return; // Scene already has a UI Canvas

            // Create a simple Canvas
            GameObject canvasGo = new GameObject("BootCanvas");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasGo.AddComponent<GraphicRaycaster>();

            // Black Background
            GameObject bgGo = new GameObject("Background");
            bgGo.transform.SetParent(canvasGo.transform, false);
            Image bg = bgGo.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.1f, 1f);
            RectTransform bgRect = bg.rectTransform;
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            // Loading Text
            GameObject textGo = new GameObject("LoadingText");
            textGo.transform.SetParent(canvasGo.transform, false);
            Text text = textGo.AddComponent<Text>();
            text.text = "Loading...";
            
            Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.font = f;
            
            text.fontSize = 64;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            
            RectTransform textRect = text.rectTransform;
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.sizeDelta = new Vector2(600, 200);
            textRect.anchoredPosition = Vector2.zero;
        }
    }
}
