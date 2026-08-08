using System;
using System.Collections.Generic;
using System.Linq;
using GuildMaster.Database;
using GuildMaster.Definitions;
using GuildMaster.Infrastructure.DataProviders;
using GuildMaster.Infrastructure.Serialization;
using GuildMaster.Runtime.Save;
using UnityEditor;
using UnityEngine;

namespace GuildMaster.Editor.Tools
{
    /// <summary>
    /// Editor-only dungeon resource verification. It reads the player's actual unlock state and
    /// the loaded encounter/search drop tables, then writes isolated DEV_TEST_ inventory records.
    /// It never invents definitions and cleanup never touches non-DEV_TEST_ records.
    /// </summary>
    public sealed class DungeonVerificationWindow : EditorWindow
    {
        private const string TestPrefix = "DEV_TEST_";
        private const string MenuRoot = "Tools/GuildMaster/Dungeon Verification";

        [MenuItem(MenuRoot)]
        public static void Open()
        {
            GetWindow<DungeonVerificationWindow>("Dungeon Verification").Show();
        }

        [MenuItem(MenuRoot + "/Give Unlocked Dungeon Materials")]
        public static void GiveUnlockedDungeonMaterialsMenu() => GiveUnlockedDungeonMaterials();

        [MenuItem(MenuRoot + "/Clear DEV_TEST Dungeon Materials")]
        public static void ClearDevTestDungeonMaterialsMenu() => ClearDevTestDungeonMaterials();

        private Vector2 _scroll;

        private void OnGUI()
        {
            EditorGUILayout.LabelField("DEV_TEST dungeon resource helpers", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Reads every dungeon currently unlocked in the local save and its real enemy/search drop tables. " +
                "Only material definitions are injected, each as x20. Stop Play Mode before running a command.",
                MessageType.Info);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.Space(6f);
            if (GUILayout.Button("Give Unlocked Dungeon Materials", GUILayout.Height(28f)))
                GiveUnlockedDungeonMaterials();
            if (GUILayout.Button("Clear DEV_TEST Dungeon Materials", GUILayout.Height(28f)))
                ClearDevTestDungeonMaterials();
            EditorGUILayout.EndScrollView();
        }

        private static void GiveUnlockedDungeonMaterials()
        {
            if (!EnsureEditMode()) return;
            var save = LoadSave();
            if (save == null) return;
            var database = BuildDatabase();

            var unlocked = database.GetAll<DungeonDefinition>()
                .Where(dungeon => dungeon != null && IsDungeonUnlockedForPlayer(dungeon, save.CurrentData))
                .OrderBy(dungeon => dungeon.id, StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (unlocked.Count == 0)
            {
                ShowError("No unlocked dungeon definitions were found.");
                return;
            }

            var materialIds = CollectUnlockedDungeonMaterialIds(database, unlocked);
            if (materialIds.Count == 0)
            {
                ShowError("Unlocked dungeons have no resolvable material/resource drops.");
                return;
            }

            var added = new List<string>();
            foreach (string materialId in materialIds.OrderBy(id => id, StringComparer.OrdinalIgnoreCase))
            {
                save.CurrentData.Items.Add(new ItemSaveData
                {
                    DefinitionId = materialId,
                    InstanceId = $"{TestPrefix}DUNGEON_MATERIAL_{materialId}_{Guid.NewGuid():N}",
                    StackCount = 20,
                    IsLocked = false
                });
                added.Add($"{materialId} x20");
            }

            if (!save.Save(out var error))
            {
                ShowError($"Could not save dungeon test materials: {error?.Message ?? "unknown error"}");
                return;
            }

            string message = $"Unlocked dungeons: {unlocked.Count}\n" +
                             string.Join(", ", unlocked.Select(dungeon => dungeon.id)) +
                             $"\n\nAdded materials: {added.Count}\n" +
                             string.Join("\n", added);
            Debug.Log($"[DungeonVerification] {message}");
            EditorUtility.DisplayDialog("Dungeon Verification", message, "OK");
        }

        private static void ClearDevTestDungeonMaterials()
        {
            if (!EnsureEditMode()) return;
            var save = LoadSave();
            if (save == null) return;

            int before = save.CurrentData.Items.Count;
            save.CurrentData.Items.RemoveAll(item => item != null &&
                !string.IsNullOrEmpty(item.InstanceId) &&
                item.InstanceId.StartsWith(TestPrefix + "DUNGEON_MATERIAL_", StringComparison.OrdinalIgnoreCase));
            int removed = before - save.CurrentData.Items.Count;

            if (!save.Save(out var error))
            {
                ShowError($"Could not save dungeon material cleanup: {error?.Message ?? "unknown error"}");
                return;
            }

            string message = $"Cleared {removed} DEV_TEST dungeon material instance(s). Real items, pets, characters and currency were not touched.";
            Debug.Log($"[DungeonVerification] {message}");
            EditorUtility.DisplayDialog("Dungeon Verification", message, "OK");
        }

        private static HashSet<string> CollectUnlockedDungeonMaterialIds(
            GameDatabase database, IReadOnlyList<DungeonDefinition> dungeons)
        {
            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (DungeonDefinition dungeon in dungeons)
            {
                var enemyIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (EncounterGroupData group in dungeon.EncounterGroups ?? new List<EncounterGroupData>())
                {
                    foreach (string enemyId in group?.EnemyIds ?? new List<string>())
                        if (!string.IsNullOrEmpty(enemyId)) enemyIds.Add(enemyId);
                }

                // Keep compatibility with any unlocked definition that has not yet received
                // EncounterGroups data; this is the same safe fallback used by DungeonService.
                foreach (string enemyId in dungeon.EnemyIds ?? new List<string>())
                    if (!string.IsNullOrEmpty(enemyId)) enemyIds.Add(enemyId);

                foreach (string enemyId in enemyIds)
                {
                    if (!database.TryGet<EnemyDefinition>(enemyId, out var enemy)) continue;
                    CollectMaterialIds(database, enemy.DropTable, ids);
                }

                CollectMaterialIds(database, dungeon.SearchRoomDrops, ids);
            }

            return ids;
        }

        private static void CollectMaterialIds(GameDatabase database, IEnumerable<EnemyDropEntry> entries,
            HashSet<string> destination)
        {
            foreach (EnemyDropEntry entry in entries ?? Enumerable.Empty<EnemyDropEntry>())
            {
                if (entry == null || string.IsNullOrEmpty(entry.ItemId)) continue;
                if (!database.TryGet<ItemDefinition>(entry.ItemId, out var item)) continue;
                if (IsSafeDungeonMaterial(item)) destination.Add(item.id);
            }
        }

        private static bool IsSafeDungeonMaterial(ItemDefinition item)
        {
            if (item == null || item.Category != ItemCategory.Material) return false;
            if (item.Consumable || item.NotSellable) return false;

            string parent = item.parentClass ?? string.Empty;
            return !parent.Equals("Currency", StringComparison.OrdinalIgnoreCase) &&
                   !parent.Equals("Quest", StringComparison.OrdinalIgnoreCase) &&
                   !parent.Equals("Artifact", StringComparison.OrdinalIgnoreCase) &&
                   !parent.Equals("Unique", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsDungeonUnlockedForPlayer(DungeonDefinition dungeon, SaveData data)
        {
            if (dungeon == null || string.IsNullOrEmpty(dungeon.RequiredClearDungeonId)) return true;
            return data?.Dungeons != null && data.Dungeons.Any(c => c != null &&
                string.Equals(c.DefinitionId, dungeon.RequiredClearDungeonId, StringComparison.OrdinalIgnoreCase) &&
                c.MaxProgress >= dungeon.RequiredClearProgress);
        }

        private static SaveService LoadSave()
        {
            var save = new SaveService();
            if (save.Load(out var error)) return save;
            ShowError($"Could not load save: {error?.Message ?? "unknown load error"}");
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
                Debug.LogWarning($"[DungeonVerification] Database build reported {report.errors.Count} error(s).");
            return database;
        }

        private static bool EnsureEditMode()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode) return true;
            ShowError("Stop Play Mode before running a dungeon verification command.");
            return false;
        }

        private static void ShowError(string message)
        {
            Debug.LogError($"[DungeonVerification] {message}");
            EditorUtility.DisplayDialog("Dungeon Verification", message, "OK");
        }
    }
}
