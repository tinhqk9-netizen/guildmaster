# 🧠 Báo cáo Audit: Hệ thống Doctrine

**Dự án**: D:\Tinh\Rebuild_GuildMaster
**Ngày Audit**: 2026-08-06

---

## 1. Doctrine Inventory (Thống kê từ Java Legacy)
Toàn bộ hệ thống Doctrine được định nghĩa cứng bằng Java thông qua abstract class `Doctrine.java` và các lớp con (instances).

### 🔴 Tổng quan Legacy
- **Số lượng Doctrine hoạt động:** 8 (Affliction, Control, Fortitude, Grace, Illusion, Knowledge, Ruin, War).
- **Cấu trúc Level/Node:** Mỗi Doctrine có chính xác **6 nodes** (`l1` đến `l6`).
- **Tổng số loại Node (Abilities):** Định nghĩa cứng 40 nodes trong `DoctrineAbilityType.java` (ví dụ: `IMPROVED_HEALTH`, `CONDITIONED_REFLEXES`, `FALSE_LIFE`, `HEALING_NOVA`...).
- **Hooks hiệu ứng:** Có tổng cộng **36** hooks tác động tới Game trong `Doctrine.java` (ví dụ: `bonusConstitution()`, `bonusCounterattack()`, `forcesCounterattack()`, `freezeOnHit()`, `healingNova()`).

---

## 2. Legacy Behavior (Logic gốc)
- **Cơ chế hoạt động:** Mỗi class con (ví dụ `DoctrineOfWar.java`) sẽ override hàm `setupAbilities()` để map 6 `DoctrineAbilityType` cụ thể vào 6 slot. Khi cần lấy chỉ số, Java gọi hàm tương ứng (ví dụ: `bonusCounterattack()` -> `getValue(DoctrineAbilityType.CONDITIONED_REFLEXES)`).
- **Cost/Unlock:** Từng node có biến `maxLevel`, `cost`, và `increasePerLevel` riêng rẽ được lưu ở `DoctrineAbilityType`.
- **Dependencies:** Hiệu ứng Doctrine gắn chặt với Hero Stats (tăng Int/Dex/Con), Combat Events (hồi máu, phản đòn, đóng băng) và cả Loot/Quest (`bonusQuestPoints`).

---

## 3. Data & Model Hiện Tại (Rebuild C#)
Phát hiện mất mát dữ liệu (Data Loss) mang tính cấu trúc vô cùng nghiêm trọng!

### 🔴 Thiết kế C# phá vỡ cấu trúc 6-Node
- **Trong C#:** File `DoctrineService.cs` **CHỈ LƯU ĐÚNG MỘT LEVEL TỔNG** cho mỗi Doctrine (vd: `WarLevel`, `AfflictionLevel`) thông qua `SaveData.cs`.
- **Hệ quả:** Vì 1 Doctrine gốc chứa 6 Node độc lập, tổng level có thể lên tới 17. Việc chỉ lưu 1 biến `Level` duy nhất ở C# làm **MẤT TRẮNG toàn bộ tính tuỳ biến của 6 nodes**. Người chơi không thể nâng cấp từng node riêng lẻ như thiết kế gốc được nữa, vì game hiện tại không hề lưu Node nào đang ở level nào.

### 🔴 Không có giới hạn Max Level
- Hàm `DoctrineService.AddProgress` dùng `FormulaService` để tính sao lên cấp, nhưng **KHÔNG HỀ CÓ LOGIC KIỂM TRA MAX LEVEL**. Người chơi có thể tăng cấp Doctrine tới vô cực.

---

## 4. Runtime Integration (`CharacterService` & `CombatService`)
Hệ thống Doctrine hiện tại trong C# là **RỖNG (Placeholder) 100%**.

- **`CharacterService.cs`:** Hoàn toàn KHÔNG gọi tới `DoctrineService.GetLevel()`. Toàn bộ stat bonus của 8 Doctrines (Int, Dex, Con, Max HP, Dodge, Mana) đều bị bỏ qua.
- **`CombatService.cs`:** Hoàn toàn KHÔNG import hay đụng chạm gì tới Doctrine. 100% các combat effect (Crit, Counterattack, Freeze, Status Immunity, Lifesteal, Threat, Healing Nova) đều vứt xó.
- **`QuestService.cs`:** Chỗ duy nhất trong toàn bộ game tương tác với Doctrine là cộng điểm `AddProgress` vào khi hoàn thành Quest. Chấm hết. 
- **UI (`CharacterDetailPanel.cs`):** Chỉ đơn thuần gọi `GetLevel()` để hiển thị cho có chữ, không có tác dụng gameplay.

---

## 5. Bảng Reference Integrity

| Doctrine | Legacy Nodes (6) | Legacy Effect | C# Runtime Storage | C# Stat Support | C# Combat Support | Status |
|---|---|---|---|---|---|---|
| **Affliction** | 6 (VD: Servus Sanguinis, Mind Bender) | Lifesteal, Overheal, Petrify | Chỉ lưu 1 `AfflictionLevel` | ❌ Không | ❌ Không | Placeholder / Tiến độ ảo |
| **Control** | 6 (VD: Chill, Arcane Suppression) | Freeze, Threat, Magic Def | Chỉ lưu 1 `ControlLevel` | ❌ Không | ❌ Không | Placeholder / Tiến độ ảo |
| **Fortitude** | 6 (VD: Improved Health, Troll Res) | HP, Defense, Regen | Chỉ lưu 1 `FortitudeLevel` | ❌ Không | ❌ Không | Placeholder / Tiến độ ảo |
| **Grace** | 6 (VD: Exalted Dex, Divine Interv) | Dodge, Resurrection | Chỉ lưu 1 `GraceLevel` | ❌ Không | ❌ Không | Placeholder / Tiến độ ảo |
| **Illusion** | 6 (VD: False Life, Ephemeral) | Crit, False Life | Chỉ lưu 1 `IllusionLevel` | ❌ Không | ❌ Không | Placeholder / Tiến độ ảo |
| **Knowledge** | 6 (VD: Lore Master, Exalted Mana) | Int, Quest Points, Mana | Chỉ lưu 1 `KnowledgeLevel` | ❌ Không | ❌ Không | Placeholder / Tiến độ ảo |
| **Ruin** | 6 (VD: Genus Vampyri, Expose) | Crit Dmg, Ignor Armor | Chỉ lưu 1 `RuinLevel` | ❌ Không | ❌ Không | Placeholder / Tiến độ ảo |
| **War** | 6 (VD: Tactical, Relentless) | Con, Dex, Counterattack | Chỉ lưu 1 `WarLevel` | ❌ Không | ❌ Không | Placeholder / Tiến độ ảo |
| **Toàn bộ 40 Nodes** | Tất cả 40 Node (AbilityType) | 36 Hiệu ứng khác nhau | **BỊ XOÁ SỔ KHỎI CODE C#** | ❌ 0 / 40 | ❌ 0 / 40 | Missing Logic Data |

---

## 💡 Tổng kết Audit
Hệ thống Doctrine hiện tại của bản Rebuild chỉ là cái vỏ rỗng. Lỗi nghiêm trọng nhất không nằm ở việc chưa code hiệu ứng Combat, mà nằm ở việc **Thiết kế Data bị sai từ trong trứng nước**.

Bằng việc gộp 6 node lại thành 1 biến `Level` duy nhất ở `SaveData.cs`, hệ thống C# hiện tại đã tự tay bóp chết cơ chế nâng cấp từng Node. Trừ phi đập đi xây lại cấu trúc Save của `DoctrineService`, nếu không sẽ vĩnh viễn không thể port 40 Nodes của Legacy sang được.
