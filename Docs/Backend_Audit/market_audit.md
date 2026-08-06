---
author: Antigravity
date: 2026-08-06
target: D:\Tinh\Rebuild_GuildMaster
module: Market & Merchant System
status: AUDIT_COMPLETE
---

# 🏪 MARKET & MERCHANT SYSTEM AUDIT

> **Mục tiêu:** Audit toàn bộ hệ thống Market, Shop, và Merchant từ source Java gốc → C# Rebuild (Services, UI, Logic, Economy).
> **Trạng thái:** Tương tự hệ thống Storage, backend của Market/Merchant được port gần đúng nhưng bị thiếu hụt nghiêm trọng ở khâu liên kết UI và các vòng lặp tự động (Game Loop). Rất nhiều tính năng economy bị "đóng băng".

---

## 1. Market Inventory & Architecture

**Legacy (Java) phân chia 3 hệ thống rõ rệt:**
1.  **DialogMarket.java (Market):** Nơi người chơi "đăng bán" (Listings) đồ từ kho để lấy Gold. Cần tốn thời gian chờ (bán qua đêm).
2.  **DialogMerchant.java (Merchant):** Thương gia xuất hiện ngẫu nhiên/hàng ngày. Bán Regular Items (bằng Gold) và Special Reserve (bằng Gems).
3.  **DialogShop.java (Shop - IAP):** Cửa hàng nạp tiền thật mua Starter Pack, Adventurer Pack, v.v.

**C# Rebuild (Gộp chung & Thiếu hụt):**
*   **Service:** `MerchantService.cs` quản lý toàn bộ việc Mua và Bán.
*   **UI:** Tồn tại cả `MarketDialog.cs` (Phase cũ) và `MerchantScreen.cs` (Phase mới - chia 3 tab Buy/Sell/Listings). 
*   **Data/Stock:** C# có cấu trúc `MerchantRegularStockItems` và `MerchantSpecialReserve` trong `SaveData.cs`, nhưng **hiện tại không có bất kỳ logic game loop nào đẩy data vào đây** (Thiếu hàm tick daily).

---

## 2. Purchase Pipeline (Buying from Merchant)

*   **Legacy Behavior:**
    *   **Regular Stock:** Lấy random 1 item từ 4 Dungeon Level cao nhất đã unlock. **Giá = Base Price * Stack * 10 (Gold).**
    *   **Special Reserve:** Lấy random từ Dungeon cao nhất. **Giá = Gems (Base 50 + 5 mỗi Dungeon unlock).** Có tỷ lệ xuất hiện trang bị đặc biệt (Aegis 1000 Gems, ScarletStrand 650 Gems).
*   **C# Rebuild Behavior:**
    *   Hàm `RollRegularOffer` và `RollSpecialOffer` trong `MerchantService.cs` có tồn tại, có đọc từ `DungeonDefinition`, **nhưng hoàn toàn không được gọi ở bất cứ đâu**.
    *   Hệ quả: Tab "Buy" trên UI của Rebuild hiện tại **vĩnh viễn trống rỗng**.
    *   Nếu có data, hàm `BuyOffer()` xử lý trừ tiền và add item vào hòm đồ hoàn toàn chính xác.

---

## 3. Selling System (Player Market Listings)

Đây là nơi có **Logic Mismatch / Data Loss** nghiêm trọng nhất ảnh hưởng trực tiếp tới Economy.

*   **Legacy Formula (Thời gian bán):** 
    `Time = (PackBonus ? 0.6 : 1.0) * (0.9 ^ MarketLevel) * itemPrice * 4 * stackCount`.
    *Item càng đắt, bán càng lâu. Phải nâng cấp Market để bán nhanh hơn.*
*   **C# Rebuild (Lỗi chí mạng):**
    *   Mặc dù `FormulaService.cs` có implement hàm `GetSecondsToSell` y hệt bản gốc.
    *   Nhưng `MerchantService.cs` (dòng 55) lại **hardcode**: `if (activeItem.SecondsPassed >= DEFAULT_SELL_TIME_SECONDS)` với `DEFAULT_SELL_TIME_SECONDS = 20`.
    *   Tức là bán Rác hay bán Đồ Truyền Thuyết đều chỉ tốn đúng 20 giây! 
*   **Giá bán (Payout):** C# Rebuild dùng `ItemDef.SellPrice > 0 ? ItemDef.SellPrice : 100`, cắt bỏ cơ chế tính giá động của Java (TruncatePrice). 

---

## 4. Economy Integrity & Gold Sinks

*   **Lạm phát (Inflation Risk):** Việc bán item nào cũng chỉ tốn 20 giây ở C# phá vỡ hoàn toàn balance game. Người chơi có thể xả kho kiếm Gold cực nhanh thay vì phải treo máy qua đêm như bản gốc.
*   **Mất Gold Sink (Nâng cấp Market):**
    *   Bản gốc cho phép dùng Gold nâng cấp: **Market Listings** (Thêm slot bán, Max 10) và **Market Time** (Bán nhanh hơn, Max 25).
    *   Trong C#, `FormulaService` có tính giá nâng cấp, `SaveData` có lưu level nâng cấp, nhưng **UI Rebuild hoàn toàn không có nút Upgrade**. Không thể nâng cấp Market.

---

## 5. Refresh / Rotation System

*   **Legacy:** Dựa vào hàm `Utils.tick24Hours()`. Mỗi khi qua ngày mới, stock của Merchant tự động bị clear và populate lại theo list Dungeon unlock.
*   **C# Rebuild:** Thiếu vắng hoàn toàn khái niệm "Game Loop Tick / Daily Refresh". Việc refresh stock phụ thuộc hoàn toàn vào dev gọi hàm `RollOffer`, nhưng hiện tại code base chưa có chỗ nào trigger việc này.

---

## 6. Save / Load Integrity

*   **Data Models:** Rebuild lưu trữ an toàn các class `ItemActionSaveData` (cho đồ đang bán/đã bán) và `MerchantOfferSaveData` (cho đồ đang được chào mua).
*   **Trạng thái:** Dữ liệu không bị mất sau khi reload game. `SaveData.cs` cover đầy đủ.

---

## 7. UI Audit

*   **Hierarchy:** `MerchantScreen.cs` chia 3 Tab (Buy, Sell, Listings) là một bước UX tốt hơn bản gốc (gom chung các Dialog lắt nhắt).
*   **Missing Features:**
    *   Mất UI **Upgrade Market Capacity** (Mua thêm slot bán).
    *   Mất UI **Upgrade Market Speed** (Mua tốc độ bán).
    *   Mất UI **IAP Shop** (Không có nơi để mua các pack buff vĩnh viễn như Starter Pack).
    *   UI "Buy" không có state countdown refresh (vì backend chưa hỗ trợ).

---

## 8. Reference Integrity Table

| Feature | Legacy Behavior (Java) | Current C# Rebuild | Data/Formulas | Runtime Support | UI Support | Status |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **Merchant Refresh** | Tự động mỗi 24h tick | Chưa implement loop | ❌ | ❌ | ❌ | **Missing Logic** |
| **Merchant Stock** | Lấy từ top dungeons | Có hàm RollOffer (Không gọi) | OK | ❌ Chết lâm sàng | Có hiển thị | **Unwired** |
| **Buy Item** | Check Gold/Gems, add slot | `MerchantService.BuyOffer` | OK | OK | OK | **Functional** |
| **Sell Time Formula**| Dựa vào BasePrice x Stack | Hardcode 20 giây | Có `FormulaService` | ❌ Bị hardcode đè | N/A | **Critical Mismatch** |
| **Sell Item** | Treo bán -> Thu tiền | `MerchantService.SellItem` | OK | OK (20s) | OK | **Functional** (Wrong Balance) |
| **Upgrade Market** | Mua Slot, Mua Tốc độ | ❌ Không có UI | OK (Có Data) | ❌ | ❌ | **Missing Feature** |

---

## 9. Tổng kết

Hệ thống Market/Merchant của Rebuild đang mắc kẹt ở tình trạng **"Chỉ có khung, chưa có hồn"**:
1. **Wrong Architecture (Economy):** Thời gian bán đồ bị hardcode 20 giây, phớt lờ hoàn toàn `FormulaService`, gây lạm phát Gold nghiêm trọng.
2. **Missing Implementation:** Không có vòng lặp thời gian để tự động Refresh Merchant. Lớp UI không thiết kế các nút Upgrade Market khiến tính năng nâng cấp bị tê liệt (giống hệt lỗi của Storage).
3. **Mất hẳn hệ thống IAP Shop.**
