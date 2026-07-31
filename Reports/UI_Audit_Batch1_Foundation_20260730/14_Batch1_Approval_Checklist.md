# 14. Batch 1 Approval Checklist (Đã Cập Nhật Thực Tế)

Báo cáo phân loại các đầu việc cần phê duyệt, quyết định thiết kế và các bước triển khai tiếp theo dựa trên kết quả audit thực tế của Dự án GuildMaster.

## Phân Phối Tình Trạng Điểm Phê Duyệt (Status Distribution)

- **Nhóm A: Sẵn sàng triển khai ngay** (Code và dữ liệu UI-Backend đã đồng bộ).
- **Nhóm B: Cần điều chỉnh Backend trước** (Thiếu Model hoặc API để UI hiển thị chuẩn).
- **Nhóm C: Có thể triển khai kèm giải pháp tạm thời** (Workaround phía UI).
- **Nhóm D: Cần quyết định nghiệp vụ từ Game Designer** (Sếp Tinh quyết định).

## Bảng Đánh Giá Mức Độ Sẵn Sàng Triển Khai

| Luồng Nghiệp Vụ | Screen / Module UI | Nhóm Phân Loại | Lý Do Kỹ Thuật (Phát hiện từ Code) | Sự Phụ Thuộc (Dependency) | Đề Xuất / Hành Động Khuyến Nghị | Trạng Thái |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **Boot & Load** | `BootScreen` / Nạp game | **Nhóm A** | Boot và Load Save nạp đồng bộ (Sync) trong Constructor của ServiceContainer. Không lo trùng lặp or treo. | `ServiceContainer.cs`<br>`SaveService.cs` | Thể hiện thanh Loading mượt mà trên UI. Thiết kế màn hình lỗi loading thân thiện để show thông tin rollback. | SẴN SÀNG |
| **Tavern System**| `TavernScreen` | **Nhóm A** | Recruit Guest và Tavern Upgrades đã đồng bộ hoàn toàn kiểu dữ liệu Gold nâng cấp dạng `long`. | `TavernService.cs` | Triển khai UI chiêu mộ bình thường. Đảm bảo UI cập nhật lại danh sách Guests ngay sau khi gọi Recruit thành công. | SẴN SÀNG |
| **Character Info**| `CharacterScreen` | **Nhóm C** | Equip items có mismatch nhỏ do Controller tự động iterate tìm slot. | `EquipmentService.cs` | Sửa UI để cho phép người chơi chọn slot cụ thể (Weapon, Armor, Accessory) trước khi bấm Equip thay vì tự tìm slot. | THAY THẾ ĐƯỢC |
| **Dungeon Team** | Đội hình Party | **Nhóm B** | **Rủi ro mất đội hình**: HashSet `_partyIds` chỉ tồn tại ở runtime, không lưu trữ trong file save JSON. | `SaveData.cs` | Phải sửa schema `SaveData` để lưu danh sách Id của party hoạt động nhằm tránh reset đội hình khi thoát game. | BỊ TREO (BLOCK) |
| **Quest Claim** | `QuestScreen` | **Nhóm C** | **Rút Gems phớt lờ Doctrine**: Quest đặc biệt (rarity >= 4) bỏ qua lựa chọn doctrine của người chơi và tự động cộng Gems. | `QuestService.cs` | Trên giao diện QuestScreen, tự động ẩn nút chọn Doctrine nếu Quest được chọn có Rarity >= 4, và thay đổi text hiển thị phần thưởng thành Gems. | THAY THẾ ĐƯỢC |
| **Text Wrapping** | Craft, Inventory & Dungeon Screens | **Nhóm A** | Chữ bị bẻ đôi do constraints RectTransform hẹp và legacy Text component. | Prefabs UI | Bật chế độ `Best Fit` trên Text Legacy, đặt Horizontal Wrap Mode = `Overflow` và scale lại chiều rộng tối thiểu của UI cards. | SẴN SÀNG |
