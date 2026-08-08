# Phase 3 Workshop Craft Data Fix

Date: 2026-08-07

## Root cause

The decoded recipe data uses legacy output IDs without separators while the canonical
`ItemDefinition.id` values retain underscores. For example:

- Recipe output: `woodenbuckler`
- Item definition: `wooden_buckler`

Before this fix, `CraftService.GetCraftDurationSeconds()` could not resolve the output and
returned `0`. The next one-second runtime tick then completed the queue item, while
`ClaimCompletedCraft()` returned before `InventoryService.AddItem()` because the output
definition was missing.

## Data resolution

`CanonicalItemIdResolver` was added at the database boundary.

Resolution order:

1. Exact case-insensitive `ItemDefinition.id` match.
2. Lowercase alpha-numeric normalization, removing separators such as `_`, `-`, and spaces.
3. Ambiguous normalized matches are rejected and logged; no arbitrary choice is made.

After all JSON categories load, `DatabaseBuilder` canonicalizes recipe output and ingredient
IDs, logs unresolved records with recipe ID/output or ingredient ID/reason, and excludes invalid
recipes from the runtime recipe registry.

Measured against the current decoded data:

| Metric | Before | After |
|---|---:|---:|
| Recipes loaded | 321 | 321 |
| Recipe outputs resolved | 0 direct matches / 321 normalized matches | 321 canonical IDs |
| Ingredient references | 9 direct / 69 normalized | 78 canonical IDs |
| Invalid recipes removed | N/A | 0 |

## Craft safety

`CraftService.CanCraft()` now rejects a recipe whose output is not an
`ItemDefinition` before any ingredient is consumed. `ProgressWorkshop()` also refuses to
advance an already-corrupt persisted action with duration `0`, preventing instant completion.
Persisted workshop queue/completed actions are canonicalized on service construction and saved
when an ID changes.

No UI, recipe filter, formula, queue schema, or SaveData schema was changed.

## Files changed

- `Assets/_Game/Scripts/Database/CanonicalItemIdResolver.cs`
- `Assets/_Game/Scripts/Database/DatabaseBuilder.cs`
- `Assets/_Game/Scripts/Runtime/Services/CraftService.cs`
- `Assets/_Game/Scripts/Tests/EditMode/Phase3_WorkshopCraftDataResolutionTests.cs`
- `Docs/Backend_Audit/phase3_workshop_craft_data_fix_report.md`

## Regression tests

`GuildMaster.Tests.EditMode.Phase3_WorkshopCraftDataResolutionTests`: **6/6 passed**

- `AllRecipes_OutputResolvesToItemDefinition`
- `Craft_InvalidOutputCannotStart`
- `Craft_ValidRecipeHasPositiveDuration`
- `Craft_CompleteAddsItemToInventory`
- `Craft_CancelStillRefundsMaterial`
- `Save_LoadKeepsWorkshopQueue`

Existing `GuildMaster.Tests.EditMode.Phase3_EconomyCoreTests`: **7/7 passed**.

Compile: **0 errors, 0 warnings**.

## Backup

Pre-change source backup:

`D:\Tinh\Rebuild_GuildMaster\Backup\Phase3_Workshop_Craft_Data_Fix\`

The earlier audit save snapshot remains at:

`D:\Tinh\Rebuild_GuildMaster\Backup\Phase3_Workshop_Deep_Regression\Audit_PreFix\save.json`

## Known limitation

An unresolvable legacy action already present in a save is left in place and logged rather than
silently deleted or refunded. New recipes cannot create such an action, and known separator-only
legacy IDs are migrated to canonical IDs during `CraftService` construction.
