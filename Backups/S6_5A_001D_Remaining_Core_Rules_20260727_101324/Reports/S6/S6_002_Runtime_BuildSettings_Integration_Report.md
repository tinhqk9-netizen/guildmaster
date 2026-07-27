# S6-002 Runtime / Build Settings Integration Report

**Ngày:** 2026-07-25 · **Backup:** `Backups/S6_PreImplementation_20260725_150355/`

## Scene Audit
| Scene | Path | In Build Settings Before | In Build Settings After | Notes |
|---|---|---|---|---|
| SampleScene | `Assets/Scenes/SampleScene.unity` | ✅ (enabled) | ❌ removed | Scene template mặc định của Unity, không thuộc game, không xoá file, chỉ gỡ khỏi Build Settings |
| Boot 1 | `Assets/_Game/Scenes/Boot 1.unity` | ✅ (enabled) | ❌ removed | Scene rỗng trùng lặp (chỉ có 1 Main Camera), không xoá file — audit trước đã ghi nhận |
| Main 1 | `Assets/_Game/Scenes/Main 1.unity` | ✅ (enabled) | ❌ removed | Scene rỗng trùng lặp, không xoá file |
| **Boot** | `Assets/_Game/Scenes/Boot.unity` | ❌ | ✅ (enabled, index 0) | Scene thật. Trước đây chỉ có Main Camera + Global Light 2D, không có script chuyển scene nào |
| **Main** | `Assets/_Game/Scenes/Main.unity` | ❌ | ✅ (enabled, index 1) | Scene thật đã pass S5 (`S5_FINAL_INTEGRATION_DONE_READY_FOR_USER_PLAYTEST`) — **không đụng vào nội dung UI/wiring đã có**, chỉ thêm vào Build Settings |

## Build Settings Fix
**Old scene list (`ProjectSettings/EditorBuildSettings.asset`):**
```
0: Assets/Scenes/SampleScene.unity (enabled)
1: Assets/_Game/Scenes/Boot 1.unity (enabled)
2: Assets/_Game/Scenes/Main 1.unity (enabled)
```

**New scene list:**
```
0: Assets/_Game/Scenes/Boot.unity (enabled)
1: Assets/_Game/Scenes/Main.unity (enabled)
```

Sửa trực tiếp bằng text edit trên `.asset` (YAML), giữ đúng format Unity, dùng đúng GUID lấy từ `Boot.unity.meta` / `Main.unity.meta` hiện có (không tạo GUID mới, không đổi identity file).

## Runtime Boot Fix (chỉnh nhỏ, trong phạm vi S6-002)
**Vấn đề phát hiện khi audit:** `Boot.unity` trước đây không có bất kỳ script chuyển cảnh nào — nếu chỉ sửa Build Settings mà không thêm gì, một build thật sẽ mở màn hình `Boot` (chỉ có camera + light) và **đứng yên mãi mãi**, không bao giờ vào `Main`.

**Fix tối thiểu đã áp dụng:**
- Tạo mới `Assets/_Game/Scripts/Runtime/Boot/BootSceneLoader.cs` — 1 `MonoBehaviour` duy nhất, `Start()` gọi `SceneManager.LoadScene("Main")`. Không tạo composition, không khởi tạo Database/Save/Service nào — cố tình tối giản để **không đụng vào** 2 class `Bootstrapper` đã bị audit đánh dấu trùng lặp (việc gộp chúng lại là phần việc Runtime Integration đầy đủ hơn, ngoài phạm vi task này).
- Gắn `BootSceneLoader` vào 1 GameObject mới trong `Boot.unity`.
- Khi vào `Main.unity`, toàn bộ luồng UI/service wiring vẫn do `UIRuntimeBootstrap` (đã pass S5) đảm nhiệm y nguyên — không sửa file này.

**Không làm trong task này (đúng theo yêu cầu):**
- Không wire 12 gameplay service còn lại.
- Không gộp/xoá `Bootstrap/Bootstrapper.cs` hay `Runtime/Boot/Bootstrapper.cs` (vẫn là dead code, để lại cho S6 Runtime Integration đầy đủ quyết định).
- Không sửa `UIRuntimeBootstrap.cs`, không sửa nội dung `Main.unity`.

## Runtime Verification
| Check | Result | Evidence |
|---|---|---|
| `EditorBuildSettings.asset` trỏ đúng scene thật | ✅ | Đọc trực tiếp file, path/guid khớp `Boot.unity.meta`/`Main.unity.meta` |
| `Main.unity` không bị đụng | ✅ | Không có edit nào trên file này trong task này (chỉ Boot.unity + script mới + build settings) |
| `Main.unity` vẫn có UICanvas/SafeArea/HUD/UIRuntimeBootstrap/EventSystem `InputSystemUIInputModule` | ✅ | Grep lại trực tiếp: `InputSystemUIInputModule` có mặt, `StandaloneInputModule` không có |
| `Boot.unity` có `BootSceneLoader` trỏ đúng script guid | ✅ | Grep scene: object `BootSceneLoader` tồn tại, `m_Script guid: 8892ce4159064e1899d59c9dd6ebda4a` khớp `BootSceneLoader.cs.meta` |
| **Compile 0 lỗi CS (script mới)** | ⚠️ **CHƯA verify được** | Script mới + `.meta` được tạo bằng text edit ngoài Unity Editor, chưa qua reimport/compile thật của Unity. Editor.log hiện tại (trước task này) không có lỗi, nhưng chưa phản ánh file mới |
| Play Mode / build thật chạy Boot → Main | ⚠️ **CHƯA verify được** | Cần Unity Editor lấy lại focus để reimport, hoặc user bấm Play trên `Boot.unity` |
| `[UIRuntimeBootstrap] Wired 8 screen(s); MainHUD shown` sau khi qua Boot→Main | Chưa test | Cần Play thật để log xuất hiện |

## Scope Check
- Không fake gameplay: đúng — `BootSceneLoader` chỉ chuyển scene, không tạo/động vào bất kỳ data hay service nào.
- Không Higgsfield: đúng.
- Không generate asset: đúng.
- Không phá S5 UI: đúng — `Main.unity` và `UIRuntimeBootstrap.cs` không bị sửa.
- Không wire full service graph ngoài scope: đúng — chỉ thêm 1 script chuyển scene, không đụng tới 12 service còn thiếu (để dành S6-003 trở đi theo audit).

## Post-User-Test Verification (2026-07-25, sau khi user Play thật)
**User evidence (thao tác tay):** Mở `Boot.unity` → Bấm Play → Boot tự chuyển sang Main → HUD hiện bình thường → Stop Play.

**Verify thêm từ Editor.log/scene/build settings:**
| Check | Result | Evidence |
|---|---|---|
| `error CS` trong Editor.log | **0** | `grep "error CS"` toàn bộ log → không có kết quả |
| `Exception` trong Editor.log | **0** | `grep "Exception"` toàn bộ log → không có kết quả |
| `EditorBuildSettings.asset` | Đúng | `Boot.unity` (guid `73824d74a7c004941b1c59359a9c7de4`) index 0, `Main.unity` (guid `7ba2a0dc39f8d1c40811a48470b8e04a`) index 1 — đọc lại trực tiếp file |
| `Boot.unity` có `BootSceneLoader` đúng script guid | Đúng | `m_Script: {fileID: 11500000, guid: 8892ce4159064e1899d59c9dd6ebda4a...}` khớp `BootSceneLoader.cs.meta` |
| `Main.unity` còn nguyên UICanvas/SafeArea/HUDVisual/UIRuntimeBootstrap/EventSystem | Đúng | Grep lại tên object, đủ cả 5 |
| `Main.unity` EventSystem dùng `InputSystemUIInputModule` | Đúng | Không có `StandaloneInputModule` |
| Log tuần tự Boot→Main sau khi compile `BootSceneLoader.cs` | Có | Editor.log: `Opening scene 'Boot.unity'` → `Loaded scene 'Boot.unity'` → `Opening scene 'Main.unity'` → `Loaded scene 'Main.unity'` → `Opening scene 'Boot.unity'` (khớp thứ tự user mô tả: mở Boot, Play chuyển Main, Stop quay lại Boot) |
| `[UIRuntimeBootstrap] Wired 8 screen(s); MainHUD shown` | Có, lặp lại nhiều lần trong log | Xác nhận HUD dựng thành công mỗi khi Main.unity là scene đang chạy — khớp "HUD hiện bình thường" user báo |

**Ghi chú minh bạch:** Log scene-load của Unity Editor không phân biệt rõ 100% giữa "Editor mở scene để edit/validate" và "runtime `SceneManager.LoadScene` trong Play Mode" trong một số dòng — nên phần verify tự động ở đây được coi là **hỗ trợ/củng cố**, không phải bằng chứng độc lập thay thế cho phép thử tay của user. Kết luận DONE dựa trên cả hai: (1) user đã tự tay xác nhận hành vi đúng, (2) log không có bất kỳ dấu hiệu lỗi/exception nào mâu thuẫn với lời khai đó.

## Decision (Final)
# `S6_002_RUNTIME_BUILDSETTINGS_DONE`
