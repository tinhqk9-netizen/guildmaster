# Editable Scene UI — Audit Report

**Ngày:** 2026-07-25 · **Backup:** `Backups/Pre_S6_5_EditableSceneUI_20260725_180337/`
**Phương pháp:** Parse trực tiếp `Main.unity` (YAML), dựng lại toàn bộ cây hierarchy + trạng thái `m_IsActive` của từng GameObject; đối chiếu với `UIWiringGenerator.cs`, `UIRuntimeBootstrap.cs`, `UIService.cs`, `UIScreen.cs`.

## 🔍 Kết luận quan trọng nhất — chẩn đoán ban đầu cần điều chỉnh

Giả thuyết ban đầu là *"UI đang được tạo bằng runtime code nên Scene nhìn trống"*. **Dữ liệu thực tế cho thấy điều ngược lại:**

> **Toàn bộ 66 GameObject UI ĐÃ tồn tại đầy đủ và persist sẵn trong `Main.unity` trước khi Play.** `UIRuntimeBootstrap` **không tạo mới bất kỳ UI object nào** — nó chỉ `FindObjectsByType` / `FindFirstObjectByType` để tìm object có sẵn rồi bind service vào.

**Nguyên nhân thật khiến Scene view nhìn trống:** 7 screen bị đặt **`m_IsActive: 0`** (tắt) ngay tại edit-time bởi `UIWiringGenerator.cs` (dòng 143–145 và 202 gọi `SetActive(false)` rồi lưu scene). Object vẫn nằm trong Hierarchy (hiện dạng chữ xám mờ) nhưng **không render trong Scene view**, nên cảm giác như "không có gì".

Điều này đổi hoàn toàn hướng xử lý: **không cần bake/tạo lại UI, không cần sửa `UIRuntimeBootstrap`**. Chỉ cần một cách bật/tắt preview các screen khi edit offline.

## Bảng audit chi tiết

| UI Object | Exists In Scene Before Play | Created At Runtime | Editable Before Play | Bound By | Risk |
|---|---|---|---|---|---|
| UICanvas | ✅ Có (active) | ❌ Không | ✅ Có | — (Canvas/CanvasScaler/GraphicRaycaster có sẵn) | Không |
| SafeArea | ✅ Có (active) | ❌ Không | ✅ Có | Component `SafeArea` tự chạy | Không |
| ScreenRoot | ✅ Có (active) | ❌ Không | ✅ Có | — | Không |
| HudRoot | ✅ Có (active) | ❌ Không | ✅ Có | — | Không |
| PopupRoot | ✅ Có (active) | ❌ Không | ✅ Có | — | Không |
| OverlayRoot | ✅ Có (active, rỗng) | ❌ Không | ✅ Có | — | Không |
| HUDVisual | ✅ Có (active) | ❌ Không | ✅ Có | `HUDController.Initialize(save, ui)` | Không |
| MoneyText / GemsText | ✅ Có (active) | ❌ Không | ✅ Có | `HUDController` gán `.text` khi Play | Không |
| MoneyIcon / GemsIcon | ✅ Có (active) | ❌ Không | ✅ Có | Sprite đã gán sẵn edit-time (S5) | Không |
| 6 nav button (Btn_Inventory / Character / Dungeon / Craft / Merchant / Settings) + Text + Icon con | ✅ Có (active) | ❌ Không | ✅ Có | `HUDController.BindButtons()` gắn `onClick` khi Play | Không |
| InventoryScreen (+ ListContent, Btn_Back) | ✅ Có nhưng **`m_IsActive: 0`** | ❌ Không | ⚠️ **Có trong Hierarchy nhưng không thấy trong Scene view** | `InventoryScreen.Initialize(inventoryService)` | **Đây là vấn đề user gặp** |
| CharacterScreen (+ ListContent, Btn_Back) | ✅ Có nhưng **`m_IsActive: 0`** | ❌ Không | ⚠️ Như trên | `CharacterScreen.Initialize(characterService)` | Như trên |
| DungeonScreen (+ Title, Message, Btn_Back) | ✅ Có nhưng **`m_IsActive: 0`** | ❌ Không | ⚠️ Như trên | Chỉ `RegisterScreen` (placeholder) | Như trên |
| CraftScreen (+ Title, Message, Btn_Back) | ✅ Có nhưng **`m_IsActive: 0`** | ❌ Không | ⚠️ Như trên | Chỉ `RegisterScreen` | Như trên |
| MerchantScreen (+ Title, Message, Btn_Back) | ✅ Có nhưng **`m_IsActive: 0`** | ❌ Không | ⚠️ Như trên | Chỉ `RegisterScreen` | Như trên |
| SettingsScreen (+ Title, Message, Btn_Back) | ✅ Có nhưng **`m_IsActive: 0`** | ❌ Không | ⚠️ Như trên | Chỉ `RegisterScreen` | Như trên |
| PopupScreen (+ Title, Message, Btn_OK) | ✅ Có nhưng **`m_IsActive: 0`** | ❌ Không | ⚠️ Như trên | `RegisterDialogScreen(popup)` | Như trên |
| 6× Btn_Back | ✅ Có (active, nằm trong screen bị tắt) | ❌ Không | ⚠️ Theo screen cha | `WireBackButton()` gắn `onClick` khi Play | Không |
| EventSystem | ✅ Có (active, 1 instance duy nhất) | ❌ Không | ✅ Có | `InputSystemUIInputModule` | Không |
| UIRuntimeBootstrap | ✅ Có (active, 1 instance duy nhất) | ❌ Không | ✅ Có | Tự chạy `Start()` | Không |

**Thống kê:** 66/66 GameObject persist sẵn trong scene · **0 object được tạo runtime** · 7 object bị tắt edit-time · 0 duplicate (1 UICanvas, 1 EventSystem, 1 UIRuntimeBootstrap, 1 GraphicRaycaster).

## Trả lời 5 câu hỏi audit
1. **UI nào tồn tại sẵn trong Main.unity trước Play?** → **Tất cả.** Toàn bộ cây UICanvas → SafeArea → ScreenRoot/HudRoot/PopupRoot/OverlayRoot cùng 7 screen, HUD, 6 nav button, 6 Back button.
2. **UI nào chỉ được tạo khi Play?** → **Không có object nào.** `UIRuntimeBootstrap` chỉ dùng `FindObjectsByType`/`FindFirstObjectByType`, không có `new GameObject()` hay `Instantiate()` nào.
3. **UI nào được Editor generator tạo nhưng chưa persist đúng?** → Không có. Generator đã persist đúng và scene đã được lưu (`SaveOpenScenes`). Vấn đề chỉ là generator **chủ động tắt** các screen sau khi tạo.
4. **UI nào bị UIRuntimeBootstrap tạo runtime thay vì tìm object có sẵn?** → **Không có.** Toàn bộ đều là find-and-bind.
5. **Object nào có thể chuyển thành scene object/prefab editable?** → Không cần chuyển gì — **đã là scene object hết rồi**. Việc duy nhất cần làm là có cách bật chúng lên để nhìn/sửa khi offline.

## Vì sao bật object ở edit-time là an toàn tuyệt đối
`UIService.RegisterScreen()` gọi `screen.Hide()` **ngay tại thời điểm đăng ký** (dòng 16), rồi `UIRuntimeBootstrap` mới `ShowScreen(MainHUD)`. Nghĩa là **runtime luôn tự chuẩn hoá lại trạng thái active của mọi screen khi Play**, bất kể chúng đang bật hay tắt trong scene file. Do đó việc để screen ở trạng thái bật khi edit **không thể gây chồng lớp hay sai trạng thái lúc chạy**.

## Hướng xử lý đề xuất (Task 2)
Vì scene đã đầy đủ, giải pháp rủi ro thấp nhất là **không sửa scene structure và không sửa `UIRuntimeBootstrap`** — thay vào đó thêm một Editor tool để bật/tắt preview screen khi edit:
- `GuildMaster → UI → Preview: Show All Screens` — bật hết để nhìn/sửa layout
- `GuildMaster → UI → Preview: Show Only <screen>` — xem từng màn riêng
- `GuildMaster → UI → Preview: Reset (Hide All Screens)` — trả về trạng thái mặc định

Cách này giữ nguyên 100% logic đã pass (S5 + S6), không đụng runtime, không rủi ro duplicate.

## Final Decision
# `EDITABLE_SCENE_UI_AUDIT_DONE_READY_TO_FIX`

**Lý do:** Scene **đã** chứa đầy đủ UI object editable (66/66, không có gì tạo runtime) — nên phần lớn mục tiêu "editable before Play" thực chất đã đạt sẵn. Vấn đề thực tế hẹp hơn nhiều so với chẩn đoán ban đầu: 7 screen bị `SetActive(false)` tại edit-time khiến không nhìn thấy trong Scene view. Cần bổ sung công cụ preview để user edit offline thuận tiện — đó là toàn bộ phần việc còn lại của Task 2.
