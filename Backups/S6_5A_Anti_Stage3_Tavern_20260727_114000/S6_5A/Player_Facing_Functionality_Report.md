# Player-Facing Functionality Report

**Ngày:** 2026-07-25
**Góc nhìn:** Người chơi mở bản Unity hiện tại làm được những gì. Đánh giá dựa trên user Play test thật (2 lần) + đọc code + Editor.log.

## Bảng chức năng người chơi

| Player Action / Feature | Current User Experience | Backend Exists | Uses Real Data | Can Mutate Game State | Save Works | Status | Needed To Become Complete |
|---|---|---|---|---|---|---|---|
| **Mở game (Boot → Main)** | Bấm Play ở `Boot.unity` → tự chuyển `Main.unity` → HUD hiện | ✅ | ✅ load 1531 record | — | — | **Functional** | (Đã xong cho Editor) Cần test build Standalone thật |
| **Xem HUD money** | Thấy số `0` cạnh icon vàng | ✅ `SaveService.Money` | ✅ giá trị save thật | ❌ | ✅ | **Read-only functional** | Cần gameplay sinh/tiêu tiền (loot, bán, mua, nâng cấp) |
| **Xem HUD gems** | Thấy số `0` cạnh icon gem | ✅ `SaveService.Gems` | ✅ | ❌ | ✅ | **Read-only functional** | Cần nguồn gem (quest reward / IAP / achievement) |
| **Mở Inventory** | Panel hiện chữ `"Inventory is empty."` | ✅ `InventoryService` (add/remove/stack/capacity/consume đầy đủ) | ✅ đọc `GetAllItems()` thật | ❌ **không có nút nào** | ✅ (`SyncToSave` khi có thay đổi) | **Read-only functional** | Cần: item grid + icon, filter theo category, chi tiết item, nút **equip/use/sell**, hiện capacity (`GetCapacity()` đã có sẵn nhưng không hiện). **Và cần nguồn sinh item** — hiện không có cách nào để có item |
| **Mở Character** | Panel hiện chữ `"No characters available."` | ✅ `CharacterService` (`CreateCharacter`/`GetTotalStat`/`GainExperience`/`LevelUp`) | ✅ đọc `GetAllCharacters()` thật | ❌ | ✅ | **Read-only functional** | Cần: **luồng tạo/tuyển nhân vật** (Tavern — chưa port), hiện chỉ số qua `GetTotalStat()`, equipment slot, skill, level/exp bar. ⚠️ `GetTotalStat()` đang **hardcode levelMultiplier = 1.0f** → số sẽ **sai** so với decode |
| **Mở Dungeon** | Panel trắng + chữ `"Dungeon UI is not implemented yet."` + nút Back | ⚠️ `DungeonService` 202 dòng tồn tại nhưng **chưa khởi tạo runtime** | ❌ không đọc data nào ra UI | ❌ | ❌ | **Placeholder visual only** | Cần **toàn bộ**: list 11 dungeon, chọn party, start run, tick progress, combat turn, enemy display, loot result, clear count. Phụ thuộc: `DungeonService`+`CombatService`+`LootService`+`EnemyService`+`SkillService`+`StatusEffectService` (cả 6 chưa wire) + **orchestrator chưa tồn tại** + **công thức damage chưa port** |
| **Mở Craft** | Panel trắng + chữ `"Craft UI is not implemented yet."` + Back | ⚠️ `CraftService` tồn tại nhưng `CanCraft()` **trả `ManualRuleRequired`** | ❌ | ❌ | ❌ | **Placeholder visual only** | Cần: list 321 recipe, hiện ingredient đủ/thiếu, nút craft, workshop queue + timer, nút claim. ⚠️ **Rule timer/claim chưa xác nhận từ decode** |
| **Mở Merchant** | Panel trắng + chữ `"Merchant UI is not implemented yet."` + Back | ⚠️ `MerchantService` tồn tại nhưng `BuyItem()` **trả `DeferredPriceOrCurrencyRule`** | ❌ | ❌ | ❌ | **Placeholder visual only** | Cần: list hàng bán + giá, nút buy/sell, market listing, timer restock. ⚠️ **Rule giá/restock chưa xác nhận** |
| **Mở Settings** | Panel trắng + chữ `"Settings UI is not implemented yet."` + Back | ❌ không có service | ❌ | ❌ | ⚠️ `SaveService.Save()`/`DeleteSave()` **có sẵn nhưng không UI nào gọi** | **Placeholder visual only** | Cần: nút Save thủ công, nút Delete save, hiện version. ⚠️ Decode có **9 setting field**, Unity `SaveData` **chưa có setting nào** → cần thêm field trước |
| **Bấm Back từ mọi màn** | Về màn trước (HUD) | ✅ `UIService.Back()` + stack | — | ❌ | — | **Functional** | (Đã xong) |
| **Save khi thoát** | Bấm Stop Play → `save.json` được ghi | ✅ `OnApplicationQuit`/`OnApplicationPause` | ✅ | ⚠️ ghi state **nhưng state luôn mặc định** | ✅ 942 bytes, JSON hợp lệ | **Functional** (nhưng chưa test được nội dung thay đổi) | Cần một gameplay action thật mutate data → save → load lại → verify giá trị đổi. **Hiện chưa test được điều này** vì không có action nào |
| **Xem quest** | ❌ **không có màn quest nào** | ⚠️ `QuestService` tồn tại, chưa wire | ❌ (56 quest trong JSON không hiện ra đâu) | ❌ | ⚠️ có `QuestSaveData` | **Backend exists but not exposed** | Cần: `QuestScreen` + nút nav thứ 7, list quest, progress bar, nút claim reward. Decode `QuestsManager` 468 dòng chưa port phần roll/refresh |
| **Xem/dùng pet** | ❌ **không có gì** | ❌ không có `PetService` | ❌ (21 pet trong JSON không dùng) | ❌ | ❌ | **Not implemented** | Cần port `Pet.java` (290 dòng) + `PetAbility` + shelter/autofeed + pet tham chiến combat |
| **Chọn doctrine** | ❌ **không có gì** | ❌ **không tồn tại trong Unity** | ❌ | ❌ | ❌ | **Not implemented** | Decode có 8 doctrine + 16 save field + damage modifier + `canPickDoctrine()`. **Chưa port bất kỳ phần nào** |
| **Tuyển adventurer (Tavern)** | ❌ **không có gì** | ❌ | ❌ (129 adventurer trong JSON không dùng được) | ❌ | ❌ | **Not implemented** | Đây là **nút thắt then chốt**: không có Tavern → không có nhân vật → Character screen luôn rỗng → không thể vào dungeon → không có loot → Inventory luôn rỗng → không có tiền. **Cả chuỗi gameplay bị chặn từ đây** |
| **Nâng cấp HQ (Quarters/Storage/Workshop/Market/Shelter)** | ❌ **không có gì** | ⚠️ 3/10 formula giá có trong `FormulaService` | ❌ | ❌ | ⚠️ `SaveData` có `LevelStorage`/`UpgradeStorage`/`LevelWorkshopTime`/`LevelMarketTime` | **Backend exists but not exposed** | Cần UI nâng cấp + 14 formula còn thiếu |
| **Raid** | ❌ không có gì | ❌ | ❌ (12 raid trong JSON không dùng) | ❌ | ❌ | **Not implemented** | Decode có `places/raids/` + `ui/raids/` |
| **Xem thành tích / mua IAP** | ❌ không có gì | ❌ | ❌ | ❌ | ❌ | **Not implemented** | `AchievementsUtils.java`/`IAPWrapper.java` — **cần user xác nhận có thuộc scope rebuild hay không** |

## Tổng kết theo status

| Status | Số lượng | Danh sách |
|---|---|---|
| **Functional** | 3 | Mở game (Boot→Main), Back, Save on quit |
| **Read-only functional** | 4 | HUD money, HUD gems, Inventory, Character |
| **Placeholder visual only** | 4 | **Dungeon, Craft, Merchant, Settings** |
| **Backend exists but not exposed** | 2 | Quest, HQ upgrade |
| **Not implemented** | 5 | Pet, Doctrine, Tavern, Raid, Achievements/IAP |

## 🔴 Phát hiện quan trọng nhất: chuỗi gameplay bị chặn ngay từ đầu

Người chơi hiện **không thể thực hiện bất kỳ hành động nào làm thay đổi trạng thái game**. Không phải vì thiếu asset, mà vì thiếu chức năng:

```
Không có Tavern  →  không có adventurer
                 →  Character screen luôn rỗng
                 →  không thể lập party vào dungeon
                 →  không có combat  →  không có loot
                 →  Inventory luôn rỗng  →  không có gì để craft/bán
                 →  Money/Gems luôn = 0
                 →  save.json luôn chứa toàn giá trị mặc định
```

Đây là lý do `save.json` sinh ra chỉ có `Money=0`, `Gems=0`, mọi list rỗng — **đúng, không fake**, nhưng đồng thời chứng minh **chưa có vòng gameplay nào hoạt động**.

**Việc mở được 8 panel KHÔNG đồng nghĩa game có chức năng.** Đúng như user nhấn mạnh: 4 màn Dungeon/Craft/Merchant/Settings hiện là `Placeholder visual only`, **không được tính là DONE**.
