using System;
using System.IO;
using System.Linq;
using System.Text;
using GuildMaster.Database;
using GuildMaster.Infrastructure.DataProviders;
using GuildMaster.Infrastructure.Serialization;
using GuildMaster.Runtime.Save;
using GuildMaster.Runtime.Services;
using GuildMaster.Tools.Developer;
using UnityEditor;
using UnityEngine;
using SystemRandom = System.Random;

namespace GuildMaster.Editor.Tools
{
    /// <summary>
    /// DEV-only onboarding reset. The reset is deliberately explicit and always backs up the
    /// current local save before replacing it with the clean testing state.
    /// </summary>
    public sealed class NewPlayerStateResetWindow : EditorWindow
    {
        private const string MenuRoot = "Tools/GuildMaster/Developer/Reset New Player State";

        [MenuItem(MenuRoot)]
        public static void Open()
        {
            GetWindow<NewPlayerStateResetWindow>("New Player State").Show();
        }

        [MenuItem(MenuRoot + "/Reset Now")]
        public static void ResetNowMenu() => ResetNow();

        private void OnGUI()
        {
            EditorGUILayout.LabelField("GuildMaster Developer State", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Replaces the local save with: 100 gold, one recruited Footman, and one non-Footman Tavern visitor. " +
                "The current save is backed up before reset.",
                MessageType.Warning);

            if (GUILayout.Button("Reset New Player State", GUILayout.Height(32f)))
                ResetNow();
        }

        private static void ResetNow()
        {
            if (!EnsureEditMode()) return;
            if (!EditorUtility.DisplayDialog(
                    "Reset New Player State",
                    "This replaces the current local save. A backup will be created first. Continue?",
                    "Reset",
                    "Cancel")) return;

            string backupPath = BackupCurrentSave();
            var database = BuildDatabase();
            var save = LoadSave();
            if (save == null) return;

            try
            {
                var services = NewPlayerStateResetter.ResetToNewPlayerState(database, save, new SystemRandom());
                string report = BuildValidationReport(services, backupPath);
                Debug.Log("[NewPlayerStateReset]\n" + report);
                EditorUtility.DisplayDialog("New Player State", report, "OK");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NewPlayerStateReset] Failed: {ex.Message}\n{ex.StackTrace}");
                EditorUtility.DisplayDialog("New Player State", $"Reset failed:\n{ex.Message}", "OK");
            }
        }

        private static string BuildValidationReport(ServiceContainer services, string backupPath)
        {
            var data = services.Save.CurrentData;
            var hero = services.Character.GetAllCharacters().SingleOrDefault();
            var visitor = data.TavernGuests.SingleOrDefault();
            var equippedId = hero?.Weapon?.InstanceId;
            bool equippedVisible = !string.IsNullOrEmpty(equippedId) &&
                                    services.Inventory.GetAllItems().Any(item => item.InstanceId == equippedId);

            var sb = new StringBuilder();
            sb.AppendLine("PLAYER STATE");
            sb.AppendLine($"Gold: {data.Money}");
            sb.AppendLine();
            sb.AppendLine("STARTING HERO");
            sb.AppendLine($"Owned Heroes: {data.Characters.Count}");
            sb.AppendLine($"Hero Classes: {string.Join(", ", data.Characters.Select(c => c?.DefinitionId ?? "<null>"))}");
            sb.AppendLine($"Hero Weapon: {hero?.Weapon?.Definition?.id ?? "<none>"}");
            sb.AppendLine($"Equipped Weapon Instance: {equippedId ?? "<none>"}");
            sb.AppendLine($"Equipped Weapon Visible In Inventory: {equippedVisible}");
            sb.AppendLine();
            sb.AppendLine("FIRST TAVERN VISITOR");
            sb.AppendLine($"Tavern Visitors: {data.TavernGuests.Count}");
            sb.AppendLine($"Visitor Class: {visitor?.DefinitionId ?? "<none>"}");
            sb.AppendLine($"Visitor Weapon Instance: {visitor?.WeaponInstanceId ?? "<none>"}");
            sb.AppendLine();
            sb.AppendLine("VALIDATION");
            sb.AppendLine($"Visible Inventory Count: {services.Inventory.GetAllItems().Count}");
            sb.AppendLine($"Persisted Item Records: {data.Items.Count}");
            sb.AppendLine($"Active Dungeon: {(data.ActiveDungeon == null ? "None" : data.ActiveDungeon.DungeonDefinitionId)}");
            sb.AppendLine($"Active Expeditions: {data.ActiveExpeditions.Count}");
            sb.AppendLine($"Active Raid: {(data.ActiveRaid == null ? "None" : data.ActiveRaid.DefinitionId)}");
            sb.AppendLine($"TutorialStep: {data.TutorialStep}");
            sb.AppendLine($"Backup: {backupPath}");
            return sb.ToString();
        }

        private static string BackupCurrentSave()
        {
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string backupDirectory = Path.Combine(
                projectRoot,
                "Backup",
                "Developer_New_Player_Reset",
                DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            Directory.CreateDirectory(backupDirectory);

            string saveDirectory = Application.persistentDataPath;
            foreach (string fileName in new[] { "save.json", "save_backup.json" })
            {
                string source = Path.Combine(saveDirectory, fileName);
                if (File.Exists(source)) File.Copy(source, Path.Combine(backupDirectory, fileName), true);
            }

            File.WriteAllText(
                Path.Combine(backupDirectory, "README.txt"),
                "Backup created before Tools/GuildMaster/Developer/Reset New Player State.\n");
            return backupDirectory;
        }

        private static SaveService LoadSave()
        {
            var save = new SaveService();
            if (save.Load(out var error)) return save;
            ShowError($"Could not load current save: {error?.Message ?? "unknown error"}");
            return null;
        }

        private static GameDatabase BuildDatabase()
        {
            var database = new GameDatabase();
            var report = new DatabaseBuilder(
                new EditorExternalGameDataProvider(),
                new UnityJsonSerializer(),
                database).Build();
            if (report.errors.Count > 0)
                Debug.LogWarning($"[NewPlayerStateReset] Database build reported {report.errors.Count} error(s).");
            return database;
        }

        private static bool EnsureEditMode()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode) return true;
            ShowError("Stop Play Mode before resetting the developer save.");
            return false;
        }

        private static void ShowError(string message)
        {
            Debug.LogError($"[NewPlayerStateReset] {message}");
            EditorUtility.DisplayDialog("New Player State", message, "OK");
        }
    }
}
