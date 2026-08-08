using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GuildMaster.Database;
using GuildMaster.Definitions;
using GuildMaster.Infrastructure.DataProviders;
using GuildMaster.Infrastructure.Serialization;
using GuildMaster.Runtime.Models;
using GuildMaster.Runtime.Save;
using GuildMaster.Runtime.Services;
using UnityEditor;
using UnityEngine;

namespace GuildMaster.Editor.Tools
{
    /// <summary>
    /// Editor-only verification commands for the Phase 4 pet pipeline.
    ///
    /// The commands build the same database and ServiceContainer used by runtime, then call
    /// IPetService/InventoryService. They never construct PetSaveData and cleanup is limited to
    /// records whose InstanceId starts with DEV_TEST_.
    /// </summary>
    public sealed class PetVerificationWindow : EditorWindow
    {
        private const string TestPrefix = "DEV_TEST_";
        private const string MenuRoot = "Tools/GuildMaster/Pet Verification";

        private static readonly System.Random Random = new System.Random();
        private static string _lastReport = string.Empty;

        private Vector2 _scroll;

        [MenuItem(MenuRoot)]
        public static void Open()
        {
            GetWindow<PetVerificationWindow>("Pet Verification").Show();
        }

        [MenuItem(MenuRoot + "/Give Random Pet Egg")]
        public static void GiveRandomPetEggMenu() => GiveRandomPetEgg();

        [MenuItem(MenuRoot + "/Hatch Test Egg")]
        public static void HatchTestEggMenu() => HatchTestEgg();

        [MenuItem(MenuRoot + "/Spawn Random Owned Pet")]
        public static void SpawnRandomOwnedPetMenu() => SpawnRandomOwnedPet();

        [MenuItem(MenuRoot + "/Run 10 Hatch Trials")]
        public static void RunTenHatchTrialsMenu() => RunTenHatchTrials();

        [MenuItem(MenuRoot + "/Clear DEV_TEST Pets")]
        public static void ClearDevTestPetsMenu() => ClearDevTestPets();

        [MenuItem(MenuRoot + "/Print Pet Debug Report")]
        public static void PrintPetDebugReportMenu() => PrintPetDebugReport();

        private void OnGUI()
        {
            EditorGUILayout.LabelField("DEV_TEST Pet verification", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Editor-only commands. They use the real PetService pipeline and save to the local save file. " +
                "Stop Play Mode before running a command. Cleanup only removes DEV_TEST_ pets.",
                MessageType.Info);

            EditorGUILayout.Space(6f);
            if (GUILayout.Button("Give Random Pet Egg", GUILayout.Height(28f))) GiveRandomPetEgg();
            if (GUILayout.Button("Hatch Test Egg", GUILayout.Height(28f))) HatchTestEgg();
            if (GUILayout.Button("Spawn Random Owned Pet", GUILayout.Height(28f))) SpawnRandomOwnedPet();
            if (GUILayout.Button("Run 10 Hatch Trials", GUILayout.Height(28f))) RunTenHatchTrials();
            if (GUILayout.Button("Clear DEV_TEST Pets", GUILayout.Height(28f))) ClearDevTestPets();
            if (GUILayout.Button("Print Pet Debug Report", GUILayout.Height(28f))) PrintPetDebugReport();

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Last result", EditorStyles.boldLabel);
            _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.MinHeight(180f));
            EditorGUILayout.TextArea(_lastReport ?? string.Empty, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }

        private static void GiveRandomPetEgg()
        {
            if (!EnsureEditMode()) return;
            if (!TryBuildContext(out var services, out var save, out var database)) return;

            var egg = ChooseUnusedPetEgg(database, services.Inventory);
            if (egg == null)
            {
                ShowError("No safe pet egg is available. A safe test egg requires that the selected egg definition has no real inventory quantity.");
                return;
            }

            string instanceId = $"{TestPrefix}EGG_{Guid.NewGuid():N}";
            try
            {
                services.Inventory.AddItem(new ItemRuntime(instanceId, egg, 1));
            }
            catch (Exception ex)
            {
                ShowError($"Could not add test egg '{egg.id}': {ex.Message}");
                return;
            }

            if (services.Inventory.GetItem(instanceId) == null)
            {
                ShowError($"Inventory did not retain the test egg instance '{instanceId}'. No hatch was attempted.");
                return;
            }

            if (!save.Save(out var error))
            {
                ShowError($"Test egg was not committed: {error?.Message ?? "unknown save error"}");
                return;
            }

            Publish($"Added {egg.id} as {instanceId}.\n\nUse Hatch Test Egg to call PetService.HatchEgg.");
        }

        private static void HatchTestEgg()
        {
            if (!EnsureEditMode()) return;
            if (!TryBuildContext(out var services, out var save, out var database)) return;

            var testEgg = services.Inventory.GetAllItems()
                .FirstOrDefault(item => item != null && IsTestId(item.InstanceId) && IsPetEgg(item.Definition));
            if (testEgg == null)
            {
                ShowError("No DEV_TEST_ pet egg found. Run Give Random Pet Egg first.");
                return;
            }

            if (HasRealEggOfDefinition(services.Inventory, testEgg.Definition.id))
            {
                ShowError($"Refusing to hatch '{testEgg.Definition.id}' because a non-DEV_TEST egg of the same definition exists.");
                return;
            }

            var beforeIds = GetPetIds(services.Pet);
            PetSaveData created;
            try
            {
                created = services.Pet.HatchEgg(testEgg.Definition.id);
            }
            catch (Exception ex)
            {
                ShowError($"PetService.HatchEgg threw: {ex.Message}");
                return;
            }

            if (!TryTagAndValidateNewPet(created, beforeIds, services, database, out var validationError))
            {
                ShowError(validationError);
                return;
            }

            string petId = created.InstanceId;
            if (!save.Save(out var saveError))
            {
                ShowError($"Hatched pet was not committed: {saveError?.Message ?? "unknown save error"}");
                return;
            }

            bool reloaded = ContainsPetAfterReload(petId, out var reloadError);
            Publish(
                $"Hatched {testEgg.Definition.id}.\n" +
                $"Pet: {created.DefinitionId}\n" +
                $"Tier: {GetPetTier(database, created)}\n" +
                $"Ability: {created.Ability1}\n" +
                $"Saved and reload verified: {reloaded}" +
                (reloaded ? string.Empty : $" ({reloadError})"));
        }

        private static void SpawnRandomOwnedPet()
        {
            if (!EnsureEditMode()) return;
            if (!TryBuildContext(out var services, out var save, out var database)) return;

            var definitions = database.GetAll<PetDefinition>()
                .Where(definition => definition != null && !string.IsNullOrEmpty(definition.id))
                .ToList();
            if (definitions.Count == 0)
            {
                ShowError("No pet definitions were loaded.");
                return;
            }

            var definition = definitions[Random.Next(definitions.Count)];
            PetSaveData created;
            try
            {
                created = services.Pet.CreatePet(definition.id);
            }
            catch (Exception ex)
            {
                ShowError($"PetService.CreatePet threw: {ex.Message}");
                return;
            }

            if (created == null)
            {
                ShowError($"PetService.CreatePet could not create '{definition.id}'.");
                return;
            }

            created.InstanceId = $"{TestPrefix}PET_{Guid.NewGuid():N}";
            if (!save.Save(out var error))
            {
                ShowError($"Test pet was not committed: {error?.Message ?? "unknown save error"}");
                return;
            }

            bool reloaded = ContainsPetAfterReload(created.InstanceId, out var reloadError);
            Publish($"Created {created.DefinitionId} through PetService.CreatePet.\n" +
                    $"Tier: {definition.PetTier}\n" +
                    $"Ability: {created.Ability1}\n" +
                    $"Saved and reload verified: {reloaded}" +
                    (reloaded ? string.Empty : $" ({reloadError})"));
        }

        private static void RunTenHatchTrials()
        {
            if (!EnsureEditMode()) return;
            if (!TryBuildContext(out var services, out var save, out var database)) return;

            var tierCounts = new Dictionary<int, int> { { 1, 0 }, { 2, 0 }, { 3, 0 } };
            var petIds = new List<string>();
            var failures = new List<string>();

            for (int trial = 1; trial <= 10; trial++)
            {
                var existingTestEgg = FindExistingTestEgg(services.Inventory);
                var egg = existingTestEgg != null
                    ? existingTestEgg.Definition
                    : ChooseUnusedPetEgg(database, services.Inventory);
                if (egg == null)
                {
                    failures.Add($"Trial {trial}: no safe egg available.");
                    break;
                }

                if (existingTestEgg == null)
                {
                    try
                    {
                        services.Inventory.AddItem(new ItemRuntime(
                            $"{TestPrefix}EGG_{Guid.NewGuid():N}", egg, 1));
                    }
                    catch (Exception ex)
                    {
                        failures.Add($"Trial {trial}: add egg failed: {ex.Message}");
                        break;
                    }
                }

                var testEgg = FindExistingTestEgg(services.Inventory);
                if (testEgg == null || HasRealEggOfDefinition(services.Inventory, testEgg.Definition.id))
                {
                    failures.Add($"Trial {trial}: safety check rejected egg '{egg.id}'.");
                    break;
                }

                var beforeIds = GetPetIds(services.Pet);
                PetSaveData created;
                try
                {
                    created = services.Pet.HatchEgg(testEgg.Definition.id);
                }
                catch (Exception ex)
                {
                    failures.Add($"Trial {trial}: hatch threw: {ex.Message}");
                    break;
                }

                if (!TryTagAndValidateNewPet(created, beforeIds, services, database, out var validationError))
                {
                    failures.Add($"Trial {trial}: {validationError}");
                    break;
                }

                int tier = GetPetTier(database, created);
                if (!tierCounts.ContainsKey(tier)) tierCounts[tier] = 0;
                tierCounts[tier]++;
                petIds.Add(created.InstanceId);
            }

            bool saved = save.Save(out var saveError);
            bool reloaded = saved && petIds.All(id => ContainsPetAfterReload(id, out _));
            var report = new StringBuilder()
                .AppendLine("DEV_TEST Pet Hatch Trials")
                .AppendLine($"Completed: {petIds.Count}/10")
                .AppendLine($"Tier 1: {tierCounts[1]}")
                .AppendLine($"Tier 2: {tierCounts[2]}")
                .AppendLine($"Tier 3: {tierCounts[3]}")
                .AppendLine($"Save: {saved}")
                .AppendLine($"Reload verification: {reloaded}");
            if (!saved) report.AppendLine($"Save error: {saveError?.Message ?? "unknown save error"}");
            foreach (var failure in failures) report.AppendLine($"FAIL: {failure}");
            if (petIds.Count > 0) report.AppendLine("Pet IDs: " + string.Join(", ", petIds));

            Publish(report.ToString());
        }

        private static void ClearDevTestPets()
        {
            if (!EnsureEditMode()) return;
            var save = LoadSave();
            if (save == null) return;

            int before = save.CurrentData.Pets.Count;
            save.CurrentData.Pets.RemoveAll(pet => pet != null && IsTestId(pet.InstanceId));
            int removed = before - save.CurrentData.Pets.Count;
            if (!save.Save(out var error))
            {
                ShowError($"Could not save pet cleanup: {error?.Message ?? "unknown save error"}");
                return;
            }

            Publish($"Cleared {removed} DEV_TEST_ pet(s). Real pets were not touched.");
        }

        private static void PrintPetDebugReport()
        {
            if (!EnsureEditMode()) return;
            var save = LoadSave();
            if (save == null) return;
            var database = BuildDatabase();
            var lines = new List<string>
            {
                "DEV_TEST Pet Debug Report",
                $"Generated: {DateTime.Now:O}",
                $"Owned pets: {save.CurrentData.Pets.Count}",
                string.Empty
            };

            foreach (var pet in save.CurrentData.Pets.OrderBy(pet => pet?.InstanceId, StringComparer.OrdinalIgnoreCase))
            {
                if (pet == null)
                {
                    lines.Add("- <null pet>");
                    continue;
                }

                var definition = database.TryGet<PetDefinition>(pet.DefinitionId, out var resolved)
                    ? resolved
                    : null;
                lines.Add($"- {pet.InstanceId} | {pet.DefinitionId} | " +
                          $"Tier={(definition != null ? definition.PetTier.ToString() : "UNKNOWN")} | " +
                          $"Abilities=[{pet.Ability1},{pet.Ability2},{pet.Ability3},{pet.Ability4}] | " +
                          $"Dungeon={pet.AssignedDungeonId ?? "NONE"} | Food={pet.Food} | Favourite={pet.Favourite}");
            }

            Publish(string.Join(Environment.NewLine, lines));
        }

        private static bool TryTagAndValidateNewPet(
            PetSaveData created,
            HashSet<string> beforeIds,
            ServiceContainer services,
            GameDatabase database,
            out string error)
        {
            error = null;
            if (created == null)
            {
                error = "PetService did not return a pet.";
                return false;
            }

            var after = services.Pet.GetAllPets();
            if (!after.Any(pet => pet != null && !beforeIds.Contains(pet.InstanceId)))
            {
                error = "PetService returned a pet but no new saved pet instance was detected.";
                return false;
            }

            if (!database.TryGet<PetDefinition>(created.DefinitionId, out var definition))
            {
                error = $"Hatched pet definition '{created.DefinitionId}' does not exist in the database.";
                return false;
            }

            if (definition.PetTier < 1 || definition.PetTier > 3)
            {
                error = $"Pet '{created.DefinitionId}' has invalid tier {definition.PetTier}.";
                return false;
            }

            if (!HasGeneratedAbility(created))
            {
                error = $"Pet '{created.DefinitionId}' has no generated ability.";
                return false;
            }

            created.InstanceId = $"{TestPrefix}PET_{Guid.NewGuid():N}";
            return true;
        }

        private static ItemRuntime FindExistingTestEgg(IInventoryService inventory)
        {
            return inventory.GetAllItems()
                .Where(item => item != null && IsTestId(item.InstanceId) && IsPetEgg(item.Definition))
                .FirstOrDefault();
        }

        private static ItemDefinition ChooseUnusedPetEgg(GameDatabase database, IInventoryService inventory)
        {
            var candidates = database.GetAll<ItemDefinition>()
                .Where(IsPetEgg)
                .Where(egg => database.GetAll<PetDefinition>().Any(pet =>
                    string.Equals(pet.PetFamily, EggToFamily(egg.id), StringComparison.OrdinalIgnoreCase)))
                .Where(egg => inventory.GetQuantityByDefinitionId(egg.id) == 0)
                .ToList();
            return candidates.Count == 0 ? null : candidates[Random.Next(candidates.Count)];
        }

        private static bool HasRealEggOfDefinition(IInventoryService inventory, string definitionId)
        {
            return inventory.GetAllItems().Any(item => item != null &&
                !IsTestId(item.InstanceId) &&
                item.Definition != null &&
                string.Equals(item.Definition.id, definitionId, StringComparison.OrdinalIgnoreCase) &&
                item.StackCount > 0);
        }

        private static HashSet<string> GetPetIds(IPetService petService)
        {
            return new HashSet<string>(petService.GetAllPets()
                .Where(pet => pet != null && !string.IsNullOrEmpty(pet.InstanceId))
                .Select(pet => pet.InstanceId), StringComparer.Ordinal);
        }

        private static int GetPetTier(GameDatabase database, PetSaveData pet)
        {
            return pet != null && database.TryGet<PetDefinition>(pet.DefinitionId, out var definition)
                ? definition.PetTier
                : 0;
        }

        private static bool HasGeneratedAbility(PetSaveData pet)
        {
            return pet != null && new[] { pet.Ability1, pet.Ability2, pet.Ability3, pet.Ability4 }
                .Any(ability => !string.IsNullOrEmpty(ability) &&
                                !string.Equals(ability, "EMPTY", StringComparison.OrdinalIgnoreCase));
        }

        private static bool ContainsPetAfterReload(string instanceId, out string error)
        {
            error = null;
            var reloaded = new SaveService();
            if (!reloaded.Load(out var loadError))
            {
                error = loadError?.Message ?? "unknown load error";
                return false;
            }
            return reloaded.CurrentData.Pets.Any(pet => pet != null && pet.InstanceId == instanceId);
        }

        private static bool IsPetEgg(ItemDefinition item)
        {
            return item != null && !string.IsNullOrEmpty(item.id) &&
                   item.id.EndsWith("_egg", StringComparison.OrdinalIgnoreCase) &&
                   !string.Equals(item.id, "frozen_egg", StringComparison.OrdinalIgnoreCase);
        }

        private static string EggToFamily(string eggId)
        {
            return eggId.Substring(0, eggId.Length - "_egg".Length);
        }

        private static bool IsTestId(string id)
        {
            return !string.IsNullOrEmpty(id) && id.StartsWith(TestPrefix, StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryBuildContext(
            out ServiceContainer services,
            out SaveService save,
            out GameDatabase database)
        {
            services = null;
            save = null;
            database = null;
            try
            {
                save = LoadSave();
                if (save == null) return false;
                database = BuildDatabase();
                services = new ServiceContainer(database, save);
                return true;
            }
            catch (Exception ex)
            {
                ShowError($"Could not create verification context: {ex.Message}");
                return false;
            }
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
                Debug.LogWarning($"[PetVerification] Database build reported {report.errors.Count} error(s).");
            return database;
        }

        private static bool EnsureEditMode()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode) return true;
            ShowError("Stop Play Mode before running a pet verification command.");
            return false;
        }

        private static void ShowError(string message)
        {
            Publish("ERROR: " + message, MessageType.Error);
        }

        private static void Publish(string message, MessageType type = MessageType.Info)
        {
            _lastReport = message ?? string.Empty;
            foreach (var window in Resources.FindObjectsOfTypeAll<PetVerificationWindow>())
                window.Repaint();
            if (type == MessageType.Error) Debug.LogError($"[PetVerification] {message}");
            else Debug.Log($"[PetVerification] {message}");
            EditorUtility.DisplayDialog("Pet Verification", message, "OK");
        }
    }
}
