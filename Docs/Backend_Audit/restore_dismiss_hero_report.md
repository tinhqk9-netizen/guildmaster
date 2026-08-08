# Restore Hero Dismiss Button Report (Character Detail UI Regression)

**Dự án:** `D:\Tinh\Rebuild_GuildMaster`  
**Trạng thái:** Đã hoàn thành & Đã kiểm tra  

---

## 1. Backend Flow Audit (Kiểm tra quy trình Backend)

- **File chứa logic Dismiss:**  
  `Assets/_Game/Scripts/Runtime/Services/CharacterService.cs` (lines 369–442)
  - Interface: `ICharacterService.cs` (`CanDismissCharacter`, `DismissCharacter`)

- **Quy trình hoạt động hiện có của Backend:**
  1. **`CanDismissCharacter(instanceId, out reason)`**:  
     - Kiểm tra xem Hero có nằm trong `CurrentParty` (đội hình chính), `ActiveDungeon` (đang đi dungeon), hoặc `ActiveExpeditions` (đang đi thám hiểm) hay không.  
     - Nếu có, trả về `false` kèm lý do cụ thể (ví dụ: *"Cannot dismiss a character while they are in the active party."*).
  2. **`DismissCharacter(instanceId, out errorReason)`**:  
     - Mở khóa an toàn cho các trang bị đang đeo (`Weapon`, `Armor`, `Accessory`) ➔ Đặt `IsLocked = false` để giữ nguyên vật phẩm trong hòm đồ.
     - Xóa Hero khỏi danh sách `SaveData.CurrentData.Characters`.
     - Xóa Hero khỏi bộ nhớ runtime `_characters`.

---

## 2. File UI đã sửa (UI Modifications)

- **File UI đã chỉnh sửa:**  
  `Assets/_Game/Scripts/Runtime/UI/Character/CharacterDetailPanel.cs`

- **Chi tiết thay đổi UI:**
  1. **Khôi phục Nút "Dismiss Hero":**
     - Đặt ở phần các nút hành động (dưới Doctrine, ngay trên nút Back to Adventurers).
     - Không làm thay đổi hay xáo trộn bất kỳ section nào trong bố cục hiện tại (Portrait, Stats, Traits, Skills, Equipment, Doctrine, Promotion).
     - Trạng thái nút tự động cập nhật:
       - Nếu đủ điều kiện ➔ Hiển thị nút bấm `"Dismiss Hero"`.
       - Nếu không đủ điều kiện (ví dụ: đang ở trong Party) ➔ Hiển thị nút bị vô hiệu hóa kèm lý do: `"Dismiss (Cannot dismiss a character while they are in the active party.)"`.

  2. **Thêm Giao diện Xác nhận (Confirm Dialog Overlay):**
     - Khi bấm nút `Dismiss Hero`, giao diện `ConfirmDismissOverlay` sẽ hiển thị để hỏi lại người chơi:
       > *"Are you sure you want to dismiss [Name] (Lv.[X])? Equipped items will be unlocked and returned to inventory."*
     - Nút **`YES, DISMISS HERO`**: Gọi `DismissCharacter`, lưu SaveData, đóng popup và refresh danh sách Roster.
     - Nút **`CANCEL`**: Đóng popup xác nhận, giữ nguyên Hero.

---

## 3. Kết quả Test (Verification Results)

- **Backup:**  
  Đã tạo bản sao lưu tại `Backups/PreRestoreDismiss_Backup.zip`.

- **Unit Test EditMode (`CharacterDismissTests.cs`):**
  1. `DismissCharacter_ValidHero_RemovesFromRosterAndSaveData`: **PASSED**  
     - Thử nghiệm xóa 1 Hero ➔ Hero lập tức biến mất khỏi runtime roster và SaveData. Save/Load không còn Hero đó.
  2. `DismissCharacter_ActiveParty_FailsWithReason`: **PASSED**  
     - Đưa Hero vào `CurrentParty` ➔ Nút Dismiss bị vô hiệu hóa với đúng lý do, không thể xóa nhầm Hero đang ra trận.
  3. `DismissCharacter_EquippedItems_UnlocksItems`: **PASSED**  
     - Thử nghiệm đeo vũ khí khóa (`IsLocked = true`) ➔ Sau khi Dismiss, vũ khí tự động mở khóa (`IsLocked = false`) và nằm an toàn trong rương đồ.
  4. Các Hero khác trong danh sách không bị ảnh hưởng.
