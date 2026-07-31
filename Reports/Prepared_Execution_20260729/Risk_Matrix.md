# RISK MATRIX — Non-Functional Risk Assessment
## Generated: 2026-07-29

---

## Risk Scoring

| Score | Label | Definition |
|-------|-------|------------|
| 🔴 5 | CRITICAL | Blocks gameplay entirely. Cannot test or ship without fix. |
| 🟠 4 | HIGH | Major feature broken. Need fix before serious user testing. |
| 🟡 3 | MEDIUM | Feature partially broken. Annoying but not blocking. |
| 🟢 2 | LOW | Cosmetic / nice-to-have. |
| ⚪ 1 | INFO | Not a risk but noteworthy. |

---

## Top Risks

### CRITICAL Risks (Score 5)

| ID | Risk | Phase | Impact | Likelihood | Mitigation |
|----|------|-------|--------|------------|------------|
| R01 | **Quest never progress** (56/56 missing callers) | R2 | Users accept quests that never complete → progress stuck | CERTAIN | Wire callers: Dungeon Complete, Item Collect, Craft Claim, Kill Enemy, Equip Item, Earn Money |
| R02 | **No offline progress applied** (C4) | R0, R5 | All offline progress code (craft, merchant) exists but is never called at startup | CERTAIN | Add OfflineProgressService.ApplyOfflineProgress() call in UIRuntimeBootstrap.Start() |
| R03 | **Save corruption → total data loss** (no checksum) | R0 | WriteAllText not atomic; partial write + corrupted backup = fresh save | LOW | Add CRC checksum after JSON; verify on load |
| R04 | **Definition mismatch crash** (C6) | R0 | Loading save with items whose DefinitionId points to non-existent data | MEDIUM | Add cross-reference check in SaveService.Load() → remove orphaned items |

### HIGH Risks (Score 4)

| ID | Risk | Phase | Impact | Likelihood | Mitigation |
|----|------|-------|--------|------------|------------|
| R05 | **Equip dangling reference** (G17) | R1 | Selling/consuming equipped item → character has broken equipment slot | HIGH | RemoveItem() checks character equipment refs and clears them |
| R06 | **New game has 0 money** (C8) | R0 | Player can't afford recruit, craft, or upgrades | CERTAIN | Set Money=500 in CreateDefault(), add basic items |
| R07 | **Dungeon chain unlocked** (G05) | R2 | All dungeons available from start → progression broken | CERTAIN | Add CanStartDungeon chain check + lock UI |
| R08 | **Tavern visitors not regenerated after full load** | R1, R5 | Offline doesn't progress visitors → tavern stays empty after reload | MEDIUM | Add SaveData.NextTavernVisit + online progress on Boot |

### MEDIUM Risks (Score 3)

| ID | Risk | Phase | Impact | Likelihood | Mitigation |
|----|------|-------|--------|------------|------------|
| R09 | **Dungeon Tick runs full loop instantly** | R1 | No delta-time throttle → dungeon completes in 1 frame | HIGH | Add time slice per Tick, cooldown between actions |
| R10 | **Active dungeon not re-entered after load** | R5 | ActiveDungeon persisted but combat not resumed → data preserved but no gameplay | CERTAIN | Option A: auto-complete with formula. Option C: clear on load. |
| R11 | **No loading screen during boot** (G12) | R5 | Black screen until UI appears → poor UX | CERTAIN | Add LoadingScreen overlay with progress callback |
| R12 | **Craft progress bar missing** (G10) | R3 | No visual on how long until craft completes | CERTAIN | Add progress bar bound to CraftService queue items |
| R13 | **Market refresh timer missing** (G11) | R3 | No countdown for merchant stock refresh | CERTAIN | Add countdown display bound to next refresh time |
| R14 | **Test isolation missing** (C7) | R0 | Tests share persistentDataPath → cross-test contamination | CERTAIN | Add per-test temp filesystem path |

### LOW Risks (Score 2)

| ID | Risk | Phase | Impact | Likelihood | Mitigation |
|----|------|-------|--------|------------|------------|
| R15 | **New game loading screen** | R5 | Fresh install shows blank for 0.5-1s during boot | MEDIUM | Add LoadingScreen that self-dismisses on boot complete |
| R16 | **SaveSettings (Sound/Music) not implemented** | R5 | Audio manager uses fields that don't exist | MEDIUM | Wire Settings save fields to AudioService |
| R17 | **No offline delta cap or clock-change protection** | R5 | System clock went back → huge negative delta | LOW | Add max clamping + clock-back detection |
| R18 | **Dependency graph mismatch** | ALL | T0-0 references dependencies that don't match actual code | LOW | Verified 18/19 services match planned |

---

## Risk Heatmap

```
              LIKELIHOOD
         Low    Med    High   Certain
    ┌─────────────────────────────────
  C │ R03                    R01,R02
  r │                        
  i │ M     R04              R09   R06
  t │ E                            
  i │ D     R07    R17     R14   R10,R11
  c │                             R12,R13
  a │ L                                 
  l │ O     R18    R15,R16 R05   
  i │ W                                 
  t │                                
  y │
```

**Top remediation priority (by risk score × certainty):**
1. R01 (Quest callers) — Score 5 × Certain = **25**
2. R02 (Offline progress not called) — Score 5 × Certain = **25**
3. R06 (New game no money) — Score 4 × Certain = **16**
4. R07 (All dungeons unlocked) — Score 4 × Certain = **16**
5. R10 (Active dungeon not re-entered) — Score 3 × Certain = **15**
6. R11 (No loading screen) — Score 3 × Certain = **15**
7. R05 (Equip dangling ref) — Score 4 × High = **12**

---

## Watchpoints

| Watchpoint | Description |
|-----------|------------|
| W01 | `MigrateSave()` placeholder is empty — will cause data loss on first schema update |
| W02 | `IsAscended` bool cannot be extended to multi-level — must change to int before release |
| W03 | Combat loop confirmed via static trace but needs Unity play test for turn flow correctness |
| W04 | 11 Unity play-test gates in total — every UI screen needs prefab hierarchy verified |
| W05 | All FormulaService methods use original Java formulas — verify Android ↔ Unity formula parity |
