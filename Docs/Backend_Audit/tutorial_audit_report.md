# Legacy Tutorial / Onboarding System Audit Report

## 1. Legacy Tutorial Findings

**Tutorial system exists in Legacy:** YES.

Hệ thống Tutorial của bản Legacy Java khá đơn giản, không sử dụng Highlight, Masking hay Hand Pointer (bàn tay chỉ dẫn) để khóa UI. Thay vào đó, nó hiển thị một panel cố định (`containerTutorial`) trên màn hình `MainActivity` để nhắc nhở người chơi nhiệm vụ tiếp theo.

**Các đặc điểm chính:**
- **Thời điểm bắt đầu:** Ngay khi tạo Save mới (mặc định `tutorialStep = 1`).
- **Lưu tiến trình:** Lưu trong `Data.java` dưới dạng field `tutorialStep`.
- **Skip:** Không có nút Skip chính thức, người chơi bắt buộc phải hoàn thành các hành động để ẩn UI Tutorial.
- **Số lượng bước:** 6 bước (kết thúc ở bước 7).

**Thứ tự các bước & Trigger hoàn thành:**
1. **Step 1:** Recruit an adventurer (Chiêu mộ anh hùng ở Tavern).
   - *Trigger:* Khi nhấn chiêu mộ thành công trong `DialogTavern.java`, step tự tăng 1 -> 2.
2. **Step 2:** Send adventurer to Enchanted Forest.
   - *Trigger:* Khi đánh quái và nhận được món đồ loot đầu tiên ở Enchanted Forest (thường là đánh bại `TutorialWolf` để rớt Leather), step tăng 2 -> 3 (`Area.java` line 688).
3. **Step 3:** Craft Leather at the Workshop.
   - *Trigger:* Khi craft thành công item `Leather` trong `DialogWorkshop.java`, step tăng 3 -> 4.
4. **Step 4:** Craft Copper Armor.
   - *Trigger:* Khi craft thành công item `CopperArmor`, step tăng 4 -> 5.
5. **Step 5:** Equip the Copper Armor.
   - *Trigger:* Khi mặc Copper Armor cho nhân vật trong `DialogSelectEquipment.java`, step tăng 5 -> 6.
6. **Step 6:** Recruit a second adventurer.
   - *Trigger:* Khi quay lại Tavern chiêu mộ người thứ 2, step tăng 6 -> 7 (kết thúc tutorial).
   
*(Ở các bước 1, 6 và 7, cơ chế Gacha của Tavern bị hardcode để luôn ra các Hero cố định phục vụ Tutorial: Footman, Light Disciple, Archer).*

---

## 2. Unity Current State

**Unity currently has tutorial system:** PARTIAL (Chỉ có phần khung dữ liệu và cơ chế Gacha).

Trong project Unity Rebuild hiện tại:
- **SaveData:** Đã có trường `TutorialStep` trong `SaveData.cs`.
- **TavernService:** Đã được port logic hardcode hero cho tutorial. Tại `TavernService.cs` (line 127), game tự động roll ra `footman` (Step 1), `light_disciple` (Step 6), và `archer` (Step 7).
- **UI:** Chưa có bất kỳ UI Panel nào để hiển thị câu nhắc nhở tutorial.
- **Triggers:** Các trigger liên quan đến Dungeon (đánh rớt đồ), Workshop (craft đồ) và Equipment (mặc đồ) chưa được nối với biến `TutorialStep`.

---

## 3. Mapping Table & Missing Pieces

| Tính năng | Legacy Java | Unity hiện tại | Trạng thái |
|---|---|---|---|
| Tutorial step | Lưu 6 bước | Chỉ dùng để ép roll Gacha | Partial |
| Tutorial save | `Data.java -> tutorialStep` | `SaveData.cs -> TutorialStep` | Done |
| Popup system | `containerTutorial` trên `MainActivity` | Không có | Missing |
| Highlight system | Không có (chỉ là text nhắc nhở) | Không có | Done (Do bản gốc không có) |
| Trigger: Recruit 1st Hero | `DialogTavern.java` | Không có | Missing |
| Trigger: Dungeon Loot | `Area.java` | Không có | Missing |
| Trigger: Craft Leather | `DialogWorkshop.java` | Không có | Missing |
| Trigger: Craft Armor | `DialogWorkshop.java` | Không có | Missing |
| Trigger: Equip Armor | `DialogSelectEquipment.java`| Không có | Missing |

---

## 4. Phân tích sự khác biệt & Đề xuất kế hoạch triển khai

### Phân tích:
- Tutorial của bản Legacy quá cơ bản (chỉ là một khung Text UI chiếm diện tích trên màn hình).
- Theo tiêu chuẩn game mobile hiện đại (Idle/Gacha), Onboarding cần sử dụng hệ thống **UI Masking / Focus Highlight** (Làm tối màn hình và khoét lỗ sáng ở nút bấm cần nhấn) để ngăn người chơi bấm lung tung gây kẹt state.
- Việc port lại nguyên si cái bảng Text của Legacy sẽ làm giảm giá trị UX/UI của bản Unity Rebuild.

### Đề xuất (Recommended Implementation Plan):
1. **Không port UI Tutorial kiểu cũ:** Không tạo `containerTutorial` giống Legacy.
2. **Tạm thời giữ nguyên cơ chế Gacha:** Vẫn giữ nguyên logic ép roll `TutorialStep` trong `TavernService.cs` để test data Phase 1 & 2.
3. **Chờ hoàn thiện UI / Gameplay Phase 2 & 3:** Trước khi release, chúng ta nên triển khai một "Modern Tutorial Framework" mới trong Unity. Framework này sẽ:
   - Dùng chung biến `SaveData.TutorialStep`.
   - Có một Overlay Canvas (SortOrder cao nhất) để che toàn bộ nút bấm sai.
   - Bắn event hoàn thành step ngay tại các UseCase/Service (thay vì nhúng logic vào UI như bản cũ).

---

## 5. Kết luận

- **Tutorial system exists in Legacy:** YES (Text-based persistent prompt).
- **Unity currently has tutorial system:** NO (Only backend SaveData & Tavern forced-roll logic exists; no UI and no step progression triggers).
- **Implementation recommendation:** Tạm hoãn. Không nên port UI Tutorial cũ. Nên đợi các hệ thống cốt lõi hoàn thành và xây dựng một UI Masking / Onboarding Framework hiện đại hơn cho Unity sau này.
