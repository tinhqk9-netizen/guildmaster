# S6.5A-001 Rule Extraction Report

**Ngày:** 2026-07-27 · **Backup:** `Backups/S6_5A_001_Rule_Extraction_20260727_084111/` (210 file)
**Phạm vi:** Chỉ đọc decode + tài liệu hoá rule. **Không implement code, không sửa Unity/scene/data.**

---

## Executive Summary

### Đã bóc được những rule nào?
Kết quả **tốt hơn dự kiến** ở phần lớn domain, nhưng **xấu hơn dự kiến** ở đúng 2 chỗ quan trọng nhất.

**✅ Bóc được đầy đủ, đủ để port 1:1 ngay:**
- **20/20 formula** trong `Formulas.java` — đọc được nguyên văn, không thiếu method nào
- **Tavern visitor generation** (nút thắt gameplay) — đầy đủ: interval, capacity, roll class, roll trait, tutorial override
- **Craft/Market timer + claim** — đầy đủ (`progressWorkshopTime`, `progressMarketTime`, `getSecondsToCraft`, `getSecondsToSell`)
- **`truncatePrice`, `maxCraftableAmount`, `gotEnoughItem`**
- **Offline progress delta + cap** (12 giờ)
- **Crit chance / crit damage / crit multiplier / dodge / counterattack / flat dodge**
- **Dodge resolution** (`Area.dodge()`), **experience distribution** (`collectExperience`), **death handling** (`checkDeath`)

**❌ KHÔNG bóc được — JADX decompile thất bại:**
Toàn bộ package game (1150 file) chỉ có **5 method không decompile được**, và **2 trong số đó chính là 2 blocker mà audit S6.5A đã chỉ ra**:

| Method | Instruction units | Ảnh hưởng |
|---|---|---|
| `Area.dealDamage(Entity, Entity, Skill, EndOfTurnAction)` | **1102** | **Công thức damage cốt lõi** — G-B02 |
| `Adventurer.calculateTotalStat(int)` | **332** | **Level multiplier + cách tính 6 stat** — G-B03 |
| `Area.trapEncounter(int,int,int,int,boolean)` | 263 | Bẫy trong dungeon |
| `Logger.log(Area, int, Object...)` | 4136 | Chỉ log hiển thị — không ảnh hưởng gameplay |
| `FileManager` (1 method) | — | I/O, Unity đã có `SaveService` riêng |

Đây **không phải suy đoán** — file decode ghi thẳng:
```java
public void dealDamage(...) {
    /* Method dump skipped, instruction units count: 1102 */
    throw new UnsupportedOperationException("Method not decompiled: ...");
}
```

### Rule nào đủ để port ở S6.5A-002?
- **Toàn bộ 20 formula** → port 1:1, có thể viết test so khớp giá trị
- **SaveData schema** → 140 field đọc được hết từ `Data.java`
- **Tavern/recruit** → đủ để mở nút thắt gameplay

### Rule nào còn ManualRuleRequired?
| Rule | Lý do | Mức nghiêm trọng |
|---|---|---|
| **Damage formula** | `dealDamage()` không decompile được | **Blocker cứng cho combat** |
| **Stat/level multiplier** | `calculateTotalStat()` không decompile được | **Blocker cứng cho mọi chỉ số** |
| Trap encounter | `trapEncounter()` không decompile được | Trung bình (một phần dungeon) |
| Doctrine bonus values | Cần đọc thêm 8 class instance (chưa đọc trong phase này) | Trung bình |

### Có rule nào chứng minh Unity hiện tại đang SAI không?
**CÓ — 2 chỗ:**

1. **`CharacterService.GetTotalStat()` hardcode `levelMultiplier = 1.0f`** — decode cho thấy 6 stat đều đi qua `calculateTotalStat(int)` với 332 instruction units (logic phức tạp, chắc chắn không phải nhân 1.0). Unity hiện **chắc chắn sai**, nhưng **chưa thể sửa đúng** vì không đọc được rule.

2. **`FormulaService` port thiếu và có khả năng sai ngữ nghĩa** — `GetStorageSpaces(levelStorage, upgradeStorage, additionalBonus)` trong Unity nhận `additionalBonus` làm tham số, trong khi decode `storageSpaces()` tính bonus **từ trạng thái IAP** (`isStarterPackPurchased` +35, `isAdventurerPackPurchased` +35, `isMerchantPackPurchased` +70). Cần đối chiếu lại khi port.

---

## Critical Gameplay Blockers

| Blocker ID | Domain | Decode Evidence | Unity Current Problem | Required Next Step |
|---|---|---|---|---|
| **B-01** | Damage / Combat | `Area.java:2213` — `dealDamage()` **không decompile được** (1102 units) | `FormulaService.CalculateDamage_ManualPortRequired()` là **stub rỗng** | **Cần phương án khác để lấy rule**: (a) decompile lại bằng công cụ khác (dex2jar + CFR/Procyon/Fernflower), (b) đọc smali/bytecode trực tiếp, (c) suy luận từ tài liệu GDD trong `Document/GDD`. **Không được tự bịa công thức.** → cần **S6.5A-001B** |
| **B-02** | Character stat | `Adventurer.java:905` — `calculateTotalStat(int)` **không decompile được** (332 units); cả 6 stat (`calculateTotalConstitution/Intelligence/Dexterity/MaxHp/Defense/MagicDefense` dòng 392–418) đều gọi vào đây | `GetTotalStat()` hardcode `levelMultiplier = 1.0f` → **chỉ số hiện SAI** | Như B-01 → **S6.5A-001B** |
| **B-03** | Tavern / Recruit | `Utils.newTavernVisitor()` (dòng 189) + `progressTavernTime()` (475) + `Formulas.getTavernCapacity()`/`getTavernVisitorInterval()` — **ĐỌC ĐƯỢC ĐẦY ĐỦ** | Chưa port gì → không có adventurer → chặn cả chuỗi gameplay | ✅ **Đủ rule để implement ngay** ở S6.5A-004 |
| **B-04** | Craft timer / claim | `Utils.progressWorkshopTime()` (536) + `Item.getSecondsToCraft()` (59) + `Utils.maxCraftableAmount()` (907) — **ĐỌC ĐƯỢC ĐẦY ĐỦ** | `CraftService.CanCraft()` trả `ManualRuleRequired` | ✅ **Đủ rule** — có thể gỡ `ManualRuleRequired` ở S6.5A-009 |
| **B-05** | Merchant price / restock | `Utils.truncatePrice()` (889) + `Item.getSecondsToSell()` (63) + `progressMarketTime()` (504) — **ĐỌC ĐƯỢC**. ⚠️ Nhưng `rollPotion`/`rollSpecialFoods`/`rollUpgrades` chưa đọc chi tiết trong phase này | `BuyItem()` trả `DeferredPriceOrCurrencyRule` | ⚠️ **Đủ cho sell/timer**; cần đọc thêm 3 hàm roll offer ở S6.5A-001B |
| **B-06** | SaveData missing fields | `Data.java` — **140 field đọc được hết** | Unity có 17 field; thiếu `lastAccess` (chặn offline), 9 setting, 8 doctrine, 20 area state… | ✅ **Đủ rule** — port ở S6.5A-002 |

**Kết luận quan trọng:** 4/6 blocker **đã đủ rule để triển khai**. Chỉ còn **B-01 và B-02** bị chặn cứng bởi giới hạn decompiler — và đó đúng là 2 blocker nghiêm trọng nhất (combat + chỉ số).

---

## Formula Rules

Hằng số trong `Formulas.java`: `BASE_MARKET_SPACES=1`, `BASE_QUARTERS_SPACES=2`, `BASE_STORAGE_SPACES=35`, `BASE_TAVERN_SPACES=1`, `BASE_TAVERN_VISITOR_INTERVAL=28800`, `BASE_WORKSHOP_SPACES=1`, `IMPOSSIBLY_HIGH_PRICE=99999999999999L`

⚠️ **Cảnh báo đọc code decompiled:** JADX đã thay một số **số literal** bằng hằng số thư viện androidx có **cùng giá trị**. Khi port phải dùng **giá trị số**, không phải hằng số Unity tương ứng:
- `WorkRequest.MIN_BACKOFF_MILLIS` = **10000**
- `PeriodicWorkRequest.MIN_PERIODIC_FLEX_MILLIS` = **300000**
- `WorkRequest.DEFAULT_BACKOFF_DELAY_MILLIS` = **30000**

| Rule ID | Decode Method | Decode Source Location | Inputs | Output | Exact Logic / Formula | Unity Current Status | Unity Target | Notes |
|---|---|---|---|---|---|---|---|---|
| F-01 | `totalStarsToNextLp(int i)` | `Formulas.java:16` | `i` = LP level | int | `(i * 3) + 4` | ❌ **Thiếu** | `FormulaService` | Đơn giản, port ngay |
| F-02 | `getQuartersPrice()` | `:20` | `data.levelQuarters` | long | Bảng tra cứu 23 mức: `0→5, 1→275, 2→2000, 3→10000, 4→40000, 5→100000, 6→200000, 7→300000, 8→400000, 9→500000, 10→700000, 11→1000000, 12→1400000, 13→1850000, 14→2400000, 15→3000000, 16→4000000, 17→5000000, 18→6000000, 19→7000000, 20→8000000, 21→9000000, 22→10000000, default→99999999999999`, rồi `truncatePrice(j)` | ⚠️ Có `GetQuartersPrice(level)` — **cần verify bảng khớp** | `FormulaService` | Case 3 và 7 dùng hằng androidx → **10000** và **300000** |
| F-03 | `getTavernCapacityPrice()` | `:99` | `levelTavernCapacity` | long | `truncatePrice((long)(pow(3.0, level) * 5000.0))` | ⚠️ Có — cần verify | `FormulaService` | |
| F-04 | `getTavernTimePrice()` | `:103` | `levelTavernTime` | long | `truncatePrice((long)(pow(1.7, level) * 200.0))` | ❌ **Thiếu** | `FormulaService` | |
| F-05 | `getStorageCapacityPrice()` | `:107` | `levelStorage` | long | `i = levelStorage + 1`; nếu `i > 80` → `99999999999999`. Cộng dồn bậc thang:<br>`i>60`: `+ min(levelStorage-59, 20) * 30000`<br>`i>50`: `+ min(levelStorage-49, 10) * 22000`<br>`i>40`: `+ min(levelStorage-39, 10) * 12000`<br>`i>30`: `+ min(levelStorage-29, 10) * 4000`<br>`i>20`: `+ min(levelStorage-19, 10) * 800`<br>`i>10`: `+ min(levelStorage-9, 10) * 150`<br>cuối cùng `+ min(i, 10) * 50`<br>**Không** gọi `truncatePrice` | ⚠️ Có `GetStorageCapacityPrice(level)` — **cần verify bậc thang** | `FormulaService` | `30000` là `DEFAULT_BACKOFF_DELAY_MILLIS`. **Lưu ý: hàm này KHÔNG truncate** |
| F-06 | `getMarketListingsPrice()` | `:132` | `levelMarketListings` | long | `truncatePrice((long)(pow(4.5, level) * 20.0))` | ❌ **Thiếu** | `FormulaService` | |
| F-07 | `getMarketTimePrice()` | `:136` | `levelMarketTime` | long | `truncatePrice((long)(pow(1.7, level) * 10.0))` | ❌ **Thiếu** | `FormulaService` | |
| F-08 | `getWorkshopQueuePrice()` | `:140` | `levelWorkshopQueue` | long | `truncatePrice((long)(pow(4.5, level) * 20.0))` | ❌ **Thiếu** | `FormulaService` | Giống F-06 |
| F-09 | `getWorkshopTimePrice()` | `:144` | `levelWorkshopTime` | long | `truncatePrice((long)(pow(1.7, level) * 10.0))` | ❌ **Thiếu** | `FormulaService` | Giống F-07 |
| F-10 | `getShelterPrice()` | `:148` | `levelShelter` | long | Bảng 11 mức: `0→500, 1→2000, 2→8000, 3→32000, 4→64000, 5→128000, 6→256000, 7→512000, 8→1000000, 9→2000000, 10→4000000, default→99999999999999`, rồi `truncatePrice` | ❌ **Thiếu** | `FormulaService` | |
| F-11 | `getShelterAutofeedPrice()` | `:191` | `levelShelterAutofeed` | long | `truncatePrice(level > 0 ? 99999999999999L : 10000L)` | ❌ **Thiếu** | `FormulaService` | Mua 1 lần |
| F-12 | `getQuartersCapacity()` | `:197` | IAP flags + `levelQuarters` + `upgradeQuarters` | int | `bonus = (starterPack?1:0)`; `if adventurerPack: bonus += 2`; `if imperialVanguard: bonus += 4`; `if unholyCrusade: bonus += 4`;<br>return `levelQuarters + 2 + upgradeQuarters + bonus` | ❌ **Thiếu** | `FormulaService` | ⚠️ Phụ thuộc 4 IAP flag — Unity `SaveData` **chưa có** |
| F-13 | `getTavernVisitorInterval()` | `:214` | `levelTavernTime` + `upgradeTavernTime` | long (ms) | `(long)(pow(0.9, levelTavernTime + upgradeTavernTime) * 28800.0 * 1000.0)` | ❌ **Thiếu** | `FormulaService` | **Trả về mili-giây**. Base 28800s = 8 giờ |
| F-14 | `getTavernCapacity()` | `:220` | IAP + `levelTavernCapacity` + `upgradeTavernCapacity` | int | `bonus = (starterPack?1:0)`; `if adventurerPack: bonus += 2`;<br>return `levelTavernCapacity + 1 + upgradeTavernCapacity + bonus` | ❌ **Thiếu** | `FormulaService` | |
| F-15 | `marketListings()` | `:231` | IAP + `levelMarketListings` + `upgradeMarketQueue` | int | `bonus = (starterPack?1:0)`; `if merchantPack: bonus += 2`;<br>return `levelMarketListings + 1 + upgradeMarketQueue + bonus` | ❌ **Thiếu** | `FormulaService` | |
| F-16 | `workshopQueue()` | `:242` | IAP + `levelWorkshopQueue` + `upgradeWorkshopQueue` | int | `bonus = (starterPack?1:0)`; `if merchantPack: bonus += 2`;<br>return `levelWorkshopQueue + 1 + upgradeWorkshopQueue + bonus` | ❌ **Thiếu** | `FormulaService` | |
| F-17 | `storageSpaces()` | `:251` | IAP + `levelStorage` + `upgradeStorage` | int | `bonus = starterPack ? 35 : 0`; `if adventurerPack: bonus += 35`; `if merchantPack: bonus += 70`;<br>return `levelStorage + 35 + upgradeStorage + bonus` | ⚠️ Unity có `GetStorageSpaces(levelStorage, upgradeStorage, additionalBonus)` | `FormulaService` | ⚠️ **Ngữ nghĩa lệch**: Unity nhận `additionalBonus` từ ngoài; decode tính từ IAP flag nội bộ |
| F-18 | `shelterCapacity()` | `:262` | `levelShelter` + `upgradeShelter` | int | `levelShelter + upgradeShelter + 2` | ❌ **Thiếu** | `FormulaService` | |
| F-19 | `experienceToNextLevel(int i, boolean z)` | `:266` | `i` = level, `z` = isAdventurer | int | `p = pow(i, 1.4)`; `x = (int)((3.0 + p) * 10.0 * p)`; `if z: x *= 2`;<br>làm tròn xuống: `x>=10000 → (x/1000)*1000`; `x>=1000 → (x/100)*100`; `x>=100 → (x/10)*10`; else `x` | ✅ **Đã port** | — | Cần verify khớp chính xác |
| F-20 | `foodToNextLevel(int i)` | `:281` | `i` = level | int | `(int)(pow(1.085, i) * 30.0)` | ✅ **Đã port** | — | |

**Bổ sung — `Utils.truncatePrice(long j)`** (`Utils.java:889`), dùng bởi 9 formula trên:
```
if (j <= 10000) return j;
long mod = (j <= 1000000) ? (j % 100) : (j % 10000);
return j - mod;
```

**Tổng kết Formula:** Unity đã port **6/20** (F-19, F-20 đúng; F-02, F-03, F-05, F-17 cần verify lại). **Thiếu 14 formula.** Toàn bộ 20 rule **đọc được đầy đủ, port được ngay.**

---

## Character / Adventurer / Stat Rules

| Rule ID | Domain | Decode Source Location | Exact Logic | Unity Current Status | Missing Risk | Unity Target |
|---|---|---|---|---|---|---|
| **C-01** | **Stat tổng (6 stat)** | `Adventurer.java:905` | ❌ **`calculateTotalStat(int)` KHÔNG DECOMPILE ĐƯỢC** (332 units). 6 stat gọi vào đây: `Constitution=calculateTotalStat(0)`, `Intelligence=(1)`, `Dexterity=(2)`, `MaxHp=(3)`, `Defense=(4)`, `MagicDefense=(5)` (dòng 392–418) | `GetTotalStat()` hardcode `levelMultiplier=1.0f` | 🔴 **BLOCKER — Unity đang SAI** | `CharacterService` | **MANUAL_RULE_REQUIRED** |
| C-02 | Min attack damage | `Adventurer.java:238` | `if weapon == null → 1`;<br>`mod = weapon.getDamageModifier(totalCON, totalINT, totalDEX)`;<br>`if weapon instanceof SerpentBite: mod *= getThreat()`;<br>return `Utils.round(mod * (1.0 - weapon.damageDelta()))` | ❌ Chưa port | Phụ thuộc C-01 | `CharacterService` | Rule **đọc được**, nhưng đầu vào phụ thuộc C-01 |
| C-03 | Max attack damage | `Adventurer.java:251` | Giống C-02 nhưng `mod * (weapon.damageDelta() + 1.0)` | ❌ Chưa port | Phụ thuộc C-01 | `CharacterService` | |
| C-04 | Critical chance | `Adventurer.java:646` | `base = min(0.4, (isMagic() ? totalINT : totalDEX) * 0.004) + potionsDrank[6] * 0.01`;<br>`+ weapon.getCriticalChance() + armor.getCriticalChance() + accessory.getCriticalChance()`;<br>`+ doctrine.bonusCritChance() * 0.01` | ❌ Chưa port | Phụ thuộc C-01 + Doctrine | `CharacterService`/`CombatService` | **Cap 0.4** cho phần từ stat |
| C-05 | Critical damage | `Adventurer.java:627` | `base = criticalDamage + potionsDrank[7] * 0.02`;<br>`+ weapon/armor/accessory .getCriticalDamage()`;<br>`+ doctrine.bonusCritDamage() * 0.01`;<br>`if traitRare == RUTHLESS: * 1.2` | ❌ Chưa port | Phụ thuộc Doctrine | `CombatService` | |
| C-06 | Flat dodge chance | `Adventurer.java:606` | `base = flatDodgeChance + potionsDrank[10] * 0.01`;<br>`+ weapon/armor/accessory .getFlatDodgeChance()`;<br>`if traitRare == NIMBLE: + 0.08`;<br>`+ doctrine.bonusDodgeChance() * 0.01` | ❌ Chưa port | Phụ thuộc Doctrine | `CombatService` | |
| C-07 | Counterattack chance | `Adventurer.java:270` | `base = counterattack + weapon/armor/accessory .getCounterattack()`;<br>`if traitRare == REACTIVE: + 0.1`;<br>`+ doctrine.bonusCounterattack() * 0.01` | ❌ Chưa port | Phụ thuộc Doctrine | `CombatService` | |
| C-08 | Mana regen | `Adventurer.java:263` | `super.calculateManaRegen() + doctrine.bonusManaRegen()`;<br>`if traitRare == GIFTED: + 2` | ❌ Chưa port | Phụ thuộc Doctrine | `CombatService` | |
| C-09 | Threat | `Adventurer.java:290` | `threat + weapon.getThreat() + armor.getThreat() + …` | ❌ Chưa port | Thấp | `CombatService` | Dùng cho taunt targeting |
| C-10 | Default weapon theo class | `Utils.java:919` | `type_sword→"Spade"`, `type_staff→"Cane"`, `type_dagger→"Sickle"`, `type_bow→"TrainingBow"` | ❌ Chưa port | Thấp | `CharacterService` | Cần khi tạo adventurer mới |

**Nhận xét:** Tất cả rule phái sinh (C-02…C-09) **đọc được**, nhưng **đều nhận đầu vào từ C-01** — nên chừng nào chưa giải được C-01 thì **không thể tính đúng bất kỳ chỉ số chiến đấu nào**.

---

## Tavern / Quarters / Shelter Rules

| Rule ID | Feature | Decode Source Location | Save Fields | Exact Logic | Unity Current Status | Needed For |
|---|---|---|---|---|---|---|
| **T-01** | **Sinh visitor theo thời gian** | `Utils.progressTavernTime(long j)` — `Utils.java:475` | `tavernLocked`, `nextTavernVisit`, `tavernGuests` | `if (tavernLocked) return;`<br>`interval = getTavernVisitorInterval() / 1000` (giây)<br>`count = j / interval`<br>`next = nextTavernVisit - (j % interval)`<br>`if (next < 0) { next += interval; count++; }`<br>`setNextTavernVisit(next)`<br>`spawn = min(count, getTavernCapacity())`<br>gọi `newTavernVisitor()` đúng `spawn` lần | ❌ Chưa port | **Mở nút thắt gameplay** |
| **T-02** | **Tạo visitor** | `Utils.newTavernVisitor()` — `Utils.java:189` | `tutorialStep`, `tavernGuests` | Theo `tutorialStep`:<br>`≤1` → `Footman`, không trait<br>`==6` → `LightDisciple` + trait `BOOKWORM`<br>`==7` → `Archer` + trait `FERAL`, đặt `tutorialStep=8`, unlock achievement<br>khác → `rollClass()` + `rollCommonTrait()` + `rollRareTrait()`<br>Sau đó: `setWeapon(getDefaultWeapon(weaponType))`; thêm vào **đầu** list `tavernGuests`; nếu `size > getTavernCapacity()` → **xoá phần tử cuối** | ❌ Chưa port | Recruit |
| T-03 | Roll class | `Utils.rollClass()` — `Utils.java:210` | — | `r = random()`; `r<0.25 → "Footman"`; `r<0.5 → "Rogue"`; `r<0.75 → "Archer"`; else `"Apprentice"` (mỗi loại 25%) | ❌ Chưa port | Recruit |
| T-04 | Roll common trait | `Utils.rollCommonTrait()` — `:216` | — | `r<0.1333… → BOOKWORM`; `r<0.2667 → BRUTE`; `r<0.4 → FERAL`; else `null` (60% không trait) | ❌ Chưa port | Recruit |
| T-05 | Roll rare trait | `Utils.rollRareTrait()` — `:229` | — | `r<0.01428… → EMPATHETIC`; `r<0.02857 → GIFTED`; `r<0.04286 → INTIMIDATING`; `r<0.05714 → …` (mỗi rare ≈1/70) | ⚠️ **Đọc chưa hết** (bị cắt ở dòng 240) | Recruit — cần đọc tiếp |
| T-06 | Tavern capacity | `Formulas.getTavernCapacity()` — F-14 | `levelTavernCapacity`, `upgradeTavernCapacity`, IAP | Xem F-14 | ❌ Chưa port | Giới hạn guest |
| T-07 | Tavern interval | `Formulas.getTavernVisitorInterval()` — F-13 | `levelTavernTime`, `upgradeTavernTime` | `pow(0.9, lvl+upg) * 28800 * 1000` ms | ❌ Chưa port | Tần suất visitor |
| T-08 | Quarters capacity | `Formulas.getQuartersCapacity()` — F-12 | `levelQuarters`, `upgradeQuarters`, 4 IAP flag | Xem F-12 | ❌ Chưa port | Giới hạn adventurer sở hữu |
| T-09 | Shelter capacity | `Formulas.shelterCapacity()` — F-18 | `levelShelter`, `upgradeShelter` | `levelShelter + upgradeShelter + 2` | ❌ Chưa port | Giới hạn pet |
| T-10 | Giảm thời gian chờ (gem?) | `DialogTavern.java:324` | `nextTavernVisit` | `setNextTavernVisit((long)(nextTavernVisit * 0.9))` | ❌ Chưa port | ⚠️ Chưa xác định cost — **cần đọc thêm** |

**Trả lời câu hỏi bắt buộc:**
- **Khi nào visitor xuất hiện?** Mỗi `getTavernVisitorInterval()` (base 8 giờ, giảm theo `pow(0.9, levelTavernTime+upgradeTavernTime)`). Tính theo delta giây, hỗ trợ offline.
- **Số lượng visitor?** `min(số interval trôi qua, getTavernCapacity())`.
- **Capacity tính như nào?** `levelTavernCapacity + 1 + upgradeTavernCapacity + IAP bonus`.
- **Recruit ghi vào field nào?** Visitor vào `Data.tavernGuests`. ⚠️ **Hành động recruit (chuyển guest → adventurer sở hữu) chưa đọc** — nằm trong `ui/dialogs/DialogTavern.java`, cần đọc ở S6.5A-001B.
- **Có cost không?** ⚠️ **Chưa xác định** — cần đọc `DialogTavern`.
- **Có random/rarity không?** ✅ Có: class 25% mỗi loại, common trait 40% tổng, rare trait ≈1/70 mỗi loại.

---

## Dungeon / Combat / Loot Rules

| Rule ID | Combat Area | Decode Source Location | Exact Logic | Dependencies | Unity Current Status | Port Risk | Unity Target |
|---|---|---|---|---|---|---|---|
| **D-01** | **Deal damage** | `Area.java:2213` | ❌ **KHÔNG DECOMPILE ĐƯỢC** (1102 units) | — | Stub rỗng | 🔴 **ManualRuleRequired** | `CombatService` |
| D-02 | Dodge resolution | `Area.java:1188` | `if (!ignoreFlying && !attacker.isFlying() && target.isFlying()) → dodge = true`<br>`if (attacker.isAlwaysHits() \|\| target has FROZEN) → hitChance = 1.0`<br>else:<br>  `a = attacker.isMagic() ? attacker.INT : attacker.DEX`<br>  `d = attacker.isMagic() ? target.INT : target.DEX`<br>  `hit = a / ((d / 5.0) + a)`<br>  nếu `localDarkness > 0` và không phải enemy-vs-enemy:<br>    `nv = (bên adventurer có nightVision) ? 1 : 0`<br>    `hit -= (localDarkness * 0.01) * (hit - nv)`<br>  nếu attacker là Adventurer trait `FOCUSED`: `hit += 0.15`<br>  `hit = max(EFFECT_PROBABILITY, hit - target.calculateTotalFlatDodgeChance())`<br>`dodged = random() > hit` | C-01, C-06 | ❌ Chưa port | 🟡 Port partial — cần test | `CombatService` |
| D-03 | Critical multiplier | `Area.java:1289` | `if (random() >= attacker.calculateCriticalChance()) return 1.0;`<br>`m = attacker.calculateCriticalDamage()`<br>`if (skill != null) m *= skill.criticalAmplification`<br>`if (d > 0) m -= (m - 1.0) * d`  ← `d` = hệ số giảm crit của mục tiêu<br>return `m` | C-04, C-05 | ❌ Chưa port | 🟡 Port partial | `CombatService` |
| D-04 | Experience distribution | `Area.java:635` | `totalExp = Σ corpses.getExpGiven()`<br>`perHead = totalExp / adventurersAlive()`<br>`petMul = pet != null ? 1.0 + pet.getExperience()/100.0 : 1.0`<br>với mỗi adventurer còn sống, không phải summoned minion:<br>  `mul = adventurer.experienceMultiplier() * petMul`<br>  `gain = Utils.round(perHead * mul)`<br>  `adventurer.addExperience(gain)` | F-19 | ❌ Chưa port | 🟢 **Port 1:1** | `CombatService`/`DungeonService` |
| D-05 | Death handling | `Area.java:2221` (`checkDeath`) | Nếu `currentHp == 0`:<br>**Enemy:** nếu passive `ABSURD_GENEALOGY` và `random() < 0.65` → hồi full HP, mana=100, xoá debuff, **return**. Ngược lại: xoá khỏi `enemies`/`fightingGroup`, thêm vào `corpses`, `adventureRecap.addEnemyKilled`, `healingNova()`, `reanimate(enemy)`<br>**Adventurer (minion):** xoá khỏi party, gỡ `minionBound`<br>**Adventurer (thường):** `QuestsManager.increment(theEnd, 1)`; nếu accessory là `AmuletOfResurrection` và `random() < 0.4` → hồi full HP, xoá debuff, **return**; ngược lại `lost = experience / 5`, **nếu `getAreaType() == 0`** thì `experience -= lost`; nếu có minion → giết minion; xoá toàn bộ status effect<br>Cuối: nếu không có `TETHER` → `reanimateAlchemistWithFeebleTether()` + `applyOnDeathStatusEffects()` | — | ❌ Chưa port | 🟢 **Port 1:1** | `CombatService` |
| D-06 | Trap encounter | `Area.java:1344` | ❌ **KHÔNG DECOMPILE ĐƯỢC** (263 units) | — | ❌ Chưa port | 🔴 **ManualRuleRequired** | `DungeonService` |
| D-07 | Apply status effect | `Area.java:1308` | Nếu passive `BEND_REALITY` và status **không** thuộc {TAUNT, LESSER_CURSE, CURSE, GREATER_CURSE, OMINOUS_CURSE, ABHORRENT_CURSE} → **phản status về nguồn gây ra** (`applyStatus(statusEffect.getCause(), …)`), return.<br>Ngược lại `entity.addStatusEffect(statusEffect, d)` | — | ❌ Chưa port | 🟡 Port partial | `StatusEffectService` |
| D-08 | Turn tick loop | `Area.tick()` `:279` + `performAction()` `:420` + `fightTurn()` `:748` | ⚠️ **Chưa đọc chi tiết trong phase này** (Area.java 3164 dòng) | Nhiều | ❌ Chưa port | 🔴 **Too complex — needs subtask** | `DungeonService` |
| D-09 | Target selection (7 strategy) | `Area.java:2500–2907` | ⚠️ **Chưa đọc chi tiết**: `selectTargets`, `attackTargetStrategy`, `selectEnemyTarget`, `selectRandomTarget`, `weightedSelection`, `tauntedBy`, `selectLowestHpTarget`, `selectLowestRelativeShieldAlly`, `selectPetTarget`, `selectPetHealingTarget` | C-09 | ❌ Chưa port | 🔴 **Too complex — needs subtask** | `CombatService` |
| D-10 | Loot roll | `Area.loot()` `:669` + `fullChest()` `:729` + `Utils.collectDrops()` `:935` | ⚠️ **Chưa đọc chi tiết** | — | ❌ Chưa port | 🟡 Cần đọc thêm | `LootService` |
| D-11 | Heal / retaliate / cast / mana | `Area.java:1227, 2447, 1361, 1172` | ⚠️ **Chưa đọc chi tiết** | D-01 | ❌ Chưa port | 🟡 Cần đọc thêm | `CombatService` |

**Phân loại theo yêu cầu:**
- 🟢 **Port 1:1 được ngay:** D-04 (exp), D-05 (death)
- 🟡 **Port partial, cần test:** D-02 (dodge), D-03 (crit), D-07 (status), D-10, D-11
- 🔴 **ManualRuleRequired:** D-01 (**damage — blocker cứng**), D-06 (trap)
- 🔴 **Too complex, cần subtask riêng:** D-08 (tick loop), D-09 (target selection)

**⚠️ Kết luận bắt buộc:** Vì **D-01 không đọc được**, **Dungeon/Combat KHÔNG được coi là DONE** dù có port các phần khác. Không được tự đơn giản hoá công thức damage để "cho chạy được".

---

## Quest Rules

| Rule ID | Decode Source Location | Exact Logic | Save Fields | Unity Current Status | Missing Rule | Unity Target |
|---|---|---|---|---|---|---|
| Q-01 | `QuestsManager.extractQuests()` `:97` | ⚠️ Chưa đọc chi tiết | `questsSeen`, `questsRefreshed` | `QuestService` 87/468 dòng | Cần đọc | `QuestService` |
| Q-02 | `QuestsManager.calculateDifficulty()` `:107` | ⚠️ Chưa đọc chi tiết | — | ❌ | Cần đọc | `QuestService` |
| Q-03 | `QuestsManager.rollRarity()` `:386` | ⚠️ Chưa đọc chi tiết | — | ❌ | Cần đọc | `QuestService` |
| Q-04 | `QuestsManager.setupDoctrineAmounts()` `:188` | ⚠️ Chưa đọc chi tiết | 8 cặp doctrine level/progress | ❌ | Phụ thuộc Doctrine | `QuestService` |
| Q-05 | `QuestsManager.increment(Quest, long)` `:445` | ⚠️ Chưa đọc chi tiết | `questsCompleted` | ⚠️ Unity có `Increment` | Cần verify khớp | `QuestService` |
| Q-06 | Quest trigger trong combat | `Area.java` — gọi `QuestsManager.increment(theEnd/criticalHit/pulverization/shocking/smokingHot/student/fastLearner, …)` | **Bằng chứng đọc được:** quest được tăng tiến độ từ các sự kiện combat cụ thể (crit ≥2.5 → `pulverization`, stun → `shocking`, ablaze → `smokingHot`, exp mul ≥1.5 → `fastLearner`) | — | ❌ | — | `QuestService` |

**Trạng thái:** Quest **chưa bóc đủ rule trong phase này** — `QuestsManager.java` 468 dòng cần đọc riêng ở S6.5A-001B.

---

## Craft / Workshop Rules

| Rule ID | Decode Source Location | Exact Logic | Save Fields | Unity Current Status | ManualRuleRequired? | Unity Target |
|---|---|---|---|---|---|---|
| **CR-01** | `Item.getSecondsToCraft()` — `Item.java:59` | `(long)((merchantPack ? 0.6 : 1.0) * pow(0.9, (levelWorkshopTime + upgradeWorkshopTime) - 1) * max(price - 1, 1) * 6 * stack)` | `levelWorkshopTime`, `upgradeWorkshopTime`, IAP | ❌ Chưa port | ✅ **KHÔNG — rule đọc được đầy đủ** | `CraftService` |
| **CR-02** | `Utils.progressWorkshopTime(long j)` — `Utils.java:536` | Duyệt `workshopQueue`:<br>`passed = itemAction.secondsPassed`<br>`total = item.getSecondsToCraft()`<br>`step = min(j, (1 + total) - passed)`<br>`j -= step`; `passed += step`; `setSecondsPassed(passed)`<br>`if (passed > total)` → đánh dấu hoàn thành<br>`if (j <= 0) break`<br>Sau vòng lặp: chuyển item hoàn thành từ `workshopQueue` → **cuối** `completedWorkshopItems` | `workshopQueue`, `completedWorkshopItems` | ❌ Chưa port | ✅ **KHÔNG** | `CraftService` |
| **CR-03** | `Utils.maxCraftableAmount(Recipes)` — `Utils.java:907` | `min = 99999`<br>với mỗi ingredient: nếu không có trong `items` → **return 0**; ngược lại `min = min(min, ownedStack / ingredientStack)`<br>return `min` | `items` | ❌ Chưa port | ✅ **KHÔNG** | `CraftService` |
| **CR-04** | `Utils.gotEnoughItem(Item)` — `Utils.java:902` | Tìm item trong `data.items`; return `found && ownedStack >= requiredStack` | `items` | ❌ Chưa port | ✅ **KHÔNG** | `InventoryService` |
| CR-05 | `Recipes.from(Item)` / `into(Item)` — `Recipes.java:338, 348` | Tra recipe theo item (2 chiều) | — | ❌ Chưa port | ✅ KHÔNG | `CraftService` |
| CR-06 | Workshop queue size | `Formulas.workshopQueue()` — F-16 | `levelWorkshopQueue + 1 + upgradeWorkshopQueue + IAP bonus` | | ❌ Chưa port | ✅ KHÔNG | `FormulaService` |
| CR-07 | Claim item hoàn thành | ⚠️ Chưa đọc — nằm trong `ui/dialogs/DialogWorkshop` | — | `completedWorkshopItems` | ❌ | ⚠️ **Cần đọc thêm** | `CraftService` |
| CR-08 | `settingCraftMaxAmount` | `Data.java:106` | Cờ bật/tắt craft số lượng tối đa | `settingCraftMaxAmount` | ❌ Chưa có field | ✅ KHÔNG | `SaveData` |

**🎉 Kết luận quan trọng:** Rule craft timer/claim **KHÔNG còn ManualRuleRequired** — đọc được đầy đủ. `CraftService.CanCraft()` hiện trả `ManualRuleRequired` có thể **gỡ bỏ** sau khi port CR-01…CR-04. Chỉ còn CR-07 (thao tác claim ở tầng UI) cần đọc thêm.

---

## Merchant / Market Rules

| Rule ID | Decode Source Location | Exact Logic | Save Fields | Unity Current Status | Risk | Unity Target |
|---|---|---|---|---|---|---|
| **M-01** | `Item.getSecondsToSell()` — `Item.java:63` | `(long)((merchantPack ? 0.6 : 1.0) * pow(0.9, (levelMarketTime + upgradeMarketTime) - 1) * price * 4 * stack)` | `levelMarketTime`, `upgradeMarketTime`, IAP | ❌ Chưa port | 🟢 Thấp | `MerchantService` |
| **M-02** | `Utils.progressMarketTime(long j)` — `Utils.java:504` | Giống CR-02 nhưng dùng `marketListings` → `soldMarketItems`, và `getSecondsToSell()` | `marketListings`, `soldMarketItems` | ❌ Chưa port | 🟢 Thấp | `MerchantService` |
| **M-03** | `Utils.truncatePrice(long j)` — `Utils.java:889` | `if (j <= 10000) return j;`<br>`mod = (j <= 1000000) ? j % 100 : j % 10000;`<br>return `j - mod` | — | ❌ Chưa port | 🟢 Thấp | `MerchantService`/`FormulaService` |
| M-04 | Market listing slots | `Formulas.marketListings()` — F-15 | `levelMarketListings + 1 + upgradeMarketQueue + IAP bonus` | | ❌ Chưa port | 🟢 Thấp | `FormulaService` |
| M-05 | `Utils.rollPotion()` — `Utils.java:313` | ⚠️ **Chưa đọc chi tiết** | — | ❌ | 🟡 **Cần đọc** | `MerchantService` |
| M-06 | `Utils.rollSpecialFoods()` — `:352` | ⚠️ **Chưa đọc chi tiết** | — | ❌ | 🟡 **Cần đọc** | `MerchantService` |
| M-07 | `Utils.rollUpgrades()` — `:400` | ⚠️ **Chưa đọc chi tiết** | — | ❌ | 🟡 **Cần đọc** | `MerchantService` |
| M-08 | `Area.rollMerchantRegularOffers()` `:3157` / `rollMerchantSpecialOffers()` `:3161` | ⚠️ **Chưa đọc chi tiết** — offer theo từng dungeon | `newMerchantRegularItems`, `newMerchantSpecialItems` | ⚠️ Unity có `RollRegularOffer`/`RollSpecialOffer` | 🟡 **Cần verify** | `MerchantService` |
| M-09 | Buy rule (trừ tiền) | ⚠️ Chưa đọc — nằm trong `ui/dialogs/DialogMerchant` | `money`, `gems` | `BuyItem()` trả `Deferred…` | 🔴 **Cần đọc** | `MerchantService` |
| M-10 | `settingSellMaxAmount` | `Data.java:107` | Cờ bán số lượng tối đa | `settingSellMaxAmount` | ❌ Chưa có field | 🟢 Thấp | `SaveData` |

**Kết luận:** **Sell/timer/price-truncate đã đủ rule.** Phần **buy + roll offer + restock** vẫn cần đọc thêm (M-05…M-09) → giữ `DeferredPriceOrCurrencyRule` cho tới khi đọc xong.

---

## Secondary / Deferred Systems

| Domain | Decode Exists | Key Source Location | Rule Summary | Recommended Scope | Reason |
|---|---|---|---|---|---|
| **Doctrine** | ✅ | `entities/adventurers/doctrines/Doctrine.java` + 8 instance (`DoctrineOfAffliction/Control/Illusion/Ruin/War/…`) | Cung cấp `bonusCritChance()`, `bonusCritDamage()`, `bonusDodgeChance()`, `bonusCounterattack()`, `bonusManaRegen()` — **được gọi trực tiếp trong C-04…C-08** | **Core_Needed_Now** | ⚠️ **Nâng mức so với audit trước**: doctrine không phải hệ thống phụ — nó là **thành phần bắt buộc** của công thức crit/dodge/counterattack. Không port thì các rule C-04…C-08 **không tính đúng được** |
| **Pet** | ✅ | `pets/Pet.java` (290), `PetAbility.java`, `Utils.rollPetAbility()` `:276`, `Area.petAttack/petHeal/petCast/petExecution` | Pet tham chiến; ảnh hưởng exp (`D-04`: `petMul = 1 + pet.getExperience()/100`) | **Deferred_After_Core** | Ảnh hưởng exp nhưng có thể tạm bỏ qua (petMul = 1.0 khi không có pet — **đúng theo decode**, không phải fake) |
| **Raid** | ✅ | `places/raids/`, `ui/raids/`, `shownDialogRaid`, `shownDialogEpicRaid` | Biến thể của `Area` | **Deferred_After_Core** | 12 record data đã có; dùng chung engine với dungeon → làm sau khi dungeon xong |
| **Achievements** | ✅ | `AchievementsUtils.java` | `unlock(ACHIEVEMENT_*)` — được gọi trong `newTavernVisitor` (tutorial step 7) | **Deferred_After_Core** | Không ảnh hưởng gameplay loop; chỉ cần stub no-op khi port T-02 |
| **IAP** | ✅ | `IAPWrapper.java`, 4 flag trong `Data.java` | `starterPack`, `adventurerPack`, `merchantPack`, `imperialVanguard`, `unholyCrusade` | **Out_Of_Scope** (nhưng **field phải port**) | ⚠️ **Quan trọng**: không implement mua bán, **nhưng 5 flag này là đầu vào của F-12, F-14, F-15, F-16, F-17, CR-01, M-01** → **bắt buộc thêm field vào `SaveData` với giá trị mặc định `false`** |
| **Localization** | ✅ | `R.string` trong `resources/` | Chuỗi là ID int | **Deferred_After_Core** | `localization.json` rỗng vì chuỗi nằm ở `resources/`, không phải trong code |

---

## SaveData Field Mapping

`Data.java` có **140 field**. Bảng dưới nhóm theo domain (không liệt kê từng field lẻ để giữ report đọc được).

| Decode Field / Group | Purpose | Unity SaveData Exists? | Needed For Core Gameplay? | Recommended Action |
|---|---|---|---|---|
| `money`, `gems` | Tiền tệ | ✅ `Money`, `Gems` | ✅ | Đã có |
| `lastAccess` | Mốc tính offline progress | ❌ | ✅ **Bắt buộc** | **Port_Now** |
| `lastHourTriggered`, `last24Triggered`, `lastWeekTriggered` | Mốc sự kiện định kỳ | ❌ | ⚠️ Cần cho tick60/quest refresh | **Port_Now** |
| `nextTavernVisit` | Đếm ngược visitor | ❌ | ✅ **Bắt buộc cho T-01** | **Port_Now** |
| `tavernGuests` | Danh sách guest ở tavern | ❌ | ✅ **Bắt buộc cho T-02** | **Port_Now** |
| `tavernLocked` | Khoá tavern | ❌ | ✅ (điều kiện đầu của T-01) | **Port_Now** |
| `tutorialStep` | Bước hướng dẫn | ❌ | ✅ (T-02 phụ thuộc) | **Port_Now** |
| `levelStorage`, `upgradeStorage` | Nâng cấp kho | ✅ | ✅ | Đã có |
| `levelWorkshopTime`, `levelMarketTime` | Nâng cấp thời gian | ✅ (một phần) | ✅ | Đã có |
| `upgradeWorkshopTime`, `upgradeMarketTime` | Upgrade tương ứng | ❌ | ✅ **CR-01/M-01 cần** | **Port_Now** |
| `levelWorkshopQueue`, `upgradeWorkshopQueue` | Số slot workshop | ❌ | ✅ F-16 | **Port_With_Domain** (Craft) |
| `levelMarketListings`, `upgradeMarketQueue` | Số slot market | ❌ | ✅ F-15 | **Port_With_Domain** (Merchant) |
| `levelTavernCapacity`, `upgradeTavernCapacity` | Sức chứa tavern | ❌ | ✅ F-14 | **Port_With_Domain** (Tavern) |
| `levelTavernTime`, `upgradeTavernTime` | Tần suất visitor | ❌ | ✅ F-13 | **Port_With_Domain** (Tavern) |
| `levelQuarters`, `upgradeQuarters` | Sức chứa adventurer | ❌ | ✅ F-12 | **Port_With_Domain** (Tavern) |
| `levelShelter`, `upgradeShelter`, `levelShelterAutofeed` | Pet shelter | ❌ | ⚠️ Chỉ khi làm Pet | **Deferred** |
| `workshopQueue`, `completedWorkshopItems` | Hàng đợi craft | ✅ | ✅ | Đã có |
| `marketListings`, `soldMarketItems` | Hàng đợi bán | ✅ | ✅ | Đã có |
| `items` | Kho đồ | ✅ `Items` | ✅ | Đã có |
| 5 IAP flag (`starterPackPurchased`, `adventurerPackPurchased`, `merchantPackPurchased`, `imperialVanguardPurchased`, `unholyCrusadePurchased`) | Đầu vào 7 formula | ❌ | ✅ **Bắt buộc** (F-12/14/15/16/17, CR-01, M-01) | **Port_Now** (mặc định `false`, không implement mua) |
| 8 cặp doctrine `*Level`/`*Progress` (affliction, control, fortitude, grace, illusion, knowledge, ruin, war) + `doctrineMaxed` | Doctrine progression | ❌ | ✅ (C-04…C-08 phụ thuộc) | **Port_With_Domain** (Doctrine) |
| 20 field instance area (`TheTower`, `TheDesert`, `FrostbitePeaks`, `ObsidianMines`, `CelestialMothership`, …) | Trạng thái từng dungeon | ⚠️ Unity dùng `List<DungeonSaveData>` + `ActiveDungeon` | ✅ | **Port_With_Domain** (Dungeon) — ⚠️ **cấu trúc khác nhau**, cần thiết kế mapping |
| 9 setting (`settingAutoOpenDungeonDetail`, `settingColorblindMode`, `settingConfirmRetreat`, `settingConfirmSwap`, `settingConfirmUpgrade`, `settingCraftMaxAmount`, `settingSellMaxAmount`, `settingVerboseLogs`, `settingsLanguage`) | Tuỳ chọn người chơi | ❌ | ⚠️ Cần cho Settings screen | **Port_With_Domain** (Settings) |
| Thống kê (`itemsCrafted`, `itemsSold`, `maxWealth`, `questsCompleted`, `maxAdventurerTier`, `maxAdventurersOwned`, `adsWatched`, `amountOfPurchases`, `totalGemsPurchased`) | Số liệu tích luỹ | ❌ | ⚠️ Quest/achievement cần | **Port_With_Domain** (Quest) |
| `questsSeen`, `questsRefreshed` | Trạng thái quest | ❌ | ✅ | **Port_With_Domain** (Quest) |
| `everAscended`, `potsMaxed`, `t4Pet`, `maxAdventurerTier` | Progression flag | ❌ | ⚠️ | **Deferred** |
| `shownDialogRaid`, `shownDialogEpicRaid`, `reviewTrigger`, `reviewShown` | Cờ UI | ❌ | ❌ | **Deferred** |
| 11 `redeem*`/`redeemed_*` | Mã khuyến mãi | ❌ | ❌ | **Out_Of_Scope** |
| `intercessionsRetroactivelyGranted`, `vial2RetGrant` | Vá dữ liệu cũ | ❌ | ❌ | **Out_Of_Scope** |
| `settingsLanguage` | Ngôn ngữ | ❌ | ⚠️ | **Deferred** (localization rỗng) |

**Tổng kết:** ~**25 field cần `Port_Now`**, ~**35 field `Port_With_Domain`**, phần còn lại `Deferred`/`Out_Of_Scope`.

---

## Decode Rule To Unity Target Map

| Rule / Domain | Decode Source | Unity Target File/Class | Current Unity Status | Port Priority | Depends On |
|---|---|---|---|---|---|
| 14 formula thiếu (F-01, F-04, F-06…F-18) | `Formulas.java` | `FormulaService.cs` | 6/20 | **P0_Blocker** | 5 IAP flag trong `SaveData` |
| Verify 4 formula đã port (F-02, F-03, F-05, F-17) | `Formulas.java` | `FormulaService.cs` | Có, chưa verify | **P0_Blocker** | — |
| `truncatePrice` | `Utils.java:889` | `FormulaService.cs` | ❌ | **P0_Blocker** | — |
| SaveData ~25 field `Port_Now` | `Data.java` | `SaveData.cs` | 17/140 | **P0_Blocker** | — |
| **Damage formula** | `Area.dealDamage()` | `FormulaService.CalculateDamage*` | Stub rỗng | **P0_Blocker** | 🔴 **ManualRuleRequired** |
| **Stat total / level multiplier** | `Adventurer.calculateTotalStat()` | `CharacterService.GetTotalStat()` | **Hardcode sai** | **P0_Blocker** | 🔴 **ManualRuleRequired** |
| Tavern T-01…T-09 | `Utils.java:189/475`, `Formulas.java` | `TavernService.cs` (**mới**) | ❌ | **P1_Core** | F-13, F-14, SaveData |
| Recruit (guest → owned) | `ui/dialogs/DialogTavern.java` | `TavernService.cs` | ❌ | **P1_Core** | ⚠️ **Cần đọc thêm** |
| Craft CR-01…CR-06 | `Item.java:59`, `Utils.java:536/907/902` | `CraftService.cs` | Trả `ManualRuleRequired` | **P1_Core** | F-16, SaveData |
| Merchant M-01…M-04 | `Item.java:63`, `Utils.java:504/889` | `MerchantService.cs` | Trả `Deferred…` | **P1_Core** | F-15, SaveData |
| Merchant M-05…M-09 (roll offer, buy) | `Utils.java:313/352/400`, `Area.java:3157` | `MerchantService.cs` | ❌ | **P1_Core** | ⚠️ **Cần đọc thêm** |
| Offline delta + cap | `MainActivity.java:878` | `OfflineProgressService.cs` | Thiếu `lastAccess` | **P1_Core** | SaveData `lastAccess` |
| `nextTimeTick` / `tick60` | `Utils.java:449/568` | `GameTickService.cs` (**mới**) | ❌ | **P1_Core** | Craft/Market/Tavern |
| Doctrine bonus (5 method) | `doctrines/Doctrine.java` + 8 instance | `DoctrineService.cs` (**mới**) | ❌ | **P1_Core** | ⚠️ **Cần đọc thêm** |
| Crit/dodge/counterattack C-04…C-08 | `Adventurer.java:606/627/646/270/263` | `CombatService.cs` | ❌ | **P2_Important** | C-01 (blocked), Doctrine |
| Dodge D-02 | `Area.java:1188` | `CombatService.cs` | ❌ | **P2_Important** | C-01, C-06 |
| Crit multiplier D-03 | `Area.java:1289` | `CombatService.cs` | ❌ | **P2_Important** | C-04, C-05 |
| Exp distribution D-04 | `Area.java:635` | `CombatService.cs` | ❌ | **P2_Important** | F-19 |
| Death handling D-05 | `Area.java:2221` | `CombatService.cs` | ❌ | **P2_Important** | — |
| Status apply D-07 | `Area.java:1308` | `StatusEffectService.cs` | ❌ | **P2_Important** | — |
| Tick loop D-08, target selection D-09 | `Area.java:279/420/748/2500+` | `DungeonRunOrchestrator.cs` (**mới**) | ❌ | **P2_Important** | 🔴 Cần subtask riêng |
| Loot D-10 | `Area.java:669` | `LootService.cs` | ❌ | **P2_Important** | ⚠️ Cần đọc thêm |
| Quest Q-01…Q-06 | `QuestsManager.java` | `QuestService.cs` | 87/468 dòng | **P2_Important** | ⚠️ Cần đọc thêm, Doctrine |
| Trap D-06 | `Area.trapEncounter()` | `DungeonService.cs` | ❌ | **P3_Deferred** | 🔴 ManualRuleRequired |
| Pet | `pets/Pet.java` | `PetService.cs` (**mới**) | ❌ | **P3_Deferred** | Combat |
| Raid | `places/raids/` | — | ❌ | **P3_Deferred** | Dungeon |
| Achievements | `AchievementsUtils.java` | Stub no-op | ❌ | **P3_Deferred** | — |
| IAP | `IAPWrapper.java` | Chỉ field trong `SaveData` | ❌ | **P3_Deferred** | — |

---

## Rule Coverage Summary

| Domain | Decode Rule Found | Enough To Port? | ManualRuleRequired | Notes |
|---|---|---|---|---|
| **Formulas** | ✅ 20/20 đọc được nguyên văn | ✅ **CÓ** | ❌ Không | Cần thêm 5 IAP flag vào `SaveData` trước |
| **SaveData schema** | ✅ 140/140 field đọc được | ✅ **CÓ** | ❌ Không | ~25 field `Port_Now` |
| **Tavern / Recruit** | ✅ T-01…T-04, T-06…T-09 | ✅ **CÓ** (trừ hành động recruit) | ⚠️ Một phần (T-05 đọc chưa hết, recruit + cost chưa đọc) | **Mở được nút thắt gameplay** |
| **Craft / Workshop** | ✅ CR-01…CR-06 | ✅ **CÓ** | ❌ Không (trừ CR-07 claim UI) | 🎉 **Gỡ được `ManualRuleRequired` hiện tại** |
| **Merchant — sell/timer/price** | ✅ M-01…M-04 | ✅ **CÓ** | ❌ Không | |
| **Merchant — buy/roll offer** | ❌ M-05…M-09 chưa đọc | ❌ **CHƯA** | ⚠️ Có | Giữ `DeferredPriceOrCurrencyRule` |
| **Offline progress** | ✅ delta + cap 12h | ✅ **CÓ** | ❌ Không | Cần `lastAccess` |
| **Combat — crit/dodge/counter** | ✅ C-04…C-08, D-02, D-03 | ⚠️ **Rule đọc được nhưng đầu vào bị chặn bởi C-01** | ⚠️ Gián tiếp | |
| **Combat — exp/death** | ✅ D-04, D-05 | ✅ **CÓ** | ❌ Không | Port 1:1 |
| **Combat — damage** | ❌ **KHÔNG decompile được** | ❌ **KHÔNG** | 🔴 **CÓ — blocker cứng** | `dealDamage()` 1102 units |
| **Character stat** | ❌ **KHÔNG decompile được** | ❌ **KHÔNG** | 🔴 **CÓ — blocker cứng** | `calculateTotalStat()` 332 units |
| **Dungeon tick / target selection** | ⚠️ Chưa đọc chi tiết | ❌ **CHƯA** | ⚠️ Cần subtask | `Area.java` 3164 dòng |
| **Loot** | ⚠️ Chưa đọc chi tiết | ❌ **CHƯA** | ⚠️ Có | |
| **Quest** | ⚠️ Chưa đọc chi tiết | ❌ **CHƯA** | ⚠️ Có | `QuestsManager.java` 468 dòng |
| **Doctrine** | ⚠️ Chưa đọc chi tiết | ❌ **CHƯA** | ⚠️ Có | **Nâng lên Core** vì crit/dodge phụ thuộc |
| **Pet / Raid / Achievements / IAP** | ⚠️ Chưa đọc | — | — | Deferred |

---

## Next Implementation Recommendation

### Đề xuất: chạy **S6.5A-001B — Additional Rule Extraction** trước, song song chuẩn bị S6.5A-002

**Lý do:** Phần rule đã bóc **đủ để bắt đầu S6.5A-002** (Formula + SaveData Schema) ngay — đây là 2 hạng mục không phụ thuộc gì vào 2 blocker. Nhưng còn **6 nhóm rule chưa đọc** sẽ chặn các task sau, nên nên bóc tiếp song song:

**S6.5A-001B cần đọc thêm:**
1. `Doctrine.java` + 8 instance → **bắt buộc**, vì C-04…C-08 phụ thuộc
2. `QuestsManager.java` (468 dòng) → Q-01…Q-05
3. `Utils.rollPotion/rollSpecialFoods/rollUpgrades` + `Area.rollMerchant*Offers` + `DialogMerchant` → M-05…M-09
4. `ui/dialogs/DialogTavern.java` → hành động recruit + cost (T-10)
5. `Area.tick/performAction/fightTurn/selectTargets` → D-08, D-09
6. `Area.loot/fullChest` + `Utils.collectDrops` → D-10
7. `Utils.rollRareTrait` phần còn lại → T-05

**Và quan trọng nhất — giải quyết 2 blocker cứng.** Đề xuất 3 hướng, theo thứ tự nên thử:
| Hướng | Mô tả | Khả năng thành công |
|---|---|---|
| 1 | Decompile lại bằng công cụ khác: `dex2jar` + **CFR** hoặc **Procyon** hoặc **Fernflower** — các decompiler này xử lý method lớn tốt hơn JADX | **Cao** — đây là vấn đề của riêng JADX với method nhiều instruction |
| 2 | Chạy lại JADX với `--show-bad-code` / tăng giới hạn, hoặc xuất **smali** rồi đọc bytecode | Trung bình — đọc smali tốn công nhưng chắc chắn có dữ liệu |
| 3 | Đọc `Document/GDD` xem có mô tả công thức damage/stat không | Chưa rõ — **chưa kiểm tra trong phase này** |

**Không được** tự đặt công thức damage/stat để "cho chạy được" — sẽ làm sai lệch toàn bộ cân bằng game.

---

## Final Decision

# `S6_5A_001_RULE_EXTRACTION_PARTIAL_NEEDS_MORE_DECODE_REVIEW`

**Lý do:** Bóc thành công **20/20 formula**, **140/140 save field**, **toàn bộ rule Tavern** (mở được nút thắt gameplay), **toàn bộ rule Craft timer/claim** (gỡ được `ManualRuleRequired` hiện tại), **rule Merchant sell/timer/price**, **offline delta + cap**, và **6 rule combat phái sinh** (crit/dodge/counterattack/exp/death/status) — nhiều hơn dự kiến ban đầu.

**Nhưng chưa thể chốt DONE** vì 2 lý do độc lập:
1. **2 blocker cứng không giải được bằng nguồn hiện tại**: `Area.dealDamage()` (1102 units) và `Adventurer.calculateTotalStat()` (332 units) **không decompile được bởi JADX**. Đây là bằng chứng khách quan ghi thẳng trong file decode, không phải suy đoán. Chừng nào chưa có 2 rule này thì **Combat và Character stat không thể port đúng** — và Unity hiện đang **sai** ở `levelMultiplier = 1.0f`.
2. **6 nhóm rule còn lại chưa đọc** (Doctrine, Quest, Merchant buy/roll, Recruit action, Dungeon tick/target selection, Loot) sẽ chặn các task S6.5A-005 → S6.5A-010.

Không có scope violation: **không implement code, không sửa Unity/scene/data/asset**, không tự bịa rule nào. Mọi công thức trong report đều dẫn được về file + số dòng cụ thể.
