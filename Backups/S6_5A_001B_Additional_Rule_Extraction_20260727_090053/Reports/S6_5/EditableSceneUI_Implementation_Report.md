# Editable Scene UI — Implementation Report

**Ngày:** 2026-07-25 · **Backup:** `Backups/Pre_S6_5_EditableSceneUI_20260725_180337/` (71 file)

## Problem
Vấn đề user báo: *"Khi chưa bấm Play, Scene/Hierarchy không có đủ UI panel/object để tự edit — nhiều UI đang được tạo/wire bằng runtime code nên nhìn trong Scene offline rất trống."*

**Kết quả audit cho thấy nguyên nhân khác với giả thuyết ban đầu** (chi tiết: `EditableSceneUI_Audit_Report.md`):

- ✅ **Toàn bộ 66 GameObject UI ĐÃ persist sẵn trong `Main.unity`** — không có object nào được tạo runtime. `UIRuntimeBootstrap` chỉ dùng `FindObjectsByType`/`FindFirstObjectByType` để **tìm object có sẵn rồi bind service**, không hề có `new GameObject()` hay `Instantiate()`.
- ⚠️ **Nguyên nhân thật:** 7 screen (`InventoryScreen`, `CharacterScreen`, `DungeonScreen`, `CraftScreen`, `MerchantScreen`, `SettingsScreen`, `PopupScreen`) bị `UIWiringGenerator` đặt **`m_IsActive: 0`** tại edit-time (dòng 143–145, 202) rồi lưu scene. Object vẫn nằm trong Hierarchy nhưng hiện dạng chữ xám mờ và **không render trong Scene view** → cảm giác "trống".

Vì vậy phần việc thực tế hẹp hơn nhiều so với dự kiến: **không cần bake lại UI, không cần sửa `UIRuntimeBootstrap`, không cần tạo prefab mới**. Chỉ cần công cụ bật/tắt preview khi edit offline.

## Changes
| File/Scene | Change | Reason |
|---|---|---|
| `Assets/_Game/Scripts/Editor/UIScreenPreviewTool.cs` (**mới**) | Editor tool với 8 menu item dưới `GuildMaster → UI → Preview: ...` để bật/tắt activation của các `UIScreen` trong scene đang mở | Cho phép user nhìn thấy và chỉnh sửa từng panel trong Scene view khi chưa Play |
| `Main.unity` | **KHÔNG sửa** | Scene đã đầy đủ và đúng — sửa thêm chỉ tăng rủi ro phá S5/S6 đã pass |
| `UIRuntimeBootstrap.cs` | **KHÔNG sửa** | Đã hoạt động đúng nguyên tắc find-and-bind, không tạo duplicate. Không có lý do kỹ thuật để đụng vào |
| `UIWiringGenerator.cs` / `UIFoundationGenerator.cs` | **KHÔNG sửa** | Việc `SetActive(false)` sau khi tạo là **đúng** cho trạng thái mặc định lúc chạy; đổi đi sẽ làm scene mở ra với 7 panel chồng lên nhau |

**Quyết định thiết kế — vì sao KHÔNG bật sẵn 7 screen trong scene file:** Nếu bật cả 7 panel, mở `Main.unity` sẽ thấy chúng **chồng đè lên nhau** trong Scene view (cùng nằm trong `ScreenRoot`, cùng full-screen) — khó edit hơn hiện tại chứ không dễ hơn. Dùng tool `Preview: Only <screen>` để xem/sửa từng màn một là workflow sạch hơn hẳn. Nếu user vẫn muốn bật hết, chỉ cần chạy `Preview: Show All Screens` một lần rồi Save scene — hoàn toàn đảo ngược được bằng `Preview: Reset`.

## Scene Objects Now Editable
Toàn bộ đều **đã** editable từ trước (không phải kết quả của thay đổi này) — bảng dưới liệt kê để user biết chính xác sửa ở đâu:

| Object | Path In Hierarchy | Editable Fields |
|---|---|---|
| UICanvas | `UICanvas` | `Canvas` (render mode, sort order), `CanvasScaler` (reference resolution 1080×1920, match), `GraphicRaycaster` |
| SafeArea | `UICanvas/SafeArea` | `RectTransform`, component `SafeArea` |
| HUDVisual | `UICanvas/SafeArea/HudRoot/HUDVisual` | `RectTransform`, layout |
| MoneyText / GemsText | `.../HUDVisual/MoneyText`, `/GemsText` | `Text` (font, size, color, align), `RectTransform` |
| MoneyIcon / GemsIcon | `.../HUDVisual/MoneyIcon`, `/GemsIcon` | `Image.sprite`, màu, `RectTransform` |
| 6 nav button | `.../HUDVisual/Btn_Inventory`, `Btn_Character`, `Btn_Dungeon`, `Btn_Craft`, `Btn_Merchant`, `Btn_Settings` | `Button` (colors, transition), `Image`, `RectTransform`; con `Text` + `Icon` chỉnh riêng |
| InventoryScreen | `UICanvas/SafeArea/ScreenRoot/InventoryScreen` | `RectTransform`, con `ListContent` (`Text`), `Btn_Back` |
| CharacterScreen | `.../ScreenRoot/CharacterScreen` | Như trên |
| DungeonScreen | `.../ScreenRoot/DungeonScreen` | `Title` (`Text`), `Message` (`Text`), `Btn_Back`, `Image` nền |
| CraftScreen | `.../ScreenRoot/CraftScreen` | Như DungeonScreen |
| MerchantScreen | `.../ScreenRoot/MerchantScreen` | Như DungeonScreen |
| SettingsScreen | `.../ScreenRoot/SettingsScreen` | Như DungeonScreen |
| PopupScreen | `UICanvas/SafeArea/PopupRoot/PopupScreen` | `Title`, `Message`, `Btn_OK` |
| 6× Btn_Back | `<mỗi screen>/Btn_Back` | `Button`, `Image`, `Text` con |
| EventSystem | `EventSystem` | `InputSystemUIInputModule` |
| UIRuntimeBootstrap | `UIRuntimeBootstrap` | (không có field public cần chỉnh) |

## Runtime Binding
| Component | Finds Existing Scene Object | Creates Runtime Fallback | Notes |
|---|---|---|---|
| `UIRuntimeBootstrap` | ✅ `FindObjectsByType<UIScreen>(FindObjectsInactive.Include, ...)` | ❌ Không tạo gì | Tìm cả object đang tắt → hoạt động đúng dù screen bật hay tắt trong scene |
| `HUDController` | ✅ `FindFirstObjectByType<HUDController>` rồi `Initialize(save, ui)` | ❌ | Text/Button đã gán sẵn qua `[SerializeField]` từ edit-time |
| `InventoryScreen` | ✅ `FindFirstObjectByType<InventoryScreen>` → `Initialize(inventoryService)` | ❌ | — |
| `CharacterScreen` | ✅ `FindFirstObjectByType<CharacterScreen>` → `Initialize(characterService)` | ❌ | — |
| `PopupScreen` | ✅ lọc trong danh sách screen → `RegisterDialogScreen` | ❌ | — |
| `Btn_Back` mỗi screen | ✅ `screen.transform.Find("Btn_Back")` | ❌ | `RemoveAllListeners()` trước khi gắn → không nhân đôi handler |
| `UIScreenPreviewTool` (**editor-only**) | ✅ `FindObjectsByType<UIScreen>` | ❌ | Chỉ toggle `SetActive`, **không tạo/xoá/rewire gì**. Bọc trong `#if UNITY_EDITOR`, không vào build |

**Không có runtime fallback nào tạo object** — đúng yêu cầu "không tạo UI runtime duplicate chồng lên UI scene có sẵn".

## Vì sao toggle ở edit-time an toàn tuyệt đối với runtime
`UIService.RegisterScreen()` gọi `screen.Hide()` **ngay tại thời điểm đăng ký** (`UIService.cs:16`), sau đó `UIRuntimeBootstrap` mới gọi `ShowScreen(MainHUD)`. Nghĩa là **mỗi lần Play, runtime tự chuẩn hoá lại trạng thái active của toàn bộ screen**, bất kể scene file để chúng bật hay tắt. Do đó dù user có bật hết screen rồi save scene, lúc Play vẫn chỉ hiện đúng MainHUD như cũ.

## Verification
| Check | Result | Evidence |
|---|---|---|
| Scene có đủ object trước Play | ✅ PASS | Parse `Main.unity`: **66 GameObject**, đủ cây UICanvas → SafeArea → ScreenRoot/HudRoot/PopupRoot/OverlayRoot + 7 screen + HUD + 6 nav + 6 Back + EventSystem + UIRuntimeBootstrap |
| Không cần Play vẫn thấy panel trong Hierarchy | ✅ PASS | Toàn bộ 7 screen tồn tại trong Hierarchy (dạng inactive); sau khi chạy menu Preview sẽ hiện luôn trong Scene view |
| Không duplicate UICanvas / EventSystem / UIRuntimeBootstrap | ✅ PASS | Đếm trực tiếp: 1 / 1 / 1. `GraphicRaycaster` cũng chỉ 1 |
| Editor tool không tạo/xoá object | ✅ PASS | Đọc code: chỉ `SetActive`, `Undo.RecordObject`, `EditorUtility.SetDirty`, `MarkSceneDirty` |
| Tool không vào build | ✅ PASS | Bọc `#if UNITY_EDITOR` + nằm trong thư mục `Editor/` |
| Namespace/using hợp lệ | ✅ PASS | Dùng đúng pattern của `UIWiringGenerator.cs` (cùng namespace `GuildMaster.Editor.UI`, cùng using `GuildMaster.Runtime.UI` + `GuildMaster.Runtime.UI.Core`) — pattern này đã compile thành công từ S5 |
| Không phá Boot→Main / save / data loading | ✅ PASS | Không sửa `Main.unity`, `Boot.unity`, `UIRuntimeBootstrap.cs`, `SaveService`, provider nào |
| **Compile thật bởi Unity** | ⚠️ **Chưa verify** | Không có MCP Unity trong phiên này. Rủi ro thấp (script độc lập, editor-only, pattern đã dùng thành công) nhưng chưa có bằng chứng compile từ Unity |
| **Play test sau thay đổi** | ⚠️ **Chưa chạy** | Cần user Play 1 lần để xác nhận không có gì thay đổi ngoài ý muốn |

## User Workflow
**Để chỉnh UI khi chưa Play:**
1. Mở `Assets/_Game/Scenes/Main.unity`.
2. Chọn màn muốn sửa qua menu, ví dụ `GuildMaster → UI → Preview: Only Dungeon` — panel đó hiện lên trong Scene view, các panel khác tự ẩn để không che.
   - Hoặc `Preview: Show All Screens` nếu muốn bật hết cùng lúc (sẽ chồng lên nhau).
   - Hoặc bật/tắt thủ công bằng checkbox góc trên Inspector của từng screen — tương đương.
3. Chỉnh `RectTransform` / `Text` / `Image` / `Button` bình thường trong Hierarchy + Inspector. **Không cần sửa runtime code để move/resize panel.**
4. Xong thì `GuildMaster → UI → Preview: Reset (Hide All Screens)` để trả về trạng thái mặc định (khuyến nghị, nhưng **không bắt buộc** — runtime tự chuẩn hoá).
5. Ctrl+S lưu scene.
6. Bấm Play từ `Boot.unity` hoặc `Main.unity` để test — hành vi lúc chạy không đổi.

**Lưu ý:** Mọi thay đổi layout/màu/sprite/text làm ở bước 3 đều được lưu thẳng vào `Main.unity`, không bị runtime ghi đè. Runtime chỉ ghi đè **nội dung text động** (money/gems, danh sách item/character) và **trạng thái bật/tắt** của screen.

## Decision
# `EDITABLE_SCENE_UI_DONE_NEEDS_USER_PLAYTEST`

**Lý do:** Audit xác định scene **đã** đầy đủ object editable (66/66, không có gì tạo runtime) nên mục tiêu "editable before Play" thực chất đã đạt sẵn về mặt cấu trúc; phần còn thiếu duy nhất là khả năng **nhìn thấy** các screen bị tắt khi edit — đã bổ sung bằng `UIScreenPreviewTool` với 8 menu item, an toàn tuyệt đối với runtime (chỉ toggle activation, editor-only, không tạo/xoá/rewire). Không sửa scene, không sửa runtime, nên **không có rủi ro phá S5/S6 đã pass**. Cần 1 lần Unity reimport + Play test để xác nhận compile sạch và hành vi không đổi.
