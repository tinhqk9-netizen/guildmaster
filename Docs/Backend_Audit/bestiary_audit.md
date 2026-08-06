# Bestiary System Audit

**Dự án:** `D:\Tinh\Rebuild_GuildMaster`
**Legacy source:** `D:\Tinh\Guild Master - Idle Dungeons`
**Trạng thái:** Deep Audit Only (Không fix code, Không sửa data)

---

## 1. Bestiary Inventory

### Legacy Java
- **Định nghĩa:** Bestiary đóng vai trò là một Bách khoa toàn thư (Encyclopedia) hiển thị các quái vật theo khu vực (Area).
- **Phân loại:** Nhóm theo **Dungeons** và **Raids**. Mỗi Area chứa một danh sách `Enemy`.
- **Dữ liệu lưu trữ:** Chỉ lưu trữ trạng thái **đã phát hiện** (Discovery). Không lưu trữ số lượng tiêu diệt (Kill Count).
- **Phần thưởng:** Không có phần thưởng khi hoàn thành Bestiary.

### Rebuild C#
- **Định nghĩa:** `EnemyDefinition.cs` quản lý data quái vật tĩnh.
- **Dữ liệu lưu trữ:** Hoàn toàn **KHÔNG LƯU TRỮ** trạng thái phát hiện.

---

## 2. Data Pipeline & Progression Logic

### Legacy Java
- Trạng thái discovery được lưu trong `Data.java` dưới dạng `Set<String> seenEnemies`.
- **Trigger phát hiện (Discovery):** 
  - Trong lúc combat `Area.java:445`, khi một Enemy xuất hiện, class name (`trueClass`) của nó được add vào `seenEnemies`.
  - Một số event đặc biệt (ví dụ: `DialogRedeemCode.java` nhập code, hoặc script của raid `CelestialMothership.java`) có thể force-add enemy vào danh sách đã thấy.

### Rebuild C#
- **Missing Data:** Biến `seenEnemies` bị loại bỏ hoàn toàn khỏi `SaveData.cs`.
- **Missing Runtime:** Xóa bỏ hoàn toàn trigger phát hiện quái vật trong vòng lặp Combat (`DungeonService.cs`). Người port code trước đây đã ghi chú cứng trong `AuxiliaryController.cs:430`: *"no 'discovered' concept in the backend, so the full catalog is shown — a deliberate, documented deviation from legacy fog-of-war"*.

---

## 3. Reward / Economy Integration

- **Legacy Java:** Không liên kết với hệ thống economy.
- **Rebuild C#:** Không liên kết với hệ thống economy.

---

## 4. UI Audit

### Legacy Java
- File: `DialogBestiary.java`, `UIUtils.java`.
- **Hiển thị:** 
  - Có 2 danh sách phân nhóm rõ ràng (Dungeons List, Raids List).
  - Quái vật trong mỗi khu vực được hiển thị dưới dạng Grid.
- **Fog of War:** Quái vật chưa từng gặp sẽ bị thay hình ảnh bằng dấu chấm hỏi (`R.drawable.unknown`) và **KHÔNG THỂ** click vào để xem chi tiết (Stats, Drop info). Chỉ quái vật có trong `seenEnemies` mới hiển thị hình ảnh và mở ra popup `DialogItemDetail/EnemyDetail`.

### Rebuild C#
- File: `AuxiliaryController.cs` (Phase 9 - Bestiary Hub).
- **UI Mismatch & Regression:** 
  - Hủy bỏ hoàn toàn cấu trúc phân loại theo Dungeon/Raid. Quái vật bị gộp chung vào một danh sách dọc duy nhất, xếp theo Alphabet.
  - Phá vỡ hoàn toàn cơ chế Fog of War: Mọi quái vật đều hiển thị rõ ràng từ đầu game, có thể click vào xem thông số và rơi đồ dù chưa từng đánh.

---

## 5. Save/Load Integrity

### Legacy Java
- `seenEnemies` được parse từ mảng JSON thông qua `DataDeserializer.java`.

### Rebuild C#
- Do loại bỏ trường này khỏi `SaveData.cs`, file save khi chuyển từ Legacy sang C# sẽ bị **mất hoàn toàn dữ liệu** `seenEnemies`.

---

## 6. Reference Integrity Table

| Feature | Legacy Behavior | Current C# | Status |
|---|---|---|---|
| Discovery State | Lưu trong `Set<String> seenEnemies` | Bị xóa khỏi model `SaveData` | 🔴 Missing Data |
| Combat Discovery | Unlock enemy khi gặp trong `Area.java` | Bị xóa khỏi combat loop | 🔴 Missing Runtime |
| Bestiary Grouping | Phân loại theo Dungeon và Raid | Danh sách gộp phẳng (Alphabet) | 🔴 UI Mismatch |
| Fog of War (UI) | Quái vật chưa gặp hiển thị icon `?` | Hiển thị toàn bộ từ Level 1 | 🔴 Wrong Logic |
| Legacy Save Load | Load array `seenEnemies` từ file JSON | Bỏ qua dữ liệu này | 🔴 Missing Logic |
