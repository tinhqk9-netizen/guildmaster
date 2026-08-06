# 🧠 Báo cáo Audit: Skills System & Combat Integration

**Dự án**: D:\Tinh\Rebuild_GuildMaster
**Ngày Audit**: 2026-08-06

---

## 1. Inventory Skill Đầy Đủ
Kiểm kê file `Skills.java` trong source gốc và `skills.json` hiện tại.

### 🔴 Thống kê tổng quan
- **Tổng số Skill (Java):** 227
- **Active Skills:** 101 (Bao gồm `ACTIVE_NONE`)
- **Passive Skills:** 126 (Bao gồm `PASSIVE_NONE`)
- **Tổng số Skill (JSON):** 227 (Khớp số lượng Enum).

**Nhận xét:** Source code gốc không dùng Database hay JSON để định nghĩa Skill. Skill hoàn toàn chỉ là một file Enum `Skills.java` dùng để map tên và mô tả (`R.string.name`, `R.string.description`).

---

## 2. Skill Behavior Trong Source Gốc
Vì `Skills.java` chỉ là Enum rỗng, toàn bộ logic của Kỹ năng (Active và Passive) được **Hardcode cứng** trong source Java.

### 🔎 Active Skills (Kỹ năng chủ động)
- **Vị trí logic:** Nằm trong 1 khối `switch (entity.getActiveSkill())` siêu khổng lồ (kéo dài hàng nghìn dòng) bên trong hàm `processTurn` của `Area.java` (bắt đầu từ dòng 1368).
- **Behavior:** Mỗi `case` tự tính toán logic riêng:
  - **Heal:** `ACTIVE_HEAL`, `ACTIVE_MASS_HEAL` gọi hàm `entity.setCurrentHp(...)`.
  - **Damage:** `ACTIVE_MIGHTY_STRIKE`, `ACTIVE_METEOR_I` gọi `applyDamage(...)` kèm status effect.
- **Mana/Cost:** Mana tự hồi mỗi turn. Khi đạt 100 Mana, skill Active sẽ được tung ra và reset Mana về 0.

### 🔎 Passive Skills (Kỹ năng bị động)
- **Vị trí logic:** Văng tung tóe khắp source code (`Area.java`, `Entity.java`, `Combat.java`).
- **Behavior:** Bất kỳ chỗ nào cần check, code Java lại gọi `if (entity.getPassiveSkill() == Skills.PASSIVE_...)`:
  - Dodge logic: Kiểm tra `PASSIVE_CHAOTIC` hay `PASSIVE_PREHISTORIC_AVIAN` để tăng né.
  - Hồi máu: Kiểm tra `PASSIVE_REGENERATION_I` mỗi turn.
  - Kháng: Kiểm tra `PASSIVE_IMMUNITY`.

---

## 3. Data & Model Hiện Tại (Rebuild)

### 🔴 Tình trạng `skills.json`
- Trình parse bắt được đủ 227 ID, nhưng thất bại trong việc tách biến (`parseStatus: "partial"`, lý do `UNPARSED_ARGS`).
- Các giá trị `nameKey` và `descriptionKey` bị gom thành một chuỗi thô: `"rawArgs": "R.string.active_annihilate_name, R.string.active_annihilate_description"`.

### 🔴 Tình trạng Model (`SkillDefinition.cs`)
- File `SkillDefinition.cs` có `NameKey` và `DescriptionKey` nhưng chúng luôn `null` do JSON không map được.
- Có comment của dev trước: `// manualRuleRequired: Cooldown, Cost, Level, TargetRule, DamageFormula // deferredToS3Combat`. Điều này chứng tỏ họ đã nhận ra logic Skill gốc không có thông số tĩnh, mà phải tái tạo toàn bộ bằng tay.

### 🔴 Trạng thái liên kết (References)
- 129 Hero và 122 Monster **ĐỀU TRỐNG TRƠN** `ActiveSkillId` và `PassiveSkillId` vì file parse `adventurers.json` và `enemies.json` đã bỏ sót Enum `Skills.*`.
- Toàn bộ 227 Skill trong `skills.json` hiện đang là **Orphan (mồ côi)**, không có ai sử dụng.

---

## 4. Combat Integration (`CombatService.cs`)

Hệ thống Combat hiện tại trong C# **KHÔNG HỀ CHẠY SKILL NÀO CẢ**.

- **Active Skill rơi vào hư vô:** 
  Hàm `ProcessTurn` có check `if (!string.IsNullOrEmpty(acting.ActiveSkillId))` để hồi Mana. Do `ActiveSkillId` luôn là `null`, Hero và Monster vĩnh viễn có 0 Mana. Kể cả khi có Mana, `CombatService.cs` cũng **chưa hề có code** để tung skill.
- **Tất cả thành Basic Attack:** Mọi unit đều rơi thẳng xuống cuối hàm `ProcessTurn` và gọi `RollAttackDamage()` (Đánh thường).
- **Passive Skill biến mất:** Không có bất kỳ lệnh `if` nào trong `CombatService.cs` hay `AdventurerWrapper`/`EnemyWrapper` kiểm tra `PassiveSkillId`. Mọi cơ chế tăng né, tăng giáp, hút máu bị động đều không tồn tại.

---

## 5. Bảng Reference Integrity (Mẫu đại diện 10 Kỹ năng)

| Skill ID | Phân loại | Logic Source Gốc | Tình trạng Runtime | Liên kết (Hero/Quái) |
|---|---|---|---|---|
| `ACTIVE_MIGHTY_STRIKE` | Active | Hardcode `Area.java: switch` (Damage x1.5) | Mất `Name/Desc` (rawArgs) | Trống (`null`) |
| `ACTIVE_HEAL` | Active | Hardcode `Area.java: switch` (Hồi HP đồng minh) | Mất `Name/Desc` (rawArgs) | Trống (`null`) |
| `ACTIVE_METEOR_I` | Active | Hardcode `Area.java: switch` (AoE Damage) | Mất `Name/Desc` (rawArgs) | Trống (`null`) |
| `ACTIVE_TAUNT_I` | Active | Hardcode `Area.java: switch` (Tăng Threat) | Mất `Name/Desc` (rawArgs) | Trống (`null`) |
| `ACTIVE_PANDEMONIUM`| Active | Hardcode `Area.java: switch` (Boss Clovis) | Mất `Name/Desc` (rawArgs) | Trống (`null`) |
| `PASSIVE_THREATENING_I`| Passive | Hardcode `Area.java` (Tăng Threat) | Mất `Name/Desc` (rawArgs) | Trống (`null`) |
| `PASSIVE_IMMUNITY` | Passive | Hardcode `Area.java` (Kháng 100% Status) | Mất `Name/Desc` (rawArgs) | Trống (`null`) |
| `PASSIVE_REGENERATION_I`| Passive | Hardcode `Area.java` (Hồi HP mỗi turn) | Mất `Name/Desc` (rawArgs) | Trống (`null`) |
| `PASSIVE_CHAOTIC` | Passive | Hardcode `Entity.java` (Tăng tỷ lệ Né) | Mất `Name/Desc` (rawArgs) | Trống (`null`) |
| `PASSIVE_LICH_CURSE` | Passive | Hardcode `Area.java` (Cast độc khi chết) | Mất `Name/Desc` (rawArgs) | Trống (`null`) |

---

## 💡 Tổng kết
Hệ thống Skill gốc không phải là hệ thống Data-driven mà là Hardcode-driven. 
- JSON `skills.json` thực chất vô dụng vì nó không chứa logic.
- Khúc xương khó nhằn nhất của toàn bộ dự án này không nằm ở việc parse JSON, mà là **phải thiết kế lại toàn bộ kiến trúc Skill trong C# (Skill System / Scriptable Objects / Components)** để chuyển đổi 227 cục Hardcode trong `Area.java` thành dữ liệu xài được cho `CombatService.cs`.
