# S6.5A — Dungeon / Combat / Loot Fix Report

**Ngày:** 2026-07-27
**Backup:** `Backups/S6_5A_Claude_DungeonCombatLootFix_20260727_135018/` (321 file)
**Audit đi kèm:** `S6_5A_Claude_DungeonCombatLoot_Audit_Report.md`

---

## Audit findings (tóm tắt)

Reject của Sếp là đúng. Dungeon **chưa được implement**, không phải chỉ thiếu test:

- **7 requirement MISSING**: spawn enemy · gọi combat · death · EXP · loot vào chest · CollectDrops · PlayMode test thật
- **2 UNSAFE_FAKE_FOUND**: damage `Math.Max(1, Dexterity)` tự chế · loot chuẩn hoá theo tổng bảng nên không bao giờ miss
- **2 bug chặn cứng**: `EnemyDefinition` dùng property (JsonUtility bỏ qua → mọi stat = 0) · data `enemies.json`/`dungeons.json` rỗng

---

## Bước 1 — Khôi phục data thật từ XAPK

Dùng lại đúng phương pháp đã thành công ở S6.5A-001C (androguard + DAD trên `classes3.dex`).

| Nguồn decode | Bóc ra | Kết quả |
|---|---|---|
| `enemies/units/*.configureStatistics()` | baseMaxHp, baseCON/INT/DEX, baseDEF/MDEF, expGiven, rarity, lifesteal, counterattack | **121/121 class** |
| `enemies/units/*.getMinDamage()/getMaxDamage()` | dải sát thương | **121/121** |
| `enemies/units/*.listDrops()` | drop table (item + weight + stack) | **118/121** (3 enemy thật sự không có drop) |
| `places/dungeons/*.listEnemies()` | danh sách enemy mỗi khu | **11/11 dungeon** |

**Verify tính toàn vẹn:**
- 120/121 enemy khớp id với `enemies.json` (1 class `EmperorClovisXXVIII` không có record tương ứng)
- **0 enemy id trong dungeon bị lệch** khỏi `enemies.json`
- 266/267 drop id khớp `items.json`; 1 item (`Evo23Vial`) không tồn tại nên bị loại
- Ví dụ `abomination`: HP 1000, DEF 50, dmg 30–35, exp 196, drops `bone_fragment:946 + soul_shard:50 + potion_of_constitution:4` = **đúng 1000** → khớp thang per-mille

Data ghi vào `enemies.json` / `dungeons.json` với `parseStatus: complete_from_dex`, `manualRuleRequired: false`.

---

## Bước 2 — Files changed

| File | Thay đổi |
|---|---|
| `Definitions/EnemyDefinition.cs` | **property → field** (JsonUtility mới đọc được); thêm `MinDamage`/`MaxDamage`/`IsMagic`/`IsRanged`/`BaseLifesteal`/`Counterattack`/`SourceClass`; `Dictionary Drops` → `List<EnemyDropEntry> DropTable` |
| `Definitions/DungeonDefinition.cs` | thêm `EnemyIds`, `SourceClass` |
| `Database/EnemyDropTableLoader.cs` | **MỚI** — parse `Drops`/`DropStacks` khỏi JSON thô (JsonUtility không đọc được dictionary) |
| `Database/DatabaseBuilder.cs` | gọi loader khi nạp category `enemies` |
| `Runtime/Services/CombatService.cs` | **bỏ damage tự chế**; thêm `RollAttackDamage()` port từ `Entity.rollAttackDamage()`; mở rộng `ICombatEntityWrapper` (MinAttackDamage/MaxAttackDamage/IsMagic/RollsDamageThreeTimes/ExpGiven) |
| `Runtime/Services/ILootService.cs` | thêm `RollSingleDrop()`, `IsChestFull()`, `StackCount` |
| `Runtime/Services/LootService.cs` | **viết lại** — dùng `DecodeMath.RollFromWeightedMap` thang 1000 (có miss); chest cap đếm **tổng stack** 2000/3000 |
| `Runtime/Services/IDungeonService.cs` | thêm `CollectDrops()` |
| `Runtime/Services/DungeonService.cs` | **thay `Tick()` bằng vòng lặp thật**: `PerformAction` 7 state · `EnterRoom`/`RollEnemies` · `FightRound` (turn cap 400) · `MoveCorpsesAndAwardExperience` · `RunLoot` · `CollectDrops` · `RestoreParty` · `ResolveParty` |
| `Runtime/Services/ServiceContainer.cs` | `DungeonService` nhận Combat/Loot/Character/Inventory |
| `Tests/PlayMode/S6_5A_DungeonCombatLootActionTests.cs` | **MỚI** |

---

## Bước 3 — Rule evidence dùng

| Rule | Nguồn | Áp dụng ở |
|---|---|---|
| `Entity.rollAttackDamage()` = `min + rand()*(max-min)`, best-of-3 nếu `rollsDamageThreeTimes` | `S6_5A_Claude_DungeonCombatLoot_Audit_Report.md` (DAD) | `CombatService.RollAttackDamage` |
| `Entity.applyDamage()` | `S6_5A_001C_Entity_applyDamage_smali.txt` | Giữ nguyên (Anti đã đúng) |
| `Area.performAction()` 7 state | `S6_5A_001D_Remaining_Core_Rules_Report.md` T-02 | `DungeonService.PerformAction` |
| `Action.turnsToComplete` 5/5/2/5/5/18/12 | `S6_5A_001E_Rule_Cleanup_Report.md` | `GetActionDuration` (giữ) |
| Turn cap 400 | T-05 | `FightRound` |
| Progress reset chỉ khi `<250` | T-02 case 5 | `PerformAction` case 5 |
| `Area.collectExperience()` = `Σexp / adventurersAlive`, `Utils.round` | D-04 | `MoveCorpsesAndAwardExperience` |
| `Utils.rollFromWeightedMap` thang 1000, có miss | `S6_5A_001E` | `LootService.RollSingleDrop` |
| `Area.fullChest()` tổng stack 2000/3000 | L-02 | `LootService.IsChestFull` |
| Loot vào `area.drops`, `collectDrops` mới vào kho | L-01/L-07 | `RunLoot` + `CollectDrops` |

**Không có công thức nào tự chế.** Không sửa rule để ép drop.

---

## Bước 4 — PlayMode test

`S6_5A_DungeonCombatLootActionTests.DungeonRun_Fights_Kills_Loots_Collects_AndPersists`

Test **fail nếu** chỉ có state loop chạy:

| Assert | Chặn điều gì |
|---|---|
| `BaseMaxHp > 0` | bắt lại BUG-1 nếu tái phát |
| `sawEnemy` | spawn thật, không phải state suông |
| `sawDamage` (HP giảm) | **không PASS nếu enemy không nhận damage** |
| `sawDeath` | phải có enemy chết |
| `PendingDrops.Count > 0` | loot phải vào **chest** |
| `inventoryBefore` ghi nhận trước khi collect | chứng minh loot **không** bypass chest |
| `transferred > 0 && inventoryAfter > before` | CollectDrops thật sự chuyển |
| `progressAfterReload == progressBefore` | save/reload giữ tiến độ |
| `inventoryAfterReload > 0` | item không bốc hơi sau reload |

Chọn dungeon có enemy **yếu nhất + có drop table**, tick tối đa 4000 lần. **Không** seed cứng, **không** sửa weight, **không** add item trực tiếp.

---

## Bước 5 — Verify đã làm

| Kiểm tra | Kết quả |
|---|---|
| Ngoặc cân bằng (bỏ string/comment) | ✅ 7/7 file OK |
| Tham chiếu `EnemyDefinition.Drops` cũ | ✅ 0 |
| Tham chiếu `.NameKey` cũ | ✅ 0 |
| Caller `ILootService` ngoài `DungeonService` | ✅ 0 (không ai gãy vì đổi chữ ký) |
| API test dùng có tồn tại | ✅ `CanAddItem`/`AddItem`/`GetQuantityByDefinitionId`/`TryGet`/`CreateCharacter` đều có |
| Enemy id trong dungeon khớp `enemies.json` | ✅ 0 lệch |
| Drop id khớp `items.json` | ✅ 266/267, đã loại 1 id không tồn tại |

---

## ⚠️ Chưa verify — cần Unity

| Hạng mục | Trạng thái |
|---|---|
| **Unity compile 0 CS** | ❌ **CHƯA CHẠY** — không có MCP Unity trong phiên này |
| **EditMode tests** | ❌ chưa chạy |
| **PlayMode `S6_5A_DungeonCombatLootActionTests`** | ❌ **chưa chạy** |
| **PlayModeTestResults.xml** | ❌ chưa có |

Em **không** cập nhật `S6_5A_Runtime_Action_Smoke_Report.md` từ PARTIAL sang PASS, vì điều kiện là "test thật qua đủ damage/death/loot/collect/save" — mà test chưa chạy lần nào.

---

## Remaining risks

| Risk | Mức | Ghi chú |
|---|---|---|
| **R-1** Test chưa chạy | 🔴 Cao | Toàn bộ kết luận phụ thuộc lần chạy đầu tiên |
| **R-2** Adventurer damage tạm là 1–1 | 🟡 Vừa | Decode: damage đến từ `weapon.getDamageModifier(CON,INT,DEX)`; chưa port weapon modifier nên tay không = 1 (đúng nhánh `weapon == null` của decode). **Enemy dùng số thật**. Đánh thắng vẫn được nhưng chậm — test có budget 4000 tick |
| **R-3** `RollEnemies` spawn 1 enemy/phòng | 🟡 Vừa | Decode composes phòng từ cùng list nhưng **rule số lượng chưa recover**. Chọn 1 để không bịa — ghi rõ là giới hạn, không phải rule |
| **R-4** Chưa có dodge/crit/status/cast/heal/retaliate | 🟡 Vừa | `dealDamage` đầy đủ còn nhiều nhánh; phần đã port là đường chính (roll → applyDamage) |
| **R-5** `EmperorClovisXXVIII` không có record | 🟢 Thấp | 1/121, không thuộc dungeon nào đã map |
| **R-6** Target selection vẫn chưa nối vào combat | 🟡 Vừa | `TargetSelectionService` có 15 strategy nhưng `ProcessTurn` vẫn chọn địch đầu còn sống |

---

## Final Decision

# `S6_5A_DUNGEON_COMBAT_LOOT_NEEDS_FIX`

**Lý do chọn mức này thay vì VERIFIED:**

Phần implement + data đã hoàn tất và bám evidence — nhưng theo đúng tiêu chí Sếp đặt ra, **không được claim VERIFIED khi test chưa chạy**. Cụ thể còn thiếu:
- Unity compile 0 CS (chưa có bằng chứng)
- PlayMode test chưa chạy lần nào → chưa chứng minh được damage/death/loot/collect/save

Đây đúng là tình huống "làm xong nhưng chưa nghiệm thu", không phải "đã nghiệm thu".

### Việc cần Sếp làm (1 lần)

1. Mở Unity, chờ reimport (có data JSON mới + 2 file code mới)
2. Console phải **0 lỗi CS**
3. `Window > General > Test Runner` → **PlayMode** → chạy `S6_5A_DungeonCombatLootActionTests`
4. Báo lại kết quả — kèm `Reports/S6_5A/S6_5A_DungeonCombatLoot_ActionTest_Result.md` (test tự ghi ra)

Nếu pass đủ 9 assert → lúc đó mới được chuyển:
- `S6_5A_Runtime_Action_Smoke_Report.md`: Dungeon PARTIAL → PASS
- Decision: `S6_5A_DUNGEON_COMBAT_LOOT_VERIFIED`

Nếu fail, log sẽ chỉ đúng assert nào gãy để sửa tiếp trong scope.
