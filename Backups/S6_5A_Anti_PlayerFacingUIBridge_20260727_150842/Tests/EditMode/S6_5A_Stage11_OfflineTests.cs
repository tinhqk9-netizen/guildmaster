using System;
using NUnit.Framework;
using GuildMaster.Database;
using GuildMaster.Infrastructure.DataProviders;
using GuildMaster.Infrastructure.Serialization;
using GuildMaster.Runtime.Save;
using GuildMaster.Runtime.Services;

namespace GuildMaster.Tests.EditMode
{
    [TestFixture]
    public class S6_5A_Stage11_OfflineTests
    {
        private GameDatabase _database;
        private ServiceContainer _container;

        [SetUp]
        public void Setup()
        {
            _database = new GameDatabase();
            var provider = new EditorExternalGameDataProvider();
            var serializer = new UnityJsonSerializer();
            var builder = new DatabaseBuilder(provider, serializer, _database);
            builder.Build();

            _container = new ServiceContainer(_database);
        }

        [Test]
        public void OfflineProgressService_CalculateOfflineDeltaSeconds_CapsAt12Hours()
        {
            var offline = _container.OfflineProgress;
            long now = 1000000000L;

            // 24 hours offline = 86400s
            long save24hAgo = now - 86400L;
            long delta = offline.CalculateOfflineDeltaSeconds(save24hAgo, now);

            // Must cap at 43200s (12 hours)
            Assert.AreEqual(43200L, delta);
        }

        [Test]
        public void OfflineProgressService_ApplyOfflineProgress_SimulatesCraftAndMarketAndUpdatesSaveTime()
        {
            var offline = _container.OfflineProgress;
            var save = _container.Save;

            save.CurrentData.Metadata.SaveTimeUnix = 1000000000L;
            long currentUnix = 1000010000L; // 10000s offline

            var result = offline.ApplyOfflineProgress(currentUnix);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(10000L, result.DeltaSeconds);
            Assert.AreEqual(currentUnix, save.CurrentData.Metadata.SaveTimeUnix);
        }
    }
}
