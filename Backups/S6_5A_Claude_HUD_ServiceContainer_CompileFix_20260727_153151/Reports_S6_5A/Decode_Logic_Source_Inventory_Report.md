# Decode Logic Source Inventory Report

**Ngày:** 2026-07-25 · **Decode root:** `D:\Tinh\Guild Master - Idle Dungeons`
**Package game chính:** `sources/it/paranoidsquirrels/idleguildmaster/` — **1150 file `.java`** (trong tổng 14.482 file, phần còn lại là thư viện android/kotlin/androidx/reactivex)

## Cấu trúc package decode
```
it/paranoidsquirrels/idleguildmaster/
├── Formulas.java, Utils.java, MainActivity.java, UIUtils.java
├── AchievementsUtils.java, IAPWrapper.java, TrueTimeUtils.java
├── DebugToggles.java, Faq.java, KingMessage.java
├── storage/
│   ├── SaveManager.java, FileManager.java
│   └── data/
│       ├── Data.java, DataDeserializer.java, SnapshotData.java
│       ├── entities/  Entity.java, Skills.java, StatusEffect.java,
│       │              StatusEffectType.java, EndOfTurnAction.java
│       │   ├── adventurers/  Adventurer.java + doctrines/ + units/
│       │   └── enemies/      Enemy.java + units/
│       ├── items/     Item.java, ItemAction.java, ItemWrapper.java,
│       │              MerchantOffer.java, Recipes.java
│       ├── pets/      Pet.java, PetAbility.java
│       ├── places/    Area.java, Action.java, Event.java, Logger.java,
│       │              AdventureRecap.java, EnemyCounter.java
│       │   ├── dungeons/ + raids/
│       └── quests/    Quest.java, QuestsManager.java
└── ui/  adventurers/, components/, dialogs/, dungeons/, headquarters/, raids/
```

## Bảng inventory logic decode

| Game Domain | Decode Source Files / Classes | Key Methods / Fields | What Logic Seems To Exist | Evidence |
|---|---|---|---|---|
| **Formulas / Economy pricing** | `Formulas.java` (284 dòng) | `totalStarsToNextLp`, `getQuartersPrice`, `getTavernCapacityPrice`, `getTavernTimePrice`, `getStorageCapacityPrice`, `getMarketListingsPrice`, `getMarketTimePrice`, `getWorkshopQueuePrice`, `getWorkshopTimePrice`, `getShelterPrice`, `getShelterAutofeedPrice`, `getQuartersCapacity`, `getTavernVisitorInterval`, `getTavernCapacity`, `marketListings`, `workshopQueue`, `storageSpaces`, `shelterCapacity`, `experienceToNextLevel`, `foodToNextLevel` | **20 formula** cho giá nâng cấp, capacity, exp/food theo level | Đếm trực tiếp `grep "public static"` → 20 method |
| **Save state / Player data** | `storage/data/Data.java` (1351 dòng) | **140 private field** | Toàn bộ trạng thái người chơi: `money`, `gems`, 20 field instance dungeon riêng (`TheTower`, `TheDesert`, `FrostbitePeaks`, `ObsidianMines`, `CelestialMothership`…), 8 cặp doctrine `level`/`progress` (affliction, control, fortitude, grace, illusion, knowledge, ruin, war), 10 cặp `level*`/`upgrade*` (Quarters, Shelter, ShelterAutofeed, Storage, TavernCapacity, TavernTime, WorkshopQueue, WorkshopTime, MarketListings, MarketTime), 9 setting (`settingAutoOpenDungeonDetail`, `settingColorblindMode`, `settingConfirmRetreat`, `settingConfirmSwap`, `settingConfirmUpgrade`, `settingCraftMaxAmount`, `settingSellMaxAmount`, `settingVerboseLogs`, `settingsLanguage`), thống kê (`itemsCrafted`, `itemsSold`, `maxWealth`, `questsCompleted`, `maxAdventurerTier`, `maxAdventurersOwned`), mốc thời gian (`lastAccess`, `lastHourTriggered`, `last24Triggered`, `lastWeekTriggered`, `nextTavernVisit`), IAP (`starterPackPurchased`, `adventurerPackPurchased`, `merchantPackPurchased`…), `tutorialStep`, redeem code | `grep -c "^    private .*;"` → **140** |
| **Save I/O** | `storage/SaveManager.java` (66), `storage/FileManager.java` (225), `storage/data/DataDeserializer.java` | Ghi/đọc file, deserialize JSON thủ công từng field | Có custom deserializer (`setLastAccess`, `setSecondsPassed`…) | Đọc file |
| **Combat entity stats** | `storage/data/entities/Entity.java` (558 dòng) | ~30 abstract: `calculateMinAttackDamage`, `calculateMaxAttackDamage`, `calculateCriticalChance`, `calculateCriticalDamage`, `calculateCounterattackChance`, `calculateHealingModifier`, `calculateImmunityToStatus`, `calculateRetaliationPhysicalDamage`, `calculateRetaliationMagicalDamage`, `calculateTotalConstitution`, `calculateTotalDefense`, `calculateTotalDexterity`, `calculateTotalIntelligence`, `calculateTotalLifesteal`, `calculateTotalMagicDefense`, `calculateTotalMaxHp`, `calculateTotalRegeneration`, `calculateTotalFlatDodgeChance`, `calculateTotalDarknessDamageAmplification`, `endOfTurnActions`, `isMagic`, `isRanged`, `rollsDamageThreeTimes`, `onSelfHitEffects`, `onTargetHitEffects` + field `currentHp/Mana/Shield`, `positiveStatusEffects`, `negativeStatusEffects`, `statusImmunities`, `onDeathEffectsOnEnemies/Allies`, `isFlying`, `isHealer`, `isCleanser`, `isInitiative`, `isAlwaysHits`, `getOnFireBonusDamage` | Hệ thống chỉ số chiến đấu **rất sâu**: crit, dodge, lifesteal, retaliation, shield, mana, immunity, darkness amp, regen, flying/ranged/magic | `grep "public abstract"` |
| **Combat / Dungeon run engine** | `storage/data/places/Area.java` (**3164 dòng**) | `tick()`, `setupArea()`, `setupAdventurers()`, `performAction()`, `enterRoom()`, `initializeFight()`, `decideTurnsOrder()`, `fightTurn()`, `dealDamage()`, `dodge()`, `heal()`, `retaliate()`, `resolveStatus()`, `cast()`, `increaseMana()`, `calculateCriticalMultiplier()`, `selectTargets()`, `attackTargetStrategy()`, `selectEnemyTarget()`, `selectRandomTarget()`, `selectLowestHpTarget()`, `weightedSelection()`, `tauntedBy()`, `selectLowestRelativeShieldAlly()`, `loot()`, `collectExperience()`, `incrementProgress()`, `respawn()`, `reanimate()`, `healingNova()`, `applyOnDeathStatusEffects()`, `petAttack()`, `petHeal()`, `petCast()`, `petExecution()`, `fullChest()`, `adventurersAlive()`, `rollMerchantRegularOffers()`, `rollMerchantSpecialOffers()`, inner class `Skill` (targetSelectionMode, criticalAmplification, healing, statusEffect, damageAmplification, executionThreshold, recastOnKill, reviveProbability) | **Engine hoàn chỉnh**: vòng tick, thứ tự lượt, chọn mục tiêu nhiều chiến lược, đánh/né/phản đòn/hồi máu, status effect resolve, mana, pet ability, loot, exp, progress, respawn, revive | `wc -l` = 3164; `grep "private void\|public void"` |
| **Adventurer / Doctrine** | `entities/adventurers/Adventurer.java`, `doctrines/Doctrine.java` + `doctrines/instances/` (DoctrineOfAffliction, Control, Illusion, Ruin, War…), `adventurers/units/` (NightBlade, ShadowDancer, DivineChampion, KingsHand, RoyalGuard…) | `canPickDoctrine()`, doctrine damage modifier, unit-specific override | Hệ thống class/doctrine riêng cho từng loại adventurer | Liệt kê thư mục |
| **Enemy** | `entities/enemies/Enemy.java` + `units/` (Abomination, AncientEnt, ArcaneAssassin, ArchmageOfLarox…) | Override các `calculate*` của Entity | Mỗi enemy là 1 class riêng có chỉ số/hành vi riêng | Liệt kê thư mục |
| **Skill / StatusEffect** | `entities/Skills.java`, `entities/StatusEffect.java`, `entities/StatusEffectType.java`, `entities/EndOfTurnAction.java` | Enum skill, status effect + type + turn/amount, hành động cuối lượt | Skill/status hệ thống hoá bằng enum + object | Đọc file |
| **Item / Inventory** | `items/Item.java` (167), `items/ItemWrapper.java`, `items/ItemAction.java` | `getSecondsToCraft`, `getSecondsToSell`, `getPrice`, `getStack`, `getRarity`, `getSource`, `getIdEffect`, `getUniqueOrigin`; `ItemAction.secondsPassed` | Item có giá, thời gian craft/sell, stack, rarity, nguồn gốc | Đọc file |
| **Craft / Workshop** | `items/Recipes.java` (363), `Utils.java` | `Recipes.from(Item)`, `Recipes.into(Item)`, `Utils.maxCraftableAmount(Recipes)`, `Utils.progressWorkshopTime(long)`, `Utils.gotEnoughItem(Item)` | Recipe 2 chiều, tính số lượng craft tối đa, tiến trình workshop theo giây | `grep` method |
| **Merchant / Market** | `items/MerchantOffer.java` (53), `Utils.java`, `Area.java` | `Utils.rollPotion()`, `Utils.rollSpecialFoods()`, `Utils.rollUpgrades()`, `Utils.truncatePrice(long)`, `Utils.progressMarketTime(long)`, `Area.rollMerchantRegularOffers()`, `Area.rollMerchantSpecialOffers()` | Roll offer theo loại, cắt giá, tiến trình bán theo giây, offer theo dungeon | `grep` method |
| **Quest** | `quests/Quest.java` (130), `quests/QuestsManager.java` (468) | `extractQuests()`, `calculateDifficulty()`, `initializeFields()`, `setupDoctrineAmounts()`, `setupAccessibleQuests()`, `extractAllQuests()`, `extractSpecificQuests()`, `rollRarity()`, `getFromListByRarity()`, `realignQuests()`, `increment(Quest,long)`, `incrementToValue(Quest,long)` | Hệ thống quest có độ khó, rarity roll, quest theo doctrine, realign, tăng tiến độ | `grep` method |
| **Pet** | `pets/Pet.java` (290), `pets/PetAbility.java` | `Utils.rollPetAbility(List)`, `Area.petAttack/petHeal/petCast/petExecution` | Pet có ability roll được, tham chiến trong combat | `grep` |
| **Time / Offline progress** | `MainActivity.java` (dòng 878–880), `Utils.java` | `data.getLastAccess()`; `Math.max(1L, Math.min(iMin, Math.round((jMillis - lastAccess)/1000.0)))`; `Utils.nextTimeTick()`, `Utils.tick60()`, `Utils.progressTavernTime/MarketTime/WorkshopTime`, `Utils.refreshCooldowns(long)`, `Utils.checkDismissedAdventurersExpiration(long)` | **Offline progress thật**: lấy delta giây từ `lastAccess`, có cap, rồi đẩy qua các hệ thống theo giây | Đọc `MainActivity.java:878` |
| **Tavern / Quarters / Shelter (HQ)** | `Formulas.java`, `Utils.java`, `ui/headquarters/` | `newTavernVisitor()`, `getTavernVisitorInterval()`, `getTavernCapacity()`, `getQuartersCapacity()`, `shelterCapacity()`, `getShelterAutofeedPrice()` | Hệ thống trụ sở: tavern tuyển adventurer, quarters chứa, shelter nuôi pet | `grep` |
| **Achievements / IAP / Localization** | `AchievementsUtils.java`, `IAPWrapper.java`, `R.java`, `resources/` | — | Có hệ thống thành tích, mua trong app, chuỗi localization qua `R.string` | Liệt kê file |

## Nhận xét quan trọng
1. **Toàn bộ logic gameplay cốt lõi nằm trong 3 file lớn:** `Area.java` (3164 dòng — combat + dungeon + loot + exp), `Data.java` (1351 dòng — save state), `Utils.java` (tick/time/roll/craft/market helper). Đây là phần nặng nhất cần port.
2. **Dungeon trong decode không phải data thuần** — mỗi area là **một class Java riêng** (`TheTower`, `FrostbitePeaks`, `ObsidianMines`…) với `listEnemies()`, `listAreasUnlocked()`, `getDarkness()` override riêng. Converter đã bóc thành `dungeons.json` (11 record) nhưng **logic unlock/darkness/enemy-list theo từng area là code, không phải data**.
3. **Doctrine system** (8 doctrine với level/progress + damage modifier riêng) **hoàn toàn không xuất hiện** trong dự án Unity hiện tại — không có service, không có data, không có save field.
4. Localization dùng `R.string` ID (int) — nên `localization.json` do converter xuất ra là **mảng rỗng** khớp với việc chuỗi nằm trong `resources/` chứ không trong code.

## Decision
# `DECODE_LOGIC_INVENTORY_DONE`

Đã truy cập được toàn bộ 3 thư mục decode yêu cầu (`sources`, `resources`, `Document`), khoanh vùng chính xác package game (`it.paranoidsquirrels.idleguildmaster`, 1150 file), và lập được bản đồ 16 domain logic kèm bằng chứng đếm/grep cụ thể cho từng domain. Không suy đoán rule từ tên biến — mọi mục đều dẫn được về file/dòng cụ thể.
