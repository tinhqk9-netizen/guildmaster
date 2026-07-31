# RESTORE_0 — FOUNDATION

**Goal:** Baseline verification. Confirm boot → service wiring → save/load → data deserialization all work. **No implementation, no code changes, only read/confirm/map.**

**Effort:** ~1 day (read + map)
**Risk:** 🔴 HIGH — if boot or save/load is broken, nothing else works
**Gate:** compile PASS, data deserialization PASS, service wiring full map, save/load cycle PASS

---

## Tasks

### T0-0: Exhaustive Foundation Map

Before touching anything, produce a complete inventory:

**ServiceContainer.cs** — annotate EVERY line of `Initialize()`:
```
Line 12: container.Register<ISaveService, SaveService>()     → SaveService.cs verified
Line 15: container.Register<ICharacterService, CharacterService>() → CharService.cs verified
... (all 19 services)
```

**Bootstrapper.cs** — map exact call order:
```
1. ServiceContainer.Initialize()
2. GameDatabase.LoadAll()
3. SaveService.LoadGame()
4. UIService.Initialize()
5. PlayMode checks if offline progress needed
```

**NormalizeAfterLoad()** — list ALL null guards:
```
- Metadata → null check + new SaveMetadata()
- Items → null → new List<ItemSaveData>()
- Characters → null → new List<CharacterSaveData>()
- Dungeons → null → new List<DungeonSaveData>()
... (all 15+ guard clauses)
```

**Deliverable:** `RESTORE_0_Map.txt` — full annotated call chain with line numbers.

### T0-1: Verify Data Deserialization

Read `GameDatabase.LoadAll()` to confirm:
- Paths to JSON files (Resources/Data/*.json)
- Fallback behavior if file missing
- Error handling for malformed JSON
- Complete list of DefinitionBase subclasses loaded

### T0-2: Verify Save/Load End-to-End

1. Trace `SaveService.SaveGame()` — confirm windoid WriteFile path
2. Trace `SaveService.LoadGame()` — confirm windoid ReadFile → JsonUtility.FromJson
3. Confirm save file location (Application.persistentDataPath)
4. Confirm backup file (save_backup.json) write behavior
5. Confirm NormalizeAfterLoad() runs AFTER deserialization, BEFORE any service access

### T0-3: Produce Metadata Map

For every field in SaveData.cs:
- What type?
- Is it value type (no null risk) or reference type (needs guard)?
- Is NormalizeAfterLoad() guarding it?
- Does any runtime code read it?
- Does any runtime code write it?

---

## Verification Gate — RESTORE_0 PASS Criteria

| Check | Method | Expected |
|-------|--------|----------|
| Bootstrapper.Start() call chain complete | Static read | All 5 steps present |
| ServiceContainer.Initialize() — 19 services | Static read | 19 Register<> calls |
| GameDatabase.LoadAll() — all JSON loaded | Static read | ALL data files listed |
| SaveService.SaveGame() windoid path | Static read | Correct persistentDataPath |
| SaveService.LoadGame() → NormalizeAfterLoad() | Static read | guards exist for all reference type fields |
| Save/load cycle (mock save file) | NOT_RUN (no Unity) | GATE: must verify in editor |
| Compile (Unity) | NOT_RUN | GATE: 0 errors |
| **Phase exit** | All checks complete | Mapped and documented |
