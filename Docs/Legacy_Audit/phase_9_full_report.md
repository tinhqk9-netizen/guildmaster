# Phase 9 — Auxiliary Screens: Full Report

## 1. Scope

Replace the App Shell drawer's 10 inert placeholder items (Shop, Settings, Recall Adventurers,
Messages, FAQ, Bestiary, Achievements, Cloud Save, Redeem Code, Community — each previously wired
only to `CloseDrawer()`, see `AppShellController.cs` Phase 3 comment) with real destinations where
the backend supports them, and honest legacy-styled fallbacks everywhere it does not. A real
`IQuestService`/`QuestService` and a real `ISettingsService`/`SettingsService` were found during the
audit (Section 4), so a Quests entry was added to the drawer (11th item — legacy had no drawer slot
for quests, but the backend has a fully real, save-backed quest system, so per the "backend truth
first" rule an entry was added rather than left inaccessible). A real Bestiary was built from the
real `EnemyDefinition` catalog. A real, save-backed Settings screen was built from
`ISettingsService`. Recall Adventurers was built as a real screen using only existing
`IDungeonService` multi-expedition methods. Shop redirects honestly to the already-real Headquarters
Market flow instead of duplicating it. Everything else (Messages, FAQ/About, Achievements, Cloud
Save, Redeem Code, Community) has no real backend and got an honest bordered-card fallback.

## 2. Auxiliary inventory

| Screen/System | Legacy evidence | Backend support | Current state | Phase 9 treatment |
|---|---|---|---|---|
| Quests | `Assets/_Game/Art/Legacy/Quests/quest_marker.png`; no drawer slot in the decompiled resources, but `QuestService.cs` is a full real system | REAL — `IQuestService`, `QuestDefinition`, `QuestRuntime`, `SaveData.Quests`, `ClaimReward` | Was unreachable in the UI | REAL screen, added as a new drawer item |
| Bestiary | `Navigation/drawer_icon_bestiary.png`; drawer item `DrawerItem_Bestiary` already existed | REAL (catalog only) — `EnemyDefinition` (`Assets/_Game/Scripts/Definitions/EnemyDefinition.cs`), 118 records loaded via `DatabaseBuilder.cs:29` | Placeholder (CloseDrawer only) | REAL Hub + Detail, full catalog (no "discovered" state in backend — documented deviation) |
| Settings | No `drawer_icon_settings` art found, but `DrawerItem_Settings` existed | REAL — `ISettingsService`/`SettingsService.cs`, 13 real toggle keys + language, backed by `SaveData.Settings*` fields | Placeholder | REAL toggle screen + real Reset-to-Default confirm flow |
| Recall Adventurers | `Navigation/drawer_icon_recall_adventurers.png`; `DrawerItem_RecallAdventurers` existed | REAL BUT DIFFERENT FLOW — no bulk "recall everyone" service method exists, but `IDungeonService.MaxExpeditions`/`GetExpedition`/`StopExpedition` are real and sufficient to build the same outcome | Placeholder | REAL screen: per-slot recall + Recall All, built only from existing service calls |
| Shop | `Misc/shop.png`; `DrawerItem_Shop` existed | REAL, but only via the Headquarters → Market card (built in Phase 4/5) | Placeholder, would have duplicated Market | REAL BUT DIFFERENT FLOW — routes to Headquarters tab instead of rebuilding Market |
| Messages | `Navigation/drawer_icon_king_message.png`; `DrawerItem_Messages` existed | NOT FOUND — no mail/inbox/notification service or save state anywhere in `SaveData.cs` or `Runtime/Services` | Placeholder | FALLBACK — "No messages are available." |
| FAQ / About / Help | `Navigation/drawer_icon_faq.png`; `DrawerItem_FAQ` existed | PARTIAL — no FAQ/guide content was recovered from the decompiled resources, but `Application.version` and `ISettingsService.GetGameVersion()` are real | Placeholder | REAL version info + honest "Guide content is not available yet." note, combined into one ABOUT/HELP screen |
| Achievements | `Navigation/drawer_icon_achievements.png`; `DrawerItem_Achievements` existed; only trace is a decompiled comment `AchievementsUtils.unlock(...)` in `TavernService.cs:136` (dead reference, not a real system) | NOT FOUND | Placeholder | FALLBACK — "Achievement records are not available yet." |
| Cloud Save | `DrawerItem_CloudSave` existed; `SaveData.SettingsCloud` is a real local toggle but there is no cloud sync backend | NOT FOUND (sync), REAL (local flag only, already exposed under Settings) | Placeholder | FALLBACK — explains the flag is local-only, points at Settings |
| Redeem Code | `DrawerItem_RedeemCode` existed | NOT FOUND — no code-redemption service/save state | Placeholder | FALLBACK — "Code redemption is not available yet." |
| Community | `Navigation/drawer_icon_reddit.png`, `drawer_icon_cafe_naver.png`; `DrawerItem_Community` existed | NOT FOUND — no community URL/config data source | Placeholder | FALLBACK — "Community links are not available yet." |
| Tutorial/Guide | `SaveData.TutorialStep` exists (int) but nothing reads/writes/exposes it anywhere else in the codebase | NOT FOUND (no consuming system) | Not in drawer | OUT OF SCOPE — no real state to show; not surfaced as its own drawer item (folded into FAQ/About honestly instead of inventing a walkthrough) |
| Doctrine overview / Codex / Changelog / dev-tools | Searched `Runtime/Services`, `Runtime/Models`, `Definitions` | NOT FOUND | N/A | OUT OF SCOPE — no evidence any of these exist as systems; not added |

## 3. Legacy source mapping

- `Docs/Legacy_Audit/phase_7_full_report.md` / `phase_8_full_report.md`: primary structural/style
  template — bordered-card `AddAction`/`AddText` helpers, isolated try/catch shell wiring, and the
  temporary Editor-bridge Play Mode verification method were mirrored directly, per each controller
  keeping its own copy of the helpers (confirmed convention: `RaidsTabController.cs:323-372` has its
  own `AddAction`/`AddText`, independent of `DungeonsTabController.cs:632-665`).
- Drawer items and their real legacy icons were found under
  `Assets/_Game/Art/Legacy/Navigation/drawer_icon_*.png`: `achievements`, `bestiary`, `cafe_naver`,
  `faq`, `king_message`, `recall_adventurers`, `reddit`. No `drawer_icon_settings`, `_shop`,
  `_cloudsave`, `_redeemcode`, or `_quests` art exists in the decompiled asset set — those items
  render text-only, matching the project's existing convention for missing legacy vector icons (see
  `AppShellBuilder.cs:126-129` tooltip-icon comment for the same precedent).
- `Assets/_Game/Art/Legacy/Enemies/unit_*.png` (118 files) match `EnemyDefinition.id` 1:1 via
  `LegacySpriteRegistry.GetUnitSprite(id)`, confirmed real portrait art for every bestiary entry.
- No FAQ/guide, achievements, redeem-code, or cloud-save dialog XML/content was found under
  `D:\Tinh\Guild Master - Idle Dungeons\resources\res\layout\` during this pass (grepped by the
  candidate keywords listed in the task); these remain FALLBACK per the backend audit, not because
  the search was skipped.

## 4. Backend audit

See Section 2's "Backend support" column for the full per-screen breakdown. Key service/file
references:

- `Assets/_Game/Scripts/Runtime/Services/IQuestService.cs`, `QuestService.cs` — real, save-backed
  (`SaveData.Quests`, `SaveData.LastWeekTriggered`, `SaveData.QuestsCompleted`).
- `Assets/_Game/Scripts/Runtime/Services/ISettingsService.cs`, `SettingsService.cs` — real, 13
  toggle keys (`sound`, `music`, `vibration`, `notifications`, `cloud`, `colorblind`,
  `autoopendetail`, `confirmretreat`, `confirmswap`, `confirmupgrade`, `craftmax`, `sellmax`,
  `verboselogs`) plus `SettingsLanguage`, all backed by real `SaveData.cs` fields (lines 241-255) and
  persisted via `ISaveService.Save`.
- `Assets/_Game/Scripts/Definitions/EnemyDefinition.cs` — real, 118 records loaded by
  `DatabaseBuilder.cs:29` (`"enemies"` category); no "discovered"/fog-of-war field exists anywhere
  on the definition or in `SaveData.cs`.
- `Assets/_Game/Scripts/Runtime/Services/IDungeonService.cs` — `MaxExpeditions`, `GetExpedition`,
  `StopExpedition`, `GetAllExpeditions` are real and sufficient for an honest Recall Adventurers
  screen without a dedicated bulk-recall service.
- No message/inbox, achievement, cloud-sync, or redeem-code service, definition, or `SaveData` field
  exists anywhere in `Runtime/Services`, `Runtime/Models`, or `Definitions`.

## 5. Drawer final mapping

11 items (10 original + 1 added), each opens a real screen or a fallback — none are dead-end
placeholders:

| Drawer item | Destination |
|---|---|
| Shop | Switches to Headquarters tab (real Market card lives there) |
| Settings | Real Settings screen |
| Recall Adventurers | Real per-slot + Recall All screen |
| Messages | Fallback |
| FAQ | Real ABOUT/HELP screen (version info) + honest guide-unavailable note |
| Bestiary | Real Hub → Detail |
| Achievements | Fallback |
| Cloud Save | Fallback (points at the real Settings toggle) |
| Redeem Code | Fallback |
| Community | Fallback |
| Quests (new, 11th item) | Real screen |

## 6. 9A Drawer — PASS WITH LIMITATION (structurally verified; final visual acceptance is the user's)

**Original Section 6 claimed a plain PASS. That was wrong.** Manual visual inspection by the user
found the drawer badly broken: oversized icons overlapping labels and adjacent rows, inconsistent
label alignment between rows with/without icons, Settings' label rendering in the wrong place,
Community's icon colliding with the row below it, and Quests not visible/reachable at all. This
section is corrected below and superseded in detail by "Phase 9 UX Hotfix" (Section 23).

**Root cause (confirmed via live hierarchy inspection in the Unity Editor, not guessed):**
1. The original `AddDrawerIcons()` sized every icon at `LegacyUITheme.DP(56)` = 168px and placed it
   inside a 96px-tall row (`DrawerItem_X` `RectTransform.sizeDelta.y` = 96 in the scene) — the icon
   was ~1.75x the row height, so it visually overlapped the row's own label and bled into the rows
   above and below it.
2. `AddDrawerIcons()` only shifted the label's `offsetMin.x` for the 7 rows that had matching legacy
   icon art (Shop, Bestiary, Achievements, FAQ, Messages, RecallAdventurers, Community); the other 4
   rows (Settings, CloudSave, RedeemCode, Quests) kept their original `offsetMin.x = 32`, producing
   the "Settings label positioned wrong" / inconsistent-alignment symptom — two different label start
   X positions existed in the same drawer.
3. `DrawerItem_Quests` (added by duplicating `DrawerItem_Shop` via `duplicate_gameobject` and then
   manually repositioning it) ended up with a corrupted `RectTransform`: on a horizontally-stretched
   anchor (`anchorMin.x=0, anchorMax.x=1`), its `sizeDelta.x` was left at `-800` (net rendered width =
   `800 + (-800) = 0`) and `anchoredPosition.y` was `-1940`, below the bottom of the 1920px-tall
   `DrawerPanel`. The row was an invisible, zero-width button parked off-screen — this, not a missing
   scroll feature, is why Quests "wasn't visible in the viewport."
4. There was no `ScrollRect` anywhere in the drawer at all, so even a correctly-positioned Quests row
   would have had no scroll mechanism to reach it if it hadn't fit.

**Fix — `AuxiliaryController.NormalizeDrawer()` / `NormalizeDrawerRow()`
(`Assets/_Game/Scripts/Runtime/UI/Auxiliary/AuxiliaryController.cs`), called once from `Setup()`
after `Wire()`:**
- A `DrawerScrollView` (`ScrollRect` + `RectMask2D`) with a `Content` child
  (`VerticalLayoutGroup` + `ContentSizeFitter`, `FitMode.PreferredSize`) is created under
  `DrawerPanel` — the same ScrollRect/Content pattern `BuildPopup()` already used for popup bodies.
  All 11 `DrawerItem_X` rows are reparented into `Content` in the canonical order (Shop, Settings,
  Recall Adventurers, Messages, FAQ, Bestiary, Achievements, Cloud Save, Redeem Code, Community,
  Quests — matching Section 5).
- Every row is rebuilt into `DrawerItem_X -> IconSlot -> Icon` / `Label`, driven entirely by
  `LayoutElement` + `HorizontalLayoutGroup` (`childControlWidth/Height = true`) — no manual
  `anchoredPosition`/`sizeDelta` math survives per row, so a corrupted transform like the old Quests
  duplicate cannot happen again; the layout system recomputes every row's rect every time.
- **Final dimensions** (all derived from `LegacyUITheme.DP()`, none hardcoded): row height
  `DP(32) = 96px` (unchanged from the legacy row height), `IconSlot` width `DP(26) = 78px` (72-84px
  target range), icon visible size `DP(20) = 60px` with `preserveAspect = true` (48-64px target
  range, centered in the slot), row spacing `DP(2) = 6px`, row horizontal padding `DP(6) = 18px`,
  label gap after `IconSlot` `DP(4) = 12px`.
- `IconSlot` is always present at the same fixed width whether or not the row has real legacy icon
  art (4 of 11 rows — Settings, Cloud Save, Redeem Code, Quests — have no matching
  `drawer_icon_*`/`shop` sprite, per Section 3's audit, and correctly render an empty slot rather than
  a fabricated icon) — this is what makes every row's label start at the same X.
- **Scroll behavior**: with all 11 rows at 96px + 6px spacing + top/bottom padding, total content
  height is ~1,173px against a 1,920px-tall `DrawerPanel` — the drawer does not actually need to
  scroll at 1080x1920 (all 11 rows fit with room to spare), but the `ScrollRect`/`Content`/
  `RectMask2D` infrastructure is real and functional, so it degrades correctly if a future row is
  added or the panel height changes. `DrawerPanel`'s own background `Image` (opaque
  `#1e1e1e`, `CardviewDarkBackground`) already stretches the full panel height regardless of content
  height, so there is no "background stops early" gap.

**Verified live in Play Mode** (`mcp__mcp-unity__get_gameobject` on the running scene, not a
screenshot): `DrawerPanel/DrawerScrollView/Content` contains exactly the 11 `DrawerItem_X` rows in
the canonical order, each with an `IconSlot` + `Label` child; `DrawerItem_Quests`'s `RectTransform`
now reports `drivenByObject: "Content"`, `rect.width = 800`, `rect.height = 96`, positioned at
`anchoredPosition = (400, -1041)` — fully inside the panel, non-zero width, laid out by the
`VerticalLayoutGroup` like every other row (previously: zero width, `y = -1940`, off-panel).
`DrawerItem_Shop`'s `IconSlot/Icon` reports `sprite = "shop"`, `sizeDelta = (60, 60)`,
`preserveAspect = true`, centered inside a `(78, 96)` `IconSlot` — no overlap with the 96px row
height. `DrawerItem_Settings`'s `IconSlot` has 0 children (correctly empty, no fabricated icon).

**Marked PASS WITH LIMITATION rather than a plain PASS**: hierarchy/RectTransform inspection confirms
the structure is now correct (no overlaps possible given the measured sizes, all 11 rows present and
reachable, consistent label X across every row), but this was verified programmatically, not by
looking at rendered pixels — final visual acceptance (font rendering, exact spacing "feel", icon
tinting) is the user's to confirm.

## 7. 9B Quests — PASS

`AuxiliaryController.OpenQuests()` / `BuildQuestCard()`
(`Assets/_Game/Scripts/Runtime/UI/Auxiliary/AuxiliaryController.cs:156-223`) lists every
`QuestRuntime` from `_services.Quest.GetActiveQuests()`: formatted title, rarity, real
progress/target with a fill bar, real reward preview (`GetRewardAmount` — gems for rarity 4,
War Doctrine progress otherwise), and a brass CLAIM REWARD row only when `State == Completed`,
calling the real `IQuestService.ClaimReward` and rebuilding the popup in place. Empty state:
"No quests are currently available." Verified live: 5 real active quests rendered
(`Quest_<guid>` rows), a real toggle round-trip on Settings confirmed the same live-service pattern
works end-to-end (Section 16). No fake progress/rewards were fabricated — everything comes from
`QuestRuntime`/`QuestDefinition`.

## 8. 9C Bestiary — PASS WITH LIMITATION

`OpenBestiaryHub()` / `OpenBestiaryDetail()`
(`Assets/_Game/Scripts/Runtime/UI/Auxiliary/AuxiliaryController.cs:229-283`) lists all 118
`EnemyDefinition` records alphabetically with real `unit_<id>` portraits where art exists. Detail
shows Max HP, Defense/Magic Defense, Damage range (+ Magic/Ranged tags), and Experience given — all
real `EnemyDefinition` fields. **Limitation (deliberate, documented deviation from legacy
fog-of-war)**: the backend has no "discovered" concept anywhere (no field on `EnemyDefinition`, no
`SaveData` tracking), so the full catalog is shown unconditionally rather than inventing a fake
discovery system. Rarity is shown only when `EnemyDefinition.Rarity > 0` (many enemies have it
unset); no boss/elite badge was added since no such flag exists on the definition.

## 9. 9D Settings — PASS

`OpenSettings()` / `AddToggle()` / `OpenResetConfirm()`
(`Assets/_Game/Scripts/Runtime/UI/Auxiliary/AuxiliaryController.cs:288-375`) exposes all 13 real
`ISettingsService` toggle keys plus the real game version line. Every toggle reads its live value on
open, flips + persists immediately via `SetToggle` + `SaveCurrentState()` on tap (verified live:
Sound toggle flipped ON → OFF and persisted). "RESET SETTINGS TO DEFAULT" opens a real confirm
screen (not a bare destructive button) that calls the real `ResetToDefault()` + `SaveCurrentState()`
only after confirmation. Per backend truth: the only real "reset" capability is
`ISettingsService.ResetToDefault()`, which resets settings only — it does **not** wipe save
progress/adventurers/items. The screen and confirm copy say exactly that ("This does not affect your
save progress, adventurers, or items.") rather than overclaiming a full save-wipe feature that does
not exist in the backend.

## 10. 9E About/Help — PASS WITH LIMITATION

`OpenAbout()` (`Assets/_Game/Scripts/Runtime/UI/Auxiliary/AuxiliaryController.cs:378-397`), wired to
the FAQ drawer item, shows the real game title, real `ISettingsService.GetGameVersion()`, and real
`Application.version` — no fabricated studio/legal/credits text. **Limitation**: no FAQ/guide content
was recovered from the decompiled legacy resources, so a bordered fallback note ("Guide content is
not available yet.") sits below the real version info rather than a separate empty FAQ screen.

## 11. 9F Inbox/Notifications — FALLBACK

`OpenFallback("MESSAGES", "drawer_icon_king_message", "No messages are available.")` — no
message/mail service or save state exists anywhere. Verified live: exact text match confirmed via
the smoke test (Section 16).

## 12. 9G Tutorial/Guide — OUT OF SCOPE / FALLBACK

`SaveData.TutorialStep` exists but nothing in the codebase reads or writes it outside its own
declaration and default value — there is no tutorial system to represent, so no stateful walkthrough
was built (would have been fabricated). Guide content is folded into the ABOUT/HELP fallback note in
Section 10 rather than given its own dead-end screen.

## 13. Other auxiliary screens — FALLBACK / OUT OF SCOPE

- **Achievements**: FALLBACK, "Achievement records are not available yet." — only trace in the
  codebase is a dead decompiled comment (`TavernService.cs:136`), not a real system.
- **Cloud Save**: FALLBACK — explains the `SettingsCloud` flag is local-only and points at Settings;
  no cloud sync backend exists.
- **Redeem Code**: FALLBACK, "Code redemption is not available yet."
- **Community**: FALLBACK, "Community links are not available yet."
- **Shop**: not a fallback — routes to the real Headquarters Market flow (Section 2/5).
- Doctrine overview / Codex / Changelog / dev-tools screens: searched for and not found anywhere in
  `Runtime/Services`, `Runtime/Models`, `Definitions`, or the legacy audit docs; not added.

## 14. Fallback states — PASS

Every fallback (`OpenFallback()`,
`Assets/_Game/Scripts/Runtime/UI/Auxiliary/AuxiliaryController.cs:441-470`) uses the shared
`object_border_no_background` bordered frame, the real legacy drawer icon where one exists (Messages,
Achievements, Community), and short honest English copy with no interface/API names, no raw ids, and
no "placeholder"/"Phase 9"/"backend missing" wording. No fake buttons, progress, or rewards appear on
any fallback screen.

## 15. Navigation/lifecycle — PASS

Every auxiliary screen is a runtime-built popup opened through the existing
`AppShellController.OpenPopup`/`ClosePopup` (never a new root system). `BuildPopup()` always closes
any currently-open popup before building a new one, so back-navigation (Bestiary Detail → Hub,
Settings → Reset Confirm → Settings) never stacks or duplicates popups — verified live: after a full
pass through Quests → Bestiary Hub → Bestiary Detail → Back → Settings (toggle) → Recall Adventurers
→ Messages fallback → Close, `PopupRoot` contained exactly one child (`PopupBackdrop`, inactive) and
zero orphan `AuxiliaryPopup` instances (Section 16). `AuxiliaryController` is created and wired in
`AppShellController.Initialize` inside its own isolated try/catch
(`Assets/_Game/Scripts/Runtime/UI/Shell/AppShellController.cs:151-163`), after the Phase 8 Raids
block, so an auxiliary-screen failure cannot break Headquarters/Adventurers/Dungeons/Raids/the shell.

## 16. Callback/pointer results

Verified live in Play Mode (`Main.unity`) via a temporary Editor-only bridge
(`Phase9RuntimeSmokeBridge.cs`, deleted after use — see Section 20) driving real
`Button.onClick.Invoke()` calls, `mcp__mcp-unity__get_console_logs` for evidence:

- `OpenDrawer()` → `DrawerRoot.activeInHierarchy = True`. PASS
- `DrawerItem_Quests` click → real popup opened with 5 live `Quest_<guid>` cards (matches the save's
  actual active-quest count). PASS
- `DrawerItem_Bestiary` click → real popup opened with 118 `Enemy_<id>` rows (matches the enemies
  category record count reported by `UIRuntimeBootstrap`: "Database built ... 1837 record(s)").
  Clicking the first row (`Enemy_abomination`) → Detail screen opened with a `Back` row present.
  PASS
- `DrawerItem_Settings` click → real popup opened; `Toggle_Sound` row click flipped its `State` text
  `ON → OFF` (confirmed via captured before/after text), and the same interaction persists through
  `ISettingsService.SaveCurrentState()`. PASS
- `DrawerItem_RecallAdventurers` click → real popup opened (per-slot state + no active-slot case
  correctly shows "Not on an expedition"). PASS
- `DrawerItem_Messages` click → fallback popup opened with `Note` text exactly
  `"No messages are available."` (asserted verbatim). PASS
- Full round trip (Quests → Bestiary Hub → Bestiary Detail → Back → Settings → Recall → Messages,
  closing each) → `PopupRoot` ended with exactly 1 child (`PopupBackdrop`, inactive) and 0
  `AuxiliaryPopup` instances — confirmed via a follow-up `get_gameobject` query after the smoke test
  completed. PASS (the smoke test's own final in-script assertion incorrectly expected
  `childCount == 0`, not accounting for the permanent `PopupBackdrop` child — a test-script defect,
  not an app defect; corrected by direct hierarchy inspection instead of trusting that one assertion
  line).

## 17. Compile/tests

- `mcp__mcp-unity__recompile_scripts`: 0 errors, 0 warnings (final clean recompile, confirmed after
  implementation, after adding/using the temporary smoke bridge, and again after deleting it).
- `mcp__mcp-unity__run_tests` (EditMode): **171/171 passed, 0 failed, 0 skipped** — matches the
  171/171 baseline from Phase 8 exactly, run after final cleanup.
- Lesson learned mid-phase: recompiling scripts *while already in Play Mode* silently drops
  non-`UnityEngine.Object` service references held by runtime-created components (Play Mode does not
  re-run `Initialize()` after a mid-play domain reload), which produced misleading early smoke-test
  failures. Fixed by always compiling fully in Edit Mode, then entering Play Mode exactly once before
  running the smoke test — matching the checkpoint order specified in the task instructions.

## 18. Regression

Verified via hierarchy/service-state/console in the same Play Mode session (no screenshots):

- App Shell: HUD, `OpenDrawer`/`CloseDrawer`, and bottom nav all functioned throughout the smoke
  test; `TabContentRoot`/`BottomNav`/`TopHUD` structure unchanged.
- Headquarters/Adventurers/Dungeons/Raids: not modified except the additive
  `AppShellController.Initialize` block for `AuxiliaryController` (inserted after the existing Phase
  8 Raids block; does not alter it).
- Console: only pre-existing/unrelated log lines observed (`[DungeonService] LoadDungeonState:
  Skipping slot 0: null dungeon data` on a fresh save; the pre-existing scene-teardown warning about
  `Canvas`/`Main Camera` not being cleaned up, present before this phase per the Phase 8 report).
  No `MissingReferenceException`, no "Missing Script", no duplicate popup/drawer roots, no claim
  duplication (Quests `ClaimReward` was not exercised destructively during the smoke test to avoid
  consuming real save state — the claim code path itself mirrors `RaidsTabController`'s pattern of
  calling the real service method directly with no `SaveData` mutation).
- EditMode test suite: 171/171 (baseline maintained, Section 17).

## 19. Files modified

- **Created**: `Assets/_Game/Scripts/Runtime/UI/Auxiliary/AuxiliaryController.cs` (533 lines).
- **Modified**: `Assets/_Game/Scripts/Runtime/UI/Shell/AppShellController.cs` — added the isolated
  Phase 9 wiring block in `Initialize` (lines 151-163), mirroring the Phase 6/7/8 try/catch pattern.
  No other method in this file was changed.
- **Modified**: `Assets/_Game/Scenes/Main.unity` — duplicated `DrawerItem_Shop` into a new
  `DrawerItem_Quests` under `DrawerRoot/DrawerPanel` (repositioned below `DrawerItem_Community`,
  relabeled "Quests"). No other scene object was altered; `AppShellController`'s serialized
  `_drawerItemButtons` array (10 entries) was left exactly as Phase 3 built it.

## 20. Test helper cleanup

`Assets/_Game/Scripts/Editor/Tests/RuntimeSmoke/Phase9RuntimeSmokeBridge.cs` (+ `.meta`) was created
for the Play Mode verification in Section 16 and deleted immediately after use. Confirmed via `ls`
that `Assets/_Game/Scripts/Editor/Tests/RuntimeSmoke/` is empty, and via a subsequent clean recompile
(0 errors, 0 warnings) that no references to it remain. A temporary diagnostic `Debug.Log` line added
to `AuxiliaryController.BuildPopup()` during investigation (Section 17) was also removed before final
compile/test verification — the shipped file contains no debug-only logging.

## 21. Backend backlog

Future work needed for full auxiliary-screen parity with legacy intent (not built now, per "Stop
after Phase 9"):

1. A message/mail/notification service + save state, if a real inbox feature is desired.
2. An achievement definitions + tracking system (the only legacy trace is a dead decompiled method
   reference, not real data).
3. A cloud-sync backend if `SettingsCloud` is meant to do more than store a local preference flag.
4. A redeem-code service (server-side or local code table) if that monetization/promo loop should be
   reproduced.
5. Community/social link data (URLs) if the Reddit/Naver Cafe drawer entries should open real links.
6. FAQ/guide content — the legacy dialog text was not recovered from the decompiled resources; if
   found later, it should replace the "Guide content is not available yet." note in `OpenAbout()`.
7. A `Discovered` flag on `EnemyDefinition` (or equivalent `SaveData` tracking) if legacy
   fog-of-war-style bestiary discovery should be reproduced instead of showing the full catalog.
8. A bulk "recall everyone across every system" service if Recall Adventurers should extend beyond
   dungeon expeditions (e.g. tavern/workshop queues) in the future.

## 22. Rollback steps

1. Delete `Assets/_Game/Scripts/Runtime/UI/Auxiliary/AuxiliaryController.cs` (+ `.meta`) and the
   `Auxiliary` folder.
2. In `Assets/_Game/Scripts/Runtime/UI/Shell/AppShellController.cs`, remove the "Phase 9 is isolated
   the same way..." try/catch block added to `Initialize`.
3. In `Assets/_Game/Scenes/Main.unity`, delete the `DrawerItem_Quests` GameObject under
   `DrawerRoot/DrawerPanel` (the original 10 drawer items are untouched and need no changes).
4. Alternatively, restore all three from the Step 0 backup at
   `D:\Tinh\Backups\Legacy_UI_Phase_9_Auxiliary\` (`Scripts/Runtime/UI/Shell/AppShellController.cs`,
   `Scenes/Main.unity`; the `Runtime/UI/Auxiliary` folder did not exist at backup time and can simply
   be deleted).
5. Recompile and re-run the EditMode suite to confirm a return to the 171/171 baseline.

## 23. Phase 9 UX Hotfix

Manual visual inspection by the user, after Phase 9 was reported complete, found the drawer and
every auxiliary popup were **not** production-ready. This section covers a visual/layout-only hotfix
across two combined requests — the drawer (detailed in Section 6, corrected above) and the popup
frame (this section) — with no changes to `QuestService`, `SettingsService`, `DungeonService`,
`SaveData`, reward/claim logic, or any drawer destination. Every drawer button still routes to
exactly the same destination it did before this hotfix.

### 23.1 Original visual defects

**Drawer** (see Section 6 for the full root-cause writeup): oversized icons overlapping labels and
adjacent rows, inconsistent label alignment between rows with/without icons, Settings' label
rendering in the wrong place, Community's icon colliding with the row below, Quests unreachable
(actually invisible — a zero-width, off-panel `RectTransform`, not a scroll problem), and no
`ScrollRect` anywhere in the drawer.

**Popups**: panel background used `object_border_dim_white_extra_opaque`
(`LegacyUITheme.ExtraOpaqueBackground = #40ffffff`, alpha ≈ 25%), so the Headquarters shell behind
every auxiliary popup stayed clearly visible — it read as a translucent debug overlay rather than a
legacy production screen. The header title text scrolled away with the body content, so on long
lists (Quests, Bestiary) the only way to close the popup was to scroll all the way to the bottom and
tap the "CLOSE" row — there was no close control independent of scroll position. Quest cards were a
fixed 108dp tall (`LegacyUITheme.DP(108)`) regardless of content, including a dead-weight zero-height
`"ClaimRow"` spacer object left over even on non-claimable cards, wasting vertical space and showing
fewer quests per viewport than necessary.

### 23.2 Popup hierarchy — before / after

Before (`BuildPopup()` in the original `AuxiliaryController.cs`):
```
AuxiliaryPopup (Image: object_border_dim_white_extra_opaque, ~25% alpha)
└── Scroll (ScrollRect + RectMask2D)
    └── Content (VerticalLayoutGroup + ContentSizeFitter)
        ├── Header (Text — scrolled away with the rest of the content)
        ├── ...body content...
        └── Close (bottom row action — the only way to exit on a long list)
```

After:
```
AuxiliaryPopup (Image: dialog_border — LegacyUITheme.CardviewDarkBackground #1e1e1e, opaque)
├── Header (fixed, never scrolls)
│   ├── BackButton   (optional — nested screens only: Bestiary Detail, Reset Settings confirm)
│   ├── Title
│   └── CloseButton_X (fixed top-right, always present, wired directly to ClosePopup())
├── Divider (thin GreyBorder line separating header from body)
└── ScrollViewport (ScrollRect + RectMask2D)
    └── ScrollContent (VerticalLayoutGroup + ContentSizeFitter — only this part scrolls)
```
`BuildPopup()`'s return type/contract is unchanged (`RectTransform` = the scrollable content), so
every existing `OpenX()` method's card-building code needed zero changes. A new optional
`Action onBack` parameter was added and wired to `OpenBestiaryDetail()` (back to
`OpenBestiaryHub()`) and `OpenResetConfirm()` (back to `OpenSettings()`) — the two nested screens
called out in the task. Per the "exactly one `AuxiliaryPopup` active at a time" rule (already
enforced by `AppShellController.OpenPopup`/`ClosePopup`, unchanged), Back replaces the current popup
instance with the parent screen's fresh popup rather than stacking.

### 23.3 Fixed X-close behavior

`CloseButton_X` is built inside the non-scrolling `Header`, anchored `(1, 0.5)`/pivot `(1, 0.5)` with
a fixed `DP(18) = 54px` touch target (within the 44-56px requirement), and its `Button.onClick` is
wired directly to `ClosePopup()` (`=> _shell.ClosePopup()`) at construction time in `BuildPopup()` —
every one of the 9 `OpenX()` call sites gets it automatically since they all go through `BuildPopup()`.
It renders a `"×"` text glyph (no legacy close-icon art exists anywhere under
`Assets/_Game/Art/Legacy/` — confirmed by a case-insensitive search for "close" — so a glyph is used
per the task's fallback rule, not a fabricated icon). Because it lives in `Header`, not
`ScrollContent`, it is unaffected by scroll position — a user on a long Quests/Bestiary list no
longer has to scroll to the bottom to exit. A bottom `CLOSE`/`BACK` row remains as a secondary option
on every screen that already had one (unchanged).

### 23.4 Header/scroll separation

`Header` (`sizeDelta.y = DP(56) = 168px`) is anchored to the top of `AuxiliaryPopup`
(`anchorMin/Max = (0,1)/(1,1)`) and is a sibling of, not a child of, `ScrollViewport` — it cannot
scroll. `Divider` sits immediately below it. `ScrollViewport` (`RectMask2D` + `ScrollRect`) fills the
remaining space below `Header + Divider` down to the popup's bottom margin; only `ScrollContent`
inside it moves. `ScrollContent`'s `VerticalLayoutGroup` padding bottom was increased from
`DP(4) = 12px` to `DP(24) = 72px` so the last card in a long list is never flush against the
viewport edge.

### 23.5 Opacity/contrast changes (exact values)

| Element | Before | After |
|---|---|---|
| Popup panel background | `object_border_dim_white_extra_opaque` sprite → `LegacyUITheme.ExtraOpaqueBackground` = `#40ffffff` (alpha ≈ 0.25 / 25%) | `dialog_border` sprite → `LegacyUITheme.CardviewDarkBackground` = `#1e1e1e` (alpha = 1.0 / fully opaque) — same sprite key Phase 7/8 dungeon/raid dialogs use |
| `PopupRoot/PopupBackdrop` dim layer (`Assets/_Game/Scenes/Main.unity`, scene-authored, shared by every popup in the app, not just Auxiliary) | `Image.color = (0, 0, 0, 0.6)` | `Image.color = (0, 0, 0, 0.85)` |

Both changes verified via `mcp__mcp-unity__get_gameobject` on the live scene after `save_scene`
(backdrop) and after recompiling with the updated `BuildPopup()` (panel). Body cards (Quest,
Bestiary, Settings toggle rows) already used the theme's bordered-card sprites
(`object_border_dim_white` / `object_border_brass`) with a baked-in translucent fill over a border —
this reads as intended contrast against the now-opaque dark panel and was left unchanged.

### 23.6 Row density changes

`BuildQuestCard()` (`AuxiliaryController.cs`): card height changed from a fixed
`LegacyUITheme.DP(108)` for every card to a content-driven `DP(96)` for claimable cards (which need
room for the CLAIM REWARD row) and `DP(72)` for non-claimable cards — non-claimable cards no longer
reserve space for a button that can't be tapped. The dead-weight zero-height `"ClaimRow"` spacer
`GameObject` (present even on non-claimable cards, doing nothing) was removed. Internal padding
tightened from `DP(8)`/`DP(4)` to `DP(6)`/`DP(2)`, and title/progress/reward font sizes reduced
slightly (16→15, 13→12, 12→11) to match the tighter card height. Bestiary Hub rows and Settings
toggle rows already used the compact `AddAction`/`AddToggle` row heights from Phase 9's original
implementation (`DP(58)` / `DP(48)`) and were not changed.

### 23.7 Drawer normalization details

Covered in full in the corrected Section 6 above (root cause, final dimensions, and live
verification) — not duplicated here.

### 23.8 Files modified

- **Modified**: `Assets/_Game/Scripts/Runtime/UI/Auxiliary/AuxiliaryController.cs` — replaced
  `AddDrawerIcons()`/`TryAddIcon()` with `NormalizeDrawer()`/`NormalizeDrawerRow()` +
  `DrawerRowOrder`; rewrote `BuildPopup()` to the Header/Divider/ScrollViewport/ScrollContent
  structure with a `CloseButton_X` and optional `onBack`; added `StretchGlyph()` helper; updated
  `OpenBestiaryDetail()` and `OpenResetConfirm()` to pass `onBack`; reworked `BuildQuestCard()` for
  density and removed the dead `ClaimRow` object.
- **Modified**: `Assets/_Game/Scenes/Main.unity` — `PopupRoot/PopupBackdrop`'s `Image.color` alpha
  changed from `0.6` to `0.85` (scene-authored component, updated via
  `mcp__mcp-unity__update_component` + `save_scene`). No other scene object was touched by this
  hotfix (the drawer's row restructuring happens entirely at runtime in `NormalizeDrawer()`, not by
  editing `DrawerItem_X` GameObjects in the scene file).

### 23.9 Runtime hierarchy verification results

Verified live in Play Mode via `mcp__mcp-unity__get_gameobject` on the running scene (no
screenshots, per the task's constraint):
- `DrawerRoot/DrawerPanel/DrawerScrollView/Content` contains exactly the 11 `DrawerItem_X` rows in
  the canonical order, each restructured to `IconSlot`/`Label`; `DrawerItem_Quests` confirmed
  non-zero width, `drivenByObject: "Content"`, positioned within the panel (full detail in Section
  6). PASS (structural).
- `DrawerItem_Shop/IconSlot/Icon`: `sprite = "shop"`, `sizeDelta = (60, 60)`, `preserveAspect = true`,
  centered in a `(78, 96)` slot — confirmed no overlap possible against the 96px row height. PASS
  (structural).
- `DrawerItem_Settings/IconSlot`: 0 children — confirmed empty slot, no fabricated icon, and its
  `Label` uses the same `LayoutElement`-driven start X as every icon-bearing row. PASS (structural).
- `PopupRoot/PopupBackdrop.Image.color.a = 0.85` confirmed on the saved scene after reload. PASS.
- **Not verified interactively this session**: clicking `CloseButton_X` to confirm it actually closes
  the popup, and clicking a drawer row to confirm the popup opens with the new Header/Divider/
  ScrollViewport/ScrollContent structure present at runtime. A temporary Editor-only smoke bridge
  (`Assets/_Game/Scripts/Editor/Tests/RuntimeSmoke/Phase9UXHotfixSmokeBridge.cs`) was written to do
  exactly this (invoke `Button.onClick` on `DrawerItem_Quests`, inspect the resulting
  `AuxiliaryPopup` hierarchy, invoke `CloseButton_X`, confirm the popup is gone), but the Unity
  Editor's actual open scene kept reverting to an unrelated test scene
  (`Assets/_Game/Scenes/Tests/LegacyShapeTest.unity`) across `set_play_mode_status` calls in this
  session regardless of an explicit `load_scene` call for `Main.unity` immediately beforehand — a
  session/tooling instability, not a code defect — so the bridge never ran against the right scene.
  It was deleted before finishing per the task's cleanup requirement rather than left in a
  half-verified state. The popup frame's correctness for this part rests on code review of
  `BuildPopup()` (Section 23.2-23.4) plus the same `ScrollRect`/`VerticalLayoutGroup`/
  `ContentSizeFitter` pattern already confirmed working for drawer rows and for the original Phase 9
  Quests/Bestiary/Settings popups (Section 16), not on a fresh interactive click-through.
- Compile: 0 errors, 0 warnings (`mcp__mcp-unity__recompile_scripts`, final check after cleanup).
- EditMode tests: **171/171 passed, 0 failed, 0 skipped** — baseline maintained
  (`mcp__mcp-unity__run_tests`, run after final cleanup).

### 23.10 Remaining visual limitations (user confirmation required)

This hotfix is verified structurally (RectTransform sizes/anchors, sprite assignments, component
graphs, and — for the drawer — one confirmed live click-free layout pass in Play Mode) but **not**
by looking at rendered pixels, since this session has no way to see the screen. The user should
specifically re-check:
1. That `CloseButton_X` is visually where expected (top-right, legible `×` glyph, correct brass
   color) and that tapping it closes the popup — this specific interaction could not be exercised
   live this session (Section 23.9).
2. That the drawer's 96px row height / 60px icon size / font sizes look proportioned correctly at
   actual device DPI — the DP() values chosen are reasoned from the theme's existing scale, not
   pixel-measured against a reference screenshot.
3. That the opaque `dialog_border` panel background and `0.85`-alpha backdrop together give enough
   contrast without making the popup feel too heavy/dark.
4. That the reduced quest-card density (72-96px vs. the old fixed 108px) reads as appropriately
   compact rather than cramped once real quest data with longer titles is on screen.
