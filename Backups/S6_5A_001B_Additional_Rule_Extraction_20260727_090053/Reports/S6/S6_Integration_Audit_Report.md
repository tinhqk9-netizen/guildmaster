# S6 Integration Audit Report

**Ngày:** 2026-07-25 · **Batch:** S6 Batch 1 — Audit & Plan Only (không sửa code) · **Verify bằng:** đọc trực tiếp source/scene/data trên đĩa + Editor.log

## Current Runtime State
| Area | Status | Evidence | Risk |
|---|---|---|---|
| Compile | **0 lỗi CS** | `Editor.log` không có `error CS` (log liên tục từ sau S5 repair, chưa restart) | Thấp |
| Playable entry point | Chỉ `Main.unity` mở tay + Play trong Editor chạy được | `UIRuntimeBootstrap` log `Wired 8 screen(s)` xác nhận | — |
| Build Settings scenes | **KHÔNG khớp scene thật** | `ProjectSettings/EditorBuildSettings.asset`: `Assets/Scenes/SampleScene.unity`, `Boot 1.unity`, `Main 1.unity` — không có `Boot.unity`/`Main.unity` (bản có UI/wiring thật) | **Cao** — Build/Build&Run sẽ ra scene rỗng |
| Duplicate scene files | `Boot 1.unity` & `Main 1.unity` chỉ có 1 Main Camera, gần như byte-identical với nhau, không có script nào gắn | So sánh trực tiếp nội dung `.unity` | Trung bình — gây nhầm lẫn, cần dọn hoặc named rõ |
| Duplicate Bootstrapper class | 2 class cùng tên `Bootstrapper` khác namespace: `GuildMaster.Bootstrap.Bootstrapper` (S1, dead-end — `LoadMainScene()` chỉ log, không `SceneManager.LoadScene`) và `GuildMaster.Runtime.Boot.Bootstrapper` (S2/S3 composition root: Database/Factory/FormulaService/SaveService) | Đọc 2 file `Bootstrapper.cs` | Trung bình |
| `Runtime.Boot.Bootstrapper` gắn vào scene nào? | **Không gắn ở đâu cả** — không tìm thấy component trong bất kỳ `.unity` nào | `grep "class Bootstrapper"` + grep scene files | **Cao** — composition root S2/S3 hiện là dead code, không chạy runtime |
| Composition thực sự chạy | `UIRuntimeBootstrap.cs` (S5) tự dựng lại 1 bản composition **riêng, trùng lặp** (GameDatabase, SaveService, FormulaService, RuntimeFactory, ItemService, InventoryService) ngay trong `Start()`, không tái dùng `Runtime.Boot.Bootstrapper` | Đọc `UIRuntimeBootstrap.cs:37-53` | Trung bình — 2 composition path song song, dễ lệch nếu sửa 1 bên quên bên kia |
| `DatabaseBuilder.Build()` report bị bỏ qua trong path đang chạy thật | `UIRuntimeBootstrap.cs:45`: `new DatabaseBuilder(...).Build();` — kết quả trả về không được kiểm tra `hasFatalErrors`, không log ra Console | Đọc code | Trung bình — nếu load data lỗi ở build thật (non-Editor), sẽ fail âm thầm |

## Data Integration
| Data Type | Source File | Loaded? | Count | Status |
|---|---|---:|---:|---|
| Manifest | `manifest.json` (qua `EditorExternalGameDataProvider`, mặc định trỏ `D:\Tinh\Game Decode Converter\output\production_staging`, **NGOÀI** thư mục Unity project) | Có (Editor only) | 10 categories | OK trong Editor; **build thật (non-Editor) cần `Assets/StreamingAssets/GameData` — thư mục này hiện RỖNG** |
| adventurers | adventurers.json | Có (qua DatabaseBuilder → `AdventurerDefinition`) | 129 | Loaded |
| dungeons | dungeons.json | Có → `DungeonDefinition` | 11 | Loaded |
| enemies | enemies.json | Có → `EnemyDefinition` | 122 | Loaded |
| items | items.json | Có → `ItemDefinition` | 607 | Loaded |
| pets | pets.json | Có → `PetDefinition` | 21 | Loaded |
| quests | quests.json | Có → `QuestDefinition` | 56 | Loaded |
| raids | raids.json | Có → `RaidDefinition` | 12 | Loaded |
| recipes | recipes.json | Có → `RecipeDefinition` | 321 | Loaded |
| skills | skills.json | Có → `SkillDefinition` | 227 | Loaded |
| status_effects | status_effects.json | Có → `StatusEffectDefinition` | 25 | Loaded |
| localization | localization.json | Có, nhưng qua `LocalizationService` riêng (không qua manifest loop) | — | **Không dùng ở đâu** — không UI screen nào gọi `ILocalizationService.GetText()` |
| assets_manifest | assets_manifest.json | Không dùng bởi `AssetManifestService` trong runtime path hiện tại (chỉ dùng ở Editor: `AssetCatalogBuilder`) | — | Chỉ Editor-time |
| **StreamingAssets/GameData** | Assets/StreamingAssets/GameData | **RỖNG** (0 file) | 0 | **Blocker cho build thật** — `StreamingAssetsGameDataProvider` sẽ throw `FileNotFoundException` khi tìm `manifest.json` |
| Bản sao data trong repo Unity | `Tools/DecodeConverter/output/production_staging/*.json` | Không được code nào đọc (`EditorExternalGameDataProvider` trỏ ra ngoài `D:\Tinh\Game Decode Converter\...`) | 15 file | **Stale/không dùng** — md5 khác bản thật đang được đọc (record count trùng nhưng `generatedAt` khác: 2026-07-13 vs 2026-07-17) → dễ gây nhầm "sửa nhầm bản không dùng" |
| `Assets/_Game/Data/Definitions`, `Assets/_Game/Data/Runtime` | thư mục thật | Rỗng (0 file) | 0 | Không dùng — chỉ có `.meta`, không phải blocker vì không có code nào tham chiếu |

**DefinitionRegistry (GameDatabase) khi Build() chạy thành công trong Editor:** 10 loại definition, tổng **1531 record thật** từ decode (129+11+122+607+21+56+12+321+227+25). Đã có test tích hợp thật (`S3B2DDataImportTests.cs`) assert `recipes.Count > 0`, `dungeons.Count > 0`, offers > 0 bằng `EditorExternalGameDataProvider` thật — không phải mock.

## Service Wiring Matrix
| Service | Exists | Constructed At Runtime | Has Dependencies | Used By UI | Status |
|---|---|---|---|---|---|
| ItemService | ✅ | ✅ (`UIRuntimeBootstrap`) | `RuntimeFactory`, `GameDatabase` | Gián tiếp qua InventoryService | **WIRED** |
| InventoryService | ✅ | ✅ (`UIRuntimeBootstrap`) | Save/Formula/Item/DB | ✅ `InventoryScreen` | **WIRED** |
| EquipmentService | ✅ | ❌ | `IInventoryService`, `ISaveService` | ❌ | Implemented, chỉ test (`S2VerificationTests`), chưa wire |
| CharacterService | ✅ | ❌ | Save/Formula/DB/Factory/Inventory | ❌ (CharacterScreen đọc thẳng `SaveData.Characters`, bỏ qua service) | Implemented + có test, chưa wire; UI đọc raw save data |
| EnemyService | ✅ | ❌ | `GameDatabase`, `IInstanceIdGenerator` | ❌ | Implemented, **0 test**, chưa wire |
| SkillService | ✅ | ❌ | Stateless helper (không constructor DI) | ❌ | Implemented, **0 test**, chưa wire |
| StatusEffectService | ✅ | ❌ | Stateless helper (nhận `CharacterRuntime`/`EnemyRuntime` theo method) | ❌ | Implemented, **0 test**, chưa wire |
| DungeonService | ✅ | ❌ | `ISaveService`, `GameDatabase` | ❌ (DungeonScreen = placeholder trắng) | Implemented, **0 test**, chưa wire |
| CombatService | ✅ | ❌ | Stateless helper (`ProcessTurn(...)`) | ❌ | Implemented, **0 test**, chưa wire |
| LootService | ✅ | ❌ | Stateless helper (`RollLoot(...)`) | ❌ | Implemented, **0 test**, chưa wire |
| QuestService | ✅ | ❌ | `ISaveService`, `GameDatabase` | ❌ | Implemented, **0 test**, chưa wire |
| CraftService | ✅ | ❌ | `GameDatabase`, `IInventoryService`, `ISaveService` | ❌ (CraftScreen = placeholder trắng) | Implemented, **0 test**, chưa wire |
| MerchantService | ✅ | ❌ | `GameDatabase`, `IInventoryService`, `ISaveService` | ❌ (MerchantScreen = placeholder trắng) | Implemented, **0 test**, chưa wire |
| OfflineProgressService | ✅ | ❌ | `ISaveService`, `ICraftService`, `IMerchantService` | ❌ | Implemented, **0 test**, chưa wire — phụ thuộc Craft+Merchant nên phải wire sau 2 service đó |
| DatabaseService | ✅ | ❌ (chỉ trong `Bootstrap.Bootstrapper` dead-end) | `GameDatabase` | ❌ | Thin wrapper, không ai dùng |
| LocalizationService | ✅ | ❌ ở path thật (chỉ trong dead-end Bootstrapper) | `IGameDataProvider`, `IJsonSerializer` | ❌ | Load được nhưng không UI nào gọi `GetText` |
| AssetManifestService | ✅ | Chỉ Editor-time (`AssetCatalogBuilder`) | provider/serializer | N/A (Editor tool) | Đúng scope, không cần runtime |

**Tổng kết:** 2/14 gameplay service (`ItemService`, `InventoryService`) thực sự chạy ở runtime. 12/14 đã code xong, compile sạch, nhưng **chưa từng được `new` lên ngoài test** (và 9 trong số đó chưa có test nào luôn) — đây là core gap mà S6-002 phải lấp.

## UI Integration Matrix
| Screen | Runtime Registered | Data Source | Shows Real Data | Placeholder | Status |
|---|---|---|---|---|---|
| HUD | ✅ | `ISaveService.CurrentData.Money/Gems` | ✅ (0/0 mặc định, đúng vì chưa có gameplay tạo currency) | Không | FUNCTIONAL |
| Inventory | ✅ | `IInventoryService.GetAllItems()` | ✅ ("Inventory is empty." — đúng, chưa có item nào được add) | Không | FUNCTIONAL |
| Character | ✅ | **`ISaveService.CurrentData.Characters` trực tiếp** (bỏ qua `CharacterService`) | ✅ ("No characters available." — đúng, chưa có nhân vật khởi tạo) | Không, nhưng không dùng service tầng đúng | FUNCTIONAL (nhưng kiến trúc lệch — nên đổi sang CharacterService ở S6-003) |
| Dungeon | ✅ (đăng ký UIScreen) | Không có | N/A | Có (panel trắng + Back) | PLACEHOLDER_PANEL |
| Craft | ✅ | Không có | N/A | Có | PLACEHOLDER_PANEL |
| Merchant | ✅ | Không có | N/A | Có | PLACEHOLDER_PANEL |
| Settings | ✅ | Không có | N/A | Có | PLACEHOLDER_PANEL |
| Popup | ✅ (`RegisterDialogScreen`) | Gọi thủ công qua `IUIService` | N/A | Không (dùng thật, chỉ chưa ai gọi) | FUNCTIONAL, chưa có caller |

## Save Integration
| Feature | Status | Evidence |
|---|---|---|
| SaveService tồn tại | ✅ | `Runtime/Save/SaveService.cs` |
| Load save | ✅ hoạt động | `Load()` fallback về `SaveData` rỗng nếu chưa có file — đúng hành vi, không fake |
| **Save (ghi file) có được gọi ở đâu trong game code không?** | ❌ **KHÔNG** | `grep "\.Save(out"` trên toàn bộ `Assets/_Game/Scripts` (trừ Tests) → 0 kết quả. Không có `OnApplicationQuit`/`OnApplicationPause` hook nào gọi Save |
| Save file thật đã từng sinh ra chưa | ❌ Chưa | Không tìm thấy `save.json` trong `Application.persistentDataPath` (`AppData/LocalLow/...`) |
| Auto-save / manual save | ❌ Không có cơ chế nào | Không có timer, không có nút Save trong UI hiện tại |
| SaveData schema | Đầy đủ | `SaveData.cs`: Money, Gems, LevelStorage, UpgradeStorage, WorkshopQueue, MarketListings, Items, Characters, Quests, Dungeons, Skills, ActiveDungeon |
| In-memory mutation | ✅ | `InventoryService.SyncToSave()` ghi vào `CurrentData.Items` trong RAM mỗi lần add/remove — nhưng mất khi thoát app vì không `Save()` |

## Deferred / ManualRuleRequired Areas
| Area | Reason | Safe Behavior |
|---|---|---|
| Character creation flow | Không có code nào tạo `CharacterSaveData` khởi tạo (starter adventurer) — chưa rõ rule gốc (Java) cho "adventurer khởi đầu" | Giữ nguyên "No characters available." — không tự bịa nhân vật mặc định |
| Dungeon/Craft/Merchant/Settings UI thật | Ngoài scope S5, cần rule/luồng chi tiết (giá, công thức hiển thị, danh sách quái theo dungeon...) | Giữ placeholder cho tới khi có task riêng xác định rule |
| CombatService/LootService integration vào 1 luồng dungeon chạy được | Cần DungeonService + EnemyService + CombatService + LootService phối hợp, chưa có "dungeon run" orchestrator nào được viết | DEFERRED — thiết kế orchestrator là việc S6-002, không tự suy đoán rule combat/loot chưa decode rõ |
| OfflineProgressService | Phụ thuộc Craft+Merchant đã wire xong mới có ý nghĩa | DEFERRED tới sau S6-002 phần Craft/Merchant |
| StreamingAssets/GameData rỗng | Build thật (device/standalone) sẽ không đọc được data vì thư mục rỗng; hiện tại project chỉ chạy được qua Editor nhờ `EditorExternalGameDataProvider` trỏ ra ngoài dataPath | MANUAL_RULE_REQUIRED — cần quyết định: (a) copy production_staging JSON vào StreamingAssets/GameData như một build step, hay (b) giữ nguyên vì phạm vi hiện tại chỉ cần chạy trong Editor để nghiệm thu. Không tự ý copy nếu chưa rõ có phải "build thật" nằm trong phạm vi S6 hay không |
| Localization không được dùng | Không UI screen nào gọi `ILocalizationService` — text hiện là tiếng Anh hard-code trong code generator | Deferred, không phải bug chặn |

## Recommended S6 Implementation Plan

- **S6-001 Data Integration** — Quyết định rule cho `StreamingAssets/GameData` (copy JSON vào đó hay giữ Editor-only); dọn/khai báo rõ 2 bản `production_staging` (repo vs `D:\Tinh\Game Decode Converter`) để tránh sửa nhầm bản không dùng; không đổi format DTO nếu không cần.
- **S6-002 Runtime Integration** — Gộp 1 composition root duy nhất (khuyến nghị: giữ `UIRuntimeBootstrap` làm nguồn thật, xoá/khai tử `Bootstrap/Bootstrapper.cs` và `Runtime/Boot/Bootstrapper.cs` nếu không dùng, hoặc merge `UIRuntimeBootstrap` để tái dùng `Runtime.Boot.Bootstrapper`); construct thêm `EquipmentService`, `CharacterService`, `EnemyService`, `DungeonService`, `QuestService`, `CraftService`, `MerchantService`, `OfflineProgressService`; sửa Build Settings để trỏ đúng `Boot.unity`/`Main.unity` thật (hoặc xoá scene rỗng trùng lặp).
- **S6-003 UI Integration** — Đổi `CharacterScreen` sang đọc qua `CharacterService` thay vì raw SaveData; giữ Dungeon/Craft/Merchant/Settings placeholder cho tới khi có service+rule rõ ràng (không tự làm UI thật nếu rule combat/craft/merchant chưa xác nhận).
- **S6-004 Save Integration** — Thêm nơi gọi `SaveService.Save()` thật (ví dụ sau mỗi thao tác thay đổi state quan trọng, hoặc `OnApplicationQuit`/`OnApplicationPause`), xác nhận sinh ra `save.json` thật trong `persistentDataPath`.
- **S6-005 Performance Optimization** — Sau khi có runtime flow thật để đo (chưa đo được vì chưa có gameplay loop chạy); benchmark sau S6-002/003.
- **S6-006 Regression Testing** — Chạy lại toàn bộ EditMode tests + smoke test sau mỗi thay đổi wiring; thêm test tích hợp cho các service mới được wire.
- **S6-007 Bug Fixing** — Theo defect phát sinh từ 006.
- **S6-008 Sprint Review** — Tổng hợp báo cáo cuối S6.

## Final Decision
# `S6_AUDIT_DONE_READY_FOR_IMPLEMENTATION`

**Lý do:** Không có lỗi compile, không có scope violation, dữ liệu decode thật tồn tại và load được (verify bằng test tích hợp thật `S3B2DDataImportTests`). Có gap rõ ràng và đã định vị chính xác (composition root trùng lặp, Build Settings sai scene, 12/14 service chưa wire, save không bao giờ ghi file, StreamingAssets rỗng) nhưng đều là việc **implement có kế hoạch rõ**, không phải thiếu dữ liệu không thể xác minh. Riêng mục "StreamingAssets/GameData rỗng" được đánh dấu MANUAL_RULE_REQUIRED — cần quyết định phạm vi (chỉ chạy Editor hay cần build thật) trước khi S6-001 đụng vào, không tự quyết.
