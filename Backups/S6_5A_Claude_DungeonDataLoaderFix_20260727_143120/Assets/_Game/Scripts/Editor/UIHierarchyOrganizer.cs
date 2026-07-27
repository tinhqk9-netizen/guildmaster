#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using GuildMaster.Runtime.UI;
using GuildMaster.Runtime.UI.Core;

namespace GuildMaster.Editor.UI
{
    /// <summary>
    /// Groups the root objects of Main.unity by game function so the Hierarchy reads as
    /// Systems / Cameras / UI instead of a flat list, and orders the screens along the
    /// gameplay flow (Character -> Inventory -> Dungeon -> Craft -> Merchant -> Settings).
    ///
    /// Deliberately conservative — it only reparents root objects and reorders siblings:
    ///   * No object is renamed. UIRuntimeBootstrap resolves Btn_Back by name
    ///     (screen.transform.Find("Btn_Back")) and UIWiringGenerator resolves the roots by
    ///     the relative paths "SafeArea/HudRoot", "SafeArea/ScreenRoot", "SafeArea/PopupRoot",
    ///     so renaming either would break them.
    ///   * Nothing inside the Canvas is reparented, so no RectTransform values shift.
    ///     The new parents sit at the scene root with a default Transform, and a
    ///     ScreenSpaceOverlay Canvas ignores its parent transform entirely.
    ///
    /// Runtime lookups keep working because they resolve by component
    /// (FindObjectsByType&lt;UIScreen&gt;, FindFirstObjectByType&lt;HUDController&gt;, ...) and
    /// GameObject.Find scans the whole scene by name regardless of parenting.
    /// </summary>
    public static class UIHierarchyOrganizer
    {
        private const string SystemsGroup = "Systems";
        private const string CamerasGroup = "Cameras";
        private const string UIGroup = "UI";

        /// <summary>Screen order follows the gameplay flow, not the creation order.</summary>
        private static readonly UIScreenId[] ScreenOrder =
        {
            UIScreenId.Character,
            UIScreenId.Inventory,
            UIScreenId.Dungeon,
            UIScreenId.Craft,
            UIScreenId.Merchant,
            UIScreenId.Settings
        };

        [MenuItem("GuildMaster/UI/Organize Hierarchy By Game Function", priority = 1)]
        public static void OrganizeHierarchy()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                Debug.LogWarning("[UIHierarchyOrganizer] No valid scene open.");
                return;
            }

            int moved = 0;
            moved += GroupRootObject(SystemsGroup, "UIRuntimeBootstrap");
            moved += GroupRootObject(CamerasGroup, "Main Camera");
            moved += GroupRootObject(CamerasGroup, "Global Light 2D");
            moved += GroupRootObject(UIGroup, "UICanvas");
            moved += GroupRootObject(UIGroup, "EventSystem");

            int reordered = OrderScreensByGameFlow();

            OrderGroup(SystemsGroup, 0);
            OrderGroup(CamerasGroup, 1);
            OrderGroup(UIGroup, 2);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveOpenScenes();

            Debug.Log($"[UIHierarchyOrganizer] Grouped {moved} root object(s) into " +
                      $"{SystemsGroup}/{CamerasGroup}/{UIGroup}, reordered {reordered} screen(s), and saved the scene. " +
                      "No object was renamed and nothing inside the Canvas was reparented.");
        }

        /// <summary>Finds (or creates) a root group and moves the named root object under it.</summary>
        private static int GroupRootObject(string groupName, string objectName)
        {
            GameObject target = FindRootObject(objectName);
            if (target == null)
            {
                Debug.LogWarning($"[UIHierarchyOrganizer] '{objectName}' not found at the scene root — skipped.");
                return 0;
            }

            GameObject group = FindRootObject(groupName);
            if (group == null)
            {
                group = new GameObject(groupName);
                Undo.RegisterCreatedObjectUndo(group, $"Create {groupName} group");
            }

            if (target.transform.parent == group.transform) return 0;

            Undo.SetTransformParent(target.transform, group.transform, $"Group {objectName}");
            return 1;
        }

        /// <summary>Sorts the screens under ScreenRoot into gameplay order.</summary>
        private static int OrderScreensByGameFlow()
        {
            UIScreen[] screens = Object.FindObjectsByType<UIScreen>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int count = 0;

            for (int i = 0; i < ScreenOrder.Length; i++)
            {
                foreach (UIScreen screen in screens)
                {
                    if (screen.ScreenId != ScreenOrder[i] || screen.IsPopup) continue;

                    Undo.RecordObject(screen.transform, "Reorder screens");
                    screen.transform.SetSiblingIndex(i);
                    count++;
                    break;
                }
            }

            return count;
        }

        private static void OrderGroup(string groupName, int index)
        {
            GameObject group = FindRootObject(groupName);
            if (group != null) group.transform.SetSiblingIndex(index);
        }

        private static GameObject FindRootObject(string name)
        {
            foreach (GameObject root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (root.name == name) return root;
            }
            return null;
        }
    }
}
#endif
