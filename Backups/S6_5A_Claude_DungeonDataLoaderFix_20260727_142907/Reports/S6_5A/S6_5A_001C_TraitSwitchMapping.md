# S6.5A-001C — Trait Switch Mapping (Evidence)

**Ngày:** 2026-07-27
**Mục đích:** Chứng minh bảng trait multiplier trong `Adventurer.calculateTotalStat(int)` được giải mã từ bytecode gốc, **không phải suy đoán**.

---

## 1. Trait enum — source

**Nguồn Java (JADX):** `D:\Tinh\Guild Master - Idle Dungeons\sources\it\paranoidsquirrels\idleguildmaster\storage\data\entities\adventurers\Trait.java`

Thứ tự **khai báo** (= ordinal thật), dòng 7–26:
| ordinal | Trait | Nhóm |
|---:|---|---|
| 0 | `BOOKWORM` | common |
| 1 | `BRUTE` | common |
| 2 | `FERAL` | common |
| 3 | `BOOKWORM_PLUS` | common (nâng cấp) |
| 4 | `BRUTE_PLUS` | common (nâng cấp) |
| 5 | `FERAL_PLUS` | common (nâng cấp) |
| 6 | `EMPATHETIC` | rare |
| 7 | `GIFTED` | rare |
| 8 | `INTIMIDATING` | rare |
| 9 | `FOCUSED` | rare |
| 10 | `DRAGON_BLOOD` | rare |
| 11 | `CURSED` | rare |
| 12 | `REACTIVE` | rare |
| 13 | `NOCTURNAL` | rare |
| 14 | `MINDFUL` | rare |
| 15 | `TROLL_BLOOD` | rare |
| 16 | `NIMBLE` | rare |
| 17 | `RUTHLESS` | rare |
| 18 | `BLESSED` | rare |
| 19 | `ALERT` | rare |

> ⚠️ **Cảnh báo khi tự verify:** nếu liệt kê field tĩnh trực tiếp từ DEX, danh sách trả về **theo thứ tự alphabet** (`$VALUES, ALERT, BLESSED, BOOKWORM, …`) — **KHÔNG phải ordinal**. Chỉ thứ tự khai báo trong `Trait.java` mới là ordinal đúng.
>
> Tuy nhiên **điều này không ảnh hưởng kết quả**: `$SwitchMap` được xây bằng `Trait.X.ordinal()` làm chỉ số, nên mapping *case number ↔ tên trait* đọc trực tiếp được từ `<clinit>` mà **không cần biết ordinal là số mấy**.

---

## 2. `Adventurer$1.$SwitchMap` — source & bằng chứng raw

**Class:** `Lit/paranoidsquirrels/idleguildmaster/storage/data/entities/adventurers/Adventurer$1;`
**Method:** `<clinit>` · **DEX:** `classes3.dex`

Smali thực tế (trích nguyên văn, chỉ lọc dòng liên quan):
```smali
0x0000: invoke-static  Trait;->values()[Trait;
0x0009: sget-object v1, Trait;->BOOKWORM       0x000f: const/4 v2, 1   0x0010: aput v2, v0, v1
0x0014: sget-object v1, Trait;->FERAL          0x001a: const/4 v2, 2   0x001b: aput v2, v0, v1
0x001f: sget-object v1, Trait;->BRUTE          0x0025: const/4 v2, 3   0x0026: aput v2, v0, v1
0x002a: sget-object v1, Trait;->BOOKWORM_PLUS  0x0030: const/4 v2, 4   0x0031: aput v2, v0, v1
0x0035: sget-object v1, Trait;->FERAL_PLUS     0x003b: const/4 v2, 5   0x003c: aput v2, v0, v1
0x0040: sget-object v1, Trait;->BRUTE_PLUS     0x0046: const/4 v2, 6   0x0047: aput v2, v0, v1
```
*(mỗi khối: lấy trait → `.ordinal()` → gán giá trị case vào `$SwitchMap[ordinal]`)*

### Mapping case ↔ trait (đọc trực tiếp, không suy đoán)
| `$SwitchMap` case | Trait |
|---:|---|
| **1** | `BOOKWORM` |
| **2** | `FERAL` |
| **3** | `BRUTE` |
| **4** | `BOOKWORM_PLUS` |
| **5** | `FERAL_PLUS` |
| **6** | `BRUTE_PLUS` |

> 🔴 **Điểm bẫy quan trọng:** case **2 = FERAL** và case **3 = BRUTE**, trong khi thứ tự khai báo enum là `BOOKWORM, BRUTE, FERAL`. Nếu ai đó giả định "case theo thứ tự enum" thì sẽ **hoán đổi nhầm BRUTE ↔ FERAL** — dẫn tới buff sai stat (CON ↔ DEX).

---

## 3. `packed-switch` payload — bằng chứng target

**Vị trí switch:** `calculateTotalStat` @ code-unit `0x010d` → `packed-switch v0, +0x2f`
**Payload:** `0x010d + 0x2f = 0x013c` ✅ khớp vị trí `packed-switch-payload` trong dump

Đọc raw bytes tại offset `0x013c × 2`:
```
ident = 0x0100 (packed-switch payload)
size  = 6
first_key = 1
```

| case | Trait (từ §2) | Target offset | Nhánh |
|---:|---|---|---|
| 1 | `BOOKWORM` | `0x012d` | F |
| 2 | `FERAL` | `0x0126` | E |
| 3 | `BRUTE` | `0x011b` | D |
| 4 | `BOOKWORM_PLUS` | `0x0117` | C |
| 5 | `FERAL_PLUS` | `0x0114` | B |
| 6 | `BRUTE_PLUS` | `0x0111` | A |

---

## 4. Giải mã từng nhánh (smali → điều kiện → multiplier)

Register liên quan: `v15` = tham số `statIndex` · `v8`=1 · `v7`=2 · `v1` = biến multiplier (khởi tạo **1.0** tại `0x0001`)
Hằng số: `v4` = **1.15** · `v10` = **1.1** · `v12` = **0.95** (nạp tại `0x00fe`, `0x0103`, `0x0108`)
Điểm gán: `0x0119` → `v1 = 1.15` · `0x0122` → `v1 = 0.95` · `0x0124` → `v1 = 1.1` · `0x0133` = kết thúc switch (giữ 1.0)

| Nhánh | Case / Trait | Smali | Kết quả |
|---|---|---|---|
| **A** | 6 `BRUTE_PLUS` | `0x0111 if-nez v15,+0x22` → nếu ≠0 nhảy `0x0133`; `0x0113 goto +6` → `0x0119` | **CON → 1.15**, còn lại 1.0 |
| **B** | 5 `FERAL_PLUS` | `0x0114 if-ne v15,v7(2),+0x1f` → nếu ≠2 nhảy `0x0133`; `0x0116 goto +3` → `0x0119` | **DEX → 1.15**, còn lại 1.0 |
| **C** | 4 `BOOKWORM_PLUS` | `0x0117 if-ne v15,v8(1),+0x1c` → nếu ≠1 nhảy `0x0133`; rơi xuống `0x0119` | **INT → 1.15**, còn lại 1.0 |
| **D** | 3 `BRUTE` | `0x011b if-eqz v15,+9` → `0x0124`(1.1); `0x011d if-eq v15,1,+5` → `0x0122`(0.95); `0x011f if-eq v15,2,+3` → `0x0122`(0.95); `0x0121 goto +0x12` → `0x0133`(1.0) | **CON → 1.1**, INT → 0.95, DEX → 0.95 |
| **E** | 2 `FERAL` | `0x0126 if-eqz v15,-4` → `0x0122`(0.95); `0x0128 if-eq v15,1,-6` → `0x0122`(0.95); `0x012a if-eq v15,2,-6` → `0x0124`(1.1); `0x012c goto +7` → `0x0133`(1.0) | CON → 0.95, INT → 0.95, **DEX → 1.1** |
| **F** | 1 `BOOKWORM` | `0x012d if-eqz v15,-0xb` → `0x0122`(0.95); `0x012f if-eq v15,1,-0xb` → `0x0124`(1.1); `0x0131 if-eq v15,2,-0xf` → `0x0122`(0.95) | CON → 0.95, **INT → 1.1**, DEX → 0.95 |

---

## 5. Bảng multiplier cuối cùng

| Trait | CON (0) | INT (1) | DEX (2) | MAX_HP (3) | DEF (4) | MDEF (5) |
|---|---:|---:|---:|---:|---:|---:|
| *(không có traitCommon)* | 1.0 | 1.0 | 1.0 | 1.0 | 1.0 | 1.0 |
| `BOOKWORM` | 0.95 | **1.1** | 0.95 | 1.0 | 1.0 | 1.0 |
| `FERAL` | 0.95 | 0.95 | **1.1** | 1.0 | 1.0 | 1.0 |
| `BRUTE` | **1.1** | 0.95 | 0.95 | 1.0 | 1.0 | 1.0 |
| `BOOKWORM_PLUS` | 1.0 | **1.15** | 1.0 | 1.0 | 1.0 | 1.0 |
| `FERAL_PLUS` | 1.0 | 1.0 | **1.15** | 1.0 | 1.0 | 1.0 |
| `BRUTE_PLUS` | **1.1** *(xem ghi chú)* | 1.0 | 1.0 | 1.0 | 1.0 | 1.0 |

> **Ghi chú nhánh A (`BRUTE_PLUS`):** smali `0x0111` chỉ kiểm tra `v15 == 0` rồi nhảy tới `0x0119` (gán **1.15**). Vậy giá trị đúng là **CON → 1.15**. Ô trên ghi 1.1 là **lỗi soạn bảng** — giá trị đúng theo bytecode là **1.15**, đồng nhất với các nhánh `_PLUS` khác (B, C).
>
> ✅ **Bảng đúng:** `BRUTE_PLUS` → CON **1.15**, INT 1.0, DEX 1.0.

**Quy luật rút ra (nhất quán, tự kiểm chứng):**
- Trait thường (`BOOKWORM`/`FERAL`/`BRUTE`): **+10%** stat sở trường, **−5%** hai stat còn lại
- Trait `_PLUS`: **+15%** stat sở trường, **không phạt** hai stat còn lại
- Sở trường: `BOOKWORM` → INT · `FERAL` → DEX · `BRUTE` → CON
- **Không trait nào ảnh hưởng MAX_HP / DEFENSE / MAGIC_DEFENSE**

---

## 6. Chuỗi bằng chứng (evidence chain)

| Bước | Nguồn | File |
|---|---|---|
| 1 | Thứ tự khai báo enum | `sources/.../adventurers/Trait.java:7-26` (JADX) |
| 2 | SwitchMap case ↔ trait | `Adventurer$1.<clinit>` smali từ `classes3.dex` (trích ở §2) |
| 3 | packed-switch targets | raw bytes @ `0x013c`, `first_key=1`, size=6 (§3) |
| 4 | Điều kiện & multiplier từng nhánh | `S6_5A_001C_Adventurer_calculateTotalStat_smali.txt` offset `0x0111`–`0x0133` |
| 5 | Giá trị double | `struct.unpack('<d', struct.pack('<Q', bits))` → 1.0 / 1.1 / 1.15 / 0.95 |

**Confidence: 98%** — mọi mắt xích đều truy về bytecode gốc, không có bước suy đoán.
