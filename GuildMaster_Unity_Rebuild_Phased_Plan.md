# KẾ HOẠCH DỰNG LẠI GUILD MASTER - IDLE DUNGEONS BẰNG UNITY

**Tên dự án:** Rebuild Guild Master  
**Nguồn tham chiếu/decode:** `D:\Tinh\Guild Master - Idle Dungeons`  
**Unity project mới:** `D:\Tinh\Rebuild_GuildMaster`  
**Môi trường thực hiện:** Unity + Antigravity + Unity MCP  
**Mục tiêu:** Dựng lại một game Unity chạy được, build được Android, sử dụng tối đa dữ liệu, tài nguyên, UI reference và logic có trong bản decode.

> Lưu ý pháp lý: chỉ sử dụng code, hình ảnh, âm thanh, tên gọi và nội dung nếu công ty bạn có quyền hợp pháp để dùng và phát hành.

---

# 1. KẾT QUẢ KIỂM TRA BẢN DECODE

Bản tại `D:\Tinh\Guild Master - Idle Dungeons` không phải Unity project. Đây là mã Android đã decompile, bao gồm Java source và Android resources.

## 1.1. Thông tin ứng dụng

- Package: `it.paranoidsquirrels.idleguildmaster`
- Version name: `2.147`
- Version code: `158`
- Android min SDK: `24`
- Android target/compile SDK: `35`
- Chế độ màn hình: portrait
- Có tích hợp Google Ads, Google Play Games, Billing/IAP và cloud snapshot.

## 1.2. Khối lượng dữ liệu đã thấy

| Nhóm | Số lượng gần đúng |
|---|---:|
| Tổng file | 17.404 |
| Java source | 14.482 |
| Java thuộc game chính | 1.150 |
| PNG | 1.051 |
| WebP | 5 |
| XML | 902 |
| Android layout | 201 |
| Item instance | 607 |
| Adventurer-related file | 132 |
| Enemy-related file | 122 |
| Dungeon | 11 |
| Raid | 12 |
| Quest | 56 |
| Pet | 21 |

## 1.3. Những khu vực decode quan trọng

```text
D:\Tinh\Guild Master - Idle Dungeons\
├── resources\
│   ├── AndroidManifest.xml
│   └── res\
│       ├── drawable\
│       ├── layout\
│       ├── values\
│       │   ├── strings.xml
│       │   ├── colors.xml
│       │   ├── dimens.xml
│       │   └── styles.xml
│       └── mipmap*
└── sources\
    └── it\paranoidsquirrels\idleguildmaster\
        ├── MainActivity.java
        ├── Formulas.java
        ├── Utils.java
        ├── UIUtils.java
        ├── IAPWrapper.java
        ├── storage\
        │   ├── FileManager.java
        │   ├── SaveManager.java
        │   └── data\
        │       ├── Data.java
        │       ├── entities\
        │       ├── items\
        │       ├── pets\
        │       ├── places\
        │       └── quests\
        └── ui\
```

## 1.4. UI chính đã thấy trong decode

Các màn hình chính:

- Headquarters
- Adventurers
- Dungeons
- Raids

Các popup/hệ thống phụ:

- Storage
- Tavern
- Workshop
- Market
- Merchant
- Shop
- Quarters
- Shelter
- Quests
- Bestiary
- Item Detail
- Entity Detail
- Dungeon Detail
- Send Team
- Collect Drops
- Idle Progress
- Recipes/Crafting
- Equipment selection
- Promotion
- Doctrine
- Pet detail/feeding/merge
- Settings
- Redeem code
- Raid refill

---

# 2. NGUYÊN TẮC THỰC HIỆN

## 2.1. Không sửa trực tiếp thư mục decode

`D:\Tinh\Guild Master - Idle Dungeons` chỉ được dùng để đọc và đối chiếu.

Không:

- đổi tên file;
- di chuyển file;
- chỉnh Java;
- chỉnh XML;
- xóa tài nguyên;
- đặt Unity project vào bên trong thư mục decode.

## 2.2. Mọi code Unity nằm ở thư mục mới

Toàn bộ sản phẩm mới phải nằm tại:

```text
D:\Tinh\Rebuild_GuildMaster
```

## 2.3. Không cố convert Java sang C# nguyên xi

Java decompile thường có:

- tên biến mất nghĩa;
- synthetic class;
- lambda class;
- UI và gameplay bị trộn;
- Android-specific API;
- lỗi do decompiler;
- code khó bảo trì.

Cách làm đúng:

1. Đọc Java để hiểu dữ liệu và luật.
2. Thiết kế model C# sạch.
3. Chép lại công thức và behavior cần thiết.
4. Dùng asset/XML như tài liệu tham chiếu.
5. Nghiệm thu từng module.

## 2.4. Ưu tiên bản chạy được trước

Thứ tự ưu tiên:

1. Unity project chạy.
2. Build Android được.
3. Dữ liệu load được.
4. Main navigation hoạt động.
5. Character, inventory, dungeon loop hoạt động.
6. Save/offline hoạt động.
7. Sau đó mới làm full combat, pet, doctrine, raid, shop và online services.

---

# 3. CẤU TRÚC THƯ MỤC DỰ ÁN MỚI

Tạo cấu trúc:

```text
D:\Tinh\Rebuild_GuildMaster\
├── Assets\
│   ├── _Game\
│   │   ├── Art\
│   │   │   ├── Backgrounds\
│   │   │   ├── Characters\
│   │   │   ├── Enemies\
│   │   │   ├── Items\
│   │   │   ├── Pets\
│   │   │   ├── Areas\
│   │   │   └── UI\
│   │   ├── Audio\
│   │   ├── Data\
│   │   │   ├── Definitions\
│   │   │   └── Runtime\
│   │   ├── Prefabs\
│   │   │   ├── UI\
│   │   │   ├── Cards\
│   │   │   └── Dialogs\
│   │   ├── Scenes\
│   │   │   ├── Boot.unity
│   │   │   └── Main.unity
│   │   ├── Scripts\
│   │   │   ├── Core\
│   │   │   ├── Data\
│   │   │   ├── Save\
│   │   │   ├── Inventory\
│   │   │   ├── Characters\
│   │   │   ├── Equipment\
│   │   │   ├── Areas\
│   │   │   ├── Combat\
│   │   │   ├── Economy\
│   │   │   ├── Buildings\
│   │   │   ├── Quests\
│   │   │   ├── Pets\
│   │   │   ├── Offline\
│   │   │   └── UI\
│   │   └── Settings\
│   └── StreamingAssets\
│       └── GameData\
├── Docs\
│   ├── Rebuild_Plan.md
│   ├── Progress.md
│   ├── Known_Issues.md
│   └── Decisions.md
├── ReverseReference\
│   └── README.md
├── Tools\
├── Reports\
└── Builds\
    └── Android\
```

Không copy toàn bộ decode vào `Assets`, vì Unity sẽ scan hàng chục nghìn file không dùng được.

---

# 4. TỔNG QUAN CÁC PHASE

| Phase | Tên | Kết quả chính |
|---:|---|---|
| 00 | Chuẩn bị project | Unity project, template và MCP hoạt động |
| 01 | Audit template Unity | Biết chính xác tận dụng được module nào |
| 02 | Audit decode | Có bản đồ source, data, UI, asset và logic |
| 03 | Chốt kiến trúc mới | Folder, scene, service và data model ổn định |
| 04 | Import tài nguyên | Sprite/background/UI asset dùng được trong Unity |
| 05 | Dựng data definitions | C# model cho item, character, enemy, area, quest, pet |
| 06 | Nhập content lõi | Một tập dữ liệu mẫu chạy được |
| 07 | Bootstrap và DataManager | Boot scene load toàn bộ config |
| 08 | Save/Load nền tảng | Local save, backup, migration |
| 09 | UI shell | Main scene, top bar, bottom navigation và popup |
| 10 | Inventory/Storage | Xem, stack, sort, filter, sell item |
| 11 | Adventurer | Character list, stats, EXP, level |
| 12 | Equipment | Equip/unequip và tính stat |
| 13 | Dungeon loop MVP | Chọn team, timer, hoàn thành, nhận thưởng |
| 14 | Loot/Reward | Drop table, weighted random, reward popup |
| 15 | Offline progress | Thoát game, quay lại và resolve tiến trình |
| 16 | Combat cơ bản | Turn/combat simulation có log |
| 17 | Combat đầy đủ | Skill, target, status effect, death |
| 18 | Headquarters/Buildings | Storage, quarters, tavern, workshop, market, shelter |
| 19 | Workshop/Crafting | Recipe, queue và timer |
| 20 | Tavern/Merchant/Market | Recruit, offer, buy/sell, refresh |
| 21 | Quest | Quest state, progress, refresh, claim |
| 22 | Promotion/Trait/Doctrine | Tiến hóa adventurer và hệ build |
| 23 | Pet | Own, feed, equip, merge, ability |
| 24 | Raid | Raid try, boss, stages, reward |
| 25 | Bestiary/Achievement | Collection, unlock, achievement |
| 26 | Localization/Settings | Text, format, audio, theme, vibration |
| 27 | Ads/IAP/Cloud | Tích hợp lại bằng Unity SDK nếu cần |
| 28 | Content migration đầy đủ | Đưa phần lớn content decode vào game |
| 29 | QA/Balance | Regression, economy, save, edge cases |
| 30 | Optimization | Memory, loading, atlas, pooling, mobile |
| 31 | Android Release | APK/AAB, signing, final build |

---

# PHASE 00 — CHUẨN BỊ UNITY PROJECT

## Mục tiêu

Tạo project mới tại `D:\Tinh\Rebuild_GuildMaster`, import template Unity có sẵn và xác nhận project build được trước khi chỉnh gameplay.

## Task

1. Mở Unity Hub.
2. Chọn Unity LTS phù hợp với template.
3. Tạo project:
   ```text
   D:\Tinh\Rebuild_GuildMaster
   ```
4. Nếu template là package/project khác, import theo đúng hướng dẫn của template.
5. Cài Unity MCP.
6. Kết nối Antigravity với project.
7. Tạo Git repository.
8. Thêm `.gitignore` chuẩn Unity.
9. Mở project, clear Console.
10. Chạy Play Mode.
11. Tạo thử Android build rỗng.

## Bắt buộc kiểm tra

- Không có compile error.
- MCP đọc được scene hierarchy.
- MCP tạo được một GameObject thử nghiệm.
- MCP đọc được Console.
- Template chạy được.
- Project có thể đóng/mở lại không lỗi.
- Android platform switch thành công.

## Output

```text
D:\Tinh\Rebuild_GuildMaster\Assets
D:\Tinh\Rebuild_GuildMaster\Packages
D:\Tinh\Rebuild_GuildMaster\ProjectSettings
D:\Tinh\Rebuild_GuildMaster\.git
```

## Không làm trong phase này

- Không import decode.
- Không viết combat.
- Không làm UI game.
- Không đổi kiến trúc template lớn.
- Không cài package không cần thiết.

## Nghiệm thu

- [ ] Unity project mở được.
- [ ] Template chạy được.
- [ ] MCP hoạt động.
- [ ] Console không có lỗi đỏ.
- [ ] Build thử Android thành công.

---

# PHASE 01 — AUDIT TEMPLATE UNITY CÓ SẴN

## Mục tiêu

Xác định phần nào của template tái sử dụng được và phần nào phải viết mới.

## Cần kiểm tra

### Core

- Bootstrap/GameManager
- Scene loading
- Service locator hoặc dependency injection
- Event bus
- Coroutine/task helper
- Object pooling

### Gameplay

- Character stats
- Inventory
- Equipment
- Mission
- Quest
- Combat
- Timers
- Offline rewards
- Currency
- Shop
- Crafting
- Save/load

### UI

- Navigation
- Popup system
- Scroll list
- Card prefab
- Toast
- Confirmation dialog
- Loading overlay
- Safe area
- Localization binding

### Build

- Android package
- Build settings
- Input system
- Resolution/orientation
- Addressables
- Unity IAP/Ads package

## Tạo báo cáo

```text
D:\Tinh\Rebuild_GuildMaster\Reports\Template_Audit.md
```

Bảng bắt buộc:

| Module | Có sẵn | Dùng nguyên | Cần adapter | Viết lại | Ghi chú |
|---|---|---|---|---|---|
| Save |  |  |  |  |  |
| Inventory |  |  |  |  |  |
| Character |  |  |  |  |  |
| Equipment |  |  |  |  |  |
| Mission/Dungeon |  |  |  |  |  |
| Combat |  |  |  |  |  |
| Quest |  |  |  |  |  |
| Shop |  |  |  |  |  |
| Crafting |  |  |  |  |  |
| UI navigation |  |  |  |  |  |

## Quyết định cuối phase

Mỗi module phải được gắn một nhãn:

- `REUSE`
- `ADAPT`
- `REPLACE`
- `NEW`

## Nghiệm thu

- [ ] Có `Template_Audit.md`.
- [ ] Không còn module chưa được đánh giá.
- [ ] Biết module nào của template là nền tảng chính.
- [ ] Có danh sách package cần giữ.
- [ ] Có danh sách package nên loại bỏ.

---

# PHASE 02 — AUDIT BẢN DECODE

## Mục tiêu

Biến bản decode thành tài liệu tham chiếu có tổ chức.

## Nguồn bắt buộc đọc

```text
D:\Tinh\Guild Master - Idle Dungeons\resources\AndroidManifest.xml
D:\Tinh\Guild Master - Idle Dungeons\resources\res\layout
D:\Tinh\Guild Master - Idle Dungeons\resources\res\drawable
D:\Tinh\Guild Master - Idle Dungeons\resources\res\values\strings.xml
D:\Tinh\Guild Master - Idle Dungeons\sources\it\paranoidsquirrels\idleguildmaster
```

## Nhóm source gameplay cần map

### Core/save

- `MainActivity.java`
- `Formulas.java`
- `Utils.java`
- `TrueTimeUtils.java`
- `storage\FileManager.java`
- `storage\SaveManager.java`
- `storage\data\Data.java`
- `storage\data\DataDeserializer.java`

### Entity/combat

- `entities\Entity.java`
- `entities\Skills.java`
- `entities\StatusEffect.java`
- `entities\StatusEffectType.java`
- `entities\EndOfTurnAction.java`
- `adventurers\Adventurer.java`
- enemy base classes

### Item/equipment

- `items\Item.java`
- `items\ItemWrapper.java`
- `items\ItemAction.java`
- `items\Recipes.java`
- `items\MerchantOffer.java`
- `items\abstractClasses\*`
- `items\instances\*`

### Areas

- `places\Area.java`
- `places\Action.java`
- `places\Event.java`
- `places\AdventureRecap.java`
- `places\Logger.java`
- `places\dungeons\*`
- `places\raids\*`

### Quest/pet

- `quests\Quest.java`
- `quests\QuestsManager.java`
- `quests\instances\*`
- `pets\Pet.java`
- `pets\PetAbility.java`
- `pets\instances\*`

## Báo cáo cần tạo

```text
Reports\
├── Decode_Module_Map.md
├── Decode_UI_Map.md
├── Decode_Data_Map.md
├── Decode_Formula_Map.md
└── Decode_Asset_Map.md
```

## Nội dung Decode_Module_Map

| Java/source | Unity module mới | Mức ưu tiên | Ghi chú |
|---|---|---:|---|
| Data.java | SaveData + runtime state | Critical | Không copy 1:1 |
| Area.java | AreaRuntime + CombatResolver | Critical | Logic lớn, cần tách |
| Recipes.java | RecipeDatabase | High | Data-driven |
| QuestsManager.java | QuestManager | High | Sau MVP |
| MainActivity.java | Bootstrap/UI orchestration | Medium | Không port Android code |

## Nghiệm thu

- [ ] Biết mỗi package phục vụ hệ thống gì.
- [ ] Có danh sách class gameplay quan trọng.
- [ ] Có danh sách asset game-specific.
- [ ] Có danh sách màn hình và popup.
- [ ] Không nhầm thư viện Android với source game.

---

# PHASE 03 — CHỐT KIẾN TRÚC UNITY

## Mục tiêu

Thiết kế project mới dựa trên template, không phụ thuộc cấu trúc Android cũ.

## Scene

### Boot.unity

Trách nhiệm:

- init service;
- load static data;
- load local save;
- chạy migration;
- tính offline progress;
- chuyển sang Main.

### Main.unity

Chứa:

- top currency bar;
- screen container;
- bottom navigation;
- popup layer;
- toast layer;
- loading layer.

Không tạo một scene riêng cho từng popup.

## Service đề xuất

```text
GameBootstrap
GameDataService
SaveService
TimeService
InventoryService
AdventurerService
EquipmentService
AreaService
CombatService
RewardService
QuestService
PetService
EconomyService
BuildingService
OfflineProgressService
UIService
AudioService
```

Nếu template đã có service tương đương thì dùng adapter, không tạo bản trùng.

## Quy tắc dependency

```text
UI
 ↓
Application Services
 ↓
Runtime Models
 ↓
Static Definitions
 ↓
Save Repository
```

Không cho:

- definition gọi UI;
- save model chứa MonoBehaviour;
- ScriptableObject giữ runtime state;
- popup tự sửa file save;
- combat phụ thuộc prefab UI.

## Nghiệm thu

- [ ] Có sơ đồ service.
- [ ] Có folder structure.
- [ ] Có scene responsibilities.
- [ ] Có dependency rules.
- [ ] Không có hai manager cùng một chức năng.

---

# PHASE 04 — IMPORT VÀ PHÂN LOẠI ASSET

## Mục tiêu

Sử dụng các hình ảnh hữu ích trong decode làm sprite/background/UI reference trong Unity.

## Nguồn

```text
D:\Tinh\Guild Master - Idle Dungeons\resources\res\drawable
D:\Tinh\Guild Master - Idle Dungeons\resources\res\mipmap*
```

## Không copy toàn bộ drawable

Trong `drawable` có cả asset của AndroidX/Material. Chỉ lấy file game-specific.

Loại bỏ các prefix phổ biến:

```text
abc_
design_
mtrl_
material_
notification_
test_
googleg_
common_
```

## Phân loại

- Item icon
- Adventurer icon
- Enemy icon
- Pet icon
- Dungeon/Raid image
- Building image
- Currency icon
- Skill/status icon
- UI panel/button/background
- App icon

## Unity import setting

### Icon

- Texture Type: Sprite
- Sprite Mode: Single
- Alpha Is Transparency: On
- Mip Maps: Off
- Compression: None hoặc High Quality
- Max Size: phù hợp kích thước gốc

### Background

- Texture Type: Sprite hoặc Default theo template
- Mip Maps: Off với UI
- Compression: Android ASTC/ETC2 khi release

## Output

```text
Assets\_Game\Art\
├── Items
├── Characters
├── Enemies
├── Pets
├── Areas
├── Buildings
└── UI
```

## Nghiệm thu

- [ ] Không có Android library asset lẫn vào game asset.
- [ ] Sprite không bị mờ bất thường.
- [ ] Transparency đúng.
- [ ] Có placeholder khi thiếu sprite.
- [ ] Tên file ổn định và không trùng.

---

# PHASE 05 — TẠO STATIC DATA DEFINITIONS

## Mục tiêu

Tạo model C# sạch để biểu diễn content decode.

## Data types bắt buộc

```text
ItemDefinition
AdventurerDefinition
EnemyDefinition
SkillDefinition
StatusEffectDefinition
DungeonDefinition
RaidDefinition
QuestDefinition
PetDefinition
RecipeDefinition
BuildingDefinition
MerchantOfferDefinition
LocalizationEntry
```

## Ví dụ ItemDefinition

```csharp
[Serializable]
public sealed class ItemDefinition
{
    public string Id;
    public string DisplayNameKey;
    public string DescriptionKey;
    public string IconId;
    public ItemCategory Category;
    public ItemRarity Rarity;
    public long BasePrice;
    public StatModifierDefinition[] StatModifiers;
    public string[] Tags;
}
```

## Ví dụ AreaDefinition

```csharp
[Serializable]
public sealed class AreaDefinition
{
    public string Id;
    public AreaType AreaType;
    public string DisplayNameKey;
    public string DescriptionKey;
    public string ImageId;
    public int TeamSize;
    public int RequiredPower;
    public int DurationSeconds;
    public EncounterDefinition[] Encounters;
    public DropEntryDefinition[] Drops;
}
```

## Chọn định dạng

Khuyến nghị deadline:

- JSON trong `StreamingAssets/GameData`;
- C# DTO để deserialize;
- không tạo hàng trăm ScriptableObject thủ công ở giai đoạn đầu.

Có thể chuyển sang ScriptableObject sau khi game ổn định.

## Nghiệm thu

- [ ] Có ID duy nhất.
- [ ] Runtime state tách khỏi definition.
- [ ] Có validation ID/sprite/reference.
- [ ] Data có thể load mà không phụ thuộc UI.
- [ ] Không hard-code tên item trong gameplay.

---

# PHASE 06 — NHẬP DATA MẪU ĐỂ CHẠY MVP

## Mục tiêu

Không nhập toàn bộ 607 item ngay. Tạo một lát cắt nhỏ đại diện cho toàn bộ hệ thống.

## Data mẫu

- 20 item
- 5 adventurer class
- 10 enemy
- 2 dungeon
- 1 raid placeholder
- 5 quest
- 3 pet
- 10 recipe
- 5 skill
- 5 status effect

## Tiêu chí chọn mẫu

Phải có đủ:

- weapon;
- armor;
- accessory;
- consumable;
- material;
- common/rare item;
- physical/magic adventurer;
- normal/boss enemy;
- healing/damage/buff/debuff skill.

## Output

```text
Assets\StreamingAssets\GameData\
├── items.json
├── adventurers.json
├── enemies.json
├── skills.json
├── status_effects.json
├── dungeons.json
├── raids.json
├── quests.json
├── pets.json
└── recipes.json
```

## Nghiệm thu

- [ ] JSON hợp lệ.
- [ ] Không có duplicate ID.
- [ ] Không có missing reference.
- [ ] Sprite mapping hoạt động.
- [ ] Dữ liệu đủ để chạy loop dungeon đầu tiên.

---

# PHASE 07 — BOOTSTRAP VÀ DATA MANAGER

## Mục tiêu

Unity khởi động, load static data và cung cấp lookup cho toàn game.

## Flow

```text
Launch
→ GameBootstrap
→ Load definitions
→ Validate definitions
→ Load save
→ Apply migration
→ Resolve offline time
→ Open Main scene
```

## API tối thiểu

```csharp
GetItem(string id)
GetAdventurer(string id)
GetEnemy(string id)
GetDungeon(string id)
GetRaid(string id)
GetQuest(string id)
GetPet(string id)
GetRecipe(string id)
```

## Error handling

- ID không tồn tại → log error + placeholder.
- JSON lỗi → dừng boot bằng error panel.
- Sprite thiếu → default sprite.
- Data version không tương thích → migration hoặc báo lỗi rõ.

## Nghiệm thu

- [ ] Boot scene load thành công.
- [ ] Console in đúng số lượng definition.
- [ ] Main scene chỉ mở sau khi load xong.
- [ ] Không dùng `Resources.Load` rải rác khắp project.
- [ ] Có data validation report.

---

# PHASE 08 — SAVE/LOAD NỀN TẢNG

## Mục tiêu

Tạo save local ổn định, có backup và hỗ trợ update phiên bản.

## SaveData tối thiểu

```text
Version
LastSaveUnixTime
Money
Gems
Inventory
OwnedAdventurers
EquippedItems
DungeonStates
RaidStates
QuestStates
PetStates
BuildingLevels
WorkshopQueue
MerchantState
Settings
```

## File

```text
Application.persistentDataPath\save.json
Application.persistentDataPath\save_backup.json
```

## Quy tắc

- Save atomically: ghi file tạm rồi replace.
- Trước khi overwrite save chính, copy sang backup.
- Save sau giao dịch quan trọng.
- Autosave theo interval.
- Không save mỗi frame.
- Có reset save trong debug menu.
- Có version migration.

## Nghiệm thu

- [ ] New game hoạt động.
- [ ] Save/load hoạt động.
- [ ] Backup restore hoạt động.
- [ ] Save không mất khi force close.
- [ ] Save cũ có migration placeholder.
- [ ] Không duplicate reward khi load lại.

---

# PHASE 09 — UI SHELL

## Mục tiêu

Dựng giao diện khung giống flow bản decode, dùng template Unity làm nền.

## Màn hình chính

- Headquarters
- Adventurers
- Dungeons
- Raids

## Thành phần cố định

### Top bar

- Money
- Gems
- Optional notification/message
- Settings

### Bottom navigation

- Headquarters
- Adventurers
- Dungeons
- Raids

### Overlay layers

- Popup
- Modal confirmation
- Reward popup
- Loading
- Toast
- Tooltip

## Android XML chỉ dùng làm reference

Đối chiếu:

```text
activity_main.xml
fragment_headquarters.xml
fragment_adventurers.xml
fragment_dungeons.xml
fragment_raids.xml
```

Không cố chuyển XML thành prefab tự động ở giai đoạn này.

## Nghiệm thu

- [ ] Đổi bốn tab không reload scene.
- [ ] Safe area đúng.
- [ ] Portrait responsive.
- [ ] Popup block input nền.
- [ ] Back button đóng popup trước.
- [ ] Không tạo Canvas lồng nhau không cần thiết.

---

# PHASE 10 — INVENTORY VÀ STORAGE

## Mục tiêu

Dựng lại hệ thống item sở hữu của người chơi.

## Chức năng

- Add item
- Remove item
- Stack item
- Query amount
- Sort
- Filter
- Select item
- Item detail
- Sell
- Capacity
- Full storage handling

## UI reference

```text
dialog_storage.xml
dialog_item_detail.xml
dialog_sell.xml
layout_item.xml
layout_item_big.xml
layout_item_big_grid.xml
```

## Edge cases

- Item equipment unique hoặc stack theo rule.
- Inventory full.
- Sell item đang equip.
- Amount âm.
- Long money overflow.
- Item ID không còn tồn tại sau update.

## Nghiệm thu

- [ ] Nhận item từ debug.
- [ ] Item hiện đúng icon/tên/số lượng.
- [ ] Sort/filter hoạt động.
- [ ] Sell cập nhật money.
- [ ] Save/load giữ inventory.
- [ ] Không mất item khi popup đóng.

---

# PHASE 11 — ADVENTURER SYSTEM

## Mục tiêu

Dựng character runtime dựa trên các class trong decode.

## Chức năng

- Own adventurer
- Base stats
- Runtime stats
- EXP
- Level
- Tier/class
- Health state
- Assignment state
- Detail UI
- Recruit placeholder

## UI reference

```text
fragment_adventurers.xml
layout_adventurer.xml
layout_adventurer_summary.xml
dialog_entity_detail.xml
dialog_choose_adventurer.xml
```

## Stat groups cần khảo sát từ decode

- HP
- Constitution
- Intelligence
- Dexterity
- Defense
- Magic defense
- Damage
- Healing
- Speed/turn priority nếu có
- Trait/doctrine/potion modifier

## Nghiệm thu

- [ ] Danh sách adventurer hiển thị.
- [ ] Mỗi adventurer có instance ID riêng.
- [ ] Level/EXP hoạt động.
- [ ] Runtime stat được tính từ definition.
- [ ] Assigned adventurer không thể được gửi hai nơi.
- [ ] Save/load giữ state.

---

# PHASE 12 — EQUIPMENT SYSTEM

## Mục tiêu

Cho adventurer equip item và tính stat chính xác.

## Slot dựa trên decode

Khảo sát các abstract class:

```text
Weapon
Armor
Accessory
```

Bổ sung potion/consumable theo rule thật sau audit.

## Chức năng

- Equip
- Unequip
- Replace
- Compatibility by class/type
- Stat recompute
- Equipped item lock
- Equipment selection popup

## UI reference

```text
dialog_select_equipment.xml
layout_select_equipment.xml
layout_adventurer.xml
```

## Nghiệm thu

- [ ] Equip đúng slot.
- [ ] Không equip item không hợp lệ.
- [ ] Stat thay đổi đúng.
- [ ] Item đã equip có trạng thái rõ.
- [ ] Sell item equip bị chặn hoặc xác nhận.
- [ ] Save/load equipment đúng.

---

# PHASE 13 — DUNGEON LOOP MVP

## Mục tiêu

Có gameplay loop đầu tiên chơi được từ đầu đến cuối.

## Flow

```text
Mở Dungeons
→ Chọn dungeon
→ Xem detail
→ Chọn team
→ Start
→ Timer chạy
→ Complete
→ Collect reward
→ Cập nhật inventory/EXP/money
```

## UI reference

```text
fragment_dungeons.xml
layout_dungeon.xml
dialog_dungeon_detail.xml
dialog_send_team.xml
dialog_collect_drops.xml
dialog_recall_adventurers.xml
```

## Runtime state

```text
AreaId
State
StartTime
ExpectedFinishTime
TeamInstanceIds
Progress
Seed
PendingReward
```

## MVP rule

Ở phase này có thể dùng power simulation đơn giản để bảo đảm loop hoạt động.

Không bắt buộc port toàn bộ `Area.java` ngay.

## Nghiệm thu

- [ ] Chọn team được.
- [ ] Team bị lock khi đang đi.
- [ ] Timer hoạt động theo Unix time.
- [ ] Thoát màn hình timer vẫn chạy.
- [ ] Complete tạo reward đúng một lần.
- [ ] Collect trả adventurer về trạng thái available.
- [ ] Save/load giữa chuyến đi hoạt động.

---

# PHASE 14 — LOOT VÀ REWARD

## Mục tiêu

Tạo reward system dùng chung cho dungeon, raid, quest, ads và shop.

## Reward types

- Money
- Gems
- Item
- EXP
- Pet
- Unlock
- Raid try
- Building resource nếu có

## Weighted drop

Mỗi drop entry có:

```text
ItemId
Weight/Chance
MinAmount
MaxAmount
Condition
RollCount
```

## Quy tắc quan trọng

- Random seed được lưu nếu cần chống reroll bằng force close.
- Reward được tạo một lần và lưu thành pending reward.
- Collect mới chuyển reward vào inventory.
- Inventory full phải có xử lý.

## Nghiệm thu

- [ ] Weighted random đúng.
- [ ] Không nhận reward hai lần.
- [ ] Pending reward survive restart.
- [ ] Reward popup hiển thị đúng.
- [ ] Quest progress nhận event reward.

---

# PHASE 15 — OFFLINE PROGRESS

## Mục tiêu

Khi người chơi đóng game, dungeon/workshop/merchant timer vẫn tiến triển.

## Nguồn tham khảo

- `TrueTimeUtils.java`
- `Data.lastAccess`
- `dialog_idle_progress.xml`
- các trường `lastHourTriggered`, `last24Triggered`, `lastWeekTriggered`

## Flow

```text
Save last access
→ Close app
→ Reopen
→ now - lastAccess
→ Resolve area/workshop/merchant/tavern timers
→ Show offline summary
→ Save resolved state
```

## Bảo vệ

- Clamp offline duration.
- Không dùng `Time.time`.
- Dùng UTC Unix timestamp.
- Tránh resolve hai lần.
- Không tin hoàn toàn clock thiết bị nếu game cần chống gian lận; bản deadline có thể chấp nhận local clock và ghi log.

## Nghiệm thu

- [ ] Offline 5 phút hoạt động.
- [ ] Offline qua ngày hoạt động.
- [ ] Dungeon complete offline.
- [ ] Workshop complete offline.
- [ ] Không duplicate.
- [ ] Idle progress popup đúng.

---

# PHASE 16 — COMBAT CƠ BẢN

## Mục tiêu

Thay power simulation bằng combat resolver cơ bản.

## Nguồn decode

```text
Entity.java
Skills.java
Area.java
Action.java
Event.java
AdventureRecap.java
Logger.java
```

## Kiến trúc

```text
CombatContext
CombatEntity
CombatTeam
TurnResolver
TargetResolver
DamageResolver
SkillResolver
CombatLog
CombatResult
```

## Bản đầu

- Turn order
- Basic attack
- Physical/magic damage
- Defense
- Heal
- Death
- Victory/defeat
- Combat log
- Deterministic seed

## Không làm ngay

- toàn bộ special-case của 121 enemy;
- animation chiến đấu phức tạp;
- tất cả status effect;
- mọi skill hiếm.

## Nghiệm thu

- [ ] Combat chạy headless không cần UI.
- [ ] Cùng seed cho cùng kết quả.
- [ ] Không infinite loop.
- [ ] Có turn cap.
- [ ] Có log để debug.
- [ ] Dungeon dùng combat result.

---

# PHASE 17 — COMBAT ĐẦY ĐỦ

## Mục tiêu

Port các rule phức tạp từ decode.

## Cần làm

- Skill target constants trong `Area.java`
- All allies/enemies
- Random target
- Lowest HP target
- Shield target
- Buff/debuff
- Status duration
- End-of-turn action
- Silence/skip behavior
- Passive skills
- Trait and doctrine hooks
- Resurrection
- Boss mechanics
- Special item effects

## Phương pháp

Không port toàn bộ một lần.

Tạo bảng:

| Java mechanic | C# mechanic | Test case | Status |
|---|---|---|---|
| TARGET_RANDOM_ENEMY | RandomEnemyTargetRule | combat_target_001 |  |
| Silence | SilenceStatus | combat_status_001 |  |
| Resurrection | ResurrectionEffect | combat_death_003 |  |

## Nghiệm thu

- [ ] Skill phổ biến chạy đúng.
- [ ] Status phổ biến chạy đúng.
- [ ] Boss không crash.
- [ ] Combat log đủ đọc.
- [ ] Có automated tests cho formulas chính.

---

# PHASE 18 — HEADQUARTERS VÀ BUILDINGS

## Mục tiêu

Dựng hub chính và level upgrade.

## Building fields thấy trong `Data.java`

- Storage
- Quarters
- Tavern capacity
- Tavern time
- Workshop queue
- Workshop time
- Market listings
- Market time
- Shelter
- Shelter autofeed

## UI reference

```text
fragment_headquarters.xml
dialog_quarters.xml
dialog_shelter.xml
dialog_tavern.xml
dialog_workshop.xml
dialog_market.xml
```

## Chức năng

- Building cards
- Current level
- Next effect
- Upgrade price
- Upgrade action
- Unlock condition
- Save level

## Formula source

`Formulas.java` là nguồn chính để đối chiếu giá và scaling.

## Nghiệm thu

- [ ] Upgrade trừ tiền đúng.
- [ ] Capacity thay đổi.
- [ ] Timer modifier thay đổi.
- [ ] UI update ngay.
- [ ] Save/load giữ level.
- [ ] Max level xử lý đúng.

---

# PHASE 19 — WORKSHOP VÀ CRAFTING

## Mục tiêu

Dựng recipe, queue và timer crafting.

## Nguồn

```text
Recipes.java
dialog_recipes.xml
dialog_craft.xml
dialog_workshop.xml
layout_craft*.xml
layout_workshop_item.xml
```

## Chức năng

- Recipe list
- Ingredient check
- Queue
- Craft duration
- Speed bonus by building
- Finish offline
- Claim item
- Queue capacity

## Nghiệm thu

- [ ] Không craft thiếu nguyên liệu.
- [ ] Ingredient được trừ đúng lúc.
- [ ] Queue survive restart.
- [ ] Offline complete.
- [ ] Claim không duplicate.
- [ ] Upgrade workshop tác động đúng.

---

# PHASE 20 — TAVERN, MERCHANT VÀ MARKET

## Mục tiêu

Dựng recruit và thương mại.

## UI reference

```text
dialog_tavern.xml
layout_tavern_adventurer.xml
dialog_merchant.xml
dialog_buy_from_merchant.xml
dialog_market.xml
layout_market_item.xml
```

## Tavern

- Generate candidate
- Visit timer
- Capacity
- Recruit price
- Duplicate/class rules
- Refresh

## Merchant

- Regular offers
- Special offers
- Refresh timer
- Purchased state

## Market

- Listing count
- Selling timer
- Claim money
- Upgrade effect

## Nghiệm thu

- [ ] Offer state được save.
- [ ] Refresh theo timer.
- [ ] Buy không duplicate.
- [ ] Market listing lock item.
- [ ] Offline market completion.
- [ ] Money cập nhật đúng.

---

# PHASE 21 — QUEST SYSTEM

## Mục tiêu

Dựng 56 quest theo cơ chế data-driven.

## Nguồn

```text
Quest.java
QuestsManager.java
quests\instances\*
dialog_quests.xml
dialog_refresh_quests.xml
layout_quest.xml
```

## Quest event types

- Item obtained
- Item sold
- Item crafted
- Dungeon completed
- Raid completed
- Enemy killed
- Money earned/spent
- Adventurer recruited/promoted
- Building upgraded
- Pet obtained/merged

## Kiến trúc

Gameplay phát event; QuestService lắng nghe.

Không để từng module tìm quest rồi sửa trực tiếp.

## Nghiệm thu

- [ ] Quest progress đúng.
- [ ] Save/load đúng.
- [ ] Claim một lần.
- [ ] Refresh đúng rule.
- [ ] Quest complete offline nếu hợp lệ.
- [ ] 5 quest mẫu chạy trước khi nhập full 56.

---

# PHASE 22 — PROMOTION, TRAIT VÀ DOCTRINE

## Mục tiêu

Dựng hệ phát triển adventurer nâng cao.

## UI reference

```text
dialog_promotion_choices.xml
layout_promote_adventurer.xml
layout_adventurer_promotion.xml
layout_adventurer_change_trait.xml
layout_trait.xml
dialog_change_trait_rare.xml
dialog_choose_doctrine.xml
dialog_doctrine.xml
dialog_doctrine_reset.xml
layout_doctrine*.xml
```

## Chức năng

- Promotion requirement
- Promotion choice
- Class path
- Trait assign/reroll
- Doctrine selection
- Doctrine progression
- Doctrine reset
- Stat/skill modifier hooks

## Nghiệm thu

- [ ] Promotion không mất adventurer.
- [ ] Equipment compatibility được kiểm lại.
- [ ] Trait áp dụng vào stat/combat.
- [ ] Doctrine modifier có test.
- [ ] Reset có confirmation.
- [ ] Save migration hỗ trợ data mới.

---

# PHASE 23 — PET SYSTEM

## Mục tiêu

Dựng pet runtime, feeding, equip và merge.

## Nguồn

```text
Pet.java
PetAbility.java
pets\abstractClasses\*
pets\instances\*
dialog_choose_pet.xml
dialog_pet_detail.xml
dialog_merge_pet.xml
layout_pet_feeding.xml
layout_pet_grid.xml
```

## Chức năng

- Own pet
- Pet tier/level
- Feed
- Ability
- Assign pet
- Merge
- Auto-feed by shelter upgrade
- Pet detail UI

## Nghiệm thu

- [ ] Pet inventory hoạt động.
- [ ] Feeding trừ đúng tài nguyên.
- [ ] Ability hook đúng.
- [ ] Merge validate đúng input.
- [ ] Save/load giữ pet.
- [ ] Auto-feed không chạy hai lần.

---

# PHASE 24 — RAID SYSTEM

## Mục tiêu

Dựng 12 raid dựa trên Area framework.

## Nguồn

```text
places\raids\*
fragment_raids.xml
dialog_refill_raid_try.xml
```

## Chức năng

- Raid unlock
- Raid tries
- Refill
- Boss/stage
- Raid-specific reward
- Cooldown/reset
- Epic raid type nếu có

## Tái sử dụng

- Team selection
- Combat resolver
- Reward service
- Area runtime
- Offline timer nếu raid hỗ trợ

## Nghiệm thu

- [ ] Dungeon và raid không duplicate code cốt lõi.
- [ ] Try được trừ đúng.
- [ ] Refill đúng rule.
- [ ] Reward pending.
- [ ] Boss mechanic không crash.
- [ ] Save/load raid state.

---

# PHASE 25 — BESTIARY VÀ ACHIEVEMENT

## Mục tiêu

Hoàn thiện collection/progression phụ.

## Nguồn

```text
AchievementsUtils.java
dialog_bestiary.xml
layout_bestiary_element.xml
layout_bestiary_enemy.xml
```

## Chức năng

- Enemy discovered
- Kill count
- Drop discovered
- Achievement progress
- Achievement unlock
- Optional platform achievement sync

## Nghiệm thu

- [ ] Discover event hoạt động.
- [ ] Kill count đúng.
- [ ] UI bestiary filter được.
- [ ] Achievement local hoạt động trước online sync.
- [ ] Không phụ thuộc Google Play Games để game chạy.

---

# PHASE 26 — LOCALIZATION VÀ SETTINGS

## Mục tiêu

Dùng `strings.xml` làm nguồn text và dựng settings.

## Nguồn

```text
resources\res\values\strings.xml
resources\res\values\plurals.xml
dialog_settings.xml
```

## Chức năng

- Key-based localization
- Number formatting
- Plural support
- Music volume
- SFX volume
- Vibration
- Theme nếu cần
- Language selection
- Privacy/support links nếu cần

## Nghiệm thu

- [ ] Không hard-code text lớn trong prefab.
- [ ] Missing key có log.
- [ ] Settings save được.
- [ ] Audio settings áp dụng ngay.
- [ ] UI không vỡ với text dài.

---

# PHASE 27 — ADS, IAP VÀ CLOUD SAVE

## Mục tiêu

Tái tích hợp dịch vụ bằng SDK Unity; không port Android Java wrapper.

## Nguồn tham khảo

- `AndroidManifest.xml`
- `IAPWrapper.java`
- `MainActivity.java`
- `SnapshotData.java`

## Thứ tự

1. Game chạy hoàn toàn offline trước.
2. Ads wrapper interface.
3. IAP wrapper interface.
4. Cloud save wrapper.
5. Google Play Games sign-in.
6. Test environment.
7. Production config.

## Không làm

- Không copy Ads Application ID cũ khi chưa xác nhận quyền sở hữu.
- Không dùng product ID cũ khi chưa được công ty xác nhận.
- Không để IAP failure làm block boot.

## Nghiệm thu

- [ ] Có mock service trong Editor.
- [ ] Game chạy khi không đăng nhập.
- [ ] Purchase restore có xử lý.
- [ ] Rewarded ad chỉ reward sau callback thành công.
- [ ] Cloud conflict có chiến lược resolve.
- [ ] Secret/config không commit công khai.

---

# PHASE 28 — MIGRATE FULL CONTENT

## Mục tiêu

Sau khi hệ thống ổn định, đưa phần lớn content decode vào Unity.

## Thứ tự nhập

1. Items
2. Adventurers
3. Enemies
4. Skills/statuses
5. Dungeons
6. Recipes
7. Quests
8. Pets
9. Raids
10. Merchant/market data

## Khối lượng tham chiếu

- 607 item
- 132 adventurer-related file
- 122 enemy-related file
- 11 dungeon
- 12 raid
- 56 quest
- 21 pet

## Quy trình mỗi batch

```text
Nhập 20–50 records
→ Validate
→ Play Mode smoke test
→ Fix missing reference
→ Commit
→ Batch tiếp theo
```

## Report bắt buộc

```text
Reports\
├── Missing_Sprites.md
├── Missing_Strings.md
├── Missing_References.md
├── Parse_Exceptions.md
└── Content_Progress.md
```

## Nghiệm thu

- [ ] Mọi record có unique ID.
- [ ] Không missing required reference.
- [ ] Có placeholder list rõ.
- [ ] Content progress được cập nhật.
- [ ] Không nhập một batch quá lớn không thể rollback.

---

# PHASE 29 — QA VÀ BALANCE

## Mục tiêu

Đảm bảo game không chỉ chạy mà còn có progression hợp lý.

## Test matrix

### Save

- New game
- Existing save
- Corrupt save
- Backup recovery
- Version migration
- Force close during save

### Inventory

- Full capacity
- Sell equipped
- Stack max
- Unknown item

### Dungeon/Raid

- Start/recall/complete
- Force close
- Offline completion
- Team member invalid
- Pending reward

### Combat

- All dead
- Enemy all dead
- Turn cap
- Resurrection
- Silence
- Multi-target
- Status expiration

### Economy

- Negative currency
- Overflow
- Upgrade max
- Duplicate claim
- Purchase cancellation

## Balance

Đối chiếu:

- `Formulas.java`
- từng item/adventurer/enemy class;
- dungeon/raid definitions.

Không cần giống 100% ngay, nhưng phải ghi lại khác biệt trong `Decisions.md`.

## Nghiệm thu

- [ ] Không có blocker.
- [ ] Không mất save.
- [ ] Không duplicate reward/currency.
- [ ] Progression đầu game chơi được.
- [ ] Có regression checklist.
- [ ] Có danh sách known issue rõ.

---

# PHASE 30 — OPTIMIZATION

## Mục tiêu

Game chạy ổn trên Android thật.

## Hạng mục

- Sprite atlas
- Addressables nếu cần
- Async loading
- Pool list/card
- Avoid Instantiate/Destroy liên tục
- Reduce Canvas rebuild
- GC allocation
- JSON load time
- Save file size
- Memory usage
- Battery/idle timer
- UI overdraw

## Thiết bị test

Ít nhất:

- Android cấu hình thấp
- Android tầm trung
- Màn hình dài 19.5:9
- Tablet nếu hỗ trợ

## Nghiệm thu

- [ ] Không crash memory.
- [ ] UI scroll ổn.
- [ ] Boot time chấp nhận được.
- [ ] Save không giật rõ.
- [ ] Không tụt FPS nghiêm trọng.
- [ ] APK size được kiểm soát.

---

# PHASE 31 — ANDROID RELEASE

## Mục tiêu

Tạo bản APK/AAB cuối có thể bàn giao hoặc phát hành.

## Cấu hình

- Package ID mới hoặc package ID được công ty duyệt
- Version code/name
- Portrait orientation
- Min SDK
- Target SDK
- Keystore
- Signing
- IL2CPP/Mono theo yêu cầu
- ARM64
- Internet permission nếu dùng services
- Privacy policy nếu có Ads/IAP/analytics

## Build outputs

```text
D:\Tinh\Rebuild_GuildMaster\Builds\Android\
├── Rebuild_GuildMaster_Debug.apk
├── Rebuild_GuildMaster_Release.apk
└── Rebuild_GuildMaster_Release.aab
```

## Final checklist

- [ ] Clean build.
- [ ] Install/update được.
- [ ] Save survive update.
- [ ] Không dùng test Ads/IAP config.
- [ ] Không có debug menu trong release.
- [ ] Không có secret trong log.
- [ ] Có release notes.
- [ ] Có final known issues.
- [ ] Source được tag trong Git.

---

# 5. MILESTONE QUẢN LÝ TIẾN ĐỘ

## Milestone A — Foundation Ready

Bao gồm Phase 00–08.

Hoàn thành khi:

- Unity/template/MCP hoạt động;
- data mẫu load được;
- save/load hoạt động;
- có Android test build.

## Milestone B — Playable MVP

Bao gồm Phase 09–15.

Hoàn thành khi:

- UI shell hoàn chỉnh;
- inventory, adventurer, equipment hoạt động;
- chạy dungeon;
- nhận reward;
- offline progress;
- save/load không lỗi.

Đây là mốc quan trọng nhất cho deadline.

## Milestone C — Gameplay Complete

Bao gồm Phase 16–24.

Hoàn thành khi:

- combat đầy đủ;
- buildings/workshop/market/tavern;
- quest/promotion/trait/doctrine/pet/raid hoạt động.

## Milestone D — Content Complete

Bao gồm Phase 25–28.

Hoàn thành khi:

- bestiary/settings/services;
- phần lớn content decode đã được đưa vào Unity.

## Milestone E — Release Candidate

Bao gồm Phase 29–31.

Hoàn thành khi:

- QA;
- optimization;
- APK/AAB release.

---

# 6. THỨ TỰ ƯU TIÊN KHI DEADLINE GẤP

Nếu deadline không cho phép làm toàn bộ ngay, thực hiện theo đường critical path:

```text
Phase 00
→ Phase 01
→ Phase 02
→ Phase 03
→ Phase 04
→ Phase 05
→ Phase 06
→ Phase 07
→ Phase 08
→ Phase 09
→ Phase 10
→ Phase 11
→ Phase 12
→ Phase 13
→ Phase 14
→ Phase 15
→ Build APK MVP
```

Tạm hoãn:

- full combat edge cases;
- doctrine;
- pet merge nâng cao;
- bestiary;
- achievement online;
- Ads;
- IAP;
- cloud save;
- polish không cốt lõi.

---

# 7. CÁCH GIAO VIỆC CHO ANTIGRAVITY + MCP

Mỗi lần chỉ giao một phase hoặc một task nhỏ.

## Prompt khung

```text
Project path:
D:\Tinh\Rebuild_GuildMaster

Decode/reference path:
D:\Tinh\Guild Master - Idle Dungeons

Implement Phase XX only.

Before editing:
1. Inspect the existing Unity template and reuse its systems where appropriate.
2. Read only the decode files relevant to this phase.
3. Do not modify the decode/reference folder.
4. Do not change unrelated Unity modules.
5. Do not install new packages unless strictly required.
6. Keep the project compiling after every meaningful change.

Required output:
- Implemented files/assets.
- A report at Reports\Phase_XX_Report.md.
- List of changed files.
- Test steps.
- Known issues.
- Unity Console must have no compile errors.

Acceptance criteria:
[paste checklist from this plan]
```

## Quy tắc bắt buộc

- Không giao nhiều phase cùng lúc.
- Mỗi phase phải có commit riêng.
- Sau phase phải chạy Play Mode.
- Sau module lớn phải build Android smoke test.
- Không cho agent rename hàng loạt asset dùng chung.
- Không cho agent xóa code template nếu chưa chứng minh không dùng.
- Nếu có compile error, dừng feature mới và sửa lỗi trước.

---

# 8. FILE THEO DÕI TIẾN ĐỘ

Tạo:

```text
D:\Tinh\Rebuild_GuildMaster\Docs\Progress.md
```

Nội dung mẫu:

```markdown
# Progress

| Phase | Status | Started | Completed | Commit | Notes |
|---:|---|---|---|---|---|
| 00 | Not Started | | | | |
| 01 | Not Started | | | | |
...
| 31 | Not Started | | | | |

Status:
- Not Started
- In Progress
- Review
- Done
- Blocked
```

## Current status đề xuất

```text
Current phase: Phase 00
Current milestone: Foundation Ready
Playable APK: No
Full content migrated: No
Release candidate: No
```

---

# 9. DEFINITION OF DONE CHO TOÀN DỰ ÁN

Dự án được coi là hoàn thành khi:

- [ ] Unity project sạch và mở được.
- [ ] Build Android thành công.
- [ ] Có main navigation.
- [ ] Headquarters hoạt động.
- [ ] Adventurer system hoạt động.
- [ ] Inventory/equipment hoạt động.
- [ ] Dungeon hoạt động.
- [ ] Combat hoạt động.
- [ ] Loot/reward hoạt động.
- [ ] Save/load/backup hoạt động.
- [ ] Offline progress hoạt động.
- [ ] Buildings/crafting/market/tavern hoạt động.
- [ ] Quest hoạt động.
- [ ] Pet hoạt động.
- [ ] Raid hoạt động.
- [ ] Phần lớn content decode đã được nhập.
- [ ] Không có blocker bug.
- [ ] Không có lỗi compile.
- [ ] Không duplicate reward/currency.
- [ ] Save survive update.
- [ ] Có APK/AAB bàn giao.

---

# 10. VIỆC CẦN LÀM NGAY SAU KHI ĐỌC PLAN

1. Tạo Unity project mới tại:
   ```text
   D:\Tinh\Rebuild_GuildMaster
   ```
2. Import template có sẵn.
3. Cài và kiểm tra Unity MCP.
4. Init Git.
5. Copy file plan này vào:
   ```text
   D:\Tinh\Rebuild_GuildMaster\Docs\Rebuild_Plan.md
   ```
6. Thực hiện Phase 00.
7. Sau khi Phase 00 đạt checklist, thực hiện Phase 01.
8. Không bắt đầu nhập full content trước khi Milestone B chạy ổn.
