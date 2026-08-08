# Phase 4 - Content System Restoration Report

Date: 2026-08-07
Project: `D:\Tinh\Rebuild_GuildMaster`

## Scope and backup

Phase 4 was implemented from the existing Legacy Java sources and decoded game data. No Phase 5 work was started.

Pre-change snapshot:

`D:\Tinh\Rebuild_GuildMaster\Backup\Phase4_Content_Restoration\`

The backup contains the scoped service, definition, data, UI, and test files as they existed before Phase 4 changes.

## 4.1 Tavern / recruitment

Implemented and verified:

- `TavernService` keeps the Legacy visitor flow: visitor generation, independent common/rare trait rolls, starter weapon creation, guest capacity, and free recruitment.
- Recruitment validates the guest and rolls back the guest if character creation fails.
- Successful recruitment saves state.
- `TavernDialog` displays both real trait slots and the starter weapon resolved through `IInventoryService`.
- Existing Tavern regression tests and starter-weapon tests pass.

Primary files:

- `Assets/_Game/Scripts/Runtime/Services/TavernService.cs`
- `Assets/_Game/Scripts/Runtime/UI/Headquarters/TavernDialog.cs`
- `Assets/_Game/Scripts/Tests/EditMode/B2_TavernStarterWeaponTests.cs`

## 4.2 Quest system

Implemented and verified:

- All 56 quest definitions currently present in `quests.json` and represented in the Legacy `QuestsManager.accessibleQuests` list are loaded.
- The eight Legacy doctrine pools and the shared general/Kings pool are restored at the database boundary.
- Quest instances persist their selected reward pool and doctrine context, because one definition can belong to both a general pool and a doctrine pool.
- Claiming a general/Kings quest awards Gems; claiming a doctrine quest awards the selected doctrine progress. The old rarity-based reward inference and fabricated doctrine-choice UI were removed.
- Quest UI reads real target progress and persisted reward context.
- Craft, market sale, dungeon, and Ancient Grave raid hooks now call the existing quest service using source-traced IDs.

Primary files:

- `Assets/_Game/Scripts/Database/DatabaseBuilder.cs`
- `Assets/_Game/Scripts/Runtime/Services/QuestService.cs`
- `Assets/_Game/Scripts/Runtime/Services/CraftService.cs`
- `Assets/_Game/Scripts/Runtime/Services/MerchantService.cs`
- `Assets/_Game/Scripts/Runtime/Services/RaidService.cs`
- `Assets/_Game/Scripts/Runtime/Save/SaveData.cs`
- `Assets/_Game/Scripts/Runtime/UI/Quest/QuestScreen.cs`
- `Assets/_Game/Scripts/Runtime/UI/Auxiliary/AuxiliaryController.cs`

Data note: an older audit report says 36 quests, but the current Legacy Java `accessibleQuests` list and the project data contain 56 definitions. The implementation follows the source/data actually present, not the stale count.

## 4.3 Pet system

Implemented and verified:

- 21 pet definitions are enriched with the seven Legacy families, tier, ability count, and guaranteed first ability.
- Egg hatching uses the Legacy family mapping and 75/20/5 tier roll.
- Pet state includes food, favourite, four ability slots, and dungeon assignment.
- Pets are assigned to a dungeon area, not a hero. Assignment is carried into dungeon runtime/save state.
- `EXPERIENCE` and `DROPS` bonuses are applied to dungeon XP/loot paths.
- Food feed power is loaded from item data; Shelter exposes hatch, favourite, feed, assign, and unassign actions using real inventory/dungeon state.

Primary files:

- `Assets/_Game/Scripts/Runtime/Services/PetService.cs`
- `Assets/_Game/Scripts/Runtime/UI/Headquarters/ShelterDialog.cs`
- `Assets/_Game/Scripts/Runtime/UI/Headquarters/PetDetailPanel.cs`
- `Assets/_Game/Scripts/Runtime/Models/DungeonRuntime.cs`
- `Assets/_Game/Scripts/Runtime/Save/ActiveDungeonSaveData.cs`
- `Assets/_Game/Scripts/Definitions/ItemDefinition.cs`
- `Assets/_Game/Scripts/Database/ItemFieldsLoader.cs`

## 4.4 Raid system

Implemented and verified:

- A separate `RaidService` and `RaidRuntime` were added; `DungeonService` is not used as the raid controller.
- The 12 Legacy raids are enriched with source-derived room sequences, multi-enemy rooms, empty rooms, boss flags, and the five source-confirmed unique reward IDs.
- Raid unlock checks use existing dungeon progress.
- Raid UI supports selection, unlock requirement, start, current-room fight, abandon, and reward collection.
- Raid loot uses the existing `LootService` and enemy drop tables.

Primary files:

- `Assets/_Game/Scripts/Database/RaidContentCatalog.cs`
- `Assets/_Game/Scripts/Definitions/RaidDefinition.cs`
- `Assets/_Game/Scripts/Runtime/Models/RaidRuntime.cs`
- `Assets/_Game/Scripts/Runtime/Services/RaidService.cs`
- `Assets/_Game/Scripts/Runtime/UI/Raid/RaidsTabController.cs`

Known limitation: the Legacy raid-specific narrative/event callbacks and custom boss side effects are not present in the decoded C# data boundary. No fake event or reward was invented; room and boss data that could be traced was restored.

## 4.5 Bestiary

Implemented and verified:

- `SeenEnemyIds` is retained in `SaveData` and normalized for older saves.
- `BestiaryService` validates enemy IDs and records discovery.
- Dungeon enemy spawning marks enemies as seen.
- Bestiary UI groups entries by Dungeon and Raid, hides undiscovered entries, and blocks undiscovered detail access.

Primary files:

- `Assets/_Game/Scripts/Runtime/Services/BestiaryService.cs`
- `Assets/_Game/Scripts/Runtime/Services/ServiceContainer.cs`
- `Assets/_Game/Scripts/Runtime/Services/DungeonService.cs`
- `Assets/_Game/Scripts/Runtime/UI/Auxiliary/AuxiliaryController.cs`
- `Assets/_Game/Scripts/Runtime/Save/SaveData.cs`

## Verification

- Unity script compile: **0 errors**.
- Compile warnings: 8 existing unused mock-save event warnings in test fixtures; no Phase 4 production warning/error was introduced.
- Full EditMode: **207/207 passed, 0 failed, 0 skipped**. This includes four Phase 4 content regression tests.
- Phase 4 tests cover: catalog counts/mappings, pet hatch/assignment/feed/unassignment, Bestiary discovery validation, and raid start from current party.
- A Play Mode boot smoke was started successfully after waiting for the Unity transport to recover. Unity entered Play Mode and boot logged the App Shell and 11 wired screens.
- Unity's automated PlayMode Test Runner itself aborted during its scene-save/exit task (`InvalidOperationException: This cannot be used during play mode`); this is a Test Runner/editor-state failure, not a Phase 4 compile failure. Manual runtime action verification remains required for Tavern/Quest/Pet/Raid/Bestiary clicks.

## Remaining limitations before Phase 5

- Character doctrine assignment is not represented in the current `CharacterSaveData`; therefore doctrine quest quantity generation remains zero until that pre-existing model gap is restored from the Legacy doctrine-selection flow.
- Raid active progress is intentionally session-only, matching the Legacy audit; unique rewards and normal drops are held until Collect Rewards.
- Legacy raid narrative events and custom boss side effects require source-specific event data that is not available in the current decoded JSON boundary.
- No UX polish, animation, balance pass, or Phase 5 content was started.

## Rollback

Restore the backed-up scoped files from:

`D:\Tinh\Rebuild_GuildMaster\Backup\Phase4_Content_Restoration\`

Then refresh the Unity Asset Database and recompile. Do not delete Phase 0-3 files outside the backup scope.
