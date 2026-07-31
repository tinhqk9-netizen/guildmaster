# SAVE / LOAD SAFETY AUDIT
## Generated: 2026-07-29

---

## 1. Persistence Strategy

| Aspect | Detail |
|--------|--------|
| Format | JSON via `JsonUtility.ToJson()` (Unity engine) |
| Primary file | `Application.persistentDataPath + "/save.json"` |
| Backup file | `Application.persistentDataPath + "/save_backup.json"` |
| Read method | `JsonUtility.FromJson<SaveData>()` |
| Schema version | `SaveMetadata.SaveVersion = 1` |

---

## 2. Write Safety

### Save Flow
```
Save()
  1. Metadata.SaveVersion = 1
  2. Metadata.SaveTimeUnix = Unix timestamp now
  3. Metadata.GameVersion = Application.version
  4. JsonUtility.ToJson(CurrentData, prettyPrint: true)  ← Serialize
  5. File.Copy(save.json → save_backup.json, overwrite)  ← BACKUP FIRST
  6. File.WriteAllText(save.json, json)                   ← THEN WRITE
```

### Safety Guarantees

| Guarantee | Status | Evidence |
|-----------|--------|----------|
| Backup-before-write | ✅ | File.Copy before WriteAllText (SaveService.cs:108-113) |
| Write atomic for FS | ⚠️ | File.WriteAllText is NOT atomic for files >4KB |
| JSON is valid Unity-serializable | ✅ | SaveData is `[Serializable]` with public fields |
| Corrupt save → backup fallback | ✅ | Load() tries backup on primary failure |
| Both corrupt → fresh default | ✅ | CreateDefault() → NormalizeAfterLoad() |
| Save every action or only on quit? | ⚠️ | SaveService only saves on demand (Save() called from service mutations) |

### Danger: Partial Write
If Unity crashes mid-WriteAllText, save.json is truncated. But backup.json still has the previous valid save. Load() will:
1. Read save.json → JSON error → catch
2. Read save_backup.json → SUCCESS → load from backup
3. User loses only the last mutation, not the entire save.

**Verdict:** ✅ Acceptable safety for MVP.

---

## 3. Load Safety

### Load Flow
```
Load()
  1. HasSaveFile(save.json)?
     → YES: ReadAllText → FromJson<SaveData> → check null → NormalizeAfterLoad()
       → SUCCESS: CurrentData = loaded, return true
       → CATCH: try backup
     → NO: CreateDefault() → NormalizeAfterLoad() → return true
  2. Backup load:
     → ReadAllText(save_backup.json) → FromJson → NormalizeAfterLoad()
       → SUCCESS: return true
       → CATCH: CreateDefault() → return false
```

### Security Analysis

| Risk | Mitigation | Status |
|------|-----------|--------|
| Corrupt save.json | Backup fallback | ✅ |
| Both files corrupt | Fresh default | ✅ |
| Null refs on deserialize | NormalizeAfterLoad guards null lists | ✅ (14 list guards) |
| Missing fields (old save) | C# defaults (0/false/null) | ✅ |
| Invalid DefinitionId | ❌ No validation | **C6 — MISSING** |
| Empty save (new game) | Currently Money=0 | **C8 — MISSING** |
| SaveVersion mismatch | ❌ No migration | **C5 — MISSING** |

---

## 4. JsonUtility Serialization Analysis

### Rule: JsonUtility reads fields, NOT properties

SaveData uses public fields throughout. This is CORRECT.

Example of correct pattern:
```csharp
[Serializable]
public class SaveData
{
    public long Money;          // ✅ Field — serialized
    public List<ItemSaveData> Items = new List<ItemSaveData>();  // ✅ Field with default
}
```

Example of WRONG pattern (would break save):
```csharp
[Serializable]
public class SaveData
{
    public long Money { get; set; }  // ❌ Property — NOT serialized by JsonUtility
}
```

**All SaveData types checked:**
- ✅ SaveData — all public fields
- ✅ CharacterSaveData — all public fields
- ✅ ItemSaveData — all public fields
- ✅ QuestSaveData — all public fields
- ✅ ItemActionSaveData — all public fields
- ✅ ActiveDungeonSaveData — all public fields
- ✅ MerchantOfferSaveData — all public fields
- ✅ StatusEffectSaveData — all public fields
- ✅ SkillSaveData — all public fields
- ✅ DungeonSaveData — all public fields
- ✅ SaveMetadata — all public fields
- ✅ CombatEncounterSaveData — all public fields

**Verdict: ALL SaveData types use public fields — JsonUtility-safe.**

---

## 5. NormalizeAfterLoad Guard Coverage

### List Guards (14 total)
All reference-type lists are guarded with `if (x == null) x = new List<T>()`
- 14 list guards confirmed per SaveData.cs:277-295

### Missing: `ActiveDungeon` guard
- `ActiveDungeonSaveData ActiveDungeon = null` — NOT guarded in NAL
- This is intentional: null = "no active dungeon". Not a bug.

### Character-level Guards (4 per character)
- PositiveStatusEffects, NegativeStatusEffects, PotionsDrank, Trait

---

## 6. Save Timing Hazards

| Hazard | Impact | Mitigation |
|--------|--------|------------|
| Save during combat | ActiveDungeon partially written | ✅ SaveDungeonState() serializes full state |
| Save during craft | Craft timer state | ✅ ItemActionSaveData has CompletionTimeUnix |
| Save during merchant sell | Market listing timer | ✅ ItemActionSaveData has CompletionTimeUnix |
| Save during system clock change | Offline delta wrong | ⚠️ CalculateOfflineDeltaSeconds has clamp? Needs verification |
| Rapid save spam (every update) | Perf overhead | ⚠️ Save() called on each service mutation, not throttled |

---

## 7. Overall Safety Score

| Category | Score | Notes |
|----------|-------|-------|
| Write atomicity | 🟡 7/10 | Backup-before-write but not truly atomic |
| Load resilience | 🟢 9/10 | Triple fallback, null guards |
| Data integrity | 🟡 6/10 | No checksum, no DefinitionId validation |
| Migration support | 🔴 2/10 | Empty placeholder |
| Schema correctness | 🟢 9/10 | All public fields, JsonUtility-safe |
| **Overall** | **🟡 6.6/10** | **Functional but 3 critical gaps** |
