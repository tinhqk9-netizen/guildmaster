# Phase 3.4 — Storage / Economy UI Completion

Date: 2026-08-07  
Project: `D:\Tinh\Rebuild_GuildMaster`

## Scope

Completed only Phase 3.4. No Phase 4 content or large UI redesign was started.

## Audit before implementation

- `StorageDialog` displayed item capacity but had no upgrade control. The legacy formula and `SaveData.LevelStorage`/`UpgradeStorage` were present, but `IInventoryService` had no upgrade hook.
- `WorkshopDialog` already used the real `ICraftService` queue, duration, cancel and collect APIs, but did not expose queue-capacity or craft-speed upgrades.
- `MarketDialog` already used the real `IMerchantService` listing/stock APIs, but did not expose listing-capacity or market-speed upgrades.
- Existing backend upgrade methods were retained for Workshop and Market. Storage received only the missing service adapter over the existing legacy formula and save fields; no new economy formula or mechanic was introduced.

## Implemented UI

### Storage

- Added current capacity and storage level/next cost display.
- Added `Upgrade` button with affordability and max-level gating.
- Button calls `IInventoryService.UpgradeStorageCapacity()`.
- Successful upgrade refreshes the capacity, currency/HUD callback and item grid through the existing dialog callback.

### Workshop

- Added queue-capacity level/next-cost row and button.
- Added craft-speed level/next-cost row and button.
- Buttons call `ICraftService.UpgradeQueueCapacity()` and `ICraftService.UpgradeCraftSpeed()`.
- Disabled/max state is reflected from live money and current level.

### Market

- Added listing-capacity level/next-cost row and button.
- Added market-speed level/next-cost row and button.
- Buttons call `IMerchantService.UpgradeMarketListings()` and `IMerchantService.UpgradeMarketTime()`.
- Disabled/max state is reflected from live money and current level.

## Files modified

Runtime/service API and implementation:

- `Assets/_Game/Scripts/Runtime/Services/IInventoryService.cs`
- `Assets/_Game/Scripts/Runtime/Services/InventoryService.cs`
- `Assets/_Game/Scripts/Runtime/UI/Headquarters/StorageDialog.cs`
- `Assets/_Game/Scripts/Runtime/UI/Headquarters/WorkshopDialog.cs`
- `Assets/_Game/Scripts/Runtime/UI/Headquarters/MarketDialog.cs`

Idempotent builders and regenerated prefabs:

- `Assets/_Game/Scripts/Editor/UI/Legacy/StorageDialogBuilder.cs`
- `Assets/_Game/Scripts/Editor/UI/Legacy/WorkshopDialogBuilder.cs`
- `Assets/_Game/Scripts/Editor/UI/Legacy/MarketDialogBuilder.cs`
- `Assets/_Game/Prefabs/UI/Headquarters/StorageDialog.prefab`
- `Assets/_Game/Prefabs/UI/Headquarters/WorkshopDialog.prefab`
- `Assets/_Game/Prefabs/UI/Headquarters/MarketDialog.prefab`

Regression tests:

- `Assets/_Game/Scripts/Tests/EditMode/Phase3_EconomyCoreTests.cs`

## Backend API used

- Storage: `GetStorageLevel`, `GetUpgradeStorageCapacityPrice`, `UpgradeStorageCapacity`; price is `IFormulaService.GetStorageCapacityPrice`, maximum level remains legacy level 80.
- Workshop: `GetQueueCapacityLevel`, `GetUpgradeQueueCapacityPrice`, `UpgradeQueueCapacity`, `GetCraftSpeedLevel`, `GetUpgradeCraftSpeedPrice`, `UpgradeCraftSpeed`.
- Market: `GetMarketListingsLevel`, `GetUpgradeMarketListingsPrice`, `UpgradeMarketListings`, `GetMarketTimeLevel`, `GetUpgradeMarketTimePrice`, `UpgradeMarketTime`.

All successful actions deduct currency and persist through the existing service save flow. UI does not mutate `SaveData` directly.

## Verification

- Unity script recompile: **0 errors, 0 warnings**.
- Focused `Phase3_EconomyCoreTests`: **6/6 passed**.
  - Storage upgrade increases level/capacity, deducts currency, and survives JSON save/load round-trip.
  - Queue-capacity, market-listing and market-speed upgrades deduct currency and survive save/load.
  - Existing craft-speed, multi-queue, offline and market refresh tests remain passing.
- Full Unity EditMode suite: **196/196 passed, 0 failed, 0 skipped**.
- Builders executed successfully for all three prefabs and rewired the existing Headquarters references.
- Static prefab check: all new serialized upgrade fields are assigned; no `m_Script: {fileID: 0}` reference was found in the regenerated prefabs.
- Fresh Play Mode smoke flow:
  - Storage flow opened, action flow completed, close returned `IsPopupOpen=False` and `orphanExists=False`.
  - Workshop flow opened/closed and recipe overlay flow completed.
  - Market flow opened/closed.
  - Existing regression checks for Quarters, Tavern and Storage passed.
  - No new red console error was produced. The only observed error was the pre-existing scene cleanup warning involving `Canvas`/`Main Camera`.

The natural fresh save had insufficient currency for the first storage upgrade, so the runtime button was verified in its correct disabled state. The successful upgrade path is covered by the focused service regression test without modifying SaveData in Play Mode.

## Backup / rollback

Backup created before edits:

`D:\Tinh\Rebuild_GuildMaster\Backup\Phase3_Storage_UI\`

To roll back Phase 3.4, restore the backed-up files over the corresponding `Assets/_Game/...` paths, then re-run `Assets/Refresh` and rebuild the three dialog prefabs from their restored builders. No Phase 4 files were changed.

## Known limitation

The existing save naturally used for Play Mode did not contain enough gold to exercise a successful click on each upgrade button. The backend success path, currency deduction, capacity/level change, and save/load persistence are covered by EditMode tests using the real service implementations and formula.
