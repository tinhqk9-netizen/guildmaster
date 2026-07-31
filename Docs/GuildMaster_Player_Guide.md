# 🏰 Guild Master — Cẩm Nang Người Chơi

> Chào mừng bạn đến với Guild Master! Đây là game idle dungeon nơi bạn xây dựng và quản lý một guild (hội) gồm các anh hùng, gửi họ vào hầm ngục để chiến đấu, thu thập chiến lợi phẩm, và ngày càng lớn mạnh hơn.

---

## 📖 Mục Lục

1. [Khởi Đầu](#1-khởi-đầu)
2. [Tiền Tệ & Nâng Cấp](#2-tiền-tệ--nâng-cấp)
3. [Nhân Vật (Adventurers)](#3-nhân-vật-adventurers)
4. [Chỉ Số Của Nhân Vật](#4-chỉ-số-của-nhân-vật)
5. [Trang Bị (Equipment)](#5-trang-bị-equipment)
6. [Túi Đồ (Inventory)](#6-túi-đồ-inventory)
7. [Tửu Quán (Tavern) — Tuyển Dụng](#7-tửu-quán-tavern--tuyển-dụng)
8. [Workshop — Chế Tạo (Crafting)](#8-workshop--chế-tạo-crafting)
9. [Chợ (Merchant) — Mua Bán](#9-chợ-merchant--mua-bán)
10. [Hầm Ngục (Dungeon)](#10-hầm-ngục-dungeon)
11. [Chiến Đấu (Combat)](#11-chiến-đấu-combat)
12. [Chiến Lợi Phẩm & Rương (Loot)](#12-chiến-lợi-phẩm--rương-loot)
13. [Nhiệm Vụ (Quest)](#13-nhiệm-vụ-quest)
14. [Học Thuyết (Doctrine)](#14-học-thuyết-doctrine)
15. [Thăng Cấp (Promotion / Ascension)](#15-thăng-cấp-promotion--ascension)
16. [Tiến Trình Khi Offline](#16-tiến-trình-khi-offline)
17. [Các Loại Vật Phẩm](#17-các-loại-vật-phẩm)
18. [Mẹo & Chiến Thuật](#18-mẹo--chiến-thuật)

---

## 1. Khởi Đầu

Khi bắt đầu game, bạn sẽ có:
- **500 vàng** làm vốn
- **1 nhân viên Footman** cơ bản
- Một số vật phẩm và nguyên liệu căn bản

**Giao diện chính:**
- **HUD (thanh trên cùng)**: hiển thị số vàng, số gem, và các nút điều hướng đến các màn hình khác
- Các màn hình chính: Tửu Quán → Kho Đồ → Nhân Vật → Hầm Ngục → Chế Tạo → Chợ → Nhiệm Vụ → Cài Đặt

**Luồng chơi cơ bản:**
1. Vào Tửu Quán → Tuyển dụng nhân vật mới
2. Vào Kho Đồ → Trang bị vũ khí, giáp cho nhân vật
3. Vào Dungeon → Chọn hầm ngục, gửi nhân vật đi đánh
4. Thu thập loot → Chế tạo đồ mới / Bán lấy vàng
5. Nâng cấp công trình → Mở rộng sức chứa
6. Lặp lại!

---

## 2. Tiền Tệ & Nâng Cấp

### Tiền tệ
| Loại | Dùng để |
|------|---------|
| **Vàng (Money)** | Nâng cấp, mua hàng, chế tạo — kiếm từ dungeon, bán đồ, nhiệm vụ |
| **Gem (Gems)** | Mua hàng đặc biệt, nâng cấp VIP — kiếm từ nhiệm vụ và sự kiện |

### Các công trình có thể nâng cấp

| Công trình | Tác dụng | Giá tăng dần theo |
|------------|----------|-------------------|
| 🛏️ **Ký túc xá (Quarters)** | Tăng sức chứa nhân vật tối đa | Bảng giá cố định 23 bậc, từ 5 vàng → 10 triệu |
| 🍺 **Sức chứa Tửu Quán** | Càng nhiều khách cùng lúc | `3^cấp × 5000` |
| ⏱️ **Tốc độ khách Tửu Quán** | Khách đến nhanh hơn | `1.7^cấp × 200` |
| 📦 **Kho đồ (Storage)** | Tăng sức chứa túi đồ | Giá lũy tiến theo vùng (không làm tròn!) |
| ⚒️ **Workshop Queue** | Chế tạo được nhiều món cùng lúc | `4.5^cấp × 20` |
| ⏱️ **Tốc độ Workshop** | Chế tạo nhanh hơn | `1.7^cấp × 10` |
| 🏪 **Chợ (Market Listings)** | Bán được nhiều món cùng lúc | `4.5^cấp × 20` |
| ⏱️ **Tốc độ Chợ** | Bán nhanh hơn | `1.7^cấp × 10` |

**Mẹo:** Nâng storage lên trước vì giá không bị làm tròn nên tương đối rẻ ở đầu game.

### Các gói mua (Purchase Flags)
Có 5 gói mua bằng gem ảnh hưởng đến sức chứa và công thức:
- Starter Pack, Adventurer Pack, Merchant Pack, Imperial Vanguard, Unholy Crusade

---

## 3. Nhân Vật (Adventurers)

### Class cơ bản
Khi tuyển dụng từ Tửu Quán, nhân vật sẽ thuộc một trong 4 class:

| Class | Thiên hướng | Vũ khí | Giáp |
|-------|-------------|--------|------|
| ⚔️ **Footman** | Cân bằng, thiên phòng thủ | Kiếm/Khiên | Giáp nặng |
| 🗡️ **Rogue** | Nhanh nhẹn, sát thương vật lý | Dao găm | Giáp nhẹ |
| 🏹 **Archer** | Tầm xa, tốc độ cao | Cung | Giáp nhẹ |
| 🔮 **Apprentice** | Phép thuật, sát thương phép | Gậy/Pháp trượng | Áo choàng |

### Cấp độ (Level)
- Nhân vật tăng cấp khi tích lũy đủ kinh nghiệm
- **Công thức kinh nghiệm cần cho cấp tiếp theo:**
  - Càng lên cao càng cần nhiều exp
  - Công thức gốc: dùng `pow(cấp_hiện_tại, 1.4)` nhân với hệ số
  - Adventurer cần gấp **đôi** so với Pet
  - Ở cấp cao, lượng exp cần được làm tròn xuống (bỏ số lẻ)

### Thăng cấp (Ascension / Promotion)
Khi nhân vật đủ cấp và có vật phẩm yêu cầu, bạn có thể **thăng cấp**:
- ✅ Level reset về 1
- ✅ Chỉ số được nhân với hệ số (x1.1, x1.2, ... tùy bậc)
- ✅ Có thể thăng cấp nhiều lần (Bronze → Silver → Gold → ...)
- ❌ Kinh nghiệm reset về 0

**Càng thăng cấp cao, càng mạnh!** Nhưng nhớ rằng nhân vật sẽ yếu tạm thời ngay sau khi thăng (vì level về 1).

### Đặc tính (Trait)
Khi tuyển dụng, nhân vật có thể có đặc tính đặc biệt:
- **Thường:** BOOKWORM (tăng INT), BRUTE (tăng CON), FERAL (tăng DEX)
- **Hiếm:** Các đặc tính mạnh hơn với hệ số nhân cao hơn

### Skill
Mỗi nhân vật có:
- **Active Skill (kỹ năng chủ động)**: dùng trong combat, tốn mana
- **Passive Skill (kỹ năng bị động)**: luôn có tác dụng

---

## 4. Chỉ Số Của Nhân Vật

| Chỉ số | Viết tắt | Ảnh hưởng |
|--------|----------|-----------|
| 💪 **Constitution (CON)** | Thể lực | Tăng sát thương vật lý, tăng HP |
| 🧠 **Intelligence (INT)** | Trí tuệ | Tăng sát thương phép |
| 💨 **Dexterity (DEX)** | Nhanh nhẹn | Tăng tốc độ đánh, né tránh |
| ❤️ **MaxHP** | Máu tối đa | Càng nhiều càng trâu |
| 🛡️ **Defense (DEF)** | Giáp vật lý | Giảm sát thương vật lý |
| 🔮 **Magic Defense (MDEF)** | Giáp phép | Giảm sát thương phép |
| ⚡ **Tốc độ (Initiative)** | - | Quyết định ai đánh trước |

### Cách chỉ số được tính

**Công thức căn bản:**
```
Chỉ số cuối = (Chỉ số nền × Hệ số thăng cấp + Đồ trang bị) × Hệ số đặc tính
```

**Những điều cần biết:**
- **CON, INT, DEX, HP** — được nhân với hệ số thăng cấp (càng thăng cấp càng tăng mạnh)
- **DEF, MDEF** — KHÔNG được nhân với hệ số thăng cấp (chỉ tăng từ đồ và đặc tính)
- **Đồ trang bị** cộng thẳng vào chỉ số trước khi nhân đặc tính
- **Potion** uống vào sẽ cộng vĩnh viễn vào chỉ số (theo mapping: CON=slot0, DEX=slot1, INT=slot2, HP*5=slot3, DEF=slot4, MDEF=slot5)

### Ảnh hưởng từ Học Thuyết (Doctrine)
- **War**: +2 CON, +2 DEX mỗi cấp
- **Knowledge**: +2 INT mỗi cấp
- **Fortitude**: +15 HP, +3 DEF mỗi cấp
- **Ruin**: +25 HP mỗi cấp
- **Grace**: Cấp 2 trở lên → nhân đôi hiệu ứng của Accessory
- **Illusion**: +3 MDEF mỗi cấp

---

## 5. Trang Bị (Equipment)

### Các slot trang bị
| Slot | Loại đồ | Ví dụ |
|------|---------|-------|
| ⚔️ **Vũ khí (Weapon)** | Kiếm, Dao, Cung, Gậy, Súng, ... | Mỗi class chỉ xài được loại vũ khí tương ứng |
| 👕 **Giáp (Armor)** | Giáp nhẹ, Giáp nặng, Áo choàng, ... | Tùy class, mỗi class mặc được loại giáp riêng |
| 💍 **Phụ kiện (Accessory)** | Nhẫn, Bùa, Mặt dây | Ai cũng đeo được, bonus x2 nếu Grace≥2 |

### Quy tắc trang bị
- **Ràng buộc class**: Footman không thể xài cung, Apprentice không thể mặc giáp nặng
- **"Generic"** là loại vũ khí ai cũng xài được
- **Khi trang bị**, đồ vẫn nằm trong túi đồ nhưng bị **khóa** (lock) — không bị bán nhầm
- **Khi tháo đồ**, đồ được mở khóa
- **Khi bán/xóa đồ**, nếu đồ đó đang được trang bị, hệ thống tự động clear slot cho nhân vật

---

## 6. Túi Đồ (Inventory)

- **Sức chứa ban đầu**: 35 chỗ
- **Có thể nâng cấp** qua Storage (lên tối đa 80+)
- **Vật phẩm stack được**: Nguyên liệu (Material) và Thuốc (Consumable)
- **Vật phẩm không stack**: Vũ khí, Giáp, Phụ kiện — mỗi món 1 slot

### Các thao tác trong túi đồ
- **Dùng (Use)**: với vật phẩm tiêu hao (thuốc, food)
- **Trang bị (Equip)**: chuyển đến màn hình nhân vật
- **Khóa/Mở khóa (Lock)**: tránh bán nhầm
- **Bán (Sell)**: gửi lên chợ, bán trong 20 giây

---

## 7. Tửu Quán (Tavern) — Tuyển Dụng

Tửu Quán là nơi bạn kiếm nhân vật mới!

### Cơ chế
- **Mặc định**: khách mới đến mỗi **8 tiếng** (có thể giảm bằng nâng cấp)
- **Sức chứa Tửu Quán**: bao nhiêu khách có thể đợi cùng lúc
- **Ký túc xá (Quarters)**: sức chứa tối đa nhân vật bạn có thể sở hữu
- **Tuyển dụng**: chọn khách → trả vàng → nhân vật gia nhập guild

### Đặc điểm của khách
- Class được random: 25% mỗi loại (Footman, Rogue, Archer, Apprentice)
- Có thể có **trait đặc biệt** (tỉ lệ random)
- Cấp độ và chỉ số phụ thuộc vào cấp độ Tửu Quán

### Các nâng cấp Tửu Quán
1. **Sức chứa** — nhiều khách hơn cùng lúc, giá: `3^cấp × 5000`
2. **Thời gian** — khách đến nhanh hơn, giá: `1.7^cấp × 200`
3. **Ký túc xá** — nuôi nhiều nhân vật hơn

---

## 8. Workshop — Chế Tạo (Crafting)

Workshop cho phép bạn biến nguyên liệu thành đồ mới!

### Cách chế tạo
1. Chọn **công thức (Recipe)** — mỗi công thức có nguyên liệu yêu cầu
2. Hệ thống kiểm tra:
   - ✅ Recipe tồn tại
   - ✅ Đủ nguyên liệu
   - ✅ Queue còn chỗ
3. Nguyên liệu bị tiêu thụ ngay
4. Sản phẩm vào **hàng đợi (queue)**, chế tạo trong **10 giây** (mặc định)
5. Khi hoàn thành, qua mục **Completed** để nhận đồ

### Nâng cấp Workshop
- **Queue**: chế tạo nhiều món cùng lúc (giá: `4.5^cấp × 20`)
- **Tốc độ**: chế tạo nhanh hơn (giá: `1.7^cấp × 10`)

---

## 9. Chợ (Merchant) — Mua Bán

### Mua hàng
- **Hàng thường (Regular Stock)**: random từ dungeon hiện tại
- **Hàng đặc biệt (Special Stock)**: đồ hiếm, tỉ lệ thấp
- Có thể mua bằng **Vàng** hoặc **Gem** (tùy món)
- Mua xong, đồ vào túi — hàng biến khỏi quầy

### Bán hàng
- Chọn đồ trong túi → **Sell**
- Đồ lên **chợ (market listing)**, bán trong 20 giây
- Hết thời gian → nhận tiền

---

## 10. Hầm Ngục (Dungeon)

Đây là nội dung chính của game!

### Cách hoạt động
1. **Chọn dungeon** từ danh sách (mở dần theo tiến trình)
2. **Chọn party** (tối đa 5 nhân vật?)
3. **Start** → party bắt đầu thám hiểm

### Luồng dungeon
- Party **tự động đi** qua dungeon, mỗi bước là 1 tick
- Có thể gặp: **quái vật**, **bẫy**, **sự kiện**
- **Gặp quái** → vào combat turn-based
- **Hết quái** → nhận loot, đi tiếp
- Lặp lại cho đến khi:
  - 🏆 **Thắng**: clear dungeon, nhận thưởng lớn
  - 💀 **Thua**: mất tiến trình (về mốc 250 nếu đã đi xa)
  - 🏃 **Rút lui**: về 0, giữ đồ đã nhặt

### Giới hạn
- **Tối đa 400 turn** trong một dungeon
- Nếu đánh nhau quá lâu → tự động kết thúc
- Mỗi dungeon có thể clear nhiều lần

### Chain Gating
Dungeon có thể yêu cầu clear dungeon trước đó mới mở khóa. VD: phải clear "Khu rừng tối" mới vào được "Hang Sâu".

---

## 11. Chiến Đấu (Combat)

### Lượt đánh (Turn-based)
Mỗi lượt chiến đấu diễn ra như sau:

1. **Kiểm tra kết thúc**: còn ai sống không?
2. **Sắp xếp thứ tự**: entity có tốc độ (initiative) cao nhất đánh trước — nếu bằng nhau thì DEX cao hơn đánh trước
3. **Hồi phục đầu turn**: entity được hồi `Regen` HP
4. **Tăng Mana**: entity có skill nhận mana, đủ 100 → xài skill
5. **Chọn mục tiêu**: tự động chọn kẻ địch
6. **Tấn công**: roll sát thương trong khoảng [Min, Max], trừ defense
7. **Kiểm tra kết thúc** lại

### Sát thương
- **Sát thương vật lý**: giảm bởi Defense (DEF)
- **Sát thương phép**: giảm bởi Magic Defense (MDEF)
- **Sát thương cơ bản**: `min + random() × (max - min)`

### Kết quả
- **Victory**: hết quái → loot
- **Defeat**: hết adventurer → mất tiến trình

### Skill trong combat
- Entity có Active Skill sẽ tích mana mỗi turn
- Khi mana = 100 → xài skill (mana về 0)
- Skill có thể: gây sát thương, hồi máu, tăng buff, gây debuff

---

## 12. Chiến Lợi Phẩm & Rương (Loot)

### Quái rơi đồ thế nào?
- Mỗi enemy có **drop table** riêng
- **Cơ chế roll**: tỉ lệ trên thang 1000 — nếu tổng các tỉ lệ < 1000 thì có % không rơi gì
- VD: Quái A có: ItemX (tỉ lệ 300), ItemY (tỉ lệ 200) → 50% còn lại là không rơi gì

### Rương (Chest)
- Loot tạm thời được giữ trong **rương**
- **Sức chứa rương**: 2000 stack (mặc định), 3000 (nếu mua Merchant Pack)
- Khi đầy rương → không nhặt thêm được nữa
- **Collect**: nhấn nút để chuyển loot vào túi đồ

---

## 13. Nhiệm Vụ (Quest)

Nhiệm vụ cho bạn mục tiêu và phần thưởng!

### Cách hoạt động
- Nhiệm vụ có: **mục tiêu** (target), **tiến trình** (progress), **độ hiếm** (rarity 1-10)
- Mỗi độ hiếm có mục tiêu khác nhau (càng hiếm càng khó)
- Khi đạt mục tiêu → **Completed** → nhận thưởng

### Loại nhiệm vụ
Nhiệm vụ được trigger từ các hành động trong game:
- Giết X quái trong dungeon Y
- Chế tạo X món đồ
- Bán X món ở chợ
- Clear dungeon Z
- Thu thập X vàng

### Học thuyết (Doctrine) từ Quest
Phần thưởng quest thường là **điểm Học Thuyết** — dùng để tăng chỉ số vĩnh viễn cho tất cả nhân vật.

---

## 14. Học Thuyết (Doctrine)

Có **8 học thuyết**, mỗi học thuyết tăng chỉ số cho toàn bộ guild:

| Học thuyết | Buff cho toàn guild |
|------------|---------------------|
| ⚔️ **War** | +2 CON, +2 DEX mỗi cấp |
| 📚 **Knowledge** | +2 INT mỗi cấp |
| 🏋️ **Fortitude** | +15 HP, +3 DEF mỗi cấp |
| 💀 **Ruin** | +25 HP mỗi cấp |
| ✨ **Grace** | Cấp 2: nhân đôi hiệu ứng Accessory |
| 🔮 **Illusion** | +3 MDEF mỗi cấp |
| 🌑 **Affliction** | (chưa port đầy đủ) |
| 🌀 **Control** | (chưa port đầy đủ) |

Mỗi học thuyệt có **cấp độ** và **điểm tích lũy** hướng tới cấp tiếp theo.
**Công thức**: Cần `cấp × 3 + 4` sao để lên cấp tiếp.

**Mẹo**: War và Fortitude là ưu tiên hàng đầu — tăng CON/DEX/HP/DEF cho toàn đội!

---

## 15. Thăng Cấp (Promotion / Ascension)

Khi nhân vật đủ cấp, bạn có thể cho họ **thăng cấp (ascend)**:

### Yêu cầu
- ✅ Đạt cấp yêu cầu (VD: cấp 10 để lên Bronze)
- ✅ Có vật phẩm yêu cầu (VD: 1 Huy hiệu Đồng)
- ✅ Chưa thăng cấp bậc đó

### Kết quả
- **Level reset** về 1
- **Exp reset** về 0
- **Hệ số nhân** tăng (VD: x1.0 → x1.1 → x1.2 → ...)
- **Chỉ số nền** được nhân với hệ số mới

### Các bậc thăng cấp
- Bậc 0: Ban đầu (hệ số x1.0)
- Bậc 1: Bronze (x1.1)
- Bậc 2: Silver (x1.2)
- Bậc 3: Gold (x1.3)
- ...

**Chiến thuật:** Thăng cấp sớm để nhân vật mạnh hơn về lâu dài, nhưng sẽ yếu tạm thời vì level về 1.

---

## 16. Tiến Trình Khi Offline

Game vẫn chạy khi bạn tắt! Khi bạn quay lại:

### Những gì tiến triển khi offline
- ✅ **Workshop**: đồ trong queue vẫn được chế tạo
- ✅ **Chợ**: đồ bán vẫn được bán
- ✅ **Tửu Quán**: thời gian vẫn trôi (khách mới có thể đến)
- ✅ **Dungeon**: vẫn tự động đánh! (tick mỗi giây, tối đa 12 tiếng)

### Giới hạn
- Offline tối đa **12 tiếng** — quá 12 tiếng không tính thêm

---

## 17. Các Loại Vật Phẩm

| Loại | Ví dụ | Tính chất |
|------|-------|-----------|
| ⚔️ **Weapon** (Vũ khí) | Kiếm, Dao, Cung, Gậy | Trang bị, không stack |
| 👕 **Armor** (Giáp) | Giáp nhẹ, Giáp nặng, Áo | Trang bị, không stack |
| 💍 **Accessory** (Phụ kiện) | Nhẫn, Bùa | Trang bị, không stack |
| 🧪 **Consumable** (Thuốc) | HP Potion, EXP Potion | Dùng 1 lần, stack được |
| 📦 **Material** (Nguyên liệu) | Sắt, Gỗ, Vải, Đá | Làm đồ, stack được |
| 🥩 **Pet Food** (Thức ăn pet) | Thịt, Cá | Cho pet ăn |
| 📐 **Blueprint** (Bản vẽ) | Công thức chế tạo | Mở khóa craft mới |

### Độ hiếm (Rarity 1-5)
- Càng hiếm → chỉ số càng cao → càng đắt
- Độ hiếm ảnh hưởng đến màu sắc tên và khung

---

## 18. Mẹo & Chiến Thuật

### 🥇 Ưu tiên đầu game
1. **Nâng Storage** lên cấp 10-15 — rẻ, tăng nhiều slot
2. **Tuyển 3-4 nhân vật** — có đội hình đầy đủ trước
3. **Nâng War Doctrine** — CON/DEX cho toàn đội
4. **Farm dungeon dễ** — kiếm nguyên liệu, loot, exp

### ⚔️ Chiến thuật combat
- Cân bằng đội hình: 1 tank (Footman) + 1 dps (Rogue/Archer) + 1 phép (Apprentice)
- Trang bị vũ khí trước, giáp sau
- Defense và MDEF không tăng theo thăng cấp → cần đồ tốt

### 💰 Kiếm tiền
- Bán nguyên liệu thừa qua Chợ (chờ 20 giây)
- Chế tạo đồ bán được giá hơn bán nguyên liệu
- Làm nhiệm vụ → nhận vàng + điểm học thuyết

### 📈 Tăng sức mạnh lâu dài
- **Doctrine** là khoản đầu tư dài hạn tốt nhất (buff toàn đội vĩnh viễn)
- **Thăng cấp** nhân vật chủ lực trước
- **Nâng Tavern** để có nhiều nhân vật hơn

### ❌ Sai lầm cần tránh
- Không nâng Storage sớm → nhanh đầy túi
- Không thăng cấp → nhân vật yếu về late game
- Bán nhầm đồ đang trang bị (đã được khóa để tránh)
- Để Workshop/Chợ queue rỗng khi offline

---

> *Chúc bạn xây dựng guild hùng mạnh nhất!* 🏰⚔️
