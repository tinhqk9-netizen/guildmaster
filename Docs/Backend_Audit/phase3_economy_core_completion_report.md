# Phase 3.1–3.3 — Economy & Idle Loop Backend Completion

Date: 2026-08-06  
Project: `D:\Tinh\Rebuild_GuildMaster`  
Backup: `D:\Tinh\Rebuild_GuildMaster\Backup\Phase3_Economy_Core\`

## Scope

Completed only the backend economy/idle-loop work for Phase 3.1–3.3. Storage UI, Phase 4 content, scene hierarchy, and large UI redesign were not started.

The repository does not contain a separate `MarketService`; the existing `MerchantService` owns both merchant stock and player market listings. The implementation keeps that architecture.

## Audit before changes

The legacy Java source confirms:

- `Item.getSecondsToCraft()` uses the item base price, stack, workshop time levels, and the Merchant Pack multiplier.
- `Item.getSecondsToSell()` uses the corresponding market formula.
- `Utils.progressWorkshopTime()` and `Utils.progressMarketTime()` carry remaining delta into subsequent queue entries and complete an entry only after `secondsPassed > secondsToCraft/secondsToSell`.
- `DialogWorkshop` cancels a queued craft and refunds the recipe ingredients.
- `DialogMarket` cancels a listing and returns the listed item to inventory.
- `Utils.tick24Hours()` rotates regular merchant stock; `Utils.tickWeek()` rotates special reserve stock.
- Market upgrade prices and capacity/time formulas are defined in `Formulas.java`.

Primary source evidence:

- `D:\Tinh\Guild Master - Idle Dungeons\sources\it\paranoidsquirrels\idleguildmaster\Formulas.java`
- `D:\Tinh\Guild Master - Idle Dungeons\sources\it\paranoidsquirrels\idleguildmaster\Utils.java`
- `D:\Tinh\Guild Master - Idle Dungeons\sources\it\paranoidsquirrels\idleguildmaster\storage\data\items\Item.java`
- `D:\Tinh\Guild Master - Idle Dungeons\sources\it\paranoidsquirrels\idleguildmaster\ui\dialogs\DialogCraft.java`
- `D:\Tinh\Guild Master - Idle Dungeons\sources\it\paranoidsquirrels\idleguildmaster\ui\dialogs\DialogWorkshop.java`
- `D:\Tinh\Guild Master - Idle Dungeons\sources\it\paranoidsquirrels\idleguildmaster\ui\dialogs\DialogMarket.java`

Existing audit reports used:

- `Docs/Backend_Audit/workshop_craft_audit.md`
- `Docs/Backend_Audit/market_audit.md`
- `Reports/S6_5A/S6_5A_Stage6_Craft_Report.md`
- `Reports/S6_5A/S6_5A_Stage7_Merchant_Report.md`
- `Reports/S6_5A/S6_5A_Stage11_Offline_Report.md`

## Implemented changes

### Phase 3.1 — Offline simulation

- `OfflineProgressService` now uses `SaveData.LastAccess` as the authoritative runtime timestamp, with `Metadata.SaveTimeUnix` as a legacy-save fallback.
- Timestamp fields are advanced after simulation dispatch, so a failing simulation does not consume the offline window before effects are applied.
- Offline progress now advances:
  - Workshop queue via `CraftService.ProgressWorkshop`.
  - Market listings via `MerchantService.ProgressMarket`.
  - Active dungeon/combat expeditions via `IDungeonService.FastForward`.
- `GameLoopService` uses the offline service for catch-up, then advances tavern/quest state and performs the final save.
- Runtime ticks invoke scheduled merchant refresh checks without changing UI architecture.

### Phase 3.2 — Workshop

- Removed the hardcoded craft duration path from `CraftService`.
- Added `IFormulaService.GetSecondsToCraft(...)` to the shared formula contract.
- Craft duration now uses the legacy item-price/stack/workshop-level formula and purchase flags.
- `ProgressWorkshop` consumes a large delta across all queue entries, preserving remainder time and completing multiple items in one call.
- Added `CancelCraft(instanceId)` with legacy recipe ingredient refund.
- Added workshop queue capacity and craft-speed upgrade APIs with legacy price formulas and level caps.
- `WorkshopDialog` now reads duration from the service and exposes the existing queue state through cancel callbacks; no visual redesign was made.

### Phase 3.3 — Market/Merchant

- Removed the hardcoded 20-second sell duration.
- Sell duration now uses the legacy item-price/stack/market-level formula.
- `ProgressMarket` drains multiple listings from one delta and preserves remainder time.
- Payout now uses the legacy item base price (`ItemDefinition.Price`) rather than the previous fallback `SellPrice/100` path.
- Added listing capacity validation, locked/not-sellable validation, and `CancelListing(instanceId)` refund flow.
- Implemented the existing `BuyItem` compatibility method by resolving real regular/special stock and routing through `BuyOffer`.
- Added daily regular-stock and weekly special-stock refresh hooks driven by `Last24Triggered` and `LastWeekTriggered`.
- Replaced per-call `new Random()` with one service-level RNG for weighted merchant offers.
- Added Market Listings and Market Time upgrade APIs, with legacy formulas and level caps.
- `MarketDialog` reads live duration/payout values and exposes cancellation through the existing row callback; no layout redesign was made.

## Files modified

- `Assets/_Game/Scripts/Runtime/Formulas/IFormulaService.cs`
- `Assets/_Game/Scripts/Runtime/Services/ICraftService.cs`
- `Assets/_Game/Scripts/Runtime/Services/CraftService.cs`
- `Assets/_Game/Scripts/Runtime/Services/IMerchantService.cs`
- `Assets/_Game/Scripts/Runtime/Services/MerchantService.cs`
- `Assets/_Game/Scripts/Runtime/Models/MerchantResult.cs`
- `Assets/_Game/Scripts/Runtime/Services/OfflineProgressService.cs`
- `Assets/_Game/Scripts/Runtime/Services/GameLoopService.cs`
- `Assets/_Game/Scripts/Runtime/Services/ServiceContainer.cs`
- `Assets/_Game/Scripts/Runtime/UI/Headquarters/WorkshopDialog.cs`
- `Assets/_Game/Scripts/Runtime/UI/Headquarters/MarketDialog.cs`
- `Assets/_Game/Scripts/Tests/EditMode/S6_5A_Stage7_MerchantTests.cs`
- `Assets/_Game/Scripts/Tests/EditMode/Phase3_EconomyCoreTests.cs`

`SaveData` schema was sufficient for this phase; no SaveData field or migration was added.

## Verification

### Unity compile

- Unity script recompilation: **successful, 0 errors**.
- The final compile reports 8 existing `CS0067` warnings from older mock-save test events. No warning originates from the Phase 3 implementation files.

### New Phase 3 regression tests

`Phase3_EconomyCoreTests`: **4/4 passed**

- `Workshop_FormulaProcessesMultipleQueueItemsAndUsesSpeedUpgrade`
- `Workshop_CancelRefundsIngredientsAndSpeedUpgradeUsesFormulaPrice`
- `Offline_12HoursDrainsCraftAndMarketQueuesWithoutTimestampLoss`
- `Market_RefreshesRegularStockAfterOfflineDayAndSupportsUpgrade`

The offline test applies a 24-hour gap and verifies the 12-hour cap, drains multiple craft/market entries, preserves timestamps, and observes one dungeon persistence call from the in-memory save double.

### Full EditMode suite

- **194/194 passed**
- 0 failed
- 0 skipped

Unity Console error query after the targeted run: **no errors**.

## Known limitations

- The legacy weekly special reserve contains additional hardcoded special-item/potion/food/upgrade rolls. The current C# data model exposes weighted special offers but does not expose the full Java item-factory/unique-origin pipeline, so the refresh hook restores the weighted dungeon special offer and gem pricing without inventing a new data model.
- The existing UI has no dedicated visual controls for the newly exposed speed/listing upgrade APIs. This phase only makes the backend/controller hooks available as requested; no Storage UI or large UI redesign was started.
- Actual device/offline wall-clock validation remains a manual runtime check. EditMode coverage validates the capped delta and state transitions deterministically.

## Rollback

To roll back this phase, restore the backed-up files from:

`D:\Tinh\Rebuild_GuildMaster\Backup\Phase3_Economy_Core\`

and remove the newly added `Assets/_Game/Scripts/Tests/EditMode/Phase3_EconomyCoreTests.cs` plus its generated `.meta` file. Do not delete or restore unrelated pre-existing worktree changes.

Phase 3.4 Storage UI and Phase 4 content were not started.
