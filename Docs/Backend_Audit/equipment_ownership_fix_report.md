# Equipment Ownership + Tavern Starter Weapon Fix

Date: 2026-08-07  
Project: `D:\Tinh\Rebuild_GuildMaster`

## Scope

Only the equipment ownership flow and Tavern starter-weapon lifecycle were changed. Phase 5, combat formulas, traits, pets and raids were not changed by this task.

## Backup

Backup created before implementation:

`D:\Tinh\Rebuild_GuildMaster\Backup\Phase_Equipment_Ownership\`

The backup contains the five affected runtime service files and the pre-change Tavern tests.

## Root causes

### 1. Tavern starter weapon leak

`TavernService.GenerateVisitor()` created an `ItemRuntime` and called `InventoryService.AddItem()` while creating the visitor. Therefore an un-recruited or expired visitor already owned a weapon in the player inventory.

The visitor now stores only a generated `WeaponInstanceId`. The runtime item is materialized by `CharacterService.ResolveEquippedWeapon()` when the visitor is recruited (or when an existing saved character is loaded), using the class definition's real `StarterWeaponId`.

### 2. Equipment appeared in both inventory and character

The old `EquipmentService` locked and referenced the item from the character but `InventoryService.GetAllItems()` still returned it. Unequip also only unlocked the item; the visible inventory could not represent the ownership transition consistently.

The persisted item record is retained so save/load can reconstruct the equipment, but `InventoryService` now treats an item referenced by any character equipment slot as character-owned and excludes it from inventory-facing queries and capacity calculations. Equip locks/references it; unequip clears the character reference and makes it visible again. Direct removal of an equipped instance is rejected to prevent dangling character references.

## Ownership flow after fix

```text
Visitor generation
  -> CharacterSaveData.WeaponInstanceId only
  -> no ItemRuntime, no InventoryService.AddItem

Recruit
  -> CharacterService resolves StarterWeaponId
  -> AddEquippedItem() stores one locked ownership record
  -> CharacterSaveData.WeaponInstanceId references that same instance
  -> inventory-facing queries hide it

Equip inventory item
  -> CharacterSaveData slot references existing instance
  -> item locked
  -> item disappears from visible inventory

Unequip
  -> CharacterSaveData slot cleared
  -> item unlocked
  -> same instance returns to visible inventory

Save/load
  -> one ItemSaveData record + one CharacterSaveData equipment reference
  -> CharacterService restores the same ItemRuntime
  -> no duplicate inventory item
```

## Files modified

- `Assets/_Game/Scripts/Runtime/Services/TavernService.cs`
  - Removed visitor-time starter `ItemRuntime` creation and inventory insertion.
  - Kept cleanup compatibility for old leaked records.
- `Assets/_Game/Scripts/Runtime/Services/CharacterService.cs`
  - Added canonical equipped-weapon resolution/materialization at recruit/load boundary.
- `Assets/_Game/Scripts/Runtime/Services/InventoryService.cs`
  - Added equipped ownership filtering, capacity handling and `AddEquippedItem()`.
  - Prevented direct removal of equipped instances.
- `Assets/_Game/Scripts/Runtime/Services/IInventoryService.cs`
  - Added the equipped-item registration API.
- `Assets/_Game/Scripts/Runtime/Services/EquipmentService.cs`
  - Preserved one-instance equip/unequip references and synchronized lock state.
- `Assets/_Game/Scripts/Tests/EditMode/B2_TavernStarterWeaponTests.cs`
  - Updated legacy starter-weapon expectations and made weapon assertions deterministic through the tutorial archer branch.
- `Assets/_Game/Scripts/Tests/EditMode/EquipmentOwnershipTests.cs`
  - Added six ownership regression tests.

## Regression tests

`EquipmentOwnershipTests`:

1. `VisitorSpawn_DoesNotCreateInventoryItem`
2. `RecruitVisitor_CreatesEquippedStarterWithoutInventoryDuplicate`
3. `Equip_RemovesItemFromVisibleInventory`
4. `Unequip_RestoresItemToVisibleInventory`
5. `SaveLoad_PreservesEquipmentWithoutInventoryDuplicate`
6. `VisitorExpiration_DoesNotLeakStarterWeapons`

Targeted result: **6/6 passed**.  
Full EditMode result: **226/226 passed, 0 failed, 0 skipped**.  
Unity console error query after the run: **0 errors**.

Compile result: **PASS**. Unity reported no compile errors; only pre-existing unused-event warnings in mock test services.

## Rollback

To roll back this task, restore the backed-up files from:

`D:\Tinh\Rebuild_GuildMaster\Backup\Phase_Equipment_Ownership\`

Restore the corresponding files under `Assets/_Game/Scripts/` and remove the added `EquipmentOwnershipTests.cs` if the pre-task state is required. Do not restore unrelated project files.

## Verification limitation

No runtime UI/layout was changed. The ownership flow was verified through Unity EditMode service tests and a clean Unity console error query. A manual Play Mode check is still recommended for the visible inventory/equipment screen, but it is not required to validate the service ownership invariants covered here.
