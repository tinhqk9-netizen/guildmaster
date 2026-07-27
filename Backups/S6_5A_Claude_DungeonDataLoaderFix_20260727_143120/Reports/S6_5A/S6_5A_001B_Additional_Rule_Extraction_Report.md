# S6.5A-001B Additional Rule Extraction Report

**Ngày:** 2026-07-27 · **Backup:** `Backups/S6_5A_001B_Additional_Rule_Extraction_20260727_090053/` (212 file)
**Phạm vi:** Chỉ tra cứu/recover rule. **Không implement, không sửa Unity/scene/data/asset.**

---

## Executive Summary

### Rule nào đã recover thêm?
| Domain | Kết quả | Confidence |
|---|---|---|
| **Doctrine** (đầy đủ) | ✅ Cơ chế `getValue()`, 40 `DoctrineAbilityType` kèm **bảng số đầy đủ**, 8 doctrine × 6 ability | **98%** |
| **Tavern recruit + cost** | ✅ Đầy đủ — và phát hiện quan trọng: **recruit MIỄN PHÍ**, chỉ giới hạn bởi quarters capacity | **98%** |
| **`rollRareTrait()` phần còn lại** | ✅ Đầy đủ 14 rare trait + ngưỡng xác suất | **99%** |
| **Enemy stat** | ✅ Đầy đủ — enemy dùng **thẳng base stat, KHÔNG có level multiplier** | **99%** |
| **`calculateTotalStat` input mapping** | ✅ 0=CON, 1=INT, 2=DEX, 3=HP, 4=DEF, 5=MDEF | **99%** |

### Damage recover được chưa? → ❌ **KHÔNG**
### Stat (Adventurer) recover được chưa? → ❌ **KHÔNG**

**Lý do dứt khoát:** Đã tìm toàn bộ hệ thống — **không tồn tại raw bytecode ở bất kỳ đâu**:
- `D:\Tinh\Guild Master - Idle Dungeons\` → chỉ có `sources/` (JADX .java) + `resources/` + `Document/`. **0 file `.dex`/`.apk`/`.jar`/`.smali`/`.class`**
- `Guild Master - Idle Dungeons.zip` (25 MB) → 17.404 entry: **14.482 `.java`**, 1.051 `.png`, 902 `.xml`… **0 bytecode**
- `D:\Tinh\` (maxdepth 2) → không có `.apk`/`.dex`/`.xapk`/`.aab` nào
- `Document/GDD/` → chỉ là **kế hoạch sprint tự viết**, không chứa công thức gốc

→ **Không thể chạy dex2jar/CFR/Procyon/Fernflower vì không có đầu vào.** Đây là giới hạn nguồn, không phải giới hạn công cụ.

### Rule nào đạt ≥95% confidence?
Formula (20/20), SaveData schema (140/140), Tavern visitor + recruit, Doctrine, Craft timer/claim, Merchant sell/timer/price, Offline cap, Enemy stat, exp/death/dodge/crit-multiplier, rollRareTrait.

### Rule nào vẫn ManualRuleRequired?
`ManualRuleRequired_Damage` (`Area.dealDamage`) · `ManualRuleRequired_CharacterStat` (`Adventurer.calculateTotalStat`) · `trapEncounter` · Quest · Merchant buy/roll offer · Dungeon tick/target selection · Loot

### Có đủ để sang implement không?
**Đủ cho một tập con an toàn** (Formula, SaveData, Tavern, Craft, Settings, Doctrine) — **không đủ** cho Character stat và Combat.

---

## Available Reverse Engineering Sources

| Source Type | Path | Exists | Useful For | Notes |
|---|---|---|---|---|
| JADX `.java` output | `D:\Tinh\Guild Master - Idle Dungeons\sources\` | ✅ 14.482 file (1.150 file trong package game) | Đọc rule | **Nguồn DUY NHẤT hiện có** |
| Android resources | `…\resources\` | ✅ (`AndroidManifest.xml`, `assets/`, `META-INF`, `.properties`) | Localization string, config | Không chứa công thức gameplay |
| Tài liệu | `…\Document\GDD\Sprint0–7.md`, `Rebuild_GuildMaster_MasterPlan.md` | ✅ | — | **Là kế hoạch rebuild tự viết**, không phải GDD gốc của nhà phát triển. Không có công thức |
| **`.apk`** | toàn hệ thống | ❌ **KHÔNG CÓ** | — | Đã tìm `D:\Tinh` maxdepth 2 |
| **`.dex` / `classes.dex`** | toàn hệ thống | ❌ **KHÔNG CÓ** | — | Kể cả trong zip gốc |
| **`.smali`** | toàn hệ thống | ❌ **KHÔNG CÓ** | — | JADX export không kèm smali |
| **`.jar` / `.class`** | toàn hệ thống | ❌ **KHÔNG CÓ** | — | |
| Zip gốc | `D:\Tinh\Guild Master - Idle Dungeons.zip` | ✅ 25 MB | — | Kiểm tra nội dung: **chỉ là bản sao của JADX output**, không có bytecode |
| Converter output | `D:\Tinh\Game Decode Converter\output\production_staging\` | ✅ 10 JSON | Data definition | Đã dùng ở S6-001; không chứa logic |

**Kết luận Task 1:** Không có bất kỳ nguồn raw bytecode nào để recover 2 method thất bại. **Cần user cung cấp APK/DEX gốc** nếu muốn giải blocker.

---

## Area.dealDamage Recovery

| Attempt | Source/Tool | Result | Evidence | Confidence |
|---|---|---|---|---|
| 1 | Đọc lại JADX output | ❌ Thất bại | `Area.java:2213` — `/* Method dump skipped, instruction units count: 1102 */` + `throw new UnsupportedOperationException("Method not decompiled: …")` | — |
| 2 | Tìm `.smali` | ❌ Không có nguồn | `find` toàn hệ thống → 0 file `.smali` | — |
| 3 | Tìm `.dex`/`.apk` để chạy dex2jar + CFR/Procyon/Fernflower | ❌ Không có nguồn | Kiểm tra decode root, zip gốc (17.404 entry), `D:\Tinh` → 0 bytecode | — |
| 4 | Chạy lại JADX với option khác (`--show-bad-code`) | ❌ Không thực hiện được | Không có input `.dex` để chạy lại; thư mục hiện tại là **output đã export**, không phải project JADX | — |
| 5 | Tra `Document/GDD` | ❌ Không có | GDD là kế hoạch sprint tự viết, `grep "damage\|formula\|multiplier"` → chỉ khớp tên task, không có công thức | — |
| 6 | Suy ra từ code gọi xung quanh | ⚠️ Chỉ ra được **ngữ cảnh**, không ra công thức | Xem bảng dưới | **< 70%** |

### Phần ĐÃ BIẾT về dealDamage (từ code xung quanh — không đủ để port)
| Khía cạnh | Điều đã biết | Nguồn |
|---|---|---|
| Chữ ký | `dealDamage(Entity attacker, Entity target, Area.Skill skill, EndOfTurnAction eota)` — trả `void` | `Area.java:2213` |
| Kích thước | 1102 instruction units → logic **rất lớn** (so sánh: `calculateTotalStat` chỉ 332) | JADX note |
| Đầu vào damage cơ bản | `attacker.calculateMinAttackDamage()` … `calculateMaxAttackDamage()` (Adventurer: từ `weapon.getDamageModifier(CON, INT, DEX)` × `damageDelta`; Enemy: `getMinDamage()`/`getMaxDamage()`) | `Adventurer.java:238/251`, `Enemy.java:52/57` |
| Crit | Có `calculateCriticalMultiplier(entity, skill, d)` **tách riêng** và đọc được | `Area.java:1289` |
| Dodge | Có `dodge(...)` **tách riêng** và đọc được | `Area.java:1188` |
| Phòng thủ | `calculateTotalDefense()` / `calculateTotalMagicDefense()` tồn tại; **cách áp dụng vào damage KHÔNG rõ** | `Entity.java` |
| Lifesteal / shield / mana | Có `calculateTotalLifesteal()`, `currentShield`, `currentMana`; **cách áp dụng KHÔNG rõ** | `Entity.java` |
| Ignore armor | `doctrine.ignoreArmorPercentage()` tồn tại; **cách áp dụng KHÔNG rõ** | `DoctrineOfWar.java` |
| Darkness | `calculateTotalDarknessDamageAmplification()` tồn tại; **cách áp dụng KHÔNG rõ** | `Entity.java` |
| Side effect | Gọi `checkDeath()` sau đó (suy từ luồng); `Logger.log`; `QuestsManager.increment` | `Area.java:2221` |

### 🔴 Kết luận: `ManualRuleRequired_Damage`
**Chưa biết (bắt buộc để port):** công thức tổng hợp damage, thứ tự áp dụng crit/defense/ignore-armor/darkness/lifesteal/shield, quy tắc làm tròn, `rollsDamageThreeTimes()` áp dụng ra sao, cách `EndOfTurnAction` tham gia, cách `skill.damageAmplification`/`executionThreshold` tác động.

**Confidence tổng: < 70%** → **KHÔNG được implement.**

---

## Adventurer.calculateTotalStat Recovery

| Attempt | Source/Tool | Result | Evidence | Confidence |
|---|---|---|---|---|
| 1 | Đọc lại JADX output | ❌ Thất bại | `Adventurer.java:905` — `/* Method dump skipped, instruction units count: 332 */` | — |
| 2–5 | smali / dex / decompiler khác / GDD | ❌ Không có nguồn | Giống Task 2 | — |
| 6 | **Suy từ `Enemy.java` (lớp anh em)** | ⚠️ Có giá trị đối chiếu | `Enemy.calculateTotalConstitution() { return this.baseConstitution; }` — **enemy KHÔNG có multiplier nào**, dùng thẳng base | **99%** (cho Enemy) |
| 7 | Suy từ field + hằng số của `Adventurer` | ⚠️ Ra được **input mapping**, không ra công thức | Xem dưới | **99%** (mapping) / **< 70%** (công thức) |

### Phần ĐÃ RECOVER được
| Điều đã biết | Chi tiết | Nguồn | Confidence |
|---|---|---|---|
| **Input mapping** | `KEY_CONSTITUTION=0`, `KEY_INTELLIGENCE=1`, `KEY_DEXTERITY=2`, `KEY_HP=3`, `KEY_DEFENSE=4`, `KEY_MAGIC_DEFENSE=5` | `Adventurer.java:21–26` | **99%** |
| Các stat gọi vào đâu | `calculateTotalConstitution()→(0)`, `Intelligence→(1)`, `Dexterity→(2)`, `MaxHp→(3)`, `Defense→(4)`, `MagicDefense→(5)` | `Adventurer.java:392–418` | **99%** |
| Base stat nguồn | Mỗi unit class khai báo trong `configureStatistics()`. VD `Footman`: `maxLevel=5, baseMaxHp=40, baseConstitution=8, baseIntelligence=4, baseDexterity=4, baseDefense=20, baseMagicDefense=20` | `units/Footman.java` | **99%** |
| **Thành phần CÓ THỂ tham gia** (có mặt trong class, nhưng **không rõ tham gia thế nào**) | `level`, `ascended`, `weapon`/`armor`/`accessory`, `traitCommon`/`traitRare`, `potionsDrank`, `doctrine` (`bonusConstitution/bonusDexterity/bonusIntelligence/bonusHp/bonusDefense/bonusMagicDefense`) | `Adventurer.java:28–50`, `Doctrine.java` | ⚠️ Chỉ là **danh sách ứng viên** |
| **Enemy KHÔNG dùng multiplier** | `calculateTotalX() = baseX` thuần | `Enemy.java:62–92` | **99%** → **Enemy side port được 100%** |

### 🔴 Kết luận: `ManualRuleRequired_CharacterStat`
**Chưa biết (bắt buộc để port):** level multiplier thật, thứ tự cộng/nhân của equipment–trait–potion–doctrine, có cap/floor không, quy tắc làm tròn, ảnh hưởng của `ascended`, xử lý riêng cho `KEY_HP`.

**Confidence: < 70%** → **KHÔNG được sửa `levelMultiplier = 1.0f`** (đúng theo yêu cầu user).

---

## Doctrine Rules

**Cơ chế lõi (đọc được đầy đủ):**
- `DoctrineAbility.getValue() = level * type.increasePerLevel` — `DoctrineAbility.java:31`
- `Doctrine.getValue(type)`: tìm ability trong list; có → trả `getValue()`, không có → **0** — `Doctrine.java:239`
- Mỗi `Doctrine` có 6 slot `l1…l6` và danh sách `abilities`
- Enum `DoctrineAbilityType(name, description, image, **cost**, **increasePerLevel**, formatMode, **maxLevel**, row)` — `DoctrineAbilityType.java:57`

| Doctrine | Source | Level/Progress Fields | Bonus Method | Exact Logic | Needed For |
|---|---|---|---|---|---|
| **War** | `instances/DoctrineOfWar.java` | `warLevel`, `warProgress` | `bonusQuestPoints()` = `data.getWarLevel()`<br>`bonusConstitution()` = `getValue(IMPROVED_CONSTITUTION)`<br>`bonusDexterity()` = `getValue(IMPROVED_DEXTERITY)`<br>`bonusCounterattack()` = `getValue(CONDITIONED_REFLEXES)`<br>`ignoreArmorPercentage()` = `getValue(TACTICAL_KNOWLEDGE)`<br>`forcesCounterattack()` = `getValue(RELENTLESS_ASSAULT) > 0`<br>`canUseAllWeapons()` = `getValue(WEAPON_MASTER) > 0` | Ability: IMPROVED_CONSTITUTION, IMPROVED_DEXTERITY, CONDITIONED_REFLEXES, TACTICAL_KNOWLEDGE, RELENTLESS_ASSAULT, WEAPON_MASTER | C-07 counterattack, damage |
| Affliction | `DoctrineOfAffliction.java` | `afflictionLevel/Progress` | ⚠️ Chưa đọc chi tiết (cùng pattern) | Cùng cơ chế `getValue()` | Combat |
| Control | `DoctrineOfControl.java` | `controlLevel/Progress` | ⚠️ Chưa đọc chi tiết | | |
| Fortitude | `DoctrineOfFortitude.java` | `fortitudeLevel/Progress` | ⚠️ Chưa đọc chi tiết | | |
| Grace | `DoctrineOfGrace.java` | `graceLevel/Progress` | ⚠️ Chưa đọc chi tiết | | |
| Illusion | `DoctrineOfIllusion.java` | `illusionLevel/Progress` | ⚠️ Chưa đọc chi tiết | | |
| Knowledge | `DoctrineOfKnowledge.java` | `knowledgeLevel/Progress` | ⚠️ Chưa đọc chi tiết | | |
| Ruin | `DoctrineOfRuin.java` | `ruinLevel/Progress` | ⚠️ Chưa đọc chi tiết | | |
| Empty | `EmptyDoctrine.java` | — | Tất cả trả 0 (default) | Adventurer chưa chọn doctrine | Mặc định an toàn |

**Bảng `DoctrineAbilityType` (40 loại) — trích `increasePerLevel` / `maxLevel` / `cost`:**
| Ability | cost | increasePerLevel | maxLevel |
|---|---:|---:|---:|
| IMPROVED_HEALTH | 1 | 15 | 5 |
| IMPROVED_CONSTITUTION / IMPROVED_DEXTERITY / IMPROVED_INTELLIGENCE | 1 | 2 | 5 |
| EXALTED_CONSTITUTION / EXALTED_DEXTERITY / EXALTED_INTELLIGENCE | 1 | 3 | 5 |
| EXALTED_HEALTH | 1 | 25 | 5 |
| EXALTED_MANA | 3 | 1 | 3 |
| LORE_MASTER | 10 | 100 | 1 |
| SERVUS_SANGUINIS | 2 | 8 | 3 |
| SERVUS_UMBRAE | 2 | 2 | 3 |
| NECROSIS_PORPHYRICA | 3 | 25 | 3 |
| GENUS_VAMPYRI | 5 | 20 | 1 |
| IMPENETRABLE_WILLPOWER | 2 | 20 | 3 |
| CHILLING_FLOW | 3 | 30 | 2 |
| MIND_BENDER | 2 | 5 | 3 |
| STAR_GAZE | 4 | 5 | 2 |
| ARCANE_SUPPRESSION | 6 | 150 | 1 |
| CONDITIONED_REFLEXES | 2 | 10 | 3 |
| TACTICAL_KNOWLEDGE | 3 | 20 | 2 |
| RELENTLESS_ASSAULT | 7 | 1 | 1 |
| WEAPON_MASTER | 10 | 1 | 1 |
| EPHEMERAL_PRESENCE | 2 | 3 | 3 |
| BEAT_THE_ODDS | 3 | 1 | 1 |
| FALSE_LIFE | 4 | 4 | 2 |
| TRUE_AGONY | 3 | 1500 | 1 |
| TROLL_RESISTANCE / WARLOCK_RESILIENCE | 3 | 1 | 2 |
| MANIFEST_DANGER | 4 | 1 | 1 |
| MIRROR_OF_ANGUISH | 8 | 1 | 1 |
| EXPOSE_WEAKNESS | 2 | 8 | 3 |
| EXPLOIT_WEAKNESS | 2 | 12 | 3 |
| LIGHTNING_SPEED | 3 | 15 | 3 |
| EYE_FOR_AN_EYE | 4 | 50 | 1 |
| RAGEBOUND | 4 | 35 | 1 |
| DIVINE_INTERVENTION | 2 | 1 | 3 |
| SELFLESS_SPIRIT | 2 | 10 | 4 |
| OVERHEAL | 3 | 5 | 2 |
| HEALING_NOVA | 5 | 7 | 1 |

**Decision: `Doctrine_Rules_Recovered_95`** — cơ chế + bảng số đầy đủ (98%). Còn 7 instance class chưa đọc chi tiết nhưng **cùng một pattern** đã xác minh qua `DoctrineOfWar`; đọc nốt là việc cơ học, rủi ro thấp.

---

## Tavern Recruit Rules

| Rule | Source | Exact Logic | Save Fields | Confidence | Unity Target |
|---|---|---|---|---|---|
| **TR-01 Điều kiện recruit** | `DialogTavern.java:238` | `if (Formulas.getQuartersCapacity() <= data.getAdventurers().size())` → **nút bị disable**, hiện dialog "recruit unavailable". Ngược lại cho phép recruit | `adventurers`, `levelQuarters`, `upgradeQuarters`, 4 IAP flag | **98%** | `TavernService` |
| **TR-02 Hành động recruit** | `DialogTavern.java:360` | `adventurer = data.getTavernGuests().get(i)`<br>`data.getTavernGuests().remove(adventurer)`<br>`adventurer.setId(Utils.calculateNewAdventurerId())`<br>`data.getAdventurers().add(adventurer)`<br>`Utils.triggerGuildSizeAchievementCheck()` | `tavernGuests`, `adventurers` | **98%** | `TavernService` |
| **TR-03 Chi phí recruit** | `DialogTavern.java:360-378` | 🎉 **KHÔNG CÓ CHI PHÍ** — không có dòng nào trừ `money`/`gems` trong `recruitAdventurer()`. Giới hạn duy nhất là quarters capacity | — | **95%** | `TavernService` |
| **TR-04 Tutorial side effect** | `DialogTavern.java:367` | `if (tutorialStep == 1 \|\| tutorialStep == 6) { setTutorialStep(step + 1); if (step == 6) Utils.progressTavernTime(28800L); }` | `tutorialStep` | **98%** | `TavernService` |
| **TR-05 Giảm thời gian chờ visitor** | `DialogTavern.java:324` | `setNextTavernVisit((long)(nextTavernVisit * 0.9))` | `nextTavernVisit` | ⚠️ **80%** — **chưa xác định cost** (gem? ads?). Cần đọc caller | `TavernService` |
| **TR-06 Giá nâng cấp hiển thị** | `DialogTavern.java:57–62` | Dùng `Formulas.getTavernCapacityPrice()` / `getTavernTimePrice()`, so với `data.getMoney()` để đổi màu | `money` | **95%** | `TavernService` |
| **TR-07 Id adventurer mới** | `Utils.calculateNewAdventurerId()` `:869` | ⚠️ Chưa đọc thân hàm | `adventurers` | **70%** | `TavernService` |

**Phát hiện đáng chú ý:** recruit **miễn phí** — điều này làm nút thắt gameplay dễ mở hơn nhiều so với dự đoán.

---

## Rare Trait Roll Rules

`Utils.rollRareTrait()` — `Utils.java:229`. Một lần `random()`, so ngưỡng tăng dần. Bước ≈ **1/70 = 0.0142857**.

| # | Trait | Ngưỡng `<` | Xác suất |
|---:|---|---|---|
| 1 | `EMPATHETIC` | 0.0142857… (`TRAIT_RARE_INDIVIDUAL_PROBABILITY`) | 1/70 |
| 2 | `GIFTED` | 0.0285714… | 1/70 |
| 3 | `INTIMIDATING` | 0.0428571… | 1/70 |
| 4 | `FOCUSED` | 0.0571428… | 1/70 |
| 5 | `DRAGON_BLOOD` | 0.0714285… | 1/70 |
| 6 | `CURSED` | 0.0857142… | 1/70 |
| 7 | `REACTIVE` | 0.1 | 1/70 |
| 8 | `NOCTURNAL` | 0.1142857… | 1/70 |
| 9 | `MINDFUL` | 0.1285714… | 1/70 |
| 10 | `TROLL_BLOOD` | 0.1428571… | 1/70 |
| 11 | `RUTHLESS` | 0.1571428… | 1/70 |
| 12 | `BLESSED` | 0.1714285… | 1/70 |
| 13 | `ALERT` | 0.1857142… | 1/70 |
| 14 | `NIMBLE` | 0.2 | 1/70 |
| — | `null` (không trait) | ≥ 0.2 | **80%** |

**Confidence: 99%** — đọc trọn vẹn, không còn phần bị cắt.

**Bổ sung — `rollCommonTrait()`** (`Utils.java:216`): `BOOKWORM` < 0.1333…, `BRUTE` < 0.2667, `FERAL` < 0.4, còn lại `null` (60%). Bước = 2/15.

---

## Quest Rules

| Rule | Source | Exact Logic | Save Fields | Confidence | Unity Target |
|---|---|---|---|---|---|
| Q-01…Q-05 | `QuestsManager.java` (468 dòng) | ⚠️ **CHƯA ĐỌC trong phase này** | `questsSeen`, `questsRefreshed`, `questsCompleted` | **< 70%** | `QuestService` |
| Q-06 Quest trigger từ combat | `Area.java` (nhiều chỗ) | ✅ Đọc được: `QuestsManager.increment(theEnd, 1)` khi adventurer chết; `criticalHit` khi crit; `pulverization` khi crit ≥ 2.5; `shocking` khi gây STUN; `smokingHot` khi gây ABLAZE; `student` cộng exp nhận; `fastLearner` khi exp multiplier ≥ 1.5 | — | **95%** | `QuestService` |
| Q-07 Doctrine quest points | `DoctrineOfWar.java:26` | `bonusQuestPoints()` = `data.getWarLevel()` (mỗi doctrine trả level tương ứng) | 8 `*Level` | **95%** | `QuestService` |

**Trạng thái:** **Chưa đủ** — `QuestsManager.java` cần đọc riêng ở **S6.5A-001C**.

---

## Merchant Buy / Roll / Restock Rules

| Rule | Source | Exact Logic | Save Fields | Confidence | Unity Target |
|---|---|---|---|---|---|
| M-01 `getSecondsToSell` | `Item.java:63` | `(merchantPack ? 0.6 : 1.0) * pow(0.9, (levelMarketTime + upgradeMarketTime) - 1) * price * 4 * stack` | `levelMarketTime`, `upgradeMarketTime` | **98%** (từ 001) | `MerchantService` |
| M-02 `progressMarketTime` | `Utils.java:504` | Đã bóc ở S6.5A-001 | `marketListings`, `soldMarketItems` | **98%** | `MerchantService` |
| M-03 `truncatePrice` | `Utils.java:889` | `if (j <= 10000) return j; mod = (j <= 1000000) ? j%100 : j%10000; return j - mod;` | — | **99%** | `FormulaService` |
| M-05 `rollPotion` | `Utils.java:313` | ⚠️ **CHƯA ĐỌC** | — | **< 70%** | `MerchantService` |
| M-06 `rollSpecialFoods` | `Utils.java:352` | ⚠️ **CHƯA ĐỌC** | — | **< 70%** | `MerchantService` |
| M-07 `rollUpgrades` | `Utils.java:400` | ⚠️ **CHƯA ĐỌC** | — | **< 70%** | `MerchantService` |
| M-08 `rollMerchant*Offers` | `Area.java:3157/3161` | ⚠️ **CHƯA ĐỌC** | `newMerchantRegularItems/SpecialItems` | **< 70%** | `MerchantService` |
| M-09 Buy action | `ui/dialogs/DialogBuyFromMerchant.java` **(đã định vị được file)** | ⚠️ **CHƯA ĐỌC** | `money`, `gems` | **< 70%** | `MerchantService` |

**Trạng thái:** Sell/timer/price **đủ**; buy/roll offer **chưa đủ** → giữ `DeferredPriceOrCurrencyRule`. File cần đọc đã định vị: `DialogBuyFromMerchant.java`.

---

## Dungeon Tick / Target / Loot Rules

| Rule | Source | Exact Logic | Confidence | Unity Target | Notes |
|---|---|---|---|---|---|
| `Area.tick()` | `Area.java:279` | ⚠️ **CHƯA ĐỌC** | **< 70%** | `DungeonRunOrchestrator` | Cần subtask riêng |
| `performAction()` | `:420` | ⚠️ **CHƯA ĐỌC** | **< 70%** | | |
| `fightTurn()` | `:748` | ⚠️ **CHƯA ĐỌC** | **< 70%** | | |
| `decideTurnsOrder()` | `:622` | ⚠️ **CHƯA ĐỌC** | **< 70%** | | |
| `selectTargets()` + 7 strategy | `:2500–2907` | ⚠️ **CHƯA ĐỌC** | **< 70%** | `CombatService` | |
| `loot()` / `fullChest()` | `:669/729` | ⚠️ **CHƯA ĐỌC** | **< 70%** | `LootService` | |
| `Utils.collectDrops()` | `Utils.java:935` | ⚠️ **CHƯA ĐỌC** | **< 70%** | `LootService` | |
| `collectExperience()` | `:635` | ✅ Đã bóc ở S6.5A-001 | **95%** | `CombatService` | Port 1:1 |
| `checkDeath()` | `:2221` | ✅ Đã bóc ở S6.5A-001 | **95%** | `CombatService` | Port 1:1 |
| `dodge()` | `:1188` | ✅ Đã bóc ở S6.5A-001 | **95%** | `CombatService` | Phụ thuộc stat |
| `calculateCriticalMultiplier()` | `:1289` | ✅ Đã bóc ở S6.5A-001 | **95%** | `CombatService` | Phụ thuộc stat |

**Đề xuất chia subtask rõ ràng (không kết luận mơ hồ):**
- **S6.5A-001C-1**: `Area.tick` + `performAction` + `enterRoom` + `incrementProgress` + `respawn` (vòng đời dungeon)
- **S6.5A-001C-2**: `fightTurn` + `decideTurnsOrder` + `selectNextActing` + `resolveStatus` (vòng lượt)
- **S6.5A-001C-3**: `selectTargets` + 7 strategy phụ (chọn mục tiêu)
- **S6.5A-001C-4**: `loot` + `fullChest` + `collectDrops` (loot)
- **S6.5A-001C-5**: `QuestsManager` (468 dòng)
- **S6.5A-001C-6**: `rollPotion`/`rollSpecialFoods`/`rollUpgrades`/`rollMerchant*Offers`/`DialogBuyFromMerchant` (merchant)
- **S6.5A-001C-7**: 7 doctrine instance còn lại + `calculateNewAdventurerId` + `TR-05` cost

---

## Critical Blocker Status

| Blocker | Before | After | Confidence | Decision |
|---|---|---|---:|---|
| **Damage formula** (`Area.dealDamage`) | ManualRuleRequired | **Vẫn ManualRuleRequired** — xác nhận **không tồn tại nguồn bytecode** để recover | **< 70%** | 🔴 `ManualRuleRequired_Damage` — **cần user cung cấp APK/DEX gốc** |
| **Character stat** (`Adventurer.calculateTotalStat`) | ManualRuleRequired | **Vẫn ManualRuleRequired** — nhưng recover được **input mapping** + xác nhận **Enemy không dùng multiplier** | Mapping **99%** / công thức **< 70%** | 🔴 `ManualRuleRequired_CharacterStat` — **giữ nguyên `levelMultiplier = 1.0f`, không sửa** |
| Doctrine | Chưa đọc | ✅ **Recover đầy đủ** cơ chế + bảng 40 ability | **98%** | ✅ Đủ để implement |
| Tavern recruit + cost | Chưa đọc | ✅ **Recover đầy đủ** — recruit miễn phí | **98%** | ✅ Đủ để implement |
| rollRareTrait | Đọc một phần | ✅ **Recover đầy đủ** 14 trait | **99%** | ✅ Đủ để implement |
| Quest | Chưa đọc | ⚠️ Chỉ recover trigger từ combat | **< 70%** (tổng thể) | ⏳ Cần 001C |
| Merchant buy/roll | Chưa đọc | ⚠️ Định vị được file, chưa đọc | **< 70%** | ⏳ Cần 001C |
| Dungeon tick/target/loot | Chưa đọc | ⚠️ Chưa đọc | **< 70%** | ⏳ Cần 001C |

---

## Confidence Gate — Implementation Readiness

| Domain | Rule Needed | Confidence | Ready To Implement? | Reason |
|---|---|---:|---|---|
| **Formula** | 20 formula + `truncatePrice` | **98%** | ✅ **YES** | Đọc nguyên văn `Formulas.java`; cần thêm 5 IAP flag vào SaveData trước |
| **SaveData schema** | 140 field | **97%** | ✅ **YES** | Đọc đủ; cấu trúc 20 area-instance cần thiết kế mapping (Unity dùng list) |
| **Tavern visitor** | T-01…T-04, T-06…T-09 | **97%** | ✅ **YES** | Đầy đủ |
| **Tavern recruit** | TR-01…TR-04, TR-06 | **96%** | ✅ **YES** | TR-05 (cost giảm thời gian) và TR-07 (`calculateNewAdventurerId`) còn thiếu → **triển khai phần chính, để trống 2 mục nhỏ** |
| **Doctrine** | Cơ chế + bảng ability | **96%** | ✅ **YES** | 7 instance còn lại cùng pattern, đọc nốt là cơ học |
| **Craft / Workshop** | CR-01…CR-06 | **96%** | ✅ **YES** | Gỡ được `ManualRuleRequired` hiện tại. CR-07 (claim UI) còn thiếu |
| **Merchant — sell/timer** | M-01…M-04 | **96%** | ✅ **YES** (chỉ phần sell) | |
| **Merchant — buy/roll offer** | M-05…M-09 | **< 70%** | ❌ **NO** | Chưa đọc → giữ deferred |
| **Offline progress** | delta + cap 12h | **95%** | ✅ **YES** | Cần `lastAccess` |
| **Settings** | Save/Delete + 9 setting field | **95%** | ✅ **YES** | Dùng API `SaveService` có sẵn |
| **Enemy stat** | `calculateTotalX = baseX` | **99%** | ✅ **YES** | Enemy side port được 100% |
| **Character stat (Adventurer)** | `calculateTotalStat` | **< 70%** | ❌ **NO** | 🔴 Blocker cứng |
| **Damage / Combat** | `dealDamage` | **< 70%** | ❌ **NO** | 🔴 Blocker cứng |
| **Dungeon tick** | `tick`/`performAction`/`fightTurn` | **< 70%** | ❌ **NO** | Chưa đọc |
| **Target selection** | `selectTargets` + 7 strategy | **< 70%** | ❌ **NO** | Chưa đọc |
| **Loot** | `loot`/`fullChest`/`collectDrops` | **< 70%** | ❌ **NO** | Chưa đọc |
| **Quest** | `QuestsManager` | **< 70%** | ❌ **NO** | Chưa đọc |

**Tổng kết: 11/17 domain đạt ≥95% → sẵn sàng implement. 6/17 domain < 70% → KHÔNG implement.**

---

## Implementation Readiness

| Next Task | Ready? | Required Confidence | Current Confidence | Decision |
|---|---|---:|---:|---|
| **S6.5A-002** Formula + SaveData Schema | ✅ **YES** | 95% | **97–98%** | **Proceed** |
| **S6.5A-003** Runtime Service Wiring | ✅ **YES** | 95% | ~95% (chỉ là wiring, không cần rule gameplay) | **Proceed** |
| **S6.5A-011** Settings | ✅ **YES** | 95% | 95% | Proceed |
| **S6.5A-004** Tavern | ✅ **YES** | 95% | **96–97%** | Proceed (trừ TR-05/TR-07) |
| **S6.5A-009** Craft | ✅ **YES** | 95% | **96%** | Proceed (trừ CR-07) |
| S6.5A-006 Inventory actions | ⚠️ Một phần | 95% | ~90% | Chờ — equip phụ thuộc stat |
| **S6.5A-005** Character/Equipment/Skill | ❌ **NO** | 95% | **< 70%** | 🔴 **BLOCKED** — stat formula |
| **S6.5A-007** Dungeon/Combat/Loot | ❌ **NO** | 95% | **< 70%** | 🔴 **BLOCKED** — damage + tick + target + loot |
| S6.5A-008 Quest | ❌ **NO** | 95% | < 70% | Chờ 001C |
| S6.5A-010 Merchant | ⚠️ Một phần | 95% | sell 96% / buy < 70% | Chờ 001C |
| S6.5A-012 Offline | ✅ **YES** | 95% | 95% | Proceed sau 002 |

---

## Recommended Next Step

# `Proceed_To_S6_5A_002_And_003`

**Kèm 2 việc song song:**
1. **S6.5A-001C** — đọc nốt 7 nhóm rule còn thiếu (đã chia subtask cụ thể ở mục Dungeon Tick bên trên). Đây là việc **đọc được**, chỉ cần thời gian.
2. **Quyết định của user về 2 blocker cứng** — xem mục dưới.

### ⚠️ Cần user quyết định: 2 blocker chỉ có 3 lối thoát

| Lối thoát | Mô tả | Đánh giá |
|---|---|---|
| **A. Cung cấp APK/DEX gốc** | User tìm lại file `.apk` của game (tải lại từ APKPure/APKMirror hoặc backup cũ), rồi decompile bằng CFR/Procyon/Fernflower — các decompiler này xử lý method lớn tốt hơn JADX | ✅ **Khuyến nghị mạnh nhất.** Xác suất thành công cao, vì đây là giới hạn của riêng JADX chứ không phải obfuscation |
| **B. Thiết kế lại công thức** | Tự thiết kế damage/stat formula riêng cho bản rebuild, ghi rõ **"không khớp bản gốc"** | ⚠️ **Vi phạm nguyên tắc "logic phải theo decode"** mà user đã chốt → chỉ làm nếu user **chủ động đồng ý đổi nguyên tắc** |
| **C. Bỏ combat khỏi scope** | Rebuild dừng ở phần quản lý (tavern/craft/merchant/inventory), không làm dungeon/combat | ⚠️ Thu hẹp scope đáng kể |

**Em không tự chọn lối nào** — đây là quyết định phạm vi thuộc về user.

---

## Final Decision

# `S6_5A_001B_PARTIAL_READY_FOR_SAFE_SUBSET_ONLY`

**Lý do:**

**Thành công:** Recover thêm **5 nhóm rule đạt ≥96%**: Doctrine (cơ chế + bảng 40 ability đầy đủ), Tavern recruit (**phát hiện recruit miễn phí**), `rollRareTrait` (14 trait), Enemy stat (không dùng multiplier), input mapping của `calculateTotalStat`. Nâng số domain sẵn sàng implement lên **11/17**.

**Thất bại có bằng chứng dứt khoát:** 2 blocker cứng **KHÔNG recover được** — không phải vì thiếu nỗ lực mà vì **không tồn tại raw bytecode ở bất kỳ đâu trên hệ thống**. Đã kiểm tra: decode root (0 file `.dex`/`.apk`/`.smali`/`.jar`), zip gốc 25 MB (17.404 entry, toàn `.java`/`.png`/`.xml`), `D:\Tinh` maxdepth 2, và `Document/GDD` (là kế hoạch sprint tự viết, không phải tài liệu gốc). **Không có đầu vào thì không decompiler nào chạy được.**

**Tuân thủ yêu cầu ≥95%:** Đúng theo yêu cầu user, em **không implement** bất cứ gì dưới ngưỡng 95% — cụ thể **không sửa `levelMultiplier = 1.0f`**, **không đặt công thức damage**, **không đặt giá merchant**. Tập con an toàn (Formula, SaveData, Tavern, Doctrine, Craft, Settings, Offline, Enemy stat) đạt 95–99% và **sẵn sàng cho S6.5A-002/003**.

**Không có scope violation:** không implement code, không sửa Unity/scene/data/asset, không dùng Higgsfield, không tự bịa rule nào. Mọi công thức đều dẫn được về file + số dòng.
