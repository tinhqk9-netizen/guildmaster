#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using GuildMaster.Runtime.UI.Legacy;
using GuildMaster.Runtime.UI.Shell;

namespace GuildMaster.Editor.UI.Legacy
{
    /// <summary>
    /// Phase 3 verification: captures the 4 required proof screenshots
    /// (phase_3_headquarters_shell, phase_3_adventurers_shell, phase_3_drawer_open,
    /// phase_3_popup_layer) while the game is running in Play Mode, by driving
    /// <see cref="AppShellController"/>'s public API (the same methods real button clicks call)
    /// and then calling ScreenCapture.CaptureScreenshot. No mcp-unity screenshot tool exists, so
    /// this is the mechanism used instead — it exercises real runtime behavior, not a mockup.
    /// </summary>
    public static class AppShellScreenshotTool
    {
        private const string OutputDir = "Docs/Legacy_Audit/Asset_Gallery";

        [MenuItem("Tools/Guild Master/Legacy UI/Screenshot/01 Headquarters Shell")]
        public static void CaptureHeadquarters()
        {
            var shell = GetShellOrWarn();
            if (shell == null) return;

            shell.ClosePopup();
            shell.CloseDrawer();
            shell.SwitchTab(0);
            DelayFrames(2, () => Capture("phase_3_headquarters_shell.png"));
        }

        [MenuItem("Tools/Guild Master/Legacy UI/Screenshot/02 Adventurers Shell")]
        public static void CaptureAdventurers()
        {
            var shell = GetShellOrWarn();
            if (shell == null) return;

            shell.ClosePopup();
            shell.CloseDrawer();
            shell.SwitchTab(1);
            DelayFrames(2, () => Capture("phase_3_adventurers_shell.png"));
        }

        [MenuItem("Tools/Guild Master/Legacy UI/Screenshot/03 Drawer Open")]
        public static void CaptureDrawerOpen()
        {
            var shell = GetShellOrWarn();
            if (shell == null) return;

            shell.ClosePopup();
            shell.SwitchTab(0);
            shell.OpenDrawer();
            DelayFrames(2, () => Capture("phase_3_drawer_open.png"));
        }

        [MenuItem("Tools/Guild Master/Legacy UI/Screenshot/04 Popup Layer")]
        public static void CapturePopupLayer()
        {
            var shell = GetShellOrWarn();
            if (shell == null) return;

            shell.CloseDrawer();
            shell.SwitchTab(0);

            // Spawn a throwaway test popup to prove the PopupRoot/backdrop actually blocks and
            // renders above tab content + HUD. Destroyed immediately after the capture.
            GameObject testPopup = new GameObject("TestPopup_ScreenshotOnly", typeof(RectTransform));
            var rt = (RectTransform)testPopup.transform;
            var img = testPopup.AddComponent<Image>();
            img.color = LegacyUITheme.CardviewDarkBackground;

            GameObject label = new GameObject("Label", typeof(RectTransform));
            label.transform.SetParent(testPopup.transform, false);
            var labelRt = (RectTransform)label.transform;
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = Vector2.zero;
            labelRt.offsetMax = Vector2.zero;
            var text = label.AddComponent<Text>();
            text.text = "Test Popup\n(PopupRoot proof — deleted after screenshot)";
            text.alignment = TextAnchor.MiddleCenter;
            text.color = LegacyUITheme.DimWhite;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 32;

            bool opened = shell.OpenPopup(testPopup);
            if (!opened)
            {
                Debug.LogWarning("[AppShellScreenshotTool] OpenPopup refused (duplicate popup?) — capturing anyway.");
            }
            else
            {
                rt.sizeDelta = new Vector2(700, 500);
                rt.anchoredPosition = Vector2.zero;
            }

            // Give the Canvas a render frame before capturing, then clean up a frame after that.
            DelayFrames(2, () =>
            {
                Capture("phase_3_popup_layer.png");
                DelayFrames(2, () =>
                {
                    shell.ClosePopup();
                    Object.Destroy(testPopup);
                });
            });
        }

        private static void DelayFrames(int frames, System.Action action)
        {
            if (frames <= 0) { action(); return; }
            EditorApplication.delayCall += () => DelayFrames(frames - 1, action);
        }

        // ─── Phase 5A: Quarters Dialog test + screenshots (simple, non-chained steps) ──

        [MenuItem("Tools/Guild Master/Legacy UI/Test/5A Step 1 - Open Quarters + Screenshot")]
        public static void Q_Step1_Open()
        {
            var shell = GetShellOrWarn();
            if (shell == null) return;
            shell.ClosePopup();
            shell.CloseDrawer();
            shell.SwitchTab(0);
            var cardGo = GameObject.Find("Card_Quarters");
            cardGo.GetComponent<Button>().onClick.Invoke();
            DelayFrames(2, () => Capture("phase_5a_quarters_before_upgrade.png"));
        }

        [MenuItem("Tools/Guild Master/Legacy UI/Test/5A Step 2 - Click Upgrade Once + Screenshot")]
        public static void Q_Step2_UpgradeOnce()
        {
            var dialogGo = GameObject.Find("Popup_quarters");
            if (dialogGo == null) { Debug.LogError("[5A Step2] No popup open."); return; }
            var btn = FindDeepChild(dialogGo.transform, "UpgradeButton")?.GetComponent<Button>();
            var capText = FindDeepChild(dialogGo.transform, "CapacityText")?.GetComponent<Text>();
            if (btn == null) { Debug.LogError("[5A Step2] UpgradeButton not found."); return; }
            string before = capText != null ? capText.text : "?";
            btn.onClick.Invoke();
            DelayFrames(2, () =>
            {
                string after = capText != null ? capText.text : "?";
                string cardStatus = GameObject.Find("Card_Quarters/Status")?.GetComponent<Text>()?.text;
                Debug.Log($"[5A Step2] Capacity before='{before}' after='{after}'. Card status now='{cardStatus}'.");
                Capture("phase_5a_quarters_after_upgrade.png");
            });
        }

        [MenuItem("Tools/Guild Master/Legacy UI/Test/5A Step 3 - Spam Upgrade Until Disabled + Screenshot")]
        public static void Q_Step3_SpamUntilDisabled()
        {
            var dialogGo = GameObject.Find("Popup_quarters");
            if (dialogGo == null) { Debug.LogError("[5A Step3] No popup open."); return; }
            var btn = FindDeepChild(dialogGo.transform, "UpgradeButton")?.GetComponent<Button>();
            if (btn == null) { Debug.LogError("[5A Step3] UpgradeButton not found."); return; }

            int clicks = 0;
            while (btn.interactable && clicks < 30)
            {
                btn.onClick.Invoke();
                clicks++;
            }
            Debug.Log($"[5A Step3] Clicked {clicks} time(s). interactable now={btn.interactable}.");
            DelayFrames(2, () => Capture("phase_5a_quarters_disabled.png"));
        }

        [MenuItem("Tools/Guild Master/Legacy UI/Test/5A Step 4 - Close + Verify No Orphan")]
        public static void Q_Step4_CloseAndVerify()
        {
            var dialogGo = GameObject.Find("Popup_quarters");
            if (dialogGo == null) { Debug.LogError("[5A Step4] No popup open."); return; }
            var closeBtn = FindDeepChild(dialogGo.transform, "CloseButton")?.GetComponent<Button>();
            if (closeBtn == null) { Debug.LogError("[5A Step4] CloseButton not found."); return; }
            closeBtn.onClick.Invoke();
            DelayFrames(3, () =>
            {
                var shell = Object.FindFirstObjectByType<AppShellController>();
                bool stillOpen = shell != null && shell.IsPopupOpen;
                bool orphan = GameObject.Find("Popup_quarters") != null;
                Debug.Log($"[5A Step4] After Close: IsPopupOpen={stillOpen}, orphanExists={orphan}.");
            });
        }

        // ─── Phase 5B: Tavern dialog verification ──────────────────────────

        [MenuItem("Tools/Guild Master/Legacy UI/Test/5B Tavern Full Flow")]
        public static void TestTavernDialogFlow()
        {
            var shell = GetShellOrWarn();
            if (shell == null) return;

            shell.ClosePopup();
            shell.CloseDrawer();
            shell.SwitchTab(0);

            var cardGo = GameObject.Find("Card_Tavern");
            if (cardGo == null || !cardGo.TryGetComponent<Button>(out var cardButton))
            {
                Debug.LogError("[5B Tavern] Card_Tavern not found or has no Button.");
                return;
            }

            cardButton.onClick.Invoke();
            DelayFrames(2, () =>
            {
                var dialog = GameObject.Find("Popup_tavern");
                if (dialog == null)
                {
                    Debug.LogError("[5B Tavern] Popup_tavern did not open.");
                    return;
                }

                Capture("phase_5b_tavern.png");
                // CaptureScreenshot is asynchronous. Leave the popup rendered for a few frames
                // between captures so a later Close does not overwrite the earlier proof image.
                DelayFrames(3, () =>
                {
                    var recruit = FindDeepChild(dialog.transform, "RecruitButton")?.GetComponent<Button>();
                    bool hasGuest = recruit != null;
                    if (hasGuest)
                    {
                        Capture("phase_5b_tavern_guest.png");
                        DelayFrames(3, () => ContinueTavernFlow(dialog, recruit));
                    }
                    else
                    {
                        ContinueTavernFlow(dialog, null);
                    }
                });
            });
        }

        private static void ContinueTavernFlow(GameObject dialog, Button recruit)
        {
            if (recruit == null || !recruit.interactable)
            {
                Capture("phase_5b_tavern_disabled.png");
                Debug.Log("[5B Tavern] Recruit disabled: no valid guest or quarters capacity is full.");
                DelayFrames(3, () => CloseTavernAndVerify(dialog));
                return;
            }

            recruit.onClick.Invoke();
            DelayFrames(3, () =>
            {
                Debug.Log($"[5B Tavern] Recruit invoked. Guest cards now={dialog.GetComponentsInChildren<Button>(true).Length}.");
                Capture("phase_5b_tavern_after_recruit.png");
                DelayFrames(3, () => CloseTavernAndVerify(dialog));
            });
        }

        private static void CloseTavernAndVerify(GameObject dialog)
        {
            var close = FindDeepChild(dialog.transform, "CloseButton")?.GetComponent<Button>();
            if (close == null)
            {
                Debug.LogError("[5B Tavern] CloseButton not found.");
                return;
            }

            close.onClick.Invoke();
            DelayFrames(3, () =>
            {
                var shell = Object.FindFirstObjectByType<AppShellController>();
                bool stillOpen = shell != null && shell.IsPopupOpen;
                bool orphan = GameObject.Find("Popup_tavern") != null;
                Debug.Log($"[5B Tavern] After Close: IsPopupOpen={stillOpen}, orphanExists={orphan}.");
            });
        }

        /// <summary>By-name search using GetComponentsInChildren (proven reliable pattern from
        /// Phase 4's 6-card test), instead of Transform.Find's "/" path syntax.</summary>
        private static Transform FindDeepChild(Transform parent, string targetName)
        {
            foreach (Transform t in parent.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == targetName) return t;
            }
            return null;
        }

        // ─── Phase 5A: Quarters Dialog test + screenshots ───────────────────

        [MenuItem("Tools/Guild Master/Legacy UI/Test/Test Quarters Dialog Full Flow")]
        public static void TestQuartersDialogFlow()
        {
            var shell = GetShellOrWarn();
            if (shell == null) return;

            shell.ClosePopup();
            shell.CloseDrawer();
            shell.SwitchTab(0);

            var cardGo = GameObject.Find("Card_Quarters");
            if (cardGo == null || !cardGo.TryGetComponent<Button>(out var cardBtn))
            {
                Debug.LogWarning("[AppShellScreenshotTool] Card_Quarters not found.");
                return;
            }

            string cardStatusBefore = GameObject.Find("Card_Quarters/Status")?.GetComponent<Text>()?.text;
            cardBtn.onClick.Invoke();

            DelayFrames(2, () =>
            {
                Capture("phase_5a_quarters_before_upgrade.png");

                var dialogGo = GameObject.Find("Popup_quarters");
                if (dialogGo == null) { Debug.LogError("[TestQuartersDialogFlow] Dialog did not open."); return; }

                var upgradeBtnGo = dialogGo.transform.Find("UpgradeSection/UpgradeButton");
                var capacityText = dialogGo.transform.Find("CapacityText")?.GetComponent<Text>();
                string capBefore = capacityText != null ? capacityText.text : "?";

                if (upgradeBtnGo == null || !upgradeBtnGo.TryGetComponent<Button>(out var upgradeBtn))
                {
                    Debug.LogError("[TestQuartersDialogFlow] UpgradeButton not found in dialog.");
                    return;
                }

                upgradeBtn.onClick.Invoke(); // 1st upgrade — should succeed (starting money=20)

                DelayFrames(2, () =>
                {
                    string capAfter = capacityText != null ? capacityText.text : "?";
                    string cardStatusAfter = GameObject.Find("Card_Quarters/Status")?.GetComponent<Text>()?.text;
                    Capture("phase_5a_quarters_after_upgrade.png");

                    Debug.Log($"[TestQuartersDialogFlow] Capacity before='{capBefore}' after='{capAfter}'. " +
                              $"HeadquartersHub card status before='{cardStatusBefore}' after='{cardStatusAfter}'.");

                    // Keep upgrading until unaffordable (starting money is small, so this should
                    // hit "not enough money" quickly) to capture the disabled state.
                    RepeatUpgradeUntilDisabled(upgradeBtn, dialogGo, 10);
                });
            });
        }

        private static void RepeatUpgradeUntilDisabled(Button upgradeBtn, GameObject dialogGo, int attemptsLeft)
        {
            if (attemptsLeft <= 0)
            {
                Debug.LogWarning("[TestQuartersDialogFlow] Ran out of attempts without hitting a disabled state.");
                return;
            }

            if (!upgradeBtn.interactable)
            {
                DelayFrames(2, () =>
                {
                    Capture("phase_5a_quarters_disabled.png");
                    Debug.Log("[TestQuartersDialogFlow] Reached disabled (max level or not enough money) state.");

                    var shell = Object.FindFirstObjectByType<AppShellController>();
                    var closeBtnGo = dialogGo.transform.Find("CloseButton");
                    if (closeBtnGo != null && closeBtnGo.TryGetComponent<Button>(out var closeBtn))
                    {
                        closeBtn.onClick.Invoke();
                    }
                    DelayFrames(2, () =>
                    {
                        bool stillOpen = shell != null && shell.IsPopupOpen;
                        bool orphan = GameObject.Find("Popup_quarters") != null;
                        Debug.Log($"[TestQuartersDialogFlow] After Close: shell.IsPopupOpen={stillOpen}, orphan={orphan}.");
                    });
                });
                return;
            }

            upgradeBtn.onClick.Invoke();
            DelayFrames(1, () => RepeatUpgradeUntilDisabled(upgradeBtn, dialogGo, attemptsLeft - 1));
        }

        // ─── Phase 4 fix verification: all 6 cards, open+close+reopen-different ────

        [MenuItem("Tools/Guild Master/Legacy UI/Test/Test All 6 Building Popups")]
        public static void TestAllBuildingPopups()
        {
            var shell = GetShellOrWarn();
            if (shell == null) return;

            shell.ClosePopup();
            shell.CloseDrawer();
            shell.SwitchTab(0);

            string[] cardNames = { "Quarters", "Tavern", "Storage", "Market", "Workshop", "Shelter" };
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("[TestAllBuildingPopups] Results:");
            int pass = 0;

            foreach (string name in cardNames)
            {
                var cardGo = GameObject.Find($"Card_{name}");
                if (cardGo == null || !cardGo.TryGetComponent<Button>(out var cardBtn))
                {
                    sb.AppendLine($"  [FAIL] {name}: card/button not found");
                    continue;
                }

                // 1. Open
                cardBtn.onClick.Invoke();
                bool openedOk = shell.IsPopupOpen;

                // 2. Find the Close button inside the now-open popup and click it
                var popupGo = GameObject.Find($"Popup_{name.ToLowerInvariant()}");
                Button closeBtn = null;
                if (popupGo != null)
                {
                    var closeTransform = popupGo.transform.Find("CloseButton");
                    if (closeTransform != null) closeTransform.TryGetComponent(out closeBtn);
                }

                bool closeFound = closeBtn != null;
                if (closeFound) closeBtn.onClick.Invoke();

                // 3. Verify popup closed + destroyed (no orphan) + backdrop off
                bool closedOk = !shell.IsPopupOpen;
                bool destroyedOk = GameObject.Find($"Popup_{name.ToLowerInvariant()}") == null;

                bool allOk = openedOk && closeFound && closedOk && destroyedOk;
                if (allOk) pass++;
                sb.AppendLine($"  [{(allOk ? "PASS" : "FAIL")}] {name}: opened={openedOk} closeBtnFound={closeFound} closedAfterClick={closedOk} noOrphan={destroyedOk}");
            }

            // 4. Immediately open a different card after the loop to prove back-to-back works
            var lastCard = GameObject.Find("Card_Quarters");
            bool reopenOk = false;
            if (lastCard != null && lastCard.TryGetComponent<Button>(out var lastBtn))
            {
                lastBtn.onClick.Invoke();
                reopenOk = shell.IsPopupOpen;
                shell.ClosePopup();
            }
            sb.AppendLine($"  Reopen-after-all-closed: {(reopenOk ? "PASS" : "FAIL")}");

            sb.AppendLine($"\n  TOTAL: {pass}/{cardNames.Length} cards passed full open->close->no-orphan cycle.");
            Debug.Log(sb.ToString());
        }

        // ─── Phase 4: Headquarters Hub ──────────────────────────────────

        [MenuItem("Tools/Guild Master/Legacy UI/Screenshot/05 Headquarters Hub")]
        public static void CaptureHeadquartersHub()
        {
            var shell = GetShellOrWarn();
            if (shell == null) return;

            shell.ClosePopup();
            shell.CloseDrawer();
            shell.SwitchTab(0);

            var scroll = Object.FindFirstObjectByType<ScrollRect>();
            if (scroll != null) scroll.verticalNormalizedPosition = 1f; // top

            DelayFrames(2, () => Capture("phase_4_headquarters_hub.png"));
        }

        [MenuItem("Tools/Guild Master/Legacy UI/Screenshot/06 Headquarters Scrolled")]
        public static void CaptureHeadquartersScrolled()
        {
            var shell = GetShellOrWarn();
            if (shell == null) return;

            shell.ClosePopup();
            shell.CloseDrawer();
            shell.SwitchTab(0);

            var scroll = Object.FindFirstObjectByType<ScrollRect>();
            if (scroll != null) scroll.verticalNormalizedPosition = 0f; // bottom

            DelayFrames(2, () => Capture("phase_4_headquarters_scrolled.png"));
        }

        [MenuItem("Tools/Guild Master/Legacy UI/Screenshot/07 Building Popup")]
        public static void CaptureBuildingPopup()
        {
            var shell = GetShellOrWarn();
            if (shell == null) return;

            shell.ClosePopup();
            shell.CloseDrawer();
            shell.SwitchTab(0);

            var hub = Object.FindFirstObjectByType<HeadquartersHubController>();
            if (hub == null)
            {
                Debug.LogWarning("[AppShellScreenshotTool] No HeadquartersHubController found.");
                return;
            }

            // Click the Tavern card's button (same code path a real tap uses).
            var cardGo = GameObject.Find("Card_Tavern");
            if (cardGo == null || !cardGo.TryGetComponent<Button>(out var cardBtn))
            {
                Debug.LogWarning("[AppShellScreenshotTool] Card_Tavern button not found.");
                return;
            }
            cardBtn.onClick.Invoke();

            DelayFrames(2, () => Capture("phase_4_building_popup.png"));
        }

        // ─── Phase 5C: Storage dialog verification (simplified grid, real Item Detail) ────

        [MenuItem("Tools/Guild Master/Legacy UI/Test/5C Storage Full Flow")]
        public static void TestStorageDialogFlow()
        {
            var shell = GetShellOrWarn();
            if (shell == null) return;

            shell.ClosePopup();
            shell.CloseDrawer();
            shell.SwitchTab(0);

            var cardGo = GameObject.Find("Card_Storage");
            if (cardGo == null || !cardGo.TryGetComponent<Button>(out var cardBtn))
            {
                Debug.LogError("[5C Storage] Card_Storage not found or has no Button.");
                return;
            }

            cardBtn.onClick.Invoke();
            DelayFrames(2, () =>
            {
                var dialog = GameObject.Find("Popup_storage");
                if (dialog == null)
                {
                    Debug.LogError("[5C Storage] Popup_storage did not open.");
                    return;
                }

                Capture("phase_5c_storage.png");
                DelayFrames(3, () =>
                    Storage_FindAndCapture(dialog, "Weapon", "phase_5c_item_detail_weapon.png", () =>
                        Storage_CloseDetailThen(dialog, () =>
                            Storage_FindAndCapture(dialog, "Armor", "phase_5c_item_detail_armor.png", () =>
                                Storage_CloseDetailThen(dialog, () =>
                                    Storage_TestAction(dialog, () =>
                                        Storage_CloseDetailThen(dialog, () => Storage_CloseAndVerify(dialog))))))));
            });
        }

        /// <summary>Clicks item slots (freshly re-queried, in case the grid was rebuilt by a prior
        /// action) one by one until the resulting real Item Detail info text starts with
        /// <paramref name="categoryPrefix"/>, captures a screenshot, then calls <paramref name="next"/>.</summary>
        private static void Storage_FindAndCapture(GameObject dialog, string categoryPrefix, string screenshotName, System.Action next)
        {
            Storage_TryEachSlot(dialog, Storage_GetSlots(dialog), 0, categoryPrefix, screenshotName, next);
        }

        private static void Storage_TryEachSlot(GameObject dialog, List<Button> slots, int index, string categoryPrefix, string screenshotName, System.Action next)
        {
            if (index >= slots.Count)
            {
                Debug.LogWarning($"[5C Storage] No item with category '{categoryPrefix}' found in the current save — skipping {screenshotName}.");
                next?.Invoke();
                return;
            }

            slots[index].onClick.Invoke();
            DelayFrames(2, () =>
            {
                string info = FindDeepChild(dialog.transform, "ItemInfo")?.GetComponent<Text>()?.text ?? "";
                string name = FindDeepChild(dialog.transform, "ItemName")?.GetComponent<Text>()?.text ?? "?";
                if (info.StartsWith(categoryPrefix))
                {
                    Capture(screenshotName);
                    Debug.Log($"[5C Storage] Captured {screenshotName} for '{name}' — {info.Replace("\n", " | ")}");
                    // ScreenCapture.CaptureScreenshot is asynchronous — hold this frame on screen a
                    // few extra frames so the write actually flushes before the next step changes
                    // what's rendered (see TavernDialogBuilder's identical note from Phase 5B).
                    DelayFrames(3, () => next?.Invoke());
                }
                else
                {
                    Storage_TryEachSlot(dialog, slots, index + 1, categoryPrefix, screenshotName, next);
                }
            });
        }

        /// <summary>Walks item slots looking for one whose real Item Detail exposes an interactable
        /// Unequip/Use/Sell action (in that priority order), triggers it once, captures the result,
        /// then calls <paramref name="next"/>. Never invents an action — only clicks buttons the
        /// panel itself already enabled from real backend state.</summary>
        private static void Storage_TestAction(GameObject dialog, System.Action next)
        {
            Storage_TryActionOnSlots(dialog, Storage_GetSlots(dialog), 0, next);
        }

        private static void Storage_TryActionOnSlots(GameObject dialog, List<Button> slots, int index, System.Action next)
        {
            if (index >= slots.Count)
            {
                Debug.LogWarning("[5C Storage] No item with an available (unlocked/valid) action found in the current save — skipping phase_5c_item_detail_action.png.");
                next?.Invoke();
                return;
            }

            slots[index].onClick.Invoke();
            DelayFrames(2, () =>
            {
                var unequipBtn = FindDeepChild(dialog.transform, "UnequipButton")?.GetComponent<Button>();
                var useBtn = FindDeepChild(dialog.transform, "UseButton")?.GetComponent<Button>();
                var sellBtn = FindDeepChild(dialog.transform, "SellButton")?.GetComponent<Button>();

                Button actionBtn = null;
                string actionName = null;
                if (unequipBtn != null && unequipBtn.gameObject.activeInHierarchy && unequipBtn.interactable) { actionBtn = unequipBtn; actionName = "Unequip"; }
                else if (useBtn != null && useBtn.gameObject.activeInHierarchy && useBtn.interactable) { actionBtn = useBtn; actionName = "Use"; }
                else if (sellBtn != null && sellBtn.interactable) { actionBtn = sellBtn; actionName = "Sell"; }

                if (actionBtn == null)
                {
                    var itemCloseBtn = FindDeepChild(dialog.transform, "ItemDetailCloseButton")?.GetComponent<Button>();
                    itemCloseBtn?.onClick.Invoke();
                    DelayFrames(1, () => Storage_TryActionOnSlots(dialog, slots, index + 1, next));
                    return;
                }

                string beforeCapacity = FindDeepChild(dialog.transform, "CapacityText")?.GetComponent<Text>()?.text ?? "?";
                Debug.Log($"[5C Storage] Triggering '{actionName}' action (capacity before: {beforeCapacity}).");
                actionBtn.onClick.Invoke();
                DelayFrames(3, () =>
                {
                    string afterCapacity = FindDeepChild(dialog.transform, "CapacityText")?.GetComponent<Text>()?.text ?? "?";
                    Debug.Log($"[5C Storage] Action '{actionName}' completed. Capacity after: {afterCapacity}.");
                    Capture("phase_5c_item_detail_action.png");
                    DelayFrames(3, () => next?.Invoke());
                });
            });
        }

        private static void Storage_CloseDetailThen(GameObject dialog, System.Action next)
        {
            var itemCloseBtn = FindDeepChild(dialog.transform, "ItemDetailCloseButton")?.GetComponent<Button>();
            var overlay = FindDeepChild(dialog.transform, "ItemDetailOverlay");
            if (itemCloseBtn != null && overlay != null && overlay.gameObject.activeSelf)
            {
                itemCloseBtn.onClick.Invoke();
                DelayFrames(2, () => next?.Invoke());
            }
            else
            {
                next?.Invoke();
            }
        }

        private static List<Button> Storage_GetSlots(GameObject dialog)
        {
            var slots = new List<Button>();
            var gridContent = FindDeepChild(dialog.transform, "GridContent");
            if (gridContent != null)
            {
                foreach (Transform child in gridContent)
                {
                    if (child.TryGetComponent<Button>(out var slotBtn)) slots.Add(slotBtn);
                }
            }
            return slots;
        }

        private static void Storage_CloseAndVerify(GameObject dialog)
        {
            var closeBtn = FindDeepChild(dialog.transform, "CloseButton")?.GetComponent<Button>();
            if (closeBtn == null)
            {
                Debug.LogError("[5C Storage] CloseButton not found.");
                return;
            }
            closeBtn.onClick.Invoke();
            DelayFrames(3, () =>
            {
                var shell = Object.FindFirstObjectByType<AppShellController>();
                bool stillOpen = shell != null && shell.IsPopupOpen;
                bool orphan = GameObject.Find("Popup_storage") != null;
                Debug.Log($"[5C Storage] After Close: IsPopupOpen={stillOpen}, orphanExists={orphan}.");

                // Regression check: Quarters and Tavern must still open/close normally.
                DelayFrames(2, () => Storage_RegressionCheckQuartersTavern(shell));
            });
        }

        private static void Storage_RegressionCheckQuartersTavern(AppShellController shell)
        {
            if (shell == null) { Debug.LogError("[5C Storage] Shell missing for regression check."); return; }

            var quartersCard = GameObject.Find("Card_Quarters");
            if (quartersCard != null && quartersCard.TryGetComponent<Button>(out var quartersBtn))
            {
                quartersBtn.onClick.Invoke();
                bool quartersOpened = shell.IsPopupOpen;
                shell.ClosePopup();
                bool quartersClosed = !shell.IsPopupOpen;
                Debug.Log($"[5C Storage] Regression — Quarters opened={quartersOpened}, closedAfter={quartersClosed}.");
            }

            var tavernCard = GameObject.Find("Card_Tavern");
            if (tavernCard != null && tavernCard.TryGetComponent<Button>(out var tavernBtn))
            {
                tavernBtn.onClick.Invoke();
                bool tavernOpened = shell.IsPopupOpen;
                shell.ClosePopup();
                bool tavernClosed = !shell.IsPopupOpen;
                Debug.Log($"[5C Storage] Regression — Tavern opened={tavernOpened}, closedAfter={tavernClosed}.");
            }
        }

        // ─── Phase 5D: Workshop dialog verification ─────────────────────────

        [MenuItem("Tools/Guild Master/Legacy UI/Test/5D Workshop Full Flow")]
        public static void TestWorkshopDialogFlow()
        {
            var shell = GetShellOrWarn();
            if (shell == null) return;

            shell.ClosePopup();
            shell.CloseDrawer();
            shell.SwitchTab(0);

            var cardGo = GameObject.Find("Card_Workshop");
            if (cardGo == null || !cardGo.TryGetComponent<Button>(out var cardBtn))
            {
                Debug.LogError("[5D Workshop] Card_Workshop not found or has no Button.");
                return;
            }

            cardBtn.onClick.Invoke();
            DelayFrames(2, () =>
            {
                var dialog = GameObject.Find("Popup_workshop");
                if (dialog == null)
                {
                    Debug.LogError("[5D Workshop] Popup_workshop did not open.");
                    return;
                }

                Capture("phase_5d_workshop.png");
                DelayFrames(3, () => Workshop_CheckEmptyThen(dialog));
            });
        }

        private static void Workshop_CheckEmptyThen(GameObject dialog)
        {
            var emptyState = FindDeepChild(dialog.transform, "EmptyState");
            bool isEmpty = emptyState != null && emptyState.gameObject.activeSelf;
            Debug.Log($"[5D Workshop] Queue empty state active={isEmpty}.");

            if (isEmpty)
            {
                Capture("phase_5d_workshop_empty.png");
                DelayFrames(3, () => Workshop_OpenRecipes(dialog));
            }
            else
            {
                Workshop_CheckReadyThen(dialog);
            }
        }

        private static void Workshop_CheckReadyThen(GameObject dialog)
        {
            var listContent = FindDeepChild(dialog.transform, "ListContent");
            bool hasReadyRow = false;
            if (listContent != null)
            {
                foreach (Transform row in listContent)
                {
                    var status = FindDeepChild(row, "Status")?.GetComponent<Text>();
                    if (status != null && status.text == "Ready!") { hasReadyRow = true; break; }
                }
            }

            if (hasReadyRow)
            {
                Debug.Log("[5D Workshop] Completed/ready item found in queue list.");
                Capture("phase_5d_workshop_ready.png");
                DelayFrames(3, () => Workshop_TryCollect(dialog));
            }
            else
            {
                Workshop_OpenRecipes(dialog);
            }
        }

        private static void Workshop_TryCollect(GameObject dialog)
        {
            var listContent = FindDeepChild(dialog.transform, "ListContent");
            Button collectBtn = null;
            if (listContent != null)
            {
                foreach (Transform row in listContent)
                {
                    var btn = FindDeepChild(row, "ActionButton")?.GetComponent<Button>();
                    if (btn != null) { collectBtn = btn; break; }
                }
            }

            if (collectBtn == null)
            {
                Workshop_OpenRecipes(dialog);
                return;
            }

            string queueCountBefore = FindDeepChild(dialog.transform, "QueueCountText")?.GetComponent<Text>()?.text ?? "?";
            collectBtn.onClick.Invoke();
            DelayFrames(3, () =>
            {
                string queueCountAfter = FindDeepChild(dialog.transform, "QueueCountText")?.GetComponent<Text>()?.text ?? "?";
                Debug.Log($"[5D Workshop] Collect invoked. Queue count before='{queueCountBefore}' after='{queueCountAfter}'.");
                Workshop_OpenRecipes(dialog);
            });
        }

        private static void Workshop_OpenRecipes(GameObject dialog)
        {
            var recipesBtn = FindDeepChild(dialog.transform, "RecipesButton")?.GetComponent<Button>();
            if (recipesBtn == null)
            {
                Debug.LogError("[5D Workshop] RecipesButton not found.");
                Workshop_CloseAndVerify(dialog);
                return;
            }

            recipesBtn.onClick.Invoke();
            DelayFrames(3, () =>
            {
                Capture("phase_5d_workshop_recipes.png");
                DelayFrames(3, () => Workshop_TryCraft(dialog));
            });
        }

        private static void Workshop_TryCraft(GameObject dialog)
        {
            var recipeOverlay = FindDeepChild(dialog.transform, "RecipeOverlay");
            Button craftBtn = null;
            if (recipeOverlay != null)
            {
                foreach (var btn in recipeOverlay.GetComponentsInChildren<Button>(true))
                {
                    if (btn.name == "ActionButton" && btn.interactable) { craftBtn = btn; break; }
                }
            }

            if (craftBtn == null)
            {
                Debug.LogWarning("[5D Workshop] No recipe with an interactable Craft button — natural save cannot craft right now.");
                Workshop_CloseRecipesThen(dialog, () => Workshop_CloseAndVerify(dialog));
                return;
            }

            Debug.Log("[5D Workshop] Triggering Craft on a real recipe with a satisfied CanCraft() gate.");
            craftBtn.onClick.Invoke();
            DelayFrames(3, () =>
            {
                Workshop_CloseRecipesThen(dialog, () =>
                {
                    Capture("phase_5d_workshop_queue.png");
                    Debug.Log("[5D Workshop] Captured queue state after successful craft.");
                    DelayFrames(3, () => Workshop_CloseAndVerify(dialog));
                });
            });
        }

        private static void Workshop_CloseRecipesThen(GameObject dialog, System.Action next)
        {
            var recipeOverlay = FindDeepChild(dialog.transform, "RecipeOverlay");
            var recipeClose = FindDeepChild(dialog.transform, "RecipeCloseButton")?.GetComponent<Button>();
            if (recipeOverlay != null && recipeOverlay.gameObject.activeSelf && recipeClose != null)
            {
                recipeClose.onClick.Invoke();
                DelayFrames(2, () => next?.Invoke());
            }
            else
            {
                next?.Invoke();
            }
        }

        private static void Workshop_CloseAndVerify(GameObject dialog)
        {
            var closeBtn = FindDeepChild(dialog.transform, "CloseButton")?.GetComponent<Button>();
            if (closeBtn == null)
            {
                Debug.LogError("[5D Workshop] CloseButton not found.");
                return;
            }
            closeBtn.onClick.Invoke();
            DelayFrames(3, () =>
            {
                var shell = Object.FindFirstObjectByType<AppShellController>();
                bool stillOpen = shell != null && shell.IsPopupOpen;
                bool orphan = GameObject.Find("Popup_workshop") != null;
                Debug.Log($"[5D Workshop] After Close: IsPopupOpen={stillOpen}, orphanExists={orphan}.");

                DelayFrames(2, () => Workshop_RegressionCheck(shell));
            });
        }

        private static void Workshop_RegressionCheck(AppShellController shell)
        {
            if (shell == null) { Debug.LogError("[5D Workshop] Shell missing for regression check."); return; }

            foreach (string name in new[] { "Quarters", "Tavern", "Storage" })
            {
                var card = GameObject.Find($"Card_{name}");
                if (card != null && card.TryGetComponent<Button>(out var btn))
                {
                    btn.onClick.Invoke();
                    bool opened = shell.IsPopupOpen;
                    shell.ClosePopup();
                    bool closed = !shell.IsPopupOpen;
                    Debug.Log($"[5D Workshop] Regression — {name} opened={opened}, closedAfter={closed}.");
                }
            }
        }

        // ─── Phase 5E: Shelter dialog verification ──────────────────────────

        [MenuItem("Tools/Guild Master/Legacy UI/Test/5E Shelter Full Flow")]
        public static void TestShelterDialogFlow()
        {
            var shell = GetShellOrWarn();
            if (shell == null) return;

            shell.ClosePopup();
            shell.CloseDrawer();
            shell.SwitchTab(0);

            var cardGo = GameObject.Find("Card_Shelter");
            if (cardGo == null || !cardGo.TryGetComponent<Button>(out var cardBtn))
            {
                Debug.LogError("[5E Shelter] Card_Shelter not found or has no Button.");
                return;
            }

            cardBtn.onClick.Invoke();
            DelayFrames(2, () =>
            {
                var dialog = GameObject.Find("Popup_shelter");
                if (dialog == null) { Debug.LogError("[5E Shelter] Popup_shelter did not open."); return; }

                Capture("phase_5e_shelter.png");
                DelayFrames(3, () =>
                {
                    var emptyState = FindDeepChild(dialog.transform, "EmptyState");
                    bool isEmpty = emptyState != null && emptyState.gameObject.activeSelf;
                    Debug.Log($"[5E Shelter] Empty state active={isEmpty}.");
                    if (isEmpty) Capture("phase_5e_shelter_empty.png");
                    DelayFrames(2, () => Shelter_TryOpenPetDetail(dialog));
                });
            });
        }

        private static void Shelter_TryOpenPetDetail(GameObject dialog)
        {
            var gridContent = FindDeepChild(dialog.transform, "GridContent");
            Button firstSlot = null;
            if (gridContent != null)
            {
                foreach (Transform child in gridContent)
                {
                    if (child.TryGetComponent<Button>(out var b)) { firstSlot = b; break; }
                }
            }

            if (firstSlot == null)
            {
                Debug.LogWarning("[5E Shelter] No pets in current save — Pet Detail not exercised.");
                Shelter_CloseAndVerify(dialog);
                return;
            }

            firstSlot.onClick.Invoke();
            DelayFrames(3, () =>
            {
                Capture("phase_5e_pet_detail.png");
                var closeBtn = FindDeepChild(dialog.transform, "PetDetailCloseButton")?.GetComponent<Button>();
                closeBtn?.onClick.Invoke();
                DelayFrames(2, () => Shelter_CloseAndVerify(dialog));
            });
        }

        private static void Shelter_CloseAndVerify(GameObject dialog)
        {
            var closeBtn = FindDeepChild(dialog.transform, "CloseButton")?.GetComponent<Button>();
            if (closeBtn == null) { Debug.LogError("[5E Shelter] CloseButton not found."); return; }
            closeBtn.onClick.Invoke();
            DelayFrames(3, () =>
            {
                var shell = Object.FindFirstObjectByType<AppShellController>();
                bool stillOpen = shell != null && shell.IsPopupOpen;
                bool orphan = GameObject.Find("Popup_shelter") != null;
                Debug.Log($"[5E Shelter] After Close: IsPopupOpen={stillOpen}, orphanExists={orphan}.");
                DelayFrames(2, () => RegressionCheck("5E Shelter", shell));
            });
        }

        // ─── Phase 5F: Market dialog verification ───────────────────────────

        [MenuItem("Tools/Guild Master/Legacy UI/Test/5F Market Full Flow")]
        public static void TestMarketDialogFlow()
        {
            var shell = GetShellOrWarn();
            if (shell == null) return;

            shell.ClosePopup();
            shell.CloseDrawer();
            shell.SwitchTab(0);

            var cardGo = GameObject.Find("Card_Market");
            if (cardGo == null || !cardGo.TryGetComponent<Button>(out var cardBtn))
            {
                Debug.LogError("[5F Market] Card_Market not found or has no Button.");
                return;
            }

            cardBtn.onClick.Invoke();
            DelayFrames(2, () =>
            {
                var dialog = GameObject.Find("Popup_market");
                if (dialog == null) { Debug.LogError("[5F Market] Popup_market did not open."); return; }

                var emptyState = FindDeepChild(dialog.transform, "EmptyState");
                bool isEmpty = emptyState != null && emptyState.gameObject.activeSelf;
                Debug.Log($"[5F Market] Empty state active={isEmpty}.");

                if (isEmpty)
                {
                    Capture("phase_5f_market_empty.png");
                    DelayFrames(3, () => Market_CloseAndVerify(dialog));
                    return;
                }

                var listContent = FindDeepChild(dialog.transform, "ListContent");
                bool hasSelling = false, hasSold = false;
                if (listContent != null)
                {
                    foreach (Transform row in listContent)
                    {
                        if (row.name == "Divider_Selling") hasSelling = true;
                        if (row.name == "Divider_Sold") hasSold = true;
                    }
                }

                if (hasSelling) Capture("phase_5f_market_selling.png");
                DelayFrames(2, () =>
                {
                    if (hasSold) Capture("phase_5f_market_sold.png");
                    DelayFrames(2, () => Market_TryClaim(dialog));
                });
            });
        }

        private static void Market_TryClaim(GameObject dialog)
        {
            var listContent = FindDeepChild(dialog.transform, "ListContent");
            Button claimBtn = null;
            if (listContent != null)
            {
                foreach (Transform row in listContent)
                {
                    var btn = FindDeepChild(row, "ActionButton")?.GetComponent<Button>();
                    if (btn != null && btn.interactable)
                    {
                        var label = btn.GetComponentInChildren<Text>();
                        if (label != null && label.text == "Claim") { claimBtn = btn; break; }
                    }
                }
            }

            if (claimBtn == null)
            {
                Debug.LogWarning("[5F Market] No sold item to claim in current save — Claim not exercised.");
                Market_CloseAndVerify(dialog);
                return;
            }

            claimBtn.onClick.Invoke();
            DelayFrames(3, () =>
            {
                Debug.Log("[5F Market] Claim invoked.");
                Capture("phase_5f_market_claim.png");
                DelayFrames(2, () => Market_CloseAndVerify(dialog));
            });
        }

        private static void Market_CloseAndVerify(GameObject dialog)
        {
            var closeBtn = FindDeepChild(dialog.transform, "CloseButton")?.GetComponent<Button>();
            if (closeBtn == null) { Debug.LogError("[5F Market] CloseButton not found."); return; }
            closeBtn.onClick.Invoke();
            DelayFrames(3, () =>
            {
                var shell = Object.FindFirstObjectByType<AppShellController>();
                bool stillOpen = shell != null && shell.IsPopupOpen;
                bool orphan = GameObject.Find("Popup_market") != null;
                Debug.Log($"[5F Market] After Close: IsPopupOpen={stillOpen}, orphanExists={orphan}.");
                DelayFrames(2, () => RegressionCheck("5F Market", shell));
            });
        }

        private static void RegressionCheck(string tag, AppShellController shell)
        {
            if (shell == null) { Debug.LogError($"[{tag}] Shell missing for regression check."); return; }

            foreach (string name in new[] { "Quarters", "Tavern", "Storage", "Workshop" })
            {
                var card = GameObject.Find($"Card_{name}");
                if (card != null && card.TryGetComponent<Button>(out var btn))
                {
                    btn.onClick.Invoke();
                    bool opened = shell.IsPopupOpen;
                    shell.ClosePopup();
                    bool closed = !shell.IsPopupOpen;
                    Debug.Log($"[{tag}] Regression — {name} opened={opened}, closedAfter={closed}.");
                }
            }
        }

        private static AppShellController GetShellOrWarn()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[AppShellScreenshotTool] Not in Play Mode — AppShellController.Initialize() hasn't run yet.");
                return null;
            }

            var shell = Object.FindFirstObjectByType<AppShellController>();
            if (shell == null)
            {
                Debug.LogWarning("[AppShellScreenshotTool] No AppShellController found in the running scene.");
            }
            return shell;
        }

        private static void Capture(string fileName)
        {
            System.IO.Directory.CreateDirectory(OutputDir);
            string path = $"{OutputDir}/{fileName}";
            ScreenCapture.CaptureScreenshot(path);
            Debug.Log($"[AppShellScreenshotTool] Captured '{path}' (relative to project root).");
        }
    }
}
#endif
