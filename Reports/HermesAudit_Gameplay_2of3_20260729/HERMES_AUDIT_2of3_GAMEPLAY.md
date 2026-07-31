# HERMES AUDIT 2/3 — GAMEPLAY + ECONOMY TOÀN DIỆN

- **Project:** Guild Master (Unity 6000.3.17f1)
- **Date:** 2026-07-29
- **Auditor:** Hermes Agent (Nous Research)
- **Worktree:** agent-a4b047a2947e2d706
- **Trạng thái:** ⚠️ NHIỀU BLOCKER — chưa thể playtest

---

## MỤC LỤC

1. [Combat Core — S1](#s1-combat-core)
2. [Skills & Status Effects — S2](#s2-skills--status-effects)
3. [Adventurer & Equipment — S3](#s3-adventurer--equipment)
4. [Tavern, Quarters, Party — S4](#s4-tavern-quarters-party)
5. [Dungeon — S5](#s5-dungeon)
6. [Raid — S6](#s6-raid)
7. [Loot — S7](#s7-loot)
8. [Quest — S8](#s8-quest)
9. [Inventory & Storage — S9](#s9-inventory--storage)
10. [Workshop & Recipes — S10](#s10-workshop--recipes)
11. [Merchant — S11](#s11-merchant)
12. [Market — S12](#s12-market)
13. [Shop — S13](#s13-shop)
14. [Production Call Graph — S14](#s14-production-call-graph)
15. [Gameplay Save Coverage — S15](#s15-gameplay-save-coverage)
16. [Player Usable Flow Audit — S16](#s16-player-usable-flow-audit)
17. [Blocker Register — S17](#s17-blocker-register)
18. [Evidence Index — S18](#s18-evidence-index)

---

## S1: Combat Core

### Current State

**File:** `Runtime/Services/CombatService.cs` (202 dòng)

CombatService là một turn-based combat engine rất cơ bản. Cấu trúc:

```
EnterCombat() → khởi tạo encounter, collect alive entities
TickCombatTurn() → mỗi turn:
  1. Get next acting entity
  2. Roll initiative? → KHÔNG, hardcoded IsInitiative=false cho mọi entity
  3. Roll target selection
  4. Roll damage
  5. Apply damage
  6. Post-turn (regeneration, mana regen)
  7. Check win/lose
```

### Damage Formula (DecodeMath cơ bản)

```
raw = Random.Range(minDamage, maxDamage + 1)
  - Adventurer: min=1, max=1 (hardcoded — comment: "Until the weapon damage-modifier port lands")
  - Enemy: min=Definition.MinDamage, max=Definition.MaxDamage

defenseReduction = defense * 0.01
flatReduction (từ các nguồn chưa implement)
effectiveRaw = raw - defenseReduction - flatReduction
clamped = Math.Max(0, effectiveRaw)

Shield absorbs trước → còn lại trừ CurrentHp
```

### What Works ✅

| Tính năng | Mô tả |
|-----------|-------|
| Turn queue | Entity-based turn queue, skip dead entities |
| Basic damage | Damage roll with armor reduction |
| Shield | Shield absorbs before HP damage |
| Mana regen | Regen 1 mana/turn (nhưng không bao giờ dùng — skills không hoạt động) |
| HP regen | Adventurer regen=1, Enemy regen=0 |
| Win check | Tất cả enemy chết → win |
| Lose check | Tất cả adventurer chết → lose |

### What's Broken/Blocked ⚠️

| Tính năng | Trạng thái | Chi tiết |
|-----------|-----------|----------|
| **Adventurer damage** | 🚫 BLOCKER | Hardcoded 1-1. Comment: "Until the weapon damage-modifier port lands" |
| **Initiative system** | ⚠️ ISSUE | IsInitiative=false cho mọi entity. Tất cả entity đánh theo turn order chứ không theo speed |
| **Skill execution** | 🚫 BLOCKER | Skills không được execute trong combat. CombatService không gọi SkillService |
| **Status effects in combat** | 🚫 BLOCKER | Status effects không được apply/check trong damage resolution. Frost, poison, stun, v.v. không có effect |
| **Magic damage from adventurers** | ⚠️ ISSUE | IsMagic=false cho adventurer. Adventurer chỉ đánh physical |
| **RollsDamageThreeTimes** | ⚠️ ISSUE | Hardcoded false cho mọi entity. Multi-hit attacks không hoạt động |
| **Target selection with taunts** | ⚠️ ISSUE | TargetSelectionService có basic rules nhưng taunt không được check |
| **Combat rewards** | ⚠️ ISSUE | Không có loot/exp sau combat. LootService không được gọi |
| **Mana usage** | ⚠️ ISSUE | Mana regen nhưng không có skill nào consume mana |

### File References

- `Runtime/Services/CombatService.cs` — toàn bộ file (202 dòng)
- `Runtime/Services/TargetSelectionService.cs` — targeting logic (~100 dòng)
- `Runtime/Formulas/DecodeMath.cs` — TruncatePrice, ClampStat (80 dòng)
- `Runtime/Models/CharacterRuntime.cs` — dòng 39: CurrentHp=100 hardcoded

---

## S2: Skills & Status Effects

### Skills

**Definition:** `Definitions/SkillDefinition.cs` — CHỈ có 2 field:
```csharp
public string NameKey { get; set; }      // PROPERTY - không deserialize được
public string DescriptionKey { get; set; } // PROPERTY - không deserialize được
```

**JSON Data:** `StreamingAssets/GameData/skills.json`
- **227 skills** — parseStatus `"partial"` CHO TẤT CẢ
- **parseReasons:** `["UNPARSED_ARGS"]` cho tất cả
- rawArgs chỉ chứa string refs (nameKey, descriptionKey)
- **Không có** damage formula, cooldown, mana cost, level requirement, target rule
- Java constructors quá phức tạp, decompiler không parse được

**Runtime:** `Runtime/Models/SkillRuntime.cs` — 13 dòng, chỉ skeleton
**Service:** `Runtime/Services/SkillService.cs` — 20 dòng, interface stub
**Combat integration:** KHÔNG — CombatService không chạy skill nào

### Status Effects

**Definition:** `Definitions/StatusEffectDefinition.cs` — CHỈ có:
```csharp
public StatusEffectType Type { get; set; }  // PROPERTY
public bool IsNegative { get; set; }        // PROPERTY  
public bool IsSerialized { get; set; }      // PROPERTY
```

**JSON Data:** `StreamingAssets/GameData/status_effects.json`
- **25 effects** — parseStatus `"partial"` CHO TẤT CẢ
- parseReasons: `["UNPARSED_ARGS"]`
- Không có duration, magnitude, tick behavior

**Runtime:** `Runtime/Models/StatusEffectRuntime.cs`:
```csharp
public StatusEffectDefinition Definition;
public float RemainingDuration;
public int CurrentMagnitude;
public string SourceEntityId;
```

**Service:** `Runtime/Services/StatusEffectService.cs` — 92 dòng
- Có thể Add/Remove effect
- Có ProgressDuration (tick)
- **KHÔNG được gọi từ CombatService** — effects không ảnh hưởng combat

### Enum Coverage (33 types)

```
TAUNT, DEFENSIVE_STANCE, STUN, SILENCE, ABLAZE, POISON,
REGENERATION, 5x CURSES, BLEED, DELIRIUM, FRENZY, ANOINTED,
INSPIRE, EXALT, PETRIFY, TERRIFY, FROZEN, ...
```

### Verdict

| Hạng mục | Rating |
|----------|--------|
| Skill definitions | 🚫 BLOCKER — 227 skills partial, 0 có data combat |
| Status effect definitions | 🚫 BLOCKER — 25 effects partial, không có duration/magnitude |
| SkillService implementation | 🚫 BLOCKER — chỉ stub 20 dòng |
| Combat-skill integration | 🚫 BLOCKER — không tồn tại |
| StatusEffectService call from Combat | 🚫 BLOCKER — không được gọi |
| Enum completeness | ✅ OK — đầy đủ 25 type |

---

## S3: Adventurer & Equipment

### AdventurerDefinition

**File:** `Definitions/AdventurerDefinition.cs` (34 dòng)

✅ **Public FIELDS** — deserialize hoạt động (có comment warning về bug cũ)
- BaseMaxHp, BaseConstitution, BaseIntelligence, BaseDexterity, BaseDefense, BaseMagicDefense
- WeaponType, ArmorType
- NextClasses, PassiveSkill, ActiveSkill
- `ManualRuleRequired_PotionDrinkerType` — chưa port

**JSON:** `StreamingAssets/GameData/adventurers.json` — 129 adventurers
- parseStatus `"full"` ✅
- Flat fields at root (BaseMaxHp: 25, etc.) map trực tiếp

### CharacterRuntime

**File:** `Runtime/Models/CharacterRuntime.cs` (43 dòng)
- InstanceId, DefinitionId, Definition
- Level, Experience
- Weapon, Armor, Accessory items
- CurrentHp=100 (hardcoded — constructor dòng 40)
- CurrentMana, CurrentShield
- PositiveStatusEffects, NegativeStatusEffects
- IsAscended, Trait, PotionsDrank[6]
- ActiveSkillId, PassiveSkillId

### CharacterService

**File:** `Runtime/Services/CharacterService.cs` (~260 dòng)
- Hire từ Tavern
- Level up (tăng stats)
- Stats calculation với equipment modifiers
- Equipment stat aggregation

### EquipmentService

**File:** `Runtime/Services/EquipmentService.cs` (~140 dòng)
- Equip/Unequip item vào slot
- Slot validation (WeaponType/ArmorType matching)
- Equipment change notification

### Verdict

| Hạng mục | Rating |
|----------|--------|
| AdventurerDefinition loading | ✅ OK — public fields, JSON full parse |
| CharacterRuntime model | ✅ OK — đủ fields cho gameplay cơ bản |
| CharacterService hire/level | ✅ OK — logic đầy đủ |
| EquipmentService equip/unequip | ✅ OK — slot validation hoạt động |
| Equipment stat apply | ✅ OK — equipment modifiers factored |
| PotionDrinkerType port | ⚠️ Not urgent — deferred |
| CurrentHp=100 hardcoded | ⚠️ ISSUE — không dùng FormulaService.MaxHp |

---

## S4: Tavern, Quarters, Party

### TavernService

**File:** `Runtime/Services/TavernService.cs` (~180 dòng)
- Visitor pool management
- Refresh visitors (cost-based)
- Hire visitor → CharacterService
- Visitor quality scaling

### What Works ✅
- Generate visitor pool từ database
- Visitor quality depends on guild level/renown
- Hire flow hoàn chỉnh

### What's Missing ⚠️
- **Party formation:** Không có hệ thống party riêng. ActiveDungeonSaveData lưu AdventurerInstanceIds nhưng không có UI/service để chọn party
- **Quarters assignment:** SaveData có `QuarterAssigned` slots (6 slots) nhưng không có UI để assign
- **Dismiss/fire adventurer:** Chưa có

### Verdict

| Hạng mục | Rating |
|----------|--------|
| Tavern visitor gen | ✅ OK |
| Hire flow | ✅ OK |
| Party selection UI | ⚠️ ISSUE — không có màn hình chọn party cho dungeon |
| Quarters assignment | ⚠️ ISSUE — save data tồn tại nhưng không có UI |
| Dismiss adventurer | ⚠️ ISSUE — chưa implement |

---

## S5: Dungeon

### DungeonService

**File:** `Runtime/Services/DungeonService.cs` (554 dòng)

Đây là service lớn nhất, implement:

```
Explore(action):
  - Resolve fog-of-war
  - Generate events (chest, shrine, trap, combat, merchant, exit)
  - Advance floor progress
  
EnterCombat() → CombatService.StartCombat
ResolveCombatWin() → loot, advance
Rest() → heal party
ExitDungeon() → collect pending drops
```

### What Works ✅
- Room-by-room progression với floor tracking
- Event generation từ pool (chests, shrines, traps, merchant, combat, exit)
- Fog tracking
- Merchant encounter (gọi MerchantService.RollOffer)
- Save/restore trạng thái dungeon

### What's Missing/Blocked ⚠️

| Tính năng | Rating | Chi tiết |
|-----------|--------|----------|
| Enemy pool generation | ⚠️ ISSUE | EnemyIds từ DungeonDefinition nhưng chưa verify scaling |
| Combat → loot flow | ⚠️ ISSUE | LootService có logic nhưng chưa verify integration |
| Raid transition | ⚠️ ISSUE | Dungeon có exit-to-raid event nhưng chưa implement |
| Floor scaling difficulty | ⚠️ ISSUE | Chưa rõ scaling formula |
| Trap damage formula | ⚠️ ISSUE | Basic implementation |

### Dungeon Save State

```
ActiveDungeonSaveData:
  - DungeonDefinitionId
  - Progress / MaxProgress (floor tracking)
  - AdventurerInstanceIds
  - PendingDrops
  - EncounterState (CombatEncounterSaveData)
  - ActionState (Type, TurnsPassed)
```

### Verdict

| Hạng mục | Rating |
|----------|--------|
| Dungeon progression | ✅ OK — room-by-room |
| Event generation | ✅ OK — đa dạng event types |
| Combat integration | ✅ OK — gọi CombatService |
| Combat → loot | ⚠️ ISSUE — chưa verify |
| Raid transition | ⚠️ ISSUE — skeleton |
| Save/restore dungeon | ✅ OK — đầy đủ save state |

---

## S6: Raid

### Current State

**Definition:** Không tìm thấy RaidDefinition cụ thể (chỉ có trong manifest category mapping)

DungeonService có `ExitToRaid` event type nhưng không có implementation raid riêng.

### Verdict

| Hạng mục | Rating |
|----------|--------|
| RaidDefinition | ⚠️ ISSUE — category tồn tại nhưng không rõ nội dung |
| Raid gameplay loop | 🚫 BLOCKER — chưa implement |
| Integration với Dungeon | ⚠️ Chỉ có event exit-to-raid |

---

## S7: Loot

### LootService

**File:** `Runtime/Services/LootService.cs` (~100 dòng)
- Weighted drop rolling từ EnemyDropEntry tables
- Drop table applied sau deserialization (EnemyDropTableLoader)

### EnemyDropEntry

```
public string ItemId;
public int Weight;       // per-mille, 1000 scale
public int StackCount = 1;
```

### EnemyDefinition

```
[NonSerialized] public List<EnemyDropEntry> DropTable = new List<EnemyDropEntry>();
```
→ NonSerialized vì JsonUtility không đọc được dictionary từ JSON. DropTable được parse riêng qua EnemyDropTableLoader.

### Verdict

| Hạng mục | Rating |
|----------|--------|
| Drop table loading | ✅ OK — special handling qua EnemyDropTableLoader |
| Weighted roll | ✅ OK — per-mille scale |
| Integration vơi Combat | ⚠️ ISSUE — CombatService không gọi LootService sau combat |
| Integration với Dungeon | ⚠️ ISSUE — DungeonService.ResolveCombatWin cần gọi loot |

---

## S8: Quest

### QuestDefinition

**File:** `Definitions/QuestDefinition.cs` (11 dòng)
```csharp
public long TargetProgress { get; set; }  // PROPERTY - không deserialize được!
public string TrueClass { get; set; }     // PROPERTY - không deserialize được!
```

🚫 **Cả 2 fields đều là properties** — JsonUtility sẽ ignore. Quest data không load được.

### QuestService

**File:** `Runtime/Services/QuestService.cs` (~150 dòng)
- Accept quest
- Progress quest
- Complete quest (check TargetProgress)
- Reward claim

### Quest Save State

SaveData có:
- `ActiveQuests` — quests đang theo dõi
- `CompletedQuests` — quests đã hoàn thành

### Verdict

| Hạng mục | Rating |
|----------|--------|
| QuestDefinition loading | 🚫 BLOCKER — properties vs JsonUtility |
| QuestService accept/complete | ✅ OK — logic hoạt động nếu data load |
| Quest rewards | ⚠️ ISSUE — không rõ reward structure |
| Quest progress tracking | ✅ OK — progress tracking |

---

## S9: Inventory & Storage

### InventoryService

**File:** `Runtime/Services/InventoryService.cs` (~200 dòng)
- Add item
- Remove item (by instance or definition)
- Has quantity by definition ID
- Consume by definition ID
- CanAddItem (check capacity)
- Storage capacity từ save data (storage upgrades)

### Storage Capacity

FormulaService.WorkshopQueue và level-based storage upgrades.

### Verdict

| Hạng mục | Rating |
|----------|--------|
| Add/Remove items | ✅ OK |
| Stack management | ✅ OK |
| Capacity check | ✅ OK |
| Storage upgrade tracking | ✅ OK |

---

## S10: Workshop & Recipes

### RecipeDefinition

**File:** `Definitions/RecipeDefinition.cs` (20 dòng)
```csharp
public string OutputItemId;        // FIELD ✅
public List<IngredientData> Ingredients;  // FIELD ✅
```

### Recipe JSON

`StreamingAssets/GameData/recipes.json` — **210KB**
- Nhiều recipes có parseStatus `"partial"`
- `manualRuleRequired: true` cho recipes phức tạp
- Ingredients array thường empty `[]` cho recipes partial
- rawArgs string chứa Java constructor args không parse được

### CraftService

**File:** `Runtime/Services/CraftService.cs` (179 dòng)
- GetMaxCraftable(recipeId) — tính từ inventory
- CanCraft(recipeId) — validate recipe, ingredients, queue
- TryStartCraft(recipeId) — consume ingredients, add to queue
- GetQueue / GetCompletedItems
- ProgressWorkshop(deltaSeconds) — tick-based completion
- ClaimCompletedCraft(instanceId) — claim vào inventory

### Craft Flow

```
TryStartCraft → consume ingredients → add to WorkshopQueue
→ ProgressWorkshop (offlineTime hoặc timer) → item moves to CompletedWorkshopItems
→ ClaimCompletedCraft → ItemRuntime created → InventoryService.AddItem
```

### What Works ✅
- Full queue-based crafting pipeline
- Ingredient validation và consumption
- Timer-based completion
- Claim flow

### What's Blocked ⚠️

| Vấn đề | Rating | Chi tiết |
|--------|--------|----------|
| Recipe data partial | ⚠️ ISSUE | Nhiều recipes ingredients empty |
| ItemDefinition stats broken | 🚫 BLOCKER | Output items có zero stats vì properties issue |
| Craft duration hardcoded | ⚠️ ISSUE | DEFAULT_CRAFT_DURATION_SECONDS = 10 (không từ recipe) |
| Queue capacity formula | ✅ OK | FormulaService.WorkshopQueue |

---

## S11: Merchant

### MerchantService

**File:** `Runtime/Services/MerchantService.cs` (187 dòng)
- RollRegularOffer(dungeonId) — weighted từ dungeon's RegularMerchantOffers
- RollSpecialOffer(dungeonId) — weighted từ dungeon's SpecialMerchantOffers
- BuyOffer(offer, isSpecial) — currency check, deduct, add item
- GetRegularStock / GetSpecialStock

### What Works ✅
- Weighted offer rolling
- Currency validation (Money và Gems)
- Inventory capacity check trước khi mua
- Offer removal từ stock sau khi mua

### What's Blocked ⚠️

| Vấn đề | Rating | Chi tiết |
|--------|--------|----------|
| Item stats zero | 🚫 BLOCKER | Mua item nhưng stats = 0 (ItemDefinition properties issue) |
| BuyItem single | ⚠️ ISSUE | Trả về DeferredPriceOrCurrencyRule — chưa implement |
| Merchant stock persistence | ✅ OK | Save/restore qua MerchantRegularStockItems |
| Dungeon merchant offers | ✅ OK | Load từ DungeonDefinition.RegularMerchantOffers |

---

## S12: Market

### Market System

**File:** `Runtime/Services/MerchantService.cs` — integrated trong MerchantService

**Sell flow:**
```
SellItem(definitionId, stackCount):
  → InventoryService.ConsumeByDefinitionId
  → Add ItemActionSaveData to MarketListings
  → ProgressMarket(deltaSeconds): tick → move SoldMarketItems
  → ClaimSoldItem: lookup price từ ItemDefinition.SellPrice
    → ItemDefinition.SellPrice là PROPERTY → luôn = 0 (hoặc default 100)
```

### What Works ✅
- Listing items for sale
- Timer-based sale completion
- Claim sold items

### What's Blocked ⚠️

| Vấn đề | Rating | Chi tiết |
|--------|--------|----------|
| SellPrice từ ItemDefinition | 🚫 BLOCKER | SellPrice là property → luôn 0. Code fallback về 100 |
| Price economy | 🚫 BLOCKER | Item prices = 0 vì properties issue |
| Market level scaling | ✅ OK | FormulaService.MarketLevel |

---

## S13: Shop

### Trạng thái

Shop không có service riêng biệt. Các chức năng mua bán được phân bổ:
- **MerchantService.BuyOffer** — mua từ merchant dungeon
- **MerchantService.SellItem** — bán ra market

Không có "general shop" để mua item cơ bản bằng money.

### Verdict

| Hạng mục | Rating |
|----------|--------|
| General shop | ⚠️ ISSUE — không có shop service riêng |
| Buy from merchant | ✅ OK — dungeon merchant |
| Sell to market | ✅ OK — market listing |
| Item pricing | 🚫 BLOCKER — ItemDefinition.Price = 0 |

---

## S14: Production Call Graph

### Service Container Wiring

**File:** `Runtime/Services/ServiceContainer.cs` (4,076 dòng? actually ~100 dòng)

```
ServiceContainer
├── GameDatabase (singleton)
├── ICharacterService → CharacterService
│   ├── GameDatabase
│   ├── IInventoryService
│   └── ISaveService
├── ICombatService → CombatService
│   └── ITargetSelectionService
├── ICraftService → CraftService
│   ├── GameDatabase
│   ├── IInventoryService
│   ├── ISaveService
│   └── IFormulaService
├── IDungeonService → DungeonService
│   ├── GameDatabase
│   ├── ICombatService
│   ├── ICharacterService
│   ├── ILootService
│   ├── IMerchantService
│   └── ISaveService
├── IEnemyService → EnemyService
│   └── GameDatabase
├── IEquipmentService → EquipmentService
│   ├── ICharacterService
│   └── ISaveService
├── IInventoryService → InventoryService
│   ├── GameDatabase
│   └── ISaveService
├── IItemService → ItemService
│   └── GameDatabase
├── ILootService → LootService
│   └── GameDatabase
├── IMerchantService → MerchantService
│   ├── GameDatabase
│   ├── IInventoryService
│   └── ISaveService
├── IQuestService → QuestService
│   ├── GameDatabase
│   ├── ICharacterService (comment: IQuestRewardService?)
│   ├── IInventoryService
│   └── ISaveService
├── ISettingsService → SettingsService
│   └── ISaveService
├── ISkillService → SkillService (STUB)
├── IStatusEffectService → StatusEffectService
│   └── ISaveService
├── ITargetSelectionService → TargetSelectionService
├── ITavernService → TavernService
│   ├── GameDatabase
│   ├── ICharacterService
│   └── ISaveService
├── IDoctrineService → DoctrineService
│   ├── GameDatabase
│   ├── ISaveService
│   └── IFormulaService
└── IOfflineProgressService → OfflineProgressService
    ├── ICraftService
    ├── IMerchantService
    ├── ISaveService
    └── ISettingsService
```

### Critical Observations

| Vấn đề | Chi tiết |
|--------|----------|
| **SkillService là stub** | 20 dòng, không có implementation thật |
| **SkillService không được inject vào CombatService** | CombatService chỉ có ITargetSelectionService |
| **StatusEffectService không được inject vào CombatService** | Status effects tồn tại nhưng combat không biết |
| **Combat không gọi LootService** | Không có ILootService trong CombatService constructor |
| **QuestService.ICharacterService dependency** | Có comment "// Should this be IQuestRewardService?" — chưa chắc chắn |
| **FormulaService optional ở CraftService** | `= null` default → có thể null |

---

## S15: Gameplay Save Coverage

### SaveData Structure

**File:** `Runtime/Save/SaveData.cs` (306 dòng)

```
SaveData
├── Meta
│   ├── Money (long)
│   ├── Gems (long)
│   ├── LastSavedAt (DateTime)
│   ├── LastOfflineTick (DateTime)
│   └── LastSeenRealTime (DateTime)
├── Progression
│   ├── PurchaseFlags (HashSet<string>)
│   ├── DoctrineLevels (Dictionary<string, int>)
│   ├── CompletedDungeonIds (HashSet<string>)
│   └── Statistics (SerializableDictionary)
├── Characters
│   ├── Characters (list)
│   ├── TavernVisitors (list)
│   └── QuarterAssigned (string[6])
├── Items
│   ├── Items (list)
│   └── StorageUpgrades (int)
├── Quests
│   ├── ActiveQuests (list)
│   └── CompletedQuests (list)
├── Dungeon
│   ├── ActiveDungeonState (ActiveDungeonSaveData)
│   │   ├── DungeonDefinitionId
│   │   ├── Progress, MaxProgress
│   │   ├── AdventurerInstanceIds
│   │   ├── PendingDrops
│   │   └── EncounterState (CombatEncounterSaveData)
│   │       ├── Enemies
│   │       ├── Corpses
│   │       ├── TurnsFighting
│   │       └── SavedActingEntityId
├── Workshop
│   ├── LevelWorkshopQueue (int)
│   ├── UpgradeWorkshopQueue (int)
│   ├── WorkshopQueue (list)
│   └── CompletedWorkshopItems (list)
├── Market
│   ├── MerchantRegularStockItems (list)
│   ├── MerchantSpecialReserve (list)
│   ├── MarketListings (list)
│   └── SoldMarketItems (list)
├── Settings
│   └── PlayerSettings (SavedSettings)
└── Upgrades
    ├── LevelMarket (int)
    └── StorageUpgradeLevels??? (trong Items)
```

### Coverage Assessment

| Gameplay Area | Saved? | Details |
|---------------|--------|---------|
| Currency | ✅ | Money, Gems |
| Adventurers | ✅ | Characters list with full state |
| Equipment | ✅ | Per-character Weapon/Armor/Accessory |
| Items in inventory | ✅ | Items list |
| Active dungeon | ✅ | Full state with encounter |
| Workshop queue | ✅ | Queue + completed items |
| Market listings | ✅ | Listings + sold items |
| Merchant stock | ✅ | Regular + Special stock |
| Quests active | ✅ | ActiveQuests list |
| Completed quests | ✅ | CompletedQuests |
| Tavern visitors | ✅ | TavernVisitors |
| Quarters assignment | ✅ | QuarterAssigned[6] |
| Doctrine levels | ✅ | DoctrineLevels dictionary |
| Purchase flags | ✅ | Permanent upgrades tracking |
| Settings | ✅ | PlayerSettings |
| Statistics | ✅ | Statistics dictionary |
| Dungeon completion | ✅ | CompletedDungeonIds |
| Offline time | ✅ | LastOfflineTick, LastSeenRealTime |
| **Party composition** | ✅ | Implied from Characters existence |
| **Active quest progress** | ✅ | QuestRuntime tracking |
| **Guild name** | ❌ | Không có trong save |
| **Renown/Reputation** | ❌ | Không rõ — có thể trong Statistics |

### Verdict

✅ **Save coverage rất tốt** — hầu hết gameplay state được persist. Chỉ thiếu guild name (cosmetic).

---

## S16: Player Usable Flow Audit

### End-to-End Flows

#### Flow 1: Open Game → Idle → See Guild

```
1. Game boots → DatabaseBuilder builds DB
   ✅ Dependencies: manifest.json → 10 categories load
   ⚠️ Items load với stats=0 (properties issue)
2. Save loaded → ServiceContainer init
   ✅ 17 services registered
3. OfflineProgressService.ProcessOffline()
   ✅ Workshop progresses, market sells complete
4. UI renders guild screen
   ❓ Chưa audit UI layer
```

**Verdict:** ⚠️ Game boot được nhưng item stats = 0

#### Flow 2: Hire Adventurer from Tavern

```
1. Open Tavern screen
   ✅ TavernService.GetVisitors()
2. See visitor stats
   ✅ AdventurerDefinition loaded correctly
3. Hire → CharacterService.Hire
   ✅ Character added to save
4. Adventurer appears in roster
   ✅ Save contains character
```

**Verdict:** ✅ Hoạt động

#### Flow 3: Equip Adventurer

```
1. Open Character screen
   ❓ UI chưa audit
2. Select equipment slot
   ✅ EquipmentService.EquipItem
3. Validate weapon/armor type
   ✅ Slot validation
4. Stats update
   ✅ Equipment modifiers applied
```

**Verdict:** ✅ Service layer hoạt động, chờ UI audit

#### Flow 4: Send Party to Dungeon

```
1. Select adventurers for party
   ❌ Không có party selection UI
2. Select dungeon
   ✅ Dungeon list từ DB
3. Enter dungeon
   ✅ DungeonService.Explore
4. Progress through rooms
   ✅ Event generation, combat
5. Loot drops
   ⚠️ LootService có logic nhưng cần verify integration
6. Exit dungeon
   ✅ Collect pending drops
```

**Verdict:** ⚠️ Thếu party selection UI

#### Flow 5: Combat

```
1. Enter combat encounter
   ✅ CombatService.EnterCombat
2. Adventurers attack
   ✅ Basic attack, damage=1
3. Enemies attack
   ✅ Enemy damage từ definition
4. Skills/abilities
   🚫 Skills không hoạt động
5. Status effects
   🚫 Effects không apply
6. Win → loot
   ⚠️ Loot không được gọi tự động
7. Lose → retreat
   ✅ Party wipe detection
```

**Verdict:** 🚫 Combat cơ bản chạy nhưng skills/effects/loot không hoạt động

#### Flow 6: Craft Item at Workshop

```
1. Open Workshop
   ✅ Recipe list từ DB
2. Select recipe to craft
   ✅ CraftService.CanCraft
3. Consume ingredients
   ✅ Ingredient validation
4. Wait for completion
   ✅ Timer-based (10s default)
5. Claim crafted item
   ✅ ClaimCompletedCraft
```

**Verdict:** ⚠️ Recipe data partial, item stats = 0

#### Flow 7: Buy from Merchant

```
1. Encounter merchant in dungeon
   ✅ Merchant offer rolling
2. Browse offers
   ✅ Regular/Special stock
3. Buy item
   ✅ Currency check + deduction
4. Item added to inventory
   ✅ InventoryService.AddItem
```

**Verdict:** ⚠️ Mua được nhưng item stats = 0

#### Flow 8: Sell on Market

```
1. List item for sale
   ✅ MerchantService.SellItem
2. Wait for buyer
   ✅ Timer (20s default)
3. Claim money
   ✅ ClaimSoldItem
4. Money added
   ✅ Price từ ItemDefinition.SellPrice (fallback=100)
```

**Verdict:** ⚠️ Bán được nhưng SellPrice không load từ JSON

#### Flow 9: Complete Quest

```
1. Accept quest
   ✅ QuestService.AcceptQuest
2. Progress quest
   ✅ Progress tracking
3. Complete quest
   ✅ Check TargetProgress
4. Claim reward
   ❓ Reward structure không rõ
```

**Verdict:** ⚠️ Quest progress tracking OK, rewards undefined

#### Flow 10: Offline Progress

```
1. Close game
   ✅ Save on quit
2. Reopen after hours
   ✅ OfflineProgressService.ProcessOffline
3. Workshop completed
   ✅ Craft progresses
4. Market sold
   ✅ Market progresses
5. Resources generated
   ❓ Resource gen formulae
```

**Verdict:** ✅ Offline progress pipeline hoạt động

---

## S17: Blocker Register

### 🚫 CRITICAL BLOCKERS (Không thể playtest)

| # | Blocker | Files | Impact | Root Cause |
|---|---------|-------|--------|------------|
| B1 | **Item stats = 0** | ItemDefinition.cs (properties), items.json (fields dict), UnityJsonSerializer.cs | ALL economy systems: equipment, crafting, merchant, market không có giá trị item. Items sinh ra với stats=0, price=0 | ItemDefinition dùng C# properties (`{ get; set; }`) thay vì public fields. JsonUtility chỉ serialize được public fields. AdventurerDefinition.cs có comment cảnh báo bug này |
| B2 | **Skills không có data** | skills.json (227 partial), SkillDefinition.cs, SkillService.cs | Combat không có skills. Adventurers không thể dùng active/passive skills | Java decompiler không parse được complex constructors. Tất cả skill args là UNPARSED_ARGS |
| B3 | **Status effects không có data** | status_effects.json (25 partial), StatusEffectDefinition.cs | Combat không có status effects. Frozen, poison, stun, regen, v.v. không hoạt động | Giống B2 — UNPARSED_ARGS |
| B4 | **SkillService là stub** | SkillService.cs (20 dòng) | Không có method để execute skill trong combat | Chưa implement |
| B5 | **Combat không gọi skills/effects/loot** | CombatService.cs, SkillService.cs, StatusEffectService.cs, LootService.cs | Combat chỉ basic attack. Không skills, effects, loot sau combat | CombatService không inject các service này |

### ⚠️ HIGH PRIORITY ISSUES

| # | Issue | Files | Impact | Notes |
|---|-------|-------|--------|-------|
| I1 | **Adventurer damage = 1** | CombatService.cs dòng hardcoded | Combat không có scaling. Level 1 hay 50 đều damage 1 | Comment: "Until the weapon damage-modifier port lands" |
| I2 | **QuestDefinition properties** | QuestDefinition.cs | TargetProgress và TrueClass không load | Cùng properties vs JsonUtility bug |
| I3 | **No party selection UI** | — | Không thể chọn adventurer cho dungeon | ActiveDungeonSaveData có AdventurerInstanceIds nhưng không có UI |
| I4 | **No initiative system** | CombatService.cs | Turn order không dựa trên speed | IsInitiative=false hardcoded |
| I5 | **ItemType/ParentClass mapping** | ItemDefinition.cs, AdventurerDefinition.cs | ItemType không load (property) + không map từ parentClass | Equipment validation có thể sai |
| I6 | **Recipe data partial** | recipes.json | Nhiều recipes missing ingredients | Cần manual port |
| I7 | **Raid chưa implement** | DungeonService.cs (exit-to-raid) | Raid content không playable | Chưa có RaidService |

### ✅ WORKING SYSTEMS (Không cần sửa ngay)

| System | Status |
|--------|--------|
| AdventurerDefinition loading | ✅ OK |
| EnemyDefinition loading | ✅ OK (complete_from_dex) |
| DungeonDefinition loading | ✅ OK |
| Database building pipeline | ✅ OK (manifest + 10 categories) |
| Save/Load | ✅ OK (toàn diện) |
| Inventory management | ✅ OK |
| Craft flow (queue-based) | ✅ OK (chờ item stats fix) |
| Merchant offers | ✅ OK (chờ item stats fix) |
| Market listing | ✅ OK |
| Tavern visitor generation | ✅ OK |
| Character hire/level/equip | ✅ OK |
| Dungeon exploration | ✅ OK |
| Offline progress | ✅ OK |
| Settings persistence | ✅ OK |

---

## S18: Evidence Index

### Files Read & Line References

| # | File Path | Lines | Content |
|---|-----------|-------|---------|
| E1 | `Services/CombatService.cs` | 202 | Combat engine, turn queue, damage formula |
| E2 | `Services/DungeonService.cs` | 554 | Dungeon exploration, events, progression |
| E3 | `Services/CraftService.cs` | 179 | Queue-based crafting pipeline |
| E4 | `Services/MerchantService.cs` | 187 | Merchant offers, buy/sell, market |
| E5 | `Services/InventoryService.cs` | ~200 | Item add/remove/consume/capacity |
| E6 | `Services/CharacterService.cs` | ~260 | Adventurer hire/level/stats |
| E7 | `Services/EquipmentService.cs` | ~140 | Equip/unequip/slot validation |
| E8 | `Services/TavernService.cs` | ~180 | Visitor pool, hire flow |
| E9 | `Services/QuestService.cs` | ~150 | Quest accept/progress/complete |
| E10 | `Services/LootService.cs` | ~100 | Weighted drop rolling |
| E11 | `Services/SkillService.cs` | 20 | STUB — no implementation |
| E12 | `Services/StatusEffectService.cs` | 92 | Effect tracking, duration tick |
| E13 | `Services/TargetSelectionService.cs` | ~100 | Target rules |
| E14 | `Services/ItemService.cs` | 40 | Create item from definition |
| E15 | `Services/EnemyService.cs` | 30 | Create enemy runtime |
| E16 | `Services/OfflineProgressService.cs` | 70 | Offline time processing |
| E17 | `Services/DoctrineService.cs` | ~100 | Doctrine upgrades |
| E18 | `Services/SettingsService.cs` | 110 | Player settings |
| E19 | `Formulas/FormulaService.cs` | 252 | All gameplay formulas |
| E20 | `Formulas/DecodeMath.cs` | 80 | Math utilities |
| E21 | `Save/SaveData.cs` | 306 | Complete save structure |
| E22 | `Save/CombatEncounterSaveData.cs` | 25 | Combat save state |
| E23 | `Save/ActiveDungeonSaveData.cs` | 20 | Dungeon save state |
| E24 | `Save/DungeonActionState.cs` | 11 | Action state enum |
| E25 | `Definitions/ItemDefinition.cs` | 32 | All properties — BROKEN |
| E26 | `Definitions/SkillDefinition.cs` | 20 | Only name/description — BROKEN |
| E27 | `Definitions/StatusEffectDefinition.cs` | 18 | Minimal — BROKEN |
| E28 | `Definitions/QuestDefinition.cs` | 11 | TargetProgress property — BROKEN |
| E29 | `Definitions/AdventurerDefinition.cs` | 34 | Public fields — ✅ WORKS |
| E30 | `Definitions/EnemyDefinition.cs` | 66 | Public fields + drop table — ✅ WORKS |
| E31 | `Definitions/DungeonDefinition.cs` | 31 | Public fields — ✅ WORKS |
| E32 | `Definitions/RecipeDefinition.cs` | 20 | Public fields — ✅ WORKS |
| E33 | `Definitions/PetDefinition.cs` | 3 | EMPTY |
| E34 | `Definitions/DefinitionBase.cs` | 18 | Public fields (id, className, etc.) |
| E35 | `Definitions/ItemEnums.cs` | 50 | All enums (Category, Slot, StatType) |
| E36 | `Definitions/Enums/StatusEffectType.cs` | 33 | 25 effect types |
| E37 | `Models/CharacterRuntime.cs` | 43 | Character runtime model |
| E38 | `Models/EnemyRuntime.cs` | 38 | Enemy runtime model |
| E39 | `Models/ItemRuntime.cs` | ~20 | Item runtime model |
| E40 | `Models/SkillRuntime.cs` | 13 | Skill runtime skeleton |
| E41 | `Models/StatusEffectRuntime.cs` | ~15 | Status effect runtime |
| E42 | `Models/QuestRuntime.cs` | ~30 | Quest runtime state |
| E43 | `Models/DungeonRuntime.cs` | ~50 | Dungeon runtime state |
| E44 | `Database/GameDatabase.cs` | 82 | Type-keyed dictionary registry |
| E45 | `Database/DatabaseBuilder.cs` | 162 | Load pipeline, 10 categories |
| E46 | `Infrastructure/Serialization/UnityJsonSerializer.cs` | 15 | JsonUtility wrapper |
| E47 | `Core/RuntimeFactory.cs` | ~45 | Creates instances from def + level |
| E48 | `Services/ServiceContainer.cs` | ~100 | DI wiring for all services |

### JSON Data Files

| File | Records | Parse Status | Works? |
|------|---------|-------------|--------|
| `adventurers.json` | 129 | full | ✅ |
| `enemies.json` | 122 | complete_from_dex | ✅ |
| `items.json` | 607 | full (but fields dict) | 🚫 |
| `dungeons.json` | 11 | complete_from_dex | ✅ |
| `skills.json` | 227 | partial | 🚫 |
| `status_effects.json` | 25 | partial | 🚫 |
| `recipes.json` | ~200 | partial | ⚠️ |
| `pets.json` | 21 | ? | ⚠️ |
| `quests.json` | ? | ? | 🚫 |

---

## TỔNG KẾT

### Critical Path để Gameplay Hoạt Động

```
Priority 1 (BLOCKERS — phải sửa để game chạy):
  B1: ItemDefinition → chuyển properties → public fields
      + Thêm custom deserializer cho "fields" dictionary
      + Map parentClass → ItemType
  B5: CombatService → inject SkillService, StatusEffectService, LootService

Priority 2 (Combat cơ bản):
  I1: Adventurer damage từ weapon stats (sau khi B1 fixed)
  B2/B3: Skill/StatusEffect data port từ Java constructors
  B4: Implement SkillService.Execute()
  
Priority 3 (Economy):
  I2: QuestDefinition → public fields
  I5: ItemType mapping
  I6: Recipe data hoàn chỉnh

Priority 4 (UI & Polish):
  I3: Party selection screen
  I4: Initiative system
  I7: Raid implementation
```

### Đánh giá tổng thể

| Khu vực | Điểm | Ghi chú |
|---------|------|---------|
| Foundation (Database, Save, DI) | ⭐⭐⭐⭐⭐ | Rất vững chắc |
| Adventurer & Equipment | ⭐⭐⭐⭐ | Hoạt động, thiếu UI party |
| Dungeon exploration | ⭐⭐⭐⭐ | Hoạt động tốt |
| Workshop & Crafting | ⭐⭐⭐ | Pipeline OK, recipe data partial |
| Merchant & Market | ⭐⭐⭐ | Logic OK, item stats = 0 |
| **Combat** | ⭐ | Skills/Effects không hoạt động |
| **Items/Economy** | ⭐ | Properties bug → stats = 0 |
| **Skills & Status Effects** | ⭐ | Data missing, combat không dùng |
| Quest | ⭐⭐ | Service logic OK, data không load |
| Raid | ⭐ | Chưa implement |

**Kết luận:** Chưa thể playtest. Cần sửa ItemDefinition properties bug trước tiên (ảnh hưởng mọi hệ thống economy), sau đó implement skill/status effect data và combat integration.

---

*Hết phần HERMES AUDIT 2/3 — GAMEPLAY + ECONOMY TOÀN DIỆN*
*Chuẩn bị: HERMES AUDIT 3/3 — UI, SAVE SAFETY & INTEGRATION*
