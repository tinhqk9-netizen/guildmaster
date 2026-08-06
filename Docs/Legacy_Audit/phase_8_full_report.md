# Phase 8 — Raids: Full Report

## 1. Scope

Replace the `Tab_Raids` technical placeholder (`TabPlaceholderView`, "Raids\n(placeholder — Phase 4+)") with a
real, backend-truthful Raids tab. The backend audit (Section 3) found that raids have real
*unlock-condition* data but zero gameplay backend (no `RaidService`, no `RaidRuntime`, no raid save
state, no team/attempt/reward system). Scope therefore settled on: a real Raids Hub listing all 12
raid definitions with correctly computed lock/unlock state (backed by real save data), and a Raid
Detail screen showing that real state plus one honest, legacy-styled fallback note for everything
the backend does not support. No Team Setup / Active Raid / Summary / Rewards screens were built —
per the CRITICAL RULES and the 8C–8F section rules, building them would require inventing mechanics
that don't exist anywhere in the backend.

## 2. Legacy source mapping

- `Docs/Legacy_Audit/legacy_navigation_flow.md`: Raids is bottom-nav tab index 3, historically hidden
  until `RaidsFragment.VISIBLE = true` (`refreshRaidsFragmentVisibility()`). The Phase 1-7 shell
  already always shows all 4 tabs (established before Phase 8, out of scope to change per the "don't
  touch Phase 1-7" rule) — noted as backlog in Section 20.
- `Docs/Legacy_Audit/legacy_screen_asset_map.csv`: only 3 raid-related rows exist —
  `dialog_refill_raid_try` (a gem-cost retry/refill dialog for a per-raid attempt-token system) and
  `layout_dungeon` referencing `epic_raid`. This confirms the legacy raid design centered on a
  limited-attempts + gem-refill monetization loop — a mechanic with zero backend equivalent here.
- Legacy art found under `Assets/_Game/Art/Legacy/`: `Navigation/bottom_nav_raids.png` (already wired
  to the tab icon by `AppShellBuilder.cs` since Phase 3), `Navigation/epic_raid.png` (generic raid
  banner, no per-raid banners exist), `Dungeons/raid_try_available.png` /
  `raid_try_unavailable.png` (attempt-token icons for the retry system above — not used, since no
  attempt system exists to represent).
- No raid-specific XML layout files were found under `D:\Tinh\Guild Master - Idle Dungeons\resources\res\layout\`
  beyond the dialog/reference above; raids reused the generic dungeon-family container/dialog
  patterns (`container_dungeon`, `dialog_dungeon_detail`-style chrome), which is why this
  implementation reuses the exact Phase 7 `DungeonsTabController` visual conventions (bordered cards,
  `object_border_*` sprites, banner-right-docked layout).
- `Docs/Legacy_Audit/phase_7_full_report.md`: primary structural/style template — Hub → Detail card
  pattern, `AddAction`/`AddText`/bordered-card helpers, isolated try/catch wiring in
  `AppShellController.Initialize`, and the temporary Editor-bridge Play Mode verification method were
  all mirrored directly from this report.

## 3. Backend audit

| Legacy feature | Backend support | Data available | UI treatment |
|---|---|---|---|
| Raid list (12 raids) | REAL | `RaidDefinition` (`Assets/_Game/Scripts/Definitions/RaidDefinition.cs`) loaded via `DatabaseBuilder.cs:37` from `Assets/StreamingAssets/GameData/raids.json` (12 records, confirmed by manifest `"raids": 12`) | REAL — Hub lists all 12 |
| Raid lock/unlock state | REAL BUT DIFFERENT FLOW | `RaidDefinition.RequiredClearDungeonId` / `RequiredClearProgress` — same fields/shape as `DungeonDefinition`; no `RaidService` exposes an `IsRaidUnlocked` method, so `RaidsTabController.IsRaidUnlocked` reuses the exact rule `DungeonService.IsDungeonUnlocked` uses (`DungeonService.cs:307-316`), reading real `SaveData.Dungeons[].MaxProgress` | REAL — computed per-card and in Detail |
| Raid display name | NOT AVAILABLE | Only `id`/`className` (legacy Java class name) exist — no name field on `RaidDefinition`/`DefinitionBase` | FALLBACK — formatted from `id` via the same `Format()`/`ToTitleCase` convention already used by `DungeonsTabController`/`DungeonScreen.FormatDungeonName` for the same reason (dungeons have no name field either) |
| Raid banner art | REAL (generic only) | `epic_raid` sprite exists in `Assets/_Game/Art/Legacy/Navigation/epic_raid.png`, retrievable via `LegacySpriteRegistry.GetSprite("epic_raid")`; no per-raid banner art exists | REAL, reused honestly across all cards (no fabricated unique art) |
| Raid team setup | NOT AVAILABLE | No raid-specific team/roster concept in any service; `IPartyService` exists but nothing in the backend ever consumes a party for a raid | FALLBACK note on Detail: "raid operations are not available yet" |
| Start Raid / attempt-token / gem refill | NOT AVAILABLE | No `RaidService`, no attempt count field anywhere in `SaveData.cs`, no currency-cost refill logic | FALLBACK (no fake button, no fake gem cost) |
| Active raid state | NOT AVAILABLE | No `RaidRuntime` class, no active-raid field in `SaveData.cs` | Not built — no screen |
| Raid summary | NOT AVAILABLE | No completion/record data for raids anywhere | Not built — no screen |
| Raid rewards / claim | NOT AVAILABLE | No reward/loot service tied to raids | Not built — no screen (FALLBACK text covers it on Detail) |
| Raids tab visibility toggle (legacy hid tab until unlock) | NOT REPRODUCED | Shell always shows all 4 tabs since Phase 3; changing `AppShellController`'s tab array is out of Phase 8 scope (Phase 1-7 must not be touched) | Backlog (Section 20) |

## 4. Supported Raid features

- List of all 12 raid definitions (`ancient_grave_digging`, `celestial_mothership`,
  `divine_archeology`, `imperial_rescue`, `kaunis`, `sleeping_planet`, `the_cultist_rebels`,
  `the_dire_descent`, `the_dreadful_ascent`, `the_lost_expedition`, `the_slime_pond`, `the_tower`).
- Real lock/unlock computation per raid, backed by actual dungeon clear-progress save data.
- Real unlock-requirement text (which dungeon + what progress) shown per locked raid.

## 5. Missing backend/data

- No `RaidService`/`IRaidService`, no `RaidRuntime`.
- No raid save state in `SaveData.cs` (no `Raids` list/section exists at all).
- No raid team, attempt, cooldown, combat, reward, or currency-cost systems.
- No display name, description, or per-raid banner art field/asset.

## 6. Final architecture

Single controller `GuildMaster.Runtime.UI.Raid.RaidsTabController`
(`Assets/_Game/Scripts/Runtime/UI/Raid/RaidsTabController.cs`), mirroring
`DungeonsTabController`'s `Setup(ServiceContainer)` / `Refresh()` / idempotent `BuildRoot()`+
`ClearContent()` rebuild-on-navigate pattern, with a 2-state `Screen` enum (`Hub`, `Detail`) — kept
intentionally small because the real backend scope is small; no sub-view classes were needed.

## 7. 8A Hub — PASS

Lists all 12 `RaidDefinition`s (alphabetical by formatted display name — no legacy ordering data
exists for raids, unlike the known `DungeonOrder` sequence). Each card shows a bordered card
(`object_border_dim_white` / `_unavailable`), the generic `epic_raid` banner, formatted title,
LOCKED/UNLOCKED status, and either the unlock requirement or "Tap for details". Locked cards show a
"LOCKED" badge. Both locked and unlocked cards route to Detail (locked raids still get to see their
requirement, matching the Dungeon Hub precedent).

## 8. 8B Detail — PASS WITH LIMITATION

Shows real header (formatted raid id), `epic_raid` banner, and real
UNLOCKED/LOCKED + requirement status line. Below that, a bordered fallback frame states plainly (no
interface/API names, no "Phase 8"/"placeholder" wording): "This raid is unlocked, but raid
operations are not available yet." (unlocked) or "Raid operations are not available yet." (locked).
Single secondary action: "BACK TO RAIDS". No primary brass action is shown because there is no real
action available for any raid state — this matches the Dungeon Hub's own precedent of using
informational text (not a button) for its "Locked" state.

## 9. 8C Team Setup — FALLBACK

No raid-specific team/assignment concept exists in the backend at all (unlike Dungeons, which has
real `IPartyService` + `DungeonService.StartExpedition` integration). Per the section rule, this was
not invented; the Detail fallback note covers it ("raid operations are not available yet").

## 10. 8D Active Raid — FALLBACK (not built)

No `RaidRuntime`/active-raid save state exists. Per the section rule ("only build this screen if the
backend actually has active-raid runtime state to show ... don't invent a fake waiting screen"), no
screen was built.

## 11. 8E Summary — FALLBACK (not built)

No raid completion/record data exists to summarize. No screen built; not reachable from anywhere.

## 12. 8F Rewards — FALLBACK

No pending-rewards/claim service exists for raids. No screen built, no fake Claim button; the Detail
fallback note covers this case honestly.

## 13. 8G Fallback states — PASS

The single Detail fallback frame uses `object_border_no_background` (bordered, legacy-consistent),
the real `epic_raid` banner above it, and plain English with no interface/API names, no raw ids, and
no "placeholder"/"Phase 8"/"backend missing" wording.

## 14. Navigation/lifecycle — PASS

`RaidsTabController.Setup()` disables the pre-existing `TabPlaceholderView` and destroys its `Label`
child, then builds `Phase8RaidsContent` once. `BuildRoot()` destroys and rebuilds this root
idempotently (mirrors `DungeonsTabController` exactly), so repeated tab switches never duplicate
roots — verified live (see Section 15). Wired into `AppShellController.Initialize` at
`Assets/_Game/Scripts/Runtime/UI/Shell/AppShellController.cs` (new isolated try/catch block, after
the existing Phase 7 block) exactly like Headquarters/Adventurers/Dungeons, so a Raids UI failure
cannot break the other tabs.

## 15. Callback/pointer results

Verified live in Play Mode (`Main.unity`) via a temporary Editor-only bridge
(`Phase8RuntimeSmokeBridge.cs`, deleted after use — see Section 19) driving real
`Button.onClick.Invoke()` calls, `mcp__mcp-unity__get_console_logs` for evidence:

- `NavCell_Raids` click → `Tab_Raids.activeInHierarchy = True`. PASS
- Tab round-trip (Raids → Dungeons → Raids) → exactly 1 `Phase8RaidsContent` root remains (no
  duplicate roots). PASS
- Hub card count = 12 (matches `raids.json` record count). PASS
- Clicking the first hub card (`Raid_ancient_grave_digging`) → Detail screen renders with fallback
  note text = `"Raid operations are not available yet."` (confirmed via the unique
  `FallbackFrame/Note` node, which only exists on Detail). PASS
- Clicking "BACK TO RAIDS" → Hub re-renders with header `"RAIDS"` and 14 children (Header +
  Subheader + 12 cards) — confirmed via a follow-up hierarchy query after the frame's deferred
  `Destroy()` calls completed. PASS

## 16. Compile/tests

- `mcp__mcp-unity__recompile_scripts`: 0 errors, 0 warnings (confirmed after implementation, again
  after adding/using the temporary smoke bridge, and again after deleting it).
- `mcp__mcp-unity__run_tests` (EditMode): **171/171 passed, 0 failed, 0 skipped** — matches the
  171/171 baseline exactly, run twice (once mid-implementation, once after final cleanup).

## 17. Regression

Verified via hierarchy/service-state/console in the same Play Mode session (no screenshots):

- App Shell: HUD/tab buttons/bottom nav functioned throughout (used repeatedly to switch tabs during
  the smoke test); drawer/popup roots untouched.
- Headquarters/Adventurers/Dungeons: not modified except the additive `AppShellController.Initialize`
  block for Raids (inserted after the existing Dungeons block, does not alter it); `Tab_Dungeons` /
  `DungeonsTabController` structure confirmed unchanged in the hierarchy dump used for the tab
  round-trip check.
- Console: only pre-existing/unrelated log lines observed (`[DungeonService] LoadDungeonState:
  Skipping slot 0: null dungeon data` — expected on a fresh save; a scene-teardown warning about
  `Canvas`/`Main Camera` not being cleaned up, unrelated to Raids and present before this phase's
  changes). No `MissingReferenceException`, no "Missing Script", no duplicate roots, no orphan
  panels, no Phase-8-caused exceptions.
- EditMode test suite: 171/171 (baseline maintained, Section 16).

## 18. Files modified

- **Created**: `Assets/_Game/Scripts/Runtime/UI/Raid/RaidsTabController.cs`.
- **Modified**: `Assets/_Game/Scripts/Runtime/UI/Shell/AppShellController.cs` (added isolated Phase 8
  wiring block in `Initialize`, mirroring the Phase 7 Dungeons block).
- **Modified**: `Assets/_Game/Scenes/Main.unity` (added `RaidsTabController` component to the
  `Tab_Raids` GameObject; `TabPlaceholderView` component left in place but is disabled at runtime by
  `RaidsTabController.Setup()`, matching how Phase 6/7 handled their tabs' pre-existing placeholder
  components).

## 19. Test helper cleanup

`Assets/_Game/Scripts/Editor/Tests/RuntimeSmoke/Phase8RuntimeSmokeBridge.cs` (+ `.meta`) was created
for the Play Mode verification in Section 15 and deleted immediately after use, along with its
now-empty containing folder check (folder retained, already empty from the prior Phase 7 cleanup).
Confirmed via `ls` that `Assets/_Game/Scripts/Editor/Tests/RuntimeSmoke/` contains no files, and via
a subsequent clean recompile (0 errors) that no references to it remain.

## 20. Backend backlog

Future work needed to make Raids a real playable feature (not built now, per the "Stop after Phase
8" instruction):

1. A `RaidService`/`IRaidService` with raid-specific save state (attempt count or cooldown, active
   raid tracking) added to `SaveData.cs`.
2. A team-assignment path for raids — either reuse `IPartyService` with a raid-aware
   `DungeonService`-equivalent, or a dedicated raid party concept if raids should not compete with
   dungeon party slots.
3. Combat resolution for raids (via `ICombatService`, matching the dungeon pattern) and a reward/loot
   payout path (`ILootService`/`IInventoryService`).
4. Display names/descriptions for `RaidDefinition` (currently only `id`/`className`), and per-raid
   banner art if the legacy source assets can be recovered/generated.
5. Decide whether to reproduce the legacy attempt-token + gem-refill monetization loop
   (`dialog_refill_raid_try`) or replace it with a simpler unlimited/cooldown-based design.
6. Decide whether to reproduce the legacy tab-visibility gating (`RaidsFragment.VISIBLE`) in
   `AppShellController`.

## 21. Rollback steps

1. Delete `Assets/_Game/Scripts/Runtime/UI/Raid/RaidsTabController.cs` (+ `.meta`).
2. In `Assets/_Game/Scripts/Runtime/UI/Shell/AppShellController.cs`, remove the "Phase 8 is isolated
   the same way..." try/catch block added to `Initialize`.
3. In `Assets/_Game/Scenes/Main.unity`, remove the `RaidsTabController` component from `Tab_Raids`
   (re-enable `TabPlaceholderView` if desired — it was left in place and disabled, not deleted).
4. Alternatively, restore all three from the Step 0 backup at
   `D:\Tinh\Backups\Legacy_UI_Phase_8_Raids\` (`Scripts/Runtime/UI/Shell/AppShellController.cs`,
   `Main.unity`; the `Runtime/UI/Raid` folder did not exist at backup time and can simply be deleted).
5. Recompile and re-run the EditMode suite to confirm a return to the 171/171 baseline.
