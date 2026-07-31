using System;
using System.Collections.Generic;
using System.IO;
using GuildMaster.Runtime.Save;
using GuildMaster.Runtime.Services;
using GuildMaster.Runtime.Models;
using NUnit.Framework;
using UnityEngine;

namespace GuildMaster.Tests.Batch1
{
    public class Batch1FoundationTests
    {
        private SaveService _saveService;
        private string _testSavePath;
        private string _testBackupPath;

        [SetUp]
        public void Setup()
        {
            _saveService = new SaveService();
            _testSavePath = Path.Combine(Application.persistentDataPath, "save.json");
            _testBackupPath = Path.Combine(Application.persistentDataPath, "save_backup.json");
            CleanupFiles();
        }

        [TearDown]
        public void Teardown()
        {
            CleanupFiles();
        }

        private void CleanupFiles()
        {
            if (File.Exists(_testSavePath)) File.Delete(_testSavePath);
            if (File.Exists(_testBackupPath)) File.Delete(_testBackupPath);
        }

        [Test]
        public void SaveService_Load_NoSave_ReturnsFreshNewGame()
        {
            _saveService.Load(out _);
            Assert.AreEqual(SaveLoadResult.FreshNewGame, _saveService.LastLoadStatus);
        }

        [Test]
        public void SaveService_Save_FiresEvents()
        {
            bool startedFired = false;
            bool completedFired = false;
            bool successResult = false;

            _saveService.OnSaveStarted += () => startedFired = true;
            _saveService.OnSaveCompleted += (success) => {
                completedFired = true;
                successResult = success;
            };

            _saveService.Save(out _);

            Assert.IsTrue(startedFired, "OnSaveStarted was not fired.");
            Assert.IsTrue(completedFired, "OnSaveCompleted was not fired.");
            Assert.IsTrue(successResult, "Save should be successful.");
        }
        
        [Test]
        public void OfflineSummaryBuilder_CalculatesDiffCorrectly()
        {
            var saveService = new MockSaveService();
            
            var testSaveData = SaveData.CreateDefault();
            testSaveData.Money = 1000;
            testSaveData.LastAccess = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 3600; // 1 hour ago
            testSaveData.Items.Add(new ItemSaveData { DefinitionId = "Wood", StackCount = 10 });
            
            saveService.SetData(testSaveData);
            
            Action mockCatchup = () => {
                testSaveData.Money += 500;
                testSaveData.Items.Add(new ItemSaveData { DefinitionId = "Wood", StackCount = 20 });
                testSaveData.Items.Add(new ItemSaveData { DefinitionId = "Stone", StackCount = 5 });
            };

            var builder = new OfflineProgressSummaryBuilder(saveService);
            var summary = builder.BuildSummary(mockCatchup);

            Assert.AreEqual(3600, summary.AppliedSeconds);
            Assert.AreEqual(500, summary.MoneyDelta);
            Assert.AreEqual(0, summary.GemsDelta);
            Assert.IsTrue(summary.ItemDeltas.ContainsKey("Wood"));
            Assert.AreEqual(20, summary.ItemDeltas["Wood"]);
            Assert.IsTrue(summary.ItemDeltas.ContainsKey("Stone"));
            Assert.AreEqual(5, summary.ItemDeltas["Stone"]);
        }

        private class MockSaveService : ISaveService
        {
            public SaveData CurrentData { get; private set; }
            public SaveLoadResult LastLoadStatus { get; set; }

            public event Action OnSaveStarted;
            public event Action<bool> OnSaveCompleted;

            public void SetData(SaveData data)
            {
                CurrentData = data;
            }

            public bool HasSaveFile() => true;
            public bool Load(out Exception error) { error = null; return true; }
            public bool Save(out Exception error) { error = null; return true; }
            public void DeleteSave() { }
        }
    }
}
