# Headquarters Functional UI Audit

**Purpose:** Reference document for a UI artist/designer redesigning the visual appearance of the Headquarters screen. This is a **read-only functional audit** — no code, prefab, or scene was modified to produce it. Every existing action, button, and data field is documented so 100% of current gameplay functionality can be preserved through the redesign.

**Scope:** Top HUD, and the 6 Headquarters buildings — Quarters, Tavern, Storage, Market, Workshop, Shelter.

---

## Screen Overview

### Root panel structure

- **AppShellController** (`Assets/_Game/Scripts/Runtime/UI/Shell/AppShellController.cs`) is the top-level shell for the whole game screen, not just Headquarters. It owns:
  - The **Top HUD** (title + currency readouts + menu button).
  - **4 main tabs**: Headquarters, Adventurers, Dungeons, Raids (`_tabButtons` / `_tabButtonIcons` / `_tabPanels`).
  - A **drawer** (hamburger menu) with placeholder items.
  - A single-slot **popup root** (`OpenPopup` / `ClosePopup`) — only one popup/dialog can be open at a time, backed by a dimming backdrop that blocks input to the screen behind it.
- **HeadquartersHubController** (`Assets/_Game/Scripts/Runtime/UI/Shell/HeadquartersHubController.cs`) lives inside the Headquarters tab panel. It owns:
  - A vertically scrolling list of 6 **BuildingCardView** cards (Quarters, Tavern, Storage, Market, Workshop, Shelter), built by `HeadquartersHubBuilder.cs`.
  - References to all 6 building dialog prefabs, instantiated into the shell's popup root when a card is tapped.

### Top HUD

**Resources displayed** (`AppShellController.cs:24-30`, refreshed by `RefreshHud()` at `AppShellController.cs:169-182`):
- Gems (`_gemsText`) — from `Save.CurrentData.Gems`.
- Platinum, Gold, Silver, Copper (`_platinumText/_goldText/_silverText/_copperText`) — from `Save.CurrentData.Money`, converted via `LegacyCurrencyAdapter.FromMoney`.
- Screen title text (`_screenTitleText`) — updates per active tab.

**Buttons:**
- **Menu (hamburger) button** → opens the drawer (`OpenDrawer`, `AppShellController.cs:222-225`).
- **4 tab buttons** (Headquarters / Adventurers / Dungeons / Raids) → `SwitchTab` (`AppShellController.cs:197-218`); active tab icon tints `LegacyUITheme.BrassBorder`, inactive tint `DimWhite`.

**Navigation:** Tab bar is the only top-level navigation; Headquarters buildings are reached only via the building card list (no shortcut from HUD).

**Hidden interactions:** The **drawer** items — Shop, Settings, Recall, Messages, FAQ, Bestiary, Achievements, Cloud, Redeem, Community — exist visually but are **explicitly inert placeholders** (`AppShellController.cs:86-97`, comment confirms "no dialogs exist yet"). They render but do nothing when tapped. This is a pre-existing, intentional stub — do not treat as a bug to silently drop in redesign; flag as future scope.

---

## Building Audit

For each building: current visual composition, then current interaction, then backend connection.

### Building: Quarters

**Visual (Hub card):** Icon `sign_quarters`, Title "Quarters", Status `"{current}/{cap}"` where current = `Character.GetAllCharacters().Count`, cap = `Tavern.GetQuartersCapacity()`. (`HeadquartersHubController.cs:64-69`)

**Visual (Dialog)** (`QuartersDialog.cs`, built by `QuartersDialogBuilder.cs`):
- Title "Quarters"
- "Capacity: {n}" text
- Benefit description text: "Increases the maximum number of adventurers you can recruit."
- Currency cost display (`LegacyCurrencyView` — platinum/gold/silver/copper icons)
- Upgrade button (label toggles "UPGRADE" ↔ "MAX"; text renders red when unaffordable)
- Close button

**Interaction:**
- **Upgrade** → `OnUpgradeClicked()` (`QuartersDialog.cs:98-117`) → `ITavernService.UpgradeQuarters()`
- **Close** → closes dialog, returns to hub

**Backend:**
- `ITavernService.UpgradeQuarters()`
- `ITavernService.GetQuartersCapacity()`
- `ITavernService.GetUpgradeQuartersPrice()`
- `Save.CurrentData.Money` (affordability check)
- Max-level sentinel: price ≥ 99999999999999

---

### Building: Tavern

**Visual (Hub card):** Icon `sign_tavern`, Title "Tavern", Status `"{current}/{cap}"` where current = `Tavern.GetGuests().Count`, cap = `Tavern.GetTavernCapacity()`. (`HeadquartersHubController.cs:71-76`)

**Visual (Dialog)** (`TavernDialog.cs`, built by `TavernDialogBuilder.cs`):
- Title "Tavern"
- Summary row: "Guests {n}/{cap}", "Quarters {owned}/{quartersCap}"
- Timer text: "Tavern full" / "Next visitor in {timer}" / "Visitor arriving soon"
- **Guest Capacity** upgrade row: "Guest Capacity — Level {n} • Next cost {price} gold" (or MAX), with its own Upgrade button
- **Visitor Speed** upgrade row: "Visitor Speed — Level {n} • Next cost {price} gold" (or MAX), with its own Upgrade button
- Scrollable guest list; each guest card shows: portrait, Name, "Class: {name}", "Level {n}", "Traits: {list}", "Starting weapon: {id}", "Recruit: Free", RECRUIT button
- Close button

**Interaction:**
- **Guest Capacity Upgrade** → `ITavernService.UpgradeTavernCapacity()`
- **Visitor Speed Upgrade** → `ITavernService.UpgradeTavernTime()`
- **RECRUIT** (per guest card) → gated by `ITavernService.CanRecruit()`, then `ITavernService.RecruitGuest(index, out newCharacter)`; also triggers a refresh of the Adventurers tab
- **Close** → closes dialog

**Backend:**
- `ITavernService.GetGuests()`, `.GetTavernCapacity()`, `.GetQuartersCapacity()`
- `.GetNextVisitorTimerSeconds()`
- `.GetUpgradeTavernCapacityPrice()`, `.GetTavernCapacityLevel()`
- `.GetUpgradeTavernTimePrice()`, `.GetTavernTimeLevel()`
- `ICharacterService.GetAllCharacters()`
- `IInventoryService.GetItem` (for starter weapon display name)
- `CanRecruit()`, `RecruitGuest()`

---

### Building: Storage

**Visual (Hub card):** Icon `sign_storage`, Title "Storage", Status `"{current}/{cap}"` where current = `Inventory.GetAllItems().Count`, cap = `Inventory.GetCapacity()`. (`HeadquartersHubController.cs:78-83`)

**Visual (Dialog)** (`StorageDialog.cs`, built by `StorageDialogBuilder.cs`):
- Title "Storage"
- "Available: {n} / {cap} • Equipped: {n}" text
- Capacity upgrade row: "Capacity upgrade — Level {n} • Next cost: {price}g" (or MAX), with Upgrade button
- 5-column item grid; each slot shows: icon, quantity badge "x{n}" if stack > 1, ownership label "A:{available} E:{equipped}"
- **Item Detail overlay** (`StorageItemDetailPanel.cs`): Icon, Name, Info block (type/category, quantity, availability, stat summary, Price/Rarity if set, equipped-owner + slot, lock icon), hint text, action buttons, Close button

**Interaction:**
- **Capacity Upgrade** → `IInventoryService.UpgradeStorageCapacity()`
- **Item slot click** → opens Item Detail overlay for that item
- **Unequip** (Item Detail, only for Weapon/Armor/Accessory and only when actually equipped) → `IEquipmentService.Unequip(owner, slot)`
- **Use** (Item Detail, only for Consumable category) → `IInventoryService.UseConsumable(instanceId, null)` — note: target character is always `null` here, i.e. no character picker is offered from this screen
- **Sell** (Item Detail, disabled if `NotSellable` or item locked) → `IMerchantService.SellItem(definitionId, stackCount)`
- **Close** → closes overlay / dialog

**Backend:**
- `IInventoryService.GetAllItems()`, `.GetCapacity()`, `.UpgradeStorageCapacity()`, `.UseConsumable()`
- `IEquipmentService.Unequip()`
- `IMerchantService.SellItem()`
- `ItemDefinition.GetStatSummary()`

---

### Building: Market

**Visual (Hub card):** Icon `sign_market`, Title "Market", Status `"Selling {n} • Sold {n}"` from `Merchant.GetMarketListings().Count` / `.GetSoldMarketItems().Count`. (`HeadquartersHubController.cs:85-92`)

**Visual (Dialog)** (`MarketDialog.cs`, built by `MarketDialogBuilder.cs`; auto-refreshes every 1s):
- Title "Market"
- **Listing Capacity** upgrade row: "Listing capacity • Level {n} • Next: {price}g" (or MAX), Upgrade button
- **Market Speed** upgrade row: "Market speed • Level {n} • Next: {price}g" (or MAX), Upgrade button
- **Selling** section (divider): one row per active/queued listing — "Selling... {s}s (est. {payout}g)" with progress bar for the active (index 0) item, "Waiting... (est. {payout}g)" for queued, Cancel button on each
- **Sold** section (divider): one row per sold item — "Payout: {n}g", Claim button
- **Buy** section (divider): one row per regular/special stock offer — "Price: {n}g/gem", Buy button (disabled if unaffordable or inventory full)
- Close button

**Interaction:**
- **Listing Capacity Upgrade** → `IMerchantService.UpgradeMarketListings()`
- **Market Speed Upgrade** → `IMerchantService.UpgradeMarketTime()`
- **Cancel** (per selling item) → `IMerchantService.CancelListing(instanceId)`
- **Claim** (per sold item) → `IMerchantService.ClaimSoldItem(instanceId)`
- **Buy** (per stock offer) → `IMerchantService.BuyOffer(offer, isSpecial)`
- **Close** → closes dialog

**Backend:**
- `IMerchantService.GetMarketListings()`, `.GetSoldMarketItems()`, `.GetRegularStock()`, `.GetSpecialStock()`
- `.GetSellDurationSeconds()`, `.GetMarketListingsLevel()`, `.GetUpgradeMarketListingsPrice()`
- `.GetMarketTimeLevel()`, `.GetUpgradeMarketTimePrice()`
- `IDatabaseService.TryGet<ItemDefinition>` (for payout price display)
- **Note:** payout amount shown on "Selling"/"Sold" rows is computed client-side by `ComputeExpectedPayout` (`MarketDialog.cs:246-254`), duplicating server-side pricing logic rather than reading it from a service call — a fragile coupling flagged in code comments, not a UI concern but relevant if the redesign changes how/where payout is displayed.

---

### Building: Workshop

**Visual (Hub card):** Icon `sign_workshop`, Title "Workshop", Status `"{current}/{cap}"` from `Craft.GetQueue().Count` / `Craft.GetQueueCapacity()`. (`HeadquartersHubController.cs:94-99`)

**Visual (Dialog)** (`WorkshopDialog.cs`, built by `WorkshopDialogBuilder.cs`; auto-refreshes every 0.5s):
- Title "Workshop"
- Queue count text: "{n} / {cap}"
- **Queue Capacity** upgrade row: "Queue capacity • Level {n} • Next: {price}g" (or MAX), Upgrade button
- **Craft Speed** upgrade row: "Craft speed • Level {n} • Next: {price}g" (or MAX), Upgrade button
- Unified queue/completed list: active item (index 0) shows "Crafting... {s}s" + progress bar; queued items show "Waiting..." with a Cancel button; completed items show "Ready!" with a Collect button
- **Recipes** button → opens Recipe overlay
- Close button

**Recipe overlay** (`WorkshopRecipePanel.cs`, built inline in `WorkshopDialogBuilder.cs`):
- Title "Recipes"
- "Show Craftable Only" toggle checkbox
- Sections: "AVAILABLE RECIPES" / "UNAVAILABLE RECIPES" (or "CRAFTABLE ONLY" when toggled)
- Per-recipe row: output item icon + name, status checklist (ingredient owned/required counts, colored red when short) or failure reason ("Missing materials" / "Queue Full" / "Unavailable"), Craft button (interactable only when `CanCraft` passes)

**Interaction:**
- **Queue Capacity Upgrade** → `ICraftService.UpgradeQueueCapacity()`
- **Craft Speed Upgrade** → `ICraftService.UpgradeCraftSpeed()`
- **Collect** (per completed item) → `ICraftService.ClaimCompletedCraft(instanceId)`
- **Cancel** (per queued item) → `ICraftService.CancelCraft(instanceId)`, followed by an explicit `Save.Save()` call (comment notes the service mutates in-memory only)
- **Recipes** → opens Recipe overlay
- **Craft** (per recipe, in overlay) → `ICraftService.TryStartCraft(recipeId)`
- **Craftable-only toggle** → client-side list filter only
- **Close** (dialog and overlay) → closes

**Backend:**
- `ICraftService.GetQueue()`, `.GetQueueCapacity()`, `.UpgradeQueueCapacity()`, `.UpgradeCraftSpeed()`
- `.ClaimCompletedCraft()`, `.CancelCraft()`, `.TryStartCraft()`
- **Note:** `ICraftService.GetMaxCraftable(recipeId)` exists on the backend but is never called — there is currently no batch-craft / quantity-picker UI; crafting always produces `outputStack = 1` per action (confirmed by code comment in `WorkshopRecipePanel.cs`). Not a redesign concern unless batch crafting is added later.

---

### Building: Shelter

**Visual (Hub card):** Icon `sign_shelter`, Title "Shelter", Status `"{n} pet(s)"` — **no capacity denominator is shown** because `IPetService` currently has no capacity getter (confirmed in code comments). (`HeadquartersHubController.cs:101-106`)

**Visual (Dialog)** (`ShelterDialog.cs`, built by `ShelterDialogBuilder.cs`):
- Title "Shelter"
- Count text: "{n} pet(s) {n} egg(s)"
- 5-column grid mixing:
  - **Pet slots**: icon, Name (with "★ " prefix if Favourite), "Lv.{n} Assigned/Unassigned"
  - **Egg slots**: icon, "HATCH x{n}" label; tapping directly triggers hatch

**Pet Detail overlay** (`PetDetailPanel.cs`, built inline in `ShelterDialogBuilder.cs`):
- Icon, Name
- Info block: Level, "Food: {n}/{toNextLevel}", Favourite Yes/No, Assigned dungeon, Family/Tier, Abilities list, EXP/Drop expedition bonus %
- Dynamically built action buttons (only those relevant to current pet state are shown):
  - **FAVOURITE / UNFAVOURITE** (toggle)
  - **UNASSIGN FROM DUNGEON** (only if currently assigned)
  - **ASSIGN: {dungeon name}** — one button per unlocked dungeon (only if unassigned)
  - **FEED {item} x1 (+{n} food)** — or disabled "FEED: NO FOOD AVAILABLE" if no feed item owned
  - **RELEASE PET** — opens a confirmation sub-dialog before executing; shows failure text "Cannot release an active expedition companion" if release is blocked
  - Close

**Interaction:**
- **Egg tap** → `IPetService.HatchEgg(eggDefinitionId)`
- **FAVOURITE/UNFAVOURITE** → `IPetService.SetFavourite(instanceId, bool)`
- **UNASSIGN FROM DUNGEON** → `IPetService.UnassignFromDungeon(instanceId)`
- **ASSIGN: {dungeon}** → `IPetService.AssignToDungeon(instanceId, dungeonId)`, gated by `IDungeonService.IsDungeonUnlocked`
- **FEED** → `IPetService.FeedWithItem(petInstanceId, itemInstanceId, 1)`
- **RELEASE PET** → confirmation, then `IPetService.ReleasePet(instanceId)`
- **Close** → closes overlay / dialog

**Backend:**
- `IPetService.HatchEgg()`, `.SetFavourite()`, `.UnassignFromDungeon()`, `.AssignToDungeon()`, `.FeedWithItem()`, `.ReleasePet()`
- `IDungeonService.IsDungeonUnlocked()`
- **Note:** Several `PetDefinition` fields (BaseAttack/Defense/MaxHp/Speed, multipliers, SkillDefinitionId, EvolutionDefinitionId/Level) are supported by the model but never populated by current JSON data (`pets.json` only supplies `id`), so those fields never render regardless of UI design — not a UI gap, a data-content gap.

---

## Button / Action Inventory

| Button | Location | Backend Method / Service | Keep for redesign |
|---|---|---|---|
| Menu (hamburger) | Top HUD | Opens drawer (`AppShellController.OpenDrawer`) | YES |
| Tab: Headquarters/Adventurers/Dungeons/Raids | Top HUD | `AppShellController.SwitchTab` | YES |
| Drawer items (Shop, Settings, Recall, Messages, FAQ, Bestiary, Achievements, Cloud, Redeem, Community) | Drawer | None — inert placeholders | YES (preserve as future-scope placeholders; do not delete) |
| Building card tap ×6 | Headquarters hub list | `HeadquartersHubController.OpenBuildingPopup(featureId)` | YES |
| Upgrade (Quarters) | Quarters dialog | `ITavernService.UpgradeQuarters()` | YES |
| Close (Quarters) | Quarters dialog | Dialog close callback | YES |
| Guest Capacity Upgrade | Tavern dialog | `ITavernService.UpgradeTavernCapacity()` | YES |
| Visitor Speed Upgrade | Tavern dialog | `ITavernService.UpgradeTavernTime()` | YES |
| RECRUIT (per guest) | Tavern dialog | `ITavernService.RecruitGuest()` (gated by `CanRecruit()`) | YES |
| Close (Tavern) | Tavern dialog | Dialog close callback | YES |
| Capacity Upgrade (Storage) | Storage dialog | `IInventoryService.UpgradeStorageCapacity()` | YES |
| Item slot tap | Storage dialog grid | Opens Item Detail overlay | YES |
| Unequip | Storage → Item Detail | `IEquipmentService.Unequip()` | YES |
| Use | Storage → Item Detail | `IInventoryService.UseConsumable()` | YES |
| Sell | Storage → Item Detail | `IMerchantService.SellItem()` | YES |
| Close (Storage / Item Detail) | Storage dialog | Dialog/overlay close callback | YES |
| Listing Capacity Upgrade | Market dialog | `IMerchantService.UpgradeMarketListings()` | YES |
| Market Speed Upgrade | Market dialog | `IMerchantService.UpgradeMarketTime()` | YES |
| Cancel (per selling item) | Market dialog | `IMerchantService.CancelListing()` | YES |
| Claim (per sold item) | Market dialog | `IMerchantService.ClaimSoldItem()` | YES |
| Buy (per stock offer) | Market dialog | `IMerchantService.BuyOffer()` | YES |
| Close (Market) | Market dialog | Dialog close callback | YES |
| Queue Capacity Upgrade | Workshop dialog | `ICraftService.UpgradeQueueCapacity()` | YES |
| Craft Speed Upgrade | Workshop dialog | `ICraftService.UpgradeCraftSpeed()` | YES |
| Collect (per completed craft) | Workshop dialog | `ICraftService.ClaimCompletedCraft()` | YES |
| Cancel (per queued craft) | Workshop dialog | `ICraftService.CancelCraft()` + `Save.Save()` | YES |
| Recipes | Workshop dialog | Opens Recipe overlay | YES |
| Craft (per recipe) | Workshop → Recipe overlay | `ICraftService.TryStartCraft()` | YES |
| Show Craftable Only (toggle) | Workshop → Recipe overlay | Client-side filter only | YES |
| Close (Workshop / Recipes) | Workshop dialog | Dialog/overlay close callback | YES |
| Egg tap (hatch) | Shelter dialog grid | `IPetService.HatchEgg()` | YES |
| Pet slot tap | Shelter dialog grid | Opens Pet Detail overlay | YES |
| FAVOURITE / UNFAVOURITE | Shelter → Pet Detail | `IPetService.SetFavourite()` | YES |
| UNASSIGN FROM DUNGEON | Shelter → Pet Detail | `IPetService.UnassignFromDungeon()` | YES |
| ASSIGN: {dungeon} (per unlocked dungeon) | Shelter → Pet Detail | `IPetService.AssignToDungeon()` | YES |
| FEED {item} x1 | Shelter → Pet Detail | `IPetService.FeedWithItem()` | YES |
| RELEASE PET | Shelter → Pet Detail | Confirmation → `IPetService.ReleasePet()` | YES |
| Close (Shelter / Pet Detail) | Shelter dialog | Dialog/overlay close callback | YES |

---

## Backend Connection Map

| Building | Services used |
|---|---|
| Top HUD | `Save.CurrentData` (Gems, Money), `LegacyCurrencyAdapter` |
| Quarters | `ITavernService` (`UpgradeQuarters`, `GetQuartersCapacity`, `GetUpgradeQuartersPrice`, `GetQuartersLevel`*) |
| Tavern | `ITavernService` (guests, capacity, timer, recruit, capacity/speed upgrades), `ICharacterService.GetAllCharacters`, `IInventoryService.GetItem` |
| Storage | `IInventoryService` (items, capacity, upgrade, use consumable, lock*), `IEquipmentService` (unequip, equip*), `IMerchantService.SellItem` |
| Market | `IMerchantService` (listings, sold items, stock offers, capacity/speed upgrades, cancel/claim/buy), `IDatabaseService.TryGet<ItemDefinition>` |
| Workshop | `ICraftService` (queue, capacity/speed upgrades, claim, cancel, start craft, max-craftable*) |
| Shelter | `IPetService` (hatch, favourite, assign/unassign, feed, release, create*, add exp*), `IDungeonService.IsDungeonUnlocked` |

`*` = backend method exists but currently has **no UI entry point** — see next section.

---

## Missing UI Exposure

Backend capabilities that exist but are **not currently reachable from any Headquarters UI**. These are informational only — **do not add UI for these as part of the visual redesign**; they are flagged for a separate scope decision.

1. **`ITavernService.GetQuartersLevel()`** — Quarters dialog shows capacity and upgrade price, but never displays the raw upgrade level number.
2. **`IInventoryService.ToggleLockItem`** — item lock state is *displayed* (🔒 icon) and *gates* the Sell button in Storage's Item Detail panel, but there is no Lock/Unlock button anywhere to actually change it.
3. **`IEquipmentService.Equip` / `CanEquip`** — Storage's Item Detail panel only offers Unequip; equipping an item is only reachable from the separate Adventurers/Characters screen, not from Headquarters.
4. **`IMerchantService.GetMarketListingsCapacity()`** — declared but never called; Market dialog only shows level/next-upgrade-price, not the raw current capacity number.
5. **Selling an item is only reachable via Storage → Item Detail → Sell.** There is no "list an item for sale" action inside the Market dialog itself, even though Market is where listings are managed/viewed.
6. **`ICraftService.GetMaxCraftable(recipeId)`** — no batch/quantity picker exists for crafting; every craft action produces exactly 1 output.
7. **`IPetService.CreatePet(definitionId, ownerCharacterId)`** — pets can only enter the game via `HatchEgg`; there is no "create/summon pet" UI path.
8. **`IPetService.AddExp` / raw XP progress** — Pet Detail only shows expedition bonus %, not a raw XP progress bar toward next level.
9. **Shelter capacity** — no capacity number or cap is shown anywhere for Shelter (backend has no getter for it either — this is a backend gap, not just a UI gap).
10. Several `PetDefinition` stat fields (BaseAttack/Defense/MaxHp/Speed, multipliers, skill/evolution IDs) are modeled but never populated by current data files, so no UI currently renders them — a data-content gap, not fixable by UI redesign alone.

---

## Data Visibility Audit

### Top HUD
**Shown:** Gems, Platinum, Gold, Silver, Copper, active tab title.
**Missing but useful:** none identified — HUD is minimal by design.

### Quarters
**Shown:** Capacity, upgrade cost, affordability (color).
**Missing but useful:** Current upgrade level number (`GetQuartersLevel()` exists but unused).

### Tavern
**Shown:** Guest count/capacity, Quarters count/capacity, next-visitor timer, Guest Capacity level + cost, Visitor Speed level + cost, per-guest portrait/name/class/level/traits/starting weapon.
**Missing but useful:** none beyond what's listed in Missing UI Exposure.

### Storage
**Shown:** Available/capacity, equipped count, capacity upgrade level + cost, per-item icon/quantity/availability, item detail (type, quantity, stat summary, price/rarity if set, equipped owner+slot, lock state).
**Missing but useful:** No way to *act* on the lock state shown (see #2 above).

### Market
**Shown:** Listing capacity level + cost, market speed level + cost, per-listing status/timer/progress/expected payout, per-sold-item payout, per-offer price.
**Missing but useful:** Raw listing capacity number (`GetMarketListingsCapacity()` unused).

### Workshop
**Shown:** Queue count/capacity, queue capacity level + cost, craft speed level + cost, per-item crafting/waiting/ready status with progress bar, per-recipe ingredient checklist and craftability reason.
**Missing but useful:** none beyond batch-craft quantity (explicitly out of scope — backend hard-codes stack of 1).

### Shelter
**Shown:** Pet count, egg count, per-pet name/favourite/level/assignment, pet detail (level, food progress, favourite, assigned dungeon, family/tier, abilities, expedition bonus %).
**Missing but useful:** Capacity/cap number (backend gap, not a UI-only fix); raw XP progress bar (backend has `AddExp` but only bonus % is surfaced).

---

## UI Problems

Presentation-only issues observed — no gameplay/economy/system changes implied.

1. **Dead "NEW" badge.** Every building card has a "NEW" indicator element built into the prefab (`HeadquartersHubBuilder.cs`), but `HeadquartersHubController.RefreshCards()` always passes `showNew:false` — it is permanently inactive across all 6 cards today. Redesign can repurpose or remove this visual slot since it currently serves no function, but note it in case a future feature intends to use it.
2. **Inconsistent status text patterns across cards.** Most cards show `"{current}/{cap}"`, but Market shows `"Selling {n} • Sold {n}"` and Shelter shows only `"{n} pet(s)"` with no denominator — this inconsistency in information density/format across otherwise-identical card layouts may read as visually unbalanced or make it harder to scan capacity at a glance.
3. **Two-tier upgrade rows (Tavern, Market, Workshop) read as similar but separate blocks** — Guest Capacity vs Visitor Speed, Listing Capacity vs Market Speed, Queue Capacity vs Craft Speed. Each pair currently has near-identical layout/wording ("X — Level {n} • Next cost {price}"), which is functionally fine but currently has no strong visual hierarchy distinguishing the two upgrade types from each other or signaling which is more impactful.
4. **Storage's item detail buttons (Unequip/Use/Sell) are conditionally shown/hidden per item category**, which can make the action row feel inconsistent in height/position across different items — a presentation issue worth smoothing in redesign (e.g., a fixed action-row footprint with clearly disabled-vs-hidden states) without changing which actions are available for which item types.
5. **Drawer items look tappable but are fully inert** — visually this can read as broken/unfinished to a player. A redesign could visually mark them as "coming soon" without adding functionality (functionality change is out of scope for this task; call this out for a product/PM decision separately).
6. **Confirmation flow only exists for Release Pet**; every other potentially costly action (upgrades, sells, cancels) executes immediately on tap. This is a purely presentational/UX-flow observation — not a suggestion to change economy or add friction, just noting the current asymmetry for the designer's awareness.
7. **Recipe overlay's craftable/uncraftable split plus the "Craftable Only" toggle** duplicate similar information in two different states (section headers vs. filtered single list) — worth a cleaner single visual pattern in redesign.

---

## UI Redesign Requirements

### Must keep (every existing function, unchanged):

- Top HUD: Gems/Platinum/Gold/Silver/Copper display, menu button → drawer, 4-tab navigation.
- Drawer: all 10 placeholder items present (even though inert).
- Headquarters hub: 6 building cards (Quarters, Tavern, Storage, Market, Workshop, Shelter), each opening its respective dialog.
- **Quarters:** capacity display, upgrade cost display, Upgrade button, Close button.
- **Tavern:** guest/quarters counts, visitor timer, Guest Capacity upgrade (level+cost+button), Visitor Speed upgrade (level+cost+button), per-guest card (portrait, name, class, level, traits, starting weapon, Recruit button), Close button.
- **Storage:** available/capacity/equipped counts, Capacity upgrade (level+cost+button), item grid (icon, quantity badge, ownership label), Item Detail overlay (icon, name, info block, Unequip/Use/Sell buttons as conditionally applicable, Close).
- **Market:** Listing Capacity upgrade, Market Speed upgrade, Selling section (status/timer/progress/Cancel), Sold section (payout/Claim), Buy section (price/Buy), Close.
- **Workshop:** queue count, Queue Capacity upgrade, Craft Speed upgrade, unified queue/completed list (status/progress/Collect/Cancel), Recipes button → Recipe overlay (Craftable Only toggle, available/unavailable sections, per-recipe ingredient checklist + Craft button), Close.
- **Shelter:** pet/egg counts, pet+egg grid, egg tap-to-hatch, Pet Detail overlay (icon, name, info block, Favourite toggle, Assign/Unassign, Feed, Release with confirmation, Close).
- All backend method calls listed in the Button/Action Inventory table must remain wired to equivalent UI controls after redesign — labels, icons, and layout may change, but the call sites and their trigger conditions (gating, disabled states) must be preserved.

### Should improve (visual only):

- Visual hierarchy and consistency of status text across building cards (item #2 above).
- Clearer visual distinction between the two upgrade types per building (item #3).
- Cleaner, more consistent action-row treatment in Storage's Item Detail overlay (item #4).
- Unify the craftable/uncraftable presentation pattern in the Workshop Recipe overlay (item #7).
- Overall navigation clarity between hub → dialog → nested overlay (Item Detail, Pet Detail, Recipe panel) so players always have a clear sense of "how many levels deep" they are and how to back out.
- Information hierarchy: make capacity/level/cost/status legible at a glance, especially in the two-tier upgrade rows.

### Must not change:

- Backend services (`ITavernService`, `IInventoryService`, `IEquipmentService`, `IMerchantService`, `ICraftService`, `IPetService`, `IDungeonService`, `ICharacterService`, `IDatabaseService`).
- `SaveData` / `Save.CurrentData` structure or save/load flow.
- Any gameplay flow, economy value, formula, or progression logic.
- Which actions are available for which item/pet/recipe states (conditional button visibility logic must be preserved, even if its visual presentation changes).
- The single-popup-at-a-time behavior and popup/overlay nesting model (Dialog → Item/Pet Detail/Recipe overlay), unless explicitly asked to redesign that interaction pattern.

---

*Report generated for UI redesign preparation. No source files, prefabs, or scenes were modified in producing this document.*
