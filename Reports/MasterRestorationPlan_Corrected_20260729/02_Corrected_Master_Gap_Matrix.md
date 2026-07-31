# CORRECTED MASTER GAP MATRIX

---

## 🔴 FEATURE_BLOCKER

| ID | Gap | Audit Source | Sub-system | Impact | Effort | Phase | Notes |
|----|-----|-------------|-----------|--------|--------|-------|-------|
| G01 | PetDefinition.cs is empty — no fields match pets.json schema | 2/3, 3/3 | Pets | Entire pet feature: no model, no service, no save, no UI | ~3–5 days | RESTORE_4 | PET_DEFINITION_MISSING |
| G02 | AscensionService missing — only bool IsAscended in SaveData | 2/3, 3/3 | Ascension | No cost formula, no stat multiplier calc, no multi-tier | ~1 day | RESTORE_4 | ASCENSION_SERVICE_MISSING |

## 🔴 PROGRESSION_BLOCKER

| ID | Gap | Audit Source | Sub-system | Impact | Effort | Phase | Notes |
|----|-----|-------------|-----------|--------|--------|-------|-------|
| G03 | PromotionSystem — no service, no save field (PromotionTier), no UI | 2/3, 3/3 | Promotion | No tier progression, no stat scaling | ~2–3 days | RESTORE_4 | PROMOTION_MISSING |
| G04 | DoctrineScreen — no UI screen, no prefab, no UIScreenId entry | 3/3 | Doctrine | Player cannot view 8 doctrine levels/progress | ~0.5 day | RESTORE_4 | DOCTRINE_UI_MISSING |

## 🟡 STRUCTURAL

| ID | Gap | Audit Source | Sub-system | Impact | Effort | Phase | Notes |
|----|-----|-------------|-----------|--------|--------|-------|-------|
| G05 | Dungeon chain unlock — no CanStartDungeon() validation | 2/3 | Dungeon | Chain dungeons can be entered without clearing prev | ~0.5 day | RESTORE_2 | UNLOCK_GATE_MISSING |
| G06 | Quest production callers — unclear which callers exist for 56 quests | 2/3 | Quest | Some quest conditions may never fire | Analyze | RESTORE_2 | QUEST_CALLER_UNVERIFIED |
| G07 | Active-state restoration — no exact mid-dungeon restore | 2/3 | Save/Dungeon | Dungeon state may reset on restart | ~0.5 day | RESTORE_5 | STATE_RESTORE_UNVERIFIED |
| G08 | Raid backend/UI — no Raid screen or RaidService | 3/3 | Raid | Raid feature entirely missing | ~1–2 days | RESTORE_2 | RAID_MISSING |

## 🟡 USABILITY

| ID | Gap | Audit Source | Sub-system | Impact | Effort | Phase | Notes |
|----|-----|-------------|-----------|--------|--------|-------|-------|
| G09 | Dungeon auto-tick runs on Update() — no throttling | 2/3 | Dungeon | Dungeon completes in seconds | ~0.25 day | RESTORE_1 | TICK_RATE_UNVERIFIED |
| G10 | Craft timer — no real-time progress bar | 3/3 | Craft | Player can't see craft remaining time | ~0.25 day | RESTORE_3 | CRAFT_PROGRESS_MISSING |
| G11 | Market refresh timer — no countdown | 3/3 | Merchant | Player can't see when market refreshes | ~0.25 day | RESTORE_3 | MARKET_TIMER_MISSING |
| G12 | Loading screen — none present | 3/3 | Bootstrap | White screen on boot | ~0.25 day | RESTORE_5 | LOADING_SCREEN_MISSING |

## 🟢 LEGACY_OR_RESERVED

| ID | Gap | Audit Source | Sub-system | Impact | Effort | Phase | Notes |
|----|-----|-------------|-----------|--------|--------|-------|-------|
| G13 | LevelShelter — orphan field, no runtime usage | 1/3 | SaveData | Data bloat only | None | RESTORE_4 | DECIDE REUSE/MIGRATE/DEPRECATE |
| G14 | UpgradeShelter — orphan field, no runtime usage | 1/3 | SaveData | Data bloat only | None | RESTORE_4 | DECIDE REUSE/MIGRATE/DEPRECATE |
| G15 | LevelShelterAutofeed — orphan field, no storage usage | 1/3 | SaveData | Data bloat only | None | RESTORE_4 | DECIDE REUSE/MIGRATE/DEPRECATE |

## 🟡 DESIGN_INTEGRATION

| ID | Gap | Audit Source | Sub-system | Impact | Effort | Phase | Notes |
|----|-----|-------------|-----------|--------|--------|-------|-------|
| G16 | Doctrine reward pipeline — Quest→Doctrine integration exists but UI can't display result | 2/3, 3/3 | Quest/Doctrine | Player can claim quest but not see doctrine progress | ~0.25 day | RESTORE_4 | Combined with G04 |
| G17 | Equip dangling reference guard — RemoveItem doesn't clear WeaponInstanceId/ArmorInstanceId/AccessoryInstanceId | 2/3 | Equipment | Deleting equipped item creates dead reference | ~0.25 day | RESTORE_1 | EQUIP_DANGLING_REF |
| G18 | Unlock configuration — no unlock gates/gated content system | 3/3 | Progression | Can't control what features are locked | ~0.5 day | RESTORE_2 | UNLOCK_CONFIG_MISSING |

---

## Gap Summary

| Category | Count | IDs |
|----------|-------|-----|
| 🔴 FEATURE_BLOCKER | 2 | G01, G02 |
| 🔴 PROGRESSION_BLOCKER | 2 | G03, G04 |
| 🟡 STRUCTURAL | 4 | G05, G06, G07, G08 |
| 🟡 USABILITY | 4 | G09, G10, G11, G12 |
| 🟢 LEGACY_OR_RESERVED | 3 | G13, G14, G15 |
| 🟡 DESIGN_INTEGRATION | 3 | G16, G17, G18 |
| **Total** | **18** | |

---

## Reclassification from Original

| Gap | Original Priority | Corrected Priority | Rationale |
|-----|------------------|-------------------|-----------|
| Pets | "Block playtest" core | FEATURE_BLOCKER — RESTORE_4 | Not in core loop path |
| Promotion | "Block playtest" core | PROGRESSION_BLOCKER — RESTORE_4 | Mid-game feature, not startup-blocking |
| Doctrine UI | "Block playtest" core | PROGRESSION_BLOCKER — RESTORE_4 | Features can work without viewing doctrine tree |
| Ascension | Feature gap | FEATURE_BLOCKER — RESTORE_4 | Part of designed systems phase |
| Shelter fields | "Remove orphan" P0 | LEGACY_OR_RESERVED — PENDING SHELTER DESIGN | Cannot remove without migration plan |
