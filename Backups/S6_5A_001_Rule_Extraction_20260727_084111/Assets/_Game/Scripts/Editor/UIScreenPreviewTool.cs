#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using GuildMaster.Runtime.UI;
using GuildMaster.Runtime.UI.Core;

namespace GuildMaster.Editor.UI
{
    /// <summary>
    /// Edit-time preview toggles for the UIScreens in the open scene.
    ///
    /// Every UI object already lives in Main.unity, but UIWiringGenerator leaves the screens
    /// deactivated, so they are invisible in the Scene view until you press Play. These menu
    /// items flip that state while authoring, which is safe at runtime: UIService.RegisterScreen
    /// hides every screen when it registers, so play mode always re-normalises the state
    /// regardless of what was left enabled in the scene file.
    ///
    /// This tool only toggles activation — it never creates, deletes, or rewires anything.
    /// </summary>
    public static class UIScreenPreviewTool
    {
        /// <summary>
        /// Authoring default: turns every screen on and writes it into the scene file, so opening
        /// Main.unity always shows the full UI without pressing Play. Unlike the Preview items
        /// below, this one saves the scene — the state is meant to persist.
        /// </summary>
        [MenuItem("GuildMaster/UI/Make All Panels Visible In Scene", priority = 0)]
        public static void MakeAllPanelsVisibleInScene()
        {
            UIScreen[] screens = FindScreens();
            if (screens.Length == 0)
            {
                Debug.LogWarning("[UIScreenPreview] No UIScreen found in the open scene. Open Assets/_Game/Scenes/Main.unity first.");
                return;
            }

            const string label = "Make All Panels Visible In Scene";
            foreach (UIScreen screen in screens)
            {
                Undo.RecordObject(screen.gameObject, label);
                screen.gameObject.SetActive(true);
                EditorUtility.SetDirty(screen.gameObject);
            }

            MarkSceneDirty();
            EditorSceneManager.SaveOpenScenes();

            string names = string.Join(", ", screens.Select(s => s.gameObject.name));
            Debug.Log($"[UIScreenPreview] Made {screens.Length} panel(s) visible and saved the scene: {names}. " +
                      "Disable individual panels from the Hierarchy as needed — play mode re-normalises this automatically.");
        }

        [MenuItem("GuildMaster/UI/Preview: Show All Screens", priority = 100)]
        public static void ShowAllScreens()
        {
            SetScreens(active: true, "Show All Screens");
        }

        [MenuItem("GuildMaster/UI/Preview: Reset (Hide All Screens)", priority = 101)]
        public static void HideAllScreens()
        {
            SetScreens(active: false, "Reset (Hide All Screens)");
        }

        [MenuItem("GuildMaster/UI/Preview: Only Inventory", priority = 120)]
        public static void OnlyInventory() => ShowOnly(UIScreenId.Inventory);

        [MenuItem("GuildMaster/UI/Preview: Only Character", priority = 121)]
        public static void OnlyCharacter() => ShowOnly(UIScreenId.Character);

        [MenuItem("GuildMaster/UI/Preview: Only Dungeon", priority = 122)]
        public static void OnlyDungeon() => ShowOnly(UIScreenId.Dungeon);

        [MenuItem("GuildMaster/UI/Preview: Only Craft", priority = 123)]
        public static void OnlyCraft() => ShowOnly(UIScreenId.Craft);

        [MenuItem("GuildMaster/UI/Preview: Only Merchant", priority = 124)]
        public static void OnlyMerchant() => ShowOnly(UIScreenId.Merchant);

        [MenuItem("GuildMaster/UI/Preview: Only Settings", priority = 125)]
        public static void OnlySettings() => ShowOnly(UIScreenId.Settings);

        private static UIScreen[] FindScreens()
        {
            return Object.FindObjectsByType<UIScreen>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        }

        private static void SetScreens(bool active, string label)
        {
            UIScreen[] screens = FindScreens();
            if (screens.Length == 0)
            {
                Debug.LogWarning("[UIScreenPreview] No UIScreen found in the open scene. Open Assets/_Game/Scenes/Main.unity first.");
                return;
            }

            foreach (UIScreen screen in screens)
            {
                Undo.RecordObject(screen.gameObject, label);
                screen.gameObject.SetActive(active);
                EditorUtility.SetDirty(screen.gameObject);
            }

            MarkSceneDirty();
            Debug.Log($"[UIScreenPreview] {label}: {screens.Length} screen(s) set to active={active}. " +
                      "Play mode re-normalises this automatically.");
        }

        private static void ShowOnly(UIScreenId screenId)
        {
            UIScreen[] screens = FindScreens();
            if (screens.Length == 0)
            {
                Debug.LogWarning("[UIScreenPreview] No UIScreen found in the open scene. Open Assets/_Game/Scenes/Main.unity first.");
                return;
            }

            if (screens.All(s => s.ScreenId != screenId))
            {
                Debug.LogWarning($"[UIScreenPreview] No UIScreen with ScreenId '{screenId}' in the open scene.");
                return;
            }

            string label = $"Preview Only {screenId}";
            foreach (UIScreen screen in screens)
            {
                bool shouldShow = screen.ScreenId == screenId;
                Undo.RecordObject(screen.gameObject, label);
                screen.gameObject.SetActive(shouldShow);
                EditorUtility.SetDirty(screen.gameObject);
            }

            MarkSceneDirty();
            Debug.Log($"[UIScreenPreview] Showing only '{screenId}'. Play mode re-normalises this automatically.");
        }

        private static void MarkSceneDirty()
        {
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }
    }
}
#endif
