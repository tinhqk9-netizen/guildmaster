# S6.5A Function Completion Plan

> ## ✅ CẬP NHẬT 4 — 2026-07-27, sau S6.5A-001D (Remaining Core Rules)
>
> ### Dump 58 method smali + 50 method DAD, **0 thất bại**
> | Nhóm | Kết quả | Confidence |
> |---|---|---|
> | **Dungeon tick** | State machine **7 trạng thái** (`Action 0..6`) đọc được hoàn toàn | 96% |
> | **Target selection** | **15 targeting strategy** + `attackTargetStrategy` | 93–97% |
> | **Loot** | `loot()`, `fullChest()`, pet bonus, Geode | 95% |
> | **Quest** | `calculateDifficulty`, `rollRarity`, 14 quest trigger | 90–98% |
> | **Merchant roll** | `rollPotion`/`rollSpecialFoods`/`rollUpgrades`/`truncatePrice` | 93–95% |
> | **Trap** | `trapEncounter` (JADX từng bỏ) nay đọc được | 90% |
>
> ### Rule cân bằng quan trọng — nếu bịa sẽ sai
> - Cap **400 lượt** đánh mỗi trận (chỉ dungeon; raid không cap)
> - Khi thua: progress **chỉ reset về 0 nếu `progress < 250`**
> - Chest cap **2000** stack (**3000** nếu merchantPack)
> - Geode drop **0.05%** mỗi corpse · pet bonus roll = `pet.getDrops()/100`
> - Quest rarity: `<0.7`→1 · `<0.9`→2 · `<0.97`→3 · `≥0.97`→4
> - Shield từ end-of-turn cap **20% maxHp**
> - Quest `tabulaRasa` trigger khi **≥4 kill trong 1 lượt**
> - **Loot KHÔNG vào thẳng kho** — vào `area.drops` (rương tạm), phải `collectDrops` mới vào `data.items`. **Không có money/gems drop trực tiếp từ quái**
>
> ### Confidence Gate: **13/17 domain ≥95%**
> ✅ **YES:** Formula · SaveData · Doctrine · Tavern visitor · Tavern recruit · Character stat · Damage · ApplyDamage · **Dungeon tick** · **Loot** · Craft · Merchant sell/timer · Offline · Settings
> ⚠️ **PARTIAL:** Target selection (93%) · Quest (90%, **claim reward <80%**)
> ❌ **NO:** Merchant **buy** (<80% — `DialogBuyFromMerchant` chưa dump)
>
> ### Unity target mới cần tạo
> `DungeonRunOrchestrator` · `TargetSelectionService` · `DoctrineService` · `TavernService` · `SettingsService`
>
> ### 6 risk đã ghi nhận (kèm hành động)
> R-01 Merchant buy · R-02 Quest claim · R-03 Target helper 92–93% · R-04 `rollFromWeightedMap` · R-05 `Action` class chưa dump · R-06 khối lượng combat lớn → **chia nhỏ S6.5A-007**
>
> ### Khuyến nghị: chạy **S6.5A-001E** (nhẹ) song song với S6.5A-002
> Dump nốt: `DialogBuyFromMerchant` · quest claim caller · `rollFromWeightedMap` · class `Action` · `Area.cast/heal/retaliate/searchRoom/rollEnemies`
>
> **Decision: `S6_5A_001D_DONE_READY_FOR_S6_5A_002`** — Formula + SaveData không phụ thuộc gì trong danh sách trên, làm được ngay.
>
> Chi tiết: `S6_5A_001D_Remaining_Core_Rules_Report.md`

---

> ## ✅ CẬP NHẬT 3 — 2026-07-27, sau S6.5A-001C (XAPK Recovery)
>
> **🎉 CẢ 2 BLOCKER CỨNG ĐÃ GIẢI QUYẾT.** Cập nhật 2 bên dưới (kết luận "không tồn tại raw bytecode") **đã SAI** — `New folder.zip` chứa XAPK v2.147 với APK + 3 DEX đầy đủ.
>
> | Blocker | Trạng thái mới | Confidence |
> |---|---|---|
> | `Adventurer.calculateTotalStat` | ✅ **RECOVERED** — công thức + bảng trait 6×6 | **98%** |
> | `Area.dealDamage` | ✅ **RECOVERED** — pipeline 6 hệ số + `applyDamage` | **95%** |
> | `Entity.applyDamage` | ✅ **RECOVERED** — xác nhận chéo 3 nguồn | **99%** |
>
> **Công cụ:** không cần Java — dùng **androguard 4.1.4** (Python) để parse DEX, dump smali, chạy DAD decompiler, đọc `packed-switch` payload raw.
>
> **3 chi tiết tinh vi trong `calculateTotalStat`:** potion index đảo (INT→`potions[2]`, DEX→`potions[1]`) · DEF/MDEF **không** nhân ascended multiplier · MAX_HP cộng `level-1` **trước** khi nhân, potion **×5**.
>
> Evidence: 7 file trong `Reports/S6_5A/` (`…_XAPK_Recovery_Report.md`, 3 file smali, `…_DAD_Decompile_Output.md`, `…_TraitSwitchMapping.md`, `…_Recovered_Rule_Summary.md`)

---

> ## 🔴 CẬP NHẬT 2 — 2026-07-27, sau S6.5A-001B (Decompiler Recovery)
>
> ⚠️ **LƯU Ý: kết luận "không tồn tại raw bytecode" trong mục này đã bị BÁC BỎ bởi Cập nhật 3.** Giữ lại để tham chiếu lịch sử.
>
> ### Kết luận dứt khoát về 2 blocker cứng
> Đã tìm toàn hệ thống: **KHÔNG tồn tại raw bytecode nào** (`.apk`/`.dex`/`.smali`/`.jar`/`.class`) — kể cả trong zip gốc 25 MB (17.404 entry đều là `.java`/`.png`/`.xml`). `Document/GDD` là **kế hoạch sprint tự viết**, không phải tài liệu gốc.
>
> → **Không có đầu vào thì không decompiler nào (CFR/Procyon/Fernflower/dex2jar) chạy được.**
>
> | Blocker | Trạng thái | Confidence |
> |---|---|---|
> | `Area.dealDamage()` — damage formula | 🔴 **ManualRuleRequired_Damage** | < 70% |
> | `Adventurer.calculateTotalStat()` — stat/level multiplier | 🔴 **ManualRuleRequired_CharacterStat** | < 70% (mapping 0..5 thì 99%) |
>
> **→ S6.5A-005 (Character stat) và S6.5A-007 (Dungeon/Combat) BỊ CHẶN CỨNG.** Không sửa `levelMultiplier = 1.0f`.
>
> ### Rule recover thêm được (≥96%) — mở thêm task
> | Domain | Confidence | Ghi chú |
> |---|---|---|
> | **Doctrine** | 98% | Cơ chế `getValue() = level × increasePerLevel` + bảng đầy đủ 40 `DoctrineAbilityType` |
> | **Tavern recruit** | 98% | 🎉 **Recruit MIỄN PHÍ** — chỉ giới hạn bởi `getQuartersCapacity()` |
> | **`rollRareTrait`** | 99% | 14 rare trait, mỗi loại 1/70, 80% không trait |
> | **Enemy stat** | 99% | `calculateTotalX() = baseX` — **không multiplier** → Enemy port được 100% |
>
> ### Confidence Gate: 11/17 domain sẵn sàng
> ✅ **Ready (≥95%):** Formula · SaveData · Tavern visitor · Tavern recruit · Doctrine · Craft · Merchant(sell/timer) · Offline · Settings · Enemy stat · Service wiring
> ❌ **Blocked (<70%):** Character stat · Damage/Combat · Dungeon tick · Target selection · Loot · Quest · Merchant(buy/roll)
>
> ### Thêm task mới: S6.5A-001C
> Đọc nốt 7 nhóm rule còn thiếu — **đều đọc được**, chỉ cần thời gian:
> `001C-1` Area.tick/performAction/enterRoom · `001C-2` fightTurn/decideTurnsOrder/resolveStatus · `001C-3` selectTargets + 7 strategy · `001C-4` loot/fullChest/collectDrops · `001C-5` QuestsManager (468 dòng) · `001C-6` rollPotion/rollSpecialFoods/rollUpgrades/DialogBuyFromMerchant · `001C-7` 7 doctrine instance còn lại + calculateNewAdventurerId
>
> ### ⚠️ CẦN USER QUYẾT ĐỊNH — 2 blocker chỉ có 3 lối thoát
> | Lối thoát | Đánh giá |
> |---|---|
> | **A. Cung cấp APK/DEX gốc** rồi decompile bằng CFR/Procyon | ✅ **Khuyến nghị mạnh nhất** — đây là giới hạn của riêng JADX với method lớn, không phải obfuscation |
> | **B. Tự thiết kế lại công thức** damage/stat | ⚠️ **Vi phạm nguyên tắc "logic phải theo decode"** — chỉ làm nếu user chủ động đổi nguyên tắc |
> | **C. Bỏ combat khỏi scope** (chỉ giữ tavern/craft/merchant/inventory) | ⚠️ Thu hẹp scope đáng kể |
>
> ### Thứ tự khuyến nghị (cập nhật)
> `002` → `003` → `011` Settings → `004` Tavern → `009` Craft → `012` Offline → *(song song `001C`)* → `010` Merchant(sell) → `006` Inventory → **dừng trước `005`/`007` cho tới khi user chọn lối thoát A/B/C**
>
> Chi tiết: `S6_5A_001B_Additional_Rule_Extraction_Report.md`

---

> ## ⚠️ CẬP NHẬT 1 — 2026-07-27, sau S6.5A-001 Rule Extraction
>
> Kết quả bóc rule làm **thay đổi đáng kể** kế hoạch bên dưới. Đọc mục này trước.
>
> ### Tin tốt — 4 task được gỡ chặn, có thể làm sớm hơn dự kiến
> | Task | Thay đổi |
> |---|---|
> | **S6.5A-002** (Formula + SaveData) | ✅ **20/20 formula và 140/140 save field đọc được đầy đủ** → port 1:1 được ngay, rủi ro thấp |
> | **S6.5A-004** (Tavern) | ✅ **Rule sinh visitor/interval/capacity/roll class+trait đọc được đầy đủ** → mở được nút thắt gameplay |
> | **S6.5A-009** (Craft) | ✅ **Rule timer/claim/maxCraftableAmount đọc được đầy đủ** → **gỡ được `CraftFailureReason.ManualRuleRequired`** (trái với dự đoán ban đầu) |
> | **S6.5A-012** (Offline) | ✅ Rule delta + cap 12 giờ đọc được |
>
> ### Tin xấu — 2 blocker CỨNG không giải được bằng nguồn decode hiện tại
> JADX **không decompile được** đúng 2 method quan trọng nhất (bằng chứng ghi thẳng trong file decode):
> - `Area.dealDamage()` — **1102 instruction units** → **công thức damage**
> - `Adventurer.calculateTotalStat(int)` — **332 units** → **level multiplier + 6 stat**
>
> **Hệ quả:** **S6.5A-005** (Character stat) và **S6.5A-007** (Dungeon/Combat) **KHÔNG THỂ hoàn thành đúng** cho tới khi giải được 2 blocker này. Không được tự đặt công thức thay thế.
>
> ### Thêm task mới: S6.5A-001B
> | Task mới | Mục tiêu | Ưu tiên |
> |---|---|---|
> | **S6.5A-001B** Additional Rule Extraction | (a) **Giải 2 blocker**: thử `dex2jar` + CFR/Procyon/Fernflower, hoặc đọc smali, hoặc tra `Document/GDD`. (b) Đọc nốt 6 nhóm rule còn thiếu: Doctrine, Quest, Merchant buy/roll offer, Recruit action + cost, Dungeon tick/target selection, Loot | **P0 — chạy song song với S6.5A-002** |
>
> ### Thay đổi ưu tiên
> - **Doctrine nâng từ `Deferred` lên `Core_Needed_Now`** — vì `bonusCritChance/bonusCritDamage/bonusDodgeChance/bonusCounterattack/bonusManaRegen` được gọi **trực tiếp** trong công thức crit/dodge/counterattack của `Adventurer`. Không port doctrine thì các rule đó không tính đúng được.
> - **5 IAP flag phải port vào `SaveData`** (mặc định `false`, không implement mua) vì là **đầu vào của 7 formula**: F-12, F-14, F-15, F-16, F-17, CR-01 (craft time), M-01 (sell time).
> - **Thứ tự khuyến nghị mới:** `001B` (song song) → `002` → `003` → `011` (Settings, thắng nhanh) → `004` (Tavern, mở nút thắt) → `009` (Craft, đã đủ rule) → `006` → `010` → `005`/`007` (**chỉ khi 001B giải được blocker**) → `008` → `012` → `013`
>
> Chi tiết đầy đủ: `S6_5A_001_Rule_Extraction_Report.md`

---

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
