# CODE ARCHAEOLOGY — Rescued Specification
## Generated: 2026-07-29

---

## 1. Service Layer Architecture

```
SaveService ────► SaveData [Serializable: 145 public fields, 14 list guards]
    │
    ├── GameDatabase ──► ScriptableObject definitions
    ├── InventoryService ──► Items list + CRUD
    ├── TavernService ──► Guests + Recruit + Upgrades
    ├── CharacterService ──► Character roster + stat calc
    ├── EquipmentService ──► Equip/Unequip per slot
    ├── DungeonService ──► State machine (Start→Tick→Complete→Loot)
    ├── CombatService ──► Stats → DAD formula → HP delta
    ├── LootService ──► DropTable → weighted roll → items
    ├── CraftService ──► Queue + Timer → complete → claim
    ├── MerchantService ──► Stock + Buy + Sell + Market
    ├── QuestService ──► 56 quest types → Increment → ClaimReward
    ├── DoctrineService ──► 8 types: Level + Progress → Formula
    ├── OfflineProgressService ──► Delta → Craft + Merchant
    ├── FormulaService ──► 252 lines of ported Java formulas
    ├── ItemService ──► CreateFromDefinitionId
    ├── UIService ──► Screen registry + stack nav
    └── TimeService ──► Delta simulation + timeout calc
```

**Total: 18 services** (plan claimed 19 — one was merged or removed)

---

## 2. SaveData Schema

### Metadata
| Field | Type | Default | Notes |
|-------|------|---------|-------|
| SaveVersion | int | 1 | NOT validated on Load |
| SaveTimeUnix | long | 0 | Used for offline delta |
| GameVersion | string | "" | Reference only |

### Core Fields
| Field | Type | Default | Guarded |
|-------|------|---------|---------|
| Money | long | 0 | ❌ Should default 500 |
| Gems | long | 0 | ✅ Acceptable |
| Items | List<ItemSaveData> | new() | ✅ Null guard |
| Characters | List<CharacterSaveData> | new() | ✅ Null guard |
| TavernGuests | List<CharacterSaveData> | new() | ✅ Null guard |
| ActiveDungeon | ActiveDungeonSaveData | null | ❌ Not guarded (intentional) |
| WorkshopQueue | List<ItemActionSaveData> | new() | ✅ Null guard |
| CompletedWorkshopItems | List<ItemActionSaveData> | new() | ✅ Null guard |
| MerchantRegularStockItems | List<MerchantOfferSaveData> | new() | ✅ Null guard |
| MerchantSpecialReserve | List<MerchantOfferSaveData> | new() | ✅ Null guard |
| MarketListings | List<ItemActionSaveData> | new() | ✅ Null guard |
| Quests | List<QuestSaveData> | new() | ✅ Null guard |
| QuestsCompleted | int | 0 | — |
| ItemsCrafted | int | 0 | — |

### Character Fields (per character)
| Field | Type | Notes |
|-------|------|--------|
| InstanceId | string | Guid |
| DisplayName | string | Generated name |
| DefinitionId | string | CharacterDefinition ref |
| Level | int | 1+ |
| CurrentHp, MaxHp, CurrentMp, MaxMp | int | Combat stats |
| BaseAttack, BaseDefense, BaseMagicDef, BaseSpeed | int | Core stats |
| WeaponInstanceId, ArmorInstanceId, AccessoryInstanceId | string | Equipment refs |
| IsAscended | bool | ❌ Should be int |
| PositiveStatusEffects, NegativeStatusEffects | List<StatusEffectSaveData> | Guarded |
| PotionsDrank | List<int> | Guarded |
| Trait | List<int> | Guarded |

### Dungeon State
| Field | Type | Notes |
|-------|------|--------|
| DungeonId | string | Which dungeon |
| CurrentFloor | int | Progress tracker |
| MaxFloor | int | Total floors |
| EncounterLog | List<CombatEncounterSaveData> | Combat history |
| State | DungeonState | Active/Complete/Abandoned |
| Enemies | List<EnemyRuntime> | Current room enemies |

### Doctrine Fields (8 pairs)
| Doctrine | Level Field | Progress Field |
|----------|------------|----------------|
| Affliction | AfflictionLevel | AfflictionProgress |
| Control | ControlLevel | ControlProgress |
| Fortitude | FortitudeLevel | FortitudeProgress |
| Grace | GraceLevel | GraceProgress |
| Illusion | IllusionLevel | IllusionProgress |
| Knowledge | KnowledgeLevel | KnowledgeProgress |
| Ruin | RuinLevel | RuinProgress |
| War | WarLevel | WarProgress |

### Settings
| Field | Type | Default | Notes |
|-------|------|---------|-------|
| SettingsSound | bool | true | ❌ default should be true |
| SettingsMusic | bool | true | ❌ default should be true |

---

## 3. Game Cycle (critical path)

```
1. BOOT
   UIRuntimeBootstrap.Awake() → InitServices()
   UIRuntimeBootstrap.Start()
     ├── IF HasSaveFile → Load() → NormalizeAfterLoad()
     │     └── MISSING: ApplyOfflineProgress()
     └── IF Fresh → CreateDefault() → NormalizeAfterLoad()

2. LOBBY
   HUDController.Show() → Shows Money/Gems + nav buttons
     └── Available: Tavern | Inventory | Character | Dungeon | Craft
               Merchant | Quest | Settings

3. TAVERN → RECRUIT → COMBAT CYCLE
   TavernScreen → RecruitGuest()
     → CharacterService.RecruitCharacter()
     → InventoryScreen → Equip via EquipmentService
     → DungeonScreen → StartDungeon()
     → DungeonService.Tick() → CombatService.ProcessTurn() → AdvanceProgress()
     → RunLoot() → CollectDrops()
     → Equipment drops → Inventory

4. PROGRESSION (GAPS)
   ❌ Dungeon Complete → QuestService.Increment()
   ❌ Items Obtained → QuestService.Increment()
   ❌ Craft Complete → QuestService.Increment()
   ❌ Offline Progress → Called at boot?

5. SAVE
   On quit / auto → Save() → backup → write → completed
```

---

## 4. File Inventory (archeological layers)

### Layer 1: Ported from Java (existing files)
- `SaveData.cs`, `CharacterRuntime.cs`, `ItemRuntime.cs`
- `CombatService.cs`, `DungeonService.cs`, `TavernService.cs`
- `FormulaService.cs` (252 lines with @source annotations)
- `SaveService.cs`, `GameDatabase.cs`
- `InventoryService.cs`, `EquipmentService.cs`
- `DoctrineService.cs`, `QuestService.cs`
- Various definition types and enums

### Layer 2: New Unity-native (existing files)
- `ServiceContainer.cs`, `UIService.cs`, `UIScreen.cs`
- `Bootstrapper.cs`, `UIRuntimeBootstrap.cs`
- `HUDController.cs`, all UI Screen classes
- `OfflineProgressService.cs`
- `ItemService.cs`, `LootService.cs`
- `TimeService.cs`

### Layer 3: Placeholders (existing files - need filling)
- `PetDefinition.cs` — empty shell (G01)
- `MigrateSave()` — empty placeholder (C5)

### Layer 4: Missing (do not exist)
- `RaidService.cs` / `IRaidService.cs` (G08)
- `PromotionService.cs` / `IPromotionService.cs` (G03)
- `DoctrineScreen.cs` / DoctrineScreen prefab (G04)
- `PetService.cs` / PetSaveData (G01)
- `LoadingScreen.cs` (G12)
- `UnlockService.cs` (G18)
- `MainMenuScreen.cs` (optional)
