# Phase 3 Economy UX Fix Report

Date: 2026-08-07  
Project: `D:\Tinh\Rebuild_GuildMaster`  
Scope: Phase 3 Economy UI fixes only. Phase 4 was not started.

## Result

Completed the three requested tasks:

1. Market cancel persistence and live remaining-time refresh.
2. Workshop recipe availability grouping and material feedback.
3. Editor-only economy verification tooling using the `DEV_TEST_` prefix.

## Task 1 — Market Cancel + Timer

### Root cause

The callback path was already bound to the correct listing instance:

`MarketDialog.Refresh()` → `WorkshopRowBuilder.CreateRow(...)` → `OnCancelClicked(instanceId)` → `IMerchantService.CancelListing(instanceId)`.

`MerchantService.CancelListing` correctly found the listing, returned the listed item through `InventoryService.AddItem`, and removed the listing. It did not persist the updated save data afterward. Therefore the UI could appear cancelled while a reload could restore the old active listing.

The dialog also had no periodic refresh while `GameLoopService` advanced `SecondsPassed`, so the displayed countdown could remain at its previous value until another UI refresh.

### Fix

- `MerchantService.CancelListing` now calls `_saveService.Save(out _)` after the refund and listing removal.
- `MarketDialog` now refreshes once per realtime second while enabled.
- The timer continues to use `MerchantService.GetSellDurationSeconds(item)` and `duration - SecondsPassed`, so it displays remaining time from runtime state rather than the initial duration.
- No market formula or backend progression rule was changed.

## Task 2 — Workshop Recipe Filter

`WorkshopRecipePanel` now separates recipes into:

- `AVAILABLE RECIPES`: valid recipes with sufficient materials, marked `✓ Craftable` and enabled.
- `UNAVAILABLE RECIPES`: disabled recipes with owned/required ingredient counts and a missing-material or unavailable reason.

The panel still calls the existing `CraftService.CanCraft` API. No craft backend, formula, queue, or save model was changed.

## Task 3 — Economy Verification Tool

Added the Unity editor menu:

- `Tools > GuildMaster > Economy Verification`
- `Give Starter Dungeon Materials`
- `Clear Test Materials`
- `Print Craftable Recipe Report`
- `Reset Market and Workshop State`

The tool:

- Uses `SaveService` and the project database provider.
- Adds only material drops from the first dungeon using instance IDs beginning with `DEV_TEST_`.
- Clears only `DEV_TEST_` items.
- Reports available and unavailable recipes to `Docs/Backend_Audit/DEV_TEST_craftable_recipe_report.txt`.
- Requires confirmation before clearing/resetting and refuses to run while Play Mode is active.

## Files modified/created

- `Assets/_Game/Scripts/Runtime/Services/MerchantService.cs`
- `Assets/_Game/Scripts/Runtime/UI/Headquarters/MarketDialog.cs`
- `Assets/_Game/Scripts/Runtime/UI/Headquarters/WorkshopRecipePanel.cs`
- `Assets/_Game/Scripts/Tests/EditMode/Phase3_EconomyCoreTests.cs`
- `Assets/_Game/Scripts/Editor/Tools/EconomyVerificationWindow.cs`
- `Docs/Backend_Audit/DEV_TEST_craftable_recipe_report.txt` (tool output)
- `Docs/Backend_Audit/phase3_economy_ux_fix_report.md`

No prefab, service API, model, save schema, or Phase 4 file was changed beyond the requested Market service persistence fix.

## Verification

### Compile

- Unity script compilation: **0 errors**.
- **8 warnings** remain from existing unused events in test mocks (`CS0067`); no new compile error was introduced.

### Automated tests

- Focused `Phase3_EconomyCoreTests`: **7/7 passed**.
- Full EditMode suite: **197/197 passed, 0 failed, 0 skipped**.
- New regression test: `Market_CancelListing_RestoresItemRemovesListingAndPersists`.
  It verifies refund, active-list removal, and exactly one save after cancel.

### Editor tool test

- Give starter materials: **12 DEV_TEST materials added**.
- Clear test materials: **12 DEV_TEST materials removed**.
- Normal save state remained intact: Money `301`, normal item count `5` after cleanup.
- Craftable recipe report generated successfully.

### Main scene runtime smoke test

Fresh Play Mode was run on `Assets/_Game/Scenes/Main.unity` at the existing mobile portrait setup.

- Market dialog opened and closed; no orphan popup remained.
- Quarters, Tavern, Storage, and Workshop regression open/close checks passed.
- Workshop queue empty state and recipe screen checks passed.
- Captured existing Workshop screenshots:
  - `Docs/Legacy_Audit/Asset_Gallery/phase_5d_workshop.png`
  - `Docs/Legacy_Audit/Asset_Gallery/phase_5d_workshop_empty.png`
  - `Docs/Legacy_Audit/Asset_Gallery/phase_5d_workshop_recipes.png`
- Unity was returned to **Edit Mode** after the smoke test.

The current natural save had no active market listing, so a manual Play Mode click on an active Cancel row was not available without altering save data. The actual listing/refund/save path is covered by the focused regression test above.

One scene-cleanup console error was present before the latest smoke-flow logs (`Canvas`/`Main Camera` cleanup). No new error was emitted by the Market or Workshop flow after the changes.

## Backup and rollback

Pre-edit backup:

`D:\Tinh\Rebuild_GuildMaster\Backup\Phase3_Economy_UX_Fix\`

To roll back the code/prefab state, restore the backed-up files from that directory, refresh Unity assets, and recompile. The generated `DEV_TEST_craftable_recipe_report.txt` is a diagnostic output and may be deleted independently. `Clear Test Materials` removes only save items with the `DEV_TEST_` prefix.

## Known limitations

- Runtime Cancel button success was not manually clicked because the current save had no active listing; automated coverage verifies the real service path.
- Recipe availability is presented as grouped enabled/disabled sections; unavailable recipes remain visible so their missing materials can be understood.
- The existing scene-cleanup error should be audited separately; it is outside this Phase 3 economy UX scope and was not caused by the changed Market/Workshop code based on the timestamp and smoke logs.

Phase 3 Economy UX Fix is complete. No Phase 4 work was started.
