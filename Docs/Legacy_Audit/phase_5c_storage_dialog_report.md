# Phase 5C — Storage Dialog report

Date: 2026-08-05 (revision 3 — CHANGES REQUESTED: restore full Item Detail functionality)

## Revision history

1. **Initial pass**: Storage dialog with 6 filter buttons + 5 sort buttons (matching legacy `dialog_storage`'s `radioGroup_visibility`/`radioGroup_order_by`), and a name+quantity-only Item Detail placeholder.
2. **Revision 2 (simplification)**: filter and sort UI/logic removed entirely per request, keeping the dialog to Title/Capacity/Grid/Close. The Item Detail placeholder was left as-is (name + quantity only).
3. **Revision 3 (this one)**: the user clarified the simplification request was about filter/sort only — **Item Detail must not be cut down** if the backend already supports more. This revision keeps the Storage dialog exactly as simplified in revision 2, and replaces the name+quantity-only placeholder with a real Item Detail view/action panel backed by the real `IEquipmentService`, `IInventoryService`, and `IMerchantService` APIs.

This report describes the current (revision 3) implementation. Sections describing removed filter/sort work are kept for historical record only.

## Scope

Storage dialog + Item Detail only. Quarters, Tavern, Workshop, Shelter, Market, Adventurers, Dungeons, Raids, App Shell, Headquarters Hub layout, services, models, SaveData, and formulas were not redesigned or changed. `HeadquartersHubController`'s Storage branch is unchanged since revision 1 (same `Setup(services, onClose, onStateChanged)` call).

Current dialog layout:
- Title ("Storage")
- Capacity (`{ownedCount} / {capacity}`) directly under the title
- 5-column item grid showing the full `GetAllItems()` list (icon + quantity), no filter/sort
- Click an item → **real Item Detail overlay** (icon, name, category/type, quantity, stats, equipped state, contextual hint) with real Equip-adjacent/Use/Sell actions where the backend clearly supports them
- Close button

## API audit — source of truth (this revision)

Audited (read-only, no edits) specifically for Item Detail:

- `Assets/_Game/Scripts/Runtime/Services/IInventoryService.cs`, `InventoryService.cs` (already audited in rev. 1/2)
- `Assets/_Game/Scripts/Runtime/Services/IEquipmentService.cs`, `EquipmentService.cs`
- `Assets/_Game/Scripts/Runtime/Services/IMerchantService.cs`, `MerchantService.cs`
- `Assets/_Game/Scripts/Runtime/Services/ICharacterService.cs`, `CharacterService.cs`
- `Assets/_Game/Scripts/Runtime/Models/CharacterRuntime.cs`, `MerchantResult.cs`
- `Assets/_Game/Scripts/Runtime/UI/Inventory/InventoryScreen.cs` (legacy S6 screen — precedent for "equip via Characters screen" hint, and for the Sell/lock gating pattern)
- `Assets/_Game/Scripts/Runtime/Services/ServiceContainer.cs` (confirmed property names: `.Inventory`, `.Equipment`, `.Merchant`, `.Character`, `.Save`)
- `Assets/_Game/Scripts/Database/DatabaseBuilder.cs`, `ItemFieldsLoader.cs` (traced exactly which `ItemDefinition` fields the data pipeline actually populates)
- `Assets/StreamingAssets/GameData/items.json` (raw data, to confirm which JSON keys exist and how they're cased)

### Equip / Unequip — `IEquipmentService`

```csharp
bool CanEquip(CharacterRuntime character, ItemRuntime item, EquipmentSlot slot);
bool Equip(CharacterRuntime character, string itemInstanceId, EquipmentSlot slot);
bool Unequip(CharacterRuntime character, EquipmentSlot slot);
```

- `Equip` requires a target `CharacterRuntime`. There is **no character-selection mechanism reachable from Storage/Item Detail** — "selected character" is a UI-only concept owned by the legacy `CharacterScreen` (`GetSelectedCharacter()`), which `HeadquartersHubController`/`StorageDialog` have no reference to. Building a character picker here would be a new UI flow, not a real-backend-driven action, so it was **not built** — matching the existing precedent in `InventoryScreen`, which already shows "Equip this via the Characters screen." for the same reason. This hint text is shown for any unequipped Weapon/Armor/Accessory item.
- `Unequip` only needs the *current* owner + slot, both of which are unambiguous and derivable from real data: `ICharacterService.GetAllCharacters()` is scanned for a `CharacterRuntime` whose `.Weapon`/`.Armor`/`.Accessory` instance id matches the clicked item. This required no invented selection UI, so **Unequip is wired as a real action.**
- `EquipmentService.Equip`/`Unequip` also maintain `ItemSaveData.IsLocked` as a byproduct (an item is locked while equipped) — confirmed by reading `EquipmentService.SyncSave`. The panel respects this: locked+equipped items are not offered Sell.

### Use (consumable) — `IInventoryService.UseConsumable`

```csharp
bool UseConsumable(string instanceId, CharacterRuntime targetCharacter);
```

- Gates on `item.Definition.Category == ItemCategory.Consumable` — the same check `InventoryService.UseConsumable` itself uses internally. (Note: the legacy `InventoryScreen` instead checked `ItemDefinition.Consumable`, a bool field that — see "Rarity/Price finding" below — is never populated by the data pipeline, so that old gate was permanently `false`. This revision uses `Category` instead, which the pipeline does populate correctly.)
- `targetCharacter` is optional in the real implementation (`if (targetCharacter != null) { heal }`) — with no character-selection available in Storage's context, **Use is wired to call `UseConsumable(instanceId, null)`**, a real, already-supported call path: the item is still consumed via the real `RemoveItem` call inside it, just without a heal target. This is not an invented behavior; it is the method's own null-safe branch.

### Sell — `IMerchantService.SellItem`

```csharp
MerchantResult SellItem(string definitionId, int stackCount);
```

- Real, unambiguous API. Gated on `!ItemDefinition.NotSellable && !ItemRuntime.IsLocked`.
- **Important real behavior, not obvious from the signature**: `SellItem` does not pay out immediately. It calls `ConsumeByDefinitionId` (removing the item now) and enqueues a `MarketListings` entry; `GameLoopService` ticks `MerchantService.ProgressMarket` each frame, and after `DEFAULT_SELL_TIME_SECONDS` (20s) the entry moves to `SoldMarketItems`, payable via `ClaimSoldItem` — which is Market UI, not built anywhere in this project yet. **Selling from Storage's Item Detail removes the item and starts this real timer, but there is currently no UI to see or claim the payout.** This is disclosed, not hidden — see "Known limitations."
- `ConsumeByDefinitionId` consumes by **definition id**, not instance id — if multiple separate item instances share the same definition id (only possible for non-stackable Weapon/Armor/Accessory categories, since Material/Consumable always merge into one instance), Sell could in theory remove stock from a different instance than the one clicked. This is an existing backend limitation (no instance-specific sell API), not something introduced here.

### Equipped-state lookup

No single API answers "is this item equipped, and by whom" — it's derived by scanning `ICharacterService.GetAllCharacters()` for a `CharacterRuntime.Weapon/.Armor/.Accessory` matching the item's `InstanceId`. This is real, live data (not SaveData directly), consistent with how `EquipmentService` itself tracks equip state.

### Critical finding: `ItemDefinition.Price` and `ItemDefinition.Rarity` are never populated

Tracing `DatabaseBuilder.LoadCategory<T>` → `EnrichItemDefinition` (sets only `Category`/`ItemType`) → `ItemFieldsLoader.Apply` (sets only `Constitution/Dexterity/Intelligence/Defense/MagicDefense/MaxHp`, via a typed DTO with lowercase field names matching `items.json`'s nested `fields` object) confirms **no code path ever assigns `ItemDefinition.Price`, `.Rarity`, `.Consumable`, or `.NotSellable`.** These are set only by Unity's `JsonUtility.FromJson`, which requires an exact case-sensitive match between the JSON key and the C# field name — `items.json`'s top-level keys are `price`/`rarity` (lowercase), which do not match the C# fields `Price`/`Rarity` (PascalCase). This is why the codebase needed dedicated enrichment classes for `Category`/`ItemType`/stats in the first place (their doc comments literally say "Unity's JsonUtility cannot infer X, so we derive them here").

**Verified empirically in Play Mode** (revision 2's Price-based sort and this revision's Item Detail both surfaced this): `Copper Sword`, `Cloth Robe`, and every other item in the current save show `Price: 0`, `Rarity: 0` — confirming the fields are always their C# default, for every item, in the live running game, not just in theory.

Consequence for Item Detail: Price and Rarity rows are only rendered when `> 0` / `!= 0` respectively — in practice this means **they never render with the current data pipeline**, which is the correct, honest behavior rather than showing "Price: 0" on every single item in the game as if that were meaningful data. `SellPrice` was already known (revision 1) to be unpopulated the same way; this finding extends that to `Price`, `Rarity`, `Consumable`, and `NotSellable`. None of these fields were modified — this is purely a UI-side conditional display decision reacting to real, observed data.

## Filter/sort (removed — unchanged since revision 2)

The category-mapping and sort-field research from revision 1 is preserved for reference in git history; it is not repeated here since it is unrelated to Item Detail. Filter/sort remain fully removed per the user's standing instruction (item 6 of this revision's request: "Không khôi phục: filter, sort, category empty state").

## Implementation

Created:

- `Assets/_Game/Scripts/Runtime/UI/Headquarters/StorageItemDetailPanel.cs` — **new**, the real Item Detail component. Reads `ItemRuntime`/`ItemDefinition` fields, resolves equipped-owner via `ICharacterService`, builds a real info block, and wires Unequip/Use/Sell to the real services described above. Every action re-reads the item via `IInventoryService.GetItem` afterward: if it still exists, the panel refreshes in place; if it's gone, the panel closes itself (never shows stale/invalid state).

Modified:

- `Assets/_Game/Scripts/Runtime/UI/Headquarters/StorageDialog.cs` — `_itemDetailPanel` is now typed `StorageItemDetailPanel` (was a bare `GameObject` + separate Text fields). `Setup()` wires the panel once (`onClose` hides it, `onItemChanged` triggers `StorageDialog.Refresh()` **and** the `onStateChanged` callback up to `HeadquartersHubController`, so the Headquarters card + HUD stay in sync with any action). Grid slot clicks now call `_itemDetailPanel.Show(item)` directly.
- `Assets/_Game/Scripts/Editor/UI/Legacy/StorageDialogBuilder.cs` — the overlay box now builds an icon `Image`, name `Text`, multi-line info `Text`, hint `Text`, a 3-button action row (Unequip/Use/Sell, each toggled active/interactable per-item by the panel itself), and a Close button, and attaches `StorageItemDetailPanel` (instead of the old 2-field placeholder) with all references wired via `SerializedObject`.
- `Assets/_Game/Scripts/Editor/UI/Legacy/AppShellScreenshotTool.cs` — `TestStorageDialogFlow` now: opens Storage → finds a Weapon-category item (captures `phase_5c_item_detail_weapon.png`) → finds an Armor-category item (captures `phase_5c_item_detail_armor.png`) → finds any item with a real, currently-interactable action button and triggers it (captures `phase_5c_item_detail_action.png`, logs before/after capacity) → closes Item Detail → closes Storage → verifies no orphan → regression-checks Quarters/Tavern. Screenshots use an extra `DelayFrames(3)` after `Capture()` before proceeding, matching the async-capture caveat already documented in `TavernDialogBuilder` (Phase 5B) — the first attempt at this test omitted that delay and captured the *next* step's frame instead of the intended one; this was caught and fixed during testing (see "Compile and test result").

No service, model, `SaveData`, or `FormulaService` file was modified in any revision.

## Item Detail — what's actually shown (real fields only)

- **Icon**: `LegacySpriteRegistry.GetItemSprite(Definition.id)` — same convention as the grid, ~98.2% accurate (see revision 1 finding on `idImage` vs `id` mismatches for 11 egg items).
- **Name**: `Definition.id` formatted to Title Case (no display-name field exists on `ItemDefinition`).
- **Category / Type**: `Definition.Category` + `Definition.ItemType` if present — both real and populated (`EnrichItemDefinition`).
- **Quantity**: `ItemRuntime.StackCount` — real.
- **Stats**: `Definition.GetStatSummary()` — an existing method on `ItemDefinition` itself, built from the real, populated `Constitution/Dexterity/Intelligence/Defense/MagicDefense/MaxHp` fields (e.g. "+3 CON, +1 DEX").
- **Price / Rarity**: shown only if non-zero — see "Critical finding" above; in the current data they never render.
- **Equipped state**: "Equipped by `{CharacterName}` (`{Slot}`)" or "Not equipped", for Weapon/Armor/Accessory only — derived from live `CharacterRuntime` equip references.
- **Locked indicator**: shown for locked-but-not-equipped items (a user-toggled lock is still possible via the legacy Inventory screen's `ToggleLockItem`, independent of equip-driven locking).
- **Description**: *not shown* — no description text field exists anywhere in the data (`items.json` only has an `idDescription` **localization key**, e.g. `"item_aberrant_fabric_description"`, not resolvable text). Omitted rather than showing a raw, meaningless key.

## Actions

| Action | Condition to show/enable | Real API call |
|---|---|---|
| Unequip | Item is Weapon/Armor/Accessory **and** currently equipped by some character | `IEquipmentService.Unequip(owner, slot)` |
| Use | `Category == Consumable` **and** not locked | `IInventoryService.UseConsumable(instanceId, null)` |
| Sell | `!NotSellable` **and** not locked | `IMerchantService.SellItem(Definition.id, StackCount)` |

Equip (from an unequipped state) is intentionally **not** a button — see "Equip / Unequip" audit note above. A hint label explains why ("Equip this via the Characters screen.") whenever an equippable item isn't currently equipped.

After every successful action: the panel re-fetches the item and refreshes itself (or closes if the item is gone), `StorageDialog.Refresh()` rebuilds the grid and capacity text, and `HeadquartersHubController`'s `RefreshCards()` + `AppShellController.RefreshHud()` run via the same `onStateChanged` callback Quarters/Tavern already use.

## Popup flow

Unchanged from revision 2: Item Detail is a child overlay of `StorageDialog` (`LayoutElement.ignoreLayout = true`, not a second `AppShellController.OpenPopup` call), so it can never conflict with the shell's single-popup guard, never orphans, and closing it always cleanly returns to the still-open Storage grid beneath. Closing Storage's own Close button destroys the whole dialog (including any open Item Detail) via `AppShellController.ClosePopup`.

## Compile and test result

- Unity batchmode recompile: **0 errors**, 0 new warnings (one intermediate compile had 9 `CS0104` ambiguous-`Object` errors from an unqualified `using System;` colliding with the file's existing bare `Object.Destroy`/`Object.FindFirstObjectByType` calls — fixed by qualifying the new test helpers' `Action` parameters as `System.Action` instead of adding the `using`).
- Prefab build (`StorageDialogBuilder`): completed successfully, Main scene re-wired, idempotent.
- Runtime full-flow test (`AppShellScreenshotTool.TestStorageDialogFlow`), fresh Play Mode, `Main.unity`, natural save (never hacked):
  - Storage opened: **8 / 36** items, 5-column grid confirmed.
  - Weapon item found and captured: `Copper Sword` — `Weapon • Sword | Quantity: x1 | Stats: +3 CON, +1 DEX | Not equipped`, hint "Equip this via the Characters screen.", Unequip correctly disabled, Sell enabled.
  - Armor item found and captured: `Cloth Robe` — `Armor • LightArmor | Quantity: x1 | Stats: +3 INT, +10 HP | Not equipped`, same hint/gating behavior.
  - Action test: found an item with an interactable action and triggered **Sell** (no unequipped/unlocked item had Unequip/Use available at that point) — capacity went `8 / 36` → `7 / 36`, the sold item disappeared from the grid immediately, Item Detail closed itself (item no longer exists — correct per the "close if gone" rule), all real-time via `IMerchantService.SellItem` → `ConsumeByDefinitionId` → grid refresh.
  - A separate run (before the async-screenshot-timing fix) also exercised **Unequip** successfully on an equipped `Copper Sword` (owner found via live `CharacterRuntime` scan, capacity correctly unchanged at `8/36` since unequip doesn't remove the item, only unlocks it) — logged as `Action 'Unequip' completed. Capacity after: 8 / 36.` This confirms both Unequip and Sell work against real data; which one gets exercised by the automated test depends on the save's current equip state.
  - Item Detail Close → back to Storage (no orphan). Storage Close → `IsPopupOpen=False`, `orphanExists=False`.
  - Regression: Quarters opened/closed normally; Tavern opened/closed normally.
- No new red console errors. The single pre-existing error (`Some objects were not cleaned up when closing the scene... Canvas / Main Camera`) predates all Phase 5C work (documented since Phase 5B) and is unrelated.

### Test coverage / not exercised

- Equip (from unequipped) has no button by design (see audit note), so there is nothing to test there.
- Only one action per test run is exercised (whichever the loop finds first — Unequip or Sell in the runs performed); Use was not hit in either run because the current save owns no Consumable-category items. The Use code path was verified by reading `InventoryService.UseConsumable`'s null-target branch directly rather than by a live click, since the save has no consumable to click.
- Sell's real payout (via the Market listing timer / `ClaimSoldItem`) was not observed end-to-end, since no Market UI exists to claim it — only the immediate removal-from-inventory side was verified.

## Screenshots

- [Storage dialog — grid only, no filter/sort UI](D:/Tinh/Rebuild_GuildMaster/Docs/Legacy_Audit/Asset_Gallery/phase_5c_storage.png)
- [Item Detail — Weapon (Copper Sword), real stats + hint + actions](D:/Tinh/Rebuild_GuildMaster/Docs/Legacy_Audit/Asset_Gallery/phase_5c_item_detail_weapon.png)
- [Item Detail — Armor (Cloth Robe), real stats + hint + actions](D:/Tinh/Rebuild_GuildMaster/Docs/Legacy_Audit/Asset_Gallery/phase_5c_item_detail_armor.png)
- [Item Detail action result — Sell (grid/capacity refreshed live)](D:/Tinh/Rebuild_GuildMaster/Docs/Legacy_Audit/Asset_Gallery/phase_5c_item_detail_action.png)

`phase_5c_storage_item_popup.png` (the old name+quantity placeholder screenshot from revision 2) has been deleted — superseded by the three real Item Detail screenshots above.

## Rollback

Restore the pre-Phase-5C files from:

`D:\Tinh\Backups\Legacy_UI_Phase_5C_Storage\`

(contains `Scripts_Before/` and `Scenes_Before/` — full snapshots of `Assets/_Game/Scripts` and `Assets/_Game/Scenes` taken before any Phase 5C edit). To roll back: restore those two folders over the current ones, delete `Assets/_Game/Prefabs/UI/Headquarters/StorageDialog.prefab` (+ `.meta`) and `Assets/_Game/Scripts/Runtime/UI/Headquarters/StorageItemDetailPanel.cs` (+ `.meta`), and recompile.

## Known limitations

- **Sell has no visible payout UI.** `IMerchantService.SellItem` is real but queues the item into a 20-second `MarketListings` timer (ticked by `GameLoopService`); money is only credited via `ClaimSoldItem`, which no Market screen exists to call. Selling from Storage removes the item immediately and starts this real timer honestly, but the player currently has no way to see or claim the resulting gold. This is a genuine gap in the existing Market feature, not something invented or hidden by this change.
- **Equip (from unequipped) has no button.** Requires a target character, and no character-selection mechanism is reachable from Storage/Item Detail without inventing new cross-screen UI. A hint text directs the player to the Characters screen instead, matching the exact precedent already set by the legacy `InventoryScreen`.
- **`Price` and `Rarity` never render** — not a bug in this dialog, but a newly-confirmed data-pipeline gap: `ItemDefinition.Price`/`.Rarity`/`.Consumable`/`.NotSellable` are never assigned by any parser code (case-sensitive `JsonUtility` field-name mismatch against the lowercase JSON keys). Verified empirically in Play Mode (every item shows `0`/`false`). Fixing this is a Database/parser change, out of scope here.
- **No description text** — only a localization *key* exists in the data (`idDescription`), never resolvable text; omitted rather than shown raw.
- **Sell can theoretically affect the wrong instance** if two separate non-stackable items share a definition id, because `IMerchantService.SellItem`/`ConsumeByDefinitionId` operate on definition id, not instance id. Pre-existing backend behavior, not introduced here.
- **No filter, no sort, no category empty-state** — removed by explicit request (revision 2); the grid always shows the full, unordered `GetAllItems()` list.
- **No storage-upgrade flow** — no API exists (see revision 1 audit); capacity is read-only.
- **Icon key mismatch for ~2% of items** (11/607 egg-type materials) — see revision 1 finding.
- **No rarity frame** — confirmed correct 1:1 port of legacy's uniform-border behavior, not an omission.

Phase 5C stops here. No Workshop or later phase was started.
