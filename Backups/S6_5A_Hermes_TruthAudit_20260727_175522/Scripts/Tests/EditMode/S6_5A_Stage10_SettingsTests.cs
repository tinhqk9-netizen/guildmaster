using System;
using NUnit.Framework;
using GuildMaster.Database;
using GuildMaster.Infrastructure.DataProviders;
using GuildMaster.Infrastructure.Serialization;
using GuildMaster.Runtime.Services;

namespace GuildMaster.Tests.EditMode
{
    [TestFixture]
    public class S6_5A_Stage10_SettingsTests
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
        public void SettingsService_SetAndGetToggle_UpdatesSaveData()
        {
            var settings = _container.Settings;
            var save = _container.Save;

            settings.SetToggle("sound", false);
            Assert.IsFalse(settings.GetToggle("sound"));
            Assert.IsFalse(save.CurrentData.SettingsSound);

            settings.SetToggle("music", false);
            Assert.IsFalse(settings.GetToggle("music"));
            Assert.IsFalse(save.CurrentData.SettingsMusic);

            settings.SetLanguage("fr");
            Assert.AreEqual("fr", settings.GetLanguage());
            Assert.AreEqual("fr", save.CurrentData.SettingsLanguage);
        }

        [Test]
        public void SettingsService_ResetToDefault_RestoresDefaults()
        {
            var settings = _container.Settings;

            settings.SetToggle("sound", false);
            settings.SetLanguage("de");
            Assert.IsFalse(settings.GetToggle("sound"));

            settings.ResetToDefault();
            Assert.IsTrue(settings.GetToggle("sound"));
            Assert.AreEqual("en", settings.GetLanguage());
        }
    }
}
