# DECODE EVIDENCE MATRIX
## Generated: 2026-07-29

Every hardcoded value or formula claim traced to source data or marked UNVERIFIED.

---

## 1. FormulaService — Constants

### From Formulas.java (decompiled original)

| Constant | Value | Source | Status |
|----------|-------|--------|--------|
| BASE_STORAGE_SPACES | 35 | Formulas.java:STORAGE_SPACES | ✅ TRACED |
| BASE_TAVERN_CAPACITY | 4 | Formulas.java:QUARTERS_SLOTS | ✅ TRACED |
| BASE_QUARTERS_CAPACITY | 2 | Formulas.java:QUARTERS_CAPACITY | ✅ TRACED |
| BASE_WORKSHOP_QUEUE | 1 | Formulas.java:WORKSHOP_QUEUE | ✅ TRACED |
| BASE_MARKET_LISTINGS | 2 | Formulas.java:MARKET_SLOTS | ✅ TRACED |
| VISITOR_INTERVAL_SECONDS | 300 | Formulas.java:TAVERN_VISITOR_INTERVAL | ✅ TRACED |

### Formula: UpgradeCost

| Parameter | Formula Pattern | Source | Status |
|-----------|----------------|--------|--------|
| Quarters | `100 * (level ^ 1.5)` | Formulas.java:upgradeCost | ✅ TRACED |
| Storage | `50 * (level ^ 1.2)` | Formulas.java:upgradeCost | ✅ TRACED |
| Workshop time | `75 * (level ^ 1.3)` | Formulas.java:upgradeCost | ✅ TRACED |
| Workshop queue | `200 * (level ^ 1.5)` | Formulas.java:upgradeCost | ✅ TRACED |
| Market time | `60 * (level ^ 1.25)` | Formulas.java:upgradeCost | ✅ TRACED |
| Market listings | `150 * (level ^ 1.4)` | Formulas.java:upgradeCost | ✅ TRACED |

### Formula: Tavern

| Parameter | Formula | Source | Status |
|-----------|---------|--------|--------|
| Tavern Capacity | `BASE + (LevelTavernCapacity * 2) + packBoni` | Formulas.java:getQuartersSlots | ✅ TRACED |
| Quarters Capacity | `BASE + (LevelQuarters * 2) + packBoni` | Formulas.java:getQuartersCapacity | ✅ TRACED |
| Visitor Interval | `BASE - (LevelTavernTime * 10) - packBoni` (min: 60) | Formulas.java:getVisitorInterval | ✅ TRACED |
| Recruit Cost | `50 + (ownedCharCount * 25)` | Formulas.java:recruitCost | ✅ TRACED |

### Formula: Combat (from Entity.java DAD = rollAttackDamage)

| Component | Source | Status |
|-----------|--------|--------|
| minDamage = minAttack | Entity.rollAttackDamage() | ✅ TRACED (CombatService.cs:87) |
| maxDamage = maxAttack | Entity.rollAttackDamage() | ✅ TRACED (CombatService.cs:88) |
| rawDamage = random(min, max) | Entity.rollAttackDamage() | ✅ TRACED |
| 3xDamageRoll flag | Entity.rollAttackDamage() | ✅ TRACED (CombatService.cs:92) |
| Defense = Base + Con | Entity.getDefense() | ✅ TRACED (AdventurerWrapper:177) |
| MagicDefense = Base + Con | Entity.getMagicDefense() | ✅ TRACED |
| damageRatio = 1.0 - (def / (def + 100)) | Entity.applyDamage() | ✅ TRACED (CombatService.cs:110) |
| flatReduction = target.FlatDamageReduction | Entity.applyDamage() | ✅ TRACED |
| reducedDamage = (1.0 - ratio) * raw - flatReduction | Entity.applyDamage() | ✅ TRACED (CombatService.cs:112) |
| finalDamage = max(1, round(reduced)) | Entity.applyDamage() | ✅ TRACED (CombatService.cs:113) |

### Formula: Craft/Merchant

| Parameter | Formula | Status |
|-----------|---------|--------|
| Craft time | `RecipeDefinition.CraftTimeSeconds` (from data) | ✅ TRACED (CraftService.cs:136) |
| Sell price | Item value * marketFee% | ✅ TRACED (MerchantService) |
| Buy price | `ComputeBuyPrice(def, dungeonId)` from Formulas | ✅ TRACED |
| Workshop Queue Cap | `WorkshopQueue(level, upgrade, flags)` | ✅ TRACED |

### Formula: Doctrine

| Parameter | Formula | Status |
|-----------|---------|--------|
| TotalStarsToNextLp(level) | `(level + 1) * 10` | ✅ TRACED (FormulaService) |
| AddProgress | Calls TotalStarsToNextLp for level calc | ✅ TRACED (DoctrineService.cs:60) |

---

## 2. Hardcoded Values in Service Code

### QuestService

| Value | Location | Source | Status |
|-------|----------|--------|--------|
| `rarity = 1` (default) | QuestService.cs:117 | Recovered Rule #2 default | ⚠️ HARDCODED — should use quest definition rarity |
| `isGems = false` | QuestService.cs:118 | Default to Doctrine reward | ⚠️ HARDCODED — should use quest definition |
| Rarity→Reward Table | QuestService.cs:99-108 | Recovered Rule #2 | ✅ TRACED to Java spec |

### TavernService

| Value | Location | Source | Status |
|-------|----------|--------|--------|
| `maxMobLevel = 2` | TavernService.cs:160 | Default guest level range | ⚠️ HARDCODED — could be data-driven |
| Default weapon assignment | TavernService.cs:170-183 | Guest generation logic | ⚠️ HARDCODED — uses specific item IDs |

### DungeonService

| Value | Location | Source | Status |
|-------|----------|--------|--------|
| `PROGRESS_KEEP_THRESHOLD` | DungeonService.cs | Floor count for progress save | ✅ TRACED |
| `MAX_FLOORS` | DungeonService.cs | Endless dungeon cap | ✅ TRACED |

---

## 3. Java-to-Unity Mapping

| Java Class/Method | Unity Class/Method | Status | Notes |
|-------------------|-------------------|--------|-------|
| `Data.java` | `SaveData.cs` | ✅ | All fields mapped with PascalCase |
| `Entity.java:rollAttackDamage()` | `CombatService.RollAttackDamage()` | ✅ | Ported with source annotations |
| `Entity.java:applyDamage()` | `CombatService.ApplyDamage()` | ✅ | Ported with source annotations |
| `Entity.java` subclasses | `AdventurerWrapper`, `EnemyRuntime` | ✅ | Wrapper pattern |
| `Formulas.java` | `FormulaService.cs` | ✅ | 252 lines with annotations |
| `pets.json` | `PetDefinition.cs` | ❌ EMPTY | Definition exists but no fields |
| `Doctrine` | `DoctrineService.cs` | ✅ | 8 doctrines fully implemented |
| `Raid` | (missing) | ❌ | Not ported yet |
| `Item.java` | `ItemRuntime.cs` | ✅ | Core model ported |
| `Character.java` | `CharacterRuntime.cs` | ✅ | Core model ported |

---

## 4. Verified Transactions

| Transaction | Source | Producer | Consumer | Verified |
|------------|--------|----------|----------|----------|
| Money deducted for recruit | TavernService:81 | RecruitGuest | SaveData.Money -= cost | ✅ |
| Money deducted for upgrade | TavernService:223-236 | Upgrade* | SaveData.Money -= price | ✅ |
| Money added from item sale | MerchantService:181 | ClaimSoldItem | SaveData.Money += earned | ✅ |
| Money deducted for buy | MerchantService:101-102 | BuyOffer | SaveData.Money -= price | ✅ |
| Gems deducted for buy | MerchantService:102 | BuyOffer | SaveData.Gems -= price | ✅ |
| Gems added from quest | QuestService:125 | ClaimReward | SaveData.Gems += amount | ✅ |
| Doctrine progress added | DoctrineService:54-85 | AddProgress | SaveData.*Progress += amount | ✅ |
| Items added to inventory | InventoryService:86-103 | AddItem | SaveData.Items + runtime | ✅ |
| Items removed from inv | InventoryService:112-128 | RemoveItem | SaveData.Items - runtime | ✅ |

All 9 verified transactions confirmed from source code trace.

---

## 5. Unverified Claims

| Claim | Why Unverified |
|-------|---------------|
| "Combat loop completes correctly" | Needs Unity in-editor play test — dungeon state transitions can't be proven from static trace alone |
| "Loot tables drop correct items" | DropTable entries need runtime simulation — weighted random selection |
| "Offline delta calculation correct" | Edge cases (clock back, first launch, long offline) need runtime test |
| "UI screens open without errors" | All screens need Unity scene to verify prefab connections |
| "Fresh install doesn't crash" | Needs platform build test |
