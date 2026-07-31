# GuildMaster UI Trace — Raw Evidence for Audit

Read-only static analysis. All paths absolute under `D:\Tinh\Rebuild_GuildMaster\`. No Unity Editor used.

Confirmed: exactly 9 `UIScreen` subclasses exist in the codebase — CharacterScreen, CraftScreen, DungeonScreen, HUDController, InventoryScreen, MerchantScreen, PopupScreen, QuestScreen, SettingsScreen, TavernScreen (10 files, but HUDController + the other 9 = still "9 screens + HUD" per the task's own count of "9 screen classes total"; HUDController itself is also a `UIScreen` subclass, so literally 10 `UIScreen`-derived classes exist, of which HUD is the always-visible root and the other 9 are navigable screens). `UIScreenId` enum (`Assets/_Game/Scripts/Runtime/UI/UIScreenId.cs` lines 1-18) has exactly: None, Loading, MainHUD, MainMenu, Inventory, Character, Dungeon, Craft, Merchant, Settings, Tavern, Quest — 12 values, of which `Loading` and `MainMenu` have **no corresponding UIScreen subclass anywhere in the UI folder** (grep confirms no `LoadingScreen.cs` / `MainMenuScreen.cs`).

---

## Core infrastructure

### IUIService.cs — `Assets/_Game/Scripts/Runtime/UI/Core/IUIService.cs`
Interface, lines 1-18. Methods: `RegisterScreen`, `ShowScreen`, `HideScreen`, `Back`, `ShowPopup`, `ClosePopup`, `RegisterDialogScreen`, `ShowInfo`, `ShowError`, `ShowDeferred`.

### UIService.cs — `Assets/_Game/Scripts/Runtime/UI/Core/UIService.cs`
- `_screens: Dictionary<UIScreenId, UIScreen>`, `_screenStack: Stack<UIScreenId>`, `_currentPopup` (lines 8-10).
- `ShowScreen` (lines 19-39): if screenId not registered, logs `Debug.LogWarning` and returns silently — **no player-facing feedback** when a screen fails to show. Hides current screen (peek of stack), pushes new id, shows it. Stack only ever grows on `ShowScreen` (no dedup) — repeated same-screen navigation pushes duplicate stack entries.
- `Back()` (lines 49-61): if stack count <=1, no-op (silently refuses to pop root). Pops current, hides it, shows previous — but does **not** remove duplicate consecutive entries, so repeated `ShowScreen(X)` calls followed by one `Back()` do not necessarily return to the pre-X screen; can require multiple `Back()` presses (no back-button "collapse to previous distinct screen" logic).
- `ShowPopup`/`ClosePopup` (lines 63-87): single popup slot (`_currentPopup`), closes any existing popup before opening new one.
- `ShowInfo`/`ShowError`/`ShowDeferred` (lines 98-125): route through single `_dialogScreen` (a `PopupScreen`). If no dialog registered, only a `Debug.LogWarning` is emitted — **message is lost, player sees nothing**. `ShowError` (line 110-113) just prefixes title with `[Error]` and calls `ShowInfo` — no distinct visual treatment (color, icon) for errors vs info.

### UIScreen.cs — `Assets/_Game/Scripts/Runtime/UI/Core/UIScreen.cs`
Base MonoBehaviour, lines 5-19. `ScreenId`, `IsPopup` fields. `Show()`/`Hide()` just toggle `gameObject.SetActive`. No animation, no transition, no `OnHide` hook for subclasses to stop coroutines except where explicitly overridden (DungeonScreen overrides `Hide()`; others do not).

### SafeArea.cs — `Assets/_Game/Scripts/Runtime/UI/Core/SafeArea.cs`
Lines 5-41. Applies `Screen.safeArea` to a RectTransform's anchors every `Update()` if changed (polls every frame, lines 16-22). Standard safe-area pattern; no throttling beyond the equality check.

### UICardFactory.cs — `Assets/_Game/Scripts/Runtime/UI/Core/UICardFactory.cs`
Static factory used by every gameplay screen to build cards at runtime (no prefabs/art):
- `ClearContainer` (lines 22-27): destroys all children — called every `Refresh()` on every screen, meaning **the entire card list is destroyed and rebuilt from scratch on every single data change**, not incrementally diffed. Confirmed pattern across Character/Craft/Dungeon/Inventory/Merchant/Quest/Tavern `Refresh()` methods.
- `CreateCard` (lines 70-139): builds Image+HorizontalLayoutGroup+Icon(placeholder colored square, line 108-110, `iconImg.sprite` never set anywhere in this factory)+TextColumn(Title+Subtitle). Button `interactable` only set if `isInteractable`; listener only added `if (isInteractable && onClick != null)` (line 136) — cards built with `isInteractable=false` (e.g. DungeonScreen's queue/enemy/party cards, CraftScreen's queue-tab cards) are **visually indistinguishable from disabled buttons but are actually non-interactive display-only cards with a Button component that never receives a listener**.
- `CreateProgressBar` (lines 149-195): returns the fill `Image`; **caller is responsible for updating fill amount on refresh** — grep of screen code shows `CreateProgressBar` is defined but not called anywhere in Character/Craft/Dungeon/Inventory/Merchant/Quest/Tavern screens (no player-facing HP/XP/quest-progress/craft-timer bars are actually rendered — progress is text-only, e.g. QuestScreen line 115 `$"Progress: {progress}/{target}  ({pct * 100f:F0}%)"` as plain text, not a bar).
- `CreateFeedbackLabel` (lines 276-291): every screen's feedback text defaults to `SuccessColor` unless the screen explicitly recolors it after `ShowFeedback`.
- No image/icon assets are ever assigned to cards via this factory — `PlaceholderIcon` flat color only (`UITemporaryTheme.cs` line 44).

### UITemporaryTheme.cs — `Assets/_Game/Scripts/Runtime/UI/Core/UITemporaryTheme.cs`
Explicitly named "temporary" (doc comment lines 6-9: "No art assets — flat colors and Unity built-in UI primitives only"). All colors/sizes/fonts hardcoded constants (lines 13-82). Font: `Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")` used everywhere (confirmed in UICardFactory and GuildMasterUnifiedApply) — legacy Unity UI Text component, not TextMeshPro. `CardWidth=310`, `CardHeight=140`, `GuestCardHeight=160`, `HudButtonWidth=340`, `HudButtonHeight=160` (lines 66-70).

---

## Screen-by-screen trace

### CharacterScreen — `Assets/_Game/Scripts/Runtime/UI/Character/CharacterScreen.cs`
Fields (lines 24-33): `_cardContainer` (RectTransform), `_summaryText`, `_detailText` (Text), `_addToPartyButton`, `_removeFromPartyButton`, `_unequipWeaponButton`, `_unequipArmorButton`, `_unequipAccessoryButton`, `_equipButton` (Button), `_feedbackText` (Text).
- `Initialize(ServiceContainer, InventoryScreen)` (line 38) — stores `ICharacterService`, `IEquipmentService`, and a direct reference to `InventoryScreen` for cross-screen selected-item queries.
- Data displayed: `c.Definition.id`, `c.Level`, `c.CurrentHp`, `c.Experience` (line 114-115), stats via `_characterService.GetTotalStat(c, StatType.*)` for Constitution/Dexterity/Defense/Intelligence/MagicDefense/ImmunityToStatus (lines 121-126), `c.Weapon/Armor/Accessory` definition ids or "(none)" (lines 130-132), `c.Trait` if non-empty (line 134-137).
- Empty state: `Refresh()` lines 83-90 — if no characters, shows "No adventurers recruited yet. Recruit from the Tavern." and "No character selected."; `BuildCards(null)` called with null → `UICardFactory.ClearContainer` only, no "Recruit" CTA link/button generated.
- Buttons → methods: `OnClickAddToParty` (200) → local `_partyIds.Add` only, **no service call**, party membership is purely UI-local state, not persisted to a backend service or save file (confirmed: no `_characterService.AddToParty` or similar exists). `OnClickRemoveFromParty` (209) same, local only. `OnClickEquipSelectedItem` (218) → `_equipmentService.Equip(...)` tried for Weapon||Armor||Accessory in sequence (224-226), result `bool done` **is used** for feedback. `OnClickUnequipWeapon/Armor/Accessory` (232-234) → `_equipmentService.Unequip(target, slot)` — **return value (if any) is discarded**; feedback always shows "Unequipped {slot}." unconditionally (line 242) even if the call is a no-op.
- Confirmation popups: none.
- Feedback: `ShowFeedback` (line 247) colors text Success/Warning only (no Failure color use in this screen despite `UITemporaryTheme.FailureColor` existing).
- Polling/Tick: none; `Refresh()` only called from `Show()` and post-action.
- Selection UI: click-to-select cards (`SelectIndex`), plus legacy `OnClickSelectNext/Previous` cycle methods (lines 189-198) still present but **not wired to any button in `GuildMasterUnifiedApply.BuildCharacter`** (grep of that method, lines 241-271, shows no Btn_Prev/Btn_Next created for Character, unlike Tavern/Inventory/Quest/Dungeon which do get Prev/Next buttons) — dead code path, unreachable via UI.

### CraftScreen — `Assets/_Game/Scripts/Runtime/UI/Craft/CraftScreen.cs`
Fields (27-37): `_cardContainer`, `_summaryText`, `_detailText`, `_tabRecipesBtn`, `_tabQueueBtn`, `_tabCompletedBtn`, `_craftButton`, `_claimButton`, `_upgradeQueueButton`, `_feedbackText`.
- 3-tab screen (enum `CraftTab` line 24): Recipes/Queue/Completed.
- Data: recipes from `_database.GetAll<RecipeDefinition>()` (line 57-60); queue/completed from `_craftService.GetQueue()`/`GetCompletedItems()` (line 68-69); `_craftService.GetQueueCapacity()`, `GetQueueCapacityLevel()`, `GetUpgradeQueueCapacityPrice()` (108-121).
- **Bug/stub found**: `currentWeightOfItem(string id)` (lines 214-218) is hardcoded to always `return 999;` with comment "Mock or replace with actual stock inventory query if needed" — meaning the Recipes tab's ingredient-availability check (`RefreshRecipeDetail`, lines 176-212) **always reports every ingredient as available** ("√" status, line 197-199) regardless of actual inventory contents, and `canCraft` is therefore always true from this check (real gating happens only inside `_craftService.TryStartCraft`). This is a materially misleading UI: the ingredient checklist in the detail panel can show all-green when the player does not actually have the materials.
- Buttons → methods: `OnClickCraftSelected` (333) → `_craftService.TryStartCraft(r.id)`, result `.Success`/`.FailureReason` **used** for feedback (341-350). `OnClickClaimSelected` (354) → `_craftService.ClaimCompletedCraft(item.InstanceId)`, bool **used** (361-370). `OnClickUpgradeQueue` (374) → `_craftService.UpgradeQueueCapacity()`, bool **used** (378-385).
- Confirmation popups: none (upgrade spends gold with no confirm step, unlike Settings' reset flow).
- Feedback: Success/Warning colors only.
- No polling; queue "In progress..." vs "Queued" status text updates only on `Refresh()` calls (triggered by tab switch or action), not via periodic Update/Tick — so queue progress (`item.SecondsPassed`, line 260) can go stale while screen is open and idle (no auto-refresh timer).

### DungeonScreen — `Assets/_Game/Scripts/Runtime/UI/Dungeon/DungeonScreen.cs`
Fields (25-49): 3 panel GameObjects (`_panelSelect`, `_panelActive`, `_panelLoot`), Select-panel fields (`_dungeonCardContainer`, `_selectedDungeonText`, `_partyText`, `_startButton`), Active-panel fields (`_activeDungeonTitle`, `_activeTurnText`, `_activeActionText`, `_combatCardContainer`, `_continueButton`, `_autoBattleButton`), Loot-panel fields (`_lootCardContainer`, `_collectLootButton`), shared (`_summaryText`, `_feedbackText`).
- `Initialize(ServiceContainer, CharacterScreen)` (line 55) — takes direct `CharacterScreen` reference for party membership.
- Three mutually-exclusive panels chosen by state (`Refresh()`, 87-115): hasLoot → Loot panel; isActive → Active panel; else → Select panel.
- **Auto-Battle coroutine** (`AutoBattleLoop`, 305-328): `while(_autoBattleActive) { yield WaitForSeconds(0.5f); ... _dungeonService.Tick(); Refresh(); }` — this is the only screen with actual polling/auto-refresh logic. Started/stopped via `OnClickToggleAutoBattle` (286-303). `Hide()` override (70-80) explicitly stops the coroutine and resets `_autoBattleActive` — good cleanup, prevents a dangling coroutine after navigating away.
- Combat detail: `_combatCardContainer` lists Enemies divider + enemy cards (`e.Definition.id`, `HP: {e.CurrentHp}`, non-interactive `isInteractable:false`, line 188-192) then Party divider + party-id cards (also non-interactive, 194-197) — **enemy/party cards are built with `onClick:null` so they show as flat cards, not selectable** despite using `CreateCard` (which normally implies clickability).
- Buttons → methods: `OnClickStartSelected` (263) → `_dungeonService.StartDungeon(id, party)` — **return value/success not checked**; feedback always says "Dungeon started!" if dungeons.Count>0 && party.Count>0, regardless of whether `StartDungeon` actually succeeded (277-278 unconditional true branch feedback only gated on precondition, not on call result). `OnClickContinue` (280) → `_dungeonService.Tick()` — **no return value to check**. `OnClickCollectLoot` (340) → `_dungeonService.CollectDrops()` returns `int collected`, **is used** for feedback (342-343).
- No confirmation popup before starting a dungeon (which consumes the party) or before auto-battle.
- Feedback uses Success/Failure colors (line 351, unlike other screens' Success/Warning pairing).
- Legacy `OnClickSelectNextDungeon/Previous` (250-259) exist and **are wired** in `GuildMasterUnifiedApply.BuildDungeon` (lines 397-398, `Btn_PrevDungeon`/`Btn_NextDungeon` — confirmed present in scene at lines 1680, 8824 of Main.unity) — unlike CharacterScreen's dead prev/next, these are reachable.

### HUDController — `Assets/_Game/Scripts/Runtime/UI/HUD/HUDController.cs`
Fields (14-25): `_moneyText`, `_gemsText` (Text); `_inventoryButton`, `_characterButton`, `_dungeonButton`, `_craftButton`, `_merchantButton`, `_settingsButton`, `_tavernButton`, `_questButton` (Button).
- `Initialize(ISaveService, IUIService)` (line 27) — binds nav buttons in `BindButtons()` (35-45), each `onClick.AddListener(() => _uiService.ShowScreen(UIScreenId.X))` for all 8 destinations. This is the **only always-visible root screen** (`ScreenId = MainHUD`, shown first by `UIRuntimeBootstrap` line 110).
- `RefreshHUD()` (47-63): sets Money/Gems text from `_saveService.CurrentData`, with a null-safe "0"/"0" fallback branch (57-62) — one of the only screens with an explicit null-data fallback path.
- No polling — `RefreshHUD()` only called from `Initialize` and `Show()` (override at line 65-69), meaning **currency display can go stale while other screens change Money/Gems and the player never returns to HUD to see it update** (HUD is hidden while any other screen is shown, per `UIService.ShowScreen` hide/show model — screens are not stacked visually, HUD included).

### InventoryScreen — `Assets/_Game/Scripts/Runtime/UI/Inventory/InventoryScreen.cs`
Fields (23-31): `_cardContainer`, `_summaryText`, `_detailText`, `_useButton`, `_equipButton`, `_lockButton`, `_lockButtonLabel` (Text), `_sellButton`, `_feedbackText`.
- `Initialize(ServiceContainer, CharacterScreen)` (line 35).
- Data: `item.Definition.id`, `item.StackCount`, `item.IsLocked`, `item.InstanceId` (103-121); type inferred as Consumable/Equipment/Material via `ItemDefinition.Consumable` (107-108, 115-117).
- `IsEquipped(ItemRuntime)` (97-101) is a **stub that always returns false** with comment "Future: query equipment service when available" — so inventory cards never show "[Equipped]" even when an item is actually equipped on a character (equip state tracked separately on `CharacterRuntime.Weapon/Armor/Accessory`, not cross-checked here). This is a real data-consistency gap: a weapon that's equipped still shows as plain, sellable-looking stock in Inventory.
- `_equipButton` present as a field (line 27) but **no `OnClickEquip...` method or button wiring exists in this class** — `GuildMasterUnifiedApply.BuildInventory` (lines 273-299) never creates or binds an Equip button for InventoryScreen (only Lock and Use are created); equip is only reachable from CharacterScreen's own Equip button which pulls `_inventoryScreen.GetSelectedItem()`. So `_equipButton` and `RefreshActions`'s `_equipButton.interactable` logic (line 132) is **dead — field is always null in the actual built scene**, confirmed no `Btn_Equip` created under InventoryScreen in `GuildMasterUnifiedApply.cs`.
- `OnClickSellSelected` (192-198): **does not call any sell service** — just shows feedback "Go to Merchant > Sell tab to sell this item." This button exists as a field/handler but is effectively a dead-end redirect, not a real action; comment at 194-196 acknowledges "Sell is handled via MerchantScreen." Also note `_sellButton` field (line 30) exists but is **never bound** in `GuildMasterUnifiedApply.BuildInventory` (no `Bind(inv, "_sellButton", ...)` call present) — so like `_equipButton`, it is permanently null at runtime; `OnClickSellSelected` is unreachable from the actual UI at all (no button triggers it).
- Confirmation popups: none.
- Feedback: Success/Warning only.

### MerchantScreen — `Assets/_Game/Scripts/Runtime/UI/Merchant/MerchantScreen.cs`
Fields (25-35): `_cardContainer`, `_summaryText`, `_detailText`, `_tabBuyBtn`, `_tabSellBtn`, `_tabListingsBtn`, `_buyRegularButton`, `_buySpecialButton`, `_sellButton`, `_claimSoldButton`, `_feedbackText`.
- `Initialize(ServiceContainer, InventoryScreen)` (line 45).
- 3 tabs: Buy (Regular+Special stock sections)/Sell/Listings (Active+Sold sections).
- Sell tab (197-232) reads `_inventoryScreen.GetSelectedItem()` directly — cross-screen coupling; if `_inventoryScreen` is null, shows "Inventory UI not attached." (204).
- Buttons → methods: `OnClickBuySelectedRegular`/`OnClickBuySelectedSpecial` (340-358) → `_merchantService.BuyOffer(offer, isSpecial)`, bool **used**. `OnClickSellSelected` (360-380) → `_merchantService.SellItem(id, count)`, `res.Success`/`res.FailureReason` **used**, and on success explicitly calls `_inventoryScreen.Refresh()` (373) — good cross-screen sync. `OnClickClaimSold` (382-401) → `_merchantService.ClaimSoldItem(item.InstanceId)`, bool **used**.
- Confirmation popups: none for Buy/Sell (irreversible gold-spend and listing actions have no confirm step).
- Feedback: Success/Warning only.
- Note: Sell tab's card built with `null` onClick (line 220-222) — card shown for visual confirmation only, not clickable (there's only one candidate item so no selection needed, consistent with design).

### PopupScreen — `Assets/_Game/Scripts/Runtime/UI/Popup/PopupScreen.cs`
Fields (9-11): `_titleText`, `_messageText` (Text), `_okButton` (Button).
- `Awake()` (13-19): wires OK button to `Hide()` directly (no callback flexibility — cannot distinguish confirm vs info-dismiss; this popup is **info/OK-only, not a yes/no confirm dialog**).
- `ShowMessage(title, msg)` (21-26) and `ShowDeferred()` (28-31, generic "This feature is currently deferred..." message) are the only two entry points; it is the single shared dialog for the whole app (`UIService._dialogScreen`).
- **No dedicated confirm/cancel popup class exists anywhere** — the only two-step confirm pattern in the whole UI is SettingsScreen's own inline reset-confirm buttons (see below), not this shared PopupScreen. Every other "irreversible" action (dungeon start consuming party, buy/sell, upgrade spend, craft-queue upgrade spend) has **no confirmation dialog at all**, relying only on interactable-gating (enough gold) before the click.

### QuestScreen — `Assets/_Game/Scripts/Runtime/UI/Quest/QuestScreen.cs`
Fields (19-24): `_cardContainer`, `_summaryText`, `_detailText`, `_claimButton`, `_cycleDoctrineButton`, `_feedbackText`.
- `_doctrines = {"war","economy","growth"}` (line 29), cycled via `OnClickCycleDoctrine` (81-85) — no visible list/dropdown, just cycles one string and updates button label text (`RefreshCycleDoctrineButton`, 71-79).
- Data: `q.Definition.id`, `q.Progress`, `q.Definition.TargetProgress`, `q.State` (87-124); percent computed as text only (113: `{pct*100f:F0}%`), no visual progress bar (per `UICardFactory.CreateProgressBar` being unused, confirmed above).
- `OnClickClaimSelected` (146-162): calls `_questService.ClaimReward(q.InstanceId, _selectedDoctrine)` at line 154 — **the call's return value (if any) is completely discarded**; feedback message "Reward claimed for ... !" is shown unconditionally whenever `q.State == QuestState.Completed`, with no check that the claim actually succeeded server-side. This is the clearest "success shown regardless of backend result" instance in the whole UI layer.
- Confirmation popups: none.
- Feedback: Success/Warning only.

### SettingsScreen — `Assets/_Game/Scripts/Runtime/UI/Settings/SettingsScreen.cs`
Fields (20-30): `_summaryText`, `_detailText` (Text); `_toggleSoundButton`, `_toggleMusicButton`, `_toggleVibrationButton`, `_toggleNotificationsButton`, `_saveButton`, `_resetButton`, `_confirmResetButton`, `_cancelResetButton` (Button); `_feedbackText`.
- **Only screen with a genuine 2-step confirm flow** (doc comment lines 9-13): `OnClickReset()` (144-148) sets `_pendingReset=true` and shows confirm/cancel buttons (via `UpdateConfirmPanel`, 79-94, toggling `SetActive` on the 3 buttons + warning-colored feedback text "⚠ Reset will ERASE all save data. Are you sure?"); `OnClickConfirmReset()` (151-158) actually calls `_settingsService.ResetToDefault()`; `OnClickCancelReset()` (161-166) aborts.
- All toggles (Sound/Music/Vibration/Notifications) call `_settingsService.SetToggle(key, val)` then unconditionally show success feedback (98-132) — no return-value check needed since `SetToggle` presumably has no failure mode, but also **no persistence confirmation**: `OnClickSave()` (136-141) is a separate manual "Save Settings" button — toggles are **not auto-persisted**, meaning if the player navigates away without pressing Save, toggle changes may be lost (depends on `ISettingsService` internal persistence semantics, not fully verifiable from this file alone — flagged as a risk, not a confirmed bug).
- No card list (`ContentArea.parent.gameObject.SetActive(false)` in `GuildMasterUnifiedApply.BuildSettings`, line 587) — this screen's `GuildMasterValidate.CheckScreen` call passes `requireCardList: false` (validator line 67), consistent.
- Data displayed: `_settingsService.GetLanguage()`, `GetToggle("sound"/"music"/"vibration"/"notifications"/"cloud")`, `GetGameVersion()` (lines 61-75) — **`cloud` toggle is read and displayed in the text block (line 65, 73) but has no corresponding `OnClickToggleCloud()` method or button anywhere in the class or in `GuildMasterUnifiedApply.BuildSettings`** — Cloud Backup is a display-only, non-interactive settings row (read but never toggleable from UI).

### TavernScreen — `Assets/_Game/Scripts/Runtime/UI/Tavern/TavernScreen.cs`
Fields (20-30): `_cardContainer`, `_timerText`, `_populationText`, `_detailText`, `_recruitButton`, `_feedbackText`, `_upgradeQuartersButton`, `_upgradeCapacityButton`, `_upgradeTimeButton`.
- Data: `_tavernService.GetGuests()`, `GetTavernCapacity()`, `GetQuartersCapacity()`, `GetNextVisitorTimerSeconds()` (51-54); per-guest `g.DefinitionId`, `g.Level`, `g.InstanceId` (99-121).
- Timer text logic (60-68): "Tavern full" / countdown / "Visitor arriving soon..." — but **no periodic re-render**: `Refresh()` is only called on `Show()` and after actions (same non-polling pattern as Craft/Quest/Merchant/Character/Inventory) — the countdown timer text will **not tick down live while the screen is open and idle**; it's a snapshot at time of `Show()`/last action, not a live clock (`FormatTimer` at 255-264 is pure formatting, no Update-driven refresh).
- Buttons → methods: `OnClickRecruitSelected` (182-201) → `_tavernService.RecruitGuest(_selectedIndex, out _)`, bool **used** (discards the `out` result entirely via `out _`, meaning whatever character/id data the service returns on success is thrown away — only success/fail is used, not the actual recruited-entity detail beyond the pre-captured `recruitedGuestName` string). `OnClickUpgradeQuarters`/`UpgradeTavernCapacity`/`UpgradeTavernTime` (203-246) → respective `_tavernService.Upgrade*()` calls, bool **used** each time.
- Confirmation popups: none (gold-spending upgrades, no confirm step — consistent with Craft/Merchant).
- Feedback: uses Success/Failure color pairing (like Dungeon) rather than Success/Warning used by Character/Craft/Inventory/Merchant/Quest — **inconsistent feedback color scheme across screens** (3 screens use Success/Failure, 5 use Success/Warning, none document which is "correct").

---

## UIRuntimeBootstrap.cs — composition root
`Assets/_Game/Scripts/Runtime/Boot/UIRuntimeBootstrap.cs`

Confirmed exact `Initialize()` call signatures (lines 79-107):
- `hud.Initialize(Services.Save, _ui)` — `(ISaveService, IUIService)` (line 80)
- `inv.Initialize(Services, chr)` — `(ServiceContainer, CharacterScreen)` (line 87)
- `chr.Initialize(Services, inv)` — `(ServiceContainer, InventoryScreen)` (line 88) — comment lines 82-84 explicitly notes both must exist before either initializes, due to mutual cross-reference.
- `tav.Initialize(Services)` — `(ServiceContainer)` (line 91)
- `crf.Initialize(Services)` — `(ServiceContainer)` (line 94)
- `mer.Initialize(Services, inv)` — `(ServiceContainer, InventoryScreen)` (line 97)
- `dun.Initialize(Services, chr)` — `(ServiceContainer, CharacterScreen)` (line 100)
- `que.Initialize(Services)` — `(ServiceContainer)` (line 103)
- `set.Initialize(Services)` — `(ServiceContainer)` (line 106)
- Popup registered via `_ui.RegisterDialogScreen(popup)` (line 108), **not** via `Initialize()`.

All lookups use `FindFirstObjectByType<T>(FindObjectsInactive.Include)` / `FindObjectsByType<UIScreen>` (lines 64, 79, 85-86, 90, 93, 96, 99, 102, 105) — **every screen must be null-checked because `Find*ObjectByType` silently returns null if the GameObject is missing** (each call is wrapped `if (x != null) x.Initialize(...)`, lines 80, 87-88, 91, 94, 97, 100, 103, 106) — so a missing screen in the scene produces **no error, no log, just a silently uninitialized/non-functional screen** (its `_service` fields stay null, and its own `Refresh()` methods each check `if (_xService == null) return;` — so the screen would just render blank forever with no diagnostic).
- `WireBackButton` (180-192): looks for a child named exactly `Btn_Back` or `Header/Btn_Back`; if absent, **silently skips** wiring — no back navigation possible unless that exact hierarchy path exists (confirmed present as `Header/Btn_Back` per `GuildMasterValidate.CheckScreen`, line 148-150, which does assert this for every screen).
- HUD explicitly excluded from back-button wiring (line 183: `if (screen.ScreenId == UIScreenId.MainHUD) return;`) — correct, since HUD is the root.
- Try/catch wraps the entire `Start()` (lines 43-116); on exception, logs error but leaves the app in whatever partial state it reached — **no fallback UI or user-facing error screen if boot fails**.
- Save persistence: `PersistSave` called only from `OnApplicationPause(true)` and `OnApplicationQuit` (lines 170-178) — no periodic autosave, no explicit "Save" trigger from most screens (only Settings has a manual Save button, and that saves settings state via `_settingsService.SaveCurrentState()`, not necessarily the same as `_save.Save()` here — these appear to be two different persistence paths, worth flagging for the formal audit to verify no data loss window between actions and pause/quit).

---

## Editor generator scripts (edit-time, not shipped in build)

### GuildMasterUnifiedApply.cs — `Assets/_Game/Scripts/Editor/GuildMasterUnifiedApply.cs`
Single menu entrypoint `GuildMaster/UI/Apply Complete Functional Game UI` (line 36). Idempotent (`EnsureCleanScreen<T>`, lines 88-117, destroys all children of an existing screen GameObject and rebuilds).

Builds these exact screens/GameObjects (all under `UICanvas/SafeArea/ScreenRoot`, confirmed screenRoot lookup line 50):
- **Tavern** (`BuildTavern`, 200-239): GameObject `TavernScreen`. Adds `TimerText`, `PopulationText` to SummaryRow; binds `_cardContainer`→ContentArea, `_timerText`, `_populationText`, `_detailText`; ActionBar buttons `Btn_RecruitSelected` ("Recruit Selected", interactable-by-default true), `Btn_Prev` ("< Prev"), `Btn_Next` ("Next >"), `Btn_UpgradeQuarters`, `Btn_UpgradeCapacity`, `Btn_UpgradeTime` (all default false/inactive until Refresh enables them); `FeedbackText` inside DetailPanel via `UICardFactory.CreateFeedbackLabel`.
- **Character** (`BuildCharacter`, 241-271): GameObject `CharacterScreen`. `SummaryText`; ActionBar buttons `Btn_AddParty` ("Add to Party"), `Btn_RemoveParty` ("Remove from Party"), `Btn_Equip` ("Equip Item"), `Btn_UnWpn` ("Unwield"), `Btn_UnArm` ("Remove Armor"), `Btn_UnAcc` ("Remove Acc."). **No Btn_Prev/Btn_Next created here** (confirmed — CharacterScreen's `OnClickSelectNext/Previous` are unreachable dead code, see above).
- **Inventory** (`BuildInventory`, 273-299): GameObject `InventoryScreen`. `SummaryText`; ActionBar `Btn_Lock` ("Lock/Unlock"), `Btn_Use` ("Use"), `Btn_Prev`, `Btn_Next`. **No `Btn_Equip` or `Btn_Sell` created** — confirms `_equipButton`/`_sellButton` fields on InventoryScreen are permanently unbound/null in the generated scene (only `_lockButton`, `_useButton`, `_lockButtonLabel` are bound, lines 290-295).
- **Craft** (`BuildCraft`, 301-338): GameObject `CraftScreen`. `TabBar_Craft` inserted at sibling index 2 (comment line 315: Header=0, SummaryRow=1, TabBar=2, ContentScroll=3, Detail=4, ActionBar=5) with `Tab_Recipes`/`Tab_Queue`/`Tab_Completed`. ActionBar `Btn_Craft`, `Btn_Claim`, `Btn_UpgradeQueue`.
- **Merchant** (`BuildMerchant`, 340-374): GameObject `MerchantScreen`. `TabBar_Merchant` at sibling index 2, `Tab_Buy`/`Tab_Sell`/`Tab_Listings`. ActionBar `Btn_BuyRegular`, `Btn_BuySpecial`, `Btn_Sell`, `Btn_ClaimSold`.
- **Dungeon** (`BuildDungeon` + 3 sub-panel builders, 376-552): GameObject `DungeonScreen` with 3 child panels under ContentArea: `Panel_Select` (contains `DungeonCardContainer` — a `GridLayoutGroup`, cellSize = CardWidth×CardHeight = 310×140, spacing 8×8 — the **only screen using GridLayoutGroup for its card container**; all other screens' `scaffold.ContentArea` presumably use a VerticalLayoutGroup via `UIScreenLayoutBuilder.Build`, not read in this pass — flagged for follow-up if that file is needed — plus `SelectedDungeonText`, `PartyText`), `Panel_Active` (inactive by default, `SetActive(false)` line 468 — contains `ActiveDungeonTitle`, `TurnText`, `ActionText`, `CombatCardContainer`), `Panel_Loot` (inactive by default, line 530 — contains `LootCardContainer`). ActionBar: `Btn_Start`, `Btn_Continue`, `Btn_AutoBattle` ("Auto: OFF"), `Btn_Collect`, `Btn_PrevDungeon`, `Btn_NextDungeon`.
- **Quest** (`BuildQuest`, 554-576): GameObject `QuestScreen`. `SummaryText`; ActionBar `Btn_Claim` ("Claim Reward"), `Btn_CycleDoctrine` ("Doctrine: WAR"), `Btn_Prev`, `Btn_Next`.
- **Settings** (`BuildSettings`, 578-616): GameObject `SettingsScreen`. **Collapses its ContentScroll entirely** (`scaffold.ContentArea.parent.gameObject.SetActive(false)`, line 587) since it has no card list. ActionBar: `Btn_Sound`, `Btn_Music`, `Btn_Vibration`, `Btn_Notifications`, `Btn_Save`, `Btn_Reset`, `Btn_ConfirmReset` (hidden by default, line 602), `Btn_CancelReset` (hidden by default, line 603). No button for the `cloud` toggle (confirms SettingsScreen finding above).
- All ActionBar buttons wired via `UIScreenLayoutBuilder.AddActionButton` (not read in this pass — helper referenced but its own file, `UIScreenLayoutBuilder.cs`, was not part of the requested reading list).
- Font size / anchor values explicit in this file: `AddTextChild` uses full-stretch anchors (anchorMin 0,0 / anchorMax 1,1, lines 179-182) with theme-driven font size passed in per call site; `AddTabButton`/`AddTabBar` use `UITemporaryTheme.TabBarHeight` (80) and `BodyFontSize` (22, theme line 75); Dungeon panel internal texts hardcode font sizes directly instead of via theme in a few spots (e.g. `ActiveDungeonTitle` uses `UITemporaryTheme.TitleFontSize` at line 483 despite being a sub-header, not a screen title — visually it will render at 34pt, same size as the screen's own H1).

### GuildMasterValidate.cs — `Assets/_Game/Scripts/Editor/GuildMasterValidate.cs`
Read-only validator, menu `GuildMaster/Verification/Validate Applied Game UI` (line 37). What it asserts as "must exist" (this defines the project's own bar for correctness):
- Exactly 1 `Canvas`, exactly 1 `EventSystem` (lines 42-46).
- `HUDController` exists; all 8 nav button fields (`_tavernButton`, `_characterButton`, `_inventoryButton`, `_dungeonButton`, `_craftButton`, `_merchantButton`, `_questButton`, `_settingsButton`) are non-null (lines 48-58, via `CheckButtonRef`).
- For each of Tavern/Character/Inventory/Dungeon/Craft/Merchant/Quest/Settings (`CheckScreen<T>`, lines 60-67): exactly one component instance exists (117-122); has `Header`, `DetailPanel`, `ActionBar` children (125-127); if `requireCardList` (all except Settings): `ContentScroll` child with a `ScrollRect` component and a `Viewport/Content` descendant (129-136); if `requireTabBar` (Craft, Merchant only): a child whose name starts with `TabBar_` (138-146); every screen (including Settings) must have `Header/Btn_Back` with a `Button` component (148-150) — **this is the validator's own confirmation that every non-HUD screen is expected to have a back button, consistent with `UIRuntimeBootstrap.WireBackButton`'s lookup.**
- `PopupScreen` exists and has a `Btn_OK` child (69-77).
- Scans **every** `Text` component in the scene (active or not, via `Resources.FindObjectsOfTypeAll`) for a hardcoded list of legacy leftover phrases (23-35): "is not implemented yet", "Dungeon (Deferred)", "Advance Time", "Tick 1", "Tick 10", "Fast Forward x10", "Toggle Party Membership", "Add/Remove Party", "First Item", "Recruit [0]" — **this list is direct evidence that earlier iterations of this UI had these exact placeholder/mock behaviors**, meaning the "Deferred", "mock tick" and "first-item-only" interaction patterns were real, shipped-then-removed anti-patterns the team already flagged as bad — relevant precedent for the audit (e.g. `CraftScreen.currentWeightOfItem` hardcoded `999` is the same category of leftover mock the validator was built to catch, but it is not a `Text` string so this validator **cannot and does not catch it**).
- Scans every GameObject's components for null/missing scripts (92-98).
- No check for "every button has a listener" or "every service call result is used" — those categories (dead-field bindings, ignored return values) are **entirely outside this validator's coverage**, which is why they survived (e.g. `InventoryScreen._equipButton`/`_sellButton` unbound, `QuestScreen.ClaimReward` return discarded).

### UIWiringGenerator.cs — `Assets/_Game/Scripts/Editor/UIWiringGenerator.cs`
`WireScene()` (step 1 of the pipeline, called from `GuildMasterUnifiedApply.Apply()` line 46). Builds the **foundation only**, not the 8 gameplay screens' internals:
- Looks up existing `UICanvas` GameObject (must already exist in the scene — this script does **not** create the Canvas or CanvasScaler itself; those pre-exist in `Main.unity`, confirmed by scene grep below).
- `HUDVisual` under `SafeArea/HudRoot` (line 58): creates `MoneyText`, `GemsText` at hardcoded anchored positions (`(-200,800)`, `(200,800)`, lines 60-61); optional currency icons if `AssetCatalog` present (63-68); 8 nav buttons at fixed y-stacked positions 750→-300 in steps of -150 (`Btn_Tavern` y=750, `Btn_Inventory` y=600, `Btn_Character` y=450, `Btn_Dungeon` y=300 — **variable named `popupBtn` for the Dungeon button, line 73, a naming leftover/typo**, `Btn_Craft` y=150, `Btn_Merchant` y=0, `Btn_Quest` y=-150, `Btn_Settings` y=-300), each sized 400×100 (`CreateButton`, line 212) — this fixed absolute-position stacking (no layout group) risks overlap/clipping on the safe-area-adjusted HudRoot on different aspect ratios since these are raw `anchoredPosition` values, not layout-driven.
- `InventoryScreen`/`CharacterScreen`/`PopupScreen` GameObjects created here as bare components (comments lines 103-105, 109-111 explicitly state child/field wiring is deferred to `GuildMasterUnifiedApply`), set inactive by default (lines 140-142).
- `PopupScreen` gets its own `Title`/`Message`/`Btn_OK` here directly (120-128) — this is the **only screen whose child text/buttons are built in this file rather than `GuildMasterUnifiedApply`**.
- Creates `UIRuntimeBootstrap` GameObject + component if missing (135-137).
- Buttons here use `Color.gray` background / `Color.black` text (lines 216, 221) — **different styling than `UITemporaryTheme`'s ButtonPrimary/Secondary greens/grays used by `UIScreenLayoutBuilder`/`UICardFactory`** — the 8 HUD nav buttons visually do not match the themed action-bar buttons used inside each screen (raw gray/black vs. themed colors), a direct visual inconsistency between the HUD nav row and every other button in the app.

---

## Main.unity scene — confirmed committed state

CanvasScaler (`Assets/_Game/Scenes/Main.unity` lines 18576-18582):
```
m_UiScaleMode: 1        (Scale With Screen Size)
m_ReferenceResolution: {x: 1080, y: 1920}   (portrait reference)
m_ScreenMatchMode: 0    (Match Width Or Height)
m_MatchWidthOrHeight: 0.5   (50/50 blend between width and height matching)
```

ProjectSettings.asset (`D:\Tinh\Rebuild_GuildMaster\ProjectSettings\ProjectSettings.asset`):
```
defaultScreenOrientation: 0   (Auto Rotation — game does NOT force portrait despite a portrait reference resolution)
allowedAutorotateToPortrait: 1
allowedAutorotateToPortraitUpsideDown: 1
allowedAutorotateToLandscapeRight: 1
allowedAutorotateToLandscapeLeft: 1
```
**Finding**: reference resolution is explicitly portrait (1080×1920) but auto-rotation is fully enabled in all 4 directions including landscape — every screen built by `GuildMasterUnifiedApply`/`UIWiringGenerator` uses portrait-oriented fixed pixel values (e.g. HUD nav button y-stack from 750 to -300, dungeon grid cellSize 310×140) with no evidence of any landscape-specific layout branch anywhere in the 9 screen classes or the 2 editor generators read. Landscape play would likely either heavily letterbox (if `m_MatchWidthOrHeight` biases toward matching height, cutting width) or clip/overlap UI (if it stretches) — this is a plausible real usability risk for the audit to flag, though not runtime-verifiable without the Editor.

Scene root GameObject names confirmed present (grep on `m_Name:` values, `Main.unity`):
`ScreenRoot` (1319), `HudRoot` (9325), `PopupRoot` (12616), `SafeArea` (18275), `UICanvas` (18542), `EventSystem` (24074) — foundation confirmed present.
Screen components confirmed present as named GameObjects: `CharacterScreen` (4450), `DungeonScreen` (9731), `PopupScreen` (9881), `MerchantScreen` (13255), `SettingsScreen` (13479), `QuestScreen` (14664), `InventoryScreen` (15863), `CraftScreen` (20799), `TavernScreen` (25798), `HUDVisual` (22968).
Tab bars confirmed present: `TabBar_Craft` (8347), `TabBar_Merchant` (13863).
Nav buttons confirmed present: `Btn_Merchant` (1200), `Btn_Tavern` (3479), `Btn_Craft` (4026, 19032 — appears twice, once as HUD nav button and once as a screen-internal `Btn_Craft` in another context — needs disambiguation if audit needs exact hierarchy, not resolved in this pass), `Btn_Inventory` (19824), `Btn_Character` (22461), `Btn_Settings` (24154), `Btn_Dungeon` (24376), `Btn_Quest` (27409).

**Conclusion**: the scene is NOT purely a "generator runs at edit-time only, nothing committed" situation — all 9 gameplay screens + HUD + foundation are fully baked into the committed `Main.unity`. The Editor scripts (`GuildMasterUnifiedApply.cs`, `UIWiringGenerator.cs`) are the tool that PRODUCED this committed state and would be needed again only if the scene is rebuilt from scratch or the generator re-run after further code changes (per its idempotent design).

---

## Systems with NO corresponding UI screen class at all

Per the task's required-systems list (Quarters/Party/Storage/Doctrine/Raid/Workshop-vs-Craft/Shop/Pets/Shelter/Promotion/Ascension/Offline-summary), cross-referenced against the 9 confirmed screen classes (Character, Craft, Dungeon, HUD, Inventory, Merchant, Popup, Quest, Settings, Tavern):

- **Quarters**: no dedicated screen; only exists as upgrade buttons (`Btn_UpgradeQuarters`) inside **TavernScreen** (`OnClickUpgradeQuarters`, `TavernScreen.cs` line 203-216) — Quarters is a sub-feature of Tavern, not its own UI.
- **Party**: no dedicated screen; party membership is local, UI-only state inside **CharacterScreen** (`_partyIds` HashSet, line 36) with no persistence to any service (`OnClickAddToParty`/`OnClickRemoveFromParty`, lines 200-216, confirmed no service call) — not even a save-backed concept, let alone a dedicated screen.
- **Storage**: no dedicated screen; `InventoryScreen` is the closest analog (doc comment line 15 references `DialogStorage.java` as its Android source) — Storage and Inventory appear to have been merged into one screen, with no separate "Storage" (e.g. a warehouse/bank distinct from carried inventory) concept anywhere in code.
- **Doctrine**: no dedicated screen; only a 3-value cycling selector (`war`/`economy`/`growth`) inside **QuestScreen** (`_doctrines` array, `OnClickCycleDoctrine`, lines 29, 81-85) used solely to pick a reward category when claiming a quest — no Doctrine overview/management screen exists.
- **Raid**: no dedicated screen and no `IRaidService`/`RaidScreen` reference found in any of the files read; `DungeonScreen` covers PvE dungeon runs only (Select/Active/Loot), nothing labeled "Raid" appears in `UIScreenId` enum or any screen class.
- **Workshop vs Craft**: only one screen, **CraftScreen**, doc-commented as "Craft / Workshop screen" (`CraftScreen.cs` line 14) — Workshop is not a separate system, it's the same screen (its own upgrade button is literally named "Upgrade Queue" / internally "Workshop Queue capacity", `GetUpgradeQueueCapacityPrice()`), so Workshop and Craft are confirmed to be the same UI, not two systems needing reconciliation — but there is no standalone "Workshop" screen if the audit's system list expected one distinct from Craft.
- **Shop**: covered by **MerchantScreen** (Buy/Sell/Listings tabs) — no separate "Shop" screen; Merchant is presumably the intended Shop equivalent.
- **Pets**: no dedicated screen; no `PetScreen`/`IPetService` reference in any UI file read, despite a `PetDefinition.cs` existing in the Definitions folder (per the git status list in the environment info) — **Pets have a data definition but confirmed zero UI screen class**.
- **Shelter**: no dedicated screen; no reference found in any of the 9 screen classes or `UIScreenId` enum.
- **Promotion**: no dedicated screen; no reference found anywhere in the UI layer read.
- **Ascension**: no dedicated screen; no reference found anywhere in the UI layer read.
- **Offline-summary**: no dedicated screen and no popup/dialog path found — `PopupScreen`'s only entry points are `ShowMessage`/`ShowDeferred` (generic info dialog), and `UIRuntimeBootstrap.Start()` builds `GameLoopRunner` (line 58-59) which likely handles offline-time simulation at the service layer, but **no UI anywhere (screen or popup) surfaces an offline-earnings/summary report to the player on session start** — this was not verifiable beyond "no such call exists in any of the 9 screens' `Show()`/`Initialize()` methods, nor in `UIRuntimeBootstrap.Start()` which only calls `_ui.ShowScreen(UIScreenId.MainHUD)` with no offline-summary popup trigger."

**Summary**: of the 12 named required systems, **7 have zero dedicated UI screen** (Quarters, Party, Doctrine, Raid, Pets, Shelter, Promotion, Ascension, Offline-summary — that's actually 9 of 12 with no dedicated screen if counted strictly; only Storage≈Inventory, Workshop≈Craft, Shop≈Merchant are folded into an existing screen under a different name). Confirmed via absence in: `UIScreenId.cs` enum (12 values, none named Raid/Pets/Shelter/Promotion/Ascension/Doctrine/Quarters/Party), the 9 `UIScreen` subclass files read in full, and `GuildMasterUnifiedApply.cs`'s 8 `Build*` methods (no Pet/Raid/Shelter/Promotion/Ascension/Quarters/Party/Doctrine build method exists).
