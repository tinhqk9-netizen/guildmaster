---
author: Antigravity
date: 2026-08-06
target: D:\Tinh\Rebuild_GuildMaster
module: Pet System
status: AUDIT_COMPLETE
---

# 🐾 PET SYSTEM AUDIT

> **Mục tiêu:** Audit toàn bộ hệ thống Pets từ source Java gốc → pet definitions → acquisition/hatching → assignment → combat effects → merge/evolution → save/load.
> **Trạng thái:** Hoàn tất. Hệ thống Pet trong bản C# Rebuild là một hệ thống **hoàn toàn tự biên dịch (hallucinated)**, đi ngược lại 100% thiết kế gốc của Legacy từ Data, Assignment, cho đến Combat.

---

## 1. Pet Inventory (Legacy Java)

Cấu trúc Pet trong Java gồm 7 Abstract Classes tương ứng với 7 hệ, mỗi hệ có 3 loại Pet (Common 75%, Uncommon 20%, Rare 5%).
**Tổng số Pet Definitions:** 21

**Danh sách chính xác (21 Pets):**
1. **Avian:** `Dove`, `Owl`, `Eagle`
2. **Construct:** `Rockling`, `Golem`, `Tesseract`
3. **Esoteric:** `FloatingEye`, `TentacleTangle`, `ThingFromTheAbyss`
4. **Insect:** `Mosquito`, `Beetle`, `Tarantula`
5. **Reptile:** `Lizard`, `TreeFrog`, `Crocodile`
6. **Wild:** `Rat`, `Squirrel`, `RedWolf`
7. **Wooden:** `FloatingSeed`, `WalkingBush`, `HolyTree`

Không có duplicate, orphan hay placeholder.

---

## 2. Pet Data & Parser Failure (Critical Data Loss)

Parser của dự án đã **thất bại hoàn toàn** trong việc trích xuất dữ liệu Pet.
* **`pets.json` hiện tại:** Chỉ chứa `className` và `id`.
* **Dữ liệu bị mất:** `printPetType`, `guaranteedFirstAbility`, `idName`, `idImage`, `abilityNumber`.

**Hậu quả (Hallucinated C# Model):**
Vì không có data, dev C# đã tự tạo ra class `PetDefinition.cs` với các field: `BaseAttack`, `BaseDefense`, `BaseMaxHp`, `BaseSpeed`, `EvolutionDefinitionId`.
👉 **ĐÂY LÀ ĐỒ GIẢ.** Trong game gốc (Java), Pet KHÔNG HỀ có HP, Attack hay Defense. Pet không trực tiếp đánh như một Hero, mà đóng vai trò là một "Aura Buff/Modifier" cho Party.

---

## 3. Acquisition & Hatching

Trứng (Egg) là item sinh ra Pet. Cấu trúc roll hoàn toàn fix cứng trong source (VD: `AvianEgg.java`, `WildEgg.java`).
* **Tỉ lệ chuẩn:** 75% ra Pet loại 1 (Common), 20% ra Pet loại 2 (Uncommon), 5% ra Pet loại 3 (Rare).
* **Code mapping:** Nằm tại `storage/data/items/instances/*Egg.java`.

---

## 4. Assignment & Party Integration (Critical Architecture Mismatch)

* **Legacy Java:** Pet **ĐƯỢC GẮN VÀO DUNGEON (Area)**.
  * Code gốc: `area.getPetExploringId()` trong `Area.java`.
  * Một Dungeon Raid chỉ mang được 1 Pet, và Pet này buff cho *toàn bộ party* trong Dungeon đó.
* **C# Rebuild:** Pet **ĐƯỢC GẮN VÀO HERO**.
  * Code hiện tại: `EquipToCharacter(pet, character)` trong `PetService.cs`.
  * Điều này phá vỡ hoàn toàn cơ chế cốt lõi của game gốc, biến Pet từ "Raid Aura" thành "Trang bị cá nhân" (Personal Equipment).

---

## 5. Combat Integration (13 Pet Abilities)

Trong Java, khả năng chiến đấu của Pet đến từ 13 Abilities (unlock ở level 1, 20, 40, 60) và được xử lý trực tiếp trong vòng lặp combat của `Area.java`.
Ở bản C#, **0/13 Abilities được implement**. Thay vào đó `PetService.cs` cộng dồn các chỉ số "BaseAttack", "BaseHp" giả mạo vào Hero.

| Ability | Legacy Behavior (`Area.java`) | C# Runtime Support |
|---|---|---|
| **FIGHTER** | `petAttack()` deal damage độc lập = `(level*0.5) + 1` | ❌ Missing |
| **HEALER** | `petHeal()` hồi máu cho party = `(level*0.2) + 1` | ❌ Missing |
| **DECOY** | `getDecoy()` % chance hút đòn đánh của Enemy vào khoảng không. | ❌ Missing |
| **OPPORTUNIST** | Kết liễu enemy nếu HP% < `getOpportunist()` | ❌ Missing |
| **MAGIC** | Kháng/gây Status Effect dựa trên tỉ lệ và turns. | ❌ Missing |
| **SAVAGE** | % chance bồi thêm sát thương khi hero bạo kích. | ❌ Missing |
| **BRIGHT** | Tăng Threat/Priority cho mục tiêu. | ❌ Missing |
| **EXPERIENCE** | `getExperience()` buff % EXP cho toàn bộ party. | ❌ Missing |
| **DROPS** | Extra loot roll chance khi dọn quái. | ❌ Missing |
| **COUNTERATTACK**| `getCounterattack()` buff % Counterattack cho party. | ❌ Missing |
| **LIFESTEAL** | `getLifesteal()` buff % Hút máu. | ❌ Missing |
| **REGENERATION** | `getRegeneration()` cộng thẳng HP regen mỗi turn. | ❌ Missing |
| **BARRIER** | `applyDamage(..., pet.getBarrier(), ...)` trừ thẳng sát thương nhận vào. | ❌ Missing |

---

## 6. Merge / Evolution (Hallucinated Mechanics)

* **C# Rebuild:** Tự bịa ra hệ thống "Tiến hóa" (`EvolutionDefinitionId`, `EvolutionLevel`) giống hệt game Pokemon.
* **Legacy Java:** **Không hề có Tiến hóa.** Việc "Merge" trong `DialogMergePet.java` đơn giản chỉ là XÓA 1 Pet (sacrifice) và chuyển đổi tổng lượng Food/Kinh nghiệm của nó (`totalFoodToNextLevel()`) sang cho 1 Pet khác (feed). Nó là một chức năng dọn rác kho Pet để cày cấp cho Pet chính, không có yêu cầu "Cùng hệ" hay "Cùng tier" gì cả.

---

## 7. Save/Load Integrity

* **Nguy cơ lỗi Save rất cao:** Nếu load một file Save từ Legacy (trên Android), hệ thống sẽ báo lỗi vì bản Java lưu Pet ID vào bên trong `Area` (Dungeon). Trong khi bản C# lại cố gắng đọc `EquippedToCharacterId` nằm trong `PetSaveData`. 
* Dữ liệu Pet từ JSON hiện tại quá thiếu sót, khiến cho hệ thống Pet trong C# trở thành một Module "trắng", chạy hoàn toàn trên code bịa (Hallucinated).

---
**Tổng kết:** Module Pet cần đập đi xây lại 100% data parser và kiến trúc gán (Assignment), chuyển từ "Equip to Hero" sang "Assign to Area".
