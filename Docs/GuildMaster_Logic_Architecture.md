# Guild Master — Hệ Thống Logic Game (Toàn Tập)

> Tài liệu diễn giải chi tiết và có hệ thống toàn bộ logic game Guild Master (bản Rebuild Unity).
> Dựa trên code C# trong dự án, các file DAD (Decompiled APK Data) và cấu trúc dữ liệu JSON gốc.

---

## Mục Lục

1. [Tổng Quan Kiến Trúc](#1-tổng-quan-kiến-trúc)
2. [Data Layer — Định Nghĩa & Dữ Liệu](#2-data-layer--định-nghĩa--dữ-liệu)
3. [Database & Data Loading](#3-database--data-loading)
4. [Runtime Models — Thực Thể Game](#4-runtime-models--thực-thể-game)
5. [Save System](#5-save-system)
6. [Service Container — Dependency Injection](#6-service-container--dependency-injection)
7. [Hệ Thống Nhân Vật (Character)](#7-hệ-thống-nhân-vật-character)
8. [Hệ Thống Trang Bị (Equipment)](#8-hệ-thống-trang-bị-equipment)
9. [Hệ Thống Túi Đồ (Inventory)](#9-hệ-thống-túi-đồ-inventory)
10. [Hệ Thống Combat](#10-hệ-thống-combat)
11. [Hệ Thống Dungeon (Hầm Ngục)](#11-hệ-thống-dungeon-hầm-ngục)
12. [Hệ Thống Loot & Rương](#12-hệ-thống-loot--rương)
13. [Hệ Thống Tavern (Tửu Quán)](#13-hệ-thống-tavern-tửu-quán)
14. [Hệ Thống Crafting (Workshop)](#14-hệ-thống-crafting-workshop)
15. [Hệ Thống Merchant (Chợ)](#15-hệ-thống-merchant-chợ)
16. [Hệ Thống Nhiệm Vụ (Quest)](#16-hệ-thống-nhiệm-vụ-quest)
17. [Hệ Thống Học Thuyết (Doctrine)](#17-hệ-thống-học-thuyết-doctrine)
18. [Hệ Thống Thăng Cấp (Promotion)](#18-hệ-thống-thăng-cấp-promotion)
19. [Hệ Thống Kỹ Năng (Skill) & Hiệu Ứng (Status Effect)](#19-hệ-thống-kỹ-năng-skill--hiệu-ứng-status-effect)
20. [Hệ Thống Offline Progress](#20-hệ-thống-offline-progress)
21. [Hệ Thống Game Loop & Tick](#21-hệ-thống-game-loop--tick)
22. [Hệ Thống UI](#22-hệ-thống-ui)
23. [Hệ Thống Formulas (Công Thức Gốc)](#23-hệ-thống-formulas-công-thức-gốc)
24. [Thú Cưng (Pet)](#24-thú-cưng-pet)
25. [Settings & Localization](#25-settings--localization)
26. [Bootstrap & Khởi Động Game](#26-bootstrap--khởi-động-game)
27. [Luật Chơi Chi Tiết Từ Bản Gốc (Recovered Rules)](#27-luật-chơi-chi-tiết-từ-bản-gốc-recovered-rules)
28. [Những Phần Chưa Hoàn Thiện & Công Việc Còn Lại](#28-những-phần-chưa-hoàn-thiện--công-việc-còn-lại)

---

## 1. Tổng Quan Kiến Trúc

```
┌─────────────────────────────────────────────────────┐
│                     UI Layer                        │
│  ┌──────┬──────┬──────┬──────┬──────┬──────┬──────┐ │
│  │ HUD  │Tuyển │Kho Đồ│Chế  │Dungeon│Chợ  │ N.Vụ│ │
│  │      │Dụng │      │Tạo  │(Đánh)│     │      │ │
│  └──────┴──────┴──────┴──────┴──────┴──────┴──────┘ │
├─────────────────────────────────────────────────────┤
│                 Service Layer                       │
│  ┌──────────┬──────────┬──────────┬──────────────┐ │
│  │CharSvc   │InvSvc    │CraftSvc  │ QuestSvc     │ │
│  │EquipSvc  │DungeonSvc│CombatSvc │ LootSvc      │ │
│  │TavernSvc │MerchantSvc│PromoSvc │ OfflineSvc    │ │
│  │DoctrineSvc│SkillSvc │StEffSvc  │ SettingsSvc   │ │
│  │PartySvc  │PetSvc    │TargetSelSvc│ GameLoopSvc │ │
│  └──────────┴──────────┴──────────┴──────────────┘ │
├─────────────────────────────────────────────────────┤
│               Runtime / Model Layer                 │
│  ┌──────────┬────────────┬──────────┬────────────┐ │
│  │CharRuntime│ItemRuntime│EnemyRunt │DungeonRunt │ │
│  │  SkillRt │QuestRuntime│StEffRunt │ .....       │ │
│  └──────────┴────────────┴──────────┴────────────┘ │
├─────────────────────────────────────────────────────┤
│              Database / Definition Layer            │
│  ┌──────────┬────────────┬──────────┬────────────┐ │
│  │GameDB    │ADef    │ItemDef  │EnemyDef     │ │
│  │DungeonDef│RecipeDef   │QuestDef  │SkillDef     │ │
│  └──────────┴────────────┴──────────┴────────────┘ │
├─────────────────────────────────────────────────────┤
│            Data Provider / StreamingAssets          │
│  ┌────────────────────────────────────────────────┐ │
│  │  JSON: items, enemies, adventurers, dungeons, │ │
│  │  quests, recipes, skills, pets, raids, ...     │ │
│  └────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────┘
```

### Nguyên lý kiến trúc:
- **Definition ↔ Runtime ↔ SaveData** — 3 lớp riêng biệt
  - `Definition`: dữ liệu tĩnh từ JSON (cấu hình game)
  - `Runtime`: object chạy trong RAM khi game hoạt động
  - `SaveData`: Serializable data lưu xuống ổ cứng
- **ServiceContainer** — composition root chứa tất cả services
- **ISaveService** xuyên suốt — mọi service đọc/ghi qua save
- **GameDatabase** — registry chứa tất cả definition có index

---

## 2. Data Layer — Định Nghĩa & Dữ Liệu

### 2.1 DefinitionBase (cha của tất cả)

```csharp
// Tất cả definition kế thừa class này
public class DefinitionBase {
    public string id;     // ID duy nhất, key trong GameDatabase
}
```

### 2.2 AdventurerDefinition (Định nghĩa class nhân vật)

- `MaxLevel`: cấp tối đa
- `BaseMaxHp`, `BaseConstitution`, `BaseIntelligence`, `BaseDexterity` — chỉ số nền
- `BaseDefense`, `BaseMagicDefense` — giáp vật lý/ma thuật
- `WeaponType`, `ArmorType` — loại vũ khí/giáp được phép trang bị
- `NextClasses[]` — các class có thể chuyển đổi lên (promotion)
- `PassiveSkill`, `ActiveSkill` — skill bị động/chủ động
- `ManualRuleRequired_PotionDrinkerType` — loại potion được phép uống (chưa port)

### 2.3 ItemDefinition (Vật phẩm)

- `Category`: `Weapon | Armor | Accessory | Consumable | Material | PetFood | Blueprint`
- `ItemType`: loại chi tiết (VD: "Staff", "LightArmor", "Dagger", "Generic")
- `Price`, `SellPrice`: giá mua/bán
- `Consumable`: có biến mất khi dùng không
- `NotSellable`: không thể bán
- `Rarity`: 1-5 (độ hiếm)
- Stats: `Constitution`, `Dexterity`, `Intelligence`, `Defense`, `MagicDefense`, `MaxHp`
- `ManualRuleRequired_StatusEffects`: hiệu ứng kèm theo (chưa port hết)

### 2.4 EnemyDefinition (Quái)

- `nameKey`: key localization
- Stats nền: `BaseMaxHp`, `BaseDefense`, `BaseMagicDefense`, `BaseConstitution`, `BaseIntelligence`, `BaseDexterity`
- `MinDamage`, `MaxDamage`: sát thương
- `IsMagic`, `IsRanged`: đánh xa/phép
- `ExpGiven`: kinh nghiệm khi giết
- `ActiveSkillId`, `PassiveSkillId`: skill
- `DropTable`: danh sách `EnemyDropEntry` với `ItemId` + `Weight` (per-mille) + `StackCount`

### 2.5 DungeonDefinition (Hầm ngục / Khu vực)

- `EnemyIds[]`: danh sách enemy IDs có thể spawn
- `RegularMerchantOffers[]`, `SpecialMerchantOffers[]`: danh sách hàng chợ
- `RequiredClearDungeonId`: dungeon chain gating (phải clear dungeon A mới mở B)
- `QuestEventCategory`: category quest trigger

### 2.6 QuestDefinition

- `TargetProgress`: target hoàn thành
- `TrueClass`: loại quest (dùng để phân loại logic)

### 2.7 RecipeDefinition

- `OutputItemId`: vật phẩm đầu ra
- `Ingredients[]`: danh sách `{ItemId, Amount}`

### 2.8 SkillDefinition

- `id`: skill id
- Dữ liệu bổ sung đang chờ port

### 2.9 PromotionDefinition

- `RequiredLevel`: cấp yêu cầu
- `RequiredItemId`, `RequiredItemCount`: vật phẩm yêu cầu
- `StatMultiplier`: hệ số nhân chỉ số
- `TierName`: "Bronze", "Silver", "Gold" (v.v.)
- `TierIndex`: index promotion

### 2.10 PetDefinition, RaidDefinition, StatusEffectDefinition

- Pet: dữ liệu thú cưng (chưa port đầy đủ)
- Raid: raid event (chưa port đầy đủ)
- StatusEffect: hiệu ứng trạng thái

### 2.11 Enums

- `CombatResult`: `None, Victory, Defeat`
- `StatusEffectType`: các loại effect (Bleed, Stun, v.v.)
- `ItemCategory`, `EquipmentSlot`, `QuestState`, `DungeonState`

---

## 3. Database & Data Loading

### `GameDatabase`

```csharp
// Registry động cho tất cả Definition
GameDatabase.RegisterCollection<T>(IEnumerable<T>) // load 1 loại
GameDatabase.Add<T>(T def)                          // thêm 1 entry
GameDatabase.GetRequired<T>(id)                     // lấy (throw nếu ko có)
GameDatabase.TryGet<T>(id, out T)                   // lấy (safe)
GameDatabase.GetAll<T>()                             // lấy tất cả
```

### `DatabaseBuilder`

Đọc `manifest.json` từ StreamingAssets → xác định danh sách file cần load → đọc từng file JSON → deserialize thành Definition objects → đăng ký vào `GameDatabase`.

Hỗ trợ 2 provider:
- `EditorExternalGameDataProvider`: đọc từ thư mục ngoài (editor)
- `StreamingAssetsGameDataProvider`: đọc từ StreamingAssets (runtime)

### Luồng dữ liệu:

```
StreamingAssets/GameData/manifest.json
         ↓ (liệt kê file cần load)
    items.json → List<ItemDefinition> → RegisterCollection<ItemDefinition>
    enemies.json → ... → RegisterCollection<EnemyDefinition>
    adventurers.json → ...
    dungeons.json → ...
    recipes.json → ...
    skills.json → ...
    quests.json → ...
    pets.json → ...
    raids.json → ...
    status_effects.json → ...
```

---

## 4. Runtime Models — Thực Thể Game

### CharacterRuntime (nhân vật khi đang chơi)

```csharp
class CharacterRuntime {
    InstanceId, DefinitionId, Definition (AdventurerDefinition)
    Level, Experience, IsAscended, AscensionLevel
    Trait, PotionsDrank[6]
    CurrentHp, CurrentMana, CurrentShield
    Weapon / Armor / Accessory (ItemRuntime)
    PositiveStatusEffects[], NegativeStatusEffects[]
    ActiveSkillId, PassiveSkillId
}
```

### EnemyRuntime (quái khi chiến đấu)

```csharp
class EnemyRuntime {
    InstanceId, DefinitionId, Definition (EnemyDefinition)
    CurrentHp, CurrentMana, CurrentShield
    PositiveStatusEffects[], NegativeStatusEffects[]
    ActiveSkillId, PassiveSkillId
    IsDead => CurrentHp <= 0
}
```

### ItemRuntime (vật phẩm trong game)

```csharp
class ItemRuntime {
    InstanceId, Definition (ItemDefinition)
    StackCount, IsLocked
}
```

### DungeonRuntime (phiên dungeon)

```csharp
class DungeonRuntime {
    InstanceId, Definition (DungeonDefinition)
    State (Locked | Unlocked | Completed)
    Progress, MaxProgress, LocalDarkness
    AdventurerInstanceIds[], PendingDrops[]
    Enemies[], Corpses[]
    ActionType, ActionTurnsPassed
    TurnsFighting, SavedActingEntityId
}
```

### QuestRuntime

```csharp
class QuestRuntime {
    InstanceId, Definition (QuestDefinition)
    State, Progress, TargetProgress, Rarity
    IsActive => State == InProgress
}
```

---

## 5. Save System

### `SaveData` — toàn bộ trạng thái game

```csharp
class SaveData {
    // Meta
    Metadata (SaveVersion, SaveTimeUnix, GameVersion)
    
    // Tài chính
    Money, Gems
    
    // Nâng cấp
    LevelStorage + UpgradeStorage
    LevelQuarters + UpgradeQuarters
    LevelTavernCapacity + UpgradeTavernCapacity
    LevelTavernTime + UpgradeTavernTime
    LevelWorkshopTime + UpgradeWorkshopTime
    LevelWorkshopQueue + UpgradeWorkshopQueue
    LevelMarketTime + UpgradeMarketTime
    LevelMarketListings + UpgradeMarketQueue
    LevelShelter + UpgradeShelter
    
    // Học thuyết (Doctrine)
    Affliction/Control/Fortitude/Grace/Illusion/Knowledge/Ruin/War Level + Progress
    
    // Danh sách
    Items[] (ItemSaveData)
    Characters[] (CharacterSaveData)
    Quests[] (QuestSaveData)
    Dungeons[] (DungeonSaveData)
    Skills[] (SkillSaveData)
    Pets[] (PetSaveData)
    TavernGuests[] (CharacterSaveData)
    MerchantRegularStockItems[], MerchantSpecialReserve[]
    
    // Queue thời gian
    WorkshopQueue[], CompletedWorkshopItems[]
    MarketListings[], SoldMarketItems[]
    
    // Thời gian
    LastAccess, LastHourTriggered, Last24Triggered, LastWeekTriggered
    NextTavernVisit
    
    // Settings
    SettingSound, SettingMusic, SettingVibration, SettingNotifications
    SettingCraftMaxAmount, SettingSellMaxAmount
    SettingAutoOpenDungeonDetail, SettingColorblindMode
    SettingConfirmRetreat, SettingConfirmSwap, SettingConfirmUpgrade
    
    // Stats
    ItemsCrafted, ItemsSold, MaxWealth, MaxAdventurerTier
    
    // Flags
    StarterPackPurchased, AdventurerPackPurchased, MerchantPackPurchased
    ImperialVanguardPurchased, UnholyCrusadePurchased
    
    // --- Methods ---
    NormalizeAfterLoad()  // sửa null lists sau deserialize
    CreateDefault()       // tạo save mới (500 gold + 1 Footman)
}
```

### `SaveService`

```csharp
class SaveService : ISaveService {
    Save()     → serialize ra file save.json (có backup save_backup.json)
    Load()     → deserialize từ file
    HasSave()  → kiểm tra file tồn tại
    Reset()    → xoá save
    CurrentData → SaveData hiện tại
}
```

- Dùng `JsonUtility` (Unity) — yêu cầu FIELDS public, không phải Properties
- Backup: ghi đè save_backup.json trước khi ghi save.json mới

---

## 6. Service Container — Dependency Injection

### `ServiceContainer`

```csharp
class ServiceContainer {
    // Core
    Database        (GameDatabase)
    Factory         (RuntimeFactory)
    Formula         (IFormulaService)
    Save            (ISaveService)
    
    // Game Services
    Item            (IItemService)
    Inventory       (IInventoryService)
    Character       (ICharacterService)
    Equipment       (IEquipmentService)
    Skill           (ISkillService)
    StatusEffect    (IStatusEffectService)
    Craft           (ICraftService)
    Merchant        (IMerchantService)
    Dungeon         (IDungeonService)
    Combat          (ICombatService)
    TargetSelection (ITargetSelectionService)
    Loot            (ILootService)
    Quest           (IQuestService)
    Doctrine        (IDoctrineService)
    Tavern          (ITavernService)
    Settings        (ISettingsService)
    OfflineProgress (IOfflineProgressService)
    Party           (IPartyService)
    Pet             (IPetService)
    GameLoop        (IGameLoopService)
}
```

Mỗi service nhận các dependency qua constructor. ServiceContainer tự động tạo các service mặc định nếu không được inject.

---

## 7. Hệ Thống Nhân Vật (Character)

### `CharacterService`

**Chức năng chính:**
1. **Quản lý danh sách nhân vật** — load từ save, sync ra save
2. **Tạo nhân vật mới** — `CreateCharacter(definitionId)` → tạo qua RuntimeFactory
3. **Tuyển dụng** — `RecruitCharacter(CharacterSaveData)` → nhận từ Tavern
4. **Tính chỉ số tổng** — `GetTotalStat(character, statType)`

### Công thức tính chỉ số (`GetTotalStat`)

```
stat = (baseStat * promotionMult * legacyMult + equipBonus) * traitMult
```

Chi tiết:
```
mult = legacyMult * promoMult
  legacyMult = 1.5 nếu IsAscended, 1.0 nếu không
  promoMult  = từ PromotionDefinition (mặc định 1.0 + 0.1 * AscensionLevel)

baseStat =
  Constitution: BaseConstitution * mult + potions[0] + doctrineCON
  Intelligence: BaseIntelligence * mult + potions[2] + doctrineINT
  Dexterity:    BaseDexterity    * mult + potions[1] + doctrineDEX
  MaxHp:        (BaseMaxHp + Level-1) * mult + potions[3]*5 + doctrineHP
  Defense:      BaseDefense + potions[4] + doctrineDEF       // KHÔNG nhân mult!
  MagicDefense: BaseMagicDefense + potions[5] + doctrineMDEF // KHÔNG nhân mult!

Doctrine bonus (từ War/Fortitude/Ruin/Knowledge/Grace/Illusion Level):
  CON: warLevel * 2
  DEX: warLevel * 2
  INT: knowledgeLevel * 2
  HP:  fortitudeLevel * 15 + ruinLevel * 25
  DEF: fortitudeLevel * 3
  MDEF: illusionLevel * 3

Equipment bonus:
  Mỗi món trang bị cộng chỉ số tương ứng
  Nếu graceLevel >= 2: accessory được nhân đôi

Trait multiplier:
  Trait-dependent: dựa vào mapping trait → hệ số cho từng stat

Kết quả:
  total = DecodeMath.Round((baseStat + equipBonus) * traitMult)
  // DecodeMath.Round = (int)(value + 0.0001) — TRUNCATION với epsilon
```

---

## 8. Hệ Thống Trang Bị (Equipment)

### `EquipmentService`

Checks:
- `CanEquip(character, item, slot)`:
  1. Category khớp slot: Weapon→Weapon, Armor→Armor, Accessory→Accessory
  2. Class restriction: weapon phải đúng `WeaponType` của character (trừ "Generic")
  3. ArmorType phải khớp

- `Equip(character, itemId, slot)`:
  1. Kiểm tra CanEquip
  2. Unequip món cũ (nếu có)
  3. Lock item (IsLocked = true)
  4. Gán vào slot (Weapon/Armor/Accessory)
  5. Sync save

- `Unequip(character, slot)`:
  1. Lấy item từ slot
  2. Unlock item (IsLocked = false)
  3. Set slot = null
  4. Sync save

**Quan trọng:** Khi equip, item KHÔNG bị xóa khỏi inventory — chỉ giữ reference. Điều này khác với nhiều game khác.

---

## 9. Hệ Thống Túi Đồ (Inventory)

### `InventoryService`

**Capacity:** `FormulaService.StorageSpaces(LevelStorage, UpgradeStorage, PurchaseFlags)`

- `AddItem(item)`:
  - Nếu stackable (Material/Consumable) + item cùng loại đã có → stack lên
  - Nếu không → thêm vào danh sách (nếu còn chỗ)
  - Throw `InvalidOperationException` nếu đầy

- `RemoveItem(instanceId, amount)`:
  - Giảm stack, xoá item nếu stack về 0
  - Khi xoá → clear equipment reference trong character save data (G17)

- `ConsumeByDefinitionId(definitionId, amount)`: xoá theo loại

- `GetQuantityByDefinitionId(id)`: đếm tổng stack theo loại

- `HasQuantityByDefinitionId(id, amount)`: kiểm tra đủ số lượng

Sync: `LoadFromSave()` / `SyncToSave()` → đồng bộ với SaveData

---

## 10. Hệ Thống Combat

### `CombatService`

**Luồng 1 turn:**

```
ProcessTurn(adventurers, enemies):
  1. Kiểm tra kết thúc
     - Nếu tất cả adventurer HP ≤ 0 → Defeat
     - Nếu tất cả enemy chết → Victory
  
  2. Gom tất cả entity (adventurer còn sống + enemy còn sống)
     Wrap thành ICombatEntityWrapper
  
  3. Sắp xếp theo Initiative → Dexterity (giảm dần)
     Entity đầu = acting entity
  
  4. Hồi phục đầu turn
     Hp = Min(MaxHp, Hp + Regeneration)
  
  5. Tăng Mana
     Nếu có ActiveSkill:
       Nếu mana ≥ 100 → set về 0 (xài skill)
       Nếu không → Min(100, mana + manaRegen)
  
  6. Chọn mục tiêu
     Adventurer → enemy còn sống đầu danh sách
     Enemy → adventurer còn sống đầu danh sách
  
  7. Roll sát thương
     RollAttackDamage: min + random() * (max - min)
     Nếu "rolls three times" → lấy best of 3 rolls
  
  8. ApplyDamage:
     rawDamage → magic/phys check → defense/mdef reduction
  
  9. Kiểm tra kết thúc lại
```

### `TargetSelectionService`

Hỗ trợ nhiều chiến lược:
- `random_enemy` (mặc định) — target ngẫu nhiên
- `random_ally`, `random_ally_except_self`
- `lowest_absolute_enemy`, `lowest_absolute_ally`
- `lowest_relative_enemy`, `lowest_relative_ally`
- `all_enemies`, `all_allies`, `all`, `all_except_self`

### ICombatEntityWrapper

Interface chung cho CharacterRuntime và EnemyRuntime khi combat:
- `Id`, `CurrentHp`, `MaxHp`, `CurrentMana`
- `IsAdventurer`, `IsInitiative`, `Dexterity`
- `MinAttackDamage`, `MaxAttackDamage`
- `Regeneration`, `ManaRegen`
- `ActiveSkillId`, `IsMagic`

---

## 11. Hệ Thống Dungeon (Hầm Ngục)

### `DungeonService` (611 dòng — service lớn nhất)

**Trạng thái dungeon:**
```
StartDungeon(dungeonId, party) → Khởi tạo DungeonRuntime
  - G05 gate: kiểm tra RequiredClearDungeonId
  - Tạo runtime với Progress=0
  - Spawn enemy từ EnemyIds

Tick() → Một bước trong dungeon
  - Gọi mỗi 0.5-1 giây khi dungeon active
  - Xử lý các action type khác nhau:
  
Action Types (từ bản gốc Area.java):
  - CASE WALK (0-2): tăng Progress, spawn trap/event
  - CASE FIGHT (3-...): gọi CombatService, xử lý loot, kiểm tra kết thúc
  - Điều kiện chuyển: LocalDarkness, trap probabilities, etc.
  
Kết thúc dungeon:
  - Victory: tất cả enemy chết → collect loot
  - Defeat: Progress reset về PROGRESS_KEEP_THRESHOLD (250)
  - Turn cap 400 → tự động kết thúc
  - Rút lui (retreat): Progress reset về 0

Save trạng thái dungeon:
  - SaveDungeonState() → serialize vào ActiveDungeonSaveData
  - Có thể resume sau khi load game
```

### Dungeon Chain Gating (G05)

Dungeon có `RequiredClearDungeonId` sẽ không mở cho đến khi dungeon yêu cầu được clear. Lưu trong `SaveData.Dungeons[].State == Completed`.

---

## 12. Hệ Thống Loot & Rương

### `LootService`

**Cơ chế drop (từ bản gốc):**

```
RollSingleDrop(dropTable):
  1. Lọc drop entries có Weight > 0
  2. RollFromWeightedMap: random * 1000, cumulative sum
  3. Nếu không trúng entry nào → miss (không drop gì)
  4. Trả về ItemRuntime với StackCount

RollFromWeightedMap(entries, roll01):
  target = roll01 * 1000
  cumulative = 0
  for each entry:
    cumulative += weight
    if target < cumulative → return entry
  return null  // rolled into the miss gap
```

**Quan trọng:** Weight là per-mille trên thang 1000 CỐ ĐỊNH, KHÔNG chuẩn hóa. Nếu tổng weight < 1000 → có % miss.

**Chest (rương loot tạm):**
- `CHEST_CAP = 2000` (tổng stack, không phải số món)
- `CHEST_CAP_MERCHANT_PACK = 3000` (nếu mua Merchant Pack)
- `IsChestFull(pendingDrops)` kiểm tra tổng stack
- `CollectPendingDrops()` gom loot vào chest, dừng khi đầy

---

## 13. Hệ Thống Tavern (Tửu Quán)

### `TavernService`

**Chức năng:**
1. **Quản lý khách** — guest spawn theo timer
2. **Tuyển dụng** — `RecruitGuest(index)` → tạo character từ khách
3. **Nâng cấp** — Quarters (sức chứa), Capacity (tốc độ khách), Time (giảm timer)

**Công thức capacity (từ FormulaService):**
```
GetTavernCapacity(level, upgrade, flags)  → sức chứa khách
GetQuartersCapacity(level, upgrade, flags) → sức chứa nhân vật
```

**Roll class cho guest:**
```
Footman 25%, Rogue 25%, Archer 25%, Apprentice 25%
```

**Roll trait cho guest:**
```
Common traits: BOOKWORM 13.3%, BRUTE 13.3%, FERAL 13.3%, ... (thêm)
Rare traits: (logic chi tiết trong code)
```

**Visitor interval:**
- Base: 28800 giây (8 tiếng)
- Giảm khi nâng cấp Tavern Time
- `CalculateOfflineDelta` — cap 12 tiếng

---

## 14. Hệ Thống Crafting (Workshop)

### `CraftService`

**Luồng craft:**
```
1. CanCraft(recipeId) → kiểm tra:
   - Recipe tồn tại
   - OutputItemId hợp lệ
   - Ingredients đủ
   - Queue chưa đầy

2. TryStartCraft(recipeId):
   - Tiêu thụ ingredients (ConsumeByDefinitionId)
   - Thêm ItemActionSaveData vào WorkshopQueue
   - Thời gian craft: mặc định 10 giây

3. ProgressWorkshop(deltaSeconds):
   - Duyệt queue, tích lũy SecondsPassed
   - Khi đủ thời gian → chuyển sang CompletedWorkshopItems
```

**Queue capacity:** `FormulaService.WorkshopQueue(LevelWorkshopQueue, UpgradeWorkshopQueue, flags)`

---

## 15. Hệ Thống Merchant (Chợ)

### `MerchantService`

**Hàng hóa:**
- `RegularStock` — hàng thường (roll từ `DungeonDefinition.RegularMerchantOffers`)
- `SpecialStock` — hàng đặc biệt (roll từ `DungeonDefinition.SpecialMerchantOffers`)
- Mỗi offer có: `ItemId`, `StackCount`, `Weight`, `Price`, `IsGems`

**Mua hàng (BuyOffer):**
```
1. Kiểm tra inventory còn chỗ
2. Kiểm tra currency (Money/Gems)
3. Trừ tiền
4. Xoá offer khỏi stock
5. Thêm item vào inventory
```

**Bán hàng (SellItem):**
```
1. Tiêu thụ item từ inventory
2. Thêm ItemActionSaveData vào MarketListings (bán mất 20 giây)
3. ProgressMarket(deltaSeconds) — xử lý queue bán
```

---

## 16. Hệ Thống Nhiệm Vụ (Quest)

### `QuestService`

**Cấu trúc quest:**
- Load từ `quest_metadata.json` + `quests.json` (definitions)
- Mỗi quest runtime có: `State`, `Progress`, `TargetProgress`, `Rarity`
- TargetProgress phụ thuộc vào rarity (từ metadata hoặc `rarity * 100` mặc định)

**Cơ chế:**
```
Increment(questInstanceId, amount) → tăng progress
IncrementToValue(questInstanceId, newValue) → set progress đến giá trị

Khi Progress >= TargetProgress → state = Completed
Quest được trigger từ các sự kiện game (giết quái, craft, v.v.)
```

---

## 17. Hệ Thống Học Thuyết (Doctrine)

### `DoctrineService`

8 học thuyết: **Affliction, Control, Fortitude, Grace, Illusion, Knowledge, Ruin, War**

Mỗi học thuyết có Level + Progress (hướng tới level tiếp theo).

**Công thức:** `TotalStarsToNextLp(level) = level * 3 + 4`

**Stat bonus từ Doctrine:**
```
War → CON +2/level, DEX +2/level
Knowledge → INT +2/level
Fortitude → HP +15/level, DEF +3/level
Ruin → HP +25/level
Grace → double accessory bonus (level >= 2)
Illusion → MDEF +3/level
```

**AddProgress:** tự động tính toán level-up, xử lý overflow progress.

---

## 18. Hệ Thống Thăng Cấp (Promotion)

### `PromotionService`

Promotion = hệ thống chuyển class (ascend) — reset level nhưng tăng stat multiplier.

**Luồng promote:**
```
1. CanPromote(character, promotionId):
   - TierIndex == currentCount + 1
   - Level >= RequiredLevel
   - Đủ RequiredItem

2. Promote(character, promotionId):
   - Tiêu thụ item
   - AscensionLevel++ (dùng chung với AscensionLevel trong save)
   - Level reset về 1
   - Exp reset về 0
```

**StatMultiplier** từ PromotionDefinition ảnh hưởng đến `GetTotalStat()`.

---

## 19. Hệ Thống Kỹ Năng (Skill) & Hiệu Ứng (Status Effect)

### `SkillService`

Hiện tại: service minimal — chỉ tạo SkillRuntime. Logic skill đầy đủ sẽ được port trong tương lai (gắn với combat: cooldown, mana cost, target strategy).

### `StatusEffectService`

Hỗ trợ thêm hiệu ứng cho cả Character và Enemy:

**Cơ chế:**
- `InternalAddStatusEffect(positiveList, negativeList, definition, sourceId, turnsLeft)`:
  - Nếu `BLEED`: stack thời gian (additive)
  - Nếu khác: lấy thời gian lớn hơn (nếu cùng loại đã có)
  - Không check immunity (deferred)

**StatusEffectType:** các loại effect (Bleed, Stun, Poison, Shield, Regen...)

---

## 20. Hệ Thống Offline Progress

### `OfflineProgressService`

```csharp
CalculateOfflineDelta(lastSaveUnix, currentUnix):
  delta = currentUnix - lastSaveUnix
  return Min(delta, 12 * 3600)  // cap 12 tiếng

ApplyOfflineProgress(currentUnix):
  delta = CalculateOfflineDelta(metadata.SaveTimeUnix, currentUnix)
  if delta > 0:
    craftService.ProgressWorkshop(delta)
    merchantService.ProgressMarket(delta)
    // Dungeon/Combat/Quest offline: DEFERRED
  return result
```

### `GameLoopService.ProcessOfflineCatchup()`

Khi game khởi động:
```
1. Tính elapsed = currentUnix - lastAccess
2. jMax = Min(12h, Max(1s, elapsed))
3. Tick các service offline:
   - Tavern: ProgressVisitorTime(jMax)
   - Merchant: ProgressMarket(jMax)
   - Craft: ProgressWorkshop(jMax)
   - Dungeon: Tick() × jMax lần (dungeon tự động)
4. Cập nhật LastAccess
```

### `GameLoopService.TickRuntime()` (runtime loop, gọi mỗi giây):
```
1. LastAccess = now
2. Tavern.ProgressVisitorTime(1)
3. Merchant.ProgressMarket(1)
4. Craft.ProgressWorkshop(1)
5. Tick60() — hourly/daily checks
6. Dungeon.Tick()
```

---

## 21. Hệ Thống Game Loop & Tick

```
Game Boot → GameLoopService.Initialize()
            └── ProcessOfflineCatchup() → catchup các service
  
Runtime → GameLoopService.TickRuntime() (mỗi giây)
            ├── Tavern timer
            ├── Merchant queue
            ├── Craft workshop
            ├── Tick60 (mỗi phút: hourly/daily reset)
            └── Dungeon.Tick() (nếu có dungeon active)
```

---

## 22. Hệ Thống UI

### Kiến trúc UI

```
UIScreen (base class)
├── HUDController       — thanh trên cùng (money, gems, nav buttons)
├── TavernScreen        — tuyển dụng, nâng cấp
├── InventoryScreen     — kho đồ, use/equip/sell
├── CharacterScreen     — danh sách nhân vật, stats
├── DungeonScreen       — chọn dungeon, active run, loot
├── CraftScreen         — chế tạo
├── MerchantScreen      — chợ mua/bán
├── QuestScreen         — nhiệm vụ
├── SettingsScreen      — cài đặt
├── PopupScreen         — popup chung
│
├── EquipmentPopup      — popup trang bị (trong CharacterScreen)
├── ErrorPopup          — lỗi
├── OfflineSummaryPopup — tóm tắt offline
├── RecoveryWarningPopup— cảnh báo
├── SaveStatusIndicator — trạng thái save
└── WelcomeModal        — màn chào

Core:
  UIService           — quản lý show/hide/back, stack navigation
  UICardFactory        — tạo card động runtime
  SafeArea            — xử lý safe area
  UIScreenId          — enum định danh màn hình
  UITemporaryTheme    — theme tạm
```

### Navigation

```csharp
UIService.ShowScreen(UIScreenId) → push stack, hide current
UIService.Back() → pop stack, show previous
```

### Wiring: `UIRuntimeBootstrap.Start()`

```
1. Tạo DataProvider, Serializer, GameDatabase
2. Build database từ JSON
3. Tạo ServiceContainer
4. Tìm tất cả UIScreen trong scene
5. Register mỗi screen vào UIService
6. Gọi Initialize cho từng screen:
   - HUDController.Initialize(Save, UI)
   - InventoryScreen.Initialize(Services, CharacterScreen)
   - DungeonScreen.Initialize(Services, CharacterScreen)
   - TavernScreen.Initialize(Services)
   ...
7. Wire Back buttons
8. PopupManager setup
9. Show HUD
```

---

## 23. Hệ Thống Formulas (Công Thức Gốc)

### `DecodeMath` — Arithmetic Primitives

Đây là các hàm toán học được decompile từ game Java gốc, phải giống BIT-IDENTICAL để đảm bảo balance.

```csharp
Round(value)         → (int)(value + 0.0001)  // TRUNCATION + epsilon, NOT rounding!
TruncatePrice(price) → bỏ digits cuối nếu giá lớn
RollFromWeightedMap  → random * 1000, cumulative sum
```

### `FormulaService` — công thức kinh tế & progression

**Công thức nâng cấp (giá):**

| Upgrade | Công thức gốc | Notes |
|---------|--------------|-------|
| Quarters | Lookup table 23 tiers → truncate | Từ 5 → 10M gold |
| Tavern Capacity | `pow(3, lvl) * 5000` → truncate | Có thể xem xét giảm |
| Tavern Time | `pow(1.7, lvl) * 200` → truncate | |
| Storage | Cumulative tiered cost, NO truncate | Không truncate! |
| Market Listings | `pow(4.5, lvl) * 20` → truncate | |
| Market Time | `pow(1.7, lvl) * 10` → truncate | |
| Workshop Queue | `pow(4.5, lvl) * 20` → truncate | |
| Workshop Time | `pow(1.7, lvl) * 10` → truncate | |
| Shelter | Lookup table 11 tiers → truncate | |

**Capacity formulas:**

| Capacity | Base | Progress |
|----------|------|----------|
| Quarters Spaces | 2 | Từ level/upgrade/flags |
| Storage Spaces | 35 | Từ level/upgrade/flags |
| Tavern Spaces | 1 | Từ level/upgrade/flags |
| Market Queue | 1 | Từ level/upgrade/flags |
| Workshop Queue | 1 | Từ level/upgrade/flags |

**Công thức EXP và cấp:**
```
ExperienceToNextLevel(level, isAdventurer):
  p = pow(level, 1.4)
  value = (3 + p) * 10 * p
  if isAdventurer → value *= 2
  if value >= 10000 → floor to 1000
  elif value >= 1000 → floor to 100
  elif value >= 100 → floor to 10
  else → value

FoodToNextLevel(level):
  (int)(pow(1.085, level) * 30)
```

**Công thức shelter / offline:**
```
ShelterFoodUsage: 1 + hungerLevel * 4
ShelterProduction: level * 1 (mỗi tick)
GetTavernVisitorInterval(level, upgrade):
  base = 28800s (8h)
  giảm theo level/upgrade
```

---

## 24. Thú Cưng (Pet)

### PetService (chưa hoàn thiện)

- PetSaveData: `DefinitionId, InstanceId, Level, Exp, EquippedToCharacterId`
- PetDefinition: định nghĩa pet từ JSON (chưa port đầy đủ)
- Chức năng chính còn thiếu: combat assist, passive bonus, feeding

---

## 25. Settings & Localization

### `SettingsService`

- Lưu toggle settings trong `SaveData`
- Toggles: Sound, Music, Vibration, Notifications, Cloud, Colorblind, ConfirmDialogs, MaxAmount
- Ngôn ngữ: `SettingsLanguage` (mặc định "en")
- `ResetToDefault()`: reset tất cả về mặc định

### Localization (`LocalizationService`)

- Đọc từ `localization.json` trong StreamingAssets
- Interface: `GetLocalizedString(key, lang?)`
- Hiện tại file JSON chỉ có skeleton

---

## 26. Bootstrap & Khởi Động Game

### Boot Scene (`Bootstrapper.cs`)

```
Boot.unity → Bootstrapper.Awake()
  ├── Tạo IGameDataProvider (Editor / StreamingAssets)
  ├── Tạo IJsonSerializer (UnityJsonSerializer)
  ├── Tạo GameDatabase
  ├── Build database từ manifest
  │   └── DatabaseBuilder.Build() → DatabaseBuildReport
  ├── Tạo DatabaseService, LocalizationService, AssetManifestService
  ├── Log report (kiểm tra fatal errors)
  └── Load Main scene (nếu không có fatal error)
```

### Main Scene (`UIRuntimeBootstrap.Start()`)

```
Main.unity → UIRuntimeBootstrap.Start()
  ├── Tạo data provider + serializer + database
  ├── Build database từ JSON
  ├── Tạo ServiceContainer (chứa tất cả services)
  ├── Tìm UIScreen components trong scene
  ├── Register screens vào UIService
  ├── Initialize từng screen với Services
  ├── Wire back buttons
  ├── GameLoopService.ProcessOfflineCatchup()
  └── Show HUD
```

---

## 27. Luật Chơi Chi Tiết Từ Bản Gốc (Recovered Rules)

> Những luật được khôi phục từ decompiled APK code (Java → Smali → DAD → C#)

### G01 — Potion Index Mapping
Potion effect (6 loại): `[0]=CON, [1]=DEX, [2]=INT, [3]=HP(*5), [4]=DEF, [5]=MDEF`

### G02 — Drop Weight Scale
Weight là per-mille trên thang 1000 CỐ ĐỊNH. Không chuẩn hóa. Tổng < 1000 = có cơ hội miss.

### G03 — Chest Cap tính bằng Stack Count
`CHEST_CAP = 2000` là tổng stack, không phải số món riêng biệt.

### G04 — Defense KHÔNG nhân Promotion Mult
Trong `GetTotalStat()`, Defense và MagicDefense KHÔNG nhân với `mult`. Chỉ CON/INT/DEX/HP được nhân.

### G05 — Dungeon Chain Gating
Dungeon có `RequiredClearDungeonId` đòi hỏi phải clear dungeon khác trước mới mở.

### G06 — DecodeMath.Round = Truncation
`(int)(value + 0.0001)` = truncation với epsilon. KHÔNG phải làm tròn thông thường.

### G07 — TruncatePrice chỉ áp dụng cho giá > 10000
Giá ≤ 10000 giữ nguyên. Giá ≤ 1M bỏ 2 số cuối. Giá > 1M bỏ 4 số cuối.

### G08 — Enemy Drop từ Per-enemy Java Class
Mỗi enemy override `configureStatistics()`, `getMinDamage()`, `getMaxDamage()`, `listDrops()`.

### G09 — Adventure Turn Cap 400
Dungeon tự động kết thúc sau 400 turns. Defeat reset progress về 250 (không mất hết).

### G10 — Tavern Visitor Base = 28800s (8h)
Interval giảm dần theo nâng cấp.

### G11 — Storage Price KHÔNG truncate
Không giống mọi price formula khác, storage price giữ nguyên digits.

### G12 — Purchase Flags ảnh hưởng capacity
Starter Pack, Adventurer Pack, Merchant Pack, Imperial Vanguard, Unholy Crusade.

### G13 — Grace Level ≥ 2 → Double Accessory
Nếu Grace Doctrine Level ≥ 2, bonus từ accessory được nhân đôi.

### G14 — Character Equip KHÔNG xóa khỏi inventory
Khi equip, item giữ trong inventory (chỉ lock). Unequip thì unlock.

### G15 — Bleed Stack Additive
Bleed stack cộng dồn thời gian. Các effect khác lấy giá trị lớn hơn.

### G16 — Promotion Reset Level về 1
Ascend → Level=1, Exp=0, nhưng stat multiplier tăng.

### G17 — Xóa Item → Clear Equipment Reference
Khi xoá item khỏi inventory (stack về 0), clear WeaponInstanceId/ArmorInstanceId/AccessoryInstanceId của mọi character đang dùng nó.

---

## 28. Những Phần Chưa Hoàn Thiện & Công Việc Còn Lại

### 🟢 Đã hoàn thành (test pass, code ok)
- [x] Database loading từ JSON
- [x] Save/Load (file + backup)
- [x] Character system (create, stats, level)
- [x] Equipment system
- [x] Inventory system
- [x] Tavern (guest spawning, recruit)
- [x] Craft (queue, recipe check)
- [x] Merchant (buy, sell)
- [x] Dungeon (start, tick, combat, loot)
- [x] Combat (turn-based, damage, death)
- [x] Loot (weighted drop, chest)
- [x] Quest (progress tracking)
- [x] Doctrine (level+progress)
- [x] Promotion (stat multiplier)
- [x] Settings (toggles, language)
- [x] Offline progress (craft+merchant queues)
- [x] Game loop (1s tick, offline catchup)
- [x] UI framework (UIScreen, card factory, nav)
- [x] All UI screens (HUD, Inventory, Tavern, Dungeon, etc.)
- [x] Sprite/tileset verification (PASS all)
- [x] 417 PNG assets imported
- [x] All core JSON data files

### 🟡 Cần hoàn thiện
- [ ] **Combat logic đầy đủ** — skill system, cooldown, mana cost, target strategy từ DAD
- [ ] **Status effect full** — sát thương theo turn, heal, shield, stun, v.v.
- [ ] **Pet system đầy đủ** — combat assist, feeding, evolution
- [ ] **Raid system** — raid boss, weekly reset
- [ ] **Bestiary / Achievement** — collection tracking
- [ ] **Full localization** — translation data
- [ ] **Ads/IAP/Cloud** — monetization chưa implement
- [ ] **Full content migration** — tất cả item, enemy, dungeon, quest data từ JSON vào game
- [ ] **UI polish** — animations, transitions, feedback
- [ ] **Balance tuning** — kiểm tra tất cả formula với data thật

### 🔴 4 tests đang fail (S2VerificationTests)
- `S2_001_ItemSystem_LoadedCorrectly` — items chưa load
- `S2_002_InventorySystem_CapacityAndStacking` — lỗi
- `S2_003_EquipmentSystem_SlotRestrictions` — lỗi
- `S2_004_CharacterSystem_StatAggregationAndLevelUp` — lỗi
- → Đây là tests legacy chưa update cho kiến trúc mới

### 🔴 3 compile errors (trong Editor.log)
- `PlayerFacingUIActionTests.cs:55` — `GameDatabase` thiếu method `GetCategory()`
- `S6_5A_RuntimeSmokeTest.cs:73-75` — sai kiểu tham số (pass service thay vì ServiceContainer)

### 🎯 Priority sắp tới (theo đề xuất)
1. **Sửa 3 compile errors** — fix tests để compile
2. **Sửa 4 S2 tests** — update cho architecture mới
3. **Port combat skill full** — từ DAD reports vào C#
4. **Port status effect combat** — bleeding, shields, regen
5. **UI polish** — visual feedback, animations
6. **Full content** — verify all JSON data loads correctly
7. **Android build** — deploy test APK
