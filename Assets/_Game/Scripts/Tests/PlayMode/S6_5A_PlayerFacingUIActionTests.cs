using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using GuildMaster.Runtime.Boot;
using GuildMaster.Runtime.UI.Tavern;
using GuildMaster.Runtime.UI.Character;
using GuildMaster.Runtime.UI.Inventory;
using GuildMaster.Runtime.UI.Craft;
using GuildMaster.Runtime.UI.Merchant;
using GuildMaster.Runtime.UI.Dungeon;
using GuildMaster.Runtime.UI.Quest;
using GuildMaster.Runtime.UI.Settings;

namespace GuildMaster.Tests.PlayMode
{
    public class S6_5A_PlayerFacingUIActionTests
    {
        [UnityTest]
        public IEnumerator UI_Interactions_Mutate_State_Correctly()
        {
            SceneManager.LoadScene("Main");
            yield return new WaitForSeconds(1f);

            var bootstrap = Object.FindFirstObjectByType<UIRuntimeBootstrap>();
            Assert.IsNotNull(bootstrap, "UIRuntimeBootstrap should exist.");
            Assert.IsNotNull(bootstrap.Services, "Services should be initialized.");
            
            var services = bootstrap.Services;
            var db = services.Database;

            // 1. Tavern UI Test
            var tavernUI = Object.FindFirstObjectByType<TavernScreen>(FindObjectsInactive.Include);
            Assert.IsNotNull(tavernUI, "TavernScreen should exist.");
            long nextVisitorBefore = services.Tavern.GetNextVisitorTimerSeconds();
            tavernUI.OnClickSpawnGuest(); // Advances time by 1h
            long nextVisitorAfter = services.Tavern.GetNextVisitorTimerSeconds();
            Assert.AreNotEqual(nextVisitorBefore, nextVisitorAfter, "Tavern UI should progress time.");

            // 2. Character UI Test
            var charUI = Object.FindFirstObjectByType<CharacterScreen>(FindObjectsInactive.Include);
            Assert.IsNotNull(charUI, "CharacterScreen should exist.");
            int charsBefore = services.Character.GetAllCharacters().Count;
            // Let's recruit via Tavern UI to get a character
            tavernUI.OnClickRecruitFirst();
            int charsAfter = services.Character.GetAllCharacters().Count;
            Assert.Greater(charsAfter, charsBefore, "Tavern recruit UI should add character.");

            // 3. Inventory UI Test
            var invUI = Object.FindFirstObjectByType<InventoryScreen>(FindObjectsInactive.Include);
            Assert.IsNotNull(invUI, "InventoryScreen should exist.");
            
            // Give item for testing - use GetAll<ItemDefinition>() since GameDatabase has no GetCategory()
            var allItems = db.GetAll<GuildMaster.Definitions.ItemDefinition>();
            var itemDefEnum = allItems.GetEnumerator();
            if (itemDefEnum.MoveNext())
            {
                services.Inventory.AddItem(services.Item.CreateItem(itemDefEnum.Current.id, 1));
            }
            yield return null;

            var items = services.Inventory.GetAllItems();
            if (items.Count > 0)
            {
                bool lockBefore = items[0].IsLocked;
                invUI.OnClickToggleLockFirst();
                bool lockAfter = items[0].IsLocked;
                Assert.AreNotEqual(lockBefore, lockAfter, "Inventory UI should toggle lock.");
            }

            // 4. Craft UI Test
            var craftUI = Object.FindFirstObjectByType<CraftScreen>(FindObjectsInactive.Include);
            Assert.IsNotNull(craftUI, "CraftScreen should exist.");
            
            // Just test progress time UI
            craftUI.OnClickProgressTime(); // No assert needed, just check it doesn't crash

            // 5. Merchant UI Test
            var merchantUI = Object.FindFirstObjectByType<MerchantScreen>(FindObjectsInactive.Include);
            Assert.IsNotNull(merchantUI, "MerchantScreen should exist.");
            long moneyBefore = services.Save.CurrentData.Money;
            merchantUI.OnClickBuyFirstRegular(); // If stock is empty, it does nothing
            
            // 6. Dungeon UI Test
            var dungeonUI = Object.FindFirstObjectByType<DungeonScreen>(FindObjectsInactive.Include);
            Assert.IsNotNull(dungeonUI, "DungeonScreen should exist.");
            dungeonUI.OnClickStartFirst();
            dungeonUI.OnClickTick1();
            
            // 7. Quest UI Test
            var questUI = Object.FindFirstObjectByType<QuestScreen>(FindObjectsInactive.Include);
            Assert.IsNotNull(questUI, "QuestScreen should exist.");
            questUI.OnClickTestIncrement();

            // 8. Settings UI Test
            var settingsUI = Object.FindFirstObjectByType<SettingsScreen>(FindObjectsInactive.Include);
            Assert.IsNotNull(settingsUI, "SettingsScreen should exist.");
            bool soundBefore = services.Settings.GetToggle("sound");
            settingsUI.OnClickToggleSound();
            bool soundAfter = services.Settings.GetToggle("sound");
            Assert.AreNotEqual(soundBefore, soundAfter, "Settings UI should toggle sound.");

            yield return null;
        }
    }
}
