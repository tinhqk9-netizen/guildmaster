using UnityEngine;
using UnityEngine.SceneManagement;
using GuildMaster.Database;
using GuildMaster.Infrastructure.DataProviders;
using GuildMaster.Infrastructure.Serialization;
using GuildMaster.Services;

namespace GuildMaster.Bootstrap
{
    public class Bootstrapper : MonoBehaviour
    {
        private void Awake()
        {
            // Do not automatically load Main here if it's asynchronous, but since it's local we do it synchronously.
            InitializeRuntime();
        }

        private void InitializeRuntime()
        {
            Debug.Log("[Bootstrapper] Initializing Game Runtime...");

            // 1. Create Data Provider
#if UNITY_EDITOR
            IGameDataProvider dataProvider = new EditorExternalGameDataProvider();
#else
            IGameDataProvider dataProvider = new StreamingAssetsGameDataProvider();
#endif

            // 2. Create Serializer
            IJsonSerializer serializer = new UnityJsonSerializer();

            // 3. Create Database
            var gameDb = new GameDatabase();

            // 4. Build Database from Manifest
            var builder = new DatabaseBuilder(dataProvider, serializer, gameDb);
            var report = builder.Build();

            // 5. Create Services
            var dbService = new DatabaseService(gameDb);
            var locService = new LocalizationService(dataProvider, serializer);
            var assetService = new AssetManifestService(dataProvider, serializer);

            // Log Build Report
            LogReport(report);

            // 6. Proceed to Main if no fatal errors
            if (report.hasFatalErrors)
            {
                Debug.LogError("[Bootstrapper] FATAL ERRORS encountered during Database Build. Halting Boot process.");
            }
            else
            {
                Debug.Log("[Bootstrapper] Runtime initialized successfully. Loading Main Scene...");
                LoadMainScene();
            }
        }

        private void LogReport(DatabaseBuildReport report)
        {
            Debug.Log($"--- Database Build Report ---");
            Debug.Log($"- Provider: {report.providerName}");
            Debug.Log($"- Manifest Loaded: {report.manifestLoaded}");
            Debug.Log($"- Files Expected: {report.expectedFiles}, Loaded: {report.loadedFiles}, Skipped: {report.skippedFiles}");
            
            foreach (var kvp in report.loadedRecordsByCategory)
            {
                Debug.Log($"  - {kvp.Key}: {kvp.Value} records");
            }

            if (report.duplicateIds.Count > 0)
            {
                Debug.LogWarning($"- Duplicate IDs found ({report.duplicateIds.Count}):\n" + string.Join("\n", report.duplicateIds));
            }

            if (report.recordCountMismatches.Count > 0)
            {
                Debug.LogError($"- Record Count Mismatches ({report.recordCountMismatches.Count}):\n" + string.Join("\n", report.recordCountMismatches));
            }

            if (report.unsupportedCategories.Count > 0)
            {
                Debug.LogWarning($"- Unsupported Categories ({report.unsupportedCategories.Count}):\n" + string.Join("\n", report.unsupportedCategories));
            }

            if (report.warnings.Count > 0)
            {
                Debug.LogWarning($"- General Warnings:\n" + string.Join("\n", report.warnings));
            }

            if (report.errors.Count > 0)
            {
                Debug.LogError($"- General Errors:\n" + string.Join("\n", report.errors));
            }
        }

        private void LoadMainScene()
        {
            // Assuming the main scene is called "Main" and added to Build Settings
            // SceneManager.LoadScene("Main");
            Debug.Log("[Bootstrapper] Would load 'Main' Scene here (Task S1-001 ends before gameplay).");
        }
    }
}
