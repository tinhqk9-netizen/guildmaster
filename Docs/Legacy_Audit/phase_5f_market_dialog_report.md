# Phase 5F — Market Dialog report

Date: 2026-08-05

## Scope

Implemented the Market dialog (Selling / Sold / Buy sections) only. Quarters, Tavern, Storage, Workshop, Shelter, App Shell, Headquarters Hub layout, services, models, SaveData, and formulas were not redesigned or changed. The only existing runtime controller change is the additive Market prefab branch in `HeadquartersHubController` and replacing the Market card's "Coming soon" placeholder text with real counts — the dialog wiring itself follows the same pattern already used for Quarters/Tavern/Storage/Workshop/Shelter.

## API audit — source of truth

Audited (read-only, no edits):

- `Assets/_Game/Scripts/Runtime/Services/IMerchantService.cs`, `MerchantService.cs`
- `Assets/_Game/Scripts/Runtime/Save/SaveData.cs` (`ItemActionSaveData`, `MerchantOfferSaveData`, `MarketListings`, `SoldMarketItems`, `MerchantRegularStockItems`, `MerchantSpecialReserve`, `LevelMarketListings`)
- `Assets/_Game/Scripts/Runtime/Services/GameLoopService.cs`, `OfflineProgressService.cs` (confirmed `ProgressMarket` is auto-ticked, online and offline — same mechanism already verified for Workshop's `ProgressWorkshop` in Phase 5D)
- `Assets/_Game/Scripts/Runtime/Services/ServiceContainer.cs` (confirmed `.Merchant`, `.Inventory`, `.Save`, `.Database` property names)
- This audit reuses the `IMerchantService.SellItem` findings already made in the Phase 5C Storage report (the Sell action lives in Storage's Item Detail, not here) and verifies them against the live-running game rather than just reading code.

### `IMerchantService` — confirmed real API surface

```csharp
MerchantOfferData RollRegularOffer(string dungeonId);
MerchantOfferData RollSpecialOffer(string dungeonId);
bool BuyOffer(MerchantOfferSaveData offer, bool isSpecial);
MerchantResult SellItem(string definitionId, int stackCount);   // called from Storage's Item Detail, Phase 5C — not this dialog
void ProgressMarket(long deltaSeconds);                          // auto-ticked by GameLoopService/OfflineProgressService — UI never calls this
bool ClaimSoldItem(string instanceId);
IReadOnlyList<MerchantOfferSaveData> GetRegularStock();
IReadOnlyList<MerchantOfferSaveData> GetSpecialStock();
IReadOnlyList<ItemActionSaveData> GetMarketListings();
IReadOnlyList<ItemActionSaveData> GetSoldMarketItems();
```

**No "Claim All" method exists.** Grepped the full interface/implementation — only a single-instance `ClaimSoldItem(instanceId)` exists. Per instruction 9 ("Claim All chỉ khi backend có API thật"), no Claim All button was built.

### Selling → Sold flow (verified against the live running game, not just read from code)

- `SellItem(definitionId, stackCount)` (called elsewhere, from Storage — see Phase 5C report) consumes the item via `ConsumeByDefinitionId` and appends an `ItemActionSaveData` to `SaveData.MarketListings`. **Money is NOT credited at this point.**
- `ProgressMarket(deltaSeconds)` — auto-ticked every frame by `GameLoopService` (and by `OfflineProgressService` for offline elapsed time) — only advances `MarketListings[0].SecondsPassed` (**strictly sequential, index-0-only**, identical pattern to Workshop's `ProgressWorkshop`). When `SecondsPassed >= DEFAULT_SELL_TIME_SECONDS` (a `private const = 20`, not exposed on `IMerchantService` — the same "hard-coded, unexposed duration" situation already documented for Workshop's craft time), the entry moves from `MarketListings` to `SoldMarketItems`.
- `ClaimSoldItem(instanceId)` computes `payout = (ItemDefinition.SellPrice > 0 ? SellPrice : 100) * StackCount` (via `DecodeMath.TruncatePrice`), adds it to `SaveData.Money`, and removes the entry from `SoldMarketItems`. **Money is only credited here, on Claim — never at sell time or at listing-completion time.** Claim is by **instance id**, not definition id.
- **Verified live, not just read**: this test run's save had accumulated real `SoldMarketItems` from Phase 5C's Storage testing (where a real Sell action was exercised on `Copper Sword`/`Abyssal Cutlass`), and `GameLoopService` had ticked enough real elapsed time across sessions for those listings to complete on their own. This dialog displayed them correctly and a live Claim call increased the player's Money and updated the HUD — see "Compile and test result."

### Buy / Merchant Stock — real API, but never populated by anything

`GetRegularStock()`/`GetSpecialStock()` read `SaveData.MerchantRegularStockItems`/`MerchantSpecialReserve` — real, real accessors. `BuyOffer(offer, isSpecial)` is a complete, real purchase flow (checks `CanAddItem`, checks `Money`/`Gems` vs `offer.Price`, deducts currency, removes the offer from stock, grants the item via `AddItem`).

**However, grepping every call site of `RollRegularOffer`/`RollSpecialOffer` (the only methods that produce a `MerchantOfferData` to push into stock) found zero callers anywhere in the runtime.** Nothing ever adds an entry to `MerchantRegularStockItems`/`MerchantSpecialReserve` in the current codebase — `Roll*Offer` requires a `dungeonId` and reads `DungeonDefinition.RegularMerchantOffers`/`SpecialMerchantOffers`, implying a dungeon-completion trigger that isn't wired to the Merchant system yet. Per instruction 9 ("Nếu buy backend chưa đủ, không bịa stock"), the Buy section calls the real `GetRegularStock()`/`GetSpecialStock()` (so it is real, not fabricated) but will always render its collapsed/empty state until that trigger is built in a future phase — no stock is invented to populate it.

## Market Dialog — what's actually shown

Single scrollable list with 3 conditionally-rendered sections (a section and its divider are omitted entirely when empty, rather than showing 3 "nothing here" placeholders):

### Selling
- Icon (`LegacySpriteRegistry.GetItemSprite`), name (formatted `DefinitionId`), quantity
- Index 0: `"Selling... {20 - SecondsPassed}s (est. {payout}g)"` + a real progress-fill bar (`SecondsPassed / 20`)
- Index > 0: `"Waiting... (est. {payout}g)"`, no progress bar (no real per-item progress exists until it reaches index 0 — same reasoning as Workshop's queue)
- No action button (selling items cannot be claimed or cancelled — no API for either)

### Sold
- Icon, name, quantity, `"Payout: {payout}g"`, real **Claim** button → `ClaimSoldItem(instanceId)`

### Buy
- Only rendered if `GetRegularStock()` or `GetSpecialStock()` is non-empty (currently never, per the audit above)
- Icon, name, quantity, `"Price: {price}{g|gem}"`, **Buy** button gated by real `CanAddItem` + real currency comparison against `offer.Price`/`offer.IsGems`

### Expected payout — real formula, not invented

`ComputeExpectedPayout` mirrors `MerchantService.ClaimSoldItem`'s exact fallback (`ItemDefinition.SellPrice > 0 ? SellPrice : 100`), used only for *display* before claim — never used to credit currency itself. Since `ItemDefinition.SellPrice` is confirmed unpopulated for every item (Phase 5C finding, re-confirmed live here: every claimed item showed `Payout: 100g`), the estimate is almost always the `100` fallback in the current data — this is disclosed, not hidden.

## Popup lifecycle

Market opens through the existing `AppShellController.OpenPopup`/`PopupRoot`, with the same singleton guard, backdrop layering, and destroy-on-close behavior already proven by every prior Phase 5 dialog. Market has no child overlay (no equivalent of Storage's Item Detail or Workshop's Recipes panel was needed — Claim/Buy are single-click row actions with no secondary view).

## Implementation

Created:

- `Assets/_Game/Scripts/Runtime/UI/Headquarters/MarketDialog.cs` — dialog controller (Selling/Sold/Buy sections, Claim/Buy actions, real payout estimate).
- `Assets/_Game/Scripts/Editor/UI/Legacy/MarketDialogBuilder.cs` — Editor prefab builder (`Tools/Guild Master/Legacy UI/Build Market Dialog`), idempotent, wires `HeadquartersHubController._marketDialogPrefab` into `Main.unity`.
- `Assets/_Game/Prefabs/UI/Headquarters/MarketDialog.prefab`.

Reused (not duplicated, per instruction 2 "hạn chế tạo test tool/wirer tạm dư thừa"):

- `Assets/_Game/Scripts/Runtime/UI/Headquarters/WorkshopRowBuilder.cs` (Phase 5D) — used as-is for every Selling/Sold/Buy row (icon, name, status text, optional progress bar, optional action button). No Market-specific row builder was created.

Modified:

- `Assets/_Game/Scripts/Runtime/UI/Shell/HeadquartersHubController.cs` — added `_marketDialogPrefab` field and a Market popup branch in `OpenBuildingPopup`. Replaced the Market card's `"Coming soon"` display (previously correct, since no dialog existed) with `"Selling {n} • Sold {n}"` from real `GetMarketListings()`/`GetSoldMarketItems()` counts, per instruction 9 ("Không để Coming Soon nếu MerchantService đã có flow thật").
- `Assets/_Game/Scripts/Editor/UI/Legacy/AppShellScreenshotTool.cs` — added Phase 5F test/verification menu item (`5F Market Full Flow`), sharing the new `RegressionCheck` helper added alongside Phase 5E's test (see that report).
- `Assets/_Game/Scenes/Main.unity` — builder serialized the Market prefab reference into `HeadquartersHubController`.

No service, model, `SaveData`, or `FormulaService` file was modified.

## Backend calls / action flow

- **Refresh (read-only)**: `GetMarketListings()`, `GetSoldMarketItems()`, `GetRegularStock()`, `GetSpecialStock()` — never mutates.
- **Claim**: `ClaimSoldItem(instanceId)` → on success, `MarketDialog.OnStateChangedInternal()` refreshes the list **and** calls the same `onStateChanged` callback `HeadquartersHubController` wires for every other dialog (`RefreshCards()` + `AppShellController.RefreshHud()`), so the Market card's Sold count and the HUD's currency both update immediately.
- **Buy**: `BuyOffer(offer, isSpecial)` → same refresh path on success (Storage card would also change, since `BuyOffer` grants an item to inventory) — real, but currently unreachable in practice since stock is never populated (see audit).
- Nothing in this dialog ever writes `SaveData` directly, computes a payout that gets credited, or advances any timer itself.

## Compile and test result

- Unity batchmode recompile: **0 errors, 0 warnings.**
- Prefab build (`MarketDialogBuilder`): completed successfully, Main scene wiring applied, idempotent.
- Runtime full-flow test (`AppShellScreenshotTool.TestMarketDialogFlow`), fresh Play Mode, `Main.unity`, natural save (never hacked):
  - `Popup_market` opened via the Market card. **Not empty** — the save had 6 real `SoldMarketItems` entries (`Abyssal Cutlass` ×2, `Copper Sword` ×4), carried over from Phase 5C's Storage Sell test plus real elapsed `ProgressMarket` ticks across sessions. No active `Selling` entry existed at test time (its 20s timer had already completed before this session), so the "Selling" section was correctly omitted (not a bug — conditionally-rendered per real data, as designed).
  - Sold section rendered correctly: real item names, `Payout: 100g` each (the `SellPrice`-unpopulated fallback, confirmed live).
  - **Claim exercised successfully, live**: the first interactable Claim button was clicked → `ClaimSoldItem` returned `true` → the row disappeared, the list refreshed, and the HUD's currency display visibly increased (platinum coin count went from 80 → 81 in the captured screenshots, confirming the 100g payout converted through `LegacyCurrencyAdapter` and rendered in the HUD).
  - Market Close → `IsPopupOpen=False`, `orphanExists=False`.
  - Regression check: Quarters, Tavern, Storage, and Workshop all opened/closed normally.
- No new red console errors. The one pre-existing error (`Some objects were not cleaned up when closing the scene... Canvas / Main Camera`) predates all Phase 5 work and is unrelated.

### Test coverage / not exercised

- **Active "Selling" state with a live countdown** was not captured in this run — the save's one prior sale had already fully completed (moved to Sold) by the time this test ran. The rendering logic (progress bar, remaining-seconds text) is code-reviewed against the real `ItemActionSaveData.SecondsPassed` semantics and the identical, already-verified Workshop queue pattern, but not visually confirmed mid-countdown here.
- **Buy** was not exercised — `GetRegularStock()`/`GetSpecialStock()` are empty in the current save and always will be until a dungeon-completion trigger populates them (see audit) — not a testing gap, a real absence of data.
- **Claim All** does not exist (no API) — nothing to test.

## Screenshots

- [Sold section — real items from Phase 5C's Sell action, real 100g payout](D:/Tinh/Rebuild_GuildMaster/Docs/Legacy_Audit/Asset_Gallery/phase_5f_market_sold.png)
- [After a live Claim — row removed, HUD currency increased](D:/Tinh/Rebuild_GuildMaster/Docs/Legacy_Audit/Asset_Gallery/phase_5f_market_claim.png)

`phase_5f_market_selling.png` and `phase_5f_market_empty.png` were not produced: no active Selling listing existed at test time (already completed), and the Market was not empty (real Sold data existed). Per instruction 13, both are disclosed as not-applicable-to-this-save rather than forced via a `SaveData` hack.

## Rollback

Restore the pre-Phase-5E/5F files from:

`D:\Tinh\Backups\Legacy_UI_Phase_5EF_Shelter_Market\`

(contains `Scripts_Before/` and `Scenes_Before/` — full snapshots of `Assets/_Game/Scripts` and `Assets/_Game/Scenes` taken before any Phase 5E/5F edit). To roll back Market specifically: restore those two folders over the current ones (this also reverts Shelter — see that report if only one should be rolled back), delete `Assets/_Game/Prefabs/UI/Headquarters/MarketDialog.prefab` (+ `.meta`), and recompile. Note: rolling back the scripts does **not** undo the real `SoldMarketItems`/`Money` state changes already persisted to the player's save by the live Claim test — that is expected, real game-state progress, not a bug to revert.

## Known limitations

- **Buy/Merchant Stock has no way to ever become non-empty** in the current codebase — `RollRegularOffer`/`RollSpecialOffer` are real but have zero callers anywhere; nothing populates `MerchantRegularStockItems`/`MerchantSpecialReserve`. The Buy section is real (calls the real APIs) but will always render empty until a dungeon-completion flow is wired in a future phase. Not fabricated.
- **No Claim All** — `IMerchantService` has no such method.
- **Sell duration (20s) is hard-coded and unexposed** — mirrors `MerchantService.DEFAULT_SELL_TIME_SECONDS` (private), the same situation as Workshop's craft duration.
- **Expected/actual payout is almost always the `100` fallback**, since `ItemDefinition.SellPrice` is confirmed unpopulated for essentially every item (Phase 5C finding, re-confirmed live in this test — all 6 claimed items paid exactly 100g).
- **Active-Selling countdown UI was not visually verified** — no listing was mid-countdown during this test run (see "Test coverage").

Phase 5F stops here (paired with Phase 5E in the same request — see `phase_5e_shelter_dialog_report.md`). No Phase 6 work was started.
