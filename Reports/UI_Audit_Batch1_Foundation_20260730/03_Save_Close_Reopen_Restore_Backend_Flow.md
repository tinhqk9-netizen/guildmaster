# 03. Save Close Reopen Restore Backend Flow (Đã Cập Nhật Thực Tế)

Báo cáo chi tiết về luồng Lưu trữ (Save), Đóng game (Close), và Khôi phục trạng thái khi Mở lại (Reopen/Restore).

- **Kiểu lưu trữ:** Tự động lưu (`Auto-save`) định kỳ mỗi 3.0s (sau lần đầu 8.0s) và lưu khi tạm dừng/thoát game (`OnApplicationPause`, `OnApplicationQuit`).
- **Trạng thái lưu trữ:** Dạng JSON văn bản thông thường (không mã hóa binary) lưu ở persistentDataPath (`save.json`).
- **Cơ chế khôi phục (Restore):** Diễn ra hoàn toàn đồng bộ (sync) ngay trong Constructor của `ServiceContainer.cs`. Các service phân tích và nạp trực tiếp danh sách từ save data thay vì dùng cơ chế event-driven.

## Ma Trận Persistence & Restoration Checkpoints

| Hoạt động Save/Restore | Class & Method Backend | Dữ Liệu Được Lưu/Khôi Phục | Trạng Thái UX / Phản Hồi UI | Chi Tiết Kỹ Thuật & Dẫn Chứng Code |
| :--- | :--- | :--- | :--- | :--- |
| **Ghi file Save tự động** | `GameLoopRunner.Update` gọi `_saveService.Save()` | Metadata (Save version, Unix Save Time, Game version) và toàn bộ `SaveData`. | Không hiển thị feedback trong game (chạy ngầm). | - Lưu đè `save.json`. Trước khi ghi, sao chép file cũ thành `save_backup.json`.<br>- Dẫn chứng: `GameLoopRunner.cs:40-47`<br>- `SaveService.cs:91-122` |
| **Ghi file khi Thoát/Tạm dừng** | `UIRuntimeBootstrap.OnApplicationPause` / `OnApplicationQuit` | Toàn bộ trạng thái save game hiện tại. | Không hiển thị feedback (chạy ngầm lúc đóng app). | - Gọi hàm phụ trợ `PersistSave(reason)`.<br>- Dẫn chứng: `UIRuntimeBootstrap.cs:156-178` |
| **Nạp dữ liệu từ Disk** | `SaveService.Load()` | Đọc chuỗi JSON từ file `save.json` và deserialize qua `JsonUtility.FromJson<SaveData>`. | Màn hình tải ban đầu. | - Tự động gọi `NormalizeAfterLoad()` để vá các trường list/array null.<br>- Cơ chế Robust Recovery: Khôi phục backup or fallback default.<br>- Dẫn chứng: `SaveService.cs:27-89` |
| **Khôi phục trạng thái Quests** | `QuestService.LoadQuests()` | Danh sách nhiệm vụ đang hoạt động trong RAM (`_activeQuests`). | Khôi phục danh sách QuestScreen. | - Đọc từ `_saveService.CurrentData.Quests`. Tạo các đối tượng `QuestRuntime`.<br>- Dẫn chứng: `QuestService.cs:94-113` |
| **Khôi phục trạng thái Hành Trang**| `InventoryService` Constructor | Danh sách Items đang sở hữu. | Khôi phục grid InventoryScreen. | - Đọc trực tiếp từ `SaveData.Items` và map vào dictionary runtime.<br>- Dẫn chứng: `InventoryService.cs:22-35` |
| **Khôi phục trạng thái Tavern Guests** | `TavernService` Constructor | Danh sách khách trọ đang chờ tuyển mộ. | Khôi phục TavernScreen cards. | - Trả về `_saveService.CurrentData.TavernGuests` qua `GetGuests()`.<br>- Dẫn chứng: `TavernService.cs:57-60` |
| **Trạng thái Active Party (Lỗi Kiến Trúc)** | **KHÔNG persist (Không có)** | Danh sách Hero đang ở trong đội hình chuẩn bị đi Dungeon. | **BỊ KHÓA / BỊ MẤT** khi thoát game or chuyển scene. | - **Backend Mismatch**: Trạng thái Party chỉ được lưu ở UI `CharacterScreen.cs:36` (`_partyIds` dạng HashSet). Không hề có trường lưu Party trong database save json! Mở lại game người chơi phải thiết lập lại đội hình.<br>- Dẫn chứng: `CharacterScreen.cs:36`, `61-74` |
