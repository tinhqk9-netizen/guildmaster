# S6.5A — Dungeon / Combat / Loot Truth Audit

**Ngày:** 2026-07-27
**Phạm vi:** audit trung thực code Anti đã bàn giao, trước khi sửa.
**Kết luận ngắn:** việc reject `S6_5A_RUNTIME_ACTION_VERIFIED` là **đúng**. Dungeon không chỉ "chưa test đủ" — nó **chưa được implement**, và có **2 bug chặn cứng** khiến combat về mặt kỹ thuật không thể gây damage.

---

## Bảng audit

| # | Requirement | Current Implementation | Current Test Coverage | Status | Evidence |
|---:|---|---|---|---|---|
| 1 | Start dungeon with real dungeon definition | `StartDungeon()` lấy `DungeonDefinition` thật từ registry, tạo `DungeonRuntime`, lưu save | Smoke test có gọi | **PARTIAL** | `DungeonService.cs:23-35` |
| 2 | Party/adventurer assigned | `AdventurerInstanceIds` được lưu vào runtime **nhưng không bao giờ resolve thành `CharacterRuntime`** | Không | **PARTIAL** | `DungeonService.cs:31` — chỉ là `List<string>`, không có chỗ nào tra ra character |
| 3 | Enemy spawned from real enemy/dungeon data | ❌ **Không có `rollEnemies` / spawn ở bất kỳ đâu**. `_activeDungeon.Enemies` chỉ được ghi khi load save | Không | **MISSING** | `grep "Enemies ="` → chỉ `LoadDungeonState` |
| 4 | State machine enters FIGHT | `Tick()` đổi `ActionType` 0→1→2→3→4→1… theo bộ đếm | Smoke test kiểm `typeBefore != typeAfter` | **PARTIAL** — vào "state số 2" nhưng không có trận đánh nào | `DungeonService.cs:187-200` |
| 5 | Target selected via recovered targeting rule | `TargetSelectionService` có 15 strategy — **nhưng `CombatService` không hề gọi nó**, tự `FirstOrDefault` | Không | **PARTIAL** | `CombatService.cs:46-50` |
| 6 | Damage via recovered dealDamage/applyDamage | `ApplyDamage()` **đúng decode** (DEF/MDEF, flat `CON/8`, min 1, shield trước HP, `DecodeMath.Round`). **Nhưng damage đầu vào là `Math.Max(1, acting.Dexterity)` — công thức tự chế** | Không | 🔴 **UNSAFE_FAKE_FOUND** | `CombatService.cs:52` |
| 7 | HP/shield mutation | Code `ApplyDamage` có mutate — nhưng không bao giờ được gọi từ dungeon loop | Không | **MISSING** (trong luồng dungeon) | `Tick()` không gọi `ProcessTurn` |
| 8 | Enemy death detected | `EnemyRuntime.IsDead` tồn tại; `ProcessTurn` có check — nhưng không được dungeon gọi | Không | **MISSING** | — |
| 9 | EXP applied | ❌ Không có `collectExperience` ở bất kỳ đâu | Không | **MISSING** | `grep "ExpGiven"` → 0 nơi dùng |
| 10 | Loot rolled via weighted map rule | `RollLoot` dùng `_random.Next(totalWeight)` — **chuẩn hoá theo tổng bảng**, nên **không bao giờ miss**. Decode dùng thang cố định 1000 với khoảng trống = miss | Không | 🔴 **UNSAFE_FAKE_FOUND** | `LootService.cs:21-27` |
| 11 | Loot vào area.drops, không vào inventory | `PendingDrops` tồn tại trong `DungeonRuntime` — nhưng **không có code nào đổ loot vào đó lúc chạy** | Không | **MISSING** | — |
| 12 | CollectDrops chuyển chest → inventory | ❌ **Không tồn tại method nào** | Không | **MISSING** | `IDungeonService` không có |
| 13 | Dungeon progress mutation | `Progress++` khi rời state FIGHT; có rule reset `<250` | Không | **PARTIAL** — tăng mà không cần đánh thắng | `DungeonService.cs:207,217` |
| 14 | Save/reload giữ state | `SaveDungeonState`/`LoadDungeonState` khá đầy đủ (enemies, corpses, drops, action) | Smoke có `Save()` | **IMPLEMENTED_NOT_TESTED** | `DungeonService.cs:46-127` |
| 15 | PlayMode test verify tất cả | Chỉ tick 6 lần rồi so `ActionType` | — | **MISSING** | `S6_5A_RuntimeActionSmokeTest.cs:131-152` |

**Tổng: 0 IMPLEMENTED_AND_TESTED · 1 IMPLEMENTED_NOT_TESTED · 5 PARTIAL · 7 MISSING · 2 UNSAFE_FAKE_FOUND**

---

## 🔴 Hai bug chặn cứng (nghiêm trọng hơn cả phần thiếu)

### BUG-1 — `EnemyDefinition` dùng property, JsonUtility bỏ qua toàn bộ

```csharp
public int BaseMaxHp { get; set; }        // JsonUtility KHÔNG đọc property
public Dictionary<string,int> Drops {...} // JsonUtility KHÔNG đọc Dictionary
```

Unity `JsonUtility` chỉ deserialize **public field**. Nghĩa là kể cả khi JSON có số, mọi `EnemyDefinition.BaseMaxHp` / `BaseDefense` / `BaseDexterity` đều là **0** lúc chạy.

Hệ quả dây chuyền: `EnemyWrapper.MaxHp => Definition.BaseMaxHp` = 0 → enemy sinh ra đã chết → combat không thể có nghĩa. **Đây là lý do kỹ thuật khiến combat không bao giờ chạy được, dù có gọi đúng.**

### BUG-2 — Data nguồn rỗng

| File | Tình trạng |
|---|---|
| `enemies.json` | **122/122** record có `stats: {}` rỗng, `parseStatus: partial`, **119** đánh dấu `manualRuleRequired` + `MISSING_STATS` |
| `dungeons.json` | **11/11** `parseStatus: partial`, **không có trường nào liệt kê enemy** |

Converter không bóc được vì trong decode, `listEnemies()` và `listDrops()` là **method Java override trong từng class**, không phải data.

→ Ngay cả khi sửa BUG-1, vẫn **không có số liệu nào để đánh nhau**.

---

## Đánh giá công bằng phần Anti làm đúng

Không phải mọi thứ đều sai — những phần này **đúng decode và em giữ nguyên**:

| Phần | Nhận xét |
|---|---|
| `ApplyDamage()` | Đúng công thức: `(1 - min(1, (1-armorIgnored) × 0.01 × DEF)) × dmg - CON/8 - barrier`, tối thiểu 1, shield trước HP, dùng `DecodeMath.Round` |
| `GetActionDuration()` | Đúng timing decode: FIGHT 2, ENTER/LOOT/SEARCH 5, RESPAWN 18, FLEE 12 |
| Rule reset progress `<250` | Đúng |
| `SaveDungeonState`/`LoadDungeonState` | Cấu trúc đầy đủ, có enemies/corpses/drops/action state |
| `TargetSelectionService` | 15 strategy implement đúng — chỉ là chưa ai gọi |
| Anti tự đánh dấu `PARTIAL_PASS_LIMITED_SCOPE` | **Trung thực** — không tô hồng kết quả dungeon |

---

## Kết luận audit

Trạng thái thật trước khi sửa:

```
S6_5A_ACTION_SMOKE_PARTIAL
BLOCKER: Dungeon / Combat / Loot chưa DONE_VERIFIED
```

**Không phải "thiếu test" mà là "thiếu implement + 2 bug chặn + thiếu data".** Cần cả ba mới nghiệm thu được:
1. Bổ sung data thật (enemy stats, drop table, dungeon enemy list)
2. Sửa `EnemyDefinition` sang field
3. Nối vòng lặp dungeon thật: spawn → fight → death → exp → loot → chest → collect

Chi tiết việc sửa: `S6_5A_Claude_DungeonCombatLoot_Fix_Report.md`.
