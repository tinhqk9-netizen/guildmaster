# S6.5A-001C — DAD Decompile Output

**Ngày:** 2026-07-27 · **Tool:** androguard 4.1.4 — decompiler **DAD** (`MethodAnalysis.get_method().get_source()`)
**Nguồn:** `classes3.dex` trích từ `it.paranoidsquirrels.idleguildmaster.apk` (trong XAPK v2.147)

> **Cách đọc file này:** DAD xuất Java-like pseudocode trực tiếp từ bytecode. Hằng số `double`/`float` được DAD in ra **dạng bit-pattern long thô** (VD `4607182418800017408`) thay vì giá trị thực (`1.0`). Bảng giải mã ở cuối file. Mọi đoạn DAD render lỗi đều được đánh dấu ⚠️ và phải đối chiếu file smali tương ứng.

---

## Bảng giải mã hằng số double (bit-pattern → giá trị)

| Bit pattern (long) | Giá trị double | Ý nghĩa trong code |
|---|---:|---|
| `4607182418800017408` | **1.0** | Hằng trung tính / `Math.max(1.0, …)` |
| `4576918229304087675` | **0.01** | Hệ số phần trăm |
| `4602678819172646912` | **0.5** | Ngưỡng nửa máu |
| `4606732058837280358` | **0.95** | Trait debuff |
| `4607632778762754458` | **1.1** | Trait buff thường / PETRIFY |
| `4607857958744122982` | **1.15** | Trait buff `_PLUS` |
| `4608308318706860032` | **1.2** | ANOINTED / INSPIRE / EXALT |
| `4608533498688228557` | **1.35** | FRENZY |
| `4608758678669597082` | **1.25** | moreDamageDealtAndTaken |
| `4609434218613702656` | **1.5** | ascended / moreDamageWhenHalfLife |
| `4611686018427387904` | **2.0** | DELIRIUM / SKELETON_KEY |
| `4613937818241073152` | **3.0** | chia turnsLeft |
| `4636737291354636288` | **100.0** | chia phần trăm pet Savage |

*(Kiểm chứng: `struct.unpack('<d', struct.pack('<Q', bits))`)*

---

## 1. `Adventurer.calculateTotalStat(int)`

**Evidence smali:** `S6_5A_001C_Adventurer_calculateTotalStat_smali.txt` (187 lệnh, 332 code-units)

**Độ tin cậy phần thân chính:** ✅ **cao** — nhánh switch stat (case 0–5), vòng lặp equipment, và biểu thức return đều khớp 1:1 với smali.

⚠️ **ĐOẠN RENDER LỖI — BẮT BUỘC đối chiếu smali:** khối `switch (Adventurer$1.$SwitchMap...[traitCommon.ordinal()])` (dòng ~91–133 trong output). DAD render sai ở 2 điểm:
1. Nhiều nhánh `case` hiện ra **rỗng** (`if (p15 == 0) { } else { … }`) — thực tế mỗi nhánh có lệnh `move-wide v1, <const>` gán multiplier.
2. Dòng `v1 = 4606732058837280358;` (0.95) bị DAD đặt **ngoài** khối switch, ngay trước `return` — **SAI**. Trong smali, lệnh gán này nằm tại offset `0x0122` và chỉ được nhảy tới từ các nhánh cụ thể.

→ Bảng trait multiplier đúng được giải mã từ **packed-switch payload thật** + `$SwitchMap`, xem `S6_5A_001C_TraitSwitchMapping.md`.

```java
private int calculateTotalStat(int p15)
    {
        double v3_6;
        long v1 = 4607182418800017408;
        if (!this.ascended) {
            v3_6 = 4607182418800017408;
        } else {
            v3_6 = 4609434218613702656;
        }
        double v3_4;
        double v3_11;
        long v4_6;
        int v9 = 0;
        if (p15 == 0) {
            v3_4 = (((int) (((double) this.baseConstitution) * v3_6)) + this.potionsDrank.get(0));
            v4_6 = this.doctrine.bonusConstitution();
            v3_11 = (v3_4 + v4_6);
        } else {
            if (p15 == 1) {
                v3_4 = (((int) (((double) this.baseIntelligence) * v3_6)) + this.potionsDrank.get(2));
                v4_6 = this.doctrine.bonusIntelligence();
            } else {
                if (p15 == 2) {
                    v3_4 = (((int) (((double) this.baseDexterity) * v3_6)) + this.potionsDrank.get(1));
                    v4_6 = this.doctrine.bonusDexterity();
                } else {
                    if (p15 == 3) {
                        v3_4 = (((int) (((double) ((this.baseMaxHp + this.level) - 1)) * v3_6)) + (this.potionsDrank.get(3) * 5));
                        v4_6 = this.doctrine.bonusHp();
                    } else {
                        if (p15 == 4) {
                            v3_4 = (this.baseDefense + this.potionsDrank.get(4));
                            v4_6 = this.doctrine.bonusDefense();
                        } else {
                            if (p15 == 5) {
                                v3_4 = (this.baseMagicDefense + this.potionsDrank.get(5));
                                v4_6 = this.doctrine.bonusMagicDefense();
                            } else {
                                v3_11 = 0;
                            }
                        }
                    }
                }
            }
        }
        long v4_24 = this.doctrine.doubleAccessoryStats();
        long v10_15 = new it.paranoidsquirrels.idleguildmaster.storage.data.items.abstractClasses.Equipment[3];
        v10_15[0] = this.weapon;
        v10_15[1] = this.armor;
        v10_15[2] = this.accessory;
        long v10_17 = java.util.Arrays.asList(v10_15).iterator();
        while (v10_17.hasNext()) {
            int v11_1 = ((it.paranoidsquirrels.idleguildmaster.storage.data.items.abstractClasses.Equipment) v10_17.next());
            if (v11_1 != 0) {
                if ((v4_24 == 0) || (!(v11_1 instanceof it.paranoidsquirrels.idleguildmaster.storage.data.items.abstractClasses.Accessory))) {
                    int v12_2 = 1;
                } else {
                    v12_2 = 2;
                }
                int v11_3;
                int v11_2;
                if (p15 == 0) {
                    v11_2 = v11_1.getConstitution();
                    v11_3 = (v11_2 * v12_2);
                } else {
                    if (p15 == 1) {
                        v11_2 = v11_1.getIntelligence();
                    } else {
                        if (p15 == 2) {
                            v11_2 = v11_1.getDexterity();
                        } else {
                            if (p15 == 3) {
                                v11_2 = v11_1.getMaxHp();
                            } else {
                                if (p15 == 4) {
                                    v11_3 = v11_1.getDefense();
                                } else {
                                    if (p15 == 5) {
                                        v11_3 = v11_1.getMagicDefense();
                                    }
                                }
                            }
                        }
                    }
                }
                v9 += v11_3;
            }
        }
        if (this.traitCommon != null) {
            switch (it.paranoidsquirrels.idleguildmaster.storage.data.entities.adventurers.Adventurer$1.$SwitchMap$it$paranoidsquirrels$idleguildmaster$storage$data$entities$adventurers$Trait[this.traitCommon.ordinal()]) {
                case 1:
                    if (p15 == 0) {
                    } else {
                        if (p15 == 1) {
                            v1 = 4607632778762754458;
                        } else {
                            if (p15 == 2) {
                            }
                        }
                    }
                    break;
                case 2:
                    if ((p15 == 0) || (p15 == 1)) {
                    } else {
                        if (p15 == 2) {
                        }
                    }
                    break;
                case 3:
                    if (p15 == 0) {
                    } else {
                        if ((p15 == 1) || (p15 == 2)) {
                        }
                    }
                    break;
                case 4:
                    if (p15 == 1) {
                        v1 = 4607857958744122982;
                    }
                    break;
                case 5:
                    if (p15 == 2) {
                    }
                    break;
                case 6:
                    if (p15 == 0) {
                    }
                    break;
                default:
            }
            v1 = 4606732058837280358;
        }
        return it.paranoidsquirrels.idleguildmaster.Utils.round((((double) (v3_11 + v9)) * v1));
    }
```

---

## 2. `Area.dealDamage(Entity, Entity, Area$Skill, EndOfTurnAction)`

**Evidence smali:** `S6_5A_001C_Area_dealDamage_smali.txt` (580 lệnh, **1102 code-units** — khớp chính xác con số JADX báo `instruction units count: 1102`)

**Độ tin cậy pipeline damage chính:** ✅ **cao** — biểu thức nhân 6 hệ số và lời gọi `applyDamage(...)` rõ ràng, khớp smali.

⚠️ **Các điểm cần đối chiếu smali khi implement:**
- DAD dùng lại tên biến (`v3_1`, `v14_7`, `v15_2`…) cho nhiều mục đích khác nhau ở các nhánh → **không suy luận kiểu dữ liệu từ tên biến**.
- Một số biến bị DAD gán kiểu sai (VD `int v11_1 = ((Equipment) ...)`, `long v10_15 = new Equipment[3]`) — đây là lỗi hiển thị kiểu, logic vẫn đúng.
- Các nhánh phụ ít gặp (summoned minion, `EndOfTurnAction` đặc biệt, passive skill hiếm như `PASSIVE_CHAOTIC`) chưa được rà từng dòng — **đây là 5% chưa đạt 100%**.
- `Area$Skill.access$000/$200/$900` là accessor tổng hợp của inner class `Skill` → cần map sang field thật (`damageAmplification`, `statusEffect`, `forceRange`).

```java
private void dealDamage(it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity p36, it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity p37, it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill p38, it.paranoidsquirrels.idleguildmaster.storage.data.entities.EndOfTurnAction p39)
    {
        it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType v3_1;
        it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity v15_2 = p37;
        int v4_6 = (p36 instanceof it.paranoidsquirrels.idleguildmaster.storage.data.entities.adventurers.Adventurer);
        Object[] v1_44 = (p37 instanceof it.paranoidsquirrels.idleguildmaster.storage.data.entities.adventurers.Adventurer);
        if ((p38 == null) || (it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill.access$900(p38) == null)) {
            if ((p39 == null) || (p39.forceRange == null)) {
                v3_1 = 0;
            } else {
                v3_1 = p39.forceRange;
            }
        } else {
            v3_1 = it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill.access$900(p38);
        }
        it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType v3_7;
        if (v3_1 == null) {
            v3_7 = p36.isRanged();
        } else {
            v3_7 = v3_1.booleanValue();
        }
        if ((p38 != null) || ((p39 != null) && (!p39.replicatesBasicAttack))) {
            it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest v8_7 = 0;
        } else {
            v8_7 = 1;
        }
        if (!this.dodge(p36, p37, v8_7, v3_7)) {
            long v9_6 = p37.getPositiveStatusEffects().iterator();
            while (v9_6.hasNext()) {
                double v10_13 = ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect) v9_6.next());
                if ((v10_13.getType() == it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.DEFENSIVE_STANCE) || (v10_13.getType() == it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.FALSE_LIFE)) {
                    if (v10_13.getType() != it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.FALSE_LIFE) {
                        int v2_15 = v10_13;
                        int v12_12 = 0;
                    } else {
                        v12_12 = ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.adventurers.Adventurer) p37).getDoctrine().damageOnFalseLifeRemoval();
                        v2_15 = v10_13;
                    }
                }
                int v14_7;
                it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType v3_8;
                it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType v29_0;
                it.paranoidsquirrels.idleguildmaster.storage.data.entities.EndOfTurnAction v5_2;
                if (v2_15 == 0) {
                    if ((p39 == null) || (!p39.flatDamage)) {
                        int v2_36 = 0;
                    } else {
                        v2_36 = 1;
                    }
                    it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest v8_0;
                    if (p38 == null) {
                        v8_0 = 4607182418800017408;
                    } else {
                        v8_0 = it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill.access$000(p38);
                    }
                    if ((p39 != null) && (p39.fromLivingCompanion)) {
                        v8_0 = ((((double) p36.getLivingCompanionBonusDamage()) * 4576918229304087675) + 4607182418800017408);
                    }
                    if ((p36.isMoreDamageWhenHalfLife()) && (((double) p36.getCurrentHp()) <= (((double) p36.calculateTotalMaxHp()) * 4602678819172646912))) {
                        v8_0 *= 4609434218613702656;
                    }
                    if (p36.isMoreDamageDealtAndTaken()) {
                        v8_0 *= 4608758678669597082;
                    }
                    if (p37.isMoreDamageDealtAndTaken()) {
                        v8_0 *= 4608758678669597082;
                    }
                    double v10_5;
                    if (v2_36 == 0) {
                        v10_5 = this.calculateCriticalMultiplier(p36, p38, p37.getCriticalReduction());
                    } else {
                        v10_5 = 4607182418800017408;
                    }
                    double v25_2;
                    it.paranoidsquirrels.idleguildmaster.storage.data.entities.Entity v15_1;
                    if (v4_6 == 0) {
                        v25_2 = v10_5;
                        v15_1 = 0;
                    } else {
                        int v13_1 = this.petExploring;
                        if ((v13_1 == 0) || ((v10_5 <= 4607182418800017408) || ((v13_1.getSavage() <= 0) || (it.paranoidsquirrels.idleguildmaster.Utils.random() >= (this.petExploring.getSavage() / 4636737291354636288))))) {
                        } else {
                            v25_2 = (v10_5 * v10_5);
                            v15_1 = 1;
                        }
                    }
                    double v10_9;
                    if (v2_36 == 0) {
                        v10_9 = ((p36.calculateTotalDarknessDamageAmplification() * ((double) this.localDarkness)) + 4607182418800017408);
                    } else {
                        v10_9 = 4607182418800017408;
                    }
                    int v13_9 = p36.getPositiveStatusEffects().iterator();
                    double v27_1 = 4607182418800017408;
                    while (v13_9.hasNext()) {
                        int v14_9 = ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect) v13_9.next());
                        if ((v14_9.getType() != it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.DELIRIUM) && (v14_9.getType() != it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.SKELETON_KEY)) {
                            int v31_1;
                            if (v14_9.getType() != it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.FRENZY) {
                                if ((v14_9.getType() != it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.ANOINTED) && ((v14_9.getType() != it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.INSPIRE) && (v14_9.getType() != it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.EXALT))) {
                                } else {
                                    v31_1 = 4608308318706860032;
                                }
                            } else {
                                v31_1 = 4608533498688228557;
                            }
                            v27_1 *= v31_1;
                        } else {
                            v27_1 *= 4611686018427387904;
                        }
                    }
                    v29_0 = v3_7;
                    it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType v30_0 = v12_12;
                    it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType v3_3 = p37.getNegativeStatusEffects().iterator();
                    while (v3_3.hasNext()) {
                        if (((it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect) v3_3.next()).getType() == it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.PETRIFY) {
                            v27_1 = 4607632778762754458;
                            break;
                        }
                    }
                    if ((p39 != null) && (p39.forceMagic != null)) {
                        it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType v3_6 = p39.forceMagic.booleanValue();
                    } else {
                        v3_6 = p36.isMagic();
                    }
                    int v12_4;
                    if (v3_6 == null) {
                        v12_4 = 4607182418800017408;
                    } else {
                        v12_4 = this.magicDamageAmplification();
                    }
                    int v31_0;
                    int v14_2;
                    if (v2_36 == 0) {
                        v31_0 = v15_1;
                        v14_2 = p36.rollAttackDamage();
                    } else {
                        v31_0 = v15_1;
                        v14_2 = ((double) p39.damage);
                    }
                    if (p39 == it.paranoidsquirrels.idleguildmaster.storage.data.entities.EndOfTurnAction.EXTRA_ATTACK_HP_TO_DAMAGE) {
                        v14_2 = ((double) p36.getCurrentHp());
                    }
                    int v2_4;
                    long v32_0;
                    int v2_3 = this.petExploring;
                    if ((v2_3 == 0) || (v1_44 == null)) {
                        v32_0 = v14_2;
                        v2_4 = 0;
                    } else {
                        v2_4 = v2_3.getBarrier();
                        v32_0 = v14_2;
                    }
                    if ((p36.getPassiveSkill() != it.paranoidsquirrels.idleguildmaster.storage.data.entities.Skills.PASSIVE_CHAOTIC) || ((v4_6 == 0) || (v1_44 == null))) {
                        v15_2 = p37;
                    } else {
                        v15_2 = p37;
                        if (((it.paranoidsquirrels.idleguildmaster.storage.data.entities.adventurers.Adventurer) p36).getId() == ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.adventurers.Adventurer) p37).getId()) {
                            v32_0 = 4607182418800017408;
                        }
                    }
                    v3_8 = v30_0;
                    v14_7 = p37.applyDamage((((((v32_0 * v25_2) * v8_0) * v10_9) * v27_1) * v12_4), v3_6, v2_4, p36.getArmorIgnored());
                    if (v4_6 != 0) {
                        if (v25_2 > 4607182418800017408) {
                            it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.increment(it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.smartFighter, ((long) v14_7));
                        }
                        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.incrementToValue(it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.annihilator, ((long) v14_7));
                        if (v1_44 == null) {
                            it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.increment(it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.warrior, 1);
                        }
                    }
                    if ((v1_44 != null) && (v14_7 <= 1)) {
                        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.increment(it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.unscathed, 1);
                    }
                    Object[] v1_1 = it.paranoidsquirrels.idleguildmaster.R$string.log_damage_dealt;
                    v5_2 = p39;
                    if (p39 != null) {
                        v1_1 = p39.log;
                    }
                    int v13_12;
                    Object[] v1_2 = Integer.valueOf(v1_1);
                    if (v31_0 == 0) {
                        if (v25_2 <= 4607182418800017408) {
                            v13_12 = 0;
                        } else {
                            v13_12 = 1;
                        }
                    } else {
                        v13_12 = 2;
                    }
                    it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 33, new Object[] {v1_2, Integer.valueOf(v13_12), p36, v15_2, Integer.valueOf(v14_7)}));
                } else {
                    p37.getPositiveStatusEffects().remove(v2_15);
                    if (v8_7 == null) {
                        it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 32, new Object[] {p37, p37}));
                    } else {
                        it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 31, new Object[] {p36, p37}));
                    }
                    it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 10, new Object[] {p37, v2_15.getType()}));
                    v29_0 = v3_7;
                    v5_2 = p39;
                    v3_8 = v12_12;
                    v14_7 = 0;
                }
                int v13_13;
                it.paranoidsquirrels.idleguildmaster.storage.data.quests.Quest v8_10;
                boolean v16;
                int v12_13;
                if (p38 == null) {
                    v8_10 = 0;
                    if ((v5_2 == null) || (v5_2.effect == null)) {
                        v13_13 = v3_8;
                        v16 = v4_6;
                        v12_13 = v29_0;
                    } else {
                        long v9_4 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                        v13_13 = v3_8;
                        v12_13 = v29_0;
                        v16 = v4_6;
                        v9_4(v5_2.effect.getType(), p36, v5_2.effect.getTurnsLeft(), v5_2.effect.getProbability());
                        this.applyStatus(v15_2, v9_4, (p36.calculateIgnoreImmunityToStatus() * 4576918229304087675));
                    }
                } else {
                    v8_10 = 0;
                    if ((((double) p37.getCurrentHp()) / ((double) p37.calculateTotalMaxHp())) < it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill.access$1000(p38)) {
                        v15_2.setCurrentHp(0);
                        it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 34, new Object[] {v15_2, p36}));
                    }
                    if (p36.getActiveSkill() == it.paranoidsquirrels.idleguildmaster.storage.data.entities.Skills.ACTIVE_THOUSAND_CUTS) {
                        it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill.access$200(p38).setTurnsLeft(it.paranoidsquirrels.idleguildmaster.Utils.round((((double) v14_7) / 4613937818241073152)));
                    }
                    if (p36.getActiveSkill() == it.paranoidsquirrels.idleguildmaster.storage.data.entities.Skills.ACTIVE_THOUSAND_CUTS_II) {
                        it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill.access$200(p38).setTurnsLeft(it.paranoidsquirrels.idleguildmaster.Utils.round((((double) v14_7) / 4611686018427387904)));
                    }
                    if (it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill.access$100(p38)) {
                    } else {
                        this.applyStatus(v15_2, it.paranoidsquirrels.idleguildmaster.storage.data.places.Area$Skill.access$200(p38), (p36.calculateIgnoreImmunityToStatus() * 4576918229304087675));
                    }
                }
                Object[] v1_28;
                Object[] v1_27 = this.petExploring;
                if ((v1_27 == null) || (!v16)) {
                    v1_28 = 0;
                } else {
                    v1_28 = v1_27.getLifesteal();
                }
                Object[] v1_30 = it.paranoidsquirrels.idleguildmaster.Utils.round((((((double) p36.calculateTotalLifesteal()) + v1_28) * 4576918229304087675) * ((double) v14_7)));
                if ((p38 != null) && (p36.getActiveSkill() == it.paranoidsquirrels.idleguildmaster.storage.data.entities.Skills.ACTIVE_FRAGMENTATION)) {
                    v1_30 = 1000;
                }
                if (v1_30 > null) {
                    int v2_17 = p36.getCurrentHp();
                    it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType v3_22 = p36.calculateTotalMaxHp();
                    int v4_2 = Math.min(v3_22, (v2_17 + v1_30));
                    p36.setCurrentHp(v4_2);
                    if (p36.getMaxLifestealOverheal() > 0) {
                        p36.setCurrentShield(Math.max(p36.getCurrentShield(), Math.min((p36.getCurrentShield() + Math.max(v8_10, ((v1_30 - v3_22) + v2_17))), it.paranoidsquirrels.idleguildmaster.Utils.round(((((double) v3_22) * 4576918229304087675) * ((double) p36.getMaxLifestealOverheal()))))));
                    }
                    it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 35, new Object[] {p36, Integer.valueOf(v1_30)}));
                    if (v16) {
                        it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.increment(it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.vampiricThirst, ((long) (v4_2 - v2_17)));
                        it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType v3_30 = ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.adventurers.Adventurer) p36).getMinionBound();
                        if ((v3_30 != null) && (((it.paranoidsquirrels.idleguildmaster.storage.data.entities.adventurers.Adventurer) p36).isHealsMinionBound())) {
                            v3_30.setCurrentHp(Math.min(v3_30.calculateTotalMaxHp(), (v3_30.getCurrentHp() + v1_30)));
                            it.paranoidsquirrels.idleguildmaster.storage.data.places.Logger.log(this, 35, new Object[] {v3_30, Integer.valueOf(v1_30)}));
                        }
                    }
                }
                if ((p39 == null) || (p39.replicatesBasicAttack)) {
                    Object[] v1_35 = p36.onTargetHitEffects().iterator();
                    while (v1_35.hasNext()) {
                        this.applyStatus(v15_2, ((it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect) v1_35.next()), (p36.calculateIgnoreImmunityToStatus() * 4576918229304087675));
                    }
                    if ((p37.getCurrentHp() < p36.getCurrentHp()) && (p36.getStunChanceOnLowerHp() > 0)) {
                        long v9_13 = new it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffect;
                        v9_13(it.paranoidsquirrels.idleguildmaster.storage.data.entities.StatusEffectType.STUN, p36, 1, p36.getStunChanceOnLowerHp());
                        this.applyStatus(v15_2, v9_13, (p36.calculateIgnoreImmunityToStatus() * 4576918229304087675));
                    }
                }
                this.checkDeath(v15_2);
                if ((p39 == null) || (p39.triggersRetaliation)) {
                    this.retaliate(p36, v15_2, v12_13, v13_13);
                }
                return;
            }
            v2_15 = 0;
        } else {
            if (v1_44 != null) {
                it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.increment(it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.hitOrMiss, 1);
            }
            return;
        }
    }
```

---

## 3. `Entity.applyDamage(double, boolean, int, double)`

**Evidence smali:** `S6_5A_001C_Entity_applyDamage_smali.txt` (54 lệnh, 92 code-units)

**Độ tin cậy:** ✅ **rất cao (~99%)** — method ngắn, **và được xác nhận chéo 3 nguồn độc lập**:
1. Smali từ DEX (phase này)
2. DAD decompile (phase này)
3. **JADX `sources/.../Entity.java:349-366`** — method này JADX **decompile được bình thường** từ trước

Ba nguồn cho ra **cùng một logic** → không còn nghi ngờ.

**Không có đoạn render lỗi.**

```java
public int applyDamage(double p7, boolean p9, int p10, double p11)
    {
        if ((this instanceof it.paranoidsquirrels.idleguildmaster.storage.data.entities.enemies.Enemy)) {
            p10 = 0;
        }
        int v9_1;
        double v11_9 = ((4607182418800017408 - p11) * 4576918229304087675);
        if (p9 == 0) {
            v9_1 = this.calculateTotalDefense();
        } else {
            v9_1 = this.calculateTotalMagicDefense();
        }
        int v9_5 = it.paranoidsquirrels.idleguildmaster.Utils.round(Math.max(4607182418800017408, ((((4607182418800017408 - Math.min(4607182418800017408, (v11_9 * ((double) v9_1)))) * p7) - ((double) this.calculateFlatDamageReduction())) - ((double) p10))));
        if ((this instanceof it.paranoidsquirrels.idleguildmaster.storage.data.entities.adventurers.Adventurer)) {
            it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.increment(it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.heavyArmor, ((long) ((int) (p7 - ((double) v9_5)))));
            it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.increment(it.paranoidsquirrels.idleguildmaster.storage.data.quests.QuestsManager.protector, ((long) v9_5));
        }
        int v7_5 = this.currentShield;
        if (v7_5 < v9_5) {
            this.currentHp = Math.max(0, ((this.currentHp - v9_5) + v7_5));
            this.currentShield = 0;
        } else {
            this.currentShield = (v7_5 - v9_5);
        }
        return v9_5;
    }
```

---
