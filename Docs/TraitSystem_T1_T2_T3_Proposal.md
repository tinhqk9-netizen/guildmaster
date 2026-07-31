# 🧬 Hệ Thống Trait T1 → T2 → T3 — Bản Thiết Kế Đề Xuất

> Đề xuất thiết kế cho hệ thống Trait 3 tầng của Guild Master.
> Quy tắc: **Tất cả trait cũ = Tier 1 (T1)**. T2, T3 là trait mới, chỉ nhận được qua **Combo**, **Breeding** hoặc **Max-Level Roll**.

---

## 1. Nguyên Tắc Thiết Kế

| Tầng | Cách nhận | Cường độ | Độ hiếm |
|------|-----------|----------|---------|
| **T1** | Tuyển dụng (Tavern), roll lúc đạt cấp 45 | ×1.10 – ×1.15 | Thường |
| **T2** | Combo 2 trait T1 / Breeding / Roll cấp 45 | ×1.25 – ×1.35 | Hiếm |
| **T3** | Combo 3 trait T2 / Breeding / Roll cấp 45 | ×1.45 – ×1.60 | Huyền thoại |

**Quy tắc combo (cố định, không ngẫu nhiên):**
- `2 × T1 → 1 × T2` — mỗi cặp T1 chỉ ra **đúng 1** T2
- `3 × T2 → 1 × T3` — mỗi bộ ba T2 chỉ ra **đúng 1** T3
- Sau khi combo: trait tiêu thụ biến mất, **giải phóng slot** cho trait mới

**Quy tắc slot:** mỗi nhân vật tối đa **3 slot trait**.

---

## 2. Trait T1 — Toàn Bộ Trait Cũ (13 cái)

> Đây là 13 trait đã có trong code (`TavernService` + `GetTraitMultiplier`). Xếp tất cả về T1, đồng thời **bổ sung hiệu ứng cụ thể** cho những trait hiện đang để trống.

### 2.1 Nhóm chỉ số (đã có hiệu ứng)

| Trait | Hiệu ứng hiện tại | Giữ nguyên |
|-------|-------------------|------------|
| **BRUTE** | CON ×1.15 | ✅ |
| **STOUT** | CON ×1.15 | ✅ |
| **BOOKWORM** | INT ×1.15 | ✅ |
| **FERAL** | DEX ×1.15 | ✅ |
| **NIMBLE** | DEX ×1.15 | ✅ |
| **KEEN_EYED** | DEX ×1.10, INT ×1.05 | ✅ |

### 2.2 Nhóm đặc biệt (cần bổ sung hiệu ứng)

| Trait | Hiệu ứng đề xuất | Lý do |
|-------|------------------|-------|
| **EMPATHETIC** | Lượng hồi máu nhận vào +15% | Trait hỗ trợ |
| **GIFTED** | Kinh nghiệm nhận được +15% | Trait tăng trưởng |
| **INTIMIDATING** | Sát thương kẻ địch −8% | Trait khống chế |
| **FOCUSED** | Hồi mana +20% mỗi lượt | Trait pháp sư |
| **DRAGON_BLOOD** | MaxHP +15% | Trait trâu bò |
| **CURSED** | Sát thương gây ra +20%, Defense −10% | Trait đánh đổi (double-edged) |
| **REACTIVE** | 15% cơ hội phản đòn khi bị đánh | Trait chiến thuật |

> Ghi chú: BRUTE/STOUT và FERAL/NIMBLE là **cặp trait trùng hiệu ứng** — đây chính là "nguyên liệu" cho combo T2 đặc trưng theo class (xem mục 3).

---

## 3. Trait T2 — Trait Mới (9 cái, từ Combo 2×T1)

> Mỗi T2 có **đúng 1 công thức combo cố định**. Hiệu ứng = bản nâng cấp của T1 gốc.

| # | Trait T2 | Công thức Combo (2×T1) | Hiệu ứng |
|---|----------|------------------------|----------|
| 1 | **TITAN** 🪨 | `BRUTE + STOUT` | CON ×1.30, MaxHP +10% |
| 2 | **SAGE** 📜 | `BOOKWORM + KEEN_EYED` | INT ×1.30, MDEF +10% |
| 3 | **WINDSTALKER** 💨 | `FERAL + NIMBLE` | DEX ×1.30, Initiative +10 |
| 4 | **PRODIGY** ⭐ | `GIFTED + BOOKWORM` | EXP +30%, INT ×1.15 |
| 5 | **HEALER** ✨ | `EMPATHETIC + FOCUSED` | Hồi máu nhận +30%, mana regen +20% |
| 6 | **DRAGONHEART** 🐉 | `DRAGON_BLOOD + BRUTE` | MaxHP +30%, CON ×1.15 |
| 7 | **SHADOW** 🌑 | `CURSED + NIMBLE` | Sát thương +25%, DEX ×1.15 |
| 8 | **OVERLORD** 👑 | `INTIMIDATING + STOUT` | Kẻ địch −15% sát thương, CON ×1.15 |
| 9 | **BERSERKER** 🔥 | `REACTIVE + BRUTE` | Phản đòn 25%, CON ×1.15 |

**Thiết kế combo hợp lý:**
- Trait cùng hệ (BRUTE+STOUT → TITAN) → T2 thuần chỉ số
- Trait hệ + trait đặc biệt (GIFTED+BOOKWORM → PRODIGY) → T2 lai (tăng trưởng + chỉ số)
- Trait đặc biệt + trait đặc biệt (EMPATHETIC+FOCUSED → HEALER) → T2 hỗ trợ
- Trait đánh đổi (CURSED) → T2 vẫn giữ tính đánh đổi nhưng mạnh hơn

---

## 4. Trait T3 — Trait Huyền Thoại (4 cái, từ Combo 3×T2)

> Mỗi T3 có **đúng 1 công thức combo cố định** từ 3 trait T2.

| # | Trait T3 | Công thức Combo (3×T2) | Hiệu ứng |
|---|----------|------------------------|----------|
| 1 | **GOD_OF_WAR** ⚡ | `TITAN + BERSERKER + DRAGONHEART` | CON ×1.50, MaxHP +30%, sát thương +20% |
| 2 | **ARCHMAGE** 🔮 | `SAGE + PRODIGY + HEALER` | INT ×1.50, mana regen +50%, MDEF +15% |
| 3 | **PHANTOM** 🌪️ | `WINDSTALKER + SHADOW + PRODIGY` | DEX ×1.50, sát thương +25%, Initiative +15 |
| 4 | **GUARDIAN** 🛡️ | `TITAN + OVERLORD + DRAGONHEART` | MaxHP +45%, DEF ×1.40, kẻ địch −15% sát thương |

**Nguyên tắc thiết kế T3:**
- Đúng 1 trụ cột rõ ràng: War / Mage / Assassin / Tank
- Chỉ số chính ×1.50 (mạnh hơn T2 ×1.30 rõ rệt)
- Có ít nhất 2 hiệu ứng phụ để tạo cảm giác "huyền thoại"
- KHÔNG kết hợp trait đánh đổi (CURSED/SHADOW) vào T3 để tránh quá mạnh mất cân bằng

---

## 5. Bảng Tỉ Lệ Xuất Hiện (Xác Suất)

### 5.1 Roll khi tuyển dụng (Tavern — hiện tại)
| Kết quả | Tỉ lệ | Trait |
|---------|-------|-------|
| Không có trait | 50% | — |
| Trait T1 thường | 40% | BRUTE / BOOKWORM / FERAL / STOUT / NIMBLE / KEEN_EYED |
| Trait T1 đặc biệt | 10% | EMPATHETIC / GIFTED / INTIMIDATING / FOCUSED / DRAGON_BLOOD / CURSED / REACTIVE |

> **Giữ nguyên** tỉ lệ này ở T1. T2/T3 KHÔNG xuất hiện khi tuyển dụng (chỉ qua combo/breeding/roll cấp 45).

### 5.2 Roll khi đạt Max Level 45 (chọn 1 trong 3)
| Kết quả | Tỉ lệ mỗi ô |
|---------|-------------|
| Trait T1 | 70% |
| Trait T2 | 24% |
| Trait T3 | 6% |

**Cơ chế:** khi nhân vật đạt cấp 45 → hiện **3 lựa chọn** (random theo bảng trên) → người chơi **chọn 1** → điền vào slot trống. Nếu đủ 3 slot: cho phép **thay thế 1 trait cũ** (mất trait cũ) hoặc từ chối.

### 5.3 Thừa hưởng khi Breeding
| Tier trait bố/mẹ | Tỉ lệ truyền cho con |
|-------------------|----------------------|
| T1 | 50% |
| T2 | 30% |
| T3 | 15% |

**Cơ chế:** con nhận tối đa 2 trait (1 từ bố + 1 từ mẹ, roll độc lập). Mỗi tier roll riêng theo bảng trên. Nếu cả 2 roll trượt → con không có trait nào.

---

## 6. Luồng Thao Tác Người Chơi

### 6.1 Combo trait
```
Mở màn hình Nhân Vật → Chọn trait → "Kết hợp"
  → Chọn 2 trait T1 đúng công thức → Xác nhận
  → 2 trait biến mất → nhận 1 trait T2 → slot giải phóng 1 chỗ
```

### 6.2 Breeding
```
Mở màn hình Breeding → Chọn bố + mẹ (2 nhân vật)
  → Trả phí (vàng/gem) → Chờ thời gian sinh (VD: 2 giờ)
  → Nhận "đứa con" (nhân vật mới cấp 1)
  → Con có class 50/50 của bố/mẹ, roll trait theo bảng 5.3
```

### 6.3 Max-Level Roll
```
Nhân vật đạt cấp 45 → Popup "Chọn 1 trong 3 trait"
  → Roll theo bảng 5.2 → người chơi chọn
```

---

## 7. Tác Động Code (Danh Sách File Cần Sửa)

> Theo nguyên tắc: **KHÔNG sửa file gốc** — copy file, sửa bản copy, đặt tên có hậu tố `_MinhEdit`.

| File gốc | Thay đổi cần thiết |
|----------|---------------------|
| `CharacterSaveData.cs` (trong SaveData.cs) | `string Trait` → `List<string> Traits` (3 slots) + migration save cũ |
| `CharacterRuntime.cs` | `string Trait` → `List<string> Traits` |
| `CharacterService.cs` | `GetTraitMultiplier` → `GetTraitsMultiplier` (duyệt 3 slots, stack hiệu ứng) |
| `TavernService.cs` | `RollCommonTrait`/`RollRareTrait` → giữ nguyên (T1), bỏ logic cũ nếu cần |
| `FormulaService.cs` | Thêm hàm: `TraitTierMultiplier(tier)`, `GetMaxLevelTraitRollTable()`, `GetBreedInheritChance(tier)` |
| `UI/Character/CharacterScreen.cs` | Hiển thị 3 slot trait + nút "Kết hợp" |
| MỚI: `TraitComboService.cs` | Tra cứu công thức combo, thực thi combo, giải phóng slot |
| MỚI: `BreedingService.cs` | Chọn bố mẹ, roll thừa hưởng, tạo con |
| MỚI: `TraitDefinition.cs` | Định nghĩa trait: id, tier, stat multipliers, special effects |
| MỚI: `trait_data.json` (StreamingAssets) | Data: 13 T1 + 9 T2 + 4 T3 + bảng combo |

---

## 8. Cân Bằng & Kiểm Tra

- [ ] **T1 (×1.15) → T2 (×1.30) → T3 (×1.50)**: chênh lệch đủ rõ, không quá nhảy vọt
- [ ] Combo cần **2 nhân vật** (mỗi người 1 trait) hoặc **1 nhân vật 2 slot** — khuyến khích xây nhiều nhân vật
- [ ] T3 **chỉ có 4 cái** → mục tiêu dài hạn, tránh spam
- [ ] CURSED/SHADOW giữ tính đánh đổi → không tạo T3 từ chúng (tránh bất cân bằng)
- [ ] Breeding có **chi phí + thời gian chờ** → không phá vỡ kinh tế game
- [ ] Save cũ (1 trait string) → migration tự động đưa trait cũ vào slot 0

---

## 9. Tóm Tắt Số Lượng

| Tier | Số trait | Cách nhận |
|------|----------|-----------|
| T1 | 13 (giữ nguyên) | Tuyển dụng, roll cấp 45 |
| T2 | 9 (mới) | Combo 2×T1, breeding, roll cấp 45 |
| T3 | 4 (mới) | Combo 3×T2, breeding, roll cấp 45 |
| **Tổng** | **26** | |

---

> *Bản thiết kế này giữ 100% trait cũ làm T1, thêm 13 trait mới (9 T2 + 4 T3) với công thức combo cố định, xác suất rõ ràng và luồng thao tác cụ thể. Sẵn sàng triển khai khi bạn duyệt.*
