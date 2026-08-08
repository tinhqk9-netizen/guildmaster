# Phase 4 — Pet Verification Tool Report

Date: 2026-08-07

## Scope

Created an Editor-only verification tool for the existing Phase 4 pet pipeline.
No changes were made to `PetService`, `PetDefinition`, `SaveData`, or gameplay logic for this task.

## Backup

Backup created before the tool edit:

`D:\Tinh\Rebuild_GuildMaster\Backup\Phase4_Pet_Verification_Tool\`

The backup contains the related service, save, definition, model, and existing editor-tool files that were audited.

## Files

Created:

- `Assets/_Game/Scripts/Editor/Tools/PetVerificationWindow.cs`
- Unity-generated `.meta` file for the editor script.
- This report.

No runtime/backend file was modified by this task.

## Menu

The tool is available at:

`Tools > GuildMaster > Pet Verification`

Commands:

- Give Random Pet Egg
- Hatch Test Egg
- Spawn Random Owned Pet
- Run 10 Hatch Trials
- Clear DEV_TEST Pets
- Print Pet Debug Report

## Implementation details

- Database definitions are loaded through `DatabaseBuilder` and `EditorExternalGameDataProvider`.
- A test egg is selected from loaded item definitions whose family has matching loaded `PetDefinition` data.
- The egg is injected through `InventoryService.AddItem(new ItemRuntime(...))`; no `PetSaveData` is constructed by the tool.
- Hatching always calls `IPetService.HatchEgg(eggDefinitionId)`.
- Fallback pet creation calls `IPetService.CreatePet(definitionId)`.
- Newly created pets are tagged with `DEV_TEST_PET_` after the service creates them so cleanup can identify them safely.
- Before hatching, the tool refuses to proceed when a non-`DEV_TEST_` egg of the same definition exists. This is required because the current `HatchEgg` API consumes by definition ID rather than inventory instance ID.
- Save verification reloads a fresh `SaveService` and checks that each created test pet is present.
- Cleanup removes only pet records whose `InstanceId` starts with `DEV_TEST_`.
- The debug report includes definition, tier, abilities, assigned dungeon, food, and favourite state.

## Verification

### Compile

- Unity script recompile: PASS — 0 warnings, 0 errors.

### EditMode

- Full EditMode run: PASS — 207/207 passed, 0 failed, 0 skipped.

### Unity Editor menu

- Main window menu executed successfully.
- `Run 10 Hatch Trials` executed twice: 20/20 completed.
- Batch 1: Tier 1 = 10, Tier 2 = 0, Tier 3 = 0.
- Batch 2: Tier 1 = 9, Tier 2 = 0, Tier 3 = 1.
- Generated abilities were present in the debug report.
- Save verification: PASS for both batches.
- Reload verification: PASS for both batches.
- `Print Pet Debug Report`: PASS.
- `Clear DEV_TEST Pets`: PASS — 20 test pets removed; real pets were not touched.

The sample confirms the real hatch flow produced multiple valid tiers, including a Tier 3 result. The 75/20/5 roll semantics remain owned by `PetService`; the verification tool does not duplicate or replace that logic.

## Known limitation

There is no separate egg-creation API in the current service contract. “Give Random Pet Egg” therefore adds a database-backed egg through `InventoryService`, then uses the existing `PetService.HatchEgg` pipeline. The safety guard prevents accidental consumption of a real egg with the same definition.

## Phase boundary

Phase 5 was not started.
