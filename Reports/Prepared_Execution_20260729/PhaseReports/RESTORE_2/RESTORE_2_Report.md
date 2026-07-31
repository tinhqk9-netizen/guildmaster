# RESTORE_2 — QUEST / RAID / PROGRESSION EXECUTION REPORT
## Generated: 2026-07-29

---

## 1. Quest System Verification

### QuestService — Full API

| Method | File:Line | SaveData Mutation | Status |
|--------|-----------|-------------------|--------|
| `GetActiveQuests()` | QuestService.cs:29 | Read-only from `_activeQuests` | ✅ STATIC_TRACE_CONFIRMED |
| `LoadQuests()` | QuestService.cs:34 | Deserializes `SaveData.Quests` → runtime | ✅ |
| `SaveQuests()` | QuestService.cs:51 | Writes runtime → `SaveData.Quests` | ✅ |
| `Increment(id, amount)` | QuestService.cs:64 | Progress + complete → SaveQuests() | ✅ |
| `IncrementToValue(id, val)` | QuestService.cs:81 | Set value → SaveQuests() | ✅ |
| `ClaimReward(id, doctrine)` | QuestService.cs:110 | Doctrine/Gems → Remove quest → QuestsCompleted++ | ✅ |

### Quest Condition Callers — MISSING

| Quest Type | Caller Needed | Actual Caller | Status |
|-----------|--------------|---------------|--------|
| DUNGEON_COMPLETE | DungeonService → QuestService.Increment() | **NOT FOUND** | ❌ MISSING_CALLER |
| COLLECT_ITEM | InventoryService.AddItem() → QuestService.Increment() | **NOT FOUND** | ❌ MISSING_CALLER |
| CRAFT_ITEM | CraftService.ClaimCompletedCraft() → Quest Service | **NOT FOUND** | ❌ MISSING_CALLER |
| KILL_ENEMIES | CombatService → QuestService.Increment() | **NOT FOUND** | ❌ MISSING_CALLER |
| EQUIP_ITEM | EquipmentService.Equip() → QuestService.Increment() | **NOT FOUND** | ❌ MISSING_CALLER |
| MONEY_EARNED | MerchantService.ClaimSoldItem() → QuestService.Increment() | **NOT FOUND** | ❌ MISSING_CALLER |

**Only caller found:** `S6_5A_RuntimeActionSmokeTest.cs:184` (Test code only)

**Verdict: ALL 56 quests have NO production callers — quests cannot progress in normal gameplay.**

### ClaimReward → Doctrine Integration

| Step | File:Line | Status |
|------|-----------|--------|
| `ClaimReward()` calls `_doctrineService.AddProgress()` | QuestService.cs:129 | ✅ CONFIRMED |
| `AddProgress()` → FormulaService for level calc | DoctrineService.cs:60 | ✅ CONFIRMED |
| All 8 doctrines handled | DoctrineService.cs:77-84 | ✅ CONFIRMED |
| Hardcoded rarity=1, always War doctrine | QuestService.cs:117-118 | ⚠️ Default behavior |
| Quest removed from active list on claim | QuestService.cs:132 | ✅ |

---

## 2. Doctrine Service — All 8 Types

| Doctrine | SaveData Field (Level) | SaveData Field (Progress) | Status |
|----------|------------------------|---------------------------|--------|
| Affliction | `AfflictionLevel` | `AfflictionProgress` | ✅ |
| Control | `ControlLevel` | `ControlProgress` | ✅ |
| Fortitude | `FortitudeLevel` | `FortitudeProgress` | ✅ |
| Grace | `GraceLevel` | `GraceProgress` | ✅ |
| Illusion | `IllusionLevel` | `IllusionProgress` | ✅ |
| Knowledge | `KnowledgeLevel` | `KnowledgeProgress` | ✅ |
| Ruin | `RuinLevel` | `RuinProgress` | ✅ |
| War | `WarLevel` | `WarProgress` | ✅ |
| `DoctrineMaxed` | `SaveData.DoctrineMaxed` | bool | ✅ |

---

## 3. Raid System — NOT IMPLEMENTED

| Component | Expected | Actual | Status |
|-----------|----------|--------|--------|
| IRaidService | Interface + 5 methods | **ZERO FILES** | ❌ NOT_PRESENT |
| RaidService | Concrete implementation | **ZERO FILES** | ❌ NOT_PRESENT |
| RaidScreen | UI Screen | **ZERO FILES** | ❌ NOT_PRESENT |
| RaidDefinition | Definition type | **ZERO FILES** | ❌ NOT_PRESENT |
| RaidSaveData | Save serialization | **NOT PRESENT** | ❌ NOT_PRESENT |

**Verdict: Raid System requires full implementation (G08).**

---

## 4. Dungeon Chain Unlock Gate (G05) — NOT IMPLEMENTED

| Check | Expected | Actual | Status |
|-------|----------|--------|--------|
| `CanStartDungeon()` | Chain conditions | **NOT PRESENT** in DungeonService | ❌ NOT_PRESENT |
| `RequiresPreviousDungeonId` | Field on DungeonDefinition | Search found no references | ❌ NOT_PRESENT |
| Lock icon on locked dungeons | UI element | **NOT IMPLEMENTED** | ❌ NOT_PRESENT |
| RequiredLevel check | Character level gate | **NOT PRESENT** | ❌ NOT_PRESENT |

---

## 5. Unlock Configuration (G18) — NOT IMPLEMENTED

| Component | Expected | Actual | Status |
|-----------|----------|--------|--------|
| IUnlockService | Interface | **ZERO FILES** | ❌ NOT_PRESENT |
| UnlockService | Implementation | **ZERO FILES** | ❌ NOT_PRESENT |
| UnlockCondition config | Config data | **NOT PRESENT** | ❌ NOT_PRESENT |
| Unlock wiring | Hooks to other services | **NOT PRESENT** | ❌ NOT_PRESENT |

---

## 6. Verification Gate — PASS Criteria

| Check | Status |
|-------|--------|
| QuestScreen opens from HUD | `NOT_RUN` (needs Unity) |
| All 56 quests mapped to callers | ❌ **ALL 56 MISSING_CALLER** |
| Quest → Doctrine reward integration | ✅ STATIC_TRACE_CONFIRMED |
| ClaimReward → SaveData mutation | ✅ STATIC_TRACE_CONFIRMED |
| RaidService registered + optional | ❌ NOT_PRESENT |
| RaidScreen opens + shows raids | ❌ NOT_PRESENT |
| Raid combat (reuses CombatService) | ❌ NOT_PRESENT |
| Dungeon chain unlock gate | ❌ NOT_PRESENT |
| Locked dungeons show lock icon | ❌ NOT_PRESENT |
| UnlockService registered + wired | ❌ NOT_PRESENT |
| Unlock conditions configurable | ❌ NOT_PRESENT |

---

## Phase Exit Verdict

| Criterion | Verdict |
|-----------|---------|
| Quest system API traced | ✅ 8 methods confirmed |
| Quest callers mapped | ❌ 56/56 MISSING_CALLER |
| Doctrine integration | ✅ All 8 types + AddProgress |
| Raid system | ❌ Full implementation needed |
| Dungeon chain unlock gate | ❌ Not implemented |
| Unlock configuration | ❌ Not implemented |
| **Phase exit** | ❌ **FAIL — multiple gaps requiring implementation** |
