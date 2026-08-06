# 🧠 Báo cáo Audit: Monster Definitions & Combat Pipeline

**Dự án**: D:\Tinh\Rebuild_GuildMaster
**Ngày Audit**: 2026-08-06

---

## 1. Definition Completeness (Kiểm kê dữ liệu)
So sánh giữa file source Java (`Abomination.java`, `KasimirTheSeer.java`, v.v.) và file parse `enemies.json`.

### 🔴 Thống kê tổng quan
- **Tổng số Enemy (JSON):** 122

### 🔴 Số lượng Field bị mất (trên tổng 122 Enemy)
- `nameKey` / `idName`: Thiếu 122 / 122
- `idDescription`: Thiếu 122 / 122
- `imageId` (Portrait): Thiếu 122 / 122
- `passiveSkill`: Thiếu 122 / 122
- `activeSkill`: Thiếu 122 / 122
- `immunityToStatus` (Kháng hiệu ứng): Thiếu 122 / 122
- `onTargetHit` (Hiệu ứng đòn đánh): Thiếu 122 / 122
- `BaseMaxHp`, `Rarity`, `ExpGiven`, `Damage`: Thiếu 2 / 122 (Bao gồm `EmperorClovisXXVIII` bị lỗi parse và class base `Enemy` bị lọt vào JSON).
- `Drops` / `DropStacks`: Lấy thành công 116 / 122 (Phần lớn thành công, các quái thiếu là do list drop rỗng bẩm sinh như `KasimirTheSeer` hoặc do lỗi parse).

**Nhận xét:** Khác với Hero, dữ liệu Monster lấy được mảng `Drops` và `BaseStats` rất tốt. Nhưng toàn bộ Kỹ năng, Kháng hiệu ứng và Hiệu ứng đòn đánh đã bốc hơi.

---

## 2. Parser Root Causes
Trình parse tạo ra `enemies.json` có flag `EXTRACTED_FROM_DEX_S6_5A`. Đây là tool chạy reflection/dump data từ file .dex, không phải Regex parse text thông thường.

### 🔎 Phân tích nguyên nhân
- **Dump thành công Drop:** Tool đã gọi thành công hàm `listDrops(int)` của từng Enemy để lấy danh sách rớt đồ (Item, Stack).
- **Lọc bỏ Object phức tạp:** Tool dex-dump đã CỐ TÌNH bỏ qua (hoặc lỗi serialize) các field tham chiếu đến Class/Enum khác. Do đó, `Skills.*`, `R.drawable.*`, và Object `StatusEffect` của `onTargetHit` bị ném đi.
- **Crash nghiêm trọng (EmperorClovisXXVIII):** Tool dump đã crash khi chạy hàm `getMinDamage()` của Emperor Clovis vì hàm này return `Logger.BOTCHED_OFFERING;`. Do môi trường mock dex không khởi tạo `Logger`, exception văng ra khiến JSON của Clovis bị rỗng hoàn toàn (`MISSING_STATS`, `MANUAL_RULE_REQUIRED`).
- **Rác Base Class:** Class trừu tượng `Enemy.java` cũng bị dump vào JSON thành ID `"enemy"`.

---

## 3. Model & Loader Integrity
Audit file `EnemyDefinition.cs` và luồng load:

- `EnemyDefinition.cs` **ĐÃ QUÊN** khai báo biến cho `immunityToStatus` và `onTargetHit`. Ngay cả khi JSON có data, model cũng không có lỗ để cắm vào.
- Các field `nameKey`, `ActiveSkillId`, `PassiveSkillId` có khai báo trong C#, nhưng JSON trả về `null` nên runtime cũng `null`.
- Cấu trúc `EnemyDropEntry` hoạt động đúng.

---

## 4. Reference Integrity & Dependency
- **Orphan/Placeholder:** `EmperorClovisXXVIII` là placeholder rỗng với 0 HP, 0 Damage. Nếu Dungeon gọi ra con boss này, người chơi sẽ thắng ngay lập tức và không nhận được Drop nào. Tương tự với ID rác `"enemy"`.
- **UI:** Không có `imageId` hay `nameKey`, giao diện combat sẽ hiển thị quái không có hình ảnh hoặc lấy fallback tồi tệ.
- **Skills/Status Effects:** Không có reference sang các hệ thống này. Quái vật bị cắt đứt hoàn toàn khỏi cây Kỹ năng.
- **Dungeons:** `DungeonDefinition.cs` lưu `EnemyIds` dạng chuỗi. Dungeon hoàn toàn có thể vô tình spam ra con boss rỗng Clovis.

---

## 5. Runtime / Combat Impact
Tác động cực kỳ nghiêm trọng đến `CombatService.cs`:

### 🔴 1. Quái vật chỉ là "Bao Cát" (HP Sponges)
- Hàm `ProcessTurn` trong `CombatService.cs` có check `!string.IsNullOrEmpty(acting.ActiveSkillId)` để hồi Mana.
- Vì tất cả quái vật không có `ActiveSkillId`, chúng **không bao giờ có Mana** và **không bao giờ dùng kỹ năng**.
- `EnemyWrapper` hardcode `Threat => 1`, `RollsDamageThreeTimes => false`, `IsInitiative => false`. Mọi cơ chế Boss (đánh 3 lần, lấy turn đầu, đe dọa cao) bị dập tắt, ép mọi quái vật thành lính quèn.

### 🔴 2. Không có Kháng Hiệu Ứng (Stunlock)
- Biến `immunityToStatus` (VD: KasimirTheSeer kháng 50% stun) không tồn tại trong C#.
- Hậu quả: Người chơi có thể stun-lock (khóa trói) Boss vĩnh viễn mà Boss không có cách nào chống cự.

### 🔴 3. Mất Hiệu ứng đòn đánh (On-Hit)
- Con `Abomination` ở bản gốc có hiệu ứng đấm choáng (`StatusEffectType.STUN`). Do parser xóa field này, đòn đánh của nó giờ hoàn toàn vô hại.

### 🔴 4. Mất Giáp Chuyên Biệt (Flat Reduction)
- Trong `EnemyWrapper.FlatDamageReduction`, lập trình viên đã viết comment thừa nhận: `[PARTIAL] LegateHadrian (+15) and TheExiled (+40) enemy-specific overrides missing`. Boss bị phế đi lớp giáp cứng.

---

## 💡 Đề xuất Fix (Chưa thực hiện)

Đã ghi chú bên dưới (không nằm trong nội dung Markdown này).
