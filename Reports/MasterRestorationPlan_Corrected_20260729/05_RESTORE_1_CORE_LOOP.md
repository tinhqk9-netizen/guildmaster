# RESTORE_1 — CORE LOOP

**Goal:** Tavern → Recruit → Combat → Loot → Equipment end-to-end confirmed or fixed.
**Effort:** ~2–3 days
**Dependencies:** RESTORE_0 PASS
**Risk:** 🟡 MEDIUM — core loop is the most complex path
**Gate:** Tavern→Loot end-to-end flow completes in-editor

---

## Scope

RESTORE_1 covers the primary player loop:

```
Tavern (recruit adventurers) 
    → Character (view, equip, manage)
    → Dungeon (enter, auto-combat, collect)
    → Loot (receive items)
    → Equipment (equip new items)
    → repeat
```

---

## Tasks

### T1-1: Trace Tavern→Recruit→Character Flow

1. Confirm `TavernScreen` opens from HUD
2. Trace `TavernScreen.Recruit()` → `TavernService.RecruitGuest()`:
   - Does it deduct cost? (FormulaService cost check)
   - Does it call `CharacterService.AddCharacter()`?
   - Does it remove guest from `SaveData.TavernGuests`?
   - Does `TavernGuestSaveData.DefinitionId` point to valid `TavernGuestDefinition` in GameDatabase?
3. Confirm `LevelQuarters`/`UpgradeQuarters` work for unlocking guest slots
4. Confirm recruited character appears in `CharacterScreen`

**Evidence:** List every method called, every SaveData field mutated. Mark STATIC_TRACE_CONFIRMED if all references exist.

### T1-2: Trace Equipment Flow

1. Confirm `CharacterScreen` shows weapon/armor/accessory slots
2. Trace `EquipmentService.Equip()`:
   - Does it verify item is equippable (item slot type matches)?
   - Does it update `CharacterSaveData.WeaponInstanceId` (or Armor/Accessory)?
   - Does it remove item from inventory?
3. Trace `EquipmentService.Unequip()`:
   - Does it clear the slot AND return item to inventory?
4. Trace `InventoryService.RemoveItem()` → does it clear equip references? (G17 — EQUIP_DANGLING_REF)
   - If not, add guard: "If removed item is equipped, clear the slot"
5. Confirm `CharacterService.GetTotalStat()` includes equipment bonuses
6. Confirm equipment screen refresh on equip/unequip

**Evidence:** Equip/Unequip → SaveData mutation confirmed. Dangling ref guard verified (or noted as missing → FIX in this phase).

### T1-3: Trace Dungeon→Combat→Loot Flow

1. Confirm `DungeonScreen` opens from HUD with available dungeons
2. Trace `DungeonScreen.StartDungeon()` → `DungeonService.StartDungeon()`:
   - Creates `ActiveDungeon` in SaveData
   - Loads enemy definitions
   - Initializes combat state
3. Trace `DungeonScreen.Tick()` → `DungeonService.Tick()`:
   - Calls `CombatService.ProcessTurn()`
   - Updates dungeon state
   - Checks win/lose conditions
4. **Fix TICK RATE (G09):** Add delta time throttle so dungeon doesn't complete in 2 seconds:
   ```csharp
   // DungeonScreen.Update() or Tick():
   if (Time.time - lastTickTime < tickInterval) return;
   lastTickTime = Time.time;
   ```
5. Trace `DungeonScreen.CollectDrops()`:
   - Calls `LootService.GenerateDrops()` (from DungeonDefinition loot tables)
   - Calls `InventoryService.AddItems()` with generated drops
   - Marks dungeon as completed in SaveData
6. **Fix EQUIP DANGLING REF (G17):** Add guard in `InventoryService.RemoveItem()`

**Evidence:** Combat formula traced, Loot generation traced, Dungeon state transitions confirmed.

### T1-4: Inventory Detail Flow

1. Confirm `InventoryScreen` shows all items
2. Trace:
   - `InventoryScreen.Show()` → `InventoryService.GetAllItems()`
   - Item filtering (weapons/armor/accessories/consumables/materials)
   - Item locking (`InventoryService.ToggleLockItem()`)
   - Item usage (`InventoryService.UseConsumable()`)
   - Item selling (button → MerchantScreen)
3. Confirm ItemSaveData → ItemRuntime conversion (RuntimeFactory)

### T1-5: Equip Dangling Ref Guard Fix

**In `InventoryService.RemoveItem()`:**

```csharp
public bool RemoveItem(string instanceId, int amount)
{
    // ... existing removal logic ...
    
    // NEW: Clear equip references if removed item was equipped
    foreach (var character in saveData.Characters)
    {
        if (character.WeaponInstanceId == instanceId)
            character.WeaponInstanceId = null;
        if (character.ArmorInstanceId == instanceId)
            character.ArmorInstanceId = null;
        if (character.AccessoryInstanceId == instanceId)
            character.AccessoryInstanceId = null;
    }
    
    return true;
}
```

---

## Verification Gate — RESTORE_1 PASS Criteria

| Check | Method | Status |
|-------|--------|--------|
| TavernScreen opens from HUD | NOT_RUN (needs editor) | GATE |
| RecruitGuest → Character created | STATIC_TRACE_CONFIRMED | ⬜ |
| CharacterScreen shows stats | STATIC_TRACE_CONFIRMED | ⬜ |
| Equip/Unequip → SaveData mutation | STATIC_TRACE_CONFIRMED | ⬜ |
| RemoveItem clears equip refs | STATIC_TRACE_CONFIRMED | ⬜ |
| All equipment bonuses in GetTotalStat() | STATIC_TRACE_CONFIRMED | ⬜ |
| DungeonScreen opens with available dungeons | NOT_RUN | GATE |
| Dungeon tick throttled (not 60fps) | STATIC_TRACE_CONFIRMED | ⬜ |
| CombatService.ProcessTurn() traced | STATIC_TRACE_CONFIRMED | ⬜ |
| Loot drops generated and added to inventory | STATIC_TRACE_CONFIRMED | ⬜ |
| Dungeon state win/lose → SaveData | STATIC_TRACE_CONFIRMED | ⬜ |
| Inventory shows all items with filtering | STATIC_TRACE_CONFIRMED | ⬜ |
| Item lock/use/sell traced | STATIC_TRACE_CONFIRMED | ⬜ |
| **End-to-end flow complete** | NOT_RUN (needs editor) | GATE |
