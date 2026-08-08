# Phase 4 — Pet UX Completion + Dungeon Resource Verification

Date: 2026-08-07

## Scope

Completed the remaining Pet Detail actions and added an Editor-only Dungeon Verification tool. Phase 5 was not started. No pet gameplay formula or SaveData schema was replaced.

## Backup

`D:/Tinh/Rebuild_GuildMaster/Backup/Phase4_Pet_UX_Dungeon_Verification/`

Backed up before editing:

- `PetService.cs`
- `PetDetailPanel.cs`
- `Phase4_ContentRestorationTests.cs`

## Pet UX changes

### Feed

Reused the existing `IPetService.FeedWithItem` pipeline:

`PetDetailPanel` → `PetService.FeedWithItem` → `IInventoryService.RemoveItem` → `PetService.Feed` → existing save flow.

The UI now:

- shows current food and the exact next-level requirement through `GetFoodToNextLevel`;
- shows the selected real Food item, quantity and its real `FeedPower`;
- calls the existing feed API and refreshes Shelter/detail state;
- shows a disabled `FEED: NO FOOD AVAILABLE` state when no valid Food item exists.

`GetFoodToNextLevel` only exposes the formula already used privately by `PetService`; no new progression formula was created.

### Release

Added `IPetService.ReleasePet` / `PetService.ReleasePet`.

- Removes only the matching `PetSaveData.InstanceId`.
- Saves through `ISaveService`.
- Confirmation overlay shows `RELEASE THIS PET?` with Release/Cancel.
- Cancel makes no mutation.
- Successful release refreshes Shelter and closes Pet Detail.
- Release is rejected while the pet is referenced by an active expedition, preventing a dangling companion reference.

### Bonus presentation

Pet Detail now uses the existing real APIs:

- `+X% Dungeon EXP`
- `+X% Additional Loot Chance`
- `No expedition bonus.` when both values are zero.

Combat bonus is not displayed because the current runtime does not expose a pet combat-bonus API. No combat value was fabricated.

## Dungeon Verification tool

Created:

- `D:/Tinh/Rebuild_GuildMaster/Assets/_Game/Scripts/Editor/Tools/DungeonVerificationWindow.cs`

Menu:

- `Tools > GuildMaster > Dungeon Verification > Give Unlocked Dungeon Materials`
- `Tools > GuildMaster > Dungeon Verification > Clear DEV_TEST Dungeon Materials`

The tool:

1. Loads the current save and resolves every currently unlocked dungeon using the same clear-gate fields as `DungeonService`.
2. Reads `EncounterGroups`, the compatibility `EnemyIds` fallback, each `EnemyDefinition.DropTable`, and `SearchRoomDrops`.
3. Resolves definitions through `GameDatabase`.
4. Adds only safe `ItemCategory.Material` resources, excluding consumables, not-sellable/unique-like records, currency, quest and artifact parent classes.
5. Writes isolated `DEV_TEST_DUNGEON_MATERIAL_...` records at x20 each.
6. Cleanup removes only that prefix; it does not touch normal items, pets, characters or currency.

## Verification results

### Automated tests

Updated:

- `D:/Tinh/Rebuild_GuildMaster/Assets/_Game/Scripts/Tests/EditMode/Phase4_ContentRestorationTests.cs`

Added coverage for:

- Feed consumes one real Food item and changes pet food/level state.
- Release removes only the selected pet.
- Release persists correctly and the pet does not return after SaveService reload.

Results:

- Targeted Phase 4 suite: **10/10 passed**.
- Full EditMode suite: **219/219 passed**, 0 failed, 0 skipped.
- Unity script recompile: **0 errors, 0 warnings**.

### Editor tool run

The real menu command reported:

- Unlocked dungeons: `enchanted_forest` (1).
- Material/resource definitions added: 13.
- Every injected record had `StackCount = 20` and a `DEV_TEST_DUNGEON_MATERIAL_` instance ID.

Cleanup was then executed and verified directly from the saved JSON:

- Remaining `DEV_TEST_DUNGEON_MATERIAL_` records: **0**.
- Currency remained unchanged at the pre-test value.
- No pet records were removed.

The first menu invocation timed out at the MCP boundary because Unity displayed its modal result dialog, but Unity’s console confirmed the command completed and the subsequent cleanup command completed successfully.

## Files changed

- `D:/Tinh/Rebuild_GuildMaster/Assets/_Game/Scripts/Runtime/Services/PetService.cs`
- `D:/Tinh/Rebuild_GuildMaster/Assets/_Game/Scripts/Runtime/UI/Headquarters/PetDetailPanel.cs`
- `D:/Tinh/Rebuild_GuildMaster/Assets/_Game/Scripts/Editor/Tools/DungeonVerificationWindow.cs`
- `D:/Tinh/Rebuild_GuildMaster/Assets/_Game/Scripts/Tests/EditMode/Phase4_ContentRestorationTests.cs`
- `D:/Tinh/Rebuild_GuildMaster/Docs/Backend_Audit/phase4_pet_ux_completion_report.md`

## Known limitations

- Pet Detail currently exposes the first valid Food inventory stack as the feed action; there is no separate food-picker API in the existing UX contract.
- Manual touch-click verification of the confirmation dialog was not automated through MCP. Service-level feed/release/save-load tests passed, and the existing Shelter button wiring remains unchanged.
- Active expedition companions must finish/stop their expedition before release; this is an intentional integrity guard.

No Phase 5 work was started.
