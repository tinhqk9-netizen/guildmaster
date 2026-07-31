# CORRECTED EXECUTIVE SUMMARY
## Guild Master — Master Restoration Plan (Corrected 2026-07-29)

---

### Scope

Plan hợp nhất từ 3 audits (Foundation 1/3, Gameplay 2/3, Designed UI Integration 3/3) với corrections:

1. **6 phase** đúng mapping — không tách Pets/Promotion/Ascension thành phase riêng
2. **Runtime claims downgraded** — STATIC_TRACE_CONFIRMED không phải VERIFIED
3. **Source of truth hierarchy** — DECODE_PROVEN > DECODE_INFERRED > DESIGNED_FOR_REBUILD > Unity code
4. **Exit criteria cứng** — không auto-declare READY_FOR_USER_PLAYTEST

---

### Tổng Quan Gap

| Mức | Số lượng | Loại |
|-----|----------|------|
| 🔴 FEATURE_BLOCKER | 2 | Pet System (no PetDefinition fields, no service, no UI, no save) + Ascension (bool only, full service missing) |
| 🔴 PROGRESSION_BLOCKER | 2 | Promotion System (no service, no save field, no UI) + Doctrine UI (no screen, no prefab) |
| 🟡 STRUCTURAL | 3 | Dungeon chain unlock (no gate validation), Quest production callers (unconfirmed), Active-state restoration (no exact restore of mid-dungeon state) |
| 🟡 USABILITY | 4 | Craft progress timer (no real-time progress bar), Market refresh timer (no countdown), Dungeon tick rate (Update() every frame), Loading screen (none) |
| 🟢 LEGACY_OR_RESERVED | 3 | LevelShelter, UpgradeShelter, LevelShelterAutofeed — pending RESTORE_4 decision |
| 🟡 DESIGN_INTEGRATION | 3 | Doctrine reward pipeline (Quest→Doctrine integration exists but UI missing), Unlock configuration (no unlock gates), Equip dangling reference guard (missing) |
| **Total** | **17** | |

---

### 6 Restoration Phases

| Phase | Name | Effort | Gate |
|-------|------|--------|------|
| RESTORE_0 | Foundation | ~1 day | compile PASS, data deserialization PASS, service wiring PASS, save/load PASS |
| RESTORE_1 | Core Loop | ~2–3 days | Tavern→Combat→Loot→Inventory end-to-end PASS |
| RESTORE_2 | Quest Raid Progression | ~2 days | 56 quests, Doctrine reward, Raid backend, unlock chain PASS |
| RESTORE_3 | Economy | ~1 day | Workshop, Merchant, Market, Shop, timers PASS |
| RESTORE_4 | Designed Systems | ~3–5 days | Pets, Promotion, Ascension, Doctrine UI, Shelter decision PASS |
| RESTORE_5 | Save Offline UI Polish | ~1–2 days | Migration, backup, offline edge cases, regression PASS |

---

### Key Changes from Original

| Item | Original | Corrected |
|------|----------|-----------|
| Phases | 7 (Phase 0-6) | 6 (RESTORE_0 → RESTORE_5) |
| Shelter fields | "Remove orphan" | LEGACY_OR_RESERVED — keep until RESTORE_4 |
| Pets priority | "Block playtest" | FEATURE_BLOCKER — moved to RESTORE_4 |
| Verification status | ✅ VERIFIED (runtime) | STATIC_TRACE_CONFIRMED (static only) |
| Source of truth | Unity code = sole source | DECODE > DESIGN > Unity hierarchy |
| Exit criteria | "Phase 1 = playtest ready" | Specific gate list, no auto-declare |
| Backup | Git reset --hard | File-level backup + SHA256 manifest |

---

**Status:** CORRECTED_PLAN_READY_FOR_SEQUENTIAL_EXECUTION
