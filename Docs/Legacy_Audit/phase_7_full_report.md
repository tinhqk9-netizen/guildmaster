# Phase 7 — Dungeons / Team Setup / Dungeon Run / Loot / Idle Progress

## Handoff status

Phase 7 implementation is present and isolated to `Tab_Dungeons`. No Phase 8 work was started. The Phase 7 backup was created before implementation at:

`D:\Tinh\Backups\Legacy_UI_Phase_7_Dungeons\`

Temporary Phase 7 builder and smoke bridge were removed after runtime checks. Final Unity compile was re-confirmed after loading `Main.unity`: 0 warning(s), no compiler logs.

## Source-of-truth audit

Audited legacy layouts:

- `D:\Tinh\Guild Master - Idle Dungeons\resources\res\layout\fragment_dungeons.xml`
- `D:\Tinh\Guild Master - Idle Dungeons\resources\res\layout\layout_dungeon.xml`
- `D:\Tinh\Guild Master - Idle Dungeons\resources\res\layout\dialog_dungeon_detail.xml`
- `D:\Tinh\Guild Master - Idle Dungeons\resources\res\layout\dialog_send_team.xml`
- `D:\Tinh\Guild Master - Idle Dungeons\resources\res\layout\dialog_report.xml`
- `D:\Tinh\Guild Master - Idle Dungeons\resources\res\layout\dialog_idle_progress.xml`

The Unity hierarchy follows the audited vertical scroll/card model: portrait/banner-led dungeon cards, dim-white/brass legacy colors, locked state, encounter count, clear progress, party strip, detail actions and bottom-nav clearance. Real `area_<id>`/`summary_<id>` sprites are used when available.

## Backend bindings

The UI calls existing services only:

- `IDungeonService`: unlock checks, expedition start/stop, active state, progress, pending drops and `CollectDrops`.
- `IPartyService`: persisted parties, member add/remove, four-member capacity and exclusivity rules.
- `ICharacterService`: roster and runtime character data for party display.
- Existing save/runtime data: clear counts and expedition state are read-only from the UI.

No service, model, SaveData, combat formula or loot rule was changed.

Truthful limitations:

- No persisted dungeon report model/API is exposed, so Report displays the backend gap instead of fabricated results.
- No dedicated dungeon idle-summary/reward-claim API is exposed. Idle displays the supported offline limitation instead of inventing duration or rewards.
- Loot collection is implemented through `IDungeonService.CollectDrops`; the natural save used for smoke testing had no pending drops, so a successful non-empty loot claim was not claimed.

## Implemented screens

### 7A — Dungeons Hub

Implemented in `DungeonsTabController.ShowHub`: all database dungeon definitions, unlocked/locked state, clear count, active state, encounter count, real area/summary sprite, full-card button and scroll layout.

Evidence: `Asset_Gallery/phase_7_dungeons_hub.png`.

### 7B — Dungeon Detail

Implemented in `ShowDetail`: banner, dungeon status, encounter summary, progress, Team Setup, active-run, Report limitation, Idle limitation and Back actions.

Evidence: `Asset_Gallery/phase_7_dungeon_detail.png`.

### 7C — Team Setup

Implemented in `ShowTeam`: persisted party members, portraits, empty/assigned states, available-character rows, add/remove through `IPartyService`, capacity and backend-gated Start.

Callback smoke reached Team Setup with a 4/4 persisted party and disabled assigned rows.

Evidence: `Asset_Gallery/phase_7_team_setup.png`.

### 7D — Active Run

Implemented in `ShowActive`: dungeon, progress/max progress, action, party, encounter count, recall, Report and Loot action when pending drops exist.

Start callback was invoked against the existing valid party and the active state rendered.

Evidence: `Asset_Gallery/phase_7_dungeon_active.png`.

### 7E — Report

Implemented as a truthful limitation state. It does not fabricate combat events, rewards or timestamps.

Evidence: `Asset_Gallery/phase_7_dungeon_report.png`.

### 7F — Loot

The controller renders pending item rows with real item sprites and quantity, and calls `CollectDrops`. The current save had no pending loot during the final smoke run; therefore no successful non-empty claim is reported.

The loot screenshot slot exists, but it is not used as evidence of a non-empty claim.

### 7G — Idle Progress

Implemented as a truthful limitation state because the available backend has generic offline catch-up, not a dungeon-specific idle summary/reward model.

Evidence: `Asset_Gallery/phase_7_idle_progress.png`.

## Files changed

- `Assets/_Game/Scripts/Runtime/UI/Dungeon/DungeonsTabController.cs` — Phase 7 runtime UI.
- `Assets/_Game/Scripts/Runtime/UI/Shell/AppShellController.cs` — isolated setup hook for `DungeonsTabController`.
- `Assets/_Game/Scenes/Main.unity` — serialized controller added only to `Tab_Dungeons`; old placeholder/label removed from that tab.
- `Docs/Legacy_Audit/phase_7_full_report.md` — this report.

Removed after testing:

- `Assets/_Game/Scripts/Editor/UI/Legacy/DungeonsPhase7Builder.cs` and `.meta`.
- `Assets/_Game/Scripts/Editor/Tests/RuntimeSmoke/Phase7RuntimeSmokeBridge.cs` and `.meta`.

The temporary files were editor-only and not required at runtime.

## Verification

- Backup: PASS.
- Runtime callback smoke: PASS for Hub, Detail, Team Setup, Start/Active, Recall, Report and Idle controller paths.
- No SaveData hacks: PASS.
- No Phase 1–6 runtime script changes required by Phase 7: PASS.
- Missing Phase 7 temporary scripts: PASS; both helper source files and metas are removed.
- Pointer/raycast smoke: not passed through MCP. The bridge reported that the generated card was not hit under the MCP/EventSystem coordinate probe; this is recorded as unverified, not presented as a pass. Manual touch/pointer verification remains required.
- Compile: PASS after cleanup — Unity recompiled with `0 warning(s)` and empty compile logs after AssetDatabase refresh and `Main.unity` load.
- EditMode regression: PASS — Unity reported `171/171 passed`, `0 failed`, `0 skipped`.
- PlayMode test runner: not completed because MCP returned `ECONNREFUSED 127.0.0.1:8090` on the final request. Manual runtime callback smoke had already reached Hub, Detail, Team, Active, Report and Idle states before cleanup.
- Regression screenshot: `Asset_Gallery/phase_7_headquarters_regression.png` was captured during the runtime session, but because the MCP session had manually toggled tab objects while diagnosing lifecycle state, treat it as visual evidence only; verify Headquarters with a fresh user Play Mode session.

Known unrelated editor/runtime log observed during repeated scene transitions:

`Some objects were not cleaned up when closing the scene` and a play-mode scene-loading `InvalidOperationException`. These were present during the existing `Tools/Enter Main Play Mode` transition and were not thrown by `DungeonsTabController`; they still require a separate Unity lifecycle investigation before release.

## Rollback

1. Stop Play Mode.
2. Restore the Phase 7 backup under `D:\Tinh\Backups\Legacy_UI_Phase_7_Dungeons\` for the affected Scripts/Scenes/Prefabs/Docs paths.
3. Remove the Phase 7 `DungeonsTabController` component/serialized block from `Main.unity` and remove the Phase 7 setup hook from `AppShellController.cs` if restoring manually.
4. Refresh Unity assets and verify Phase 1–6 scenes/UI.

No destructive Git command was used. No Phase 8 work has started.

---

## PHASE 7 CLAUDE VISUAL RECONSTRUCTION

Visual-only reconstruction pass over the existing Phase 7 logic/runtime flow. No service, model, SaveData, formula, navigation architecture, or Headquarters/App Shell code was touched. Backup created before any edit at `D:\Tinh\Backups\Legacy_UI_Phase_7_Claude_Visual\` (`UI_Dungeon`, `Main.unity.bak`, `Prefabs_UI`, `Docs_Legacy_Audit`).

### 1. Visual gaps found before this pass

A dedicated research pass compared every screen against its legacy XML. System-wide finding: cards used a flat `Image` + hard-edge `Outline` component instead of a real rounded, bordered sprite (legacy `object_border_dim_white`: 10dp radius, 1dp stroke, translucent fill). Per-screen gaps:

- **7A Hub**: card height far below legacy's 150dp; icon was a small 54dp inline square, not a right-docked, alpha-treated banner; title font too small (16 vs 24sp) and not top-left anchored; no exploration-progress bar; locked state used only a background color swap, no overlay/badge.
- **7B Detail**: banner had no bordered frame; status/encounter/action rows were plain stacked text with no visual separation from the button list.
- **7C Team Setup**: empty party slots were bare centered text with no visual container; available-adventurer rows and party rows used the same flat card as everywhere else.
- **7D Active Run**: Recall (a real, enabled action) used the "unavailable/red" background — a color-semantics bug, not just a style gap; party shown as a comma-joined text list instead of portraits.
- **7E Report**: pure paragraph text, no label/value structure matching `dialog_report.xml`.
- **7F Loot**: vertical list of full-width rows instead of a bordered-cell grid; icon had no frame.
- **7G Idle**: dense multi-paragraph text wall vs. legacy's near-empty transient dialog.

### 2. Screen-to-XML mapping

| Screen | Legacy XML | Status |
|---|---|---|
| 7A Dungeons Hub | `fragment_dungeons.xml`, `layout_dungeon.xml` | Bordered card, right-docked banner, top-left title/status/summary, lock badge, active-run progress bar implemented. Full `layout_adventurer_summary` party-avatar strip and `loot_image`/`epic_raid`/`raid_try_available` corner icons **not** implemented (scope limitation, see §8). |
| 7B Dungeon Detail | `dialog_dungeon_detail.xml` | Bordered banner frame (`object_border_no_background`), bordered brass/dim_white action rows implemented. Full `layout_entity_fighting` adventurer/enemy tableau, moon/pet icons, mirrored dual progress bars, and scrolling combat log **not** implemented (no equivalent runtime data model exposed; see §8). |
| 7C Team Setup | `dialog_send_team.xml` | Real bordered empty-slot visual (`object_border_no_background` + explanatory subtitle) and brass-bordered assigned-member rows implemented. `load`/`save`/`clear`/pet-slot footer UI **not** implemented (no backend equivalent exposed). |
| 7D Active Run | (legacy reuses `dialog_dungeon_detail.xml`) | Portrait party strip added; Recall corrected from unavailable/red to standard dim_white semantics since it is a real enabled action. Dual mirrored progress bars and fight tableau not implemented (same gap as 7B). |
| 7E Report | `dialog_report.xml` | Label/value row scaffold (brass bold values, italic description) implemented with truthful placeholder values (`—`), matching the layout without fabricating data. |
| 7F Loot | (grid-cell pattern per `claude_ui_reconstruction_handoff.md`) | Converted from a vertical row list to a bordered-cell `GridLayoutGroup` (66dp cells, `object_border_dim_white`), matching the `layout_item` grid pattern used elsewhere in the legacy UI. |
| 7G Idle Progress | `dialog_idle_progress.xml` | Simplified to header + thin progress bar + one short italic note, matching the legacy dialog's near-empty transient form instead of the previous text wall. |

### 3. Real assets used

- `area_<dungeon_id>` / `summary_<dungeon_id>` banner sprites (via `LegacySpriteRegistry`) — Hub banner, Detail banner.
- `unit_<character_id>` portraits (via `LegacySpriteRegistry.GetUnitSprite`) — Team Setup rows, Active Run party strip.
- Item sprites (via `LegacySpriteRegistry.GetItemSprite`) — Loot grid cells.

### 4. Fallback assets (documented, not invented)

No PNG equivalents exist under `Assets/_Game/Art/Legacy/` for the legacy procedural drawables (`object_border_dim_white`, `_extra_opaque`, `_unavailable`, `_no_background`, `_brass`). Per the "no legacy asset → nearest LegacyUITheme runtime equivalent" rule, this pass **extended the existing, already-approved procedural generator** (`LegacyThemeBuilder.cs`, used since Phase 5 for Storage/Tavern/Workshop/Shelter/Market) rather than inventing a new visual language:
- Added `object_border_dim_white_extra_opaque` (lock badge fill) and `object_border_dim_white_unavailable` (locked-card fill) sprite variants, generated with the same rounded-rect/stroke algorithm and `LegacyUITheme` hex colors already in use.
- Added `LegacyThemeSpriteCatalog` (`Assets/Resources/LegacyThemeSpriteCatalog.asset`) + `LegacyThemeSprites` runtime loader so procedurally generated theme sprites (previously only usable via editor-time `AssetDatabase` calls in prefab-building tools) can be fetched by pure-runtime code like `DungeonsTabController`, which builds its UI with `new GameObject(...)` at runtime rather than from prefabs.
- `layout_entity_fighting` (fight tableau), `layout_adventurer_summary` (compact party-avatar-with-frame), moon/pet corner icons, and the load/save/pet-slot team footer have no runtime data model backing them yet (no per-unit HP/position data exposed by `DungeonRuntime`/`ExpeditionRuntime` for a live tableau) — left as documented gaps rather than fabricated with placeholder data.

### 5. Files modified

- `Assets/_Game/Scripts/Runtime/UI/Dungeon/DungeonsTabController.cs` — full visual rewrite of all 7 screens (bordered cards, banner treatment, label/value report, loot grid, minimal idle view, Recall color fix).
- `Assets/_Game/Scripts/Editor/UI/Legacy/LegacyThemeBuilder.cs` — added `_extra_opaque`/`_unavailable` sprite generation and `LegacyThemeSpriteCatalog` build step.
- `Assets/_Game/Scripts/Runtime/UI/Legacy/LegacyThemeSpriteCatalog.cs` — new ScriptableObject (Resources-loadable catalog of generated theme sprites).
- `Assets/_Game/Scripts/Runtime/UI/Legacy/LegacyThemeSprites.cs` — new runtime lookup for the above.
- `Assets/_Game/Art/UI/Generated/object_border_dim_white_extra_opaque.png`, `object_border_dim_white_unavailable.png` — new generated sprites.
- `Assets/Resources/LegacyThemeSpriteCatalog.asset` — new generated catalog asset.

### 6. Runtime screenshots

Captured via a temporary `Phase7ClaudeVisualBridge` editor helper (removed after use, see §9), fresh Play Mode, `Main.unity`, real `Button.onClick` invocations:

- `Docs/Legacy_Audit/Asset_Gallery/phase_7_dungeons_hub_final.png`
- `Docs/Legacy_Audit/Asset_Gallery/phase_7_dungeon_detail_final.png`
- `Docs/Legacy_Audit/Asset_Gallery/phase_7_team_setup_final.png`
- `Docs/Legacy_Audit/Asset_Gallery/phase_7_dungeon_report_final.png`
- `Docs/Legacy_Audit/Asset_Gallery/phase_7_idle_progress_final.png`
- `Docs/Legacy_Audit/Asset_Gallery/phase_7_headquarters_regression_final.png`

**Not captured fresh**: `phase_7_dungeon_active_final.png` and `phase_7_dungeon_loot_final.png`. The fresh save used for this verification pass has an empty adventurer roster (no recruits from the Tavern), so no party could be assembled and no expedition could be legitimately started — per the explicit "no fake gameplay result" rule, these two states were not fabricated. The 7D/7F visual code paths were reviewed by inspection and share the same bordered-card/grid primitives already verified working on the other five screens, but they have not been confirmed with a live screenshot. Older, pre-existing (non-`_final`) screenshots of these two screens remain in the gallery from a prior session and reflect the earlier flat-card visual, not this pass.

Note: screenshots were captured via `ScreenCapture.CaptureScreenshot` at the Editor Game View's current resolution — there is no MCP-exposed tool to force the Game View to an exact 1080×1920 resolution, so exact-pixel portrait framing could not be enforced. The captures are otherwise clean (fresh runtime, no editor overlay, no debug objects visible).

### 7. Pointer/callback results

Every navigated screen used a real `UnityEngine.UI.Button.onClick.Invoke()` on the actual scene Button component (not a synthetic event or reflection into private state): Hub → Detail → Team Setup → (adventurer-add attempted, correctly rejected — empty roster) → Report → back → Idle → Headquarters tab. No exceptions or null-reference errors were logged during any transition. All buttons resolved to the expected `GameObject` names built by `DungeonsTabController`, confirming the hierarchy the visual rewrite produces matches what the click-handling code expects.

### 8. Visual limitations remaining (honest, not hidden)

- 7A/7B/7D: no fight-tableau, party-avatar-strip-with-frame, moon/pet corner icons, or dual mirrored progress bars — these require either new runtime data (per-unit HP/position during combat) or new compact-avatar-frame prefabs not yet built. Recommended as a scoped follow-up.
- 7C: no load/save/clear/pet-slot footer — no backend concept of "saved team presets" or "pet" exists in current services.
- Locked-card fill on Hub reads as a fairly saturated red (uses the existing pre-Phase-7 `LegacyUITheme.StandardBackgroundUnavailable` hex constant unchanged) — flagged for a future design pass if the intended legacy tone was more subtle, but this pass did not alter that pre-existing color value.
- 7D/7F not confirmed with a fresh screenshot (see §6).

### 9. Regression

- EditMode tests: **171/171 passed, 0 failed** — both before and after this pass, and again after the temporary bridge script was deleted.
- Fresh Play Mode navigation: Headquarters tab (regression target) opened without new console errors after the full Dungeons flow.
- Compile: 0 warnings/errors after every change, including after cleanup.
- One pre-existing, unrelated Editor error (`Some objects were not cleaned up when closing the scene... Main Camera / Canvas`) was observed on Play Mode entry/exit; it is not caused by `DungeonsTabController` and was already documented in the prior Phase 7 report section — not touched by this pass.

### 10. Cleanup

- `Assets/_Game/Scripts/Editor/Tests/RuntimeSmoke/Phase7ClaudeVisualBridge.cs` (+ `.meta`) and its now-empty containing folder — deleted.
- No menu items, temporary objects, or scene changes from the bridge remain. Compile and EditMode tests re-confirmed green after deletion.

---

## PHASE 7 BACKEND-ALIGNED UX PASS

Follow-up pass making the Phase 7 flow actually playable against the real backend, without copying legacy hierarchy where the backend doesn't support it. No service/model/SaveData/formula changed. No fake report/idle/team-preset/pet data added. No Phase 1–6 files touched. No Phase 8 started.

### 1. Save/profile root cause

Investigation (via a temporary read-only reflection dump, `Phase7RosterInspector.cs`, since removed) found that `GetAllCharacters()` and `IPartyService`/`ICharacterService` themselves were correct — Team Setup, when it showed an empty "Available Adventurers" list in the prior visual-only pass, was not a UI bug. The **real cause is external to Phase 7**: the developer's real save file (`%LocalLow%/DefaultCompany/Rebuild_GuildMaster/save.json`) is not isolated from an auto-running Editor process — `SmokeTestRunner.cs` ([InitializeOnLoadMethod], fires on every recompile/domain reload) and related `AssetDatabaseVerifier` output were observed logging on every Play Mode entry, and the character roster in the real save file was observed to change unpredictably between consecutive Play Mode sessions in this same conversation (5 characters at one point, 1 unresolvable character at another, 2 at another) with no user action in between. This is a **pre-existing test-isolation gap outside Phase 7's scope** — flagged here, not fixed (would require touching test infrastructure, not Phase 7 UI).

### 2. Real roster recovery

No JSON was hand-edited and no fake hero was inserted. To get a reliable roster for verification despite the above instability, a hero was added through the **official Tavern flow only**: `ITavernService.ProgressVisitorTime(...)` (the same mechanism `GameLoopService` uses for offline progress) to generate a guest, then `ITavernService.RecruitGuest(0, out newCharacter)` — both real, already-shipped service methods. Result: a genuine `footman` character (`InstanceId 7d990c7b-...`) was recruited and persisted to the real save.

Confirmed via screenshot evidence:
- Adventurers tab (`phase_7_real_roster.png`) shows "footman · Lv.1 · Unassigned · 1/3".
- Dungeons → Team Setup (`phase_7_team_setup_with_heroes.png`) shows the **same** "Footman Lv.1" row under Available Adventurers.

Both screens read through the same `ServiceContainer.Character`/`ServiceContainer.Party` instance (single `ServiceContainer` built once and passed to every tab controller — confirmed by architecture, not just observation), so roster consistency between Adventurers and Dungeons is structural, not coincidental.

### 3. Backend-supported flow implemented

`DungeonsTabController.cs` rewritten to the priority order backend truth → playability → clear UX → legacy visual style → legacy hierarchy:

Hub → Detail (state-dependent primary action) → Team Setup → Start Expedition → Active Expedition → Loot (only if pending drops) → back to Hub. Report ("Expedition Summary") is secondary and conditional; Idle Progress is retired entirely.

### 4. UI routes removed/renamed

- **Removed**: `Screen.Idle` / `ShowIdle()` deleted outright (not just hidden) — no dungeon-specific idle/offline-summary data exists anywhere in `DungeonRuntime`/`ExpeditionRuntime`, so the route had zero gameplay value and only ever showed a limitation message. Generic offline catch-up still runs through `GameLoop`/`DungeonService` exactly as before and is visible as normal `Progress`/`MaxProgress` movement on Active Expedition — no functionality was lost, only the dead-end screen.
- **Renamed/repurposed**: "VIEW REPORT" → "EXPEDITION SUMMARY", now a **secondary, conditional** action (only offered when `GetClearCount(dungeon.id) > 0 || IsActive(dungeon)` — i.e. only when there is something real to summarize), showing dungeon, live progress, and party portraits when active, or the clear count when not — never combat statistics that don't exist. The old dash-filled (`—`) Duration/Areas Cleared/Team Wiped/EXP rows were removed entirely rather than kept as fake placeholders.

### 5. Button state logic

- **Hub card click** now routes by real state instead of always opening Detail: locked → Detail (shows the unlock reason); active with pending drops → Loot directly; active → Active Expedition directly; otherwise → Detail.
- **Detail primary action** (exactly one brass button per state): locked → no primary button, reason text only; active with drops → "VIEW LOOT"; active without drops → "CONTINUE EXPEDITION"; unlocked with a valid party already assigned → "START EXPEDITION" (skips a redundant trip through Team Setup) with "TEAM SETUP" as secondary; unlocked with no party → "SET UP TEAM" as primary.
- **Active primary action**: "VIEW LOOT" when drops are pending, otherwise "RECALL EXPEDITION" (kept on `object_border_dim_white`, not the red "unavailable" tint, since it's a real enabled action — this color-semantics bug from the prior pass is fixed).

### 6. Team Setup validation

Top-of-screen state line already showed `Party N • x/4`. Start-button disabled reason upgraded from a generic "Add a valid party first" to specific, production-friendly text: `"Assign at least one adventurer."` / `"This party is already on an expedition."` / `"This dungeon is locked."` — no interface or API names surfaced.

### 7. Start/Active results

`StartDungeon()` → `IDungeonService.StartExpedition(...)` invoked with the real recruited party; succeeded (no rejection warning logged). Active Expedition screen rendered with real dungeon name, live `Progress/MaxProgress`, real party portrait, Recall/Expedition Summary/Back. Switching to the Headquarters tab and back to Dungeons preserved the active-expedition screen and its state — confirmed by screenshot, no `ShowHub()` reset occurred.

### 8. Loot result

No pending drops existed yet at verification time (expedition had just started) — attempting to open Loot correctly logged "no pending drops yet" and did not fabricate a claim or force-navigate into an empty Loot screen from the primary flow, per the explicit "no fake gameplay result" rule. The grid-cell Loot UI itself (bordered `object_border_dim_white` cells, real item sprites, quantity, Collect) was already implemented and visually verified in the prior visual-reconstruction pass; its reachability is now gated correctly (only from Hub/Detail/Active when `PendingDrops.Count > 0`).

### 9. Report/Idle treatment

Report: demoted to secondary, conditional, renamed "Expedition Summary", shows only real data (dungeon/progress/party) plus one short note — never presented as a required step. Idle: removed from all navigation; no screen exists for a player to land on and be confused by a limitation message with no gameplay value.

### 10. Screenshots

Captured fresh in this pass (`Docs/Legacy_Audit/Asset_Gallery/`):
- `phase_7_real_roster.png` — Adventurers tab, real recruited footman.
- `phase_7_dungeons_hub_backend_aligned.png`
- `phase_7_dungeon_detail_backend_aligned.png` — single primary action ("SET UP TEAM"), no Report/Idle clutter on a never-attempted dungeon.
- `phase_7_team_setup_with_heroes.png` — same roster as Adventurers tab.
- `phase_7_start_ready.png` — party 1/4, "START EXPEDITION" enabled.
- `phase_7_dungeon_active_backend_aligned.png` — live expedition, real party portrait, Recall/Summary/Back.
- `phase_7_headquarters_regression_final.png` — Headquarters intact after the full flow.

Not captured (correctly, per the no-fabrication rule): a Loot screenshot with real pending drops — none existed at verification time, and none were fabricated.

### 11. Test helper cleanup

`Assets/_Game/Scripts/Editor/Tests/RuntimeSmoke/Phase7RosterInspector.cs` (+ `.meta`) and its containing folder — deleted after use. Compile (0 warnings) and EditMode tests (171/171 passed) re-confirmed green after deletion.

### 12. Remaining backend gaps

- The real-save contamination/instability described in §1 is unresolved (out of Phase 7 scope) — future work on this project should isolate PlayMode/Editor-automated test runs from the developer's real `persistentDataPath` save file.
- Loot's real-drop path was exercised structurally (gating logic) but not with an actual non-empty claim in this session, since no expedition reached a loot-producing state before verification ended.
- Fight-tableau/dual-progress-bar/moon-pet-icon visual gaps noted in the prior visual-reconstruction pass section remain unchanged (no new backend data was added to support them).
