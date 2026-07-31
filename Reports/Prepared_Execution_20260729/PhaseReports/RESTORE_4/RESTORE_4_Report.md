# RESTORE_4 — DESIGNED SYSTEMS EXECUTION REPORT
## Generated: 2026-07-29

---

## 1. Pet System (G01) — EMPTY SHELL

### Current State

| Component | Location | Content | Status |
|-----------|----------|---------|--------|
| PetDefinition | Definitions/PetDefinition.cs | Empty `[Serializable] class PetDefinition : DefinitionBase { }` | ⚠️ SHELL |
| PetService | (missing) | **ZERO FILES** | ❌ NOT_PRESENT |
| PetSaveData | (missing) | **NOT IN SaveData** | ❌ NOT_PRESENT |
| PetScreen | (missing) | **ZERO FILES** | ❌ NOT_PRESENT |
| Pets/ directory | Scripts/Pets/ | Empty directory with .meta only | ⚠️ EMPTY |

### What Needs Implementation

| Item | Effort | Details |
|------|--------|---------|
| PetDefinition fields | ~0.5d | PetRarity, BaseAttack/Defense/HP/Speed, level mults, skill ID, evolution |
| PetSaveData + NormalizeAfterLoad | ~0.25d | InstanceId, DefinitionId, Level, Exp, IsEquipped, OwnerCharacterInstanceId |
| PetService | ~1d | CreatePet, AddExp, LevelUp, Equip/Unequip, GetStats |
| Combat integration | ~0.5d | Pet stats → CharacterService.GetTotalStat() |
| Loot integration | ~0.25d | Pet egg/part drops |
| PetScreen UI | ~1d | Grid, detail, equip/unequip, stats |
| Data JSON | ~0.5d | Pet definitions in StreamingAssets |

**Total: ~3-4 days**

---

## 2. DoctrineScreen (G04) — NOT IMPLEMENTED

| Component | Status | Notes |
|-----------|--------|-------|
| DoctrineScreen.cs | ❌ NOT_PRESENT | No UI screen class |
| DoctrineScreen prefab | ❌ NOT_PRESENT | No canvas layout |
| HUD button | ❌ NOT_PRESENT | No way to open |
| UIScreenId registration | ❌ NOT_PRESENT | Not in screen registry |

### What Exists
| Component | Location | Status |
|-----------|----------|--------|
| DoctrineService | Runtime/Services/DoctrineService.cs | ✅ Fully functional |
| IDoctrineService | Runtime/Services/IDoctrineService.cs | ✅ Interface |
| All 8 doctrines in SaveData | SaveData.cs:190-206 | ✅ Level + Progress fields |
| FormulaService doctrine calc | FormulaService.cs | ✅ TotalStarsToNextLp |

**DoctrineScreen needs: New UI file + prefab + registration. Backend already complete.**

---

## 3. Promotion System (G03) — NOT IMPLEMENTED

| Component | Status | Notes |
|-----------|--------|-------|
| PromotionDefinition | ❌ NOT_PRESENT | No definition type |
| PromotionService | ❌ NOT_PRESENT | No service |
| IPromotionService | ❌ NOT_PRESENT | No interface |
| PromotionTier in SaveData | ❌ NOT_PRESENT | Not on CharacterSaveData |
| PromotionScreen UI | ❌ NOT_PRESENT | No UI |
| Promotion JSON data | ❌ NOT_PRESENT | No StreamingAssets data |

**Verdict: Full implementation needed — design decisions required (tiers, costs, stat mults).**

---

## 4. Ascension Enhancement (G02) — MINIMAL (BOOL ONLY)

### Current State

| Aspect | Current | Expected | Status |
|--------|---------|----------|--------|
| SaveData field | `bool IsAscended` | `int AscensionLevel` | ⚠️ Bool only |
| FormulaService cost | None | `GetAscensionCost()` | ❌ NOT_PRESENT |
| Stat multiplier | None | `1.0 + (level * 0.1)` | ❌ NOT_PRESENT |
| UI | None | Ascend button on CharacterScreen | ❌ NOT_PRESENT |
| Service | None | Ascension methods | ❌ NOT_PRESENT |

### What Needs Implementation

| Item | Effort |
|------|--------|
| Change bool→int in SaveData + migration | ~0.25d |
| FormulaService ascension cost formula | ~0.25d |
| CharacterService ascension stat mult | ~0.25d |
| UI element on CharacterScreen | ~0.25d |
| Total | ~1d |

---

## 5. Shelter Fields (G13-G15)

### Current State

| Field | Type | Location | References | Status |
|-------|------|----------|------------|--------|
| `LevelShelter` | int | SaveData.cs:184 | FormulaService + tests | ⚠️ EXISTING |
| `UpgradeShelter` | int | SaveData.cs:185 | FormulaService + tests | ⚠️ EXISTING |
| `LevelShelterAutofeed` | int | SaveData.cs:186 | FormulaService + tests | ⚠️ EXISTING |

### Decision Recommendation

| Option | Action | Effort |
|--------|--------|--------|
| KEEP | LevelShelterAutofeed for future autofeed feature | ~0.1d |
| DEPRECATE | `[Obsolete]` on LevelShelter/UpgradeShelter (use quarters) | ~0.1d |

**Recommendation: Keep all 3 fields, no migration needed.**
`LevelShelterAutofeed` may be used for future auto-feeding pet system.
`LevelShelter`/`UpgradeShelter` are distinct from LevelQuarters/UpgradeQuarters in the formula system.

---

## 6. Verification Gate — PASS Criteria

| Check | Status |
|-------|--------|
| PetDefinition fields match pets.json | ❌ NOT PRESENT (empty shell) |
| PetService CreatePet → Save → Load | ❌ NOT PRESENT |
| Pet equipped → stats applied | ❌ NOT PRESENT |
| PetScreen shows pets, equip | ❌ NOT PRESENT |
| DoctrineScreen shows all 8 doctrines + progress | ❌ NOT PRESENT |
| Doctrine progress updates after quest claim | ✅ Backend works (DoctrineService) |
| PromotionDefinition matches design spec | ❌ NOT PRESENT |
| Promote → Tier → stat multiplier | ❌ NOT PRESENT |
| Promotion UI shows cost + requirements | ❌ NOT PRESENT |
| AscensionLevel (int) replaces IsAscended (bool) | ❌ NOT PRESENT (bool only) |
| Multiple ascension levels work + cost | ❌ NOT PRESENT |
| Stat multiplier with ascension | ❌ NOT PRESENT |
| Shelter fields decision documented | ✅ Decision: KEEP all 3 |
| NormalizeAfterLoad guards new fields | ❌ NOT PRESENT (no new fields added yet) |

---

## Phase Exit Verdict

| Feature | Verdict |
|---------|---------|
| Pet System (G01) | ❌ EMPTY SHELL — full implementation needed |
| DoctrineScreen (G04) | ❌ NOT_PRESENT — backend ready, UI missing |
| Promotion (G03) | ❌ NOT_PRESENT — full implementation needed |
| Ascension (G02) | ⚠️ MINIMAL — bool only, needs int + formula + UI |
| Shelter (G13-15) | ✅ Fields exist, KEEP recommendation |
| **Phase exit** | ❌ **FAIL — 4 of 5 features need implementation** |
