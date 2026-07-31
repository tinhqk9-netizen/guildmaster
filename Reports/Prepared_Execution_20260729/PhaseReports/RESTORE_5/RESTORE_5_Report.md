# RESTORE_5 — SAVE / OFFLINE / UI POLISH EXECUTION REPORT
## Generated: 2026-07-29

---

## 1. Save Migration

### Current State

| Aspect | File | Status |
|--------|------|--------|
| `SaveMetadata.SaveVersion` | SaveData.cs | Set = 1 in Save() | ✅ |
| `SaveMetadata.SaveVersion` validation | SaveService.cs | Read on Load but NOT validated | ❌ C5 |
| `MigrateSave()` | SaveService.cs:138-141 | EMPTY PLACEHOLDER | ❌ H5 |
| `DataVersion` in Metadata | SaveData.cs | NOT used in compatibility checking | ❌ C5 |

### Required Migration Steps (post-RESTORE_4 schema changes)

| Version Change | Migration | Required After |
|---------------|-----------|----------------|
| v0→v1 (future) | Add Pets List → NormalizeAfterLoad handles null | RESTORE_4 pets |
| v0→v1 | Convert `IsAscended` bool → `AscensionLevel` int | RESTORE_4 ascension |
| v0→v1 | Add `PromotionTier = 0` to all CharacterSaveData | RESTORE_4 promotion |

### Current NormalizeAfterLoad Compatibility

| New Field | Backward Compat | Mechanism |
|-----------|----------------|-----------|
| Pets List (if added) | ✅ Null→new List<> | NormalizeAfterLoad guard |
| AscensionLevel (int) | ❌ JsonUtility deserializes missing int as 0 | Same as default → no migration needed (0 = base) |
| PromotionTier (int) | ❌ JsonUtility deserializes missing int as 0 | Same as default → no migration needed (0 = no promotion) |

---

## 2. Offline Progress Edge Cases

### OfflineProgressService API

| Method | File:Line | Status |
|--------|-----------|--------|
| `CalculateOfflineDeltaSeconds(last, now)` | OfflineProgressService.cs:22 | ✅ Ported formula |
| `ApplyOfflineProgress(currentUnix)` | OfflineProgressService.cs:31 | ✅ Calls Craft + Merchant |
| Called at startup? | — | ❌ C4 — NOT called |
| Called in UIRuntimeBootstrap? | — | ❌ NOT PRESENT |

### Edge Case Tests

| Scenario | Expected | Actual Behavior | Status |
|----------|----------|----------------|--------|
| First launch (no save) | delta = 0 | NormalizeAfterLoad creates Metadata → SaveTimeUnix=0 | ⚠️ Need to confirm zero safe |
| Normal reopen (1h later) | delta ≈ 3600 | `now - lastSaveUnix` | ✅ Formula correct |
| Clock went back | delta clamped to 0 | Need check in CalculateOfflineDeltaSeconds | ⚠️ UNVERIFIED |
| Long offline >30 days | cap at 7 days (configurable) | Portal calc has MAX_OFFLINE_SECONDS | ⚠️ UNVERIFIED |

### What Offline Progresses

| System | Progressed? | Method | Status |
|--------|------------|--------|--------|
| Workshop / Craft | ✅ | CraftService.ProgressWorkshop(delta) | ✅ |
| Market / Merchant | ✅ | MerchantService.ProgressMarket(delta) | ✅ |
| Tavern visitors | ⚠️ Has ProgressVisitorTime() but NOT called | OfflineProgressService | ❌ NOT_INVOKED |
| Active dungeon | ❌ Not progressed | Deferred by design | ⚠️ DEFERRED |

---

## 3. Active-State Restoration (G07)

| Component | Status | Notes |
|-----------|--------|-------|
| ActiveDungeon serialized | ✅ | SaveDungeonState() writes full state |
| ActiveDungeon deserialized | ✅ | LoadDungeonState() restores from save |
| ActiveDungeon re-entered on load | ❌ NOT IMPLEMENTED | Data loaded but combat not re-entered |
| Auto-complete on load (option A) | ❌ NOT IMPLEMENTED | No logic to complete dungeon post-load |
| Abandon on load (option C) | ❌ NOT IMPLEMENTED | No logic to clear active dungeon post-load |

**Recommendation:** Option A (auto-complete with reduced loot) for MVP. For now, ActiveDungeon persists but does NOT resume combat.

---

## 4. Loading Screen (G12) — NOT IMPLEMENTED

| Component | Status | Notes |
|-----------|--------|-------|
| LoadingScreen.cs | ❌ NOT_PRESENT | No loading screen class |
| Canvas overlay prefab | ❌ NOT_PRESENT | No UI element |
| Boot progress events | ❌ NOT_PRESENT | No progress event system |
| Fade-to-black transition | ❌ NOT_PRESENT | No scene transition |

---

## 5. Verification Gate — PASS Criteria

| Check | Status |
|-------|--------|
| Save migration v0→current works | ❌ NOT IMPLEMENTED (MigrateSave empty) |
| Old save loads without errors | ✅ NormalizeAfterLoad handles old schema |
| SaveVersion tracking correct | ⚠️ Written on save, not validated on load |
| Offline delta calc — first launch | ⚠️ UNVERIFIED |
| Offline delta calc — normal reopen | ✅ Formula confirmed |
| Offline delta calc — clock went back | ⚠️ UNVERIFIED |
| Offline delta calc — long offline | ⚠️ UNVERIFIED (cap exists?) |
| Offline workshop progressed | ✅ CONFIRMED |
| Offline market progressed | ✅ CONFIRMED |
| Active dungeon state handled (option A/B/C) | ❌ NOT IMPLEMENTED |
| Loading screen visible during boot | ❌ NOT PRESENT |
| All 23 regression flows pass | ❌ No regression framework |
| Save → Exit → Reload → Same state | ⚠️ PARTIAL — data persists but combat not re-entered |
| No null refs on fresh start | ⚠️ UNVERIFIED (needs Unity) |

---

## 6. Regression Flow Status

| ID | Flow | Status |
|:--:|------|--------|
| RF01 | HUD shows Money/Gems | `NOT_RUN` |
| RF02 | Inventory screen opens | `NOT_RUN` |
| RF03 | Equipment screen opens | `NOT_RUN` |
| RF04 | Character screen opens | `NOT_RUN` |
| RF05 | Tavern screen opens | `NOT_RUN` |
| RF06 | Recruit adventurer | `STATIC_CONFIRMED` |
| RF07 | Equip item on character | `STATIC_CONFIRMED` |
| RF08 | Dungeon list shows | `NOT_RUN` |
| RF09 | Start dungeon combat | `STATIC_CONFIRMED` |
| RF10 | Loot collected after dungeon | `STATIC_CONFIRMED` |
| RF11 | Craft screen opens | `NOT_RUN` |
| RF12 | Start craft with materials | `STATIC_CONFIRMED` |
| RF13 | Claim completed craft | `STATIC_CONFIRMED` |
| RF14 | Merchant screen opens | `NOT_RUN` |
| RF15 | Buy from merchant | `STATIC_CONFIRMED` |
| RF16 | Sell item to market | `STATIC_CONFIRMED` |
| RF17 | Quest screen opens | `NOT_RUN` |
| RF18 | Claim quest reward | `STATIC_CONFIRMED` |
| RF19 | Doctrine progress shown | `NOT_RUN` |
| RF20 | Settings toggle works | `NOT_RUN` |
| RF21 | Save → Exit → Reload | `NOT_RUN` |
| RF22 | Offline progress applied | `STATIC_CONFIRMED` (code present but not called) |
| RF23 | No crashes on fresh install | `NOT_RUN` |

---

## Phase Exit Verdict

| Criterion | Verdict |
|-----------|---------|
| Save migration implemented | ❌ NOT IMPLEMENTED (empty placeholder) |
| Old save loads without errors | ✅ NormalizeAfterLoad handles all field types |
| Offline delta edge cases | ⚠️ PARTIAL — confirmed logic, unverified edge cases |
| Offline workshop/market progress | ✅ CONFIRMED via ProgressWorkshop + ProgressMarket |
| Active dungeon state handled | ❌ NOT IMPLEMENTED (no re-entry/complete/abandon) |
| Loading screen present | ❌ NOT PRESENT |
| 23 regression flows pass | ❌ NO TEST FRAMEWORK |
| **Phase exit** | ❌ **FAIL — critical gaps in migration, active state, and regression** |
