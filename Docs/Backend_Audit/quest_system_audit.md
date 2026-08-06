# Quest System Audit

**Dự án:** `D:\Tinh\Rebuild_GuildMaster`
**Legacy source:** `D:\Tinh\Guild Master - Idle Dungeons`
**Trạng thái:** Deep Audit Only (Không fix code, Không sửa data)

---

## 1. Quest Inventory

### Legacy Java
- Nguồn: `QuestsManager.java`, `Quest.java`
- Quest được tạo thông qua `Quest.createInstance()` thay vì load từ JSON.
- Tổng cộng có **36 quests** được define (ví dụ: `medic`, `fallingApart`, `criticalHit`, `masterCrafter`, `paleontologist`, v.v.).
- Quest không chia theo Daily/Weekly mà chia theo **Reward Type (Pool)**:
  - **Kings Quests**: Các quest thưởng Gems (luôn lấy ngẫu nhiên 5 quest từ pool chung).
  - **Doctrine Quests**: Các quest thưởng Doctrine XP. Số lượng và pool phụ thuộc vào số lượng Adventurer đang theo Doctrine nào trong Guild (Affliction, Control, Fortitude, Grace, Illusion, Knowledge, Ruin, War).

### Rebuild C#
- Nguồn: `quest_metadata.json`, `QuestDefinition.cs`, `QuestService.cs`
- Thay vì 36 quests, game chỉ có một pool tĩnh hardcode `SAFE_GENERAL_QUESTS_POOL` chứa **13 quest** cơ bản (ví dụ: `annihilator`, `hit_or_miss`, `medic`, `warrior`, v.v.).
- Thiếu định nghĩa cho 23 quest chuyên biệt (crafting, raids, dungeon đặc biệt).

---

## 2. Generation & Pool Architecture

### Legacy Java
- Hàm `QuestsManager.extractQuests()` chạy hàng tuần (dựa vào `Utils.tick24Hours` và `calendar.add(7, ...)`).
- **Cơ chế phân bổ (Distribution):**
  - Luôn sinh ra 5 `KingsQuests`.
  - Sinh ra `DoctrineQuests` tương ứng với class của Adventurer. Ví dụ: Nếu có 3 con theo hệ `War`, sẽ sinh ra 3 quests lấy từ `accessibleWarQuests`.
- Rarity của Quest được roll động (`rollRarity()`) ảnh hưởng đến target progress và phần thưởng.

### Rebuild C#
- Hàm `CheckAndTriggerWeeklyQuests()` bắt chước logic 7 ngày (604800 giây).
- **Cơ chế phân bổ hoàn toàn bị hỏng:**
  - Game chỉ sinh đúng 1 nhóm **5 quests** (gọi chung là Weekly Quests) lấy từ `SAFE_GENERAL_QUESTS_POOL`.
  - Hoàn toàn mất đi cơ chế sinh quest theo Doctrine của Adventurer. C# Rebuild không hề chia list Quest theo Doctrine.

---

## 3. Progression Triggers (Tiến độ Quest)

### Legacy Java
- Tiến độ được cập nhật qua `QuestsManager.increment()` và `incrementToValue()` rải rác khắp mọi hệ thống trong game:
  - **Combat / Area:** Update `medic`, `slowBurn`, `criticalHit`, `pulverization`, `theEnd`...
  - **Dungeon / Raid Specific:** Update `thalassophobia` khi đánh ở BlackwaterPort, `andStayDead` khi raid AncientGraveDigging.
  - **Workshop:** Update `masterCrafter` khi craft đồ.
  - **Merchant / Storage:** Update `paleontologist` khi bán item.

### Rebuild C#
- `QuestService.IncrementDefinition()` chỉ được gọi trong `DungeonService.cs` để track một số quest combat cơ bản (giết quái, chết, đi xa).
- **Mất hoàn toàn tiến độ của:** Crafting (`masterCrafter`), Market/Trading (`paleontologist`), Raids, và đa số Dungeons đặc biệt. Người chơi vĩnh viễn không thể hoàn thành các quest này nếu chúng lỡ được sinh ra.

---

## 4. Rewards & Claim Logic

### Legacy Java
- **Kings Quests:** Khi claim, luôn cộng **GEMS**.
- **Doctrine Quests:** Khi claim, lấy List ID tương ứng và cộng trực tiếp **Doctrine Progress/Level** cho Doctrine đó (ví dụ: Hoàn thành quest trong list `afflictionQuests` sẽ cộng sao cho Affliction Doctrine). Cấp độ Doctrine được tính toán trực tiếp từ XP (Stars).
- Lượng reward scale theo Rarity của Quest.

### Rebuild C# (Massive Hallucination)
- Logic Claim (`QuestService.ClaimReward`) đã bị hallucinate nghiêm trọng.
- Thay vì dựa vào Pool của Quest, C# code tự nhận định: **Nếu Rarity >= 4 thì cho Gems, ngược lại cho Doctrine XP**. Điều này hoàn toàn sai so với bản gốc.
- **UI Hallucination (`QuestScreen.cs`):** Do hệ thống không lưu Pool của quest, giao diện UI cho phép người chơi tự chọn Doctrine để cộng XP!
- Tệ hơn nữa, UI cho phép chọn giữa mảng hardcode `["war", "economy", "growth"]`. `economy` và `growth` là hai doctrine hoàn toàn **không tồn tại** trong game gốc cũng như trong `DoctrineService` của C#, dẫn đến việc nếu người chơi chọn, Doctrine XP sẽ bốc hơi vào hư vô.

---

## 5. Major Differences & Hallucinations (Tổng kết)
1. **Mất hệ thống Pool theo Doctrine:** Quests không còn liên kết với hệ thống Doctrine của Adventurer.
2. **Missing Metadata:** Chỉ support 13/36 quests.
3. **Broken Triggers:** Quests liên quan tới Crafting và Bán đồ không bao giờ tăng tiến độ vì thiếu hooks trong `CraftService` và `MerchantService`.
4. **Reward Hallucination:** Logic thưởng Gems vs Doctrine XP bịa đặt hoàn toàn dựa trên Rarity, phá vỡ logic gốc.
5. **UI Hallucination:** Tự bịa ra doctrine `economy` và `growth` trên màn hình chọn thưởng Quest.
