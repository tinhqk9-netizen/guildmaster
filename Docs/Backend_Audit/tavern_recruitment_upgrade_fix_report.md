# Tavern Recruitment Regression + Upgrade UX Fix

Date: 2026-08-07  
Project: `D:\Tinh\Rebuild_GuildMaster`

## Scope

Only Tavern recruitment refresh wiring and Tavern capacity/speed upgrade presentation were changed. Phase 5 dialog behavior, equipment ownership, combat formula, save schema and other systems were not changed.

## Backup

`D:\Tinh\Rebuild_GuildMaster\Backup\Phase_Tavern_Recruitment_Upgrade\`

Backed up before edits:

- `TavernDialog.cs`
- `TavernDialogBuilder.cs`
- `HeadquartersHubController.cs`
- `AppShellController.cs`
- `AdventurersTabController.cs`
- `S6_5A_Stage3_TavernTests.cs`
- `TavernDialog.prefab`
- `Main.unity`

## Bug 1 — Recruit succeeded but Adventurers stayed empty

### Evidence and root cause

The backend path was already correct:

```text
TavernDialog.OnRecruitClicked()
  -> ITavernService.RecruitGuest()
  -> CharacterService.RecruitCharacter()
  -> CharacterService._characters.Add()
  -> SaveData.Characters.Add()
```

`HeadquartersHubController` refreshed the Quarters/Tavern cards and HUD after the Tavern callback, but did not refresh the instantiated `AdventurersTabController`. The roster therefore kept its old generated card list and displayed its previous empty state until another setup/refresh path ran.

### Fix

Added `AppShellController.RefreshAdventurersTab()`. The Tavern popup state-change callback now calls it after a successful recruit or Tavern upgrade. It refreshes the already-instantiated Adventurers tab without reconstructing the App Shell or changing the roster data source.

## Bug 2 — Tavern upgrade UX missing in active HQ popup

### Audit result

The backend and formulas already existed:

- `ITavernService.UpgradeTavernCapacity()`
- `ITavernService.UpgradeTavernTime()`
- `GetUpgradeTavernCapacityPrice()`
- `GetUpgradeTavernTimePrice()`
- `GetTavernCapacityLevel()`
- `GetTavernTimeLevel()`
- `FormulaService.GetTavernCapacityPrice()`
- `FormulaService.GetTavernTimePrice()`

The old `TavernScreen` exposed these APIs, but the active Phase 5B `TavernDialog` only displayed guest count, quarters and timer. No new economy logic was required.

### Fix

The active Tavern popup now contains two legacy-styled upgrade rows:

- `Guest Capacity` — level, next cost in gold, `UPGRADE` button.
- `Visitor Speed` — level, next cost in gold, `UPGRADE` button.

Buttons call the existing Tavern service APIs only. They become non-interactable when the player cannot afford the cost or the formula reports the max-level threshold. After a successful action, the dialog, HQ cards, Adventurers roster and HUD refresh through the existing callback flow.

`TavernDialogBuilder` was kept idempotent and rebuilt `TavernDialog.prefab`. `Main.unity` did not change during the rebuild.

## Files modified

- `Assets/_Game/Scripts/Runtime/UI/Shell/AppShellController.cs`
  - Added `RefreshAdventurersTab()`.
- `Assets/_Game/Scripts/Runtime/UI/Shell/HeadquartersHubController.cs`
  - Tavern state-change callback now refreshes the roster.
- `Assets/_Game/Scripts/Runtime/UI/Headquarters/TavernDialog.cs`
  - Added capacity/speed upgrade text and button bindings.
  - Added affordability/max state and refresh flow.
- `Assets/_Game/Scripts/Editor/UI/Legacy/TavernDialogBuilder.cs`
  - Added two upgrade rows and serialized their references.
- `Assets/_Game/Prefabs/UI/Headquarters/TavernDialog.prefab`
  - Rebuilt by `Tools/Guild Master/Legacy UI/Build Tavern Dialog`.
- `Assets/_Game/Scripts/Tests/EditMode/TavernRecruitmentUpgradeRegressionTests.cs`
  - Added four regression tests.

No service, model, SaveData schema, Phase 5 backend or equipment ownership file was changed.

## Tests

New regression tests:

1. `RecruitHero_IsVisibleToAdventurersRosterSource`
2. `RecruitHero_PersistsAfterSaveLoad`
3. `TavernCapacityUpgrade_IncreasesLimitAndConsumesGold`
4. `TavernSpeedUpgrade_DecreasesIntervalAndConsumesGold`

Results:

- Tavern regression tests: **4/4 passed**.
- Existing equipment ownership tests: **6/6 passed**.
- Full EditMode suite: **230/230 passed, 0 failed, 0 skipped**.
- Unity compile: **PASS**, no compile errors; only unused mock-event warnings.

The full suite contains expected error logs from existing fault-injection tests and MCP path-validation tests; these did not cause test failures and no new Tavern error was reported by the targeted tests.

## Rollback

Restore the backed-up files from:

`D:\Tinh\Rebuild_GuildMaster\Backup\Phase_Tavern_Recruitment_Upgrade\`

Remove the added regression test if returning exactly to the pre-task source state. Do not restore unrelated files.

## Known limitation

The upgrade methods already existed and were reused; this task did not alter their existing save-boundary behavior. The UI reflects the live service state immediately and the requested capacity/speed currency tests pass.
