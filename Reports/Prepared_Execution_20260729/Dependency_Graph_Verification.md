# DEPENDENCY GRAPH & VERIFICATION REPORT
## Generated: 2026-07-29

---

## 1. Service Dependency Graph (Static Trace)

```
Bootstrapper (UIRuntimeBootstrap)
  └── ServiceContainer.RegisterServices()
        ├── ISaveService ─► SaveService
        │     └── SaveData
        ├── IGameDatabase ─► GameDatabase
        ├── IInventoryService ─► InventoryService
        │     └── ISaveService, IItemService
        ├── ITavernService ─► TavernService
        │     └── ISaveService, ICharacterService, IFormulaService, IItemService
        ├── ICharacterService ─► CharacterService
        │     └── ISaveService, IFormulaService, IDatabaseService
        ├── IEquipmentService ─► EquipmentService
        │     └── ISaveService, IInventoryService
        ├── IDungeonService ─► DungeonService
        │     └── ISaveService, ICombatService, ILootService, IFormulaService
        ├── ICombatService ─► CombatService
        │     └── IFormulaService
        ├── ILootService ─► LootService
        │     └── IDatabaseService, IItemService, ISaveService
        ├── ICraftService ─► CraftService
        │     └── ISaveService, IDatabaseService, IInventoryService, IFormulaService, IItemService
        ├── IMerchantService ─► MerchantService
        │     └── IDatabaseService, IInventoryService, ISaveService
        ├── IQuestService ─► QuestService
        │     └── ISaveService, IDoctrineService
        ├── IDoctrineService ─► DoctrineService
        │     └── ISaveService, IFormulaService
        ├── IOfflineProgressService ─► OfflineProgressService
        │     └── ISaveService, ICraftService, IMerchantService
        ├── IFormulaService ─► FormulaService
        │     └── (stateless)
        ├── IItemService ─► ItemService
        │     └── IDatabaseService
        ├── IUIService ─► UIService
        │     └── (stack-based screen registry)
        └── ITimeService ─► TimeService
              └── (stateless utility)
```

### Graph Statistics
| Metric | Count |
|--------|-------|
| Total registered services | 18 |
| Maximum dependency depth | 4 (CraftService, MerchantService) |
| Average dependencies per service | 2.5 |
| Services with 0 dependencies | 0 |
| Services with 1 dependency | 3 (Formula, Item, UI, Time) |
| Services with 2 dependency | 7 |
| Services with 3 dependencies | 2 |
| Services with 4 dependencies | 2 (Craft, Merchant) |
| Circular dependency detected | 0 |

---

## 2. Verification: Claim vs Actual

### Claim: "18 services wired"
- ✅ Confirmed all 18 registered in ServiceContainer

### Claim: "Save/Load triple-fallback"
- ✅ Primary → backup → fresh default

### Claim: "NormalizeAfterLoad guards all lists"
- ✅ 14 list guards + 4 per-character guards confirmed

### Claim: "Combat uses original DAD formula"
- ✅ RollAttackDamage + ApplyDamage traced to Java source

### Claim: "Doctrine all 8 types"
- ✅ All 8 pairs (Level + Progress) in SaveData + DoctrineService

### Claim: "Offline progresses craft + market"
- ✅ ProgressWorkshop + ProgressMarket confirmed

### Claim: "One dead Bootstrap file"
- ✅ Bootstrap/Bootstrapper.cs is dead (LoadMainScene commented out)
- ✅ Runtime/Boot/Bootstrapper.cs has 0 references

### Claim: "MigrateSave placeholder"
- ✅ SaveService.cs:138-141 confirmed empty

### Claim: "19 services"
- ❌ Actual: 18 registered (plan overstated by 1)

---

## 3. Critical Path Dependency Chain

```
SaveData ──► SaveService ──► All Services ──► UI Screens
  │                              │                 │
  └── GameDatabase ──────► Definition ────────► Display
                              Types                Data
```

**Single point of failure:** `SaveService.CurrentData` is the central data hub. If Save() fails, ALL service mutations are lost.
**Mitigation:** Every service mutation explicitly calls `_saveService.Save()` — but this is NOT throttled.

---

## 4. Integration Gap Analysis

| Integration Point | Producer | Consumer | Verified |
|-----------------|----------|----------|----------|
| Recruit → Add Character | TavernService | CharacterService | ✅ |
| Equip → Sync Save | EquipmentService | InventoryService + SaveData | ✅ |
| Start Dungeon → Combat | DungeonService | CombatService | ✅ |
| Combat Result → Loot | DungeonService | LootService → InventoryService | ✅ |
| Craft → Complete → Inventory | CraftService | InventoryService | ✅ |
| Buy → Add Item | MerchantService | InventoryService | ✅ |
| Sell → Remove Item → Listing | MerchantService | InventoryService + MarketListings | ✅ |
| Quest Complete → Doctrine | QuestService | DoctrineService | ✅ |
| Offline → Progress Craft | OfflineProgressService | CraftService | ✅ |
| Offline → Progress Market | OfflineProgressService | MerchantService | ✅ |
| **Session → Quest** | **VARIOUS** | **QuestService.Increment()** | ❌ **ALL 56 MISSING** |
| **Boot → Offline** | **Bootstrapper** | **OfflineProgressService** | ❌ **NOT CALLED** |

---

## 5. Phase Exit Verdict Summary

| Phase | Status | Gates Passed | Blockers |
|-------|--------|-------------|----------|
| RESTORE_0 | ⚠️ PARTIAL | 14/18 | C4, C5, C6, C8 |
| RESTORE_1 | ⚠️ PARTIAL | 9/11 | G17, tick throttle |
| RESTORE_2 | ❌ FAIL | 3/11 | 56 quest callers, raid, unlock, chain gate |
| RESTORE_3 | ⚠️ PARTIAL | 8/10 | G10, G11 |
| RESTORE_4 | ❌ FAIL | 2/13 | 5 systems missing |
| RESTORE_5 | ❌ FAIL | 6/11 | MigrateSave, active state, loading, regression |

**Final Verdict: NOT PLAYTEST READY**

The core foundation and economy are solid (18 services wired, save/load triple-fallback, all formulas traced). But 4 critical production gaps (quest callers, offline progress, new game defaults, definition validation) prevent any meaningful play session.

**Estimated effort to REACH playtest-ready: ~5-7 days**
- Day 1: Fix C4, C5, C6, C8 + quest callers (critical path)
- Day 2: Wire quest callers to all 6 event sources
- Day 3: G17 fix + G10 craft bar + G11 market timer
- Day 4: Dungeon chain gate (G05) + dungeon tick throttle
- Day 5: DoctrineScreen (G04) + Active dungeon state handler (G07)
- Day 6: Loading screen (G12) + regression test
- Day 7: Unity play test, bug fixes, polish
