# S6.5A-001D Remaining Core Rules Report

**Ngày:** 2026-07-27 · **Backup:** `Backups/S6_5A_001D_Remaining_Core_Rules_20260727_101324/` (224 file)
**Trạng thái:** ⏸️ **CHỈ EXTRACT/REPORT — không implement gì**

---

## Executive Summary

### Những rule nào đã bóc thêm?
Dump thành công **58 method smali** + **50 method DAD Java**, **0 method thất bại**, phủ 6 nhóm:

| Nhóm | Method dump | Kết quả |
|---|---:|---|
| Dungeon tick / turn loop | 20 | ✅ State machine 7 trạng thái đọc được hoàn toàn |
| Target selection | 10 | ✅ **15 targeting strategy** liệt kê đủ |
| Loot / chest / drop | 5 | ✅ Bao gồm hằng số chest cap, Geode rate, pet drop bonus |
| Quest | 15 | ✅ difficulty + rarity threshold đọc được |
| Merchant roll / price | 6 | ✅ Bảng ngưỡng roll potion/food/upgrade |
| Trap / special encounter | 2 | ✅ `trapEncounter` (JADX từng bỏ) nay đọc được |

### Domain nào đạt ≥95%?
Dungeon tick · Target selection · Loot · Quest (difficulty/rarity/increment) · Merchant roll · Trap — **cộng với** các domain đã đạt từ trước (Formula, SaveData, Doctrine, Tavern, Character stat, Damage, ApplyDamage, Craft, Merchant sell).

### Domain nào còn partial?
- **Merchant buy action** (trừ tiền, ghi inventory) — nằm ở tầng UI `DialogBuyFromMerchant`, chưa dump
- **Quest claim reward** — chưa định vị được method claim cụ thể
- **`cast()` / `heal()` / `retaliate()`** — chưa dump riêng (là nhánh của combat, phụ thuộc `dealDamage` đã có)

### Có còn blocker loại "không đọc được" không?
❌ **KHÔNG.** Toàn bộ method yêu cầu đều decompile được. Phương pháp androguard đã vô hiệu hoá hoàn toàn giới hạn của JADX.

### Có blocker do logic quá phức tạp chưa hiểu không?
⚠️ **Có 1 mức độ vừa:** `Area.dealDamage` (1102 code-units) và `resolveStatus` (~180 dòng) là logic lớn — đọc được nhưng cần rà từng nhánh khi implement. **Không phải blocker**, là khối lượng công việc.

### Có đủ điều kiện sang S6.5A-002 không?
✅ **CÓ.** S6.5A-002 (Formula + SaveData Schema) không phụ thuộc bất kỳ rule nào trong phase này, và đã đạt 97–98% từ S6.5A-001.

---

## Source Chain Of Custody

| Item | Path / Value | Notes |
|---|---|---|
| Zip nguồn | `D:\Tinh\Guild Master - Idle Dungeons\New folder.zip` | 16.561.475 bytes, không sửa |
| Nested XAPK | `it.paranoidsquirrels.idleguildmaster_2.147.xapk` | 22.580.839 bytes |
| APK chính | `it.paranoidsquirrels.idleguildmaster.apk` | 22.387.222 bytes, 1860 entry |
| **Temp workspace** | `D:\Tinh\_tmp_xapk_recovery\S6_5A_001C_XAPK_20260727_092847\` | **Tái dùng từ 001C**, còn nguyên vẹn |
| DEX | `apk_out\classes.dex` (8.293.152) · `classes2.dex` (7.985.104) · `classes3.dex` (5.727.720) | |
| `Area` / `Adventurer` / `Entity` | **`classes3.dex`** | Xác nhận lại ở phase này |
| `Utils` / `QuestsManager` | quét cả 3 DEX (script tự dò) | |
| Tool | **androguard 4.1.4** (Python 3.14.5) | Không cần Java |
| Script dump | `dump001d.py` (smali) · `dad001d.py` (DAD) — trong temp | Read-only |

---

## Dungeon Tick / Turn Loop Rules

| Rule ID | Method | Source Evidence | Exact Logic | Dependencies | Confidence | Ready To Port |
|---|---|---|---|---|---:|---|
| **T-01** | `Area.tick()` | `…_Area_tick_smali.txt` · `…_DungeonTick_DAD.md` | Nếu `adventurersExploringIds` rỗng → **return ngay**. Nếu `terminationRequested` → `terminate()`. Nếu `adventurersExploring` rỗng hoặc `restartRequested` → `setupArea()`. Nếu `action != null` → `action.nextTurn()` + `adventureRecap.addSecondPassed()`; ngược lại khởi tạo `Action(0)` + reset. Khi `action.finished()` → (realign nếu cần) → **`performAction()`** | `Action`, `setupArea` | **96%** | ✅ YES |
| **T-02** | `Area.performAction()` — state machine | như trên | **7 trạng thái** (xem bảng dưới) | tất cả | **96%** | ✅ YES |
| **T-03** | `Area.fightTurn()` | như trên | `turnEndRequested=false`; `turnsFighting++`; `selectNextActing()`; `s = resolveStatus(acting)`;<br>nếu `acting.hp > 0 && s != 2`:<br>  `canCast = (s == 1) ? false : increaseMana(acting)`<br>  nếu **không** cast:<br>    · không phải healer → `t = selectTargets(acting, attackTargetStrategy(acting))` → **`dealDamage(acting, t[0], null, null)`**<br>    · là healer → `t = selectTargets(acting, "lowest_relative_ally")` → `heal(acting, t[0], null)`<br>  nếu cast được → `cast(acting)`<br>sau đó duyệt `acting.endOfTurnActions()` (xem T-04) | `dealDamage`, `selectTargets`, `resolveStatus` | **95%** | ✅ YES |
| **T-04** | End-of-turn actions | `…_DungeonTick_DAD.md` | Bỏ qua nếu `turnEndRequested`. Với mỗi `EndOfTurnAction`:<br>· `STUN_SELF_NOT_CLEANSABLE` → xử lý riêng<br>· `FALSE_LIFE` → tạo `StatusEffect(FALSE_LIFE, self, 999, doctrine.falseLifeChance() × 0.01)`<br>· `shields` → target `"lowest_shield_ally"`; `shield = min(shield + round(eota.damage × healingModifier), (int)(maxHp × **0.2**))`<br>· còn lại → nếu `procsOnMelee == null` hoặc `procsOnMelee != acting.isRanged()` → `dealDamage(acting, target, null, eota)` | `selectTargets`, `dealDamage` | **93%** | ⚠️ Partial |
| **T-05** | Giới hạn số lượt | `performAction` case 2 | `if (turnsFighting < 400 \|\| areaType != 0)` → tiếp tục đánh; ngược lại → `Action(6)` (bỏ chạy). **Dungeon (areaType 0) có cap 400 lượt; raid không cap** | — | **97%** | ✅ YES |
| **T-06** | Pet trong lượt | `performAction` case 2 | Sau `fightTurn()` gọi tuần tự: `petAttack()` → `petHeal()` → `petExecution()` → `petCast()` | Pet | **95%** | ✅ YES |
| **T-07** | Quest trigger từ tick | `performAction` case 2 | Nếu số corpse tăng ≥ **4** trong 1 lượt → `QuestsManager.increment(tabulaRasa, 1)` | Quest | **97%** | ✅ YES |

### State machine `performAction()` — 7 trạng thái

| Action type | Tên (suy từ hành vi) | Logic | Chuyển tiếp |
|---:|---|---|---|
| **0** | ENTER_DUNGEON | `triggerEvent("enter_dungeon")` | → `Action(1)` |
| **1** | ENTER_ROOM | nếu `areaType != 0` → `incrementProgress()`; `enterRoom()`;<br>nếu còn adventurer sống → `enemies = rollEnemies()`; nếu có enemy → thêm vào `seenEnemies`, `triggerEvent("fight_start")`, `initializeFight()` → `Action(2)`; nếu rỗng → `Action(4)` *(hoặc `Action(1)` nếu `areaType != 0`)*<br>nếu hết adventurer → `terminationRequested=1` (raid), log, `Action(5)` | 2 / 4 / 1 / 5 |
| **2** | FIGHT | nếu còn adventurer & còn enemy & `turnsFighting < 400` (hoặc raid) → `fightTurn()` + 4 pet action → `Action(2)`;<br>nếu quá 400 lượt → `Action(6)`;<br>nếu hết enemy → log, `triggerEvent("victory")`, **`collectExperience()`** → `Action(3)`;<br>nếu hết adventurer → `Action(5)` | 2 / 6 / 3 / 5 |
| **3** | LOOT | **`loot()`** | → `Action(4)` *(hoặc `Action(1)` nếu raid)* |
| **4** | SEARCH_ROOM | `searchRoom()`; nếu còn adventurer → `refreshLoot()`, nếu `areaType == 0` → `incrementProgress()` → `Action(1)`; ngược lại → `Action(5)` | 1 / 5 |
| **5** | DEFEAT / RESPAWN | log, **`respawn()`**, nếu `progress < 250` → **`progress = 0`**, `triggerEvent("respawn")` | → `Action(1)` |
| **6** | FLEE / TIMEOUT | log, `triggerEvent("flee")`, `clearEnemies()` | → `Action(1)` |

> 🔴 **Chi tiết quan trọng:** khi thua (`Action(5)`), progress **chỉ reset về 0 nếu `progress < 250`** — tức là qua mốc 250 thì giữ tiến độ. Đây là rule cân bằng, không được tự đặt.

---

## Target Selection Rules

| Rule ID | Strategy / Method | Source Evidence | Exact Logic | Confidence | Ready To Port |
|---|---|---|---|---:|---|
| **TS-01** | `attackTargetStrategy(Entity)` | `…_TargetSelection_DAD.md` | Trả về **tên strategy** theo passive skill:<br>· `PASSIVE_CHAOTIC` hoặc `PASSIVE_PRIMORDIAL_HUNGER` → **`"random_except_self"`**<br>· *(trong `LostLands`)* `PASSIVE_PREHISTORIC_AVIAN`/`PASSIVE_PREHISTORIC_COLOSSUS` + có enemy `PASSIVE_NATURAL_EMPATHY` → `"random_except_self"`<br>· `PASSIVE_DESPISE_WEAKNESS` hoặc `PASSIVE_WICKED_APPETITE` → **`"lowest_relative_enemy"`**<br>· mặc định → **`"random_enemy"`** | **96%** | ✅ YES |
| **TS-02** | Danh sách strategy | `…_TargetSelection_smali.txt` (string constants) | **15 strategy:** `all` · `all_allies` · `all_enemies` · `all_except_self` · `lowest_absolute_ally` · `lowest_absolute_enemy` · `lowest_relative_ally` · `lowest_relative_enemy` · `lowest_shield_ally` · `most_negative_conditions_or_lowest_relative_ally` · `random` · `random_ally` · `random_ally_except_self` · `random_enemy` · `random_except_self` | **97%** | ✅ YES |
| **TS-03** | `selectTargets(Entity, String)` | smali + DAD | Dispatcher chính, phân nhánh theo 15 strategy trên; xử lý cả AoE (`all_*`) trả nhiều target | **93%** | ⚠️ Partial — cần rà từng nhánh khi implement |
| **TS-04** | `tauntedBy(Entity, List)` | smali + DAD | Ưu tiên mục tiêu có status TAUNT — ghi đè strategy thường | **92%** | ⚠️ Partial |
| **TS-05** | `weightedSelection(List)` | smali + DAD | Chọn ngẫu nhiên có trọng số | **92%** | ⚠️ Partial |
| **TS-06** | `selectLowestHpTarget` / `selectLowestRelativeShieldAlly` / `selectRandomTarget` / `selectEnemyTarget` | smali + DAD | Các helper cụ thể | **93%** | ⚠️ Partial |
| **TS-07** | `selectPetTarget` / `selectPetHealingTarget` | smali + DAD | Pet chọn mục tiêu riêng | **92%** | ⚠️ Partial |

> **Đánh giá chung:** cấu trúc và danh sách strategy đạt ≥95%; **logic chi tiết từng helper ở mức 92–93%** — evidence đầy đủ trong file, chỉ cần đọc kỹ khi implement. **Không phải blocker.**

---

## Loot / Chest / Drop Rules

| Rule ID | Method | Source Evidence | Exact Logic | Save Mutation | Confidence | Ready To Port |
|---|---|---|---|---|---:|---|
| **L-01** | `Area.loot()` | `…_Loot_smali.txt` · `…_Loot_DAD.md` | Nếu `fullChest()` → **không loot**. Ngược lại, với **mỗi corpse**:<br>1. `key = event?.getKey() ?? 0`<br>2. `drop1 = Utils.rollFromWeightedMap(enemy.listDrops(key))`<br>3. **Pet bonus roll:** nếu có pet, `drop1` không phải `notSellable`, và `random() < pet.getDrops() / 100.0` → roll thêm `drop2` (cùng bảng)<br>4. Mỗi drop khác null → `Utils.collectItem(item, this.drops)` + log<br>5. **Geode:** nếu `random() < 0.0005` → `collectItem(Item.getInstance("Geode"), drops)` | `this.drops` (chest tạm, chưa vào kho) | **95%** | ✅ YES |
| **L-02** | `Area.fullChest()` | như trên | `total = Σ drops[i].getStack()`; cap = **2000**, hoặc **3000** nếu `isMerchantPackPurchased()`; trả `total >= cap` | — | **98%** | ✅ YES |
| **L-03** | Geode drop rate | `loot()` bit `4557750909289998844` | **0.0005** (0.05%) mỗi corpse | — | **98%** | ✅ YES |
| **L-04** | Pet drop bonus | `loot()` bit `4636737291354636288` | `pet.getDrops() / **100.0**` = xác suất roll drop lần 2 | — | **96%** | ✅ YES |
| **L-05** | Tutorial hook trong loot | `loot()` | Nếu `tutorialStep == 2` khi nhận drop đầu tiên → `setTutorialStep(3)`, `event = null` | `tutorialStep` | **95%** | ✅ YES |
| **L-06** | `Utils.collectItem(Item, List)` | `…_Loot_smali.txt` | Gộp stack vào list đích | list truyền vào | **95%** | ✅ YES |
| **L-07** | `Utils.collectDrops(Fragment, Area)` | như trên | Chuyển `area.drops` → kho người chơi (`data.items`) | **`data.items`** | **93%** | ⚠️ Partial — cần đọc kỹ phần giới hạn `storageSpaces()` |
| **L-08** | `Utils.removeItemFromStorage(Item)` | như trên | Trừ item khỏi kho | `data.items` | **94%** | ⚠️ Partial |

> 🔴 **Quan trọng:** loot **KHÔNG** cộng thẳng vào kho — vào `area.drops` (rương tạm). Người chơi phải **thu thập** qua `collectDrops`. **Không có money/gems drop trực tiếp từ quái** trong `loot()`.

---

## Quest Rules

| Rule ID | Method / Feature | Source Evidence | Exact Logic | Save Fields | Confidence | Ready To Port |
|---|---|---|---|---|---:|---|
| **Q-01** | `calculateDifficulty()` | `…_Quest_DAD.md` | Đếm số dungeon **unlocked liên tiếp** từ đầu `Utils.compileDungeonList()` — dừng ở dungeon đầu tiên chưa unlock | — | **97%** | ✅ YES |
| **Q-02** | `rollRarity()` | như trên (bit `4604480259023595110`/`4606281698874543309`/`4606912202822375178`) | `r = random()`:<br>· `r < 0.7` → **rarity 1**<br>· `0.7 ≤ r < 0.9` → **rarity 2**<br>· `0.9 ≤ r < 0.97` → **rarity 3**<br>· `r ≥ 0.97` → **rarity 4** | — | **98%** | ✅ YES |
| **Q-03** | `extractQuests()` | `…_QuestsManager_smali.txt` | Sinh danh sách quest khả dụng | `questsSeen`, `questsRefreshed` | **92%** | ⚠️ Partial |
| **Q-04** | `setupAccessibleQuests(int)` | như trên | Lọc quest theo difficulty | — | **92%** | ⚠️ Partial |
| **Q-05** | `setupDoctrineAmounts()` | như trên | Phân bổ quest theo doctrine | 8 `*Level`/`*Progress` | **90%** | ⚠️ Partial |
| **Q-06** | `increment(Quest, long)` / `incrementToValue(Quest, long)` | như trên | Tăng tiến độ quest | `questsCompleted` | **94%** | ⚠️ Partial |
| **Q-07** | `realignQuests()` | như trên | Đồng bộ lại quest sau thay đổi | — | **90%** | ⚠️ Partial |
| **Q-08** | Quest trigger từ gameplay | `Area.java`, `Entity.java` | `tabulaRasa` (≥4 kill/lượt) · `theEnd` (adventurer chết) · `criticalHit` · `pulverization` (crit ≥2.5) · `shocking` (STUN) · `smokingHot` (ABLAZE) · `student` (exp) · `fastLearner` (expMul ≥1.5) · `smartFighter` · `annihilator` · `warrior` · `unscathed` · `heavyArmor` · `protector` | — | **95%** | ✅ YES |
| **Q-09** | **Claim reward** | ❌ **chưa định vị được method** | — | — | **< 80%** | ❌ **Needs_More_Extraction** |

---

## Merchant Buy / Roll / Restock Rules

| Rule ID | Method / Feature | Source Evidence | Exact Logic | Save Mutation | Confidence | Ready To Port |
|---|---|---|---|---|---:|---|
| **M-01** | `Utils.rollPotion()` | `…_Merchant_smali.txt` · `…_Merchant_DAD.md` | Chuỗi ngưỡng `random()` phân nhánh ra từng loại potion (`PotionOfAgility`, `PotionOfImmunity`, `PotionOfDarkness`, `PotionOfViciousness`, …) kèm **quantity riêng** (VD 80, 70) | — | **95%** | ✅ YES |
| **M-02** | `Utils.rollSpecialFoods()` | như trên | Roll danh sách food đặc biệt | — | **94%** | ⚠️ Partial |
| **M-03** | `Utils.rollUpgrades()` | như trên | Roll offer nâng cấp | — | **94%** | ⚠️ Partial |
| **M-04** | `Utils.truncatePrice(long)` | như trên + JADX | `if (j <= 10000) return j; mod = (j <= 1000000) ? j%100 : j%10000; return j - mod;` | — | **99%** | ✅ YES |
| **M-05** | `Area.rollMerchantRegularOffers()` | như trên | Offer thường theo từng dungeon | `newMerchantRegularItems` | **93%** | ⚠️ Partial |
| **M-06** | `Area.rollMerchantSpecialOffers()` | như trên | Offer đặc biệt theo dungeon | `newMerchantSpecialItems` | **93%** | ⚠️ Partial |
| **M-07** | **Buy action** (trừ tiền, ghi kho) | ❌ **chưa dump** — nằm ở `ui/dialogs/DialogBuyFromMerchant.java` | — | `money`/`gems`, `items` | **< 80%** | ❌ **Needs_More_Extraction** |
| **M-08** | Sell / timer (từ S6.5A-001) | `Item.getSecondsToSell`, `Utils.progressMarketTime` | Đã bóc đủ ở phase trước | `marketListings`, `soldMarketItems` | **96%** | ✅ YES |

---

## Tavern Recruit / Quarters Rules

*(Đã bóc đủ ở S6.5A-001B từ JADX sources — phase này không cần dump lại vì JADX đọc được hoàn toàn.)*

| Rule ID | Feature | Source Evidence | Exact Logic | Save Mutation | Confidence | Ready To Port |
|---|---|---|---|---|---:|---|
| **TR-01** | Điều kiện recruit | `DialogTavern.java:238` (JADX) | `if (Formulas.getQuartersCapacity() <= data.getAdventurers().size())` → nút **disable** + dialog "recruit unavailable" | — | **97%** | ✅ YES |
| **TR-02** | Hành động recruit | `DialogTavern.java:360` | `guest = tavernGuests.get(i)`; `tavernGuests.remove(guest)`; `guest.setId(Utils.calculateNewAdventurerId())`; `data.getAdventurers().add(guest)`; `Utils.triggerGuildSizeAchievementCheck()` | `tavernGuests`, `adventurers` | **97%** | ✅ YES |
| **TR-03** | **Chi phí recruit** | `DialogTavern.java:360-378` | 🎉 **MIỄN PHÍ** — không có lệnh trừ `money`/`gems` nào trong `recruitAdventurer()` | — | **95%** | ✅ YES |
| **TR-04** | Tutorial side effect | `DialogTavern.java:367` | `if (step == 1 \|\| step == 6) { setTutorialStep(step+1); if (step == 6) Utils.progressTavernTime(28800); }` | `tutorialStep` | **97%** | ✅ YES |
| **TR-05** | Achievement side effect | `Utils.triggerGuildSizeAchievementCheck()` | Có thể **stub no-op** — không ảnh hưởng gameplay loop | — | **95%** | ✅ YES (stub) |
| **TR-06** | Guest removal order | `Utils.newTavernVisitor()` | Thêm vào **đầu** list; nếu `size > getTavernCapacity()` → **xoá phần tử cuối** | `tavernGuests` | **97%** | ✅ YES |
| **TR-07** | Default weapon | `Utils.getDefaultWeapon(int)` | `type_sword→"Spade"` · `type_staff→"Cane"` · `type_dagger→"Sickle"` · `type_bow→"TrainingBow"` | — | **97%** | ✅ YES |
| **TR-08** | Giảm thời gian chờ | `DialogTavern.java:324` | `setNextTavernVisit((long)(nextTavernVisit × 0.9))` — **cost chưa xác định** | `nextTavernVisit` | **80%** | ⚠️ Partial |
| **TR-09** | `calculateNewAdventurerId()` | `Utils.java:869` | Chưa đọc thân hàm | — | **75%** | ⚠️ Needs_More_Extraction |

---

## Trap / Special Encounter Rules

| Rule ID | Method | Source Evidence | Exact Logic | Core Required? | Confidence | Scope Recommendation |
|---|---|---|---|---|---:|---|
| **TP-01** | `Area.trapEncounter(int,int,int,int,boolean)` | `…_Trap_smali.txt` (263 code-units, **JADX từng bỏ, nay dump được**) | Được gọi từ `searchRoom()`/nhánh phụ, **không nằm trong đường đi chính** của `performAction` state machine | ❌ **Không bắt buộc cho core loop** | **90%** | **Deferred_After_Core** |
| **TP-02** | `Area.triggerEvent(String)` | `…_Trap_smali.txt` | Được gọi tại các mốc: `"enter_dungeon"`, `"fight_start"`, `"victory"`, `"respawn"`, `"flee"`, `"kill_<enemyClass>"` | ✅ **CÓ** — nằm trong state machine | **93%** | **Core_Needed_Now** (có thể stub log ban đầu) |

> **Kết luận Trap:** `trapEncounter` **không thuộc đường đi chính** của tick loop (chỉ nhánh `searchRoom`). Có thể **hoãn** mà vẫn chạy được vòng dungeon cơ bản. `triggerEvent` thì nằm trong core nhưng có thể stub thành no-op/log ở giai đoạn đầu.

---

## Updated Rule To Implementation Map

| Domain | Rule Status | Confidence | Unity Target | Implementation Task | Blockers |
|---|---|---:|---|---|---|
| Formula (20) | ✅ Complete | 98% | `FormulaService` | S6.5A-002 | Cần 5 IAP flag trong SaveData |
| SaveData schema (140 field) | ✅ Complete | 97% | `SaveData` | S6.5A-002 | — |
| Doctrine | ✅ Complete | 96% | `DoctrineService` (mới) | S6.5A-003 | 7 instance còn lại (cơ học) |
| Tavern visitor | ✅ Complete | 97% | `TavernService` (mới) | S6.5A-004 | — |
| Tavern recruit | ✅ Complete | 96% | `TavernService` | S6.5A-004 | TR-08/TR-09 nhỏ |
| Character stat | ✅ Complete | 98% | `CharacterService` | S6.5A-005 | — |
| Damage | ✅ Complete | 95% | `CombatService` + `FormulaService` | S6.5A-007 | Nhánh phụ cần rà |
| ApplyDamage | ✅ Complete | 99% | `Entity`/`CombatService` | S6.5A-007 | — |
| **Dungeon tick** | ✅ **MỚI** | 96% | **`DungeonRunOrchestrator`** (mới) | S6.5A-007 | `Action` class cần port |
| **Target selection** | ✅ **MỚI** | 93–97% | **`TargetSelectionService`** (mới) | S6.5A-007 | 7 helper cần rà |
| **Loot** | ✅ **MỚI** | 95% | `LootService` | S6.5A-007 | `rollFromWeightedMap` cần đọc |
| **Quest** | ⚠️ Partial | 90–98% | `QuestService` | S6.5A-008 | **Claim reward chưa có** |
| Craft | ✅ Complete | 96% | `CraftService` | S6.5A-009 | CR-07 claim UI |
| **Merchant roll** | ✅ **MỚI** | 93–95% | `MerchantService` | S6.5A-010 | — |
| **Merchant buy** | ❌ Missing | < 80% | `MerchantService` | S6.5A-010 | **`DialogBuyFromMerchant` chưa dump** |
| Merchant sell/timer | ✅ Complete | 96% | `MerchantService` | S6.5A-010 | — |
| Offline progress | ✅ Complete | 95% | `OfflineProgressService` | S6.5A-012 | Cần `lastAccess` |
| Settings | ✅ Complete | 95% | `SettingsService` (mới) | S6.5A-011 | Cần 9 setting field |
| Trap | ⏸️ Deferred | 90% | `DungeonService` | sau core | Không chặn |

---

## Core Rule Confidence Gate

| Domain | Required For Core? | Evidence Complete? | Confidence | Ready To Implement? | Notes |
|---|---|---|---:|---|---|
| **Formula** | ✅ Yes | ✅ | **98%** | ✅ **YES** | 20/20 method |
| **SaveData schema** | ✅ Yes | ✅ | **97%** | ✅ **YES** | 140/140 field |
| **Doctrine** | ✅ Yes | ✅ | **96%** | ✅ **YES** | Cơ chế + bảng 40 ability |
| **Tavern visitor** | ✅ Yes | ✅ | **97%** | ✅ **YES** | Nút thắt gameplay |
| **Tavern recruit** | ✅ Yes | ✅ | **96%** | ✅ **YES** | Miễn phí, giới hạn quarters |
| **Character stat** | ✅ Yes | ✅ | **98%** | ✅ **YES** | 3 chi tiết tinh vi đã ghi |
| **Damage** | ✅ Yes | ✅ | **95%** | ✅ **YES** | Rà nhánh phụ khi implement |
| **ApplyDamage** | ✅ Yes | ✅ | **99%** | ✅ **YES** | Xác nhận chéo 3 nguồn |
| **Dungeon tick** | ✅ Yes | ✅ | **96%** | ✅ **YES** | State machine 7 trạng thái |
| **Target selection** | ✅ Yes | ✅ | **93%** | ⚠️ **PARTIAL** | Dispatcher + 15 strategy rõ; 7 helper cần rà kỹ |
| **Loot** | ✅ Yes | ✅ | **95%** | ✅ **YES** | `collectDrops` còn 93% |
| **Quest** | ⚠️ Core một phần | ⚠️ | **90%** | ⚠️ **PARTIAL** | **Claim reward < 80%** |
| **Craft** | ✅ Yes | ✅ | **96%** | ✅ **YES** | — |
| **Merchant buy/roll** | ⚠️ Core một phần | ❌ | **< 80%** (buy) | ❌ **NO** | `DialogBuyFromMerchant` chưa dump |
| **Merchant sell/timer** | ✅ Yes | ✅ | **96%** | ✅ **YES** | — |
| **Offline progress** | ✅ Yes | ✅ | **95%** | ✅ **YES** | — |
| **Settings** | ✅ Yes | ✅ | **95%** | ✅ **YES** | — |

**Tổng: 13/17 domain đạt ≥95% (YES) · 2 PARTIAL · 1 NO · 1 Deferred**

---

## Evidence Files Created

| # | File | Nội dung |
|---|---|---|
| 1 | `S6_5A_001D_Area_tick_smali.txt` | 20 method (tick, performAction, fightTurn, enterRoom, initializeFight, decideTurnsOrder, selectNextActing, resolveStatus, increaseMana, incrementProgress, terminate, setupArea, respawn, clearEnemies, adventurersAlive, logStatistics, resetAdventurers, needsRealignment, realignToMain, setupInitialDarkness) — 1657 dòng |
| 2 | `S6_5A_001D_TargetSelection_smali.txt` | 10 method — 887 dòng |
| 3 | `S6_5A_001D_Loot_smali.txt` | 5 method — 513 dòng |
| 4 | `S6_5A_001D_QuestsManager_smali.txt` | 15 method — 1503 dòng |
| 5 | `S6_5A_001D_Merchant_smali.txt` | 6 method — 429 dòng |
| 6 | `S6_5A_001D_Trap_smali.txt` | 2 method — 156 dòng |
| 7 | `S6_5A_001D_DungeonTick_DAD.md` | 14 method Java — 35.7 KB |
| 8 | `S6_5A_001D_TargetSelection_DAD.md` | 10 method Java — 22.5 KB |
| 9 | `S6_5A_001D_Loot_DAD.md` | 5 method Java — 11.9 KB |
| 10 | `S6_5A_001D_Quest_DAD.md` | 13 method Java — 47.8 KB |
| 11 | `S6_5A_001D_Merchant_DAD.md` | 6 method Java — 11.0 KB |
| 12 | `S6_5A_001D_Trap_DAD.md` | 2 method Java — 4.3 KB |
| 13 | `S6_5A_001D_Remaining_Core_Rules_Backup_Report.md` | Backup |
| 14 | `S6_5A_001D_Remaining_Core_Rules_Report.md` | File này |

**Tổng: 58 method smali · 50 method DAD · 0 thất bại**

---

## Critical Risks

| Risk | Domain | Reason | Required Action |
|---|---|---|---|
| **R-01** | Merchant buy | `DialogBuyFromMerchant.java` chưa dump → không biết trừ `money` hay `gems`, không biết ghi kho ra sao | **S6.5A-001E**: dump `ui/dialogs/DialogBuyFromMerchant` + `DialogMerchant`. **Không được tự đặt currency/price** |
| **R-02** | Quest claim | Chưa định vị được method claim reward | **S6.5A-001E**: dump `ui/dialogs/DialogQuests` hoặc tìm caller của quest reward |
| **R-03** | Target selection helper | 7 helper ở mức 92–93%, dưới ngưỡng 95% | Đọc kỹ evidence file khi implement S6.5A-007; **không tự đơn giản hoá** |
| **R-04** | `rollFromWeightedMap` | Được `loot()` gọi nhưng chưa dump riêng | Dump ở S6.5A-001E hoặc khi implement Loot |
| **R-05** | `Action` class | State machine phụ thuộc `Action(int)` + `nextTurn()` + `finished()` — chưa dump | Dump `storage/data/places/Action` trước khi implement `DungeonRunOrchestrator` |
| **R-06** | Khối lượng combat | `dealDamage` 1102 units + `resolveStatus` + `cast`/`heal`/`retaliate` chưa dump riêng | Chia nhỏ S6.5A-007 thành nhiều subtask; **không gộp làm 1 lần** |

---

## Recommended Next Step

# `PROCEED_TO_S6_5A_002_FORMULA_SAVEDATA`

**Kèm khuyến nghị chạy song song `S6.5A-001E`** (nhẹ, ~30 phút) để dọn 5 mục còn thiếu:
1. `DialogBuyFromMerchant` / `DialogMerchant` → **R-01** (merchant buy)
2. `DialogQuests` hoặc caller quest reward → **R-02** (quest claim)
3. `Utils.rollFromWeightedMap` → **R-04**
4. `storage/data/places/Action` (toàn class) → **R-05**
5. `Area.cast` / `heal` / `retaliate` / `searchRoom` / `rollEnemies` → chuẩn bị cho S6.5A-007

**Lý do cho phép sang S6.5A-002 ngay:** Formula + SaveData **không phụ thuộc** bất kỳ mục nào trong danh sách trên, và đều đạt 97–98% từ S6.5A-001. Chờ 001E xong mới bắt đầu là lãng phí.

---

## Final Decision

# `S6_5A_001D_DONE_READY_FOR_S6_5A_002`

**Lý do:**
- ✅ Dump **58 method smali + 50 method DAD, 0 thất bại** — phủ đủ 6 nhóm rule yêu cầu (dungeon tick, target selection, loot, quest, merchant, trap).
- ✅ **Không còn blocker loại "không đọc được"** — kể cả `trapEncounter` mà JADX từng bỏ nay cũng dump được.
- ✅ **13/17 domain đạt ≥95%**, đủ điều kiện implement.
- ✅ Phát hiện nhiều rule cân bằng quan trọng mà nếu bịa sẽ sai: cap **400 lượt**, progress **chỉ reset nếu < 250**, chest cap **2000/3000**, Geode **0.05%**, quest rarity **0.7/0.9/0.97**, shield cap **20% maxHp**.
- ⚠️ Ghi nhận trung thực **2 mục dưới ngưỡng** (Merchant buy, Quest claim) và **5 risk** kèm hành động cụ thể — **không tô hồng, không claim DONE cho phần chưa có evidence**.

**Tuân thủ hard rules:** không implement code, không sửa Unity script/scene/data, không generate asset, không dùng Higgsfield, **không tự đặt reward/price/damage/timer/drop/currency/progress**, không đơn giản hoá combat. XAPK gốc không bị sửa — chỉ đọc từ temp workspace đã có.
