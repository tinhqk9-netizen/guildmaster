using GuildMaster.Runtime.Save;
using UnityEditor;
using UnityEngine;

namespace GuildMaster.Editor.Tools
{
    /// <summary>
    /// Test-only editor command for adding a small amount of gold to the current local save.
    /// SaveService owns the file path, JSON format, timestamp and backup rotation.
    /// </summary>
    public static class AddTestGoldMenu
    {
        private const long GrantAmount = 100L;

        [MenuItem("Tools/Guild Master/Testing/Add 100 Gold")]
        public static void Add100Gold()
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog(
                    "Add 100 Gold",
                    "Stop Play Mode first, then run this command and start Play Mode again so the runtime reloads the save.",
                    "OK");
                return;
            }

            var saveService = new SaveService();
            if (!saveService.Load(out var loadError))
            {
                string message = loadError != null
                    ? $"Could not load the current save. No gold was added.\n\n{loadError.Message}"
                    : "Could not load the current save. No gold was added.";
                Debug.LogError($"[AddTestGoldMenu] {message}");
                EditorUtility.DisplayDialog("Add 100 Gold", message, "OK");
                return;
            }

            long before = saveService.CurrentData.Money;
            if (before > long.MaxValue - GrantAmount)
            {
                const string message = "Gold is already at the maximum supported value. No gold was added.";
                Debug.LogWarning($"[AddTestGoldMenu] {message}");
                EditorUtility.DisplayDialog("Add 100 Gold", message, "OK");
                return;
            }

            saveService.CurrentData.Money = before + GrantAmount;
            if (!saveService.Save(out var saveError))
            {
                string message = saveError != null
                    ? $"Save failed. Gold was not safely committed.\n\n{saveError.Message}"
                    : "Save failed. Gold was not safely committed.";
                Debug.LogError($"[AddTestGoldMenu] {message}");
                EditorUtility.DisplayDialog("Add 100 Gold", message, "OK");
                return;
            }

            long after = saveService.CurrentData.Money;
            Debug.Log($"[AddTestGoldMenu] Added {GrantAmount}g. Money: {before}g -> {after}g.");
            EditorUtility.DisplayDialog("Add 100 Gold", $"Added {GrantAmount}g.\n\nMoney: {before}g → {after}g.", "OK");
        }
    }
}
