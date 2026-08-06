---
author: Antigravity
date: 2026-08-06
target: D:\Tinh\Rebuild_GuildMaster
module: Loot / Rewards
status: AUDIT_COMPLETE
---

# 🎁 LOOT / REWARDS AUDIT

> **Mục tiêu:** Audit toàn bộ pipeline Loot/Rewards từ source Java gốc → enemy drops/search-room rewards/dungeon completion → item instances → inventory/storage → offline rewards → save/load.
> **Trạng thái:** Hoàn tất. Hệ thống bị mất dữ liệu Drop Weights cực nặng ở khâu Parser, và bị thiếu hụt Logic (Search Room, Pet Drops, Offline Combat) ở C# Runtime khiến kinh tế của game (Game Economy) đang bị tê liệt một phần.

---

## 1. Reward Source Inventory

* **Nguồn tạo Reward trong Legacy:**
  * **Enemy Drops:** Nguồn chính, dựa trên Weighted Map.
  * **Search-room (Empty Room):** Rớt materials (`Quartz`, `Sandstone`, v.v.) và triggers (Traps, Heal).
  * **Offline Exploration:** Mô phỏng tick combat (tối đa 12 giờ) để tạo drop thật.
  * **Pet Bonus:** Roll thêm 1 lần item drop.
  * **Quests:** Trả thưởng cố định.
* **Tình trạng hiện tại ở C# Rebuild:** Enemy drops chạy được nhưng data sai. Search-room bị xóa. Pet bonus bị xóa. Offline Exploration cho combat bị xóa.

---

## 2. Enemy Drop Definitions & Parser Loss

* **Legacy Java (`Area.java` & `Enemy.java`):**
  Hàm `public LinkedHashMap<ItemWrapper, Integer> listDrops(int i)` chứa một map các món đồ và **Weight** của chúng (tổng < 1000). 
  *VD `Wurm.java`: WurmScale (300), WurmBlood (300), InsectEgg (1).*
* **JSON Output (`enemies.json`):**
  Parser đã **THẤT BẠI HOÀN TOÀN** trong việc parse Dictionary Drop.
  *VD ở Wurm:* Nó chỉ lấy được đúng 1 key-value: `"Drops": { "wurm_scale": 1 }, "DropStacks": { "wurm_scale": 1 }`. Mất sạch Weight, mất sạch các Item phụ.
* **C# Runtime (`EnemyDropTableLoader.cs`):**
  Loader của C# đọc raw JSON block này để biến thành `DropTableEntry`. Hậu quả: Quái vật C# hiện tại **chỉ rớt đúng 1 loại item** và weight luôn bị set thành số lượng stack (hoặc value ngẫu nhiên bị parse nhầm).

---

## 3. Search-room & Event Rewards (Missing Logic)

* **Legacy Java (`searchRoom()`):**
  Khi đi vào phòng không có quái, game gọi `searchRoom()`. Dựa trên RNG 1000:
  * `< 10.0`: Nhặt `Quartz`.
  * `< 35.0`: Nhặt `Sandstone`.
  * `< 85.0`: Bị sập bẫy Silence / Mất HP.
  * `< 100.0`: Hồi HP/Mana cho cả Party.
* **C# Runtime (`DungeonService.cs` - Case 4):**
  Case 4 (SEARCH_ROOM) **KHÔNG CHỨA BẤT KỲ LOGIC NÀO** ngoài việc tăng `dungeon.Progress++`. Toàn bộ material drop từ việc dò đường và các bẫy đều đã bị bốc hơi.

---

## 4. Runtime Reward Generation

* **Roll Drop Logic (`LootService.RollSingleDrop`):**
  * Đúng: C# tái tạo chính xác hàm `Utils.rollFromWeightedMap()` bằng `DecodeMath.RollFromWeightedMap()` (scale 1000). Nếu tổng weight < 1000, có tỉ lệ quái không rớt gì (miss gap).
  * Sai (Thiếu hụt Pet): Game gốc có check `petExploring.getDrops()`, nếu Pet pass RNG, sẽ roll thêm 1 lần nữa để lấy double drop. C# vứt bỏ hoàn toàn logic Pet drop trong `RunLoot()`.
* **Storage / Inventory Full:** C# sử dụng `_lootService.IsChestFull(dungeon.PendingDrops, merchantPack)` xử lý chuẩn theo game gốc (Nếu chest đầy, drop bị hủy bỏ trước khi add).

---

## 5. EXP and Progression Rewards

* **Legacy EXP Split:**
  1. Tổng EXP của quái (`expGiven`) chia đều cho số Hero còn sống.
  2. Nhân với `experienceMultiplier()` của từng Hero (Dựa trên Traits như *Fast Learner*).
  3. Nhân với Pet EXP Bonus.
* **C# Rebuild (`DungeonService.cs` + `CharacterService.GainExperience`):**
  1. C# chia đều EXP cho số Hero còn sống (Đúng).
  2. **Bỏ qua hoàn toàn** Multiplier từ Trait và Pet (Sai). Hero có trait học nhanh không nhận thêm được chút EXP nào.
  3. C# không tích hợp quest progress (như nhiệm vụ `Fast Learner`, `Student`) vào vòng lặp EXP.

---

## 6. Offline Rewards (DEFERRED IN C#)

* **Legacy Offline:**
  Hàm `initializeThreads()` trong `MainActivity.java` tính toán thời gian Offline (`jMax` up to 12 tiếng). Sau đó chạy vòng lặp `while(j2 < j) { it3.next().tick(); }`. Tức là game mô phỏng combat thật trong nền hàng chục ngàn tick để tạo ra drop y như người chơi đang bật máy.
* **C# Rebuild (`OfflineProgressService.cs`):**
  Dev C# đã comment rõ:
  `// Dungeon tick deferred: No safe background dungeon loop implemented yet.`
  `// Combat / Quest offline logic deferred.`
  Hiện tại, C# **KHÔNG CÓ TÍNH NĂNG OFFLINE COMBAT REWARD**. Offline chỉ tăng thời gian cho Crafting (Workshop) và Market.

---

## 7. Reference Integrity Summary

| Reward Source | Legacy rule | Current data | Runtime roll | Status |
|---|---|---|---|---|
| Enemy Drops | Weighted Map (<1000) | ❌ Hỏng (Chỉ 1 item, ko weight) | `RollSingleDrop` | ❌ Parser Loss |
| Empty Room / Search | Material drops, Traps | ❌ Mất sạch | Chết logic | ❌ Missing Runtime |
| Boss Specific Drops | `event.getKey()` roll | ❌ Mất sạch | Chết logic | ❌ Missing Runtime |
| Pet Bonus Drops | Double Roll (Chance) | N/A | Chết logic | ❌ Missing Runtime |
| EXP Split | Base * Trait * Pet | N/A | Chỉ tính Base | ❌ Missing Runtime |
| Offline Combat | Simulate Combat Ticks | N/A | Bỏ qua (Deferred) | ❌ Missing Runtime |

## Tổng kết & Vấn đề lớn nhất

Hệ thống kinh tế (Loot) hiện tại trong C# **gần như không hoạt động đúng mục đích thiết kế**:
1. Lỗi Parser làm `enemies.json` mất toàn bộ Weight và Multi-drops của quái vật. Đánh Wurm chỉ rớt đúng 1 món.
2. Các nguyên liệu cơ bản rớt từ việc đi bộ trong Dungeon (Search Room) không thể kiếm được trong C#.
3. Tính năng cốt lõi "Idle" (Offline Combat Drops) hiện đang bị Deferred hoàn toàn trong code, game tắt máy là party đứng im.
