---
author: Antigravity
date: 2026-08-06
target: D:\Tinh\Rebuild_GuildMaster
module: Workshop & CraftService
status: AUDIT_COMPLETE
---

# 🛠️ WORKSHOP & CRAFTSERVICE AUDIT

> **Mục tiêu:** Audit toàn bộ hệ thống Workshop / CraftService từ source Java Legacy → parser/data → C# runtime → save/load → UI.
> **Trạng thái:** Data Model và Craft Logic cơ bản chạy được, nhưng bị hổng nghiêm trọng ở khâu tính thời gian (Time formula bị hardcode) và xử lý Offline Progress (chỉ craft được 1 item). Tầng UI một lần nữa thiếu nút Upgrade/Cancel.

---

## 1. Recipe Inventory & Data Model

*   **Legacy (Java):** Các công thức nằm trong enum `Recipes.java` (Cloth, Leather, v.v.). Output luôn là 1 item, ID trùng với tên enum. Input là List of `Item` (có số lượng).
*   **C# Rebuild:** Sử dụng `RecipeDefinition.cs` (chứa `OutputItemId` và `List<IngredientData>`). Data parse tốt, mô phỏng đúng cấu trúc 1-1 với Java. 
*   **Condition:** Cả Java và C# hiện tại không có điều kiện Unlock đặc biệt cho Recipe (miễn là đủ nguyên liệu là craft được).

---

## 2. Legacy Craft Flow vs C# Implementation

**Luồng gốc (Legacy):**
1. Người chơi chọn Recipe -> Add vào `WorkshopQueue` (dạng `ItemAction`).
2. Tốn nguyên liệu ngay lập tức.
3. Chờ thời gian craft: Thời gian được tính theo `(PackBonus ? 0.6 : 1.0) * (0.9 ^ WorkshopTimeLevel) * ItemPrice * 6 * Stack`. Đồ càng xịn craft càng lâu.
4. Có thể nhấn Cancel để huỷ craft.

**Luồng C# (Rebuild):**
1. Chạy qua `CraftService.TryStartCraft()` -> Add vào `WorkshopQueue` (lưu dạng `ItemActionSaveData`).
2. Tốn nguyên liệu tức thì -> Đúng.
3. **LỖI THỜI GIAN:** Toàn bộ công thức tính thời gian craft phức tạp của bản gốc (có trong `FormulaService.GetSecondsToCraft`) đã bị quăng sọt rác. `CraftService.cs` **hardcode 10 giây cho mọi recipe** (`DEFAULT_CRAFT_DURATION_SECONDS = 10`).
4. **THIẾU TÍNH NĂNG:** `CraftService` không có hàm Cancel, người chơi kẹt đồ trong queue vĩnh viễn cho đến khi craft xong.

---

## 3. Offline Progress Mismatch (Critical Bug)

Hệ thống craft là xương sống của game Idle, nhưng C# Rebuild đang có một lỗi logic cực nặng khi xử lý thời gian trôi qua (Offline hoặc Delta time lớn).

*   **Legacy (`Utils.progressWorkshopTime`):** Chạy vòng lặp duyệt qua `WorkshopQueue`. Nếu offline 10 tiếng, game sẽ craft liên tục từng món một cho đến khi đầy queue hoặc hết 10 tiếng.
*   **C# Rebuild (`CraftService.ProgressWorkshop`):** 
    Chỉ cộng dồn toàn bộ thời gian `deltaSeconds` vào item đầu tiên `queue[0]`. Nếu item này xong (đạt 10s), game dừng lại. Không hề có vòng lặp đẩy thời gian thừa cho `queue[1]`.
    *Hậu quả: Dù người chơi offline 1 tháng, lúc quay lại game cũng chỉ craft xong đúng 1 item duy nhất ở đầu hàng đợi.*

---

## 4. Workshop Upgrade System

*   **Legacy:** Có 2 loại nâng cấp:
    1. **Workshop Queue (Slots):** Tăng số lượng hàng đợi.
    2. **Workshop Time (Speed):** Giảm thời gian craft.
*   **C# Rebuild:**
    *   **Backend:** `CraftService` có hàm `UpgradeQueueCapacity()` nhưng **không có** hàm UpgradeTime. `FormulaService` có đủ công thức tính giá tiền cho cả hai.
    *   **UI:** File `WorkshopDialog.cs` **hoàn toàn không vẽ nút Upgrade**. Người chơi bị kẹt vĩnh viễn ở Slot cơ bản.

---

## 5. Economy Impact

*   **Phá vỡ cân bằng Progression:** Do thời gian craft bị hardcode 10 giây, người chơi có thể craft trang bị End-game (giá hàng chục nghìn Gold) nhanh bằng với việc craft 1 miếng Vải rách.
*   **Chặn luồng chơi Idle:** Vì Offline Progress bị lỗi chỉ craft 1 món, người chơi buộc phải online và treo máy nếu muốn craft hàng loạt đồ (gây ức chế).
*   **Mất Gold Sink:** Không có UI nâng cấp Workshop khiến lượng Gold trong game bị ứ đọng.

---

## 6. UI Audit

*   **Legacy UI (`DialogWorkshop.java`, `DialogCraft.java`):** Cho phép xem Queue, Hủy craft, Nhận đồ, và Nâng cấp Workshop.
*   **C# UI (`WorkshopDialog.cs`, `WorkshopRecipePanel.cs`):**
    *   Được Dev nốt rõ trong comment: *"Craft duration is hard-coded 10s... No Cancel action exists... No upgrade"*.
    *   UI chỉ cho phép xếp hàng (Queue) và nhận đồ (Claim). 
    *   Hoàn toàn thiếu chức năng Hủy (Cancel) và Nâng cấp (Upgrade).

---

## 7. Reference Integrity Table

| Feature | Legacy Behavior (Java) | Current C# Rebuild | Data/Formulas | Runtime Support | UI Support | Status |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **Craft Recipe** | Mất NL -> Đợi -> Xong | `CraftService.TryStartCraft` | OK | OK | OK | **Functional** |
| **Craft Time** | `BasePrice * 6` x (Time Buffs) | Hardcode **10s** | Có `Formula` | ❌ Bỏ qua | N/A | **Critical Mismatch** |
| **Offline Progress**| Craft dồn từng món liên tục | Chạy món đầu tiên rồi nghỉ | ❌ | ❌ Lỗi Logic | N/A | **Critical Bug** |
| **Cancel Craft** | Bấm hủy trả lại NL | Không có chức năng | ❌ | ❌ | ❌ | **Missing Feature** |
| **Upgrade Queue** | Tốn Gold, tăng Slot | ❌ Không có UI | OK (Có Data) | Có API Support | ❌ | **Missing Feature** |
| **Upgrade Speed** | Tốn Gold, giảm TG | ❌ Không có UI / Code | OK | ❌ | ❌ | **Missing Feature** |

---

## 8. Tổng kết

Hệ thống CraftService hiện tại mang tính chất "Mock/Placeholder" hơn là một tính năng hoàn chỉnh:
1. **Lỗi Balance nặng:** Thời gian craft bị đóng đinh 10 giây.
2. **Lỗi Logic Idle:** Offline Progress không hoạt động với chuỗi Queue, biến game Idle thành game cày cuốc online.
3. **UI nửa vời:** Thiếu nút Cancel và các nút Upgrade, khiến trải nghiệm người chơi bị bó hẹp. Đồ đẩy vào Queue bấm nhầm là kẹt luôn.
