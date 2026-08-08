using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using GuildMaster.Definitions;
using GuildMaster.Runtime.Boot;
using GuildMaster.Runtime.Save;

namespace GuildMaster.Tests.PlayMode
{
    public sealed class Phase4_6_RaidRuntimeSmokeTests
    {
        [UnityTest]
        public IEnumerator RaidRegularEventAndBossFlows_LoadInRuntime()
        {
            SceneManager.LoadScene("Main");
            yield return new WaitForSeconds(1f);

            var bootstrap = Object.FindFirstObjectByType<UIRuntimeBootstrap>();
            Assert.IsNotNull(bootstrap, "Main scene did not create UIRuntimeBootstrap.");
            Assert.IsNotNull(bootstrap.Services, "Runtime services were not initialized.");

            var services = bootstrap.Services;
            var heroDefinition = services.Database.GetAll<AdventurerDefinition>().First();
            var hero = services.Character.CreateCharacter(heroDefinition.id);
            services.Save.CurrentData.CurrentParty.Clear();
            services.Save.CurrentData.CurrentParty.Add(hero.InstanceId);

            Assert.IsTrue(services.Raid.StartRaid("kaunis", null, out var regularError), regularError);
            Assert.IsNotNull(services.Raid.ActiveRaid);
            Assert.Greater(services.Raid.ActiveRaid.Enemies.Count, 0, "Regular raid did not spawn its first encounter.");
            services.Raid.AbandonRaid();

            services.Save.CurrentData.Dungeons.Add(new DungeonSaveData
            {
                DefinitionId = "frostbite_peaks",
                MaxProgress = 150
            });
            Assert.IsTrue(services.Raid.StartRaid("the_cultist_rebels", null, out var eventError), eventError);
            Assert.AreEqual("halls_exploration", services.Raid.ActiveRaid.EventKey);
            services.Raid.AbandonRaid();

            Assert.IsTrue(services.Raid.StartRaid("the_tower", null, out var bossError), bossError);
            Assert.AreEqual(8, services.Raid.ActiveRaid.LegacyProgress);
            Assert.AreEqual("Lazarus", services.Raid.ActiveRaid.Enemies.Single().Definition.className);
            services.Raid.AbandonRaid();

            yield return null;
        }
    }
}
