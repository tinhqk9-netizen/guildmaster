using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using GuildMaster.Database;
using GuildMaster.Infrastructure.DataProviders;
using GuildMaster.Infrastructure.Serialization;

namespace GuildMaster.Tests.Editor
{
    [InitializeOnLoad]
    public static class SmokeTestRunner
    {
        static SmokeTestRunner()
        {
            // Auto-run on recompile
            EditorApplication.delayCall += RunSmokeTest;
        }
        public static void RunSmokeTest()
        {
            string logPath = Path.Combine(Application.dataPath, "../Reports/SmokeTestOutput.txt");
            try
            {
                var dataProvider = new EditorExternalGameDataProvider();
                var serializer = new UnityJsonSerializer();
                var database = new GameDatabase();
                var builder = new DatabaseBuilder(dataProvider, serializer, database);

                var report = builder.Build();

                string result = "SMOKE TEST RESULT\n";
                result += $"Manifest Loaded: {report.manifestLoaded}\n";
                result += $"Files Loaded: {report.loadedFiles}\n";
                result += $"Fatal Errors: {report.hasFatalErrors}\n";
                result += "Records:\n";
                foreach(var kvp in report.loadedRecordsByCategory)
                {
                    result += $"- {kvp.Key}: {kvp.Value}\n";
                }

                if (report.errors.Count > 0)
                {
                    result += "ERRORS:\n" + string.Join("\n", report.errors) + "\n";
                }

                File.WriteAllText(logPath, result);
                Debug.Log("Smoke Test Completed Successfully.");
            }
            catch (Exception ex)
            {
                File.WriteAllText(logPath, $"SMOKE TEST FAILED\n{ex}");
                Debug.LogError($"Smoke Test Failed: {ex}");
            }
        }
    }
}
