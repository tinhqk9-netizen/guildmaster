# RESTORE_3 — ECONOMY

**Goal:** Workshop, Merchant, Market, Shop full trace + timers.
**Effort:** ~1 day
**Dependencies:** RESTORE_1 PASS (economy generates/spends resources core loop produces)
**Risk:** 🟢 LOW — mostly verification + minor timer UI
**Gate:** Craft→Claim, Buy→Sell→Claim end-to-end traced, all timers visible

---

## Tasks

### T3-1: Craft System Verification

1. Confirm `CraftScreen` opens from HUD
2. Map full craft flow:
   - `CraftScreen.Show()` → `CraftService.GetQueue()`, `CraftService.GetAvailableRecipes()`
   - `CraftScreen.StartCraft(defId)` → `CraftService.TryStartCraft(defId)`:
     - Checks materials in InventoryService
     - Deducts materials
     - Adds to `SaveData.WorkshopQueue`
     - Sets completion time (based on RecipeDefinition craft time)
   - `CraftScreen.ClaimCompleted()` → `CraftService.ClaimCompletedCraft()`:
     - Checks if craft timer expired
     - Creates item via `ItemService.CreateItem(defId)` → `InventoryService.AddItem()`
     - Removes from queue → adds to `SaveData.CompletedWorkshopItems`
3. **Fix CRAFT PROGRESS BAR (G10):**
   - Add progress bar UI element to CraftScreen prefab
   - In `CraftScreen.Update()`:
     ```csharp
     // Real-time progress
     if (currentCraft != null) {
         float progress = (float)(currentCraft.CompletionTimeUnix - DateTimeOffset.UtcNow.ToUnixTimeSeconds())
             / (float)currentCraft.TotalDurationSeconds;
         // bound [0, 1]
         craftProgressSlider.value = Mathf.Clamp01(1f - progress);
     }
     ```

### T3-2: Merchant System Verification

1. Confirm `MerchantScreen` opens from HUD
2. Map full merchant flow:
   - `MerchantScreen.Show()` → `MerchantService.GetRegularStock()`, `MerchantService.GetSpecialStock()`
   - `MerchantScreen.BuyOffer(stockItemIndex)` → `MerchantService.BuyOffer()`:
     - Deducts gold from SaveData.Money
     - Adds item to InventoryService
     - Removes offer from stock
   - `MerchantScreen.SellItem(instanceId)` → `MerchantService.SellItem()`:
     - Removes item from InventoryService
     - Sets sell listing with expiry timestamp
     - `SaveData.MarketListings` updated
   - `MerchantScreen.ClaimSoldItem()` → `MerchantService.ClaimSoldItem()`:
     - Checks if buyer found (timer expired)
     - Adds gold to SaveData.Money
     - Removes listing
3. **Fix MARKET REFRESH TIMER (G11):**
   - Add countdown text to MerchantScreen
   - In `MerchantScreen.Update()`:
     ```csharp
     long remaining = SaveData.NextMarketRefreshUnix - now;
     marketTimerText.text = remaining > 0 
         ? $"Market refreshes: {FormatTime(remaining)}" 
         : "Market refreshed!";
     ```

### T3-3: Shop Verification (if separate from Merchant)

1. Confirm shop screen if exists
2. Map gem store (if exists): buying gold/gems with real currency (placeholders)

### T3-4: Economy Balance Verification

1. Confirm `FormulaService` pricing for:
   - Tavern recruit cost
   - Merchant buy/sell prices
   - Craft material costs
2. Confirm gold/gems flow:
   - Income sources: Quest rewards, Merchant sells, Dungeon loot
   - Expense sinks: Tavern, Craft materials, Merchant buys, Ascension/Promotion costs

---

## Verification Gate — RESTORE_3 PASS Criteria

| Check | Method | Status |
|-------|--------|--------|
| CraftScreen opens from HUD | NOT_RUN | GATE |
| TryStartCraft → deduct materials → queue | STATIC_TRACE_CONFIRMED | ⬜ |
| ClaimCompletedCraft → AddItem → remove from queue | STATIC_TRACE_CONFIRMED | ⬜ |
| Craft progress bar visible + updating | NOT_RUN (needs editor) | GATE |
| MerchantScreen opens from HUD | NOT_RUN | GATE |
| BuyOffer → deduct gold → add item → remove stock | STATIC_TRACE_CONFIRMED | ⬜ |
| SellItem → remove from inventory → listing | STATIC_TRACE_CONFIRMED | ⬜ |
| ClaimSoldItem → add gold → remove listing | STATIC_TRACE_CONFIRMED | ⬜ |
| Market refresh countdown visible | NOT_RUN (needs editor) | GATE |
| FormulaService costs traced for all callers | STATIC_TRACE_CONFIRMED | ⬜ |
| Gold/gems income + expense traced | STATIC_TRACE_CONFIRMED | ⬜ |
