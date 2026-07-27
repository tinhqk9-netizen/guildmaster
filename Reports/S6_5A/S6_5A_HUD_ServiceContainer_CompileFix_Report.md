# S6.5A HUD ServiceContainer Compile Fix

## Error
- `Assets/_Game/Scripts/Runtime/UI/HUD/HUDController.cs(27,32): error CS0246: The type or namespace name 'ServiceContainer' could not be found (are you missing a using directive or an assembly reference?)`
- Several subsequent compilation errors related to `GameDatabase.GetCategory()`, `EquipmentDefinition`, `ConsumableDefinition`, and unassigned `go` variable in `S6_5A_UIPlaceholderGenerator.cs`.

## Root Cause
- **Namespace/Dependency:** `HUDController` is in `GuildMaster.Runtime.UI.HUD` and `ServiceContainer` is in `GuildMaster.Runtime.Services`. A missing `using` directive caused the initial `CS0246`.
- **Architectural Misalignment:** `HUDController` is a purely presentational/navigation class that only required `ISaveService` for currency display. Injecting the entire `ServiceContainer` into it was an overreach.
- **Missing Definitions/Methods:** `EquipmentDefinition` and `ConsumableDefinition` do not exist (only `ItemDefinition` is used with a `Consumable` flag). `GameDatabase` does not have `GetCategory()` but instead uses `GetAll<T>()`.

## Fix
| File | Change | Why |
|---|---|---|
| `HUDController.cs` | Reverted `Initialize` to accept `ISaveService` directly instead of `ServiceContainer`. | Prevents unnecessary coupling to the entire Service Container for a simple HUD, and implicitly fixes the `CS0246` (MISSING_USING) without adding an unnecessary using statement. |
| `UIRuntimeBootstrap.cs` | Updated `hud.Initialize(Services, _ui)` to `hud.Initialize(Services.Save, _ui)`. | Matches the reverted HUDController signature. |
| `CharacterScreen.cs` | Changed `EquipmentDefinition` to `ItemDefinition` and directly passed `EquipmentSlot.Weapon`/`Armor`/`Accessory` for test equips. | `EquipmentDefinition` does not exist in the codebase. |
| `InventoryScreen.cs` | Changed `ConsumableDefinition` to `ItemDefinition itemDef && itemDef.Consumable`. | `ConsumableDefinition` does not exist, `ItemDefinition` has a `Consumable` boolean field. |
| `CraftScreen.cs` | Changed `_database.GetCategory("recipes")` to `_database.GetAll<RecipeDefinition>()`. | `GameDatabase` uses generic `GetAll<T>()` rather than string-based category lookup. |
| `DungeonScreen.cs` | Changed `_database.GetCategory("dungeons")` to `_database.GetAll<DungeonDefinition>()`. | Same reason as above. |
| `S6_5A_UIPlaceholderGenerator.cs` | Added `else { go = t.gameObject; }` inside `CreateButton`. | Fixes `error CS0165: Use of unassigned local variable 'go'`. |

## Call-Site Verification
- **HUDController callers:** Checked using grep. The only caller is `UIRuntimeBootstrap.cs` on line 76, which was updated successfully.
- **ServiceContainer instance count:** Only one is instantiated in `UIRuntimeBootstrap.cs`.
- **asmdef references:** Verified that both are within `GuildMaster.Runtime.asmdef` (they are in the same assembly).

## Unity Verification
- **Compile timestamp:** `2026-07-27 15:34:00`
- **CS error count:** `0`
- **New errors:** Addressed 4 subsequent script errors that surfaced during the fix process.
- **Console/Editor.log evidence:** Analyzed `Editor.log` to confirm the compile error was successfully resolved and verified `0` remaining errors after the final pass.

## Final Decision
S6_5A_COMPILE_FIXED_READY_TO_RESUME_UI_BRIDGE
