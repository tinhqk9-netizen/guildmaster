---
author: Antigravity
date: 2026-08-06
target: D:\Tinh\Rebuild_GuildMaster
module: Equipment System
status: AUDIT_COMPLETE
---

# 🛡️ EQUIPMENT SYSTEM AUDIT

> **Mục tiêu:** Audit toàn bộ hệ thống Equipment từ source Java gốc → equipment data → EquipmentDefinition/ItemDefinition → EquipmentService/CombatService → Character stats → save/load.
> **Trạng thái:** Hoàn tất Discovery & Audit. Tìm thấy lỗi mất data nghiêm trọng ở parser và sai lệch logic chiến đấu ở CombatService.

---

## 1. Equipment Inventory & Definition Completeness

### 1.1 Khác biệt cấu trúc cơ sở
* **Legacy Java:** Equipment là các class kế thừa (e.g. `Weapon` > `Sword` > `CopperSword`). Cấu trúc chứa hàng chục field Transient như `constitution`, `lifesteal`, `threat`, `criticalChance`, v.v. Không có damage cơ bản (Base Damage) vì vũ khí chỉ là vật phẩm cộng Stats.
* **Rebuild C#:** Sử dụng duy nhất `ItemDefinition.cs` chung cho mọi Item (bao gồm cả Weapon, Armor, Consumable). `DatabaseBuilder.cs` tự động dịch `parentClass` từ file JSON sang `ItemCategory` (Weapon, Armor) và `ItemType` (Sword, MediumArmor).

### 1.2 Lỗi Parser / DTO "Bỏ rơi" dữ liệu (Data Loss)
Parser xuất ra `items.json` nhưng **hoàn toàn bỏ qua các chỉ số phụ của trang bị**.
Nguyên nhân gốc rễ nằm ở C#: `ItemFieldsLoader.cs` chỉ định nghĩa 6 base stats cơ bản:
```csharp
private class ItemFieldsDto {
    public FieldValueDto constitution;
    public FieldValueDto dexterity;
    public FieldValueDto intelligence;
    public FieldValueDto defense;
    public FieldValueDto magicDefense;
    public FieldValueDto maxHp;
}
```
**🚨 MẤT HOÀN TOÀN CÁC FIELD SAU:**
* `lifesteal`, `lifestealWithMinion`
* `threat` (Rất quan trọng cho Tanker, hiện tại Tanker không có Threat từ trang bị)
* `bonusExperience`, `darknessReduction`, `counterattack`
* `criticalChance`, `criticalDamage`, `flatDodgeChance` (Các cơ chế né tránh/chí mạng bị tê liệt)
* `healingModifier`, `immunityToStatus`, `regeneration`, `initiative`

---

## 2. Lỗi Logic Tính Toán Sát Thương (Damage Calculation)

Đây là lỗi nghiêm trọng nhất làm thay đổi hoàn toàn Meta của game gốc.

### 2.1 Legacy Java (Source of Truth)
Trang bị không có thuộc tính "Base Damage". Sát thương cơ bản của một đòn đánh là **Chỉ số Stat của Tướng (Hero)**.
Vũ khí chỉ làm nhiệm vụ **chỉ định loại Stat nào sẽ được dùng để scale sát thương**, thông qua hàm `getDamageModifier`:
* `Sword.java`: Trả về `i` (Constitution / Strength) -> Sát thương tỉ lệ 100% với Constitution.
* `Staff.java`: Trả về `i3` (Intelligence) -> Sát thương tỉ lệ 100% với Intelligence.
* Độ lệch sát thương ngẫu nhiên (Damage Delta) được định nghĩa trong `Weapon.java` là `+/- 15%`.

### 2.2 Rebuild C# (CombatService.cs)
Developer C# đã **hallucinate (tự bịa ra)** một công thức tính sát thương hoàn toàn mới, hardcode thẳng vào `CombatService.cs`:
```csharp
switch (parentClass.ToLowerInvariant())
{
    case "sword":
        mod = con * 1.2 + dex * 0.4; // SAI HOÀN TOÀN (Legacy chỉ scale 1.0 CON)
        break;
    case "staff":
        mod = intel * 1.5; // SAI HOÀN TOÀN (Legacy chỉ scale 1.0 INT)
        break;
    case "dagger":
        mod = dex * 1.2 + con * 0.3; // SAI HOÀN TOÀN
        break;
    case "bow":
        mod = dex * 1.5; // SAI HOÀN TOÀN
        break;
}
```
**Hậu quả:** 
Sát thương của toàn bộ Game trong bản Rebuild đang bị thổi phồng (bơm thêm 20-50% damage) và scale sai chỉ số phụ.

---

## 3. Hệ Thống Armor & Defense

* **Cộng dồn Stats (CharacterService.cs):** Trang bị cộng trực tiếp vào `baseStat` trước khi nhân với `traitMult`. Logic này khá tương đồng với Legacy (không nhân `mult` của Ascension cho hệ thống Defense/MDEF).
* **Equipment Restrictions (EquipmentService.cs):** Khá tốt. Check đúng điều kiện `character.Definition.WeaponType` vs `item.Definition.ItemType`. Mapper trong `DatabaseBuilder.cs` hoạt động chính xác để đảm bảo Mage không thể cầm Sword.

---

## 4. Tổng Hợp Exact Counts / Lỗi

* **Orphan/Missing Logic:** Mọi hiệu ứng đặc biệt trên vũ khí (VD: Lifesteal của Vampire Sword) hoàn toàn vô tác dụng trong Rebuild do bị `ItemFieldsLoader` drop dữ liệu.
* **Hardcoded Logic:** `CombatService.cs` chứa magic numbers cho Weapons multipliers.

## 5. Kết Luận
1. **Thiếu Data nghiêm trọng:** `ItemDefinition` và `ItemFieldsLoader` cần được mở rộng để parse tất cả các Combat Modifiers còn thiếu.
2. **Sai Meta Game:** Xóa bỏ công thức tính damage tự chế trong `CombatService.cs` và khôi phục lại rule scale sát thương 100% theo Stat của Legacy.
