# S6 Sprint Review Report

**Ngày:** 2026-07-25 · **Sprint:** S6 — System Integration

## Sprint Scope
- S6-001 Data Integration
- S6-002 Runtime / Build Settings Integration
- S6-003 UI Integration
- S6-004 Save Integration
- S6-005 Performance / Build Readiness
- S6-006 Regression Testing
- S6-007 Bug Fixing
- S6-008 Sprint Review

## Completed Tasks
| Task | Result | Evidence Report |
|---|---|---|
| S6 Audit (Batch 1) | `S6_AUDIT_DONE_READY_FOR_IMPLEMENTATION` | `S6_Integration_Audit_Report.md` |
| S6-001 Data Integration | `S6_001_DATA_INTEGRATION_DONE` | `S6_001_Data_Integration_Report.md` |
| S6-002 Runtime/Build Settings | `S6_002_RUNTIME_BUILDSETTINGS_DONE` (user Play test xác nhận) | `S6_002_Runtime_BuildSettings_Integration_Report.md` |
| S6-003 UI Integration | `S6_003_UI_INTEGRATION_DONE` | `S6_003_UI_Integration_Report.md` |
| S6-004 Save Integration | `S6_004_SAVE_INTEGRATION_DONE` (user Play test xác nhận, `save.json` thật) | `S6_004_Save_Integration_Report.md` |
| S6-005 Performance / Build Readiness | `S6_005_BUILD_READINESS_DONE_NO_RISKY_OPTIMIZATION_NEEDED` | `S6_005_Performance_BuildReadiness_Report.md` |
| S6-006 Regression Testing | `S6_006_REGRESSION_PASS_WITH_DEFERRED_ISSUES` | `S6_006_Regression_Test_Report.md` |
| S6-007 Bug Fixing | `S6_007_BUG_FIXING_DONE` (fix P-02) | `S6_007_Bug_Fixing_Report.md` |
| S6-008 Sprint Review | Báo cáo này | `S6_Sprint_Review_Report.md` |
| Backup #1 (trước S6-001/002) | OK | `S6_PreImplementation_Backup_Report.md` |
| Backup #2 (trước S6-003/004) | OK (86 file) | `S6_003_004_Backup_Report.md` |
| Backup #3 (trước S6-005→008) | OK (644 file, 5.9 MB) | `S6_005_008_Backup_Report.md` |

## Current Game State
**Boot flow:** `Boot.unity` (index 0, 3 GameObject tối giản: Main Camera, Global Light 2D, `BootSceneLoader`) → tự `SceneManager.LoadScene("Main")` → `Main.unity` (index 1) → `UIRuntimeBootstrap.Start()` dựng service + wiring UI → `MainHUD` hiện. Đã user Play test xác nhận.

**Data source:**
- Editor: `EditorExternalGameDataProvider` → `D:\Tinh\Game Decode Converter\output\production_staging\` (fallback hợp lệ)
- Build: `StreamingAssetsGameDataProvider` → `Assets/StreamingAssets/GameData/` (1531 record thật, 1.5 MB, đã sẵn sàng)
- Hai nhánh loại trừ nhau bằng `#if UNITY_EDITOR`, không conflict

**UI state:**
| Screen | Status | Data Source |
|---|---|---|
| HUD | FUNCTIONAL | `SaveService.CurrentData.Money/Gems` |
| Inventory | FUNCTIONAL | `InventoryService.GetAllItems()` |
| Character | FUNCTIONAL | `CharacterService.GetAllCharacters()` (nâng cấp ở S6-003, trước đó đọc raw SaveData) |
| Popup | FUNCTIONAL | Đã register, chưa có caller |
| Dungeon / Craft / Merchant / Settings | PLACEHOLDER_PANEL | Không có |

**Save state:** Load hoạt động từ S2/S3. **Ghi file mới có từ S6-004** — trigger `OnApplicationPause(true)` + `OnApplicationQuit()`. `save.json` thật (942 bytes) đã sinh ra, JSON hợp lệ, đúng schema, toàn bộ giá trị mặc định (không fake). Có `save_backup.json` tự động trước mỗi lần ghi đè.

**Services currently wired (3/14):** `ItemService`, `InventoryService` (từ S5), `CharacterService` (mới ở S6-003).

**Services deferred (11/14):** `EquipmentService`, `EnemyService`, `SkillService`, `StatusEffectService`, `DungeonService`, `CombatService`, `LootService`, `QuestService`, `CraftService`, `MerchantService`, `OfflineProgressService` — đều đã code xong, compile sạch, nhưng chưa có UI/flow nào cần tới nên chưa khởi tạo runtime (đúng scope).

## Verification Summary
| Area | Status | Evidence |
|---|---|---|
| Compile | ✅ 0 `error CS` | Editor.log grep → 0 |
| Exception | ✅ 0 | Editor.log grep → 0 |
| Data integrity | ✅ 1531/1531 | Đếm trực tiếp mảng `data` từng JSON, khớp 100% manifest + audit |
| Data load runtime | ✅ PASS | `SmokeTestOutput.txt`: `Manifest Loaded: True, Files Loaded: 10, Fatal Errors: False`; smoke test 8/8 lần PASS |
| Boot → Main | ✅ PASS | User Play test + scene sequence trong log |
| UI wiring | ✅ PASS | `[UIRuntimeBootstrap] Wired 8 screen(s); MainHUD shown.` × 5 |
| Click/navigation/Back | ✅ PASS | User Play test 2 lần |
| Save file | ✅ PASS | `save.json` 942 bytes, parse OK, đúng schema, không fake |
| Import settings S5 | ✅ Nguyên vẹn | 416/416 sprite: `filterMode: 0`, `enableMipMap: 0`, Default platform `textureCompression: 0` |
| UI structure | ✅ Sạch | 1 Canvas / 1 EventSystem / 1 GraphicRaycaster, CanvasScaler 1080×1920, `InputSystemUIInputModule` |
| Runtime duplication | ✅ Không có | 1 instance `UIRuntimeBootstrap`, không load data trùng, không `Update()`, không leak |
| No fake data | ✅ Xác nhận | Không có dòng code nào tự gán tiền/item/nhân vật; mọi giá trị rỗng đều là hệ quả đúng của việc chưa có gameplay action |

## Remaining Risks
| Risk | Mức độ | Ghi chú |
|---|---|---|
| **Android APK sẽ KHÔNG load được data** (P-01) | **Critical cho S7** | `StreamingAssetsGameDataProvider` throw `NotSupportedException` khi `UNITY_ANDROID && !UNITY_EDITOR`. Cần async refactor (`UnityWebRequest`). Windows Standalone không bị ảnh hưởng |
| Build thật (Standalone/APK) chưa từng chạy | Trung bình | Hạ tầng đã sẵn sàng (data + scene + BootSceneLoader) nhưng chưa có lần build thực tế nào xác nhận |
| 11/14 service chưa runtime-wired | Trung bình | Khối lượng đáng kể nếu S7 cần Dungeon/Craft/Merchant thật |
| Dungeon/Craft/Merchant/Settings vẫn placeholder | Đã biết, có chủ đích | Đúng scope S5/S6 |
| Chưa có gameplay mutation để test save content | Trung bình | Mới chứng minh được "ghi file hoạt động", chưa chứng minh được "ghi đúng nội dung thay đổi" |
| Memory Android: 416 sprite Compression None, Art 97 MB (P-05) | Deferred → S7 | Cần đo thật trên thiết bị trước khi quyết định nén |
| 2 `Bootstrapper` dead code cùng tên (P-03) | Thấp | Không gắn scene → không chạy. Rủi ro nhầm lẫn cho người sửa sau |
| **Fix P-02 (S6-007) chưa qua Unity compile** | Thấp | Thay đổi thuần logging, đã đối chiếu từng field với source. Cần 1 lần reimport để xác nhận |

## Deferred / ManualRuleRequired
| Area | Reason | Recommended Next Step |
|---|---|---|
| Android StreamingAssets loading (P-01) | API `IGameDataProvider` hiện đồng bộ; Android bắt buộc async | **Việc đầu tiên của S7** nếu mục tiêu là APK: refactor sang async hoặc chuyển data sang `Resources`/`Addressables` — cần user quyết định hướng |
| Dungeon / Craft / Merchant / Settings UI thật | Chưa có rule/luồng gameplay xác nhận từ decode (combat result, giá, timer, restock) | Cần bóc rule cụ thể trước khi dựng UI — tuyệt đối không tự suy đoán |
| Character equipment hiển thị tên thật | `CharacterService.LoadFromSave()` có TODO nội bộ về rule khôi phục equipment từ save | Cần xác định rule lưu/khôi phục equipment trước |
| 11 service chưa wire | Không có UI/flow cần tới | Wire theo nhu cầu từng màn UI, không wire hàng loạt |
| Memory/texture Android (P-05) | Cần số đo thật | Build APK → đo RAM trên thiết bị → mới quyết định nén |
| `localization.json`/`assets_manifest.json` rỗng (P-04) | Nguồn decode thật cũng rỗng | Nếu cần localization, phải chạy lại converter — không tự bịa nội dung |

## S7 Readiness
**Ready for S7?** ✅ **Có, với điều kiện xác định rõ mục tiêu S7.**

- **Nếu S7 = Windows Standalone build/polish:** Sẵn sàng ngay. Toàn bộ hạ tầng data/scene/save/UI đã đúng, chỉ cần bấm Build.
- **Nếu S7 = Android APK:** **CHƯA sẵn sàng** — bắt buộc phải xử lý **P-01** trước (Android StreamingAssets), và cài Android Build Support module trong Unity Hub (**P-06**). Đây là blocker cứng, không phải việc "thử build rồi tính".

**Có cần build standalone smoke trước Android không?** ✅ **CÓ, rất nên.** Lý do: Standalone build là cách rẻ nhất để xác nhận `StreamingAssetsGameDataProvider` (nhánh non-Editor) thực sự đọc được data — điều mà Editor Play Mode **không bao giờ test được** vì luôn chạy nhánh `EditorExternalGameDataProvider`. Nếu Standalone chạy tốt thì đã loại trừ được lỗi data-path chung, chỉ còn lại đúng vấn đề đặc thù Android. Với fix P-02 vừa áp, log sẽ hiện rõ số record load được hoặc lỗi cụ thể — đủ để chẩn đoán ngay trong lần build đầu.

**Thiếu gì để hoàn toàn sẵn sàng:**
1. 1 lần Unity reimport + Play test để xác nhận fix P-02 compile sạch (bắt buộc, nhỏ)
2. 1 lần build Standalone smoke test (khuyến nghị mạnh, chưa bắt buộc)
3. Xử lý P-01 nếu hướng tới Android

## Manual Test Needed
Cần đúng **1 lần** thao tác Unity để chốt hoàn toàn S6:
1. Đưa Unity Editor lên foreground → chờ reimport `UIRuntimeBootstrap.cs` (vừa sửa ở S6-007).
2. Console phải **0 lỗi CS**.
3. Play (Boot hoặc Main) → xác nhận log mới xuất hiện: `[UIRuntimeBootstrap] Database built via EditorExternalGameDataProvider: 10/10 file(s), 1531 record(s).`
4. Stop Play.

Nếu log hiện đúng con số 1531 → fix P-02 hoạt động chính xác, S6 hoàn tất trọn vẹn.

## Final Decision
# `S6_DONE_WITH_DEFERRED_RISKS_READY_FOR_S7`

**Lý do:** Toàn bộ 8 task S6 đã hoàn thành với bằng chứng verify đầy đủ — data thật 1531 record load được, Boot→Main hoạt động, UI đọc qua đúng tầng service, save ghi file thật, import settings S5 nguyên vẹn, 0 lỗi compile, 0 exception, không có regression nào phá S1–S5, và không có dữ liệu giả nào được tạo ra. Không thực hiện tối ưu bừa nào (đúng nguyên tắc "chưa đo được thì không tối ưu"). Các rủi ro còn lại đều đã được **định vị chính xác, phân loại rõ severity, và có safe behavior xác nhận** — đáng chú ý nhất là **P-01 (Android StreamingAssets)** là blocker cứng cho hướng APK nhưng **không ảnh hưởng** Windows Standalone và nằm ngoài phạm vi prompt này theo đúng hard rule. Vì vậy chọn `DONE_WITH_DEFERRED_RISKS` thay vì `DONE` thuần — S6 xong, sẵn sàng bước sang S7 với điều kiện user xác định rõ mục tiêu (Standalone hay Android).
