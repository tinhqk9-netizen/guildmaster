# S6.5A — Dungeon Data Loader Fix: Root Cause Audit

**Ngày:** 2026-07-27
**Backup:** `Backups/S6_5A_Claude_DungeonDataLoaderFix_20260727_143120/` (328 file, gồm cả external staging)
**Triệu chứng:** PlayMode test fail tại dòng 106 — `no dungeon exposes enemies with a drop table`

---

## 🎯 Root cause — data đúng nhưng nằm sai chỗ Editor đọc

Log chính là manh mối quyết định:

```
[UIRuntimeBootstrap] Database built via EditorExternalGameDataProvider: 10/10 file(s), 1531 record(s).
```

`EditorExternalGameDataProvider` mặc định trỏ tới `D:\Tinh\Game Decode Converter\output\production_staging\`, **không phải** `Assets/StreamingAssets/GameData/` — nơi phiên trước đã ghi data khôi phục từ XAPK.

| Nguồn | enemies BaseMaxHp>0 | enemies Drops | dungeons EnemyIds |
|---|---:|---:|---:|
| `Assets/StreamingAssets/GameData/` (đã ghi) | **120** | **116** | **11** |
| `Game Decode Converter/production_staging/` (**Editor đọc**) | **0** | **0** | **0** |

Test tìm dungeon có enemy kèm drop table → không có → `chosen == null` → fail. Data hoàn toàn đúng, chỉ là runtime không nhìn thấy.

---

## Bảng audit

| Check | Expected | Actual | Status |
|---|---|---|---|
| `enemies.json` (StreamingAssets) có stats thật? | có | **120/122** BaseMaxHp>0 | ✅ PASS |
| `enemies.json` có MinDamage/MaxDamage? | có | **121** enemy có dải damage | ✅ PASS |
| `enemies.json` có drop table? | có | **116** enemy có `Drops` | ✅ PASS |
| `dungeons.json` có enemy list? | có | **11/11** có `EnemyIds` | ✅ PASS |
| Field JSON khớp `DungeonDefinition`? | khớp | `EnemyIds` khớp field mới | ✅ PASS |
| Field JSON khớp `EnemyDefinition`? | khớp | khớp sau khi đổi property→field | ✅ PASS |
| **JsonUtility đọc được field `EnemyDefinition`?** | đọc được | đã sửa property→field phiên trước | ✅ PASS |
| **JsonUtility đọc được field `AdventurerDefinition`?** | đọc được | ❌ **vẫn dùng property** → mọi stat = 0 | 🔴 **FAIL → đã sửa** |
| **`adventurers.json` có stat phẳng?** | có | ❌ stat lồng trong `baseStats.{field}.value` | 🔴 **FAIL → đã flatten** |
| `EnemyDropTableLoader` được gọi trong `DatabaseBuilder`? | có | có, tại `LoadCategory<EnemyDefinition>` | ✅ PASS |
| Loader gán drop table đúng definition? | đúng | khớp theo `id`, slice theo record | ✅ PASS |
| ID enemy trong dungeon khớp database? | 100% | **0 lệch** | ✅ PASS |
| **Test query đúng runtime database?** | đúng | đúng, nhưng **database nạp từ sai thư mục** | 🔴 **FAIL → đã sửa** |

---

## Số liệu debug (theo yêu cầu)

| Chỉ số | Giá trị |
|---|---|
| Dungeon có `EnemyIds.Count > 0` | **11 / 11** |
| Enemy có `BaseMaxHp > 0` | **120 / 122** |
| Enemy có `MinDamage`/`MaxDamage` > 0 | **121 / 122** |
| Enemy có `DropTable.Count > 0` | **116 / 122** |
| Dungeon đầu tiên + enemy ids | `the_southern_grove` → `ancient_ent, dryad, giant_moth, giant_tortoise, green_spitfang, primeval_wurm` |
| Ví dụ enemy có drop table | `abomination` → `bone_fragment` w946 x3, `soul_shard` w50 x1, `potion_of_constitution` w4 x1 (**tổng đúng 1000**) |
| Enemy id trong dungeon không tìm thấy trong DB | **0** |
| Dungeon có enemy đủ điều kiện test (HP>0 + drops) | **11 / 11** |
| Enemy dễ nhất có drop | `golden_rabbit` — HP 40, dmg 1–2, thuộc `enchanted_forest` |
| Adventurer có `BaseMaxHp > 0` (sau flatten) | **109 / 129** |

---

## 🔴 Blocker thứ hai phát hiện trong lúc audit

Trong khi verify, em thấy `AdventurerDefinition` mắc **đúng cùng lỗi** đã sửa cho `EnemyDefinition`:

```csharp
public int BaseMaxHp { get; set; }   // JsonUtility bỏ qua property
```

Cộng thêm `adventurers.json` để stat **lồng** trong `baseStats.baseMaxHp.value` thay vì phẳng. Kết quả: **0/129 adventurer** có HP lúc chạy.

Nếu chỉ sửa provider path mà bỏ qua chỗ này, test vẫn fail — chỉ là fail ở assert khác (`hero.CurrentHp` = 1).

---

## Các thay đổi đã áp dụng

| # | File | Thay đổi |
|---|---|---|
| 1 | `Infrastructure/DataProviders/EditorExternalGameDataProvider.cs` | Mặc định đọc **`StreamingAssets/GameData`** (một nguồn duy nhất). Fallback về converter staging kèm `LogWarning` nếu thiếu `manifest.json`. Nhánh có tham số (`S2VerificationTests`) **giữ nguyên** |
| 2 | `Definitions/AdventurerDefinition.cs` | property → **public field** (12 thành viên) |
| 3 | `Assets/StreamingAssets/GameData/adventurers.json` | Flatten `baseStats.{x}.value` → field phẳng `BaseMaxHp`/`BaseConstitution`/… (116 record) |
| 4 | `Runtime/Boot/UIRuntimeBootstrap.cs` | Log thêm `Enemy drop tables loaded: N`; tách counter chẩn đoán khỏi tổng record |
| 5 | `Tests/EditMode/S6_5A_DungeonDataIntegrityTests.cs` | **MỚI** — 7 test chặn trước PlayMode |
| 6 | `Tests/PlayMode/S6_5A_DungeonCombatLootActionTests.cs` | Chọn adventurer **khoẻ nhất** (đối xứng với việc chọn enemy yếu nhất), thêm assert `BaseMaxHp > 0` |

**Không** copy data sang converter staging — chọn hướng *một nguồn duy nhất* thay vì đồng bộ hai nơi, đúng yêu cầu *"không normalize bừa ở nhiều nơi gây khó debug"*.

---

## Vì sao chọn sửa provider thay vì copy data

| Phương án | Đánh giá |
|---|---|
| Copy data sang `production_staging` | ❌ Hai bản phải đồng bộ mãi mãi; lần sau lệch lại đúng lỗi này |
| **Provider đọc StreamingAssets** | ✅ Cùng nguồn với build; sửa một chỗ, hết phân kỳ Editor vs Build |

Converter staging vẫn là fallback hợp lệ khi project chưa có StreamingAssets — nhưng sẽ log cảnh báo rõ.

---

## ⚠️ Gap còn lại: adventurer damage

`Adventurer.calculateMinAttackDamage()` trong decode:
```java
if (weapon == null) return 1;
mod = weapon.getDamageModifier(CON, INT, DEX);
return Utils.round(mod * (1.0 - weapon.damageDelta()));
```

Weapon system **chưa được port**, nên adventurer hiện đánh **1 damage/lượt** — đúng nhánh `weapon == null` của decode, nhưng trong game thật adventurer luôn được `getDefaultWeapon()` gán vũ khí lúc tuyển từ tavern.

**Hệ quả cho test:** chọn adventurer khoẻ nhất (390 HP) đấu `golden_rabbit` (40 HP, 1–2 dmg) → cần ~40 lượt để giết, trong khi adventurer chịu được ~260 lượt → **thắng được**. Nhưng đây là biên an toàn hẹp và **không phản ánh cân bằng thật của game**.

→ Ghi nhận là hạng mục phải làm: **port `getDefaultWeapon` + `Weapon.getDamageModifier`**. Rule đã đọc được, không phải `ManualRuleRequired`.

---

## Trạng thái

Chưa chạy được Unity trong phiên này, nên **chưa chốt PASS**. Cần Sếp chạy lại theo thứ tự: EditMode `S6_5A_DungeonDataIntegrityTests` trước, rồi PlayMode `S6_5A_DungeonCombatLootActionTests`.
