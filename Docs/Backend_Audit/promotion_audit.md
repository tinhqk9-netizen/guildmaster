---
author: Antigravity
date: 2026-08-06
target: D:\Tinh\Rebuild_GuildMaster
module: Promotion System
status: AUDIT_COMPLETE
---

# 🛡️ PROMOTION SYSTEM AUDIT

> **Mục tiêu:** Audit toàn bộ hệ thống Promotion/Class Advancement từ source Java gốc → Hero definitions → promotion requirements → PromotionService → stat/skill/equipment changes → save/load.
> **Trạng thái:** Hoàn tất Discovery & Audit. Tìm thấy lỗi mất dữ liệu (data loss) cực lớn, dẫn đến việc hệ thống Promotion trong bản C# Rebuild bị đứt gãy hoàn toàn và thay bằng một hệ thống "fake" không hề giống với bản gốc.

---

## 1. Promotion Tree Inventory (Legacy Java)

Cây Promotion trong game gốc (Java) là một cấu trúc phân nhánh khổng lồ, được code cứng trong hàm `configureStatistics()` của từng class (e.g. `this.nextClasses.add("LightDisciple");`).

**Thống kê chính xác từ source gốc:**
* **Tổng số Class (Hero units):** 116
* **Class có nhánh Promotion:** 95
* **Tổng số cạnh (Promotion edges):** 107
* **Terminal classes (Class cuối, không thể promote):** 21
* **Vấn đề tương thích trang bị:** Đã check toàn bộ 107 nhánh promotion, **KHÔNG CÓ NHÁNH NÀO** thay đổi `weaponType` hay `armorType` khi thăng cấp. (VD: Apprentice cầm Staff thăng lên LightDisciple vẫn dùng Staff).

---

## 2. Lỗi Data Parser Cắt Mất Toàn Bộ Cây Thăng Cấp (Data Loss)

Parser xuất ra `adventurers.json` đã bỏ sót 2 trường dữ liệu cốt lõi nhất để cấu thành hệ thống Promotion:
1. `maxLevel` (Yêu cầu cấp độ để thăng cấp)
2. `nextClasses` (Danh sách các class có thể thăng cấp lên)

**Hậu quả:** 
* Field `NextClasses` trong `AdventurerDefinition.cs` (C#) luôn luôn rỗng (`[]`).
* Field `MaxLevel` trong `AdventurerDefinition.cs` bị biến mất.

---

## 3. Lỗi Hallucinated C# Logic (PromotionService.cs)

Vì bị mất hoàn toàn dữ liệu về Cây thăng cấp, dev C# đã "tự thiết kế" ra một hệ thống Promotion hoàn toàn khác biệt, phá vỡ hoàn toàn cấu trúc class của game gốc.

### 3.1 Legacy Java (Source of Truth)
* **Requirement:** Chạm mốc `maxLevel` của class hiện tại (VD: level 5, 15, 30). KHÔNG CÓ yêu cầu vàng, vật phẩm hay dungeon.
* **Mutation (Result):** Đổi thẳng Class của nhân vật (gọi hàm `Adventurer.getInstance(newClass)`). Class thay đổi dẫn đến Base Stats, Active Skill, Passive Skill thay đổi theo class mới. Level reset về 1. Trang bị được giữ nguyên.

### 3.2 Rebuild C# (PromotionService.cs)
* Tạo ra `PromotionDefinition.cs` và `PromotionService.cs` mô phỏng một hệ thống "Tier / Ascension" tuyến tính.
* **Fake Requirements:** Kiểm tra level và **đòi hỏi Item (RequiredItemId, RequiredItemCount)** (Tự bịa ra, bản gốc không hề có).
* **Fake Mutation:** Hàm `Promote` trong `PromotionService.cs` chỉ tăng biến `character.AscensionLevel++` và `character.IsAscended = true`, level reset về 1.
* **🚨 NGHIÊM TRỌNG:** Nhân vật **KHÔNG HỀ CHUYỂN CLASS**. Một Apprentice khi "promote" trong C# chỉ trở thành một "Apprentice Tier 2" (AscensionLevel 1), base stats và skill bị kẹt vĩnh viễn ở cấp độ Apprentice. Toàn bộ 116 class của game gốc trở nên vô nghĩa!

---

## 4. Hiện Trạng C# Database & Save/Load

* **Ghost Definition:** Class `PromotionDefinition.cs` tồn tại nhưng không hề được load bởi `DatabaseBuilder.cs`. Không có file `promotions.json`. Do đó, lệnh `_database.GetAll<PromotionDefinition>()` trong `PromotionService.cs` luôn trả về 0. 
* Hệ thống Promotion trong Rebuild C# hiện tại hoàn toàn **Chết (Dead Code)**, người chơi không thể thăng cấp được dưới bất kỳ hình thức nào.
* **Save/Load:** `CharacterSaveData.cs` hiện đang lưu `AscensionLevel` thay vì lưu sự thay đổi về `DefinitionId` như trong bản gốc.

---

## 5. Danh Sách 15 Promotion Edge Mẫu (Legacy)

Dưới đây là một số nhánh thăng cấp mẫu được extract thẳng từ source Java (`NextClasses`):
1. `Apprentice` -> `LightDisciple`, `Adept` (maxLevel: 5)
2. `Adept` -> `Mage`, `Cleric`
3. `Cleric` -> `Priest`, `Cultist`
4. `Rogue` -> `Cutthroat`, `Thief`
5. `Archer` -> `Marksman`, `Huntress`
6. `Footman` -> `Guard`, `Warrior`
7. `Guard` -> `Knight`, `IronDefender`
8. `Warrior` -> `Mercenary`, `Gladiator`
9. `Mage` -> `RedMage`, `WhiteMage`, `Necromancer`
10. `Priest` -> `Bishop`, `Inquisitor`
11. `Thief` -> `Assassin`, `ShadowCrawler`
12. `Huntress` -> `Beastmaster`, `Tempest`
13. `Knight` -> `Paladin`, `DarkKnight`
14. `RedMage` -> `RedArchmage`, `Pyromancer`
15. `WhiteMage` -> `WhiteArchmage`, `Oracle`

## Kết Luận
* Cây Promotion 116 class bị mất dữ liệu hoàn toàn.
* Logic thăng cấp trong C# là đồ giả (Hallucinated/Placeholder), không hoạt động và đi ngược hoàn toàn với kiến trúc của Legacy.
