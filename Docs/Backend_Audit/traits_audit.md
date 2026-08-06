# 🧠 Báo cáo Audit: Trait System & Assignment Flow

**Dự án**: D:\Tinh\Rebuild_GuildMaster
**Ngày Audit**: 2026-08-06

---

## 1. Trait Inventory (Thống kê từ Java Legacy)
Toàn bộ Trait và PetAbility được định nghĩa cứng trong các Enum `Trait.java` và `PetAbility.java`.

### 🔴 Tổng quan (Tổng: 33 Traits & Abilities)
- **Common Traits (3):** `BOOKWORM`, `BRUTE`, `FERAL`
- **Premium/IAP Common Traits (3):** `BOOKWORM_PLUS`, `BRUTE_PLUS`, `FERAL_PLUS` (Chỉ dành cho Hero mua trong Cash Shop)
- **Rare Traits (14):** `EMPATHETIC`, `GIFTED`, `INTIMIDATING`, `FOCUSED`, `DRAGON_BLOOD`, `CURSED`, `REACTIVE`, `NOCTURNAL`, `MINDFUL`, `TROLL_BLOOD`, `NIMBLE`, `RUTHLESS`, `BLESSED`, `ALERT`
- **Pet Abilities (13):** `FIGHTER`, `HEALER`, `DECOY`, `OPPORTUNIST`, `MAGIC`, `SAVAGE`, `BRIGHT`, `EXPERIENCE`, `DROPS`, `COUNTERATTACK`, `LIFESTEAL`, `REGENERATION`, `BARRIER`

---

## 2. Trait Behavior (Trong Legacy Java)
- **Assign Flow:** Trong `Utils.java` (`newTavernVisitor`), game random roll 2 trait ĐỘC LẬP: `rollCommonTrait()` (tỷ lệ cao) và `rollRareTrait()` (tỷ lệ cực thấp). Một hero **có thể có cả 2 trait cùng lúc** (`traitCommon` và `traitRare`).
- **Stat Modifiers (Common):** Tính toán trong `Adventurer.calculateTotalStat`. Tăng cơ bản Int (Bookworm), Con (Brute), Dex (Feral).
- **Combat Modifiers (Rare):** Hardcode logic ngập tràn trong `Adventurer.java` (hàm tính Mana, Né, Máu, Phản đòn) và `Area.java`.

---

## 3. Data & Model Hiện Tại (Rebuild C#)
Phát hiện sai lệch nghiêm trọng về cấu trúc dữ liệu và xử lý logic!

### 🔴 Data Model
- Bản Legacy có `Trait traitCommon` và `Trait traitRare`.
- Bản C# (`CharacterRuntime.cs`) đã gộp thành 1 biến duy nhất: `public string Trait { get; set; }`. Điều này phá vỡ thiết kế gốc, khiến Hero mất khả năng sở hữu 2 Trait.

### 🔴 Assign Flow (`TavernService.cs`)
- Tại dòng 141-142, code C# viết:
  ```csharp
  trait = RollCommonTrait();
  if (trait == null) trait = RollRareTrait();
  ```
  => Sai logic hoàn toàn! Nghĩa là nếu quay trúng Common Trait thì người chơi vĩnh viễn bị chặn không được nhận Rare Trait, và ngược lại.

### 🔴 Stat Support (`CharacterService.cs`)
- Hàm `GetTraitMultiplier` chỉ code cho 4 Traits: `BRUTE`, `BOOKWORM`, `FERAL`, `NIMBLE`.
- Phát hiện rác/ảo giác (Hallucination): Dev trước đã tự chế thêm 2 Traits là `"STOUT"` và `"KEEN_EYED"` vào `GetTraitMultiplier`, dù bản gốc Java không hề có 2 trait này.

### 🔴 Pet System
- **Hoàn toàn biến mất.** Không có file `PetRuntime.cs`, không có `PetService.cs`. Toàn bộ 13 Pet Abilities hiện là trẻ mồ côi (Orphan) không có nơi bám víu.

---

## 4. Combat Integration (`CombatService.cs`)
Hiện tại Combat Service **BỎ QUA HOÀN TOÀN TẤT CẢ RARE TRAITS**. 
Hệ thống Combat hiện tại thiếu toàn bộ các Hooks (điểm neo sự kiện) để kích hoạt Trait.

- Thiếu hook `OnTurnStart`: Không thể hồi Mana cho `GIFTED` (+2 Mana) hay Hồi máu cho `TROLL_BLOOD`.
- Thiếu hook `OnTakeDamage`: Không thể Phản đòn (`REACTIVE` +10% Counterattack).
- Thiếu hook `OnDealDamage`: Không thể Hút máu (`CURSED` +15 Lifesteal) hay tăng Chí mạng (`RUTHLESS` +20% Crit Dmg).
- Thiếu hook `Targeting`: Không có khái niệm Threat để `INTIMIDATING` thu hút quái đập mình.

---

## 5. Bảng Reference Integrity (15 Mẫu Tiêu Biểu)

| Trait / Ability ID | Legacy Behavior | Hero Refs | Pet Refs | Runtime Storage | Stat Support | Combat Support | Status |
|---|---|---|---|---|---|---|---|
| `BRUTE` | + Constitution (Máu) | C# Tavern | - | String | Khớp 1.15 | - | Hỗ trợ 1 nửa |
| `BOOKWORM_PLUS` | + Nhiều Int (Cash Shop) | - | - | String | Thiếu | - | Mất Logic C# |
| `GIFTED` | +2 Mana Regen | C# Tavern | - | String | Khớp | Trống Không | Missing Combat |
| `CURSED` | +15 Lifesteal, giảm HP | C# Tavern | - | String | Thiếu | Trống Không | Missing Combat |
| `REACTIVE`| +10% Counterattack | C# Tavern | - | String | Thiếu | Trống Không | Missing Combat |
| `EMPATHETIC`| +20% Healing power | C# Tavern | - | String | Thiếu | Trống Không | Missing Combat |
| `NIMBLE` | Tăng Dodge | C# Tavern | - | String | Sai logic (Tăng Dex) | Trống Không | Wrong Logic |
| `TROLL_BLOOD`| Tự hồi HP / Turn | C# Tavern | - | String | Thiếu | Trống Không | Missing Combat |
| `RUTHLESS`| +20% Crit Damage | C# Tavern | - | String | Thiếu | Trống Không | Missing Combat |
| `FOCUSED` | Tăng Dodge khi mù | C# Tavern | - | String | Thiếu | Trống Không | Missing Combat |
| `ALERT` | Tăng Initiative | C# Tavern | - | String | Thiếu | Trống Không | Missing Combat |
| `BLESSED` | Giảm 8% bóng tối | C# Tavern | - | String | Thiếu | Trống Không | Missing Combat |
| `FIGHTER` (Pet) | Tăng Dmg & Threat | - | Legacy | Không | Không | Không | Orphan / Missing |
| `LIFESTEAL` (Pet)| Pet Hút máu | - | Legacy | Không | Không | Không | Orphan / Missing |
| `OPPORTUNIST` (Pet)| Tăng Crit Chance | - | Legacy | Không | Không | Không | Orphan / Missing |

---

## 💡 Tổng kết Audit
- **Mất mát Data nặng nề:** Việc gộp `traitCommon` và `traitRare` thành 1 biến string duy nhất trong C# đã bóp nát cơ chế random 2 traits của bản gốc.
- **Combat rỗng tuếch:** Giống như Skills, toàn bộ hiệu ứng Combat của Trait hiện tại chỉ là chuỗi string vô tác dụng vì `CombatService` không hề bắt các Hook (Events).
- **Thú cưng bốc hơi:** Pet System chưa được port một dòng code nào sang C#. Toàn bộ hệ thống liên quan đến Pet Trait/Ability tạm thời vô dụng.
