# Báo Cáo Audit Architecture: Rebuild Guild Master

## 1. Tổng quan Project
- **Tên dự án:** Rebuild_GuildMaster
- **Unity Version:** 6000.3.17f1
- **Render Pipeline:** Universal Render Pipeline (URP)
- **Tình trạng:** Project đang ở giai đoạn khởi tạo (Phase 00). Hệ thống file và thư mục đã được setup bài bản, tuy nhiên hoàn toàn **trống trơn về mặt mã nguồn C#**. Dự án đóng vai trò như một bộ khung sạch để tiến hành chuyển ngữ mã nguồn từ bản Android Native (Java) sang Unity (C#).

## 2. Folder Structure (Cấu trúc thư mục)
Dự án được phân rã theo Domain-driven chuẩn mực:
- `Assets/_Game/Art` (Chứa ảnh chia theo Areas, Backgrounds, Characters, UI...)
- `Assets/_Game/Audio` (Âm thanh)
- `Assets/_Game/Data` (Dữ liệu tĩnh Definition và Runtime)
- `Assets/_Game/Prefabs` (Prefab chia theo Dialogs, UI, Cards)
- `Assets/_Game/Scenes` (Boot, Main)
- `Assets/_Game/Scripts` (Phân chia rạch ròi các hệ thống: Core, Combat, UI, Economy, Save...)
- `Assets/StreamingAssets/GameData` (Dữ liệu thô bên ngoài)
- Các thư mục ngoài Assets: `Docs/`, `Reports/`, `Builds/Android/`

## 3. Các Package Đang Dùng (Core Dependencies)
- `com.unity.render-pipelines.universal` (URP 2D/3D)
- `com.unity.inputsystem` (New Input System)
- `com.unity.2d.*` (Các công cụ phát triển game 2D, Tilemap, Animation)
- `com.unity.ugui` (Hệ thống giao diện người dùng)
- `com.gamelovers.mcp-unity` (Hỗ trợ AI Agent thao tác với Editor)

## 4. Danh sách Scene
- `Boot.unity`: Scene khởi động, đóng vai trò nạp dữ liệu nền, khởi tạo các Singleton Service, kiểm tra file Save trước khi vào game.
- `Main.unity`: Scene Gameplay và UI chính của người chơi.

## 5. Danh sách Module Hiện Có
*Về mặt code: 0 Module (Chưa có script nào được viết).*
*Về mặt cấu trúc đã chuẩn bị:* Core, Data, Offline, Quests, UI, Services, Save, Pets, Economy, Inventory, Equipment, Combat, Characters, Buildings, Areas.

## 6. Phân loại Module (Dựa trên đối chiếu với bản gốc Java)
- **REUSE (Dùng lại nguyên bản):** **Không có.** (Mã nguồn Java không thể chạy trực tiếp trên Unity, toàn bộ tài nguyên hình ảnh có thể REUSE nhưng code thì không).
- **ADAPT (Chuyển ngữ/Điều chỉnh):** 
  - `Formulas.java` -> Sẽ adapt thành `Static Formulas C# Class`.
  - Cấu trúc `Data.java`, `entities/*`, `items/*` -> Adapt thành hệ thống C# Data Models.
- **REPLACE (Thay thế hoàn toàn):**
  - Hệ thống UI Android Native (XML) -> Thay thế bằng **Unity UGUI**.
  - `SaveManager` & `FileManager` -> Thay thế bằng **Unity JSON Utility** hoặc **Newtonsoft.Json** và ghi vào `Application.persistentDataPath`.
  - Threading của Android -> Thay thế bằng **Coroutines** hoặc **UniTask/C# Tasks**.
- **NEW (Viết mới hoàn toàn):**
  - Hệ thống Monobehaviour Game Loop (Event System, Game State Manager, Scene Loader).
  - Hệ thống Animation Controller (vì bản Java có thể chỉ là đổi ảnh sprite thô sơ).

## 7. Kiến trúc Hiện Tại
- **Scaffolding (Khung sườn):** Trống. Chưa áp dụng bất kỳ pattern nào (MVC/MVVM/ECS) vào file thực tế. Tuy nhiên, folder structure định hướng rõ ràng việc tách biệt Data - Logic - UI.

## 8. Dependency
- Hoàn toàn chưa có external library C# (như Newtonsoft.Json, DOTween, Zenject). Mọi thứ đều đang phụ thuộc vào built-in package của Unity.

## 9. Điểm Mạnh
- **Bắt đầu từ Zero (Clean Slate):** Không mang theo nợ kỹ thuật (Technical Debt). Không có code rác cản trở.
- **Modular:** Việc chia nhỏ các folder scripts (`Combat`, `Inventory`, `Economy`) ép buộc lập trình viên phải tư duy tách biệt chức năng ngay từ Phase 01.

## 10. Điểm Yếu
- **Thiếu System Core:** Chưa có bộ Service Locator hoặc Dependency Injection (DI) để quản lý vòng đời của các Manager (VD: SaveManager, DataManager, AudioManager).
- **Thiếu Data Framework:** Chưa có template ScriptableObject để bóc tách thông số khỏi code cứng. Bản gốc Java đang hardcode rất nhiều chỉ số.

## 11. Những Rủi Ro
- **Sai số toán học:** Quá trình "Adapt" class `Formulas` từ Java sang C# có rủi ro sai số kiểu dữ liệu (Float vs Double, cách làm tròn Round/Floor) khiến cân bằng game (Balance) bị lệch so với bản gốc.
- **Save Corruption:** Việc đồng bộ và serialize cấu trúc `Data` khổng lồ thành file lưu trữ đòi hỏi phải ánh xạ cực kỳ chuẩn xác cấu trúc JSON cũ (nếu muốn tương thích save cũ), hoặc phải thiết kế cẩn thận để sau này update game không làm người chơi mất dữ liệu.
- **Hiệu năng UI (UI Overdraw):** Idle game có vô số danh sách (ScrollRect), text thay đổi liên tục. Nếu build UI bằng cách instantiate hàng loạt prefab mà không dùng Object Pooling sẽ gây sụt FPS.

## 12. Đề Xuất Kiến Trúc Để Rebuild Guild Master
Để đảm bảo dự án scale tốt và dễ bảo trì, với tư cách Architect, tôi đề xuất:

1. **Áp Dụng Kiến Trúc "Data-Driven + MVP":**
   - **Data (Model):** Tạo một class `GameData` thuần C# (POCO). Class này chỉ chứa trạng thái hiện tại (level, vàng, danh sách nhân vật). Serialize class này thành JSON để save/load.
   - **Definitions (Scriptable Objects):** Mọi thông tin cố định (tên nhân vật gốc, lượng máu cơ bản, icon) không được lưu trong file Save. Phải chuyển chúng thành `ScriptableObject` để Designer dễ chỉnh sửa trực tiếp trên Unity Editor mà không cần can thiệp code.
   - **Logic (Controller/Service):** Xây dựng các Manager (InventoryManager, CombatManager). Các manager này đọc `ScriptableObject` và tác động thay đổi lên `GameData`. Cốt lõi tính toán đặt tại class tĩnh `Formulas`.
   - **View (UI - MVP):** Các Script UI (View) chỉ làm nhiệm vụ hiển thị hình ảnh/text. Mọi thao tác bấm nút sẽ gửi event (Action/Delegate) về cho Presenter hoặc Manager xử lý. Không để code UI trực tiếp cộng trừ máu hay vàng.

2. **Core Services (Khởi tạo tại Boot.unity):**
   - Sử dụng mô hình **Service Locator** đơn giản (hoặc Singleton nếu project nhỏ) để đăng ký các hệ thống thiết yếu: `SaveSystem`, `TimeSystem` (quản lý thời gian offline), `AudioSystem`. Đánh dấu chúng `DontDestroyOnLoad`.

3. **External Dependencies Cần Bổ Sung (Phase 01):**
   - **Newtonsoft.Json:** Bắt buộc cài đặt để xử lý cấu trúc JSON phức tạp từ bản Java một cách an toàn nhất.
   - Thư viện hỗ trợ tweening UI (như **DOTween** hoặc **PrimeTween**) để làm UI mượt mà, chuyên nghiệp hơn.

*Báo cáo Audit Architecture kết thúc.*
