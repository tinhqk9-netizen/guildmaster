# Phase 2C — Combat/Dungeon UI Completion Report

Date: 2026-08-06
Project: `D:\Tinh\Rebuild_GuildMaster`

## Scope

This pass completed the active App Shell dungeon UI only. Phase 2A/2B services, models, save data, encounter logic, loot formulas, and the other UI tabs were not modified.

The existing `DungeonsTabController` already owned the active dungeon route. The implementation was extended in place so the current shell and legacy border/theme system remain the source of layout and styling.

## Backup

Snapshot created before code changes:

`D:\Tinh\Rebuild_GuildMaster\Backup\Phase2C_DungeonUI\`

Backed up files include the active dungeon controller, the legacy `DungeonScreen`, `LegacyUITheme`, `LegacyThemeSprites`, `LegacySpriteRegistry`, and `UICardFactory` (including `.meta` files where present).

## Files changed

- `Assets/_Game/Scripts/Runtime/UI/Dungeon/DungeonsTabController.cs`
- `Docs/Backend_Audit/phase2c_completion_report.md`

No backend/service/model/save-data file was changed.

`DungeonScreen.cs` was audited as the previous screen implementation, but was not changed because it is not the active App Shell controller.

## Implemented UI

### Dungeon party panel

`ShowActive()` now builds `EXPEDITION PARTY` rows from `ExpeditionRuntime.Party`.

Each row uses the existing legacy border sprites and displays:

- character portrait through `LegacySpriteRegistry.GetUnitSprite`
- character identity and level
- class/id label from the real `CharacterRuntime.Definition`
- current/max HP and an HP bar
- current mana value
- positive/negative status effects with remaining turns
- `DEAD` state with the existing failure color

Max HP is read through the existing `ICharacterService.GetTotalStat(..., StatType.MaxHp)` API, with a null-safe definition fallback for display only.

### Enemy formation panel

`ENEMY FORMATION` is built from the runtime dungeon state:

- live `DungeonRuntime.Enemies`
- defeated `DungeonRuntime.Corpses` without duplicate instance IDs

Each enemy row displays the real enemy id/class, sprite lookup, HP/max HP, HP bar, status effects, and `DEAD` state. Multiple enemies are rendered as separate rows, so EncounterGroups/multi-enemy rooms are represented without flattening the encounter to one text line.

### Room event display

`ROOM EVENT` displays `DungeonRuntime.LastRoomEvent`. If the backend has no event text for the current state, the UI shows a neutral “no room event reported” state. No new event mechanic was invented.

### Combat log

`COMBAT LOG` displays the most recent entries from `DungeonRuntime.CombatLog`, capped to the last eight visible rows. It is refreshed while the active screen is open every 0.5 seconds and does not use `Debug.Log` as the player-facing log.

The current backend records encounter, empty-room, defeated-enemy, experience, loot, bonus-loot, and search-room entries. It does not currently emit separate runtime entries for every attack, damage roll, critical, dodge, or skill event. Those details are therefore not fabricated in the UI and remain a known backend data limitation for a later scoped task.

### Loot result UI

`ShowLoot()` now uses a bordered reward panel with one reusable row per pending item. Each row shows:

- item sprite via the real item `IdImage`/definition id
- item name
- quantity
- available item stat summary when present

The existing `DungeonService.CollectDrops(int)` action remains the only collection path. The existing Collect button and popup/route lifecycle were preserved.

### Live refresh

The active screen refreshes at most twice per second. This updates HP, enemy formation, room event, combat log, and pending loot without rebuilding UI every frame.

## Backend bindings preserved

- `IDungeonService.GetExpedition`
- `IDungeonService.StartExpedition`
- `IDungeonService.StopExpedition`
- `IDungeonService.CollectDrops`
- `ICharacterService.GetTotalStat`
- `ExpeditionRuntime.Party`
- `DungeonRuntime.Enemies`
- `DungeonRuntime.Corpses`
- `DungeonRuntime.LastRoomEvent`
- `DungeonRuntime.CombatLog`
- `DungeonRuntime.PendingDrops`

No formula, combat, encounter, loot, save, or inventory behavior was changed.

## Verification

### Compile

PASS. Unity recompiled all scripts successfully.

Existing warnings: 8 `CS0067` warnings from unused mock-save events in existing test files. No new compile error or warning was produced by the Phase 2C UI change.

### Static checks

PASS:

- `git diff --check` passed.
- Only the active dungeon controller was modified for runtime UI.
- No default Unity window or temporary debug UI was added.
- Existing `LegacyUITheme`, `LegacyThemeSprites`, `LegacySpriteRegistry`, and border assets are used.
- Existing service/model/backend files are unchanged.
- UI event log is sourced from `DungeonRuntime.CombatLog`, not `Debug.Log`.

### EditMode

PASS: Unity Test Runner returned `190/190 passed, 0 failed, 0 skipped`.

This includes the Phase 2B encounter/loot/runtime regression coverage, including multi-enemy/empty-room/weighted-drop/search-room/save-load tests.

### PlayMode

NOT RUN in this environment. The Unity MCP test request failed before test execution with:

`Connection failed: connect ECONNREFUSED 127.0.0.1:8090`

No PlayMode assertion result was received, so the following cannot be marked as freshly verified here:

- visible multi-enemy formation in a live Main scene
- live combat-log updates during a running expedition
- clearing a dungeon and collecting the resulting loot through the new rows

The code is compile-verified and the backend flow is covered by the existing EditMode and previously recorded Phase 2B/PlayMode reports. A fresh manual PlayMode pass is still required when MCP Unity is reachable.

## Manual PlayMode checklist

At 1080×1920 Portrait:

1. Open Main scene and select `Dungeons`.
2. Configure a party and start an unlocked dungeon.
3. Open the active run view.
4. Confirm `EXPEDITION PARTY` shows portraits, HP, mana, status, and dead state.
5. Confirm `ENEMY FORMATION` shows every enemy row when a multi-enemy room appears.
6. Confirm `ROOM EVENT` changes for encounter/empty/search-room states.
7. Confirm `COMBAT LOG` updates without relying on Console output.
8. Wait for pending drops, open `VIEW LOOT`, verify icon/name/quantity rows.
9. Press `COLLECT LOOT`; confirm the route returns to the dungeon hub and pending drops are gone.
10. Switch to Headquarters/Adventurers and back; confirm the App Shell and other tabs are unchanged.
11. Check the Unity Console for new red errors. Existing warning-only mock-test logs are not Phase 2C runtime failures.

## Known limitations

- `DungeonRuntime.CombatLog` is runtime-only and capped by the existing backend at 20 entries; the UI shows the latest eight.
- The backend currently does not expose individual attack/damage/critical/dodge/skill event records, so the UI cannot truthfully render those event categories separately without a backend scope change.
- No screenshot was captured because the Unity MCP transport was unavailable at the PlayMode verification step.

## Rollback

To roll back this Phase 2C UI change, restore the backed-up file:

`D:\Tinh\Rebuild_GuildMaster\Backup\Phase2C_DungeonUI\Assets\_Game\Scripts\Runtime\UI\Dungeon\DungeonsTabController.cs`

Then let Unity reimport/recompile. No backend or save-data rollback is required for this phase.

Phase 3 was not started.

## Post-completion sprite mapping fix

After the initial Phase 2C pass, the Party Panel showed hero portraits but the legacy
`DungeonScreen` combat/expedition cards still showed the `UICardFactory` icon placeholder. The
root cause was that this branch created a text-only card and never assigned its child `Icon`
`Image.sprite`.

Fix backup:

`D:\Tinh\Rebuild_GuildMaster\Backup\Phase2C_UI_Sprite_Fix\`

Modified files:

- `Assets/_Game/Scripts/Runtime/UI/Dungeon/DungeonsTabController.cs`
- `Assets/_Game/Scripts/Runtime/UI/Dungeon/DungeonScreen.cs`

The portrait mapping is now consistent: `CharacterDefinition.ImageId` when populated, otherwise
`CharacterDefinition.id`, both through `LegacySpriteRegistry.GetUnitSprite`. The combat card finds
the existing `Icon` child created by `UICardFactory` and assigns the sprite; no layout or enemy
rendering was changed.

Verification after the fix:

- Unity compile: PASS; same 8 pre-existing `CS0067` warnings.
- Dungeon/combat EditMode regression: `2/2 passed`.
- Full EditMode regression: `190/190 passed`.
- Fresh PlayMode test could not start because MCP Unity returned
  `ECONNREFUSED 127.0.0.1:8090` before execution.
