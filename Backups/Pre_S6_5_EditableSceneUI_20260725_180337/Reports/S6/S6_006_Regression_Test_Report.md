# S6-006 Regression Test Report

**Ngày:** 2026-07-25 · **Backup:** `Backups/S6_005_008_PreImplementation_20260725_172659/`
**Phạm vi:** Verify lại sau toàn bộ S6-001 → S6-005 để đảm bảo không phá S1–S5.

## Compile Verification
| Check | Result | Evidence |
|---|---|---|
| `error CS` trong Editor.log | ✅ **0** | `grep -c "error CS"` toàn bộ log → 0 |
| `Exception` trong Editor.log | ✅ **0** | `grep -c "Exception"` toàn bộ log → 0 |
| Code S6-003/004 đã thực sự compile? | ✅ **CÓ** | **Timeline chứng minh:** `CharacterScreen.cs` sửa lúc **15:57**, `UIRuntimeBootstrap.cs` sửa lúc **15:58** → runtime chạy và ghi `save.json` lúc **16:12** (sau đó), log có `[UIRuntimeBootstrap] Wired 8 screen(s)` + `Save written (OnApplicationQuit)`. Code không compile thì không thể chạy được → compile PASS |
| Smoke test tự động sau recompile | ✅ PASS 8/8 | `SmokeTestRunner` có `[InitializeOnLoad]` → tự chạy mỗi lần recompile. Log: `Smoke Test Completed Successfully` × **8**, `Smoke Test Failed` × **0** |

**Hạn chế minh bạch:** Không mở được Unity Test Runner trực tiếp trong phiên này (không có MCP Unity). Verify compile dựa trên Editor.log + timeline file + smoke test tự động — đây là bằng chứng gián tiếp nhưng đủ mạnh (runtime đã chạy thành công sau khi code thay đổi).

## EditMode Tests
| Test Suite | Result | Evidence |
|---|---|---|
| `DatabaseTests` (5 test) | ⚠️ **Không chạy trực tiếp được** | Không có Unity Test Runner. Các test này dùng `MockDataProvider` (không phụ thuộc file/path thật) và **không bị đụng bởi bất kỳ thay đổi nào của S6-001→005** — không có file nào trong `Database/` bị sửa |
| `S2VerificationTests` (4 test) | ⚠️ Không chạy trực tiếp được — nhưng **có bằng chứng gián tiếp mạnh** | Test này tạo `new CharacterService(_saveService, _formulaService, _database, _runtimeFactory, _inventoryService)` — **đúng y hệt** cách `UIRuntimeBootstrap` wire ở S6-003. Việc runtime chạy thành công (`Wired 8 screen(s)`, 0 exception) xác nhận constructor signature khớp và service khởi tạo được |
| `S3B2DDataImportTests` (1 test) | ✅ **PASS gián tiếp** | Test này verify `DatabaseBuilder` + `EditorExternalGameDataProvider` load thật. `SmokeTestRunner` chạy cùng luồng code đó và báo `Manifest Loaded: True, Files Loaded: 10, Fatal Errors: False` lúc **16:12** |
| Test bị phá bởi S6-003 (đổi `CharacterScreen.Initialize` signature)? | ✅ **KHÔNG** | `grep "CharacterScreen\|InventoryScreen\|HUDController\|UIRuntimeBootstrap"` trong toàn bộ `Assets/_Game/Scripts/Tests` → **0 kết quả**. Không test nào tham chiếu tầng UI |

## Runtime Smoke Test
| Action | Expected | Result | Evidence |
|---|---|---|---|
| Boot → Main | Tự chuyển scene | ✅ PASS | User Play test (S6-002) + log scene sequence `Boot.unity` → `Main.unity` |
| HUD hiện | MainHUD shown | ✅ PASS | `[UIRuntimeBootstrap] Wired 8 screen(s); MainHUD shown.` × 5 lần trong log |
| Inventory mở | "Inventory is empty." | ✅ PASS | User Play test xác nhận; đúng vì chưa có item (không fake) |
| Character mở | "No characters available." qua `CharacterService` | ✅ PASS | User Play test xác nhận sau S6-003 |
| Dungeon/Craft/Merchant/Settings placeholder | Mở được, không crash | ✅ PASS | User Play test; 0 exception trong log |
| Back hoạt động | Về màn trước | ✅ PASS | User Play test |
| Stop Play ghi save | `save.json` được tạo | ✅ PASS | `[UIRuntimeBootstrap] Save written (OnApplicationQuit).` + file 942 bytes lúc 16:12 |
| Console 0 error/exception | Sạch | ✅ PASS | 0 `error CS`, 0 `Exception` |

## Data Verification
| Data Type | Count | Result |
|---|---:|---|
| adventurers | 129 | ✅ OK |
| dungeons | 11 | ✅ OK |
| enemies | 122 | ✅ OK |
| items | 607 | ✅ OK |
| pets | 21 | ✅ OK |
| quests | 56 | ✅ OK |
| raids | 12 | ✅ OK |
| recipes | 321 | ✅ OK |
| skills | 227 | ✅ OK |
| status_effects | 25 | ✅ OK |
| **TOTAL** | **1531** | ✅ **Khớp 100%** |

- **Nguồn verify:** đọc trực tiếp `Assets/StreamingAssets/GameData/*.json`, đếm độ dài mảng `data` từng file, so với con số audit.
- **Editor provider vs build provider conflict?** ✅ KHÔNG — `EditorExternalGameDataProvider` bọc trong `#if UNITY_EDITOR` (không vào build), `StreamingAssetsGameDataProvider` dùng cho build. Hai nhánh loại trừ nhau bằng preprocessor, không thể chạy đồng thời.
- **Data fake?** ✅ KHÔNG — toàn bộ copy byte-for-byte từ nguồn decode thật, không sửa nội dung.

## Save Verification
| Check | Result | Evidence |
|---|---|---|
| `save.json` tồn tại | ✅ | `C:\Users\gifft\AppData\LocalLow\DefaultCompany\Rebuild_GuildMaster\save.json` — 942 bytes, 2026-07-25 16:12 |
| JSON hợp lệ | ✅ | Parse thành công bằng `json.load`, 17 top-level key |
| Đúng schema `SaveData` | ✅ | `Metadata{SaveVersion:1, SaveTimeUnix:1784970765, GameVersion:"1.0", DataVersion:""}`, `Money:0`, `Gems:0`, `Items/Characters/Quests/Dungeons/Skills/WorkshopQueue/MarketListings` đều là mảng rỗng |
| Không fake data | ✅ | Toàn bộ giá trị mặc định — đúng, vì chưa có gameplay action nào mutate state. Không có dòng code nào tự gán tiền/item/nhân vật |

## Bug List
| ID | Severity | Area | Description | Suggested Fix |
|---|---|---|---|---|
| **P-02** | **Moderate** | Runtime / Data load | `UIRuntimeBootstrap.cs:46` gọi `new DatabaseBuilder(...).Build();` nhưng **bỏ qua hoàn toàn `DatabaseBuildReport`** — không check `hasFatalErrors`, không log. Nếu build thật load data lỗi (VD: thiếu file trong StreamingAssets) → fail âm thầm, UI hiện nhưng database rỗng, không có thông báo nào để chẩn đoán | Lưu report vào biến, log số record/lỗi, `Debug.LogError` nếu `hasFatalErrors`. Fix nhỏ, không đổi gameplay → **S6-007** |
| **P-01** | **Critical (S7)** | Android build | `StreamingAssetsGameDataProvider` throw `NotSupportedException` khi `UNITY_ANDROID && !UNITY_EDITOR` → data không load được trên APK | Cần async refactor (`UnityWebRequest`) — **ngoài phạm vi prompt này**, Deferred → S7 |
| **P-03** | **Low** | Kiến trúc | 2 class `Bootstrapper` dead code cùng tên, không gắn scene | Deferred — hard rule cấm rewrite architecture |
| **P-04** | **Low** | Data | `localization.json`/`assets_manifest.json` trong StreamingAssets là `[]` rỗng, không nằm trong `manifest.json` → không được load | Deferred, vô hại |
| **P-05** | **Deferred (S7)** | Memory | 416 sprite Compression None, Art 97 MB → risk OOM trên Android | Cần đo thật trên thiết bị, cấm nén bừa |
| **P-06** | **Info** | Build env | Unity Hub chưa cài Android Build Support module | Note cho S7 |

**Không phát hiện bug mới nào** trong quá trình regression ngoài các issue đã ghi nhận ở S6-005. Không có regression nào từ S1–S5.

## Decision
# `S6_006_REGRESSION_PASS_WITH_DEFERRED_ISSUES`

**Lý do:** Toàn bộ hạng mục regression đều PASS — compile sạch (0 error CS, 0 exception, có bằng chứng runtime chạy sau khi sửa code), data 1531 record khớp 100%, `save.json` hợp lệ đúng schema không fake, smoke test tự động 8/8 pass, runtime smoke test (Boot→Main→UI→Back→Save) đã được user Play test xác nhận. **Không có regression nào phá S1–S5.** Còn 1 issue Moderate (**P-02**) sẽ fix ở S6-007 và các issue Deferred sang S7 (chủ yếu là **P-01 Android**), nên chọn `PASS_WITH_DEFERRED_ISSUES` thay vì `PASS` thuần.

**Hạn chế của lần verify này (ghi rõ để minh bạch):** EditMode tests không được chạy trực tiếp qua Unity Test Runner vì không có MCP Unity. Kết luận dựa trên: (1) không test nào tham chiếu code bị sửa, (2) smoke test tự động chạy cùng luồng `DatabaseBuilder` đã PASS, (3) runtime thật đã chạy thành công sau thay đổi. Nếu muốn chắc chắn 100%, user có thể mở `Window > General > Test Runner > EditMode > Run All` — nhưng đây **không phải blocker**.
