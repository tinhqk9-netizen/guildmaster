# LIMITATIONS

---

## Verification Limitations

| Limitation | Impact |
|-----------|--------|
| **No Unity Editor access** | All verification is static code trace. UI screens, prefab bindings, scene loads, and runtime behavior are NOT verified. Every "NOT_RUN (needs editor)" item is unconfirmed. |
| **No Test Runner** | Zero unit tests, zero PlayMode tests exist. No automated regression. |
| **No runtime save/load test** | Save file integrity, JSON deserialization, and NormalizeAfterLoad behavior cannot be tested without executing Unity. |
| **No combat balance verification** | Damage formulas, HP pools, and drop rates are assumed from code — actual gameplay feel unknown. |
| **No multiplayer network test** | Not applicable (single player), but no async/threading verification done. |
| **No performance profiling** | Memory, frame rate, load times not measured. |
| **No platform-specific testing** | Windows only. No mobile, WebGL, or console verification. |
| **No audio listening** | Sound file paths may exist but actual audio playability unconfirmed. |

---

## Codebase Limitations

| Limitation | Impact |
|-----------|--------|
| **Java source unavailable for comparison** | DECODE_PROVEN and DECODE_INFERRED labels may be incomplete. Design intent may differ from implementation. |
| **PetDefinition.cs is empty** | Cannot confirm pets.json field schema without parsing Java source or guessing from context. |
| **Raid design unknown** | RAID_MISSING gap may have different requirements than assumed. |
| **Promotion design unknown** | Tier count, cost scaling, and stat multipliers are guesses without Java source or design doc. |
| **Shelter fields intent unclear** | Unknown whether OrgSheets→Shelter→Quarters rename was intentional mid-dev or accidental duplication. |
| **Doctrine max level unknown** | 8 doctrines exist but their max level per type is not confirmed (assumed from code defaults). |
| **UI prefab SerializeField bindings** | Cannot confirm that UI screens actually have their text/slider fields linked in the prefab. A screen may "work" in script but show nothing visually. |
| **Localization strings not inspected** | ui_strings.json exists but content and keys unverified. |
| **Scene files not read** | Boot order and screen instantiation partially depend on scene hierarchy. |

---

## Plan Limitations

| Limitation | Impact |
|-----------|--------|
| **Effort estimates are rough** | Based on code complexity, not actual Unity workflow time. Prefab creation, serialization, and scene wiring add time not reflected. |
| **Phase order may change** | If a dependency reveals itself only during execution, the order may need adjustment. |
| **Missing gap discovery** | New gaps may be discovered during execution that are not in the Gap Matrix. These will be documented and escalated. |
| **No Unity asset pipeline considerations** | Asset bundles, addressables, or build settings not examined. |
| **No third-party plugin dependencies verified** | Windoid header format assumed compatible with BinaryFormatter (unverified). |

---

## What This Plan CANNOT Do

- **Cannot guarantee** the game compiles without errors in Unity
- **Cannot guarantee** save files load correctly
- **Cannot guarantee** any UI screen renders correctly
- **Cannot guarantee** combat balance is fun
- **Cannot guarantee** the game runs at 60 FPS
- **Cannot guarantee** all Java design behaviors are preserved
- **Cannot declare** the game "playtest ready"

**What This Plan CAN Do:**
- Provide a comprehensive static audit of every traceable code path
- Document every gap with its severity, impact, and restoration phase
- Provide numbered implementation steps for each missing feature
- Define verification gates with pass/fail criteria
- Track execution status honestly (STATIC_TRACE_CONFIRMED / NOT_RUN / PARTIAL)

---

## Closing

This corrected Master Restoration Plan supersedes the original `MasterConsolidation/` directory. It represents:

- ✅ 6 correctly-scoped phases (RESTORE_0 → RESTORE_5)
- ✅ Honest status labels (no overclaims)
- ✅ Correct gap classification (no false blockers)
- ✅ Conservative backup strategy (no git reset --hard)
- ✅ Explicit limitations (no hidden assumptions)
- ✅ Execution gates (continuous quality controls)
- ❌ No runtime verification (needs Unity Editor)

**Next step:** Begin RESTORE_0 execution when directed.
