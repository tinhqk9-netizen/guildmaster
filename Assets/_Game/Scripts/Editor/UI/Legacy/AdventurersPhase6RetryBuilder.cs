using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using GuildMaster.Runtime.UI.Character;
using GuildMaster.Runtime.UI.Shell;

namespace GuildMaster.Editor.UI.Legacy
{
    /// <summary>Only wires the independent Phase 6 controller to Tab_Adventurers.</summary>
    public static class AdventurersPhase6RetryBuilder
    {
        [MenuItem("Tools/Guild Master/Legacy UI/Phase 6 Retry/Apply Adventurers Roster")]
        public static void Apply()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Do not run the Phase 6 builder in Play Mode.");

            var scene = EditorSceneManager.GetActiveScene();
            if (scene.path != "Assets/_Game/Scenes/Main.unity")
                throw new InvalidOperationException($"Active scene must be Main.unity, got '{scene.path}'.");

            var tab = GameObject.Find("Tab_Adventurers");
            if (tab == null) throw new InvalidOperationException("Tab_Adventurers not found.");

            var placeholder = tab.GetComponent<TabPlaceholderView>();
            if (placeholder != null) Undo.DestroyObjectImmediate(placeholder);

            var components = tab.GetComponents<AdventurersTabController>();
            for (int i = 1; i < components.Length; i++) Undo.DestroyObjectImmediate(components[i]);
            if (tab.GetComponent<AdventurersTabController>() == null)
                Undo.AddComponent<AdventurersTabController>(tab);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[AdventurersPhase6RetryBuilder] Roster controller wired without changing sibling tabs.");
        }
    }
}
