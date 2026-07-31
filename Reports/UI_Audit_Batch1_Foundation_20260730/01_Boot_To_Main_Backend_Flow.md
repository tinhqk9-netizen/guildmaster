# 01. Boot to Main Backend Flow (Đã Cập Nhật Thực Tế)

Báo cáo chi tiết về luồng khởi động (Boot) và nạp dữ liệu của trò chơi GuildMaster.

- **Scene mở đầu:** `Boot.unity` (Khi kiểm thử UI trong phạm vi S5 có thể chạy trực tiếp từ `Main.unity`).
- **Class production entry point:** `Bootstrapper.cs` (cho Boot scene) và `UIRuntimeBootstrap.cs` (cho Main scene).
- **Trình tự khởi tạo:** `Bootstrapper.Start` -> `InitializePipeline()` -> Khởi tạo `ServiceContainer`.
- **GameDatabase load ở đâu:** Được build đồng bộ trong `UIRuntimeBootstrap.Start()` (`UIRuntimeBootstrap.cs:52-53`) or `Bootstrapper.InitializePipeline()` (`Bootstrapper.cs:47-51`) thông qua `DatabaseBuilder`.
- **Save load ở đâu:** Được nạp **đồng bộ (sync)** trực tiếp trong Constructor của `ServiceContainer` (`ServiceContainer.cs:53-54`) khi gọi `newSave.Load(out _)`.
- **Offline Progress apply nằm trong startup không:** Có, tại `UIRuntimeBootstrap.cs:84` khởi tạo `OfflineProgressService` thông qua constructor của `ServiceContainer.cs:84`.

## Ma Trận Trình Tự Khởi Động Thực Tế (Boot Pipeline Verification)

| Hoạt Động | Class/Method Kích Hoạt | Trạng Thái Dữ Liệu | Giao Diện Hiển Thị | Cơ Chế Thất Bại & Bảo Vệ | Dẫn Chứng Code |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **1. Khởi tạo Database** | `DatabaseBuilder.Build()` | Đọc và parse các file JSON trong `StreamingAssets/GameData/` để nạp danh sách Items, Enemies, Quests, Dungeons... | Màn hình đen khởi động / Logo game | Nếu file manifest hoặc các definition bị lỗi hoặc thiếu dữ liệu, hệ thống ném ngoại lệ dừng pipeline. | `DatabaseBuilder.cs:51`<br>`Bootstrapper.cs:50-56` |
| **2. Khởi tạo Container & nạp Save** | `new ServiceContainer(...)` | Khởi tạo tất cả services và gọi đồng bộ `SaveService.Load()` để nạp `save.json` ở persistentDataPath. | Màn hình đen khởi động / Logo game | Nếu `save.json` bị lỗi cú pháp, tự động khôi phục từ `save_backup.json`. Nếu backup cũng lỗi, fallback về mặc định game mới (`SaveData.CreateDefault()`). | `ServiceContainer.cs:53-54`<br>`SaveService.cs:27-89` |
| **3. Khởi tạo các Service phụ thuộc** | Constructor của các Service tương ứng | Mỗi Service tự nạp và map dữ liệu từ `SaveService.CurrentData` vào RAM runtime (ví dụ Quest, Inventory...). | Màn hình đen khởi động / Logo game | Các service tự kiểm tra null dữ liệu save để tránh crash or load lỗi. | `ServiceContainer.cs:64-85`<br>`QuestService.cs:45-46` (LoadQuests) |
| **4. Load Main Scene & Wire UI** | `UIRuntimeBootstrap.Start()` | Load scene Main, đăng ký các Screen UI vào `UIService`, gọi `Initialize` trên các Screen Controller. | Chuyển cảnh sang HUD chính (`MainHUD`) | Nếu việc tìm/wire button bị null, ghi log lỗi nhưng không làm crash toàn hệ thống. | `UIRuntimeBootstrap.cs:55-111` |
| **5. Kích Hoạt Clock & Tick** | `GameLoopRunner.Initialize()` | Khởi chạy game loop. `TickRuntime()` chạy mỗi 1.0 giây, tự động lưu game (`Save`) định kỳ. | Màn hình HUD chính hoạt động | Trì hoãn lần Auto-Save đầu tiên 8.0 giây (`_isFirstSave`) để tránh ghi đè dữ liệu rỗng. | `GameLoopRunner.cs:15-47` |
