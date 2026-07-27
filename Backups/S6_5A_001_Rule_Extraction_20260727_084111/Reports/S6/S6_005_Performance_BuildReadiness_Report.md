# S6-005 Performance / Build Readiness Report

**Ngày:** 2026-07-25 · **Backup:** `Backups/S6_005_008_PreImplementation_20260725_172659/`
**Nguyên tắc áp dụng:** Chưa có gameplay loop thật để đo profiling → **audit build readiness trước, không tối ưu bừa**. Không có thay đổi code nào được thực hiện trong task này.

## Data Load Readiness
| Check | Result | Evidence | Risk |
|---|---|---|---|
| GameDatabase load ở Editor | ✅ PASS | `Reports/SmokeTestOutput.txt`: `Manifest Loaded: True`, `Files Loaded: 10`, `Fatal Errors: False`, đủ 10 category (129/11/122/607/21/56/12/321/227/25) | Thấp |
| Tổng record thật | ✅ 1531 | Khớp 100% audit + manifest | Thấp |
| StreamingAssets/GameData có data cho build | ✅ Có (1.5 MB, 13 file) | `du -sh` + count verify ở S6-001 | Thấp |
| Load lặp nhiều lần? | ✅ KHÔNG | Chỉ **1** instance `UIRuntimeBootstrap` trong `Main.unity` (grep guid `1ca1c82e...` → count = 1); 0 instance trong `Boot.unity`. `DatabaseBuilder.Build()` chạy đúng 1 lần khi vào Main | Thấp |
| Hardcode path ngoài project còn gây rủi ro build? | ✅ KHÔNG | `EditorExternalGameDataProvider` bị bọc trong `#if UNITY_EDITOR`, không compile vào build. Build dùng `StreamingAssetsGameDataProvider` → `Application.streamingAssetsPath` | Thấp |
| **Kết quả `DatabaseBuilder.Build()` bị bỏ qua trong runtime path** | ⚠️ **VẤN ĐỀ THẬT** | `UIRuntimeBootstrap.cs:46`: `new DatabaseBuilder(provider, serializer, db).Build();` — không gán biến, không check `report.hasFatalErrors`, không log gì. Nếu build thật load data lỗi → **fail âm thầm**, UI vẫn hiện nhưng rỗng, không có thông báo nào để chẩn đoán | **Moderate** |
| Data size | 1.5 MB JSON (items.json lớn nhất 870 KB) | `ls -laS` | Thấp — không đáng lo cho load time |

## Runtime Readiness
| Check | Result | Evidence | Risk |
|---|---|---|---|
| `Boot.unity` tối giản | ✅ PASS | Đúng 3 GameObject: `Main Camera`, `Global Light 2D`, `BootSceneLoader` | Thấp |
| `Main.unity` có UI runtime đúng | ✅ PASS | UICanvas / SafeArea / HUDVisual / UIRuntimeBootstrap / EventSystem đủ | Thấp |
| UIRuntimeBootstrap tạo trùng database/save/service? | ✅ KHÔNG trùng trong 1 phiên | 1 instance duy nhất, `Start()` chạy 1 lần, các service dùng chung instance `db`/`save`/`formula`/`factory` (`CharacterService` tái dùng đúng instance của `InventoryService`, không tạo bản thứ hai) | Thấp |
| Duplicate / dead bootstrap còn tồn tại? | ⚠️ CÓ (không active) | `GuildMaster.Bootstrap.Bootstrapper` và `GuildMaster.Runtime.Boot.Bootstrapper` — **cả hai đều KHÔNG được gắn vào scene nào**, nên không chạy, không tốn runtime. Nhưng cả hai đều có code `new GameDatabase()` + `new DatabaseBuilder(...)` riêng → nếu tương lai ai đó gắn nhầm vào scene sẽ gây load data 2 lần | **Moderate** (nhầm lẫn, chưa gây hại) |
| Object/hook tạo quá nhiều lần? | ✅ KHÔNG | Không có `DontDestroyOnLoad`, không có `Instantiate` trong vòng lặp, không có `Update()` nào trong `UIRuntimeBootstrap`/`BootSceneLoader` | Thấp |
| Scene transition leak? | ✅ KHÔNG | `BootSceneLoader` bị destroy tự nhiên khi `LoadScene("Main")` (không dùng `DontDestroyOnLoad`) | Thấp |

## UI Readiness
| Check | Result | Evidence | Risk |
|---|---|---|---|
| CanvasScaler 1080×1920 | ✅ PASS | `m_ReferenceResolution: {x: 1080, y: 1920}`, `m_UiScaleMode: 1` (ScaleWithScreenSize), `m_MatchWidthOrHeight: 0.5` | Thấp |
| SafeArea còn đúng | ✅ PASS | Object `SafeArea` tồn tại trong `Main.unity`, component `SafeArea.cs` còn nguyên | Thấp |
| EventSystem dùng InputSystemUIInputModule | ✅ PASS | `Unity.InputSystem::...InputSystemUIInputModule` có mặt; `StandaloneInputModule` **không còn** | Thấp |
| Số Canvas / EventSystem / GraphicRaycaster | ✅ 1 / 1 / 1 | Không có trùng lặp gây double-raycast | Thấp |
| Layout exception | ✅ 0 | `grep "Exception"` toàn bộ Editor.log → 0 kết quả | Thấp |
| Button navigation | ✅ PASS | User đã Play test 2 lần (S6-002, S6-004), click nav + Back hoạt động | Thấp |
| Placeholder panels crash? | ✅ KHÔNG | 4 placeholder mở được, 0 exception trong log | Thấp |

## Asset / Memory Readiness
| Check | Result | Evidence | Risk |
|---|---|---|---|
| Point filter (S5) | ✅ PASS 416/416 | `filterMode: 0` trên toàn bộ 416 file `.png.meta` | Thấp |
| MipMap Off (S5) | ✅ PASS 416/416 | `enableMipMap: 0` trên toàn bộ 416 file | Thấp |
| Compression None (S5) | ✅ PASS | `DefaultTexturePlatform` có `textureCompression: 0` (None). Các block `Standalone`/`Android` tuy ghi `textureCompression: 1` nhưng đều có **`overridden: 0`** → không có hiệu lực, Unity dùng Default. Import setting S5 còn nguyên vẹn | Thấp |
| Dung lượng Art | 97 MB (Enemies 34M, Characters 30M, VFX 16M, UI 8.9M, Tilesets 5.9M, Icons 3.4M) | `du -sh` | **Deferred → S7** |
| **Memory risk cho Android** | ⚠️ **RỦI RO THẬT, chưa xử lý** | 416 sprite sheet với **Compression None** → VRAM/RAM khi load lên thiết bị lớn hơn nhiều so với 97 MB trên đĩa. Trên Android tầm trung có thể gây OOM nếu load hết cùng lúc | **Deferred → S7** (KHÔNG tự nén/resize theo đúng hard rule "không tối ưu texture bừa"; đây là quyết định scope của user, cần đo thật trên thiết bị trước) |
| Đã thay đổi import setting nào chưa? | ✅ KHÔNG | Không sửa file `.meta` nào trong task này | — |

## Save Readiness
| Check | Result | Evidence | Risk |
|---|---|---|---|
| `save.json` tạo được | ✅ PASS | 942 bytes tại `AppData/LocalLow/DefaultCompany/Rebuild_GuildMaster/save.json`, JSON hợp lệ, đúng schema (verify ở S6-004) | Thấp |
| Save trigger có spam không? | ✅ KHÔNG | Chỉ 2 trigger: `OnApplicationPause(true)` và `OnApplicationQuit()`. Không có timer, không có autosave chu kỳ, không gọi trong `Update()` | Thấp |
| Autosave mỗi frame? | ✅ KHÔNG | `UIRuntimeBootstrap` không có method `Update()` nào | Thấp |
| Pause/Quit save an toàn | ✅ PASS | `PersistSave()` có null-guard (`if (_save == null) return;`), bọc kết quả `Save(out error)`, log cả nhánh lỗi. Editor.log: `Save written (OnApplicationQuit)` đúng **1 lần**, không lặp | Thấp |
| Backup save | ✅ Có sẵn | `SaveService.Save()` tự copy `save.json` → `save_backup.json` trước khi ghi đè (code có từ S2/S3) | Thấp |

## 🔴 Phát hiện quan trọng nhất: Android StreamingAssets KHÔNG hoạt động
| Mục | Nội dung |
|---|---|
| **Vấn đề** | `StreamingAssetsGameDataProvider.cs` có `#if UNITY_ANDROID && !UNITY_EDITOR` → `ReadText()` **throw `NotSupportedException("Task S1-002: AndroidWebRequest flow for StreamingAssets not yet implemented.")`**, `Exists()` luôn `return false`, `EnumerateFiles()` cũng throw |
| **Hệ quả** | Trên **APK Android thật**, toàn bộ 1531 record sẽ **KHÔNG load được**. Game sẽ chạy nhưng database rỗng hoàn toàn |
| **Nguyên nhân kỹ thuật** | Trên Android, `StreamingAssets` nằm nén bên trong APK (`jar:file://...`), `File.ReadAllText` không đọc được — bắt buộc phải dùng `UnityWebRequest` (bất đồng bộ) |
| **Vì sao KHÔNG fix trong prompt này** | (1) User cấm rõ "Không tự ý làm S7 Android Build/APK trong prompt này"; (2) fix đúng cách đòi hỏi chuyển toàn bộ luồng load data sang **async** (`IGameDataProvider` hiện là API đồng bộ) → là thay đổi kiến trúc, **không phải "fix cực nhỏ rủi ro thấp"**; (3) Build target hiện tại là **WindowsStandalone**, và Unity Hub hiện **chỉ cài WindowsStandaloneSupport module** (Editor.log: không thấy Android module) → chưa build Android được ngay cả khi muốn |
| **Ảnh hưởng tới Standalone (Windows)** | **KHÔNG ảnh hưởng** — nhánh `#else` dùng `File.ReadAllText` bình thường, data đã có sẵn trong `StreamingAssets/GameData` |

## Optimization Actions
| Action | File | Reason | Risk |
|---|---|---|---|
| **KHÔNG có action nào được thực hiện trong S6-005** | — | Đúng nguyên tắc "không tối ưu bừa": chưa có gameplay loop thật để đo profiling, và không tìm thấy bottleneck nào có bằng chứng đo đạc. Mọi vấn đề phát hiện đều là **correctness/build-readiness**, không phải performance | — |

**Phân loại issue phát hiện:**
| ID | Severity | Mô tả | Xử lý |
|---|---|---|---|
| **P-01** | **Critical (cho S7, không phải S6)** | Android StreamingAssets throw `NotSupportedException` → data không load được trên APK | **Deferred → S7** (cần async refactor + quyết định scope của user) |
| **P-02** | **Moderate** | `DatabaseBuilder.Build()` bỏ qua report trong `UIRuntimeBootstrap` → data load fail âm thầm trong build thật | **Fix ở S6-007** (fix nhỏ: lưu report + log, không đổi gameplay) |
| **P-03** | **Moderate** | 2 class `Bootstrapper` dead code cùng tên, không gắn scene, mỗi cái tự tạo `GameDatabase` riêng | **Deferred** (không xóa — hard rule cấm rewrite architecture; chỉ note để tránh gắn nhầm) |
| **P-04** | **Low** | `localization.json` + `assets_manifest.json` trong StreamingAssets là `[]` rỗng và không nằm trong `manifest.json` → không bao giờ được load | **Deferred** (vô hại, 4 bytes; nguồn thật cũng rỗng, không tự bịa nội dung) |
| **P-05** | **Deferred → S7** | Memory risk Android: 416 sprite Compression None, Art 97 MB | **Deferred → S7** (cần đo thật trên thiết bị; cấm nén/resize bừa) |
| **P-06** | **Low (thông tin)** | Unity Hub chưa cài Android Build Support module | **Note cho S7** |

## Decision
# `S6_005_BUILD_READINESS_DONE_NO_RISKY_OPTIMIZATION_NEEDED`

**Lý do:** Toàn bộ hạng mục build readiness cho **Windows Standalone** đều PASS (data load 1531 record OK, runtime không load trùng, UI sạch 1 Canvas/1 EventSystem, import settings S5 nguyên vẹn, save trigger an toàn không spam). Không tìm thấy bottleneck performance nào có bằng chứng đo đạc → **không thực hiện bất kỳ tối ưu nào**, đúng nguyên tắc "không tối ưu bừa". Các vấn đề phát hiện đều thuộc nhóm correctness/build-readiness: 1 issue Moderate (**P-02**) sẽ fix ở S6-007, còn lại là Deferred sang S7 (đáng chú ý nhất là **P-01 Android StreamingAssets** — critical cho S7 nhưng nằm ngoài phạm vi prompt này theo đúng hard rule).
