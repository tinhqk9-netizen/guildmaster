# Phase 5D — Workshop Dialog report

Date: 2026-08-05

## Scope

Implemented the Workshop dialog + its Recipes overlay only. Quarters, Tavern, Storage, App Shell, Headquarters Hub layout, services, models, SaveData, and formulas were not redesigned or changed. The only existing runtime controller change is the additive Workshop prefab branch in `HeadquartersHubController` and its refresh wiring — the same pattern already used for Quarters/Tavern/Storage.

## API audit — source of truth

Audited (read-only, no edits):

- `Assets/_Game/Scripts/Runtime/Services/ICraftService.cs`, `CraftService.cs`
- `Assets/_Game/Scripts/Runtime/Models/CraftResult.cs` (`CraftFailureReason` enum)
- `Assets/_Game/Scripts/Definitions/RecipeDefinition.cs` (`IngredientData`)
- `Assets/_Game/Scripts/Runtime/Save/SaveData.cs` (`ItemActionSaveData`, `WorkshopQueue`, `CompletedWorkshopItems`, `LevelWorkshopQueue`/`UpgradeWorkshopQueue`, `LevelWorkshopTime`/`UpgradeWorkshopTime`)
- `Assets/_Game/Scripts/Runtime/Formulas/FormulaService.cs`, `IFormulaService.cs` (`WorkshopQueue`, `GetWorkshopQueuePrice`, `GetWorkshopTimePrice`, `GetSecondsToCraft`)
- `Assets/_Game/Scripts/Runtime/UI/Craft/CraftScreen.cs` (existing S6 screen — proven precedent for every queue/recipe semantic used below)
- `Assets/_Game/Scripts/Runtime/Services/GameLoopService.cs`, `OfflineProgressService.cs` (confirmed `ProgressWorkshop` is auto-ticked, online and offline)
- `Assets/_Game/Scripts/Runtime/Services/ServiceContainer.cs` (confirmed `.Craft`, `.Database`, `.Inventory` property names)
- `Assets/StreamingAssets/GameData/recipes.json` (raw data, to quantify how many recipes actually carry ingredient data)
- `Docs/Legacy_Audit/deep_layout_hierarchy.csv` (`dialog_workshop`, `layout_workshop_item`, `dialog_recipes`, `dialog_craft`)

### `ICraftService` — confirmed real API surface

```csharp
CraftResult CanCraft(string recipeId);
CraftResult TryStartCraft(string recipeId);
void ProgressWorkshop(long deltaSeconds);         // auto-ticked by GameLoopService/OfflineProgressService — UI never calls this
int GetMaxCraftable(string recipeId);             // not used — no batch/quantity picker exists (see below)
bool ClaimCompletedCraft(string instanceId);
int GetQueueCapacity();
IReadOnlyList<ItemActionSaveData> GetQueue();
IReadOnlyList<ItemActionSaveData> GetCompletedItems();
bool UpgradeQueueCapacity();                      // real, but not wired — not requested by this phase's dialog spec (see "Known limitations")
long GetUpgradeQueueCapacityPrice();
int GetQueueCapacityLevel();
```

**No Cancel API exists.** Grepped the full interface and implementation — there is no method to remove a queued item. The Workshop dialog therefore has no Cancel button anywhere, matching instruction 3's "cancel button nếu backend có."

### Queue semantics (`ItemActionSaveData`)

Fields: `InstanceId`, `DefinitionId` (the **output** item), `StackCount`, `SecondsPassed`.

- `CraftService.ProgressWorkshop` only advances `queue[0].SecondsPassed` — crafting is **strictly sequential**, not parallel. Every other queued item sits at `SecondsPassed = 0` ("Waiting...") until it becomes index 0.
- **Craft duration is hard-coded**: `CraftService.DEFAULT_CRAFT_DURATION_SECONDS = 10` (a `private const`, not exposed anywhere on `ICraftService`). `CraftScreen.cs` (S6) independently hard-codes the same literal `10` in its own UI code (`Math.Max(0, 10 - item.SecondsPassed)`), confirming this is the actual, already-established real value — not something invented for this phase. `WorkshopDialog.CraftDurationSeconds` mirrors it with the same value and an explicit comment explaining why.
- When `SecondsPassed >= 10`, `ProgressWorkshop` moves the item from `WorkshopQueue` to `CompletedWorkshopItems` automatically (no `Collect` call needed to *finish* crafting — `Collect`/`ClaimCompletedCraft` only grants the already-finished item to inventory).
- Queue capacity: `GetQueueCapacity()` → `FormulaService.WorkshopQueue(LevelWorkshopQueue, UpgradeWorkshopQueue, PurchaseFlags)` — same formula pattern as Storage's capacity, real and populated.

### Collect (`ClaimCompletedCraft`)

Looks up the item in `CompletedWorkshopItems` by instance id, resolves its `ItemDefinition`, checks `IInventoryService.CanAddItem`, calls `AddItem`, removes it from the completed list, and calls `_saveService.Save()`. Returns `false` (no exception) if the item isn't found or inventory can't accept it (full) — the dialog only refreshes on `true`.

### Recipes — real, but incomplete data

- **No "list all recipes" method on `ICraftService`.** The real way to enumerate recipes — confirmed by reading `CraftScreen.cs`, which already does exactly this — is `ServiceContainer.Database.GetAll<RecipeDefinition>()`. Used verbatim here, not invented.
- `RecipeDefinition`: `OutputItemId` (string), `Ingredients` (`List<IngredientData>` — each `{ItemId, Amount}`). No cost/gold field, no output-stack field (`CraftService.TryStartCraft` hard-codes `outputStack = 1`), no craft-time field, no display-name field.
- `CanCraft(recipeId)` is the single real gate — validates recipe exists, has a non-empty `OutputItemId`, has a non-empty `Ingredients` list, queue has space, and every ingredient amount is owned. The Recipes panel calls this directly per recipe rather than re-implementing any of that logic.
- **Critical finding, verified against the raw data**: `Assets/StreamingAssets/GameData/recipes.json` has **321 total recipes**, of which only **78 (24.3%)** have a non-empty `Ingredients` array — the remaining **243 (75.7%)** have `Ingredients: []` and are marked `"parseStatus": "partial"` with `"parseReasons": ["MANUAL_RULE_REQUIRED", "MISSING_OUTPUT_ITEM", "MISSING_INGREDIENTS"]` in the source data itself. This is a genuine upstream data-extraction gap (the original decompiled Java source apparently didn't yield ingredient lists for most recipes), not a parser bug — `CanCraft` correctly returns `CraftFailureReason.InvalidIngredients` for all of them, and the Recipes panel correctly labels their Craft button "Unavailable" with "No ingredients defined." — confirmed visually in `phase_5d_workshop_recipes.png`.
- **No quantity/batch picker.** Legacy `dialog_craft` has a `seekBar`/`button_minus`/`button_plus`/`number` quantity selector, but `ICraftService.TryStartCraft` has no amount parameter — every call crafts exactly 1 output. Building a quantity UI would have nothing real to drive, so it was not built (instruction 5: "nếu backend chưa đủ UI flow... không tự bịa").
- **No filter/sort.** Legacy `dialog_recipes` has `radioGroup_visibility` (category filter) and `radioGroup_order_by` (sort), matching Storage's original legacy layout. Not required by this phase's instructions, and kept out for the same minimal-scope reasoning already established for Storage (revision 2). All 321 recipes are listed, scrollable, unsorted (`GetAll<RecipeDefinition>()`'s own order).
- **No craft cost (currency) shown** — `RecipeDefinition` has no price/cost field; only ingredients are consumed. Not invented.

### Upgrade APIs that exist but were not wired

- `UpgradeQueueCapacity()` / `GetUpgradeQueueCapacityPrice()` / `GetQueueCapacityLevel()` are **real, complete APIs** (mirrors `ITavernService.UpgradeQuarters`), but instruction 3's required Workshop Dialog field list does not ask for an upgrade button, so none was added — available for a future phase if requested.
- Legacy `dialog_workshop` also has a `button_upgrade_time` row (and `SaveData.UpgradeWorkshopTime` / `FormulaService.GetWorkshopTimePrice` / `GetSecondsToCraft` all exist), but **`ICraftService` has no method to actually upgrade craft time** — `GetSecondsToCraft` is a real, populated formula that `CraftService` never calls (craft duration is the hard-coded `10`, as noted above). Per "không sửa service/model/SaveData/backend/formula" and "không tự tạo action hoặc formula mới," no Upgrade Time button was built — there is no real API to call.

## Implementation

Created:

- `Assets/_Game/Scripts/Runtime/UI/Headquarters/WorkshopDialog.cs` — main dialog controller (queue + completed unified list, capacity text, Recipes/Close buttons).
- `Assets/_Game/Scripts/Runtime/UI/Headquarters/WorkshopRecipePanel.cs` — recipe overlay (real recipe list via `GetAll<RecipeDefinition>()`, ingredient owned/required text, Craft button gated by `CanCraft`).
- `Assets/_Game/Scripts/Runtime/UI/Headquarters/WorkshopRowBuilder.cs` — shared runtime row builder (icon, name, status/ingredients text, optional progress bar, optional action button) used by both the queue list and the recipe list.
- `Assets/_Game/Scripts/Editor/UI/Legacy/WorkshopDialogBuilder.cs` — Editor prefab builder (`Tools/Guild Master/Legacy UI/Build Workshop Dialog`), wires `HeadquartersHubController._workshopDialogPrefab` into `Main.unity`.
- `Assets/_Game/Prefabs/UI/Headquarters/WorkshopDialog.prefab`.

Modified:

- `Assets/_Game/Scripts/Runtime/UI/Shell/HeadquartersHubController.cs` — added `_workshopDialogPrefab` field and a Workshop popup branch in `OpenBuildingPopup`, following the exact Quarters/Tavern/Storage pattern (`Setup` → `onClose` closes the shell popup, `onStateChanged` refreshes cards + HUD). Quarters/Tavern/Storage branches were not touched.
- `Assets/_Game/Scripts/Editor/UI/Legacy/AppShellScreenshotTool.cs` — added Phase 5D test/verification menu item (`5D Workshop Full Flow`) — additive only, no existing method edited.
- `Assets/_Game/Scenes/Main.unity` — builder serialized the Workshop prefab reference into `HeadquartersHubController`.

No service, model, `SaveData`, or `FormulaService` file was modified.

## Visual hierarchy

`WorkshopDialog`
- Title ("Workshop")
- Queue count text (`{queue.Count} / {capacity}`)
- `EmptyState` text (shown when both queue and completed are empty; placed above the scroll, per the Phase 5C lesson about flexible-height scroll areas burying trailing text)
- `ListScroll` (`ScrollRect` + `RectMask2D`) → `ListContent` (`VerticalLayoutGroup`, real rows in order: queue items first, then completed items) — each row via `WorkshopRowBuilder`:
  - Queue index 0: status `"Crafting... {10 - SecondsPassed}s"` + a real progress-fill bar (`SecondsPassed / 10`)
  - Queue index > 0: status `"Waiting..."`, no progress bar (no real per-item progress data exists for unstarted items)
  - Completed items: status `"Ready!"` + a real **Collect** button (`ClaimCompletedCraft`)
- Recipes button, Close button
- `RecipeOverlay` (child of the dialog, `LayoutElement.ignoreLayout = true` — floats above everything without joining the `VerticalLayoutGroup`, never a second `AppShellController.OpenPopup` call): Title, empty state, scrollable recipe list (icon, name, ingredient owned/required text with red highlighting when insufficient, Craft button gated by `CanCraft`), its own Close button that only hides the overlay.

## Queue / Craft / Collect flow

- **Read-only queue rendering**: `WorkshopDialog.Refresh()` calls `GetQueue()`/`GetCompletedItems()`/`GetQueueCapacity()` and rebuilds the list — never mutates anything.
- **Craft**: `WorkshopRecipePanel` calls `ICraftService.TryStartCraft(recipeId)` only when the user clicks a Craft button that `CanCraft` already validated as interactable. On success, the panel refreshes its own ingredient counts and calls `onCraftChanged`, which bubbles up through `WorkshopDialog.OnStateChangedInternal()` to refresh the queue list/capacity text **and** the same `onStateChanged` callback `HeadquartersHubController` already uses for Quarters/Tavern/Storage (`RefreshCards()` + `AppShellController.RefreshHud()`), so the Workshop card and the Storage card (ingredients were just consumed from inventory) update together.
- **Collect**: clicking a completed row's Collect button calls `ClaimCompletedCraft(instanceId)`; on success, the same `OnStateChangedInternal()` path refreshes the queue/completed list, Workshop card, Storage card (item was just added to inventory), and HUD.
- Neither action ever reads/writes `SaveData` directly, decrements ingredients itself, grants the output item itself, or computes elapsed time itself — every number displayed (`SecondsPassed`, queue count, capacity) comes straight from `ICraftService`.

## Popup lifecycle

Workshop opens through the existing `AppShellController.OpenPopup`/`PopupRoot`, with the same singleton guard, backdrop layering, and destroy-on-close behavior already proven by Quarters/Tavern/Storage — `HeadquartersHubController`'s Workshop branch is structurally identical to the other three. The Recipes overlay is a child of the Workshop dialog itself (see "Visual hierarchy"), so it can never conflict with the shell's one-popup-at-a-time guard and never orphans independently of Workshop.

## Compile and test result

- Unity batchmode recompile: **0 errors, 0 new warnings** (one intermediate compile briefly failed with `CS0246: WorkshopDialog could not be found` — caused by `HeadquartersHubController.cs` referencing the brand-new `WorkshopDialog.cs` before Unity's `AssetDatabase` had imported the new file; fixed by running `Assets/Refresh` before the next recompile, not a code defect).
- Prefab build (`WorkshopDialogBuilder`): completed successfully, Main scene wiring applied, idempotent.
- Runtime full-flow test (`AppShellScreenshotTool.TestWorkshopDialogFlow`), fresh Play Mode, `Main.unity`, natural save (never hacked):
  - `Popup_workshop` opened via the Workshop card: **0 / 1** queue (naturally empty in this save).
  - Empty state correctly shown and captured (`phase_5d_workshop.png` and `phase_5d_workshop_empty.png` are byte-identical — expected, since nothing changed on screen between the two capture points; both legitimately show the same real empty-queue state).
  - No completed/ready item existed naturally, so the "ready" branch was skipped (logged, not faked).
  - Recipes overlay opened cleanly over Workshop (no second popup, no orphan) and was captured: real recipe names, real per-ingredient owned/required counts with red highlighting for shortfalls, and correct button labels (`"Missing"` for `MissingIngredients`, `"Unavailable"` for `InvalidIngredients`/no-ingredient-data recipes).
  - Craft was attempted on every recipe with an interactable Craft button — **none were interactable in this save** (every recipe either has no ingredient data at all, or the player is missing the required ingredients), so no craft was performed; logged clearly as `"No recipe with an interactable Craft button — natural save cannot craft right now."` No `SaveData` hack was used to force a success.
  - Recipes closed → back to Workshop (not the whole dialog). Workshop Close → `IsPopupOpen=False`, `orphanExists=False`.
  - Regression check: Quarters, Tavern, and Storage all opened/closed normally (`opened=True, closedAfter=True` for each).
- No new red console errors. Many `[LegacySpriteRegistry] Missing sprite '...'` **warnings** appeared while rendering the 321-recipe list — see "Known limitations." The one pre-existing error (`Some objects were not cleaned up when closing the scene... Canvas / Main Camera`) predates all Phase 5 work and is unrelated.

### Test coverage / not exercised (per instruction — no SaveData hack)

- **Craft success path**: not exercised — the current save cannot satisfy `CanCraft` for any of the 78 ingredient-bearing recipes. Verified the gating logic is correct (real `CanCraft` result drives button state/label) but the actual `TryStartCraft` success branch, and its "item appears in queue with a live countdown" behavior, were not observed end-to-end in this run.
- **Collect (`ClaimCompletedCraft`) success path**: not exercised for the same reason — no item ever reached `CompletedWorkshopItems` naturally in this save during testing.
- **Sequential queue behavior** (index 0 crafting with a progress bar vs. index > 0 "Waiting..."): implemented per the real `ProgressWorkshop` semantics and code-reviewed, but not visually confirmed with 2+ queued items since the queue never had more than 0 items during this test run.

## Screenshots

- [Workshop dialog — empty queue, minimal layout](D:/Tinh/Rebuild_GuildMaster/Docs/Legacy_Audit/Asset_Gallery/phase_5d_workshop.png)
- [Workshop dialog — empty state explicitly confirmed](D:/Tinh/Rebuild_GuildMaster/Docs/Legacy_Audit/Asset_Gallery/phase_5d_workshop_empty.png)
- [Recipes overlay — real recipe list, ingredient gating, no orphan/second-popup](D:/Tinh/Rebuild_GuildMaster/Docs/Legacy_Audit/Asset_Gallery/phase_5d_workshop_recipes.png)

`phase_5d_workshop_queue.png` and `phase_5d_workshop_ready.png` were not produced — the current save has no craftable recipe and no completed item, so neither applicable condition (instruction 11: "nếu craft tự nhiên thành công" / "nếu có completed state") was met. Per instruction 10 ("Không hack SaveData để ép test... test disabled/empty state và ghi rõ nhánh success chưa xác minh"), this is disclosed rather than forced.

## Rollback

Restore the pre-Phase-5D files from:

`D:\Tinh\Backups\Legacy_UI_Phase_5D_Workshop\`

(contains `Scripts_Before/` and `Scenes_Before/` — full snapshots of `Assets/_Game/Scripts` and `Assets/_Game/Scenes` taken before any Phase 5D edit). To roll back: restore those two folders over the current ones, delete `Assets/_Game/Prefabs/UI/Headquarters/WorkshopDialog.prefab` (+ `.meta`), and recompile.

## Known limitations

- **75.7% of recipes (243/321) have no ingredient data** in `recipes.json` itself (`Ingredients: []`, `parseStatus: "partial"`, upstream extraction gap — not a parser or UI bug). Their Craft button always shows "Unavailable." Only 78 recipes are potentially craftable given complete ingredient data.
- **Craft success and Collect success were not verified end-to-end** in this test run — the natural save had no satisfiable recipe and no completed item. The code paths are real (direct calls to `TryStartCraft`/`ClaimCompletedCraft`, gated by their own real return values) but were not observed producing a queued/collected item live.
- **No Cancel action** — `ICraftService` has no API to remove a queued item.
- **No Upgrade Queue Capacity button** — the API (`UpgradeQueueCapacity`) is real and complete, but wasn't requested by this phase's Workshop Dialog field list; available to wire in a future phase.
- **No Upgrade Craft Time button** — legacy has one, and `SaveData`/`FormulaService` have the supporting fields/formula (`UpgradeWorkshopTime`, `GetWorkshopTimePrice`, `GetSecondsToCraft`), but `ICraftService` exposes no method to actually apply it, and craft duration is hard-coded to 10s regardless. Building this button was out of scope (would require a new service method — forbidden by "không tự tạo action hoặc formula mới").
- **No quantity/batch craft picker** — legacy `dialog_craft` has one, but `TryStartCraft` always crafts exactly 1 output; a picker would have nothing real to control.
- **No filter/sort on the Recipes list** — legacy `dialog_recipes` has category filter + sort radio groups; kept out per this phase's minimal-scope instructions (same reasoning as Storage revision 2).
- **Many recipe output items have no matching legacy sprite** (`LegacySpriteRegistry` logged dozens of "Missing sprite" warnings while rendering the 321-recipe list — e.g. `wyverncape`, `mithrilsword`, `voidarmor`). These are real high-tier/crafted items whose sprite keys don't exist in the imported catalog; icons fall back to a dim placeholder, matching the same graceful-fallback behavior established in Storage/Item Detail. Not a crash, not fabricated data.

Phase 5D stops here. No Shelter or later phase was started.
