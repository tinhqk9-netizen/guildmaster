# 🧠 Báo cáo Audit: Item Data Pipeline & Logic Instances

**Dự án**: D:\Tinh\Rebuild_GuildMaster
**Ngày Audit**: 2026-08-06

---

## 1. Data Field Mapping (items.json → ItemDefinition)
Phát hiện lỗi nghiêm trọng trong luồng nạp dữ liệu. Hàng loạt field quan trọng bị vứt bỏ trong quá trình parse.

### 🔴 Thống kê & Mức độ ảnh hưởng
- `Price`: **607/607** items bị mất (Mặc định = 0). Hậu quả: Bán mọi đồ vào shop giá 0 vàng.
- `Rarity`: **607/607** items bị mất (Mặc định = 0).
- `NotSellable`: **24/24** items (các item cấm bán) bị mất (Mặc định = false). Hậu quả: Người chơi có thể bán nhầm vật phẩm nhiệm vụ.
- `idImage`: **11/11** items có custom sprite bị mất. Hậu quả: Trắng hình ảnh trong UI.

### 🔎 Root Cause
Lỗi nằm ở cả model (`ItemDefinition.cs`), loader (`ItemFieldsLoader.cs`) và cơ chế parse của `JsonUtility`:
1. **Casing & Type Mismatch:** `JsonUtility` parse JSON (có các field `price`, `rarity`) không thể map vào `ItemDefinition` (có `public long Price`, `public int Rarity`) do khác chữ hoa/thường và khác type (`rarity` trong JSON là string như "COMMON").
2. **Thiếu sót của Loader:** File `items.json` giấu một số thông tin trong object `fields`. Nhưng `ItemFieldsLoader.cs` chỉ lấy đúng các chỉ số chiến đấu (Constitution, MaxHp...), hoàn toàn bỏ qua `idImage` và `notSellable`.

---

## 2. ID và Sprite Mapping (LegacySpriteCatalog)
Bộ nhớ UI sprite mapping giữa file JSON và `LegacySpriteCatalog.asset` (tổng cộng 1032 entries).

### 🟢 Thống kê
- **Khớp trực tiếp (item.id == sprite name):** 596 items (Thành công 100%).
- **Cần fallback sang `idImage`:** 11 items.
- **Thực sự thiếu asset (Missing):** 0 items.

### 🔴 Danh sách Exact Item IDs lỗi (11 Items)
Vì lỗi ở Mục 1 không nạp `idImage`, 11 item sau đây dù CÓ asset nhưng game không biết để lấy, dẫn đến hiển thị lỗi:
`avian_egg`, `celestial_mercy`, `construct_egg`, `esoteric_egg`, `evo_23_vial`, `evo_23_vial_2`, `hellish_rations`, `insect_egg`, `reptile_egg`, `wild_egg`, `wooden_egg`.

---

## 3. Lỗ hổng DefinitionId vs InstanceId
Phát hiện sự nhập nhằng nghiêm trọng giữa Định nghĩa (Definition - "Loại kiếm") và Thực thể (Instance - "Thanh kiếm số 1", "Thanh kiếm số 2").

### 🔴 Các API sai signature
- **`MerchantService.SellItem(string definitionId, int stackCount)`**: Thay vì nhận `instanceId` của vật phẩm cụ thể người chơi đang chọn bán, hàm này lại ép truyền vào `definitionId`.
- **Hệ quả ở UI**: `StorageItemDetailPanel.cs` bắt buộc phải gọi `SellItem(_item.Definition.id)`. UI chọn đúng món đồ, backend lại ném đi cái định danh (InstanceId) của nó.

### 🔴 Lỗ hổng nuốt vật phẩm trang bị / khóa
- **`InventoryService.ConsumeByDefinitionId`**: Hàm này duyệt qua inventory và trừ số lượng của BẤT KỲ món đồ nào khớp `definitionId` cho đến khi đủ số lượng.
- **Root Cause**: Nó KHÔNG KIỂM TRA thuộc tính `IsLocked`. 
- **Mức độ**: Rất nghiêm trọng (Data Loss). Nếu người chơi bán hoặc craft một nguyên liệu, hệ thống có thể vô tình đem tiêu hủy thanh kiếm/trang bị đang nằm trên người nhân vật hoặc đang khóa an toàn.

---

## 4. Save/Load Integrity & Logic Conflict
Cấu trúc `ItemSaveData` khá ổn định (lưu đủ `DefinitionId`, `InstanceId`, `StackCount`, `IsLocked`), ID không bị đổi qua lại. TUY NHIÊN, phát hiện một logic hủy diệt (destructive logic) trong `EquipmentService`.

### 🔴 Xung đột IsLocked (Equipped vs Manually Locked)
- Tính năng `IsLocked` được dùng chung cho hai mục đích: (1) Vật phẩm đang được trang bị, (2) Người chơi tự bấm khóa trong kho.
- **Root Cause**: Hàm `EquipmentService.SyncSave()` có một đoạn code quyét toàn bộ nhân vật. Nó lấy tất cả các món đồ đang được trang bị và gán `IsLocked = true`. Sau đó, nó **gán `IsLocked = false` cho TẤT CẢ các item còn lại**.
- **Hậu quả**: Bất cứ khi nào người chơi thay đổi trang bị của MỘT nhân vật bất kỳ, TẤT CẢ các vật phẩm mà người chơi đã cất công "Bấm khóa (Lock)" trong kho sẽ bị MỞ KHÓA đồng loạt.

---
