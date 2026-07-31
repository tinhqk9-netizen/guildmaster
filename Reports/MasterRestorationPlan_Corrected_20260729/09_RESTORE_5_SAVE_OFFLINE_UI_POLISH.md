# RESTORE_5 — SAVE / OFFLINE / UI POLISH

**Goal:** Save migration, offline edge cases, UI polish, comprehensive regression.
**Effort:** ~1–2 days
**Dependencies:** RESTORE_0 through RESTORE_4
**Risk:** 🟡 MEDIUM — save migration can corrupt player data
**Gate:** All save versions loadable, offline delta correct, all screens functional, full regression pass

---

## Tasks

### T5-1: Save Migration

**Problem:** After RESTORE_4, SaveData schema may have changed (AscensionLevel, PromotionTier, Pets List). Old saves must still load.

**Implementation:**
```csharp
// In SaveService or new SaveMigrationService:
public static SaveData MigrateToCurrent(SaveData save)
{
    int fromVersion = save.Metadata.SaveVersion;
    int currentVersion = CURRENT_SAVE_VERSION;
    
    if (fromVersion == currentVersion) return save; // no migration needed
    
    Debug.Log($"Migrating save from v{fromVersion} to v{currentVersion}");
    
    // Chain migrations: v0 → v1 → v2 → ... → current
    while (fromVersion < currentVersion)
    {
        save = MigrateStep(save, fromVersion);
        fromVersion++;
        save.Metadata.SaveVersion = fromVersion;
    }
    
    return save;
}
```

**Migration Steps (example):**
```
v0 → v1:     Add Pets List to SaveData (normalize already handles null→new)
             Copy LevelShelter → LevelQuarters if shelter decision = MIGRATE
             Convert IsAscended bool → AscensionLevel int (true → 1, false → 0)
             Add PromotionTier = 0 to all CharacterSaveData
             Run NormalizeAfterLoad() to fix null lists
```

**Fallback:** If migration fails, log error + load without migration (missing fields handled by NormalizeAfterLoad).

### T5-2: Offline Progress Edge Cases

**Verify/Implement:**
1. `OfflineProgressService.CalculateOfflineDeltaSeconds()` — correct when:
   - First launch (no LastAccess → use 0 delta)
   - Normal reopen (delta = now - LastAccess)
   - System clock changed backwards → clamp to 0
   - Long offline (>30 days) → cap at max (configurable, default 7 days)
2. Offline workshop progress:
   - Crafts in queue progress by delta time
   - Complete crafts at end of queue
3. Offline market progress:
   - Market refreshed N times during offline (based on market refresh period)
   - Sold items accumulate gold
4. Offline tavern progress:
   - Guest list regenerated
   - Visitor time progressed
5. Offline dungeon state:
   - Active dungeon? → auto-complete? or abandon? or pause?

**Edge case tests:**
```csharp
// Test cases for CalculateOfflineDeltaSeconds:
// 1. LastAccess = null → delta = 0
// 2. LastAccess = 1 hour ago → delta ≈ 3600
// 3. LastAccess = 7 days ago → delta = MAX_OFFLINE_SECONDS (cap)
// 4. LastAccess = future (clock went back) → delta = 0 (clamp)
```

### T5-3: Active-State Restoration (G07)

**Verify:** Does restarting the game restore an active dungeon mid-combat?

**If not implemented:**
- Option A: Auto-complete dungeon on reload (simple, somewhat abrupt)
- Option B: Restore exact state (complex, needs serialization of combat state)
- Option C: Abandon active dungeon (safe, player may lose progress)

**Recommendation:** Option A for MVP (auto-complete with reduced loot). Option B for production.

### T5-4: Loading Screen (G12)

**Implement:** Canvas overlay during boot:

```csharp
public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private string[] loadingMessages = {
        "Initializing services...",
        "Loading game data...",
        "Preparing world...",
        "Almost ready..."
    };
    
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        StartCoroutine(ShowProgress());
    }
    
    private IEnumerator ShowProgress()
    {
        // Hook into Bootstrapper progress events
        // or estimate based on boot sequence timing
        // ...
        // Destroy when UIService.MainMenu is shown
    }
}
```

**Integration:** Bootstrapper fires progress events → LoadingScreen updates bar.

### T5-5: Comprehensive Regression (T7)

Run ALL verification flows from Runtime Verification Register:

| ID | Flow | Executed? | Pass? |
|----|------|-----------|-------|
| RF01 | HUD shows Money/Gems | ⬜ | ⬜ |
| RF02 | Inventory screen opens | ⬜ | ⬜ |
| ... | ... all 23 flows | ... | ... |
| RF23 | Settings toggle changes save | ⬜ | ⬜ |
| **Save/Load cycle** | Save → exit → reload → verify | ⬜ | ⬜ |
| **Old save compatibility** | Load pre-RESTORE save | ⬜ | ⬜ |

---

## Verification Gate — RESTORE_5 PASS Criteria

| Check | Method | Status |
|-------|--------|--------|
| Save migration v0→current works | NOT_RUN (needs editor) | GATE |
| Old save loads without errors | NOT_RUN | GATE |
| SaveVersion tracking correct | Code review | ⬜ |
| Offline delta calculation correct (4 edge cases) | Code review | ⬜ |
| Offline workshop progressed | NOT_RUN | GATE |
| Offline market progressed | NOT_RUN | GATE |
| Active dungeon state handled (option A/B/C) | NOT_RUN | GATE |
| Loading screen visible during boot | NOT_RUN | GATE |
| All 23 regression flows pass | NOT_RUN | GATE |
| Save → Exit → Reload → Same state | NOT_RUN (needs editor) | GATE |
| No null refs on fresh start (no save file) | NOT_RUN | GATE |
