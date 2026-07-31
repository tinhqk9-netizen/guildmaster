# CONTINUOUS EXECUTION GATES

---

## Gate Declaration

**I, the execution agent, acknowledge:**

1. **I am executing, not designing.** I follow the exact steps in each phase plan. I do not invent new gaps, add features, or make design decisions unless explicitly asked.
2. **I do not declare READY_FOR_USER_PLAYTEST.** The user declares this. I present evidence. The user judges.
3. **I do not skip verification.** Every gate item in the phase plan must be addressed. If I cannot verify (needs Unity editor), I mark NOT_RUN and say why.
4. **I do not downgrade severity labels.** If the plan says a gap is FEATURE_BLOCKER, I treat it as such. I don't decide it's "actually OK" during execution.
5. **I do not change SaveData field names or types** without explicit direction from the plan or the user.
6. **I do not delete SaveData fields** — they become LEGACY_OR_RESERVED until RESTORE_4 makes a decision.
7. **I document every status change.** When a task goes from STATIC_TRACE_CONFIRMED to PARTIAL to TEST_VERIFIED, I update the register.
8. **I record my confidence honestly.** If I only did a static read, the status is STATIC_TRACE_CONFIRMED. Not "verified", not "working."
9. **I do not `git reset --hard`.** File-level backup only.

---

## Pre-Execution Checklists

### Before Each Phase

```
□ Phase plan read completely
□ Dependencies satisfied (previous phase complete)
□ Git commit of current state done
□ Backup created in _backups/<phase>/<timestamp>/
□ SHA256 manifest verified
□ Save file backed up
```

### After Each Phase

```
□ Compile check (Unity or static)
□ All gate items marked with actual status
□ Evidence saved (screenshots, logs, notes)
□ Save/load cycle tested (if possible)
□ Regression: previous phase flows still work (if possible)
□ Status register updated
□ Backup checkpoint created
□ Git commit + tag
```

---

## If a Task Fails

**Failure = compile error, runtime crash, or gate item FAIL.**

1. **Stop.** Do not continue to next task.
2. **Document.** What failed, how, error message, stack trace.
3. **Rollback if needed.** Use file-level backup.
4. **Report.** Tell the user what happened, propose fix.
5. **Wait for direction.** Do not self-authorize a fix unless the plan clearly covers it.

---

## Evidence Requirements

| Evidence Type | Required For | Format |
|--------------|-------------|--------|
| Static trace | ALL tasks | Code map (file:line → reference) |
| Compile result | ALL phases | Unity console copy or "no Unity" note |
| UI verification | UI tasks | Screenshot or "not run — needs Unity" |
| Save/load cycle | SaveData changes | JSON diff or "not run" |
| Gate checklist | Phase completion | Gate table with actual/pass/notes |

---

## Status Register Update Convention

```
[YYYY-MM-DD HH:MM] RESTORE_X — Task ABC
  Status: STATIC_TRACE_CONFIRMED → PARTIAL
  Reason: Found that SaveData field X lacks NormalizeAfterLoad guard
  Evidence: SaveData.cs:182
  Action: Add null guard in implementation step
```

---

## Hard Rules

| Rule | Why |
|------|-----|
| No `git reset --hard` | Destroys all uncommitted work permanently |
| No auto-declare playtest-ready | User must examine evidence and decide |
| No field deletion without migration plan | Orphan fields are safer than broken saves |
| No unreported partial status | Hiding incompleteness leads to false confidence |
| No skipping gate items | Each gate catches a class of failure |
| No inventing scope | Stick to the plan, don't add features |
