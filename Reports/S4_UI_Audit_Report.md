# S4 UI Audit Report

## S4 Plan Summary

| Task | Description | Source |
|---|---|---|
| S4-001 | UI Mapping (Phân tích, lập screen map & flow) | `Sprint4.md` / `MasterPlan` |
| S4-002 | Canvas Generator (Tạo Canvas, Safe Area, base components, prefabs) | `Sprint4.md` / `MasterPlan` |
| S4-003 | Navigation System (Quản lý Screen Stack, Popup, chuyển màn hình) | `Sprint4.md` / `MasterPlan` |
| S4-004 | HUD (Hiển thị Currency, Level, Timer thông báo) | `Sprint4.md` / `MasterPlan` |
| S4-005 | Inventory UI (Danh sách item, sort, filter, chi tiết trang bị) | `Sprint4.md` / `MasterPlan` |
| S4-006 | Character UI (Danh sách nhân vật, chỉ số, ô trang bị, kỹ năng) | `Sprint4.md` / `MasterPlan` |
| S4-007 | Popup & Dialog System (Confirm, Error, Reward, Result popup) | `Sprint4.md` / `MasterPlan` |
| S4-008 | Sprint Review (Đánh giá tổng thể trước khi sang S5) | `Sprint4.md` / `MasterPlan` |

## Current UI Structure

| Area | Existing | Status |
|---|---|---|
| Scenes | `Assets\_Game\Scenes` | Hiện tại không có Scene UI chuyên biệt nào, chỉ có Scene gốc/Core. |
| Prefabs | `Assets\_Game\Prefabs\UI` | **Trống (Empty)** |
| Art / UI | `Assets\_Game\Art\UI` | **Trống (Empty)** |
| Scripts / UI | `Assets\_Game\Scripts\UI` | **Trống (Empty)** |
| Canvas/EventSystem | Chưa khởi tạo | Không tồn tại trong project hiện tại. |

**Nhận xét:** Unity project backend hoàn toàn sạch, chưa có rác UI. Phù hợp để xây dựng Foundation từ con số 0 trong S4-002.

## Asset / UI Source Audit

| Source | Status | Notes |
|---|---|---|
| `Assets-tham-khao\FantasyDungeon...` | Khả dụng (YES) | Có các UI kit (`ui_dialog.png`, `ui_kit.png`), có thể cắt ra dùng làm Base Panel, Button, Overlay. |
| `Assets-tham-khao\link_github` | Tham khảo (YES) | Mã nguồn `InventorySystem.cs`, `QuestSystem.cs` mang tính chất tham khảo flow, KHÔNG ĐƯỢC kéo trực tiếp vào project do S4 không sửa code backend. Chỉ nên tham khảo cách chia panel. |
| Placeholder / Thiếu Asset | Cần lưu ý | Nếu thiếu icon cụ thể cho item, quest, character... bắt buộc dùng solid color / placeholder icon trống, không được dừng luồng. |

## UI Screen → Backend Mapping

| Screen | Backend | Can implement? | Notes |
|---|---|---|---|
| Boot / Loading | YES | YES | Có `SaveData`, `GameDatabase` sẵn sàng. |
| Main HUD | YES | YES | Thể hiện được `Money`, `Gems`. Timer sẽ bị giới hạn ở data đã có. |
| Inventory | YES | YES | API `GetQuantityByDefinitionId`, `HasQuantity`, `ItemRuntime` đều đã hoàn thiện. |
| Character / Equipment | PARTIAL | YES | `CharacterSaveData` có sẵn thông tin cấp độ, ID, và `Weapon/Armor/AccessoryInstanceId`. Chỉ số hiển thị read-only tốt. |
| Dungeon Selection | YES | YES | Đã có `DungeonDefinition` (11 dungeons) và Core logic. |
| Combat Status | PARTIAL | PARTIAL | Chỉ bind được HP/Turn cơ bản do *full combat cast/formula* bị deferred. Khuyến nghị chỉ làm list cơ bản. |
| Craft / Workshop | PARTIAL | YES (Queue only)| Bind được danh sách recipe (321 recipe) và workshop queue. Claim/Progress bar không chính xác do thiếu formula. |
| Merchant / Market | PARTIAL | YES (View only) | Bind được danh sách RollOffer và List bán. Nút Buy phải popup báo `Deferred`. |
| Quest | PARTIAL | YES (View only) | Bind được list Quest. Claim reward chưa hoạt động. |

## Proposed S4 Implementation Batches

| Batch | Tasks | Scope |
|---|---|---|
| **S4 Batch 1** | S4-001 → S4-004 | Setup toàn bộ UI Foundation (Canvas, Navigation Manager, Base Panels, HUD currency bindings). Không đụng tới logic phức tạp, chỉ chuẩn bị bộ khung. |
| **S4 Batch 2** | S4-005 → S4-008 | Implement các màn hình chức năng (Inventory, Character, Popup). Bind read-only data từ backend. Đặt action listener nhưng xử lý an toàn (ví dụ gọi Buy/Craft sẽ show Dialog trả về Result enum deferred). Tiến hành Sprint Review. |

## Deferred / Blocked
- UI Combat Details: Không nên bind chi tiết log sát thương vì `damage formula` chưa có.
- UI Craft Timer: Bị chặn (blocked) vì thiếu `craft duration formula`.
- UI Merchant Buy Button: Phải bind vào `MerchantResult.Fail(Deferred)`.
- UI Quest Reward Claim Button: Bị chặn.
- UI Offline Reward Generator: Blocked.

## Risks

| Risk | Impact | Required before implementation |
|---|---|---|
| Thiếu UI Assets (Icons, Backgrounds) | Trung bình | Yêu cầu nghiêm ngặt: dùng Placeholder (Image màu xám) chứ không được phá vỡ rule cấm hardcode asset. |
| Mobile Aspect Ratio | Cao (UI vỡ) | Bắt buộc phải cấu hình `CanvasScaler` đúng tỉ lệ dọc (ví dụ 1080x1920) ngay từ S4-002. |
| Backend API không đủ cho UI Action | Cao | Không được phép "chế" API hoặc sửa `ICraftService`/`IMerchantService`. Nếu UI cần API chưa có, nút đó hiển thị "Sắp ra mắt" hoặc mờ đi. |

## Recommendation

`READY_FOR_S4_BATCH1_UI_IMPLEMENTATION`
