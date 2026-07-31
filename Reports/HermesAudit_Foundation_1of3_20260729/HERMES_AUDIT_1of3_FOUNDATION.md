# HERMES AUDIT 1/3 — FOUNDATION TOÀN DIỆN

**Dự án:** Guild Master (Unity 6000.3.17f1)  
**Thời gian:** 2026-07-29  
**Worktree:** `.claude/worktrees/agent-a4b047a2947e2d706/`  
**Scripts:** 137 `.cs` files | **Scenes:** 4 (2 active + 2 stale) | **Prefabs:** 0 | **Data files:** 13 (1,531 records)

---

## MỤC LỤC

1. [Compile Baseline](#1-compile-baseline)
2. [EditMode / PlayMode Tests](#2-editmode--playmode-tests)
3. [Source & Asset Inventory](#3-source--asset-inventory)
4. [StreamingAssets / GameData](#4-streamingassets--gamedata)
5. [Data Loaders & Production Callers](#5-data-loaders--production-callers)
6. [Bootstrap Entry Point](#6-bootstrap-entry-point)
7. [Service Construction & Dependency Wiring](#7-service-construction--dependency-wiring)
8. [Scene / Prefab Foundation Wiring](#8-scene--prefab-foundation-wiring)
9. [Tick / Update / Event Lifecycle](#9-tick--update--event-lifecycle)
10. [Save / Load](#10-save--load)
11. [Test Save Isolation](#11-test-save-isolation)
12. [Fresh-Save Initialization](#12-fresh-save-initialization)
13. [Active-State Restoration](#13-active-state-restoration)
14. [Offline Entry Point](#14-offline-entry-point)
15. [Foundation Blockers & Risk Matrix](#15-foundation-blockers--risk-matrix)

---

## 1. Compile Baseline

### Tình Trạng
| Hạng mục | Trạng thái |
|-----------|-----------|
| Scripts trong worktree | ✅ 137 files |
| Namespaces | `GuildMaster.*` (unified) |
| .NET / C# version | C# 10+ (Unity 6000) |
| External dependencies | Unity built-in packages (URP, UI, TextMeshPro) |
| Define constraints | `UNITY_EDITOR` / `!UNITY_EDITOR` for data provider |

### Potential Risks
- **0 prefabs** — tất cả UI screens đều in-scene trên Main.unity. Không có prefab variant workflow, khó maintain scene layout.
- Unity Editor compile baseline chưa được verify (không có CI/CD).
- `UNITY_EDITOR` conditional compilation được dùng ở 2 nơi (Bootstrapper + UIRuntimeBootstrap) với cùng logic.

---

## 2. EditMode / PlayMode Tests

### Test Inventory
```
EditMode Tests (11 files):
├── DatabaseTests.cs                       — DB build & records
├── S2VerificationTests.cs                 — S2 cross-module
├── S6_5A_Stage1_FoundationTests.cs        — Foundation bootstrap
├── S6_5A_Stage2_ServiceWiringTests.cs     — Service wiring
├── S6_5A_Stage3_TavernTests.cs            — Tavern domain
├── S6_5A_Stage4_CharacterTests.cs         — Character domain
├── S6_5A_Stage5_InventoryTests.cs         — Inventory domain
├── S6_5A_Stage6_CraftTests.cs             — Craft domain
├── S6_5A_Stage7_MerchantTests.cs          — Merchant domain
├── S6_5A_Stage8_DungeonCombatTests.cs     — Dungeon/Combat domain
├── S6_5A_Stage9_QuestTests.cs             — Quest domain
├── S6_5A_Stage10_SettingsTests.cs         — Settings domain
└── S6_5A_Stage11_OfflineTests.cs          — Offline progress

PlayMode Tests (3 files):
├── S6_5A_RuntimeSmokeTest.cs              — Boot.unity → Runtime
├── S6_5A_RuntimeActionSmokeTest.cs        — Boot.unity → Actions
├── S6_5A_DungeonCombatLootActionTests.cs  — Dungeon/Combat/Loot
└── S6_5A_PlayerFacingUIActionTests.cs     — UI screens mutate state

Editor Tests (3 files):
├── S3B2DDataImportTests.cs                — Data import validation
├── SceneFixer.cs                          — Scene fix utilities
└── SmokeTestRunner.cs                     — Smoke test runner
```

### Coverage Gaps
| Khoảng trống | Mức độ |
|-------------|--------|
| Không có test cho Bootstrap/Bootstrapper.cs (dead code path) | 🟡 Medium |
| Không có test cho `Runtime/Boot/Bootstrapper.cs` (orphaned) | 🟡 Medium |
| Không có test cho OfflineProgress startup initialization | 🟡 Medium |
| Không có test cho save migration edge cases | 🔴 High |
| Không có test cho `NormalizeAfterLoad()` với partial corrupt data | 🟡 Medium |
| Không có test cho DefinitionId validation on load | 🟡 Medium |
| PlayMode tests chỉ cover Main.unity (skip Boot.unity flow) | 🟡 Medium |

---

## 3. Source & Asset Inventory

### Source Code Tree
```
Assets/_Game/Scripts/
├── Bootstrap/
│   └── Bootstrapper.cs              ← DEAD CODE (unreferenced)
├── Bootstrapper/                     ← EMPTY (0 files)
├── Database/
│   ├── DatabaseBuilder.cs           ← Actual data loader
│   ├── DatabaseBuildReport.cs       ← Build diagnostics
│   ├── GameDatabase.cs              ← Registry
│   └── EnemyDropTableLoader.cs
├── DefinitionBase.cs                ← Root definition base
├── Definitions/
│   ├── AdventurerDefinition.cs
│   ├── DefinitionBase.cs
│   ├── DungeonDefinition.cs
│   ├── EnemyDefinition.cs
│   ├── Enums/ (CombatResult.cs, StatusEffectType.cs)
│   ├── ItemDefinition.cs
│   ├── ItemEnums.cs
│   ├── PetDefinition.cs
│   ├── QuestDefinition.cs
│   ├── RaidDefinition.cs
│   ├── RecipeDefinition.cs
│   ├── SkillDefinition.cs
│   └── StatusEffectDefinition.cs
├── Infrastructure/
│   ├── DataProviders/
│   │   ├── IGameDataProvider.cs
│   │   ├── EditorExternalGameDataProvider.cs
│   │   └── StreamingAssetsGameDataProvider.cs
│   └── Serialization/
│       ├── IJsonSerializer.cs
│       └── UnityJsonSerializer.cs
├── Loaders/
│   └── DTOs/
│       ├── ManifestDefinition.cs
│       └── DefinitionFile.cs
├── Runtime/
│   ├── Assets/AssetCatalog.cs
│   ├── Boot/
│   │   ├── BootSceneLoader.cs       ← Active (Boot.unity)
│   │   ├── Bootstrapper.cs          ← DEAD CODE (orphaned)
│   │   └── UIRuntimeBootstrap.cs    ← Active (Main.unity)
│   ├── Core/
│   │   ├── RuntimeFactory.cs
│   │   ├── IInstanceIdGenerator.cs
│   │   └── DefaultInstanceIdGenerator.cs
│   ├── Formulas/
│   │   ├── FormulaService.cs        ← 252 lines, all hardcoded formulas
│   │   ├── IFormulaService.cs
│   │   └── PurchaseFlags.cs
│   ├── Models/ (10 files)
│   ├── Save/
│   │   ├── ISaveService.cs
│   │   ├── SaveService.cs
│   │   ├── SaveData.cs              ← 306 lines
│   │   ├── ActiveDungeonSaveData.cs
│   │   ├── CombatEncounterSaveData.cs
│   │   └── DungeonActionState.cs
│   ├── Services/ (18 runtime services)
│   │   ├── ServiceContainer.cs      ← Composition root
│   │   └── (17 domain services)
│   └── UI/ (9 screens)
└── Services/
    ├── AssetManifestService.cs
    ├── DatabaseService.cs
    └── LocalizationService.cs
```

### Asset Inventory
| Loại | Số lượng | Ghi chú |
|------|---------|---------|
| Scene files | 6 | Boot, Main, Boot 1, Main 1, SampleScene, URP2DSceneTemplate |
| Prefabs | 0 | ❌ Không có prefabs — tất cả in-scene |
| ScriptableObjects (.asset) | 0 | ❌ Không có |
| Animations | 0 | ❌ Không có |
| Material overrides | 0 | ❌ |

---

## 4. StreamingAssets / GameData

### File Inventory
| File | Lines | Records | Trạng thái |
|------|-------|---------|-----------|
| `manifest.json` | 141 | 13 files declared | ✅ OK |
| `adventurers.json` | 5,797 | 129 | ✅ OK |
| `items.json` | 26,975 | 607 | ✅ OK |
| `recipes.json` | 5,457 | 321 | ✅ OK |
| `skills.json` | 2,506 | 227 | ✅ OK |
| `enemies.json` | 4,593 | 122 | ✅ OK |
| `dungeons.json` | 1,136 | 11 | ✅ OK |
| `quests.json` | 457 | 56 | ✅ OK |
| `raids.json` | 105 | 12 | ✅ OK |
| `pets.json` | 177 | 21 | ✅ OK |
| `status_effects.json` | 284 | 25 | ✅ OK |
| `localization.json` | 0 | N/A | ⚠️ EMPTY |
| `assets_manifest.json` | 0 | N/A | ⚠️ EMPTY |
| **Tổng** | | **1,531 records** | |

### Data Integrity
- `fullCountByCategory` vs `recordCount` mismatch:
  - dungeons: 0 full / 11 partial
  - enemies: 0 full / 122 partial
  - recipes: 0 full / 321 partial
  - skills: 0 full / 227 partial
  - status_effects: 0 full / 25 partial
- `parseStatus` trong data: mixed (`"full"`, `"complete_from_dex"`)
- `decodeVersion`: `"unknown"` — không track được version của decode process
- `sourceHash` hiện diện nhưng không được validate ở runtime
- 2 files empty (localization.json, assets_manifest.json) — services tương ứng sẽ silent-fail

### Hardcoded Values Evidence Map
| Value | Source | File | Dòng |
|-------|--------|------|------|
| Tất cả item stats (price, rarity, fields) | GameData JSON | `items.json` | Full |
| Tất cả adventurer stats | GameData JSON | `adventurers.json` | Full |
| Tất cả dung stats (enemies, merchant offers) | GameData JSON | `dungeons.json` | Full |
| Tất cả skill stats | GameData JSON | `skills.json` | Full |
| Tất cả recipe stats | GameData JSON | `recipes.json` | Full |
| Tất cả enemy stats | GameData JSON | `enemies.json` | Full |
| Tất cả quest stats | GameData JSON | `quests.json` | Full |
| Tất cả pet stats | GameData JSON | `pets.json` | Full |
| Tất cả raid stats | GameData JSON | `raids.json` | Full |
| Tất cả status effect stats | GameData JSON | `status_effects.json` | Full |
| Formula constants (base prices, multipliers) | FormulaService.cs | Codegen | All 252 lines |
| Price lookup tables | FormulaService.cs | Codegen | switch-cases |
| SaveVersion | SaveService.cs | `CurrentSaveVersion = 1` | Line 9 |
| Offline cap (12h) | OfflineProgressService.cs | `Cap12Hours = 12 * 3600` | Line 13 |
| Base storage (35) | FormulaService.cs | `BASE_STORAGE_SPACES = 35` | Code |
| Base quarters (2) | FormulaService.cs | `BASE_QUARTERS_SPACES = 2` | Code |
| Boot -> Main scene name | BootSceneLoader.cs | `_mainSceneName = "Main"` | Line 14 |
| Save file path | SaveService.cs | `save.json`, `save_backup.json` | Lines 17-18 |

**Kết luận:** KHÔNG có hardcoded game data values ngoài FormulaService. Tất cả definition data đều đọc từ JSON files.

---

## 5. Data Loaders & Production Callers

### Data Flow
```
manifest.json ──→ DatabaseBuilder.Build()
                     │
                     ├── items.json ──→ LoadCategory<ItemDefinition>
                     ├── enemies.json ─→ LoadCategory<EnemyDefinition>
                     ├── skills.json ──→ LoadCategory<SkillDefinition>
                     ├── adventurers.json → LoadCategory<AdventurerDefinition>
                     ├── dungeons.json ─→ LoadCategory<DungeonDefinition>
                     ├── quests.json ──→ LoadCategory<QuestDefinition>
                     ├── recipes.json ─→ LoadCategory<RecipeDefinition>
                     ├── pets.json ────→ LoadCategory<PetDefinition>
                     ├── raids.json ───→ LoadCategory<RaidDefinition>
                     └── status_effects.json → LoadCategory<StatusEffectDefinition>
                     │
                     └──→ GameDatabase.RegisterCollection<T>(list)
```

### Loader Analysis
| Component | Trạng thái | Ghi chú |
|-----------|-----------|---------|
| ManifestDefinition (DTO) | ✅ OK | Maps JSON to file entries |
| DefinitionFile<T> (DTO) | ✅ OK | Generic data wrapper |
| DatabaseBuilder | ✅ OK | Category loaders registered, error handling present |
| GameDatabase | ✅ OK | RegisterCollection + GetAll<T> |
| EditorExternalGameDataProvider | ✅ OK | Reads from file system |
| StreamingAssetsGameDataProvider | ✅ OK | Reads from Application.streamingAssetsPath |
| EnemyDropTableLoader | ✅ OK | Special handling for drop tables |

### Production Callers
| Caller | Data Provider | Pha |
|--------|--------------|-----|
| UIRuntimeBootstrap.Start() | EditorExternalGameDataProvider (Editor) / StreamingAssetsGameDataProvider (Build) | Main scene init |
| (All EditMode tests) | EditorExternalGameDataProvider | Test Setup |
| (All PlayMode tests) | EditorExternalGameDataProvider | Runtime test init |

---

## 6. Bootstrap Entry Point

### 🚨 CRITICAL FINDING: Dual Bootstrap + Dead Code

```
Boot.unity (Build Settings scene 0)
  ├── BootSceneLoader (component on scene)
  │   └── Start() → SceneManager.LoadScene("Main")     ← ACTUAL transition
  │
  └── Bootstrap/Bootstrapper.cs (file exists, NOT on scene)
      └── Awake() → InitializeRuntime()
          └── LoadMainScene() is COMMENTED OUT           ← DEAD CODE
              // SceneManager.LoadScene("Main");

Main.unity (Build Settings scene 1)
  └── UIRuntimeBootstrap (component on scene)
      └── Start() → creates new GameDatabase + ServiceContainer
                     from scratch                        ← DUPLICATE boot

Runtime/Boot/Bootstrapper.cs (EXISTS but UNREFERENCED)
  └── Start() → InitializePipeline()
      ├── Creates GameDatabase, ServiceContainer
      ├── RuntimeFactory, FormulaService, SaveService
      ├── Sets RuntimeReady = true
      └── NEVER CALLED BY ANY SCENE                      ← ORPHANED DEAD CODE
```

### Boot Flow (Actual)
```
Boot.unity loads
  → BootSceneLoader.Start()
  → SceneManager.LoadScene("Main")
  → Boot.unity UNLOADS (all GameObjects destroyed)
  → Main.unity loads
  → UIRuntimeBootstrap.Start()
  → Creates new GameDatabase (reads JSON again)
  → Creates new ServiceContainer (18 services)
  → Wires UI screens
  → Shows MainHUD
```

### Issues
1. **🔴 CRITICAL** — `Bootstrap/Bootstrapper.cs` có `LoadMainScene()` comment out. File tồn tại nhưng không được attach vào scene nào. Code không bao giờ chạy.
2. **🔴 CRITICAL** — `Runtime/Boot/Bootstrapper.cs` (pipeline proper với RuntimeReady flag) không được reference bởi bất kỳ file nào (không có backup). Complete orphan.
3. **🔴 CRITICAL** — Boot.unity không share bất cứ state gì với Main.unity. Tất cả initialization trên Boot.unity bị destroy khi scene transition.
4. **🟡 HIGH** — `Bootstrap/Bootstrapper.cs` tạo `DatabaseService`, `LocalizationService`, `AssetManifestService` mà Main.unity không bao giờ dùng.
5. **🔴 CRITICAL** — Không có guard `RuntimeReady`. UIRuntimeBootstrap chạy bất kể database build có lỗi hay không.

---

## 7. Service Construction & Dependency Wiring

### ServiceContainer Dependency Graph
```
ServiceContainer(GameDatabase, [ISaveService], [IFormulaService], [RuntimeFactory])
  │
  ├── Database (GameDatabase)        ← injected
  ├── Factory (RuntimeFactory)       ← injected or new DefaultInstanceIdGenerator
  ├── Formula (IFormulaService)      ← injected or new FormulaService
  ├── Save (ISaveService)            ← injected or new SaveService + Load()
  │
  ├── Item           ← Factory + Database
  ├── Inventory      ← Save + Formula + Item + Database
  ├── Character      ← Save + Formula + Database + Factory + Inventory
  ├── Equipment      ← Inventory + Save
  ├── Skill          ← (no deps)
  ├── StatusEffect   ← (no deps)
  ├── Craft          ← Database + Inventory + Save
  ├── Merchant       ← Database + Inventory + Save
  ├── Combat         ← (no deps)
  ├── TargetSelection ← (no deps)
  ├── Loot           ← (no deps)
  ├── Dungeon        ← Save + Database + Combat + Loot + Character + Inventory
  ├── Doctrine       ← Save + Formula
  ├── Quest          ← Save + Database + Doctrine
  ├── Tavern         ← Save + Formula + Character + Database
  ├── Settings       ← Save
  └── OfflineProgress ← Save + Craft + Merchant
```

### Issues
| Issue | Level |
|-------|-------|
| **ServiceContainer constructor auto-loads Save** — `new SaveService(); newSave.Load(out _);` là side-effect trong constructor | 🟡 Medium |
| **Không có IDatabaseService hoặc IGameDatabase interface** — ServiceContainer tham chiếu trực tiếp GameDatabase cụ thể, khó test/mock | 🟡 Medium |
| **Combat, TargetSelection, Loot, Skill, StatusEffect đều zero-dependency** — chưa wired với Save/Database, không persist state | 🟡 Medium |
| **OfflineProgress không được gọi ở startup** — wired trong container nhưng UIRuntimeBootstrap không gọi ApplyOfflineProgress | 🔴 High |
| **18 services = no lazy init, no DI container** — hand-wired, compile-time coupling | 🟢 Low (intentional) |
| **DungeonService phụ thuộc 5 services** — coupling cao nhất, dễ break | 🟡 Medium |

---

## 8. Scene / Prefab Foundation Wiring

### Scene Inventory
| Scene | Build Setting | Scripts on Scene | Trạng thái |
|-------|:---:|:---:|:---:|
| `Assets/_Game/Scenes/Boot.unity` | ✅ Scene 0 | BootSceneLoader | ✅ Active |
| `Assets/_Game/Scenes/Main.unity` | ✅ Scene 1 | UIRuntimeBootstrap + UIScreen(s) (9) + HUDController | ✅ Active |
| `Assets/_Game/Scenes/Boot 1.unity` (218 lines) | ❌ Not in Build | (none) | ⚠️ Stale copy |
| `Assets/_Game/Scenes/Main 1.unity` (218 lines) | ❌ Not in Build | (none) | ⚠️ Stale copy |

### Screen Registration (in UIRuntimeBootstrap.Start())
| Screen | Find | Register | Initialize | Back Button |
|--------|:----:|:--------:|:----------:|:----------:|
| HUDController | ✅ FFO | ❌ UIScreen | ✅ Initialize(ISaveService, IUIService) | N/A |
| InventoryScreen | ✅ FFO | ✅ UIScreen | ✅ Initialize(ServiceContainer) | ✅ Btn_Back |
| CharacterScreen | ✅ FFO | ✅ UIScreen | ✅ Initialize(ServiceContainer) | ✅ Btn_Back |
| TavernScreen | ✅ FFO | ✅ UIScreen | ✅ Initialize(ServiceContainer) | ✅ Btn_Back |
| CraftScreen | ✅ FFO | ✅ UIScreen | ✅ Initialize(ServiceContainer) | ✅ Btn_Back |
| MerchantScreen | ✅ FFO | ✅ UIScreen | ✅ Initialize(ServiceContainer) | ✅ Btn_Back |
| DungeonScreen | ✅ FFO | ✅ UIScreen | ✅ Initialize(ServiceContainer) | ✅ Btn_Back |
| QuestScreen | ✅ FFO | ✅ UIScreen | ✅ Initialize(ServiceContainer) | ✅ Btn_Back |
| SettingsScreen | ✅ FFO | ✅ UIScreen | ✅ Initialize(ServiceContainer) | ✅ Btn_Back |
| PopupScreen | ✅ FFO | ❌ RegisterDialogScreen | ❌ | N/A |

### Issues
- **🔴 CRITICAL** — `Boot 1.unity` và `Main 1.unity` là stale copies không được dùng, gây nhầm lẫn, cần cleanup.
- **🟡 HIGH** — 0 prefabs. Tất cả UI screens là in-scene GameObjects trên Main.unity. Không thể load/unload UI screens independently.
- **🟡 HIGH** — UIRuntimeBootstrap sử dụng `FindObjectsByType<UIScreen>()` với `FindObjectsInactive.Include`. Thứ tự registration không deterministic.
- **🟡 Medium** — PopupScreen handling đặc biệt: check `screen.IsPopup` + `screen is PopupScreen`. Nếu có popup khác, cơ chế này fail.
- **🟡 Medium** — Back button chỉ tìm `Btn_Back` một cấp (transform.Find). Nếu button ở nested child, không tìm thấy.

---

## 9. Tick / Update / Event Lifecycle

### Current Lifecycle
```
Boot.unity:
  └── BootSceneLoader.Start()
      └── SceneManager.LoadScene("Main")

Main.unity:
  └── UIRuntimeBootstrap.Start()
      ├── Database Build
      ├── Service Construction
      └── UI Wiring + ShowScreen(MainHUD)

Runtime:
  ├── No Update() / game loop tick
  ├── All interaction is event-driven (button OnClick → service mutation)
  ├── UIRuntimeBootstrap.OnApplicationPause() → PersistSave()
  ├── UIRuntimeBootstrap.OnApplicationQuit() → PersistSave()
  └── OfflineProgressService (wired but not called at startup)
```

### Issues
| Issue | Level |
|-------|-------|
| **No game loop** — không có Update() tick để xử lý thời gian thực, dungeon progress, cooldowns | 🟡 Medium (expected at this stage) |
| **Offline progress not called at startup** — player returns after hours, sees no offline rewards | 🔴 High |
| **No AutoSave timer** — save chỉ khi OnApplicationPause/Quit | 🟡 Medium |
| **No coroutine/async initialization** — tất cả synchronous. Nếu database build lâu, UI bị block | 🟢 Low (data small) |

---

## 10. Save / Load

### SaveService Implementation
```
SaveService:
  ├── Constructor: CurrentData = new SaveData()
  ├── HasSaveFile(): File.Exists(persistentDataPath/save.json)
  ├── Load():
  │   ├── Normal flow: ReadAllText → JsonUtility.FromJson<SaveData> → NormalizeAfterLoad()
  │   ├── Migration: if Metadata.SaveVersion < CurrentSaveVersion → MigrateSave()
  │   └── Fallback: backup → empty save → try/catch for each
  ├── Save():
  │   ├── Update Metadata (version, time, game version)
  │   ├── JsonUtility.ToJson(pretty)
  │   ├── Backup old save
  │   └── WriteAllText
  └── DeleteSave(): delete save.json + save_backup.json
```

### Save Data Format
```json
{
  "Metadata": { "SaveVersion": 1, "SaveTimeUnix": ..., "GameVersion": "...", "DataVersion": null },
  "Money": 0,
  "Gems": 0,
  "Items": [...],
  "Characters": [...],
  // ... 70+ fields
}
```

### Issues
| Issue | Level |
|-------|-------|
| **🔴 DataVersion never written** — `Metadata.DataVersion` field exists but `Save()` never sets it. `Load()` can't validate data compatibility | 🔴 HIGH |
| **🔴 JsonUtility + field-based serialization** — `JsonUtility.FromJson` sets missing fields to C# default (0/false/null). `NormalizeAfterLoad()` thêm null guard cho lists, nhưng KHÔNG guard cho value types (int, long, bool, float) | 🔴 HIGH |
| **🟡 MigrateSave() is empty** — không migration logic, chỉ update version number | 🟡 Medium |
| **🟡 SaveService constructor creates file** — `new SaveService();` gọi `new SaveData()` + `new SaveService.Load()`. Nếu đã có save file, constructor đọc disk. Side-effect trong constructor | 🟡 Medium |
| **🟡 No version mismatch error handling** — nếu save format thay đổi, deserialize fail với exception fallback to backup/empty. Không có user-visible error | 🟡 Medium |
| **🟡 No DefinitionId validation on load** — save có thể reference definitionId đã bị xóa khỏi database. Services vẫn dùng reference đó → null ref | 🔴 HIGH |
| **🟢 No encryption** — save là plain text JSON. Player có thể edit | 🟢 Low (not in scope) |

---

## 11. Test Save Isolation

### Current State
| Test File | Creates Save File? | Cleanup? |
|-----------|:-----------------:|:--------:|
| All EditMode tests (new ServiceContainer) | ✅ Yes (SaveService constructor calls Load) | ❌ No |
| S2VerificationTests | ✅ Yes | ❌ No |
| S6_5A_Stage1_FoundationTests | ✅ Yes | ❌ No |
| All PlayMode tests | ✅ Yes (UIRuntimeBootstrap creates ServiceContainer) | ❌ No |

### The Problem
`SaveService` constructor luôn tạo/load save file:
```csharp
public SaveService()
{
    _saveFilePath = Path.Combine(Application.persistentDataPath, "save.json");
    ...
    CurrentData = new SaveData();
}
```

Tất cả tests share cùng `Application.persistentDataPath`. Sequence:
1. Test A creates ServiceContainer → SaveService → creates/modifies save.json
2. Test B creates ServiceContainer → SaveService → Load() reads Test A's save.json
3. Test state contamination!

### Issues
| Issue | Level |
|-------|-------|
| **🔴 No test save isolation** — tests can interfere with each other through shared save files | 🔴 HIGH |
| **🔴 No [TearDown] cleanup** — save files remain after test run, contaminating subsequent runs | 🔴 HIGH |
| **🟡 No mock SaveService** — ISaveService interface exists but no test double | 🟡 Medium |
| **🟡 No in-memory SaveService** — can't run tests without file I/O | 🟡 Medium |

---

## 12. Fresh-Save Initialization

### Current State
Khi không có save file, `SaveService.Load()` trả về:
```csharp
CurrentData = new SaveData();  // All fields at C# defaults
CurrentData.NormalizeAfterLoad();
```

**Kết quả:** Fresh game có:
- Money = 0, Gems = 0
- Items = [] (empty list)
- Characters = [] (empty)
- TutorialStep = 0 (not set)
- LevelStorage = 0 (not 1)
- Settings all at default (false / empty)

### Missing New-Game Logic
| Missing | Impact |
|---------|--------|
| 🚫 No starter items/equipment | Player starts with nothing |
| 🚫 No starter money/gems | Can't buy anything |
| 🚫 No starter character | Tutorial expected a character |
| 🚫 No default settings | Sound/Music = false by default → silent game |
| 🚫 LevelStorage = 0 | Storage capacity formula may break |
| 🚫 TutorialStep = 0 but no tutorial trigger | Tutorial not started |
| 🚫 No "IsNewGame" flag | Can't distinguish fresh save from corrupt save |

### Issues
| Issue | Level |
|-------|-------|
| **🔴 No "New Game" defaults** — SaveData.Money = 0, no starter items, no tutorial state | 🔴 HIGH |
| **🔴 Can't distinguish fresh from corrupt** — empty save looks the same as a partially corrupted one | 🔴 HIGH |
| **🟡 LevelStorage = 0** — FormulaService có BASE_STORAGE_SPACES = 35 nhưng có thể không dùng được nếu LevelStorage = 0 | 🟡 Medium |
| **🟡 Settings all default** — player tạo game mới sẽ thấy không có sound/music | 🟡 Medium |

---

## 13. Active-State Restoration

### Current Flow
```
Game Launch → UIRuntimeBootstrap.Start()
  → Database Build (reads all JSON)
  → ServiceContainer(db)
    → SaveService() — Load():
      → File exists? → JsonUtility.FromJson → NormalizeAfterLoad
      → No file? → new SaveData() → NormalizeAfterLoad
  → UI Wiring
  → ShowScreen(MainHUD)
```

### State Restored From Save
| Data | Source | Restored? |
|------|--------|:--------:|
| Money/Gems | SaveData fields | ✅ Direct field |
| Items | SaveData.Items → List<ItemSaveData> | ✅ Direct list |
| Characters | SaveData.Characters → List<CharacterSaveData> | ✅ Direct list |
| Skills | SaveData.Skills → List<SkillSaveData> | ✅ Direct list |
| Quests | SaveData.Quests → List<QuestSaveData> | ✅ Direct list |
| Dungeons | SaveData.Dungeons → List<DungeonSaveData> | ✅ Direct list |
| Active Dungeon | SaveData.ActiveDungeon | ✅ Direct field |
| Equipment (Weapon/Armor/Accessory) | CharacterSaveData.InstanceId | ⚠️ By reference only |
| Craft Queue | SaveData.WorkshopQueue | ✅ Direct list |
| Market Listings | SaveData.MarketListings | ✅ Direct list |
| Settings | SaveData.Settings* | ✅ Direct fields |

### Restoration NOT Handled
| Aspect | Why Missing | Impact |
|--------|------------|--------|
| **DefinitionId validation** | No cross-check against GameDatabase | Save references deleted item → silent null |
| **Runtime service state sync** | Services read SaveData, no reactive sync | Stale in-memory state after load |
| **Active dungeon combat state** | ActiveDungeonSaveData restored but combat not re-entered | Player "in dungeon" but not actually in combat |
| **Offline progress calculation** | OfflineProgressService exists but not called on startup | Player returns after hours → no offline rewards |
| **Tutorial state** | TutorialStep saved but no tutorial system to read it | Tutorial never triggers |
| **Tavern state** | NextTavernVisit restored but not checked against real time | Visitor timer out of sync |

### Issues
| Issue | Level |
|-------|-------|
| **🔴 Offline progress not applied on startup** — save time checked but delta not calculated/rewarded | 🔴 HIGH |
| **🔴 DefinitionId references not validated** — deleted items/characters break services | 🔴 HIGH |
| **🔴 No runtime state sync** — services read SaveData directly, no observer pattern | 🟡 Medium |
| **🟡 Active dungeon not re-entered** — ActiveDungeonSaveData exists but is never re-initialized into DungeonService | 🟡 Medium |
| **🟡 Stale timers (Tavern)** — NextTavernVisit vs DateTime.UtcNow mismatch after days offline | 🟡 Medium |

---

## 14. Offline Entry Point

### OfflineProgressService
```csharp
public OfflineProgressResult ApplyOfflineProgress(long currentUnix)
{
    var metadata = _saveService.CurrentData?.Metadata;
    long delta = CalculateOfflineDeltaSeconds(metadata.SaveTimeUnix, currentUnix);
    metadata.SaveTimeUnix = currentUnix;
    
    if (delta > 0) {
        _craftService.ProgressWorkshop(delta);
        _merchantService.ProgressMarket(delta);
        // Dungeon/Combat/Quest offline deferred
    }
}
```

### Where it's (NOT) Called
| Call site | Calls OfflineProgress? |
|-----------|:---------------------:|
| UIRuntimeBootstrap.Start() | ❌ NO |
| BootSceneLoader | ❌ NO |
| Any runtime service | ❌ NO |

**OfflineProgressService is wired into ServiceContainer but NEVER INVOKED at startup.**

### Issues
| Issue | Level |
|-------|-------|
| **🔴 Offline progress never called** — player returns after offline hours, gets no progress | 🔴 HIGH |
| **🟡 Only Craft + Merchant** — Dungeon/Combat/Quest offline explicitly deferred | 🟡 Medium |
| **🟡 No OnApplicationPause offline calc** — Pause saves but doesn't process offline delta | 🟡 Medium |

---

## 15. Foundation Blockers & Risk Matrix

### 🔴 CRITICAL — Blockers cần fix trước Audit 2/3

| # | Blocker | File | Impact |
|:-:|---------|------|--------|
| **C1** | `Bootstrap/Bootstrapper.cs` — `LoadMainScene()` is COMMENTED OUT | `Bootstrap/Bootstrapper.cs:89` | Boot scene không transition đến Main qua path này. Dead code. |
| **C2** | `Runtime/Boot/Bootstrapper.cs` — COMPLETE ORPHAN, zero references | `Runtime/Boot/Bootstrapper.cs` | ServiceContainer pipeline với RuntimeReady flag không bao giờ chạy. Dead code. |
| **C3** | Two boot paths exist — Bootstrap/Bootstrapper (useless) + UIRuntimeBootstrap (duplicate work) | Both Bootstrap files | Boot.unity tạo services rồi destroy ngay khi chuyển scene. Main.unity làm lại từ đầu. |
| **C4** | UIRuntimeBootstrap không gọi OfflineProgressService.ApplyOfflineProgress | `UIRuntimeBootstrap.cs` | Player mất offline progress mỗi lần launch game. |
| **C5** | `DataVersion` trong `SaveMetadata` never written/validated | `SaveService.cs:101-104` | Save compatibility check không hoạt động. |
| **C6** | No DefinitionId validation on save load | `SaveService.cs` | Save có reference ID đã xóa → service crash. |
| **C7** | No test save isolation — tests share persistentDataPath | All test files | Test contamination, flaky results. |
| **C8** | No fresh-save initialization/New Game defaults | `SaveData.cs` | Fresh game = blank state, Money=0, no starter items. |

### 🟡 HIGH — Risks nghiêm trọng

| # | Risk | File | Impact |
|:-:|------|------|--------|
| **H1** | `Boot 1.unity` + `Main 1.unity` stale copies in Assets | `Scenes/` | Confusion, potential build ambiguity |
| **H2** | JsonUtility deserialization — value types missing fields become 0/false | `SaveService.cs:41` | Save data corruption invisible silent |
| **H3** | No [TearDown] cleanup in tests | All test files | Build artifacts accumulate |
| **H4** | UIRuntimeBootstrap chạy ngay cả khi database build lỗi | `UIRuntimeBootstrap.cs:58-62` | UI hiện lên nhưng blank + no error feedback |
| **H5** | MigrateSave() is empty placeholder | `SaveService.cs:138-141` | No migration possible |
| **H6** | PopupScreen detection uses screen.IsPopup + type check | `UIRuntimeBootstrap.cs:72-76` | Fragile — breaks with other popup types |
| **H7** | Stale timer data (Tavern, LastAccess) không được validate on load | `SaveData.cs` | Timer state out of sync |

### 🟢 LOW — Minor / Intentional

| # | Item | Lý do |
|:-:|------|-------|
| L1 | No DI container | Hand-wired ServiceContainer — fine for this scale |
| L2 | No Update loop | Event-driven architecture — expected for idle/incremental |
| L3 | No encryption | Not in scope for debug build |
| L4 | Zero prefabs | Acceptable for S6.5A scope; should be addressed in S7 |
| L5 | Empty localization.json / assets_manifest.json | Future features, not blocking |
| L6 | FormulaService hardcoded constants | All sourced from decompiled Formulas.java with source references |

---

## EXECUTIVE SUMMARY

### Tổng quan
Foundation audit hoàn tất. Project có 137 scripts, 13 data files (1,531 records), ServiceContainer với 18 runtime services. Code base structure sound, data loading pipeline hoạt động tốt, nhưng có **3 vấn đề critical** về boot flow cần giải quyết trước khi chuyển sang Audit 2/3.

### Critical Path Items (must fix before Audit 2/3)

1. **Clean up dead bootstrap code**:
   - Merge `Bootstrap/Bootstrapper.cs` → `Runtime/Boot/Bootstrapper.cs` (hoặc xoá 1 trong 2)
   - Đảm bảo `Runtime/Boot/Bootstrapper.cs` được attach vào Boot.unity và RuntimeReady flag hoạt động
   - Hoặc đơn giản hơn: xoá `Bootstrap/Bootstrapper.cs` (không dùng), xoá `Runtime/Boot/Bootstrapper.cs` (không dùng), clean up UIRuntimeBootstrap để là composition root duy nhất

2. **Add offline progress call at startup**:
   - Trong `UIRuntimeBootstrap.Start()`, sau khi tạo ServiceContainer, gọi `Services.OfflineProgress.ApplyOfflineProgress(DateTimeOffset.UtcNow.ToUnixTimeSeconds())`

3. **Add fresh-save initialization**:
   - Trong `SaveData.NormalizeAfterLoad()`, set default values cho fields quan trọng (Money > 0, LevelStorage = 1, settings defaults)

4. **Add DataVersion tracking**:
   - Set `Metadata.DataVersion` in `Save()` and validate in `Load()`

5. **Isolate test saves**:
   - Thêm `[TearDown]` cleanup save files
   - Hoặc tạo in-memory SaveService for tests

### Evidence Package
- `source_inventory.txt` — Full list of 137 .cs files
- `asset_inventory.txt` — Full list of 11 .unity/.prefab/.asset files
- `manifest.json` read — Data schema version 1.0, 13 files, 1,531 records
- FormulaService source — 252 lines with source annotations
- SaveService source — 144 lines with full error handling
- Boot flow analysis — 2 dead boot paths

---

*Audit hoàn tất. 15 foundation categories assessed. 8 critical blockers, 7 high risks identified.*
