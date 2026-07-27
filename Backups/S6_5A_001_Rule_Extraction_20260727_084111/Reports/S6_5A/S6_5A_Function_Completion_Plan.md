# S6.5A Function Completion Plan

**Ngày:** 2026-07-25
**Nguyên tắc:** Hoàn thiện **chức năng** trước, **không** làm đẹp trước, **không** generate asset trước. Thiếu asset → placeholder (ô vuông xám / text label / button trơn). Thiếu rule từ decode → `ManualRuleRequired`, **không implement bừa**.

## Thứ tự phụ thuộc (quan trọng)

```
S6.5A-001 Rule Extraction (bóc rule còn thiếu)
      │
      ├─→ S6.5A-002 Formula + SaveData Schema Completion
      │         │
      │         ├─→ S6.5A-003 Runtime Service Wiring
      │         │         │
      │         │         ├─→ S6.5A-004 Tavern/Quarters (NÚT THẮT — mở chuỗi gameplay)
      │         │         │         │
      │         │         │         ├─→ S6.5A-005 Character/Equipment/Skill
      │         │         │         ├─→ S6.5A-006 Inventory Actions
      │         │         │         └─→ S6.5A-007 Dungeon/Combat/Loot ← nặng nhất
      │         │         │                   │
      │         │         │                   └─→ S6.5A-008 Quest
      │         │         ├─→ S6.5A-009 Craft (chờ rule)
      │         │         ├─→ S6.5A-010 Merchant (chờ rule)
      │         │         └─→ S6.5A-011 Settings (làm được ngay)
      │         └─→ S6.5A-012 Offline Progress + Save Mutation Verify
      └─→ S6.5A-013 Function Regression & Review
```

## Bảng task

| Proposed Task | Domains Covered | Goal | Uses Existing Backend? | Needs Decode Rule? | Asset Requirement | Risk | Exit Criteria |
|---|---|---|---|---|---|---|---|
| **S6.5A-001** Rule Extraction | Damage formula, character stat multiplier, craft timer/claim, merchant price/restock, doctrine | Bóc chính xác 5 rule đang thiếu từ decode: `Area.dealDamage()` (dòng 2213), `Adventurer.calculate*`/level multiplier, `Utils.maxCraftableAmount`+`progressWorkshopTime`+`Item.getSecondsToCraft`, `Utils.truncatePrice`+`rollPotion`/`rollSpecialFoods`/`rollUpgrades`+`progressMarketTime`, `Doctrine.java`+8 instance. **Chỉ đọc + viết tài liệu rule, không code** | — (đọc decode) | ✅ **Đây chính là task bóc rule** | No Asset Needed | **Trung bình** — rule combat phức tạp, dễ hiểu sai | Có tài liệu `Rule_Extraction_Report.md` ghi công thức chính xác kèm số dòng nguồn cho từng rule; rule nào vẫn không đọc được thì đánh `MANUAL_RULE_REQUIRED` rõ ràng |
| **S6.5A-002** Formula + SaveData Schema Completion | Formulas, SaveData | Port **14 formula còn thiếu** (rule có sẵn nguyên văn trong `Formulas.java`); thêm field `SaveData` cần thiết: `lastAccess`, 4 mốc thời gian, 9 setting, các cặp level/upgrade Tavern/Quarters/Shelter, 8 doctrine level/progress, thống kê | ✅ mở rộng `FormulaService`/`SaveData` | ⚠️ Chỉ doctrine cần rule từ 001 | No Asset Needed | **Thấp** — port 1:1, có test đối chiếu số | 20/20 formula có test so khớp giá trị với `Formulas.java`; `SaveData` mở rộng nhưng **save cũ vẫn load được** (migration path); 0 lỗi CS |
| **S6.5A-003** Runtime Service Wiring | 11 service chưa wire + dọn Bootstrapper trùng | Khởi tạo đủ `EquipmentService`, `EnemyService`, `SkillService`, `StatusEffectService`, `DungeonService`, `CombatService`, `LootService`, `QuestService`, `CraftService`, `MerchantService`, `OfflineProgressService` trong 1 composition root duy nhất; gỡ/khai tử 2 `Bootstrapper` dead code | ✅ dùng nguyên constructor có sẵn | ❌ | No Asset Needed | **Thấp–Trung bình** — nhiều dependency, thứ tự khởi tạo | Log runtime in ra đủ 14/14 service khởi tạo thành công; 0 exception; Boot→Main vẫn chạy; chỉ còn 1 composition root |
| **S6.5A-004** Tavern / Quarters Function | Tavern, Quarters, Adventurer recruit | **Nút thắt then chốt** — có nguồn tạo adventurer thì cả chuỗi gameplay mới mở. Port `newTavernVisitor()`, visitor interval, tavern/quarters capacity; UI list visitor + nút Recruit; ghi vào `SaveData.Characters` | ⚠️ cần `CharacterService.CreateCharacter()` (đã có) + formula mới từ 002 | ❌ (rule nằm trong `Formulas.java`+`Utils.java`, đọc được) | **Placeholder OK** (list text + button) | **Trung bình** | Người chơi bấm Recruit → có adventurer thật trong Character screen → save ghi lại → load lại vẫn còn |
| **S6.5A-005** Character / Equipment / Skill Function | Character, Equipment, Skill, StatusEffect | Sửa `GetTotalStat()` dùng **level multiplier thật** (từ 001); UI stat panel, equipment slot (equip/unequip), skill list; giải quyết 3 TODO của `EquipmentService` | ✅ `CharacterService`+`EquipmentService` có sẵn | ✅ level multiplier + rule equip-remove-from-inventory | **Placeholder OK** (ô vuông xám cho slot) | **Trung bình** | Equip/unequip đổi chỉ số thật; save ghi đúng; test round-trip pass |
| **S6.5A-006** Inventory Actions | Inventory, Item | Đổi từ read-only sang có action: filter theo category, chi tiết item, nút use/sell/equip, hiện capacity (`GetCapacity()` đã có) | ✅ `InventoryService` đầy đủ | ❌ | **Placeholder OK** (ô vuông xám thay icon) | **Thấp** | Người chơi thao tác được item; số lượng đổi; save ghi lại |
| **S6.5A-007** Dungeon / Combat / Loot Function | Dungeon, Combat, Loot, Enemy, Skill, StatusEffect, Pet | **Task nặng nhất.** Viết dungeon-run orchestrator (tương đương `Area.tick()`); port `dealDamage`, `dodge`, `retaliate`, `heal`, `cast`, `resolveStatus`, `selectTargets` (7 strategy), `decideTurnsOrder`, `collectExperience`, `loot`, `incrementProgress`, unlock chain; UI chọn dungeon → party → start → xem tiến trình/combat log → nhận loot | ⚠️ `CombatService` mới 139/3164 dòng → phải mở rộng nhiều | ✅ **damage formula + crit + dodge + loot roll** (từ 001) | **Placeholder OK** (text log + ô vuông xám cho enemy) | **CAO** — đây là chỗ dễ "fake" nhất, phải cẩn thận nhất | Chạy được 1 dungeon run hoàn chỉnh: enemy chết, exp tăng, loot vào inventory, progress tăng, save ghi lại. **Damage số phải khớp công thức decode**, có test so khớp |
| **S6.5A-008** Quest Function | Quest, Doctrine | Port `extractQuests`, `calculateDifficulty`, `rollRarity`, `setupAccessibleQuests`, `realignQuests`; thêm `QuestScreen` + nav button thứ 7; nút claim reward | ⚠️ `QuestService` 87/468 dòng | ✅ doctrine amount rule (từ 001) | **Placeholder OK** | **Trung bình–Cao** | Quest hiện ra, progress tăng theo hành động thật, claim reward vào save |
| **S6.5A-009** Craft Function | Craft, Workshop, Recipe | Recipe list, ingredient đủ/thiếu, nút craft, workshop queue + timer, nút claim | ⚠️ `CraftService.CanCraft()` đang trả `ManualRuleRequired` | ✅ **timer + claim + maxCraftableAmount rule** (từ 001) | **Placeholder OK** | **Trung bình** | Craft ra item thật vào inventory theo đúng rule timer; **nếu rule không bóc được → giữ `ManualRuleRequired`, không tự đặt số** |
| **S6.5A-010** Merchant Function | Merchant, Market | List offer + giá, buy/sell, market listing, timer restock | ⚠️ `BuyItem()` đang trả `DeferredPriceOrCurrencyRule` | ✅ **price + restock + roll offer rule** (từ 001) | **Placeholder OK** | **Trung bình** | Mua/bán đổi money và inventory đúng rule; **nếu rule không rõ → giữ deferred** |
| **S6.5A-011** Settings Function | Settings, Save | Nút Save thủ công, Delete save, hiện version, 9 setting toggle ghi vào `SaveData` | ✅ `SaveService.Save()`/`DeleteSave()` có sẵn | ❌ | **Placeholder OK** (toggle trơn) | **Rất thấp** — làm được ngay | Bấm Save → file cập nhật; toggle setting → lưu → load lại vẫn giữ |
| **S6.5A-012** Offline Progress + Save Mutation Verify | OfflineProgress, Save | Thêm `lastAccess`, port `tick60`/`nextTimeTick`/`refreshCooldowns` + cap delta; **verify save round-trip**: action → save → load → giá trị đổi đúng | ⚠️ `OfflineProgressService` 64 dòng | ✅ cap delta rule (`MainActivity:878-880` — đọc được) | No Asset Needed | **Trung bình** | Thoát game 1 phút rồi vào lại → workshop/market tiến đúng số giây; test round-trip pass cho ≥3 domain |
| **S6.5A-013** Function Regression & Review | Toàn bộ | Chạy lại toàn bộ EditMode test + smoke test; verify không phá S1–S6; tổng hợp báo cáo | — | ❌ | No Asset Needed | **Thấp** | 0 lỗi CS, 0 exception, toàn bộ test pass, báo cáo tổng kết coverage mới |

## Task cần chờ rule (không được implement bừa)

| Task | Rule đang thiếu | Nếu không bóc được thì làm gì |
|---|---|---|
| S6.5A-007 | Công thức damage chính xác (crit multiplier, lifesteal, shield, retaliation) | Giữ `CalculateDamage_ManualPortRequired()` là stub, **không tự đặt số**, ghi rõ `MANUAL_RULE_REQUIRED` trong báo cáo và **không claim Dungeon là DONE** |
| S6.5A-005 | Level stat multiplier | Giữ `levelMultiplier = 1.0f` **nhưng phải ghi cảnh báo rõ trong UI/report rằng chỉ số chưa đúng decode** |
| S6.5A-009 | Craft timer + claim rule | Giữ `CraftFailureReason.ManualRuleRequired` |
| S6.5A-010 | Merchant price + restock rule | Giữ `MerchantFailureReason.DeferredPriceOrCurrencyRule` |
| S6.5A-008 | Doctrine amount rule | Bỏ phần doctrine trong quest, ghi deferred |

## Khuyến nghị thứ tự thực thi
1. **S6.5A-001** (bóc rule) — bắt buộc trước, vì 5 task khác phụ thuộc
2. **S6.5A-002 → 003** (formula + schema + wiring) — hạ tầng, rủi ro thấp
3. **S6.5A-011** (Settings) — thắng nhanh, chứng minh vòng save mutation hoạt động
4. **S6.5A-004** (Tavern) — mở nút thắt, sau bước này game mới "có gì để chơi"
5. **S6.5A-006 → 005** (Inventory actions → Character/Equipment)
6. **S6.5A-007** (Dungeon/Combat) — nặng nhất, làm khi hạ tầng đã chắc
7. **S6.5A-008 → 009 → 010** (Quest → Craft → Merchant)
8. **S6.5A-012 → 013** (Offline + Regression)

## Lưu ý về asset
**12/13 task chỉ cần `Placeholder OK` hoặc `No Asset Needed`.** Không có task nào bị chặn bởi thiếu asset. Toàn bộ S6.5B (asset finalization) **chỉ nên bắt đầu sau khi S6.5A xong**, để tránh làm đẹp cho UI rồi phải dựng lại khi thêm chức năng.
