# Editable All Panels Visible — Implementation Report

**Ngày:** 2026-07-25 · **Backup:** `Backups/Editable_All_Panels_Visible_20260725_182832/` (13 file)

## User Requirement
User muốn **tất cả panel/screen hiện sẵn** trong Hierarchy/Scene của `Main.unity` để tự edit trực tiếp khi chưa Play. Panel chồng lên nhau **không phải vấn đề** — user tự tắt object nào chưa cần trong Hierarchy. Không muốn phụ thuộc preview tạm thời hay Play Mode.

## Changes
| File/Scene | Change | Reason |
|---|---|---|
| `Assets/_Game/Scenes/Main.unity` | Đổi `m_IsActive: 0` → `m_IsActive: 1` cho **7 screen**: `InventoryScreen`, `CharacterScreen`, `DungeonScreen`, `CraftScreen`, `MerchantScreen`, `SettingsScreen`, `PopupScreen`. **Đây là thay đổi persist thẳng vào scene file**, không phải preview tạm | Mở `Main.unity` là thấy ngay toàn bộ panel, không cần chạy menu hay bấm Play |
| `Assets/_Game/Scripts/Editor/UIScreenPreviewTool.cs` | Thêm menu **`GuildMaster → UI → Make All Panels Visible In Scene`** (priority 0, nằm trên cùng). Menu này bật tất cả screen + `MarkSceneDirty()` + **`SaveOpenScenes()`** + log rõ tên từng panel đã bật | Cho user cách bật lại nhanh sau này nếu lỡ tắt/reset. Khác với `Preview: Show All Screens` ở chỗ **có lưu scene** — trạng thái persist |
| `UIWiringGenerator.cs` | **KHÔNG sửa** | Việc generator `SetActive(false)` chỉ ảnh hưởng khi user chạy lại `Wire UI Scene`. Sửa đi sẽ đụng vào code S5 đã pass mà không cần thiết — nếu lỡ chạy lại generator, chỉ cần bấm `Make All Panels Visible In Scene` là xong |
| `UIRuntimeBootstrap.cs` | **KHÔNG sửa** | Đã hoạt động đúng (find-and-bind, không tạo duplicate). Không có lý do kỹ thuật để đụng |
| Preview tool cũ (8 menu `Preview: ...`) | **GIỮ NGUYÊN 100%** | Đúng yêu cầu "không xóa Preview Tool" — user vẫn dùng được `Only <screen>` / `Reset` khi cần |

**Cách thực hiện:** Sửa trực tiếp scene file (text edit trên YAML) thay vì bắt user chạy menu — để bạn mở Unity lên là thấy kết quả ngay. Diff so với backup **đúng 7 dòng, không dư một ký tự nào**, kích thước file giữ nguyên 163.427 bytes.

## Panels Set Active
| Panel | Hierarchy Path | Active In Edit Mode |
|---|---|---|
| InventoryScreen | `UICanvas/SafeArea/ScreenRoot/InventoryScreen` | ✅ **Active** (đổi từ inactive) |
| CharacterScreen | `UICanvas/SafeArea/ScreenRoot/CharacterScreen` | ✅ **Active** (đổi từ inactive) |
| DungeonScreen | `UICanvas/SafeArea/ScreenRoot/DungeonScreen` | ✅ **Active** (đổi từ inactive) |
| CraftScreen | `UICanvas/SafeArea/ScreenRoot/CraftScreen` | ✅ **Active** (đổi từ inactive) |
| MerchantScreen | `UICanvas/SafeArea/ScreenRoot/MerchantScreen` | ✅ **Active** (đổi từ inactive) |
| SettingsScreen | `UICanvas/SafeArea/ScreenRoot/SettingsScreen` | ✅ **Active** (đổi từ inactive) |
| PopupScreen | `UICanvas/SafeArea/PopupRoot/PopupScreen` | ✅ **Active** (đổi từ inactive) |
| HUDVisual (MainHUD) | `UICanvas/SafeArea/HudRoot/HUDVisual` | ✅ Active (vốn đã active) |
| 6 nav button | `.../HUDVisual/Btn_Inventory` · `Btn_Character` · `Btn_Dungeon` · `Btn_Craft` · `Btn_Merchant` · `Btn_Settings` | ✅ Active (vốn đã active) |
| 6× Btn_Back | `<mỗi screen>/Btn_Back` | ✅ Active (giờ hiện luôn vì screen cha đã bật) |
| UICanvas / SafeArea / ScreenRoot / HudRoot / PopupRoot / OverlayRoot | `UICanvas` → `SafeArea` → 4 root | ✅ Active (vốn đã active) |
| EventSystem | `EventSystem` (root) | ✅ Active |
| UIRuntimeBootstrap | `UIRuntimeBootstrap` (root) | ✅ Active |

**Kết quả: 66/66 GameObject đều active, 0 object inactive.**

## Runtime Safety
Việc bật panel ở edit-time **không ảnh hưởng gì tới runtime**, vì:

1. **`UIService.RegisterScreen()` gọi `screen.Hide()` ngay tại thời điểm đăng ký** (`UIService.cs:16`). Khi Play, `UIRuntimeBootstrap.Start()` duyệt toàn bộ `UIScreen` để register → **mọi screen bị ẩn hết**, rồi mới `ShowScreen(UIScreenId.MainHUD)`. Runtime **tự chuẩn hoá lại trạng thái** bất kể scene file để bật hay tắt.
2. **`FindObjectsByType<UIScreen>(FindObjectsInactive.Include, ...)`** — `UIRuntimeBootstrap` tìm cả object đang tắt lẫn đang bật, nên hoạt động giống hệt trong cả hai trường hợp.
3. **Không tạo duplicate:** không có object nào được thêm/xoá — chỉ đổi giá trị boolean `m_IsActive` của 7 object đã tồn tại sẵn.
4. **Không đụng gameplay:** không sửa data, save, service, formula, hay bất kỳ logic nào. Không generate asset, không fake gameplay.

Nói ngắn gọn: **edit-time thấy hết, runtime vẫn chạy đúng như cũ.**

## Verification
| Check | Result | Evidence |
|---|---|---|
| 7 screen active trong scene file | ✅ PASS | Parse lại `Main.unity`: `InventoryScreen`, `CharacterScreen`, `DungeonScreen`, `CraftScreen`, `MerchantScreen`, `SettingsScreen`, `PopupScreen` đều `m_IsActive: 1` |
| Tổng object inactive | ✅ **0** | Quét toàn bộ 66 GameObject → không còn object nào tắt |
| Hierarchy nguyên vẹn | ✅ PASS | Dựng lại cây đầy đủ: `UICanvas → SafeArea → ScreenRoot(6 screen) / HudRoot(HUDVisual + 6 nav) / PopupRoot(PopupScreen) / OverlayRoot`, cùng `Main Camera`, `Global Light 2D`, `UIRuntimeBootstrap`, `EventSystem` |
| Không duplicate | ✅ PASS | Đếm từng tên: UICanvas ×1, EventSystem ×1, UIRuntimeBootstrap ×1, SafeArea ×1, HUDVisual ×1, và **mỗi screen đúng ×1** |
| File YAML không hỏng | ✅ PASS | Header `%YAML 1.1` + `%TAG !u!` nguyên vẹn; **diff so với backup đúng 7 dòng** (`m_IsActive: 0` → `1`), không có thay đổi ngoài ý muốn; kích thước file giữ nguyên 163.427 bytes |
| Menu mới hợp lệ | ✅ PASS | `[MenuItem("GuildMaster/UI/Make All Panels Visible In Scene", priority = 0)]`; `System.Linq` + `EditorSceneManager` đã có sẵn trong using; cùng namespace/pattern với `UIWiringGenerator.cs` đã compile thành công từ S5 |
| Preview tool cũ còn nguyên | ✅ PASS | 8 menu `Preview: ...` vẫn tồn tại đầy đủ, không bị xoá |
| Không sửa runtime/gameplay/data | ✅ PASS | Chỉ 2 file bị đụng: `Main.unity` (7 dòng boolean) + `UIScreenPreviewTool.cs` (thêm 1 method editor-only) |
| **Compile thật bởi Unity** | ⚠️ **Chưa verify** | Không có MCP Unity trong phiên này. Rủi ro thấp (method editor-only, dùng API đã có sẵn trong file) |
| **Play test sau thay đổi** | ⚠️ **Chưa chạy** | Cần user Play 1 lần xác nhận runtime vẫn chuẩn hoá đúng |

## User Workflow
1. Mở `Assets/_Game/Scenes/Main.unity` → **thấy ngay toàn bộ panel** trong Hierarchy và Scene view, không cần bấm gì.
2. Panel chồng lên nhau: tắt checkbox góc trên Inspector của panel chưa cần → chỉ còn panel đang sửa.
3. Chỉnh `RectTransform` / `Text` / `Image` / `Button` bình thường → Ctrl+S.
4. Nếu lỡ tắt hết hoặc chạy lại `Wire UI Scene`: bấm **`GuildMaster → UI → Make All Panels Visible In Scene`** để bật lại tất cả và lưu luôn.
5. Muốn xem riêng 1 màn: `GuildMaster → UI → Preview: Only <tên màn>`.
6. Bấm Play từ `Boot.unity` hoặc `Main.unity` — hành vi runtime **không đổi**, vẫn chỉ hiện MainHUD lúc bắt đầu.

## Decision
# `EDITABLE_ALL_PANELS_VISIBLE_DONE_NEEDS_USER_PLAYTEST`

**Lý do:** Đã bật thành công cả 7 screen trong `Main.unity` (66/66 object active, 0 inactive), diff sạch tuyệt đối đúng 7 dòng, hierarchy nguyên vẹn, không duplicate, không đụng runtime/gameplay/data. Bổ sung menu `Make All Panels Visible In Scene` có lưu scene, giữ nguyên toàn bộ Preview tool cũ. Runtime an toàn về mặt logic vì `UIService.RegisterScreen()` tự Hide mọi screen khi Play. Cần 1 lần Unity reimport + Play test để xác nhận compile sạch và hành vi lúc chạy không đổi.
