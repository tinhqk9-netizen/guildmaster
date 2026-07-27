using System;
using System.Collections.Generic;
using UnityEngine;
using GuildMaster.Infrastructure.DataProviders;
using GuildMaster.Infrastructure.Serialization;
using GuildMaster.Loaders.DTOs;
using GuildMaster.Definitions;

namespace GuildMaster.Database
{
    public class DatabaseBuilder
    {
        private readonly IGameDataProvider _dataProvider;
        private readonly IJsonSerializer _serializer;
        private readonly GameDatabase _database;

        private delegate void CategoryLoaderDelegate(string jsonContent, ManifestFileEntry fileEntry, DatabaseBuildReport report);
        private readonly Dictionary<string, CategoryLoaderDelegate> _categoryLoaders;

        public DatabaseBuilder(IGameDataProvider dataProvider, IJsonSerializer serializer, GameDatabase database)
        {
            _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _database = database ?? throw new ArgumentNullException(nameof(database));
            
            _categoryLoaders = new Dictionary<string, CategoryLoaderDelegate>(StringComparer.OrdinalIgnoreCase)
            {
                { "items", LoadCategory<ItemDefinition> },
                { "enemies", LoadCategory<EnemyDefinition> },
                { "skills", LoadCategory<SkillDefinition> },
                { "status_effects", LoadCategory<StatusEffectDefinition> },
                { "adventurers", LoadCategory<AdventurerDefinition> },
                { "pets", LoadCategory<PetDefinition> },
                { "recipes", LoadCategory<RecipeDefinition> },
                { "quests", LoadCategory<QuestDefinition> },
                { "dungeons", LoadCategory<DungeonDefinition> },
                { "raids", LoadCategory<RaidDefinition> }
            };
        }

        public DatabaseBuildReport Build()
        {
            var report = new DatabaseBuildReport
            {
                providerName = _dataProvider.ProviderName
            };

            string manifestPath = "manifest.json";
            if (!_dataProvider.Exists(manifestPath))
            {
                report.errors.Add($"Manifest file not found at path: {manifestPath}");
                return report;
            }

            string manifestJson = _dataProvider.ReadText(manifestPath);
            ManifestDefinition manifest;
            try
            {
                manifest = _serializer.Deserialize<ManifestDefinition>(manifestJson);
                report.manifestLoaded = true;
            }
            catch (Exception ex)
            {
                report.errors.Add($"Failed to deserialize manifest.json: {ex.Message}");
                return report;
            }

            if (manifest.files == null)
            {
                report.warnings.Add("Manifest contains no files array.");
                return report;
            }

            report.expectedFiles = manifest.files.Count;

            foreach (var fileEntry in manifest.files)
            {
                string category = fileEntry.category;
                string filename = fileEntry.filename;

                if (string.Equals(category, "localization", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(category, "assets", StringComparison.OrdinalIgnoreCase))
                {
                    // Handled by other services
                    report.skippedFiles++;
                    continue;
                }

                if (!_categoryLoaders.TryGetValue(category, out var loaderDelegate))
                {
                    report.unsupportedCategories.Add(category);
                    report.warnings.Add($"Unsupported category '{category}' in manifest.");
                    report.skippedFiles++;
                    continue;
                }

                if (!_dataProvider.Exists(filename))
                {
                    report.errors.Add($"File '{filename}' defined in manifest does not exist.");
                    continue;
                }

                try
                {
                    string jsonContent = _dataProvider.ReadText(filename);
                    loaderDelegate(jsonContent, fileEntry, report);
                    report.loadedFiles++;
                }
                catch (Exception ex)
                {
                    report.errors.Add($"Error loading or deserializing '{filename}': {ex.Message}");
                }
            }

            return report;
        }

        private void LoadCategory<T>(string jsonContent, ManifestFileEntry fileEntry, DatabaseBuildReport report) where T : DefinitionBase
        {
            var definitionFile = _serializer.Deserialize<DefinitionFile<T>>(jsonContent);
            if (definitionFile == null || definitionFile.data == null)
            {
                report.errors.Add($"Deserialization of {fileEntry.filename} returned null data.");
                return;
            }

            var list = definitionFile.data;
            if (list.Count != fileEntry.recordCount)
            {
                report.recordCountMismatches.Add($"Category {fileEntry.category}: expected {fileEntry.recordCount}, loaded {list.Count}");
            }

            // Record missing/duplicate IDs just for the report before injecting to database
            var seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in list)
            {
                if (string.IsNullOrEmpty(item.id))
                {
                    // Database ignores it and logs a warning
                }
                else if (!seenIds.Add(item.id))
                {
                    report.duplicateIds.Add($"[{fileEntry.category}] {item.id}");
                }
            }

            _database.RegisterCollection(list);
            report.loadedRecordsByCategory[fileEntry.category] = list.Count;
        }
    }
}
