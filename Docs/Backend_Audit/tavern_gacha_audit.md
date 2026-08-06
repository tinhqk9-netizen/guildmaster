# Tavern / Gacha / Recruitment System Audit

**Dự án:** `D:\Tinh\Rebuild_GuildMaster`
**Legacy source:** `D:\Tinh\Guild Master - Idle Dungeons`
**Trạng thái:** Deep Audit Only (Không fix code, Không sửa data)

---

## 1. Hero Recruitment Inventory

### Legacy Java
- Logic sinh Hero (Visitor) nằm trong `Utils.java:newTavernVisitor()`.
- **Hero pool:** 4 class cơ bản (Footman, Rogue, Archer, Apprentice).
- Tỷ lệ xuất hiện: 25% chia đều.

### Rebuild C#
- Logic nằm trong `TavernService.cs:GenerateVisitor()`.
- Tỷ lệ class được port chính xác (chia đều 25%).

---

## 2. Gacha / Roll Logic

### Legacy Java & Rebuild C#
- Không có hệ thống pity, không có rate up, hoàn toàn random 25% cho 4 class.
- Không tốn Gold/Gem để roll. Hero tự động đến Tavern dựa trên thời gian thực (Visitor Interval).

---

## 3. Tavern Visitor Generation

- **Visitor Interval (Thời gian chờ):** Tính toán chính xác theo công thức giảm dần dựa trên `UpgradeTavernTime` (giảm 10% mỗi level).
- **Giới hạn khách (Capacity):** Khách mới đến sẽ đẩy khách cũ nhất ra khỏi quán nếu quá giới hạn. Logic này được port khá sát gốc.

---

## 4. Trait / Skill Integration

### Legacy Java
- **Đa đặc điểm (Multi-Traits):** Khi tạo Hero mới, hàm gọi cả `rollCommonTrait()` (40% cơ hội) và `rollRareTrait()` (10% cơ hội). Một Hero hoàn toàn có thể sở hữu **đồng thời 2 Trait** (1 Common, 1 Rare) trong file save thông qua biến `traitCommon` và `traitRare`.
- **Trang bị khởi điểm:** Tự động gán vũ khí mặc định vào slot của Hero thông qua `adventurer.setWeapon()`.

### Rebuild C# (CRITICAL REGRESSIONS)
1. **Lỗi giới hạn Trait (Single Trait Cap):**
   - File `CharacterSaveData.cs` chỉ khai báo ĐÚNG MỘT biến `Trait` (string).
   - Logic `TavernService.cs` gán:
     ```csharp
     trait = RollCommonTrait();
     if (trait == null) trait = RollRareTrait();
     ```
   - Hậu quả: Hero C# chỉ có thể sở hữu tối đa 1 Trait. Mất đi hoàn toàn tỷ lệ xuất hiện Hero xịn (có cả Common và Rare trait). Lỗi này ảnh hưởng lớn tới core progression.

2. **Lỗi lạm phát đồ đạc (Inventory Flood & Item Dupe):**
   - Khi `GenerateVisitor()` tạo khách ghé quán, nó tạo một `ItemRuntime` vũ khí khởi điểm và dùng lệnh `_inventoryService.AddItem(weaponItem)` để **thêm thẳng vào hòm đồ của người chơi** trước cả khi họ Recruit Hero.
   - Hậu quả: Khách tự đến rồi tự đi, nhưng mỗi lần đến để lại 1 vũ khí rác trong rương của người chơi. Rương sẽ bị đầy tràn (flood) vô hạn bằng Starter Weapons, làm hỏng toàn bộ Economy và Storage.

---

## 5. Hero Recruitment Result

- **Legacy Java:** Tạo instance `Adventurer` đầy đủ, gán vào đội hình.
- **Rebuild C#:** Sử dụng `CharacterService.RecruitCharacter(guestData)` chuyển từ `TavernGuests` sang mảng `Characters`. Các chỉ số baseStats được tính toán đủ. Tuy nhiên bị mất một Trait như đã nói ở phần 4.

---

## 6. Tavern Upgrade System

- **Tavern Capacity Upgrade:** 
  - Formula Java: `Math.pow(3.0d, level) * 5000`
  - Formula C#: Trùng khớp.
- **Tavern Time Upgrade:** 
  - Formula Java: `Math.pow(1.7d, level) * 200`
  - Formula C#: Trùng khớp.
- **Quarters (Số lượng Hero tối đa):** Port đúng công thức.

---

## 7. Economy Integration

- Nâng cấp Tavern tiêu tốn Gold chuẩn xác. Recruitment không tốn Gold (nguyên bản Legacy cũng miễn phí).
- Lỗi Item Dupe làm sập Economy do người chơi có thể bán vô số Starter Weapon kiếm tiền (như đã nói ở mục 4).

---

## 8. Save / Load Integrity

- **Missing Data:**
  - `CharacterSaveData` không hỗ trợ mảng hoặc 2 trường Trait riêng biệt. Khi load file save Legacy, Hero cũ bị mất Trait thứ hai (thường là Rare trait).

---

## 9. UI Audit

- UI C# (`TavernScreen.cs`) thể hiện đủ 2 tab (Tavern & Quarters).
- Các nút Upgrade hoạt động, giá tiền hiển thị chuẩn xác.
- Do backend giới hạn 1 Trait nên UI cũng chỉ render 1 Trait.

---

## 10. Reference Integrity Table

| Feature | Legacy Behavior | Current C# | Status |
|---|---|---|---|
| Multiple Traits | Roll độc lập Common và Rare (có thể ra 2) | Chỉ lấy 1 Trait duy nhất (Common ưu tiên) | 🔴 Wrong Logic |
| Trait Save Data | Có `traitCommon` và `traitRare` | Gộp chung thành 1 string `Trait` | 🔴 Missing Data |
| Starter Weapon | Gán vào người Hero (không vào hòm) | Bị ném thẳng vào Inventory của người chơi | 🔴 Hallucinated Feature / Bug |
| Upgrade Formulas | Theo cấp số nhân Base * (multiplier ^ lvl) | Port chính xác | 🟢 OK |
| Visitor Generation| Tạo Footman, Rogue, Archer, Apprentice | Tỷ lệ và logic roll class chính xác | 🟢 OK |
