# S6.5A-001C — Recovered Rule Summary (Evidence Review Gate)

**Ngày:** 2026-07-27 · **Trạng thái:** ⏸️ **DỪNG TẠI EVIDENCE REVIEW GATE — chưa implement bất cứ gì**

---

## Bảng tổng hợp rule đã recover

| Rule | Recovered? | Confidence | Evidence File | Ready To Port? | Notes |
|---|---|---:|---|---|---|
| **`Adventurer.calculateTotalStat(int)`** | ✅ **CÓ** | **98%** | `…_Adventurer_calculateTotalStat_smali.txt` (187 lệnh / 332 code-units)<br>`…_Adventurer_calculateTotalStat_JAVA.txt`<br>`…_TraitSwitchMapping.md` | ✅ **YES** | Code-units khớp chính xác con số JADX báo (332). Toàn bộ nhánh case 0–5 + equipment loop + return đều verify được |
| **`Area.dealDamage(...)`** | ✅ **CÓ** | **95%** | `…_Area_dealDamage_smali.txt` (580 lệnh / **1102 code-units**)<br>`…_Area_dealDamage_JAVA.txt` (19.143 ký tự) | ⚠️ **YES, có điều kiện** | Code-units khớp chính xác JADX (1102). Pipeline chính rõ; **5% còn lại** là nhánh phụ (summoned minion, `EndOfTurnAction` đặc biệt, `PASSIVE_CHAOTIC`) cần rà từng dòng khi implement |
| **`Entity.applyDamage(double,boolean,int,double)`** | ✅ **CÓ** | **99%** | `…_Entity_applyDamage_smali.txt` (54 lệnh / 92 code-units)<br>DAD output trong `…_DAD_Decompile_Output.md` §3<br>**+ JADX `Entity.java:349-366`** | ✅ **YES** | **Xác nhận chéo 3 nguồn độc lập** cho cùng một logic |
| **Trait multiplier table** | ✅ **CÓ** | **98%** | `…_TraitSwitchMapping.md` (§2–§5) | ✅ **YES** | Giải mã từ `$SwitchMap` `<clinit>` + `packed-switch` payload raw. **Bẫy:** case2=FERAL, case3=BRUTE — không theo thứ tự enum |
| **Potion index mapping** | ✅ **CÓ** | **99%** | `…_Adventurer_calculateTotalStat_smali.txt` offset `0x0014`–`0x0058` | ✅ **YES** | 🔴 **CON→potion[0], INT→potion[2], DEX→potion[1]** — INT/DEX **bị đảo** so với stat index. HP→potion[3]**×5** |
| **Ascended multiplier** | ✅ **CÓ** | **99%** | smali offset `0x0000`–`0x0005` | ✅ **YES** | `ascended ? 1.5 : 1.0` (bit `4609434218613702656` / `4607182418800017408`). 🔴 **Chỉ áp dụng cho CON/INT/DEX/MAX_HP — KHÔNG áp dụng cho DEF/MDEF** |
| **Defense / Magic-defense handling** | ✅ **CÓ** | **99%** | `…_Entity_applyDamage_smali.txt` | ✅ **YES** | `mult = 1 - min(1.0, (1 - armorIgnored) × 0.01 × (isMagic ? MDEF : DEF))`; chọn MDEF/DEF theo `isMagic` |
| **Shield / HP handling** | ✅ **CÓ** | **99%** | `…_Entity_applyDamage_smali.txt` | ✅ **YES** | Trừ shield trước; nếu shield < dmg → `currentHp = max(0, hp - dmg + shield)`, `shield = 0` |
| **Rounding / cast** | ✅ **CÓ** | **98%** | cả 3 file smali | ✅ **YES** | `Utils.round(double)` ở điểm cuối; `(int)` **cắt cụt** (`double-to-int`) khi nhân ascended multiplier — **hai phép làm tròn khác nhau, không được dùng lẫn** |
| **Flat damage reduction** | ✅ **CÓ** | **97%** | JADX `Entity.java:368-377` | ✅ **YES** | `+5 mỗi status EXALT` + `calculateTotalConstitution() / 8` (chia nguyên) |
| **Damage modifier constants** | ✅ **CÓ** | **96%** | `…_DAD_Decompile_Output.md` (bảng bit-pattern) | ✅ **YES** | 1.5 half-life · 1.25 moreDamageDealtAndTaken · 2.0 DELIRIUM/SKELETON_KEY · 1.35 FRENZY · 1.2 ANOINTED/INSPIRE/EXALT · 1.1 PETRIFY |
| `Area.trapEncounter(...)` | ⏳ **Chưa trích** | — | — | ❌ | Trích được bằng cùng script; chưa cần cho damage/stat |
| `rollAttackDamage()` / `magicDamageAmplification()` / `getArmorIgnored()` | ⏳ **Chưa trích riêng** | — | Nằm trong `Entity.java`/`Adventurer.java` (JADX đọc được) | ⚠️ | Cần đọc khi implement `dealDamage` |

---

## Công thức đã chốt (để review đối chiếu)

### A. `calculateTotalStat(int statIndex)` — confidence 98%
```
mult = ascended ? 1.5 : 1.0

core = switch(statIndex):
  0 CON : (int)(baseConstitution   * mult) + potionsDrank[0]     + doctrine.bonusConstitution()
  1 INT : (int)(baseIntelligence   * mult) + potionsDrank[2]     + doctrine.bonusIntelligence()
  2 DEX : (int)(baseDexterity      * mult) + potionsDrank[1]     + doctrine.bonusDexterity()
  3 HP  : (int)((baseMaxHp + level - 1) * mult) + potionsDrank[3]*5 + doctrine.bonusHp()
  4 DEF : baseDefense        + potionsDrank[4] + doctrine.bonusDefense()        // KHÔNG × mult
  5 MDEF: baseMagicDefense   + potionsDrank[5] + doctrine.bonusMagicDefense()   // KHÔNG × mult
  default: 0

equipSum = Σ over [weapon, armor, accessory] (bỏ qua null):
    factor = (doctrine.doubleAccessoryStats() && e instanceof Accessory) ? 2 : 1
    equipSum += e.get<Stat>(statIndex) * factor

traitMult = TRAIT_TABLE[traitCommon][statIndex]   // mặc định 1.0 nếu traitCommon == null

return Utils.round((core + equipSum) * traitMult)
```

### B. `Area.dealDamage(...)` — confidence 95%
```
raw = rollAttackDamage()                       // hoặc eota.damage (flatDamage)
                                               // hoặc currentHp (EXTRA_ATTACK_HP_TO_DAMAGE)
damage = raw
       × critMultiplier                        // ×² nếu pet Savage proc
       × skillAmplification                    // Skill.damageAmplification, mặc định 1.0
                                               //   ×1.5 nếu moreDamageWhenHalfLife && hp <= maxHp*0.5
                                               //   ×1.25 mỗi bên có moreDamageDealtAndTaken
       × (1.0 + darknessAmplification × localDarkness)
       × statusMultiplier                      // tích buff attacker; PETRIFY trên target ghi đè = 1.1
       × magicMultiplier                       // 1.0 physical | magicDamageAmplification() nếu magic

final = target.applyDamage(damage, isMagic, petBarrier, attacker.getArmorIgnored())
```

### C. `Entity.applyDamage(d, isMagic, barrier, armorIgnored)` — confidence 99%
```
if (this instanceof Enemy) barrier = 0;

defStat  = isMagic ? calculateTotalMagicDefense() : calculateTotalDefense();
reduction = min(1.0, (1.0 - armorIgnored) * 0.01 * defStat);

result = Utils.round( max(1.0,
             (1.0 - reduction) * d
             - calculateFlatDamageReduction()
             - barrier) );

if (currentShield >= result) currentShield -= result;
else { currentHp = max(0, currentHp - result + currentShield); currentShield = 0; }
return result;
```

---

## 🔴 4 chi tiết tinh vi — reviewer cần soi kỹ nhất

| # | Chi tiết | Vì sao dễ sai |
|---|---|---|
| 1 | **Potion index đảo:** INT dùng `potionsDrank[2]`, DEX dùng `potionsDrank[1]` | Trực giác sẽ map 1↔1 với stat index → sai buff potion |
| 2 | **DEF/MDEF không nhân `ascended` multiplier** | 4 stat kia có nhân → dễ áp dụng nhầm cho cả 6 |
| 3 | **MAX_HP:** `(baseMaxHp + level - 1) × mult`, potion **×5** | Level cộng vào **trước** khi nhân; hệ số potion khác các stat khác |
| 4 | **case2=FERAL, case3=BRUTE** trong `$SwitchMap` | Enum khai báo `BOOKWORM, BRUTE, FERAL` → suy theo enum sẽ hoán nhầm CON↔DEX |

---

## Danh sách file evidence

| # | File | Nội dung | Kích thước |
|---|---|---|---|
| 1 | `S6_5A_001C_XAPK_Recovery_Report.md` | Quy trình recovery, path, tool | — |
| 2 | `S6_5A_001C_Area_dealDamage_smali.txt` | Smali đầy đủ + header (580 lệnh, 1102 code-units) | — |
| 3 | `S6_5A_001C_Adventurer_calculateTotalStat_smali.txt` | Smali đầy đủ + **packed-switch payload đã giải mã** | — |
| 4 | `S6_5A_001C_Entity_applyDamage_smali.txt` | Smali đầy đủ (54 lệnh, 92 code-units) | — |
| 5 | `S6_5A_001C_DAD_Decompile_Output.md` | DAD Java cho cả 3 method + bảng giải mã hằng số + ⚠️ đánh dấu đoạn render lỗi | 31 KB |
| 6 | `S6_5A_001C_TraitSwitchMapping.md` | Chuỗi bằng chứng 5 bước cho bảng trait | — |
| 7 | `S6_5A_001C_Recovered_Rule_Summary.md` | File này | — |

Phụ trợ: `S6_5A_001C_Area_dealDamage_JAVA.txt`, `S6_5A_001C_Adventurer_calculateTotalStat_JAVA.txt`, `_dad_applyDamage.txt`

---

## Final Decision

# `EVIDENCE_READY_FOR_REVIEW`

**Lý do:**
- ✅ Cả 3 method đã dump **smali đầy đủ, không cắt branch** — kèm offset hex/dec từng lệnh và payload đã giải mã.
- ✅ **Kiểm chứng tính toàn vẹn:** số code-units dump ra (**1102** và **332**) **khớp chính xác** con số JADX từng báo khi bỏ cuộc (`instruction units count: 1102` / `332`) → chứng minh đã dump đúng method, không thiếu lệnh.
- ✅ `Entity.applyDamage` được **xác nhận chéo 3 nguồn độc lập** (smali DEX, DAD, JADX sources) cho cùng một logic.
- ✅ Bảng trait có **chuỗi bằng chứng 5 bước** truy về bytecode gốc, không có bước suy đoán.
- ✅ Mọi đoạn DAD render lỗi đã được **đánh dấu ⚠️ rõ ràng** kèm chỉ dẫn đối chiếu smali.
- ⚠️ `dealDamage` ở mức **95%** — đủ ngưỡng port nhưng còn nhánh phụ cần rà khi implement; đã ghi rõ, không tô hồng.

**Không có gì được implement:** không sửa Unity code/scene/data, không generate asset, không dùng Higgsfield, không chuyển sang S6.5A-002. File XAPK gốc không bị sửa — chỉ đọc và extract sang temp.

**Chờ user review evidence trước khi cho phép bước tiếp theo.**
