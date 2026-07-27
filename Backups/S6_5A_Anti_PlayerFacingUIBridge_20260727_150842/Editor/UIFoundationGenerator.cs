#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using GuildMaster.Runtime.UI.Core;

namespace GuildMaster.Editor.UI
{
    public static class UIFoundationGenerator
    {
        [MenuItem("GuildMaster/Generate UI Foundation")]
        public static void GenerateFoundation()
        {
            // 1. Create Canvas
            GameObject canvasObj = new GameObject("UICanvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920); // Mobile portrait
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();
            canvasObj.layer = LayerMask.NameToLayer("UI");

            // 2. Create / repair Event System (correct input module for the active input backend)
            EnsureEventSystem();

            // 3. Create Safe Area
            GameObject safeAreaObj = new GameObject("SafeArea");
            safeAreaObj.transform.SetParent(canvasObj.transform, false);
            var safeAreaRect = safeAreaObj.AddComponent<RectTransform>();
            safeAreaRect.anchorMin = Vector2.zero;
            safeAreaRect.anchorMax = Vector2.one;
            safeAreaRect.sizeDelta = Vector2.zero;
            safeAreaObj.AddComponent<SafeArea>();
            safeAreaObj.layer = LayerMask.NameToLayer("UI");

            // 4. Create Roots
            CreateRoot(safeAreaObj.transform, "ScreenRoot");
            CreateRoot(safeAreaObj.transform, "HudRoot");
            CreateRoot(safeAreaObj.transform, "PopupRoot");
            CreateRoot(safeAreaObj.transform, "OverlayRoot");

            Selection.activeGameObject = canvasObj;
            Debug.Log("[UIFoundation] Generated UI Canvas Foundation.");
        }

        /// <summary>
        /// Ensures a single EventSystem exists with the input module that matches the active
        /// input backend. The project uses the Input System package, so the legacy
        /// StandaloneInputModule (which throws every frame and blocks UI clicks) is replaced
        /// with InputSystemUIInputModule.
        /// </summary>
        [MenuItem("GuildMaster/Fix EventSystem Input Module")]
        public static void FixEventSystem()
        {
            EnsureEventSystem();
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
            Debug.Log("[UIFoundation] EventSystem input module verified for the active input backend.");
        }

        private static void EnsureEventSystem()
        {
            var es = Object.FindFirstObjectByType<EventSystem>();
            if (es == null)
            {
                var go = new GameObject("EventSystem");
                es = go.AddComponent<EventSystem>();
            }

            // Legacy module throws under the Input System package and eats UI clicks — remove it.
            var legacy = es.GetComponent<StandaloneInputModule>();
            if (legacy != null) Object.DestroyImmediate(legacy);

#if ENABLE_INPUT_SYSTEM
            if (es.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>() == null)
            {
                es.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }
#else
            if (es.GetComponent<StandaloneInputModule>() == null)
            {
                es.gameObject.AddComponent<StandaloneInputModule>();
            }
#endif
        }

        private static void CreateRoot(Transform parent, string name)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(parent, false);
            var rect = root.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            root.layer = LayerMask.NameToLayer("UI");
        }
    }
}
#endif
