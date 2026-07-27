using System.Collections.Generic;
using NUnit.Framework;
using GuildMaster.Database;
using GuildMaster.Definitions;
using GuildMaster.Infrastructure.DataProviders;
using GuildMaster.Infrastructure.Serialization;
using GuildMaster.Loaders.DTOs;

namespace GuildMaster.Tests.EditMode
{
    public class MockDataProvider : IGameDataProvider
    {
        public string ProviderName => "MockProvider";
        public Dictionary<string, string> MockFiles = new Dictionary<string, string>();

        public bool Exists(string relativePath) => MockFiles.ContainsKey(relativePath);
        
        public string ReadText(string relativePath)
        {
            if (MockFiles.TryGetValue(relativePath, out string content)) return content;
            throw new System.IO.FileNotFoundException(relativePath);
        }

        public IEnumerable<string> EnumerateFiles() => MockFiles.Keys;
    }

    public class DatabaseTests
    {
        private IJsonSerializer _serializer;

        [SetUp]
        public void Setup()
        {
            _serializer = new UnityJsonSerializer();
        }

        [Test]
        public void DefinitionFile_Deserialize_SuccessfullyMapsMetadataAndData()
        {
            string json = @"{
                ""metadata"": { ""category"": ""items"", ""recordCount"": 1 },
                ""data"": [ { ""id"": ""sword_01"", ""className"": ""Weapon"" } ]
            }";

            var result = _serializer.Deserialize<DefinitionFile<ItemDefinition>>(json);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.metadata);
            Assert.AreEqual("items", result.metadata.category);
            Assert.AreEqual(1, result.metadata.recordCount);
            
            Assert.IsNotNull(result.data);
            Assert.AreEqual(1, result.data.Count);
            Assert.AreEqual("sword_01", result.data[0].id);
        }

        [Test]
        public void Manifest_Deserialize_SuccessfullyMapsFiles()
        {
            string json = @"{
                ""schemaVersion"": ""1.0"",
                ""files"": [
                    { ""filename"": ""items.json"", ""category"": ""items"", ""recordCount"": 5 }
                ]
            }";

            var result = _serializer.Deserialize<ManifestDefinition>(json);
            
            Assert.IsNotNull(result);
            Assert.AreEqual("1.0", result.schemaVersion);
            Assert.IsNotNull(result.files);
            Assert.AreEqual(1, result.files.Count);
            Assert.AreEqual("items", result.files[0].category);
        }

        [Test]
        public void GameDatabase_DuplicateIds_LogsWarningButKeepsFirst()
        {
            var db = new GameDatabase();
            var list = new List<ItemDefinition>
            {
                new ItemDefinition { id = "item1", recordHash = "A" },
                new ItemDefinition { id = "item1", recordHash = "B" } // duplicate
            };

            db.RegisterCollection(list);

            Assert.IsTrue(db.TryGet<ItemDefinition>("item1", out var item));
            Assert.AreEqual("A", item.recordHash); // Kept the first one
            Assert.AreEqual(1, db.GetAll<ItemDefinition>().Count);
        }

        [Test]
        public void DatabaseBuilder_UnsupportedCategory_DoesNotCrash()
        {
            var provider = new MockDataProvider();
            provider.MockFiles["manifest.json"] = @"{ ""files"": [ { ""filename"": ""future.json"", ""category"": ""future_content"", ""recordCount"": 1 } ] }";
            provider.MockFiles["future.json"] = @"{ ""data"": [] }";

            var db = new GameDatabase();
            var builder = new DatabaseBuilder(provider, _serializer, db);
            
            var report = builder.Build();

            Assert.IsTrue(report.manifestLoaded);
            Assert.Contains("future_content", report.unsupportedCategories);
            Assert.AreEqual(1, report.skippedFiles);
            Assert.IsFalse(report.hasFatalErrors);
        }

        [Test]
        public void DatabaseBuilder_MissingFile_GeneratesError()
        {
            var provider = new MockDataProvider();
            provider.MockFiles["manifest.json"] = @"{ ""files"": [ { ""filename"": ""missing.json"", ""category"": ""items"", ""recordCount"": 1 } ] }";
            // File "missing.json" is not added to MockFiles

            var db = new GameDatabase();
            var builder = new DatabaseBuilder(provider, _serializer, db);
            
            var report = builder.Build();

            Assert.IsTrue(report.manifestLoaded);
            Assert.IsTrue(report.errors.Count > 0);
            Assert.IsTrue(report.hasFatalErrors);
        }
    }
}
