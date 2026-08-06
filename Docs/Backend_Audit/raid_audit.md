# Raid System Audit

**Dự án:** `D:\Tinh\Rebuild_GuildMaster`
**Legacy source:** `D:\Tinh\Guild Master - Idle Dungeons`
**Trạng thái:** Deep Audit Only (Không fix code, Không sửa data)

---

## 1. Raid Inventory

### Legacy Java
- **Tổng số Raid:** 12 Raids.
  - *AncientGraveDigging, CelestialMothership, DivineArcheology, ImperialRescue, Kaunis, SleepingPlanet, TheCultistRebels, TheDireDescent, TheDreadfulAscent, TheLostExpedition, TheSlimePond, TheTower.*
- **Phân loại:** Kế thừa từ `Area.java` chung với Dungeons nhưng `getAreaType()` trả về `1` (khác `0`).
- **Đặc điểm so với Dungeon:**
  - Không có tiến trình cày cuốc (progress) vô hạn như Dungeons.
  - Các Raid có tiến trình tuyến tính cố định (ví dụ: progress từ 1 đến 12).
  - Không có tính năng `searchRoom()` (không loot phòng trống).
  - Nếu tất cả Adventurer chết, Raid lập tức thất bại (`terminationRequested = true`), không tự hồi sinh và đánh lại phòng cũ.
  - Không bị phạt 20% EXP khi chết như Dungeons.

### Rebuild C#
- **Tổng số Raid khai báo:** 12 Raids (nằm trong file `raids.json`).
- Tuy nhiên, hệ thống Raid đã bị **CẮT BỎ HOÀN TOÀN** trong mã nguồn C#. Người port code đã cố ý drop tính năng này.

---

## 2. Raid Data Structure

### Legacy Java
- Logic Raid được hardcode trực tiếp vào các class riêng rẽ trong thư mục `storage/data/places/raids/`.
- Cấu trúc: Dùng `this.progress` để quản lý các Room (phòng) trong Raid.
- **Enemy Groups & Bosses:** Mỗi progress level sẽ return một `List<Enemy>` cố định (hoặc mảng trống nếu là phòng nghỉ/thoại).
- **Special Events:** Có hệ thống event qua hàm `triggerEvent(String)` để sinh thoại (logs) ở đầu mỗi phòng hoặc khi bắt đầu combat (`enter_room`, `fight_start`).
- **Custom Mechanics:** Có cơ chế riêng như: Giết boss "KabarTheRotten" thì toàn bộ đệ tử "Necrolith" chết theo, đồng thời kích hoạt Achievements và Quest.

### Rebuild C#
- Class `RaidDefinition.cs` chỉ chứa 2 trường duy nhất:
  - `RequiredClearDungeonId`
  - `RequiredClearProgress`
- **Hoàn toàn biến mất:** KHÔNG có danh sách kẻ thù, không có sự kiện đặc biệt, không có reward, không có progress states. Toàn bộ kịch bản Raid bị xóa sổ.

---

## 3. Encounter & Combat Flow

- **Legacy Java:** Sử dụng chung combat loop với Dungeons nhưng phân nhánh ở `Action 1` (khám phá) và `Action 4` (loot).
- **Rebuild C#:** Không tồn tại. Game không có `RaidService` hay cơ chế chạy Raid. 

---

## 4. Reward System

- **Legacy Java:** Hoàn thành Raid thưởng items đặc thù, trigger Quests (ví dụ Quest `andStayDead` khi diệt boss AncientGraveDigging) và Achievements.
- **Rebuild C#:** Hoàn toàn mất tính năng thưởng.

---

## 5. Progression / Unlock

### Legacy Java
- Unlock thông qua Dungeon Progress và `Utils.compileDungeonList()`.

### Rebuild C#
- Vẫn giữ được logic unlock cơ bản qua `SaveData.Dungeons[].MaxProgress` kiểm tra đối chiếu với `RequiredClearDungeonId` từ JSON. Tuy nhiên, việc unlock không có ý nghĩa vì không thể bắt đầu chơi Raid.

---

## 6. Save / Load Integrity

- **Legacy Java:** Trạng thái Raid (đang đánh ở đâu) không được lưu vĩnh viễn (do Raid chết là kết thúc ngay). Tiến trình vượt qua chỉ là cờ check.
- **Rebuild C#:** Không lưu bất cứ gì liên quan tới tiến độ Raid vào `SaveData.cs`.

---

## 7. UI Audit

### Legacy Java
- **UI:** Cho phép chọn Raid, xếp đội hình riêng, thông báo phần thưởng, có thanh trạng thái tiến trình (rooms), và sinh logs chi tiết theo kịch bản ở mỗi phòng.

### Rebuild C#
- **UI (`RaidsTabController.cs`):** 
  - Giao diện có hiển thị danh sách 12 Raid. 
  - Tuy nhiên, trong code gốc, tác giả để lại ghi chú giải thích rằng họ không làm tính năng này.
  - Khi người chơi click vào một Raid, thay vì setup đội hình, màn hình sẽ hiển thị một thông báo **"fallback note"** nói rằng tính năng chưa hoàn thiện và không cho phép chơi. Không có combat, không có logs, không có tính năng nào hoạt động.

---

## 8. Reference Integrity Table

| Feature | Legacy Behavior | Current C# | Status |
|---|---|---|---|
| Raid Data Payload | Gồm Enemy, Events, Drops, Logs | Chỉ lưu điều kiện Unlock | 🔴 Missing Data |
| Raid Combat Flow | Cố định theo progress, chết là thua, không trừ EXP | Hoàn toàn không tồn tại | 🔴 Missing Runtime |
| Custom Encounter Events | Dialogue logs, custom boss kill scripts | Không được port sang | 🔴 Missing Runtime |
| Quest / Achievement Hooks | Gọi `QuestsManager` và `AchievementsUtils` | Không tồn tại | 🔴 Missing Runtime |
| UI & Interaction | Set team, start raid, show narrative logs | Hiển thị thông báo "Chưa làm" | 🔴 Missing Logic |
