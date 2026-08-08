# Phase 4 — Pet UX Improvement + Expedition Pet Integration

Date: 2026-08-07

## Scope

Implemented only the Phase 4 pet UX/expedition integration. Phase 5 was not started.
Backend formulas and PetSaveData schema were not redesigned.

## Audit conclusion before implementation

### Legacy assignment architecture

The Legacy Java source stores one selected pet on the area/expedition:

- `D:/Tinh/Guild Master - Idle Dungeons/sources/it/paranoidsquirrels/idleguildmaster/storage/data/places/Area.java`
  - `savedPetId` stores the selected pet for the area.
  - `petExploringId` is the active pet while the area is running.
- `D:/Tinh/Guild Master - Idle Dungeons/sources/it/paranoidsquirrels/idleguildmaster/ui/dialogs/DialogSendTeam.java`
  - the team-send dialog has one `selectedPetId` and writes it to the area before starting exploration.

Therefore the correct model is one pet companion per expedition/area. It is not equipment attached to a hero and is not a permanent dungeon assignment. The requested two-slot UI would not match the available Legacy evidence, so the implementation uses one `PET COMPANION` slot.

### Legacy bonus locations

`Area.java` applies pet behavior in several runtime paths:

- area setup/darkness;
- combat turns (`petAttack`, `petHeal`, `petExecution`, `petCast` and related effects);
- experience collection (`pet.getExperience()`);
- loot (`pet.getDrops()` additional drop behavior).

The current C# runtime exposes only the following expedition hooks:

- `IPetService.GetExperienceBonus(petInstanceId)`;
- `IPetService.GetDropBonus(petInstanceId)`.

Those existing hooks were kept and wired to the selected expedition pet. Combat pet effects are not fabricated: the current runtime does not expose a pet-aware combat API, so the detail screen explicitly reports that combat bonus data is unavailable.

## Save/migration decision

No SaveData migration is required.

`ActiveDungeonSaveData.PetInstanceId` and `DungeonRuntime.PetInstanceId` already existed, and `DungeonService.SaveDungeonState`/`LoadDungeonState` already serialize that field. Existing `PetSaveData.AssignedDungeonId` and its Shelter compatibility flow remain intact for old content, but new expedition start no longer auto-selects a pet from that field.

Load validation now clears a missing or duplicate pet reference from the in-memory expedition rather than allowing an invalid companion reference to survive. The rest of the saved expedition remains loadable.

## Implementation

### Backend/API

Modified:

- `D:/Tinh/Rebuild_GuildMaster/Assets/_Game/Scripts/Runtime/Services/IDungeonService.cs`
  - added an overload accepting `petInstanceId`;
  - added `IsPetOnExpedition` for picker validation.
- `D:/Tinh/Rebuild_GuildMaster/Assets/_Game/Scripts/Runtime/Services/DungeonService.cs`
  - accepts an optional selected pet at `StartExpedition`;
  - validates that the pet exists and is not already used by another expedition;
  - persists the selected pet through the existing `PetInstanceId` field;
  - validates pet references during load;
  - keeps the selected pet attached through tick/offline progress;
  - applies the existing EXP and DROP hooks using the expedition pet ID.

No changes were needed in:

- `D:/Tinh/Rebuild_GuildMaster/Assets/_Game/Scripts/Runtime/Models/DungeonRuntime.cs`;
- `D:/Tinh/Rebuild_GuildMaster/Assets/_Game/Scripts/Runtime/Models/ExpeditionRuntime.cs`;
- `D:/Tinh/Rebuild_GuildMaster/Assets/_Game/Scripts/Runtime/Save/ActiveDungeonSaveData.cs`;
- `D:/Tinh/Rebuild_GuildMaster/Assets/_Game/Scripts/Runtime/Services/PetService.cs`.

### UI

Modified:

- `D:/Tinh/Rebuild_GuildMaster/Assets/_Game/Scripts/Runtime/UI/Dungeon/DungeonsTabController.cs`
  - added a one-slot `PET COMPANION` section to Team Setup;
  - opens an owned-pet picker;
  - shows real pet portrait, tier, level and current EXP/DROP bonus;
  - prevents selecting a pet already assigned to another active expedition;
  - supports selecting `NO PET` and removing a selected pet before start;
  - passes the selected instance ID to `StartExpedition`;
  - shows the persisted companion in the active expedition panel.
- `D:/Tinh/Rebuild_GuildMaster/Assets/_Game/Scripts/Runtime/UI/Headquarters/PetDetailPanel.cs`
  - shows the actual EXP/DROP expedition bonuses from `PetService`;
  - shows the actual stored ability IDs;
  - states `COMBAT: Not exposed by current runtime` instead of inventing a value.

No App Shell, Headquarters layout, combat layout, or Phase 5 content was changed for this task.

## Tests

Added to:

- `D:/Tinh/Rebuild_GuildMaster/Assets/_Game/Scripts/Tests/EditMode/Phase4_ContentRestorationTests.cs`

Coverage added:

1. Start an expedition with a selected pet, save, reload, and verify the same `PetInstanceId` remains.
2. Start an expedition without a pet and verify no pet bonus reference is present.
3. Remove a pet by stopping/restarting the expedition without a companion and verify the hook is disabled.
4. Existing pet hatch/portrait/save tests remain in the same suite.

Results:

- Targeted `Phase4_ContentRestorationTests`: **9/9 passed**.
- Full EditMode suite: **218/218 passed**, 0 failed, 0 skipped.
- Unity script recompile: **0 compile errors**. The only compiler output was the existing set of 8 unused mock-save-event warnings.
- Main scene Play Mode was entered after recompilation. No new runtime error was observed after loading `Assets/_Game/Scenes/Main.unity`; the console still contained an older cleanup error from the previously used `LegacyShapeTest` scene.

## Backup

Pre-edit backup:

`D:/Tinh/Rebuild_GuildMaster/Backup/Phase4_Pet_Expedition_Integration/`

The backup contains the pre-change versions of the edited service, UI, model/save audit files and the Phase 4 test file.

## Known limitations

- Legacy evidence and the current model support one companion per expedition, not two pet slots.
- Current C# pet APIs expose EXP and DROP expedition bonuses. Pet combat/darkness/loot-special behavior from Java is not fully exposed by the current runtime; no unsupported values were added.
- Shelter's legacy `AssignedDungeonId` compatibility state remains available, but new Dungeon Team Setup selection is per expedition and is the source used by runtime EXP/DROP hooks.
- Manual touch-flow validation of every Team Setup button was not automated through MCP in this pass. The static wiring, targeted tests, full EditMode suite, Main scene load, and Play Mode entry passed.

## Rollback

To roll back only this task, restore the files in the backup folder to their original project paths, then let Unity reimport/recompile. Do not delete Phase 2–4 files outside the listed backup scope.
