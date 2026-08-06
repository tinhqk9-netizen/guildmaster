using UnityEditor;
using UnityEngine;
using GuildMaster.Runtime.UI.Shell;
using UnityEngine.SceneManagement;

namespace GuildMaster.Editor.UI.Headquarters
{
    public static class QuartersDialogWirer
    {
        [MenuItem("Tools/Guild Master/Phase 5/Wire Quarters Dialog")]
        public static void WireQuartersDialog()
        {
            var controller = Object.FindAnyObjectByType<HeadquartersHubController>();
            if (controller == null)
            {
                Debug.LogError("Could not find HeadquartersHubController in scene");
                return;
            }

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/UI/Headquarters/QuartersDialog.prefab");
            if (prefab == null)
            {
                Debug.LogError("Could not find QuartersDialog.prefab");
                return;
            }

            var so = new SerializedObject(controller);
            so.FindProperty("_quartersDialogPrefab").objectReferenceValue = prefab;
            so.ApplyModifiedProperties();

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

            Debug.Log("QuartersDialog successfully wired!");
        }
    }
}
