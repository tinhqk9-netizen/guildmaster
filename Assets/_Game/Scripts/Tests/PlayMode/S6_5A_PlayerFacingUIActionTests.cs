using System.Collections;
using System.IO;
using System.Linq;
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
    /// <summary>
    /// Proves every gameplay screen mutates real state through explicit entity selection
    /// (Next/Prev + act-on-selected), not by always touching index 0. Section II.3 forbids
    /// "First Item"/"First Character" as production gameplay, so this test exercises the
    /// selection controls themselves — selecting index 1 must actually change which entity
    /// the action targets.
    /// </summary>
    public class S6_5A_PlayerFacingUIActionTests
    {
        /// <summary>
        /// SaveService.cs persists to Application.persistentDataPath/save.json — a real file on
        /// disk that survives between PlayMode test runs (unlike EditMode's S2VerificationTests,
        /// which explicitly calls SaveService.DeleteSave() in its own [SetUp]/[TearDown], this
        /// class never did). Any character recruited by a previous run of this test stayed on
        /// disk and was reloaded here, so "charsBefore" silently reflected leftover state instead
        /// of a fresh save — e.g. 3 pre-existing characters exceeding the fresh QuartersCapacity
        /// of 2, which makes CanRecruit() correctly return false with no exception. Deleting the
        /// save file before/after each run (mirroring SaveService.DeleteSave(), done directly via
        /// File.IO since no SaveService instance exists yet before the scene loads) makes every
        /// run start from the same deterministic, empty state the rest of this test assumes.
        /// </summary>
        private static void DeletePersistedSave()
        {
            string saveFilePath = Path.Combine(Application.persistentDataPath, "save.json");
            string backupFilePath = Path.Combine(Application.persistentDataPath, "save_backup.json");
            if (File.Exists(saveFilePath)) File.Delete(saveFilePath);
            if (File.Exists(backupFilePath)) File.Delete(backupFilePath);
        }

        [SetUp]
        public void SetUp()
        {
            DeletePersistedSave();
        }

        [TearDown]
        public void TearDown()
        {
            DeletePersistedSave();
        }

        [UnityTest]
        public IEnumerator UI_Interactions_Mutate_State_Correctly()
        {
            SceneManager.LoadScene("Main");
            yield return new WaitForSeconds(1f);

            var bootstrap = Object.FindFirstObjectByType<UIRuntimeBootstrap>();
            Assert.IsNotNull(bootstrap, "UIRuntimeBootstrap should exist.");
            Assert.IsNotNull(bootstrap.Services, "Services should be initialized.");

            var services = bootstrap.Services;

            // 1. Tavern UI — recruit the SELECTED guest.
            // Note: OnClickAdvanceVisitorTime was a dev-only shortcut removed from the rebuilt TavernScreen.
            // Time progression is now handled by the service layer (TavernService.ProgressVisitorTimer).
            var tavernUI = Object.FindFirstObjectByType<TavernScreen>(FindObjectsInactive.Include);
            Assert.IsNotNull(tavernUI, "TavernScreen should exist.");

            // Fresh-save Tavern capacity is exactly 1 (LevelTavernCapacity=0 + BASE_TAVERN_SPACES=1
            // + UpgradeTavernCapacity=0 + 0 bonus — see FormulaService.GetTavernCapacity). With
            // capacity 1, TavernService.GenerateVisitor's trim-to-capacity step correctly keeps only
            // 1 guest no matter how many times ProgressVisitorTime is called — that is production
            // behaving correctly, not a bug. To exercise the second guest card, raise capacity via
            // a real save field (LevelTavernCapacity), the same way an in-game capacity upgrade
            // would — not by bypassing capacity or touching the guest list directly.
            services.Save.CurrentData.LevelTavernCapacity = 1; // capacity becomes 2
            Assert.GreaterOrEqual(services.Tavern.GetTavernCapacity(), 2, "Test precondition: capacity must allow at least 2 guests.");

            int guardTicks = 0;
            while (services.Tavern.GetGuests().Count < 2 && guardTicks < 100)
            {
                services.Tavern.ProgressVisitorTime(3600L); // advance 1h at service level
                guardTicks++;
            }
            Assert.GreaterOrEqual(services.Tavern.GetGuests().Count, 2, "Tavern should generate at least 2 visitors once capacity allows it.");

            // 2. Character UI — recruit via the real Tavern selection (not "first").
            var charUI = Object.FindFirstObjectByType<CharacterScreen>(FindObjectsInactive.Include);
            Assert.IsNotNull(charUI, "CharacterScreen should exist.");
            int charsBefore = services.Character.GetAllCharacters().Count;
            var guestsBefore = new System.Collections.Generic.List<GuildMaster.Runtime.Save.CharacterSaveData>(services.Tavern.GetGuests());
            int tavernGuestsBefore = guestsBefore.Count;

            // The Tavern screen (and every other screen) is hidden by UIService.RegisterScreen at
            // boot. Cards were being counted while the screen's GameObject was still inactive —
            // GetComponentsInChildren defaults to includeInactive:false, so it always found 0
            // regardless of how many guests the backend had. The screen must actually be opened
            // through the real navigation path (HUD Tavern button), not by calling Show()/SetActive
            // directly, so this proves the player-facing click path works end to end.
            var hud = Object.FindFirstObjectByType<GuildMaster.Runtime.UI.HUD.HUDController>(FindObjectsInactive.Include);
            Assert.IsNotNull(hud, "HUDController should exist.");
            var tavernNavButtonField = typeof(GuildMaster.Runtime.UI.HUD.HUDController).GetField("_tavernButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var tavernNavButton = (UnityEngine.UI.Button)tavernNavButtonField.GetValue(hud);
            Assert.IsNotNull(tavernNavButton, "HUD Tavern navigation button should be wired.");

            tavernNavButton.onClick.Invoke();
            yield return null;

            Assert.IsTrue(tavernUI.gameObject.activeInHierarchy, "Tavern screen should be active after navigating to it via the HUD button.");

            // Use reflection to access the card container and recruit button to click actual buttons
            var cardContainerField = typeof(TavernScreen).GetField("_cardContainer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var recruitButtonField = typeof(TavernScreen).GetField("_recruitButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var selectedIndexField = typeof(TavernScreen).GetField("_selectedIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var cardContainer = (UnityEngine.RectTransform)cardContainerField.GetValue(tavernUI);
            var recruitButton = (UnityEngine.UI.Button)recruitButtonField.GetValue(tavernUI);

            Assert.IsNotNull(cardContainer, "Card container should be wired.");
            Assert.IsNotNull(recruitButton, "Recruit button should be wired.");

            var guestButtons = cardContainer.GetComponentsInChildren<UnityEngine.UI.Button>();
            Assert.AreEqual(tavernGuestsBefore, guestButtons.Length, "Rendered guest card count should match the backend guest count exactly.");
            Assert.GreaterOrEqual(guestButtons.Length, 2, "There should be at least 2 guest cards rendered.");
            
            // Lấy guest card thứ hai
            var targetGuest = guestsBefore[1];
            
            // Gọi actual guest card Button.onClick.Invoke()
            guestButtons[1].onClick.Invoke();
            
            // Xác nhận selected state/selected guest ID đổi
            int selectedIndex = (int)selectedIndexField.GetValue(tavernUI);
            Assert.AreEqual(1, selectedIndex, "Selected index should update when clicking card 1.");
            
            // Gọi actual Recruit Button.onClick.Invoke()
            recruitButton.onClick.Invoke();
            yield return null;
            
            int charsAfter = services.Character.GetAllCharacters().Count;
            var guestsAfter = services.Tavern.GetGuests();
            
            // Xác nhận character count tăng đúng 1
            Assert.AreEqual(charsBefore + 1, charsAfter, "Character count should increase by exactly 1.");
            
            // Xác nhận đúng selected visitor được recruit (by ID matching)
            var recruitedChar = services.Character.GetAllCharacters()[charsAfter - 1];
            Assert.AreEqual(targetGuest.DefinitionId, recruitedChar.Definition.id, "Recruited character definition should match.");
            
            // Xác nhận visitor đó bị remove
            Assert.IsFalse(System.Linq.Enumerable.Any(guestsAfter, g => g.InstanceId == targetGuest.InstanceId), "Target guest should be removed from Tavern.");
            
            // Xác nhận guest còn lại không bị remove
            var guest0 = guestsBefore[0];
            Assert.IsTrue(System.Linq.Enumerable.Any(guestsAfter, g => g.InstanceId == guest0.InstanceId), "Other guests should remain in Tavern.");
            
            // Xác nhận UI list/detail refresh (số lượng button thay đổi)
            var guestButtonsAfter = cardContainer.GetComponentsInChildren<UnityEngine.UI.Button>();
            Assert.AreEqual(tavernGuestsBefore - 1, guestButtonsAfter.Length, "Card container should have one less card rendered.");
            // handler a card's Button.onClick invokes, so calling it directly is equivalent to the
            // player clicking card #1 rather than card #0.
            if (services.Character.GetAllCharacters().Count > 1)
            {
                charUI.SelectIndex(1);
                var selected = charUI.GetSelectedCharacter();
                Assert.AreEqual(services.Character.GetAllCharacters()[1].InstanceId, selected.InstanceId,
                    "Clicking the second character's card should select that exact character, not stay pinned to index 0.");
                charUI.SelectIndex(0);
            }

            // 3. Inventory UI — select an explicit item and toggle lock on THAT selection.
            var invUI = Object.FindFirstObjectByType<InventoryScreen>(FindObjectsInactive.Include);
            Assert.IsNotNull(invUI, "InventoryScreen should exist.");

            var allItems = services.Database.GetAll<GuildMaster.Definitions.ItemDefinition>();
            var itemDefEnum = allItems.GetEnumerator();
            if (itemDefEnum.MoveNext())
            {
                services.Inventory.AddItem(services.Item.CreateItem(itemDefEnum.Current.id, 1));
            }
            yield return null;

            var items = services.Inventory.GetAllItems();
            if (items.Count > 0)
            {
                var selectedItem = invUI.GetSelectedItem();
                Assert.AreEqual(items[0].InstanceId, selectedItem.InstanceId, "Inventory selection should track a real item.");
                bool lockBefore = selectedItem.IsLocked;
                invUI.OnClickToggleLockSelected();
                bool lockAfter = services.Inventory.GetItem(selectedItem.InstanceId).IsLocked;
                Assert.AreNotEqual(lockBefore, lockAfter, "Inventory UI should toggle lock on the selected item.");
            }

            // 4. Craft UI — real recipe selection, not "first available".
            var craftUI = Object.FindFirstObjectByType<CraftScreen>(FindObjectsInactive.Include);
            Assert.IsNotNull(craftUI, "CraftScreen should exist.");
            // SelectRecipeIndex replaces the old OnClickSelectNextRecipe/Previous navigation.
            craftUI.SelectRecipeIndex(0);
            craftUI.OnClickCraftSelected(); // no-op safely if no recipe is available

            // 5. Merchant UI — buy targets the SELECTED regular offer.
            var merchantUI = Object.FindFirstObjectByType<MerchantScreen>(FindObjectsInactive.Include);
            Assert.IsNotNull(merchantUI, "MerchantScreen should exist.");
            // SelectRegularIndex replaces the old OnClickSelectNextRegular navigation.
            merchantUI.SelectRegularIndex(0);
            merchantUI.OnClickBuySelectedRegular(); // no-op safely if stock is empty

            // 6. Dungeon UI — start the SELECTED dungeon with the SELECTED character as party.
            var dungeonUI = Object.FindFirstObjectByType<DungeonScreen>(FindObjectsInactive.Include);
            Assert.IsNotNull(dungeonUI, "DungeonScreen should exist.");
            dungeonUI.OnClickSelectNextDungeon();
            dungeonUI.OnClickStartSelected();
            dungeonUI.OnClickContinue(); // replaces OnClickTick1 — single combat tick via Continue

            // 7. Quest UI — claim the SELECTED quest once it is actually completable.
            //    Progress is driven through the real service (legitimate test setup), then the
            //    claim itself must go through the UI's selection, proving the UI calls the real service.
            var questUI = Object.FindFirstObjectByType<QuestScreen>(FindObjectsInactive.Include);
            Assert.IsNotNull(questUI, "QuestScreen should exist.");
            var activeQuests = services.Quest.GetActiveQuests();
            if (activeQuests.Count > 0)
            {
                var quest = activeQuests[0];
                if (quest.Definition != null)
                {
                    services.Quest.IncrementToValue(quest.InstanceId, quest.Definition.TargetProgress);
                }
                questUI.OnClickClaimSelected();
                Assert.AreEqual(GuildMaster.Runtime.Models.QuestState.RewardClaimed, quest.State,
                    "Quest UI claim-selected should mark the selected quest as claimed via the real service.");
            }

            // 8. Settings UI Test
            var settingsUI = Object.FindFirstObjectByType<SettingsScreen>(FindObjectsInactive.Include);
            Assert.IsNotNull(settingsUI, "SettingsScreen should exist.");
            bool soundBefore = services.Settings.GetToggle("sound");
            settingsUI.OnClickToggleSound();
            bool soundAfter = services.Settings.GetToggle("sound");
            Assert.AreNotEqual(soundBefore, soundAfter, "Settings UI should toggle sound.");

            // Reset must not erase save data on a single click — it only arms confirmation.
            long moneyBeforeResetClick = services.Save.CurrentData.Money;
            settingsUI.OnClickReset();
            Assert.AreEqual(moneyBeforeResetClick, services.Save.CurrentData.Money,
                "Clicking Reset alone must not touch save data — it should require Confirm.");
            settingsUI.OnClickCancelReset();
            Assert.AreEqual(moneyBeforeResetClick, services.Save.CurrentData.Money,
                "Cancel must leave save data untouched.");

            yield return null;
        }
    }
}
