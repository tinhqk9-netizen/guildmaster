# S6.5A Player-Facing UI Audit Report

**Date:** 2026-07-27

## 1. Scope of Audit
Audited files: `HUDController.cs`, `UIRuntimeBootstrap.cs`, `UIScreenId.cs`, and all screen scripts in `Runtime/UI/`.

## 2. Screen Status Table

| Screen | Exists In Scene | HUD Button | Service Wired | Shows Runtime Data | Has Player Actions | Save/Reload Verifiable | Status |
|---|---|---|---|---|---|---|---|
| Tavern | Yes (Script exists) | No | No | No | No | No | MISSING_SERVICE_WIRING |
| Character | Yes (Script exists) | Yes | Yes | Yes | No | No | PARTIAL_TEXT_ONLY |
| Inventory | Yes (Script exists) | Yes | Yes | Yes | No | No | PARTIAL_TEXT_ONLY |
| Craft | Yes (Script exists) | Yes | No | No | No | No | MISSING_SERVICE_WIRING |
| Merchant | Yes (Script exists) | Yes | No | No | No | No | MISSING_SERVICE_WIRING |
| Dungeon | Yes (Script exists) | Yes | No | No | No | No | MISSING_SERVICE_WIRING |
| Quest | Yes (Script exists) | No | No | No | No | No | MISSING_SERVICE_WIRING |
| Settings | Yes (Script exists) | Yes | No | No | No | No | MISSING_SERVICE_WIRING |

## 3. Findings
1. **UIScreenId**: `Quest` is entirely missing from the `UIScreenId` enum.
2. **HUDController**: Lacks buttons for `Tavern` and `Quest`.
3. **UIRuntimeBootstrap**: Only initializes `HUD`, `Inventory`, and `Character`. It fails to call `.Initialize(...)` for `Tavern`, `Craft`, `Merchant`, `Dungeon`, `Quest`, and `Settings`, causing them to have null services and fail to show data or execute actions.
4. **UI Scripts**:
   - The UI scripts are extremely barebones. Most just dump text strings into a `Text` component.
   - They lack actionable buttons (e.g. no "Equip", "Use", "Start Dungeon", "Tick").
   - There is no dynamic UI prefab generation for lists, so we have to use text fields or create quick placeholder buttons if we don't have prefabs.

## 4. Conclusion
The user playtest failed because the UI is completely unwired for 6/8 screens, and the remaining 2 have no actions. The required next step is to wire all services, add all HUD buttons, and implement functional placeholders with basic actionable buttons.
