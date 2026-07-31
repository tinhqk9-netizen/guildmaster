# CORRECTED TEST STRATEGY

---

## Core Principle

**No runtime testing infrastructure exists.** All current evidence is static trace only. 
This plan does NOT assume unit tests, PlayMode tests, or CI exist.

Testing is manual, in-editor verification only.

---

## Per-Phase Test Approach

| Phase | Test Method | Environment | Evidence |
|-------|------------|-------------|----------|
| RESTORE_0 | Static trace + manual code review | VS Code / static | Annotated code map |
| RESTORE_1 | Static trace → manual in-editor play-through | Unity Editor | Screenshot/log |
| RESTORE_2 | Static trace → manual in-editor play-through | Unity Editor | Screenshot/log |
| RESTORE_3 | Static trace → manual in-editor play-through | Unity Editor | Screenshot/log |
| RESTORE_4 | Static trace → manual in-editor play-through | Unity Editor | Screenshot/log |
| RESTORE_5 | Static trace → manual in-editor play-through | Unity Editor | Full regression checklist |

---

## Pre-Execution Checklist (every phase)

Before starting any phase implementation:

1. **git commit** current state with descriptive message
2. **Backup** SaveData files from persistentDataPath
3. **Tag** current commit as `pre-<phase-name>-<date>`
4. **Read** the phase plan completely
5. **Verify** dependencies are satisfied

## Post-Execution Checklist (every phase)

After completing phase implementation:

1. **Compile check:** Unity compiles without errors
2. **Save/load cycle:** Save game → exit → reopen → verify
3. **Phase gate checklist:** Run through ALL gate items in the phase plan
4. **Regression:** Ensure previously working flows still work
5. **Update status:** Mark each verification item with actual status
6. **git commit** + tag as `<phase-name>-complete-<date>`

---

## Gate Checklist Template

For each phase gate:

```markdown
### Phase: RESTORE_X — [Name]

| Gate Item | Expected | Actual | Pass? |
|-----------|----------|--------|-------|
| Compile | 0 errors | ___ errors | ___ |
| Save/load | Load old save | ___ | ___ |
| Flow X | Works | ___ | ___ |
| Flow Y | Works | ___ | ___ |
| ... | ... | ... | ... |

**Verdict:** ___ / ___ pass
**Decision:** [PASS / FAIL / PARTIAL]
**Notes:** ___
```

---

## When to Stop and Rollback

| Condition | Action |
|-----------|--------|
| Compile error > 30 min to fix | Stop, rollback file-level |
| Core save/load broken | Stop, restore from backup |
| Phase gate < 80% pass | Document remaining, move to next only if user approves |
| Can't reproduce expected pre-phase behavior | Stop, re-verify environment |
