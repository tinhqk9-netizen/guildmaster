using System;
using System.Collections.Generic;
using System.IO;
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
    /// Development-only economy verification commands. All injected inventory instances use the
    /// DEV_TEST_ prefix so cleanup cannot remove a player's normal items.
    /// </summary>
    public sealed class EconomyVerificationWindow : EditorWindow
    {
        private const string TestPrefix = "DEV_TEST_";
        private const string MenuRoot = "Tools/GuildMaster/Economy Verification";

        [MenuItem(MenuRoot)]
        public static void Open()
        {
            GetWindow<EconomyVerificationWindow>("Economy Verification").Show();
        }

        [MenuItem(MenuRoot + "/Give First Dungeon Craft Materials")]
        public static void GiveFirstDungeonCraftMaterialsMenu() => GiveFirstDungeonCraftMaterials();

        [MenuItem(MenuRoot + "/Clear Test Materials")]
        public static void ClearTestMaterialsMenu() => ClearTestMaterials();

        [MenuItem(MenuRoot + "/Print Craftable Recipe Report")]
        public static void PrintCraftableRecipeReportMenu() => PrintCraftableRecipeReport();

        [MenuItem(MenuRoot + "/Reset Market and Workshop State")]
        public static void ResetMarketAndWorkshopStateMenu() => ResetMarketAndWorkshopState();

        private Vector2 _scroll;

        private void OnGUI()
        {
            EditorGUILayout.LabelField("DEV_TEST economy helpers", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Commands operate on the local save only. Test items are always prefixed with DEV_TEST_. Stop Play Mode before running a command.",
                MessageType.Info);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.Space(6f);
            if (GUILayout.Button("Give First Dungeon Craft Materials", GUILayout.Height(28f)))
                GiveFirstDungeonCraftMaterials();
            if (GUILayout.Button("Clear Test Materials", GUILayout.Height(28f)))
                ClearTestMaterials();
            if (GUILayout.Button("Print Craftable Recipe Report", GUILayout.Height(28f)))
                PrintCraftableRecipeReport();
            if (GUILayout.Button("Reset Market / Workshop State", GUILayout.Height(28f)))
                ResetMarketAndWorkshopState();
            EditorGUILayout.EndScrollView();
        }

        private static void GiveFirstDungeonCraftMaterials()
        {
            if (!EnsureEditMode()) return;
            var save = LoadSave();
            if (save == null) return;
            var database = BuildDatabase();
            var dungeon = database.GetAll<DungeonDefinition>()
                .Where(definition => definition != null && IsDungeonUnlockedForPlayer(definition, save.CurrentData))
                .OrderBy(definition => string.IsNullOrEmpty(definition.RequiredClearDungeonId) ? 0 : 1)
                .ThenBy(definition => definition.id, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();
            if (dungeon == null)
            {
                ShowError("No dungeon definition was found.");
                return;
            }

            var quantities = CollectCraftMaterialDrops(database, dungeon);
            if (quantities.Count == 0)
            {
                ShowError($"Dungeon '{dungeon.id}' has no resolvable material drops.");
                return;
            }

            foreach (var pair in quantities)
            {
                save.CurrentData.Items.Add(new ItemSaveData
                {
                    DefinitionId = pair.Key,
                    InstanceId = $"{TestPrefix}{pair.Key}_{Guid.NewGuid():N}",
                    StackCount = 20,
                    IsLocked = false
                });
            }

            if (!save.Save(out var error))
            {
                ShowError($"Could not save test materials: {error?.Message ?? "unknown error"}");
                return;
            }

            string summary = string.Join(", ", quantities.Keys.OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
                .Select(id => $"{id} x20"));
            Debug.Log($"[EconomyVerification] Added DEV_TEST materials from dungeon '{dungeon.id}': {summary}");
            EditorUtility.DisplayDialog("Economy Verification", $"Added test materials from {dungeon.id}.\n\n{summary}", "OK");
        }

        private static void ClearTestMaterials()
        {
            if (!EnsureEditMode()) return;
            var save = LoadSave();
            if (save == null) return;

            int before = save.CurrentData.Items.Count;
            save.CurrentData.Items.RemoveAll(item => item != null &&
                !string.IsNullOrEmpty(item.InstanceId) &&
                item.InstanceId.StartsWith(TestPrefix, StringComparison.OrdinalIgnoreCase));
            int removed = before - save.CurrentData.Items.Count;

            if (!save.Save(out var error))
            {
                ShowError($"Could not save cleanup: {error?.Message ?? "unknown error"}");
                return;
            }

            Debug.Log($"[EconomyVerification] Cleared {removed} DEV_TEST material instance(s).");
            EditorUtility.DisplayDialog("Economy Verification", $"Cleared {removed} DEV_TEST material instance(s).", "OK");
        }

        private static void PrintCraftableRecipeReport()
        {
            if (!EnsureEditMode()) return;
            var save = LoadSave();
            if (save == null) return;
            var database = BuildDatabase();
            var lines = new List<string> { "DEV_TEST Craftable Recipe Report", $"Generated: {DateTime.Now:O}", string.Empty };
            var craftable = new List<string>();
            var unavailable = new List<string>();

            foreach (var recipe in database.GetAll<RecipeDefinition>().Where(recipe => recipe != null).OrderBy(recipe => recipe.id))
            {
                var missing = GetMissingIngredients(save.CurrentData, recipe);
                if (missing.Count == 0)
                    craftable.Add($"✓ {recipe.id} -> {recipe.OutputItemId}");
                else
                    unavailable.Add($"- {recipe.id} -> {recipe.OutputItemId} | Missing: {string.Join(", ", missing)}");
            }

            lines.Add("AVAILABLE RECIPES");
            lines.AddRange(craftable.Count > 0 ? craftable : new[] { "(none)" });
            lines.Add(string.Empty);
            lines.Add("UNAVAILABLE RECIPES");
            lines.AddRange(unavailable.Count > 0 ? unavailable : new[] { "(none)" });

            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string reportPath = Path.Combine(projectRoot, "Docs", "Backend_Audit", "DEV_TEST_craftable_recipe_report.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath));
            File.WriteAllLines(reportPath, lines);
            string report = string.Join(Environment.NewLine, lines);
            Debug.Log($"[EconomyVerification] Recipe report written to {reportPath}\n{report}");
            EditorUtility.DisplayDialog("Economy Verification", $"Report written:\n{reportPath}\n\nCraftable: {craftable.Count}\nUnavailable: {unavailable.Count}", "OK");
        }

        private static void ResetMarketAndWorkshopState()
        {
            if (!EnsureEditMode()) return;
            if (!EditorUtility.DisplayDialog("Reset Market / Workshop", "Clear active and completed market/workshop test state?", "Reset", "Cancel")) return;
            var save = LoadSave();
            if (save == null) return;
            save.CurrentData.MarketListings.Clear();
            save.CurrentData.SoldMarketItems.Clear();
            save.CurrentData.WorkshopQueue.Clear();
            save.CurrentData.CompletedWorkshopItems.Clear();
            if (!save.Save(out var error))
            {
                ShowError($"Could not save reset: {error?.Message ?? "unknown error"}");
                return;
            }
            Debug.Log("[EconomyVerification] Market and Workshop state reset.");
            EditorUtility.DisplayDialog("Economy Verification", "Market and Workshop state reset.", "OK");
        }

        private static Dictionary<string, int> CollectCraftMaterialDrops(GameDatabase database, DungeonDefinition dungeon)
        {
            var craftMaterialIds = new HashSet<string>(
                database.GetAll<RecipeDefinition>()
                    .Where(recipe => recipe != null && recipe.Ingredients != null)
                    .SelectMany(recipe => recipe.Ingredients)
                    .Where(ingredient => ingredient != null && !string.IsNullOrEmpty(ingredient.ItemId))
                    .Select(ingredient => ingredient.ItemId),
                StringComparer.OrdinalIgnoreCase);
            var materialIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var allDrops = new List<EnemyDropEntry>();
            foreach (var group in dungeon.EncounterGroups ?? new List<EncounterGroupData>())
            {
                foreach (string enemyId in group.EnemyIds ?? new List<string>())
                {
                    if (database.TryGet<EnemyDefinition>(enemyId, out var enemy) && enemy.DropTable != null)
                        allDrops.AddRange(enemy.DropTable);
                }
            }
            if (dungeon.SearchRoomDrops != null) allDrops.AddRange(dungeon.SearchRoomDrops);

            foreach (var drop in allDrops)
            {
                if (drop == null || string.IsNullOrEmpty(drop.ItemId) ||
                    !database.TryGet<ItemDefinition>(drop.ItemId, out var item) ||
                    item.Category != ItemCategory.Material || !craftMaterialIds.Contains(drop.ItemId)) continue;
                materialIds.Add(drop.ItemId);
            }
            return materialIds.ToDictionary(itemId => itemId, _ => 20, StringComparer.OrdinalIgnoreCase);
        }

        private static bool IsDungeonUnlockedForPlayer(DungeonDefinition dungeon, SaveData data)
        {
            if (dungeon == null || string.IsNullOrEmpty(dungeon.RequiredClearDungeonId)) return true;
            return data?.Dungeons != null && data.Dungeons.Any(c => c != null &&
                string.Equals(c.DefinitionId, dungeon.RequiredClearDungeonId, StringComparison.OrdinalIgnoreCase) &&
                c.MaxProgress >= dungeon.RequiredClearProgress);
        }

        private static List<string> GetMissingIngredients(SaveData data, RecipeDefinition recipe)
        {
            var missing = new List<string>();
            foreach (var ingredient in recipe.Ingredients ?? new List<IngredientData>())
            {
                int owned = data.Items.Where(item => item != null && item.DefinitionId == ingredient.ItemId)
                    .Sum(item => Math.Max(0, item.StackCount));
                if (owned < ingredient.Amount) missing.Add($"{ingredient.ItemId} {owned}/{ingredient.Amount}");
            }
            return missing;
        }

        private static SaveService LoadSave()
        {
            var save = new SaveService();
            if (save.Load(out var error)) return save;
            ShowError($"Could not load save: {error?.Message ?? "unknown error"}");
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
                Debug.LogWarning($"[EconomyVerification] Database build reported {report.errors.Count} error(s).");
            return database;
        }

        private static bool EnsureEditMode()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode) return true;
            ShowError("Stop Play Mode before running an economy verification command.");
            return false;
        }

        private static void ShowError(string message)
        {
            Debug.LogError($"[EconomyVerification] {message}");
            EditorUtility.DisplayDialog("Economy Verification", message, "OK");
        }
    }
}
