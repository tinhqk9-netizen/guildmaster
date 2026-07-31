# 10. Batch 1 Data Save Refresh & Safety Contract (Đã Cập Nhật Thực Tế)

Báo cáo kiểm định an toàn dữ liệu lưu trữ (Save Safety), đề xuất cơ chế đồng bộ UI (Refresh Contract) và phân tích phòng ngừa xung đột khởi tạo (Load Timing - Init Race).

## 1. Kết Quả Kiểm Định An Toàn Dữ Liệu (Save Safety Audit)

Để đảm bảo tệp save game không bị lỗi dẫn đến hỏng toàn bộ dữ liệu người chơi, hệ thống GuildMaster thực tế đã trang bị các lớp phòng vệ vững chắc sau:

- **Khởi tạo vá lỗi thiếu trường (`NormalizeAfterLoad`)**:
  - Khi game load dữ liệu JSON, nếu người chơi đang sử dụng file save cũ (hoặc file JSON bị khuyết thiếu các trường mới thêm do cập nhật game), `SaveData.NormalizeAfterLoad()` (`SaveData.cs:295-321`) sẽ tự động kiểm tra và khởi tạo các danh sách/list null về một danh sách rỗng (vd: `WorkshopQueue`, `Items`, `Characters`, `Quests`, `TavernGuests`, v.v.).
  - Việc này loại trừ hoàn toàn nguy cơ 발생 `NullReferenceException` khi truy cập dữ liệu save vừa nạp.
- **Cơ chế khôi phục từ file Backup (Robust Recovery)**:
  - Trong trường hợp tệp `save.json` chính bị lỗi cú pháp JSON or không thể đọc: `SaveService.Load(out Exception error)` (`SaveService.cs:60-88`) sẽ tự động chuyển sang đọc tệp backup `save_backup.json`.
  - Nếu tệp backup cũng hư hỏng hoặc không tồn tại, game tự động fallback khởi tạo một profile game mới (`SaveData.CreateDefault()`) để tránh việc bị treo màn hình trắng (White Screen Crash) hoặc crash game hoàn toàn lúc startup.
  - Trước khi ghi đè dữ liệu mới vào `save.json` ở hàm `Save()`, hệ thống luôn sao lưu tệp hoạt động cũ sang `save_backup.json` (`SaveService.cs:110`).

## 2. Phòng Ngừa Xung Đột Khởi Tạo (Init Race Prevention)

Một rủi ro phổ biến trong thiết kế lưu trữ là game loop cố gắng tự động lưu (Save) dữ liệu ngay khi vừa khởi động xong, trước khi quá trình nạp (Load) dữ liệu hoàn thành – dẫn đến ghi đè một file JSON trắng rỗng và xóa sạch tiến trình của người chơi.

Đối với GuildMaster:
1. **Nạp dữ liệu đồng bộ (Sync Load)**: Vì `SaveService.Load()` được chạy trực tiếp trong constructor của `ServiceContainer.cs:54`, quá trình nạp dữ liệu hoàn tất trước khi bất kỳ Object MonoBehaviour nào khác (như `GameLoopRunner`) được khởi chạy hoặc update.
2. **Trì hoãn lần Auto-Save đầu tiên**: Trong `GameLoopRunner.cs:40-45`, lần tự động lưu đầu tiên khi boot game được trì hoãn ít nhất **8.0 giây** thông qua biến kiểm tra `_isFirstSave`. Tất cả các lần tự động lưu định kỳ tiếp theo sẽ diễn ra bình thường mỗi 3.0 giây. Điều này tạo ra một cửa sổ thời gian an toàn cho toàn bộ hệ thống GameManager ổn định trạng thái trước khi thực hiện hành vi ghi đè file save.

## 3. Bản Hợp Đồng Đồng Bộ Giao Diện (Refresh UI Contract)

Để đảm bảo dữ liệu hiển thị trên UI đồng bộ tức thì với bộ nhớ Backend mà không cần liên tục thăm dò (polling), backend và UI duy trì cơ chế gọi hàm refresh sau mỗi mutate:

| Hành động của Người chơi (Action) | Service Mutation Method | Cơ Chế Đồng Bộ Trực Tiếp Lên UI |
| :--- | :--- | :--- |
| **Tuyển mộ Adventurer** | `TavernService.RecruitGuest(...)` | UI TavernScreen thực hiện gọi `Refresh()` ngay sau khi check API thành công để cập nhật lại danh sách Card khách trọ và số lượng quarters đã tuyển. |
| **Nâng cấp công trình Tavern** | `TavernService.UpgradeQuarters()` / `UpgradeTavernCapacity()` / `UpgradeTavernTime()` | UI TavernScreen update lại text hiển thị Level và nút nâng cấp tương ứng, đồng thời trừ tiền vàng hiển thị trên HUD. |
| **Mang/Tháo trang bị** | `EquipmentService.Equip()` / `Unequip()` | UI CharacterScreen gọi `Refresh()` để vẽ lại thông tin text chi tiết của Adventurer (Stats gốc + Stats bonus từ vũ khí mới) và cập nhật trạng thái nút Equip. |
| **Nhập/Xóa đội hình Battle** | `CharacterScreen.OnClickAddToParty()` / `OnClickRemoveFromParty()` | UI update biến cục bộ `_partyIds` và kích hoạt vẽ lại card của tướng tương ứng (hiển thị ký tự ★ đánh dấu đã vào đội). |
| **Nhận quà Quest** | `QuestService.ClaimReward(...)` | UI QuestScreen xóa card quest đã hoàn thành khỏi Grid và reset index lựa chọn, đồng thời UI HUD cập nhật lại lượng Vàng/Gems/Progress Doctrine tăng thêm. |
