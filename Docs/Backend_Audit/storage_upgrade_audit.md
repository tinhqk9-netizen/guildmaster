---
author: Antigravity
date: 2026-08-06
target: D:\Tinh\Rebuild_GuildMaster
module: Storage Upgrade System
status: AUDIT_COMPLETE
---

# 📦 STORAGE UPGRADE SYSTEM AUDIT

> **Mục tiêu:** Audit toàn bộ hệ thống Storage / Inventory Capacity Upgrade từ source Java gốc → item storage model → upgrade cost → progression → save/load → UI.
> **Trạng thái:** Backend Logic (Formulas & Data) tương đối chuẩn xác, nhưng **UI Rebuild hoàn toàn thiếu chức năng Upgrade**, khiến tính năng nâng cấp hòm đồ hiện tại bằng "không".

---

## 1. Storage System Inventory

*   **Legacy (Java):**
    *   **Data/Model:** `Data.java` chứa toàn bộ Items (Dạng List). Biến lưu trữ level nâng cấp là `levelStorage` (mua bằng Gold) và `upgradeStorage` (từ IAP/đặc biệt).
    *   **Logic Upgrade:** Button UI trong `DialogStorage.java` xử lý tốn Gold, gọi tăng `levelStorage`.
*   **C# Rebuild:**
    *   **Data/Model:** `SaveData.cs` chứa `LevelStorage` và `UpgradeStorage` ở file `Assets\_Game\Scripts\Runtime\Save\SaveData.cs`. Mảng Items lưu dưới dạng `List<ItemSaveData>`.
    *   **Service:** `InventoryService.cs` đảm nhiệm thêm bớt và check Capacity.
    *   **UI:** `StorageDialog.cs` chỉ thực hiện render Items.

**Tổng quan đặc tính Storage:**
*   Capacity ban đầu (Base): **35 Slots**.
*   Giới hạn `levelStorage` tối đa: **Level 80**.
*   Nâng cấp tác dụng lên **Toàn bộ Account**, không phải từng nhân vật.
*   Item Stack (Quantity) không tốn thêm Slot. 1 Slot = 1 loại Item (Definition ID).

---

## 2. Legacy Storage Capacity Logic & Upgrade Cost

**1. Capacity Formula (`Formulas.storageSpaces()`):**
Capacity = 35 (Base) + `levelStorage` + `upgradeStorage` + IAP Bonuses.
*   *IAP Bonuses:* Starter Pack (+35), Adventurer Pack (+35), Merchant Pack (+70).
*   *Max Base Capacity (Không IAP):* 35 + 80 = 115 Slots.

**2. Upgrade Cost Formula (`Formulas.getStorageCapacityPrice()`):**
Dùng tiền Gold (Game currency). Công thức tính cộng dồn (Piecewise), chia làm các bậc:

| Target Level (`next`) | Cost Incremental/Level (Gold) | Total Cost Example |
| :--- | :--- | :--- |
| L1 -> 10 | 50 | L1 = 50, L10 = 500 |
| L11 -> 20 | 150 | L11 = 650, L20 = 2000 |
| L21 -> 30 | 800 | L21 = 2800, L30 = 10000 |
| L31 -> 40 | 4000 | L31 = 14000, L40 = 50000 |
| L41 -> 50 | 12000 | L41 = 62000, L50 = 170000 |
| L51 -> 60 | 22000 | L51 = 192000, L60 = 390000 |
| L61 -> 80 | 30000 (DEFAULT_BACKOFF_DELAY_MILLIS) | L61 = 420000, L80 = 990000 |
| > 80 | 99999999999999 (IMPOSSIBLY_HIGH_PRICE) | Không thể nâng |

*Lưu ý: Game gốc do build lỗi decompiler nên lấy nhầm biến `WorkRequest.DEFAULT_BACKOFF_DELAY_MILLIS` (giá trị 30000) vào giá nâng cấp.*

---

## 3. Item Storage Model

*   **Legacy:** Dùng `Utils.collectItem()`. Nếu Item đó chưa có trong hòm -> Tính 1 Slot. Nếu đã có -> Chống dồn (Stack + quantity), **bỏ qua giới hạn dung lượng**.
*   **C# Rebuild (`InventoryService.cs`):**
    *   Hàm `CanAddItem()` và `AddItem()` tái hiện chính xác rule này.
    *   `canStack = (ItemCategory.Material || ItemCategory.Consumable)`.
    *   Nếu Item stackable và đã có sẵn trong hòm `_items.Any()`, AddItem **luôn luôn trả về true** (pass capacity check).
    *   Nếu là Slot mới (Item lần đầu nhặt, hoặc Weapon/Armor không stackable), check `_items.Count < GetCapacity()`.
*   **Unique / Equipped Items:** Đồ đang mang trên người (`EquipmentInstance`) không nằm trong `InventoryService._items`, do đó **không tốn Storage Space**.

---

## 4. Storage Upgrade Data Integrity

*   **Data / C# Implementation Mapping:**
    *   Code tính giá (`Formulas.java` -> `FormulaService.cs`) được **chuyển port chính xác 100%**. Hàm `GetStorageCapacityPrice` xử lý trơn tru cả lỗi biến 30000L.
    *   Code tính Capacity (`GetCapacity()`) cũng được C# xử lý chuẩn thông qua `FormulaService.StorageSpaces`.
    *   Data hoàn toàn là **Hardcode** trong source (cả Java và C#), không dùng JSON (hoàn toàn đúng logic legacy).

---

## 5. Save / Load Integrity

*   `SaveData.cs` hiện tại lưu trữ chính xác giá trị `LevelStorage` và `UpgradeStorage` (ngay trên đầu file, dòng 126).
*   **Save Risk:** Hiện tại C# không có rủi ro tràn Data hay mất Data Capacity do các logic lưu trữ int cơ bản đang chạy bình thường. Load save cũ hoạt động tốt.

---

## 6. Runtime Behavior (Economy & Interaction)

*   **Dungeon Rewards:** `DungeonService.Retreat()` check `_inventoryService.CanAddItem(drop)`. Nếu hòm đầy (không cùng stack), Item sẽ **bị kẹt lại ở PendingDrops** trong Dungeon (Không bị xóa mất, nhưng không thể đem về nhà). -> Đúng Legacy behavior.
*   **Market / Crafting:** Khi hòm đầy, hệ thống cũng chặn việc mua đồ hoặc Craft vũ khí mới nếu nó làm vượt quá Capacity.

---

## 7. UI Audit (Critical Missing Feature)

*   **Legacy UI (`DialogStorage.java`):**
    *   Có `buttonUpgradeSpaces`. Nhấn vào sẽ hiển thị Dialog Confirm (Giá: `Formulas.getStorageCapacityPrice()`). Mua xong trừ tiền Gold và tăng Size.
*   **C# Rebuild UI (`StorageDialog.cs`):**
    *   **Hoàn toàn không có nút Upgrade Capacity.**
    *   Code UI hiển thị text `$"{allItems.Count} / {capacity}"`.
    *   Comment từ file dev C# (dòng 15): *"No storage-upgrade action exists here... so this dialog is capacity-display-only"*.
*   **Hậu quả:** Người chơi Rebuild hiện tại bị kẹt ở mức 35 Slots Base vĩnh viễn vì không có UI để chi tiền mua Upgrade.

---

## 8. Reference Integrity Table

| Feature | Legacy Behavior | Current Data | Runtime Support | Save Support | UI Support | Status |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **Max Capacity (Level)** | Lvl 80 | Code hardcode | `FormulaService` (OK) | `SaveData` (OK) | ❌ | **UI Mismatch** |
| **Upgrade Cost Formula** | Bậc thang, xài Backoff delay | Lập trình chuẩn | `FormulaService` (OK) | N/A | ❌ | **UI Mismatch** |
| **Capacity Bypass (Stack)** | Dồn stack không tính Slot mới | Chạy đúng (`CanAddItem`) | `InventoryService` (OK) | N/A | N/A | **Correct** |
| **Overflow Handling** | Kẹt lại pending drop (Area) | Chạy đúng (`DungeonService`) | `PendingDrops` array (OK) | N/A | N/A | **Correct** |
| **Upgrade UI Button** | Mua bằng Gold, max disable | ❌ Mất tích | ❌ Thiếu event/hook | N/A | ❌ | **Missing Logic** |

---

## 💡 Tổng kết
Backend và SaveData của tính năng Storage Upgrade **đã được làm hoàn thiện** và chính xác đến từng chi tiết bug nhỏ của game gốc.
**Tuy nhiên, toàn bộ tính năng này bị vô hiệu hóa vì tầng UI (StorageDialog) hoàn toàn quên/bỏ qua việc implement Nút Upgrade.**
