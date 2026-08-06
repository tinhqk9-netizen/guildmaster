# 🧠 Báo cáo Audit: Hero Definitions & Character Pipeline

**Dự án**: D:\Tinh\Rebuild_GuildMaster
**Ngày Audit**: 2026-08-06

---

## 1. Definition Completeness (Kiểm kê dữ liệu)
So sánh giữa file source Java (`Adept.java`, `Adventurer.java`, v.v.) và file parse `adventurers.json`.

### 🔴 Thống kê
- **Tổng số Hero (Java):** 129
- **Tổng số Hero (JSON):** 129 (Khớp số lượng ID).

### 🔴 Số lượng Field bị mất khi parse (trên tổng 129 Hero)
- `nameKey` / `idName`: Thiếu 129 / 129
- `idDescription`: Thiếu 129 / 129
- `imageId` (Portrait): Thiếu 129 / 129
- `passiveSkill`: Thiếu 129 / 129
- `activeSkill`: Thiếu 129 / 129
- `weaponType`: Thiếu 129 / 129
- `armorType`: Thiếu 129 / 129
- `nextClasses` (Promotions): Thiếu 129 / 129
- `potionDrinkerType`: Thiếu 129 / 129
- `MaxLevel`: Thiếu 128 / 129

**Kết luận:** Trình parse CHỈ lấy được nhóm `BaseStats` (HP, Dex, Int, v.v.). Toàn bộ linh hồn của Hero (kỹ năng, chức nghiệp, hình ảnh) đã bốc hơi 100%.

---

## 2. Parser Root Causes
Trình parse Python (`legacy_import_tool`) dường như chỉ sử dụng một bộ Regex rất thô sơ để quét các dòng `this.base* = *`.

### 🔎 Phân tích lỗi Parser
- **Bỏ qua Resource ID:** Parser không hiểu các hằng số Java `R.string.*` và `R.drawable.*`, nên nó cắt bỏ hoàn toàn `nameKey`, `idDescription`, và `imageId`.
- **Bỏ qua Enum/Static:** Các dòng gán skill như `this.activeSkill = Skills.ACTIVE_ENERGY_BURST_II;` bị lờ đi vì chứa dấu chấm `Skills.*`. Tương tự với `PotionDrinkerType.*`.
- **Bỏ qua List Add:** Dòng code xác định cây tiến cấp (Promotion Tree) sử dụng cú pháp `this.nextClasses.add("FireWizard");`. Parser không bắt được pattern `.add()`, dẫn đến việc danh sách `NextClasses` của tất cả Hero đều rỗng `[]`.

---

## 3. Model & Loader Integrity
Dữ liệu JSON bị hỏng được chuyển thẳng vào C# mà không có cảnh báo.

### 🔎 C# Definitions (`AdventurerDefinition.cs`)
File này ĐÃ ĐƯỢC THIẾT KẾ ĐÚNG. Nó có đầy đủ các biến `NextClasses`, `PassiveSkill`, `ActiveSkill`, v.v. Nhưng vì JSON trả về không có các key này, `JsonUtility` của Unity tự động gán chúng bằng `null`.

### 🔎 Hardcode "Băng cá nhân" trong Loader (`DatabaseBuilder.cs`)
Thay vì sửa code parse Python, lập trình viên trước đây đã dùng một cách "chữa cháy" cực kỳ tệ trong `DatabaseBuilder.cs`:
- Viết hàm `EnrichAdventurerDefinition()` dài hơn 100 dòng chứa `switch-case` khổng lồ để hardcode lại thủ công `WeaponType` và `ArmorType` cho cả 129 hero.
- Tuy nhiên, họ LỜ ĐI hoàn toàn các field khó như `NextClasses` hay `Skills`. Các field này vẫn bị bỏ hoang vĩnh viễn là `null`.

---

## 4. Runtime / Save Mapping
- `CharacterRuntime.cs` có chứa property `ActiveSkillId` và `PassiveSkillId`. Nhưng vì Definition là null, runtime cũng null. Hero spawn ra không có bất kỳ kỹ năng nào.
- `Trait` (Đặc điểm hiếm/thường): Bị gán thành `string.Empty` trong `SaveData.cs`. Trong game legacy, Trait được random lúc roll tướng trong Tavern, hệ thống random này hiện đang mất tích.

---

## 5. Reference Integrity & Gameplay Impact
Toàn bộ gameplay cốt lõi của hệ thống Hero đang sụp đổ dây chuyền do hậu quả của file JSON thiếu dữ liệu:

### 🔴 1. Hệ thống Kỹ năng (Combat Skills) - Tê Liệt
Tất cả 129/129 Hero **KHÔNG CÓ KỸ NĂNG**. Vào combat, Hero chỉ có thể đánh thường (Basic Attack) vì `ActiveSkillId` và `PassiveSkillId` đều trống.

### 🔴 2. Hệ thống Tiến cấp (Promotion) - Chết hoàn toàn
Trong code C#, `PromotionService.cs` đang gọi `_database.GetAll<PromotionDefinition>()` để lấy danh sách tiến cấp.
**Nhưng sự thật là:**
- Database KHÔNG HỀ CÓ file `promotions.json`. (Legacy game không xài file này).
- Trong Legacy, cây tiến cấp (Promotion Tree) được xác định bằng list `NextClasses` nằm ngay bên trong từng Hero (Vd: `Adept` -> `FireWizard`).
- Do `NextClasses` bị parse hụt thành rỗng, hệ thống Promotion trong bản Unity hiện tại không có dữ liệu để chạy, 100% dead feature.

### 🔴 3. UI Roster & Detail - Trắng xoá
Hero không có ID ảnh chân dung (`imageId`), không có tên hiển thị localized, không có mô tả class. UI Roster sẽ hiển thị placeholder lỗi hoặc trống không.

---

