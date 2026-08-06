---
author: Antigravity
date: 2026-08-06
target: D:\Tinh\Rebuild_GuildMaster
module: Dungeon Encounter Data
status: AUDIT_COMPLETE
---

# 🏰 DUNGEON ENCOUNTER DATA AUDIT

> **Mục tiêu:** Audit toàn bộ dữ liệu encounter của Dungeon từ source Java gốc → dungeon/area definitions → enemy groups/weights → wave generation → runtime combat loop → save/load.
> **Trạng thái:** Hoàn tất. Đây là một điểm mù cực kỳ lớn (Critical Data Loss) của bộ Parser. Hệ thống spawn quái trong C# hiện tại chạy dựa trên dữ liệu lỗi, dẫn đến việc thiết kế Encounter và tiến trình Dungeon bị sai lệch hoàn toàn.

---

## 1. Dungeon Inventory

* **Tổng số Area trong Legacy:** 23
  * Normal Dungeons: 11 (`BarrenWastelands`, `BlackwaterPort`, `EnchantedForest`, `EternalBattlefield`, `FrostbitePeaks`, `HiddenCityOfLarox`, `LostLands`, `ObsidianMines`, `TheDesert`, `TheGoldenCity`, `TheSouthernGrove`).
  * Raids: 12 (`AncientGraveDigging`, `CelestialMothership`, v.v.).
* Không có duplicate, không có abstract class bị khởi tạo nhầm.

---

## 2. Encounter Definitions (Legacy Java)

Trong bản Java gốc, **KHÔNG CÓ File JSON hay cấu trúc Data nào định nghĩa Encounter**. Toàn bộ logic spawn quái được **hardcode trực tiếp** trong hàm `protected List<Enemy> rollEnemies()` của từng class Dungeon/Raid.

* **Group/Composition:** Được tạo cứng (hardcoded list), có thể từ 0 đến 6 quái. (VD: `Arrays.asList(Enemy.getInstance("Wurm"), Enemy.getInstance("SandVulture"))`).
* **Weights:** Được code cứng bằng chuỗi các lệnh `if (dRandom < X.0d)`.
  * *Ví dụ (`TheDesert.java`):* Tổng weight = 1000. 
    * `dRandom >= 450.0d` ➔ Empty room (Không có quái, chuyển ngay sang sự kiện).
    * `dRandom < 25.0d` ➔ Spawn 1 `ShahuriWarrior` (Tỉ lệ 2.5%).
    * `dRandom < 125.0d` ➔ Spawn 1 `Wurm` + 1 `SandVulture` (Tỉ lệ 2.5%).
* **Boss / Special Events:** Được điều khiển bởi `this.event.getKey()`.
  * *Ví dụ:* Ở event 2 của `TheDesert`, nó luôn spawn **5 con SandStatue** bỏ qua RNG.

---

## 3. Data & Parser Failure (The Flattening Effect)

Parser của dự án đã **bỏ qua hoàn toàn hàm `rollEnemies()`** do nó là Java Code logic chứ không phải data array.
Thay vào đó, Parser nhắm vào hàm `public List<Enemy> listEnemies()` (vốn chỉ là hàm để hiển thị danh sách quái vật trong UI).

* **JSON Output (`dungeons.json`):**
  * Tạo ra một property là `"EnemyIds": ["djinn", "sand_statue", "wurm", ...]`.
* **Hậu quả:** 
  * Bị **Flattened 100%**. Toàn bộ Weight, Group Composition, và tỉ lệ spawn Boss biến mất.
  * Tỉ lệ `Empty Room` cũng biến mất.
  * Các sự kiện dò đường, nhặt rác, dính bẫy (hàm `searchRoom()`) cũng không được parse.

---

## 4. Runtime Generation (C# Rebuild Hallucinations)

Do dữ liệu đầu vào (`EnemyIds`) chỉ là một danh sách phẳng (flat array), code runtime trong `DungeonService.cs` (`RollEnemies`) đã phải implement một cách "nhắm mắt đưa chân":

```csharp
string enemyId = pool[_random.Next(pool.Count)];
spawned.Add(new EnemyRuntime(Guid.NewGuid().ToString(), def) ...);
return spawned;
```

**Lỗi nghiêm trọng trong C# Runtime:**
1. **Luôn spawn đúng 1 con quái vật mỗi Encounter:** (Trong khi game gốc là đánh Party vs Party, spawn từ 1 tới 6 quái).
2. **Không có Weight:** Tỉ lệ ra Boss ngang bằng với tỉ lệ ra tiểu yêu (1/N).
3. **Không bao giờ Empty Room:** Nếu chưa đánh xong Dungeon thì luôn gặp quái, đánh bay hoàn toàn cơ hội rớt nguyên liệu từ `searchRoom()`.

---

## 5. Save/Load Integrity

* **C# Save:** Lưu danh sách `EnemyRuntime` (vì nó chỉ tạo 1 con).
* **Legacy Save:** Lưu danh sách `Enemy` đầy đủ của một Encounter.
* Nếu load từ Legacy sang C#, C# sẽ gặp vấn đề nếu danh sách Enemy trong file save lớn hơn 1 (mặc dù code C# có thể Deserialize thành List, nhưng vòng lặp game C# không được test với >1 quái vật phe địch).

---

## 6. Reference Integrity (Dungeon Mẫu)

Dưới đây là bảng đánh giá 15 Area (11 Dungeon + 4 Raid):

| Dungeon ID | Legacy encounter groups | Weights | Boss rule | Current data (`EnemyIds`) | Runtime generation | Status |
|---|---|---|---|---|---|---|
| `barren_wastelands` | 1-5 quái | Hardcoded | Mất | Flattened | Spawn 1 quái ngẫu nhiên | ❌ Flattened / Missing Logic |
| `blackwater_port` | 1-4 quái | Hardcoded | Mất | Flattened | Spawn 1 quái ngẫu nhiên | ❌ Flattened / Missing Logic |
| `enchanted_forest` | 1-3 quái | Hardcoded | Mất | Flattened | Spawn 1 quái ngẫu nhiên | ❌ Flattened / Missing Logic |
| `eternal_battlefield` | 1-6 quái | Hardcoded | Mất | Flattened | Spawn 1 quái ngẫu nhiên | ❌ Flattened / Missing Logic |
| `frostbite_peaks` | 1-4 quái | Hardcoded | Mất | Flattened | Spawn 1 quái ngẫu nhiên | ❌ Flattened / Missing Logic |
| `hidden_city_of_larox` | 1-4 quái | Hardcoded | Mất | Flattened | Spawn 1 quái ngẫu nhiên | ❌ Flattened / Missing Logic |
| `lost_lands` | 1-5 quái | Hardcoded | Mất | Flattened | Spawn 1 quái ngẫu nhiên | ❌ Flattened / Missing Logic |
| `obsidian_mines` | 1-5 quái | Hardcoded | Mất | Flattened | Spawn 1 quái ngẫu nhiên | ❌ Flattened / Missing Logic |
| `the_desert` | 1-4 quái | Hardcoded | Mất (event key) | Flattened | Spawn 1 quái ngẫu nhiên | ❌ Flattened / Missing Logic |
| `the_golden_city` | 1-3 quái | Hardcoded | Mất | Flattened | Spawn 1 quái ngẫu nhiên | ❌ Flattened / Missing Logic |
| `the_southern_grove` | 1-4 quái | Hardcoded | Mất | Flattened | Spawn 1 quái ngẫu nhiên | ❌ Flattened / Missing Logic |
| `ancient_grave_digging` | 1-6 quái | Hardcoded | Mất | Mất sạch | Lỗi (List rỗng) | ❌ Missing Field |
| `celestial_mothership` | 1-5 quái | Hardcoded | Mất | Mất sạch | Lỗi (List rỗng) | ❌ Missing Field |
| `divine_archeology` | 1-4 quái | Hardcoded | Mất | Mất sạch | Lỗi (List rỗng) | ❌ Missing Field |
| `imperial_rescue` | 1-5 quái | Hardcoded | Mất | Mất sạch | Lỗi (List rỗng) | ❌ Missing Field |

*Lưu ý:* Các Raids (VD: `ancient_grave_digging`) thậm chí còn bị mất luôn mảng `EnemyIds` trong file `raids.json`, khiến Runtime Generation trả về List rỗng và kẹt game.

---

## Tổng kết

Hệ thống Encounter Generator của bản Rebuild là một **Critical Failure**. Do data parser không thể parse được code Java logic (`rollEnemies()`), team dev đã chọn cách parse list UI (`listEnemies()`) dẫn tới việc C# phải bịa ra cơ chế spawn "1 quái ngẫu nhiên không weight" để game chạy được. Để fix lỗi này, bắt buộc phải viết một custom AST Parser hoặc convert tay 23 file Java thành JSON schema chuẩn cho Encounter Groups và Weights.
