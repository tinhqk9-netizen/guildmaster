# Phase 2B Completion Report — Dungeon Encounter + Loot Backend

**Project:** `D:\Tinh\Rebuild_GuildMaster`  
**Date:** 2026-08-06  
**Scope:** Dungeon encounter, loot, search-room material rewards, runtime persistence and regression tests.  
**UI/Phase 3:** Not modified.

## Status

The normal-dungeon Phase 2B backend path is complete and regression-tested:

- EncounterGroups are loaded from `dungeons.json` and are the primary runtime path.
- Multi-enemy and empty-room outcomes are weighted and observable.
- Enemy drop tables use the legacy fixed per-mille weight semantics, including miss chance.
- Multiple enemy drops, stack merging and chest/inventory capacity handling are preserved.
- Search-room material rewards are rolled into the dungeon chest.
- Active enemy/corpse/pending-loot state is serialized and hydrated on load.
- Save failure now rolls back both active-dungeon representations atomically.

## Files changed in this finalization

- `Assets/_Game/Scripts/Runtime/Services/DungeonService.cs`
  - Keeps `EncounterGroups` as the primary roll path.
  - Preserves flat `EnemyIds` only as a compatibility fallback for unconverted data.
  - Keeps multi-enemy room state in `Enemies` and `Corpses`.
  - Rolls corpse loot into `PendingDrops`, never directly into inventory.
  - Rolls `SearchRoomDrops` on the empty-room path.
  - Rolls back both `ActiveDungeon` and `ActiveExpeditions` when `Save()` fails.

- `Assets/_Game/Scripts/Definitions/DungeonDefinition.cs`
  - Documents the restored `EncounterGroupData`, `EmptyRoomWeight` and `SearchRoomDrops` fields.
  - No gameplay formula was changed.

- `Assets/StreamingAssets/GameData/dungeons.json`
  - Corrected the `enchanted_forest` `golden_rabbit` encounter weight from `-490` to `10`.
  - Source evidence: `EnchantedForest.java`, `rollEnemies()`, lines 84–86 (`dRandom < 10.0d`).

- `Assets/_Game/Scripts/Tests/EditMode/DungeonEncounterAndLootTests.cs`
  - Added loaded-data validation for EncounterGroups, SearchRoomDrops and enemy references.
  - Added save-failure rollback regression coverage.
  - Existing statistical encounter and loot tests were retained.

- `Docs/Backend_Audit/phase2b_completion_report.md`
  - This report.

## Runtime/data implementation

### Dungeon encounter

`DungeonService.RollEnemies()` checks `DungeonDefinition.EncounterGroups` first. It does not use
the flattened `EnemyIds` list when encounter groups exist.

The weighted table includes:

- one or more enemy IDs per group;
- multi-enemy rooms;
- `EmptyRoomWeight`;
- the legacy fixed 0–1000 roll scale through `DecodeMath.RollFromWeightedMap`.

Current data verification:

| Check | Result |
|---|---:|
| Normal dungeon records | 11 |
| Normal dungeons with EncounterGroups | 11/11 |
| Normal dungeons with SearchRoomDrops | 6 |
| Negative encounter weights | 0 |
| Unresolved EncounterGroups enemy references | 0 |

### Loot

Files involved:

- `Assets/_Game/Scripts/Runtime/Services/LootService.cs`
- `Assets/_Game/Scripts/Runtime/Services/ILootService.cs`
- `Assets/_Game/Scripts/Database/EnemyDropTableLoader.cs`
- `Assets/_Game/Scripts/Database/DatabaseBuilder.cs`
- `Assets/_Game/Scripts/Definitions/EnemyDefinition.cs`

Current data contains 122 enemy records; 116 have non-empty drop tables loaded by the database
builder. The drop loader preserves `Drops` weights and `DropStacks` without normalizing away the
legacy miss gap.

`LootService` preserves:

- weighted single-drop selection;
- deliberate no-drop results when the table total is below 1000;
- multiple drop types across repeated kills;
- stack count preservation and merge;
- chest capacity of 2000, or 3000 with the merchant pack;
- transfer to inventory only through `CollectDrops()` and inventory capacity checks.

### Save/load

`ActiveDungeonSaveData` stores progress, party, pending drops, action state and
`CombatEncounterSaveData`. The encounter save stores both `Enemies` and `Corpses`, including HP,
mana, shield and status effects. `DungeonService.LoadDungeonState()` validates slot, dungeon,
party and character uniqueness before committing each expedition.

Finalization fix: if `ISaveService.Save()` fails, the in-memory `ActiveExpeditions` list is now
restored together with the legacy `ActiveDungeon` field. This prevents a partially rolled-back
active state.

## Verification

### Compile

- Unity script recompilation: **0 errors**.
- Unity reported 8 existing `CS0067` warnings from unused mock-test events; no Phase 2B production
  compile error was reported.

### EditMode

Full current EditMode run:

- **131 passed**
- **0 failed**
- **0 skipped**

Phase 2B targeted run:

- `DungeonEncounterAndLootTests`: **8/8 passed**
- `B1_DungeonOfflineSaveFreezeTests`: **5/5 passed**
- 12-hour catch-up timing: **5 ms** in-memory, **3 Save() calls** in the existing B1 regression
  harness.

Statistical evidence from the Phase 2B target run:

- Encounter test, 1000 rolls: 92 single, 435 multi-enemy, 473 empty-room; weight-400 group 427
  hits versus weight-10 group 8 hits.
- Loot test, 1000 rolls: common 900, rare 57, nothing 43.
- Multi-drop test, 300 kills: all 3 configured item types observed.
- Search-room test, 200 rolls: 55 rewards observed.
- Save-failure rollback test: passed for both active state fields.

### PlayMode

The existing latest runtime report is:

`Reports/S6_5A/S6_5A_DungeonCombatLoot_ActionTest_Result.md`

It records a PASS for bootstrap, start dungeon, enemy spawn, damage, death, chest loot,
`CollectDrops`, progress and save/reload persistence.

A fresh PlayMode invocation was attempted after the final code/data changes, but MCP Unity
returned `ECONNREFUSED 127.0.0.1:8090` twice. Therefore this report does **not** claim a fresh
PlayMode execution from this finalization turn. The existing PlayMode report remains evidence of
the runtime flow, while a fresh rerun is still required once the Unity MCP transport is online.

## Known limitations / not silently claimed as complete

These are legacy features visible in the Java source but not represented by the current normal
DungeonService data model:

1. **Event-key/event-progress encounter branches:** Legacy `Area.event` changes encounter and
   search behavior in several dungeon/raid classes. Current `DungeonRuntime` has no persisted
   event object/key/progress, so those branches are not implemented in Phase 2B.
2. **Search-room trap/heal/status branches:** material rewards are restored; non-material status
   effects from `searchRoom()` are not modeled.
3. **Raid-specific encounter/event flow:** `raids.json` currently does not expose the normal
   `EnemyIds`/`EncounterGroups` data shape, and `DungeonService` consumes `DungeonDefinition`,
   not a dedicated Raid runtime. Raid parity is therefore not claimed here.
4. **Pet extra-drop bonus:** the runtime has a documented extension point, but no caller supplies
   a non-zero bonus in this phase.
5. Two enemy JSON records have incomplete base stats: `emperor_clovis_xxviii` and `enemy`. Their
   drop parsing is not a substitute for missing combat stats.

These limitations require a separate legacy event/raid data-model task if full Java parity is
required. No new mechanic was invented to mask them.

## Backup and rollback

Pre-edit snapshot:

`D:\Tinh\Rebuild_GuildMaster\Backup\Phase2B_Finalization\`

To roll back this finalization, restore the backed-up files from that directory to their original
relative paths. The backup was created before the finalization edits and does not require Git
reset, checkout, clean or deletion of unrelated user work.

