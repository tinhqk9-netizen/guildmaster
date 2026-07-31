# CORRECTED DEPENDENCY GRAPH

```
Direction: A → B means "A depends on B" (B must be stable before A can be built)
```

---

## High-Level Phase Dependencies

```
RESTORE_0 (Foundation)
    ├── no dependencies (baseline)
    └── provides: stable boot, service wiring, save/load, data deserialization
    
RESTORE_1 (Core Loop)
    ├── depends on: RESTORE_0
    └── provides: Tavern→Combat→Loot→Inventory end-to-end
    
RESTORE_2 (Quest Raid Progression)
    ├── depends on: RESTORE_1 (needs core loop for quest completion triggers)
    └── provides: 56 quests, Raid, unlock chain
    
RESTORE_3 (Economy)
    ├── depends on: RESTORE_1 (core loop generates resources)
    └── provides: Workshop, Merchant, Market, timers

RESTORE_4 (Designed Systems)
    ├── depends on: RESTORE_1, RESTORE_2, RESTORE_3
    │   (Pets need inventory, Promotion needs character service,
    │    Doctrine needs quest reward pipeline)
    └── provides: Pets, Promotion, Ascension, Doctrine UI, Shelter decision

RESTORE_5 (Save Offline UI Polish)
    ├── depends on: RESTORE_0, RESTORE_1, RESTORE_2, RESTORE_3, RESTORE_4
    │   (save migration needs stable schema, offline needs all systems)
    └── provides: Migration, backup, offline, final regression
```

---

## Detailed Sub-system Dependency Map

### Foundation Layer (RESTORE_0)

```
Bootstrapper
  ├── ServiceContainer.Initialize() ───────────────────── no deps
  ├── GameDatabase.LoadAll()
  │     ├── JsonUtility (unity built-in)
  │     └── Data files in Resources/Data/ *.json
  ├── SaveService.LoadGame()
  │     ├── Windoid (file I/O wrapper)
  │     ├── JsonUtility.FromJson<SaveData>
  │     └── NormalizeAfterLoad()
  └── UIService.Initialize()
        └── UIScreen registry (loaded screens)
```

### Core Loop Layer (RESTORE_1)

```
TavernService
  ├── GameDatabase (TavernGuestDefinition, QuartersDefinition)
  ├── SaveData (TavernGuests, LevelQuarters, UpgradeQuarters)
  └── FormulaService (cost calculations)

CharacterService
  ├── GameDatabase (CharacterDefinition)
  ├── SaveData (Characters)
  └── InventoryService (equipment)

CombatService
  ├── CharacterService (stats)
  ├── EquipmentService (equipped bonuses)
  ├── SaveData (active dungeon state)
  └── FormulaService (damage formulas)

DungeonService
  ├── GameDatabase (DungeonDefinition, EnemyDefinition)
  ├── SaveData (Dungeons, ActiveDungeon)
  ├── CombatService (process turn)
  └── LootService (drop generation)

LootService
  ├── GameDatabase (LootTableDefinition)
  └── InventoryService (add items)

InventoryService
  ├── SaveData (Items)
  └── ItemService (get by definition)

EquipmentService
  ├── SaveData (Characters → Weapon/Armor/AccessoryInstanceId)
  ├── InventoryService (get item instances)
  └── CharacterService (recalculate stats)
```

### Quest Raid Progression Layer (RESTORE_2)

```
QuestService
  ├── GameDatabase (QuestDefinition — 56 quests)
  ├── SaveData (Quests)
  ├── CharacterService (character conditions)
  ├── InventoryService (item conditions)
  ├── DungeonService (dungeon conditions)
  ├── DoctrineService (reward integration)
  └── QuestScreen (UI refresh)

RaidService (MISSING — G08)
  ├── GameDatabase (RaidDefinition)
  └── CombatService (reuse combat)

UnlockService (MISSING — G05)
  ├── GameDatabase (unlock conditions)
  └── DungeonService/DungeonScreen (gate UI)
```

### Economy Layer (RESTORE_3)

```
CraftService
  ├── GameDatabase (RecipeDefinition)
  ├── SaveData (WorkshopQueue)
  ├── InventoryService (materials)
  └── FormulaService (craft time)

MerchantService
  ├── GameDatabase (MerchantOfferDefinition)
  ├── SaveData (MarketListings, MerchantRegularStockItems)
  ├── InventoryService (buy/sell)
  └── FormulaService (pricing)

MarketService (part of Merchant)
  ├── SaveData (SoldMarketItems, MerchantRegularStockItems)
  └── InventoryService (claim sold items)
```

### Designed Systems Layer (RESTORE_4)

```
PetService (MISSING — G01)
  ├── GameDatabase (PetDefinition — EMPTY)
  ├── SaveData (Pets — MISSING field)
  ├── CharacterService (equip to character)
  └── CombatService (stat bonuses)

PromotionService (MISSING — G03)
  ├── GameDatabase (PromotionDefinition — MISSING)
  ├── SaveData (PromotionTier — MISSING)
  ├── CharacterService (stat multipliers)
  └── InventoryService (item costs)

AscensionService (MISSING — G02)
  ├── SaveData (AscensionLevel — currently bool IsAscended)
  ├── CharacterService (stat multipliers)
  ├── InventoryService (cost deduction)
  └── FormulaService (cost formulas)

DoctrineScreen (MISSING — G04)
  ├── DoctrineService (level/progress data)
  └── UIService (screen registration)

Shelter decision (G13-G15)
  └── Must decide: reuse/migrate/rename/deprecate LevelShelter/UpgradeShelter/LevelShelterAutofeed
```

### Save Offline UI Polish Layer (RESTORE_5)

```
SaveMigrationService (MISSING)
  ├── SaveData (multiple versions)
  └── Backup files

OfflineProgressService
  ├── CraftService (workshop progress)
  ├── MerchantService (market refresh)
  ├── TavernService (visitor regeneration)
  └── All services (active-state restoration)

AudioService (MISSING)
  └── SettingsService (volume)

LocalizationService (MISSING)
  ├── GameDatabase (ui_strings.json)
  └── SettingsService (language)
```

---

## Validation: No Circular Dependencies

The dependency graph is a DAG (directed acyclic graph). Key property checks:

| Check | Result |
|-------|--------|
| Services only reference SaveData, GameDatabase, or other services? | ✅ — no circular references |
| UI depends on Services, not vice versa? | ✅ — UIScreen → Service, never Service → UIScreen |
| SaveData has no service references? | ✅ — POCO serializable |
| GameDatabase has no service references? | ✅ — data-only |
| Can phases be executed in sequence without rework? | ✅ — no phase depends on a later phase |
