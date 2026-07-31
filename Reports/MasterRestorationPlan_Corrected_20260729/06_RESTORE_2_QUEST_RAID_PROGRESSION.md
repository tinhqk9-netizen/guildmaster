# RESTORE_2 — QUEST RAID PROGRESSION

**Goal:** Quest system verified end-to-end, Raid backend + UI implemented, unlock chain operational.
**Effort:** ~2 days
**Dependencies:** RESTORE_1 PASS (needs core loop for quest triggers)
**Risk:** 🟡 MEDIUM — Raid is new implementation
**Gate:** 56 quests ALL have confirmed callers, Raid entry + combat works, chain unlock gates verified

---

## Tasks

### T2-1: Quest System Verification

1. Confirm `QuestScreen` opens from HUD
2. Trace `QuestScreen.Show()` → `QuestService.GetActiveQuests()`
3. **Critical: Map ALL 56 QuestDefinition IDs to their production callers:**
   - Search for each quest condition type in the codebase
   - Examples:
     - `QUEST_TYPE_DUNGEON_COMPLETE` → where is Dungeon completion checking for quests?
     - `QUEST_TYPE_COLLECT_ITEM` → where is InventoryService.AddItem() checking for quests?
     - `QUEST_TYPE_CRAFT_ITEM` → where is CraftService.ClaimCompletedCraft() calling quest progress?
     - `QUEST_TYPE_KILL_ENEMIES` → where is CombatService tracking kills?
     - etc.
   - For each quest ID (001 → 056): WHO calls its progress? TRACE OR MARK MISSING_CALLER
4. Trace `QuestScreen.ClaimReward()` → `QuestService.ClaimReward()`:
   - Does it call `DoctrineService.AddProgress()`? (G16 link)
   - Does it give gold/items?
   - Does it set quest state to Completed?
5. Confirm quest auto-generation when below minimum threshold (if implemented)
6. Confirm daily quest reset (if implemented)

**Deliverable:** `RESTORE_2_QuestCallerMap.csv` — all 56 quests with caller path or MISSING_CALLER

### T2-2: Quest→Doctrine Integration

1. Confirm `DoctrineService.AddProgress()` exists and is called by Quest reward flow
2. Map all 8 doctrine types (Affliction, Control, Fortitude, Grace, Illusion, Knowledge, Ruin, War) to quest reward types
3. Confirm `SaveData` has doctrine level/progress for all 8 types

### T2-3: Raid Backend + UI (G08 — NEW IMPLEMENTATION)

**Backend (`RaidService.cs`):**
```csharp
public interface IRaidService
{
    IReadOnlyList<RaidDefinition> GetAvailableRaids();
    bool CanStartRaid(string raidId);
    RaidResult StartRaid(string raidId);
    RaidState GetRaidState(string raidId);
    RaidLoot CollectRaidRewards(string raidId);
}

// RaidResult: { Success, PartyDefeated, NotEnoughPartyMembers, RaidOnCooldown }
// RaidDefinition extends DungeonDefinition or is separate
```

**Implementation approach:**
- Reuse `CombatService` for raid combat (raid = multi-wave dungeon)
- Add `RaidSaveData` to `SaveData` (or extend `DungeonSaveData` with raid flag)
- Raid cooldown timer
- Raid party = all characters (not just 4)

**UI (`RaidScreen.cs`):**
- Show available raids with recommended power level
- Raid state: Locked, Ready, InProgress, Completed, OnCooldown
- Raid detail: waves, rewards, party composition
- Collect rewards button

### T2-4: Dungeon Chain Unlock Gate (G05)

**In `DungeonService.CanStartDungeon()`:**

```csharp
public bool CanStartDungeon(string dungeonId)
{
    var def = GameDatabase.Get<DungeonDefinition>(dungeonId);
    if (def == null) return false;
    
    // If this dungeon requires completing a previous one
    if (!string.IsNullOrEmpty(def.RequiresPreviousDungeonId))
    {
        var prevDungeon = saveData.Dungeons
            .FirstOrDefault(d => d.DefinitionId == def.RequiresPreviousDungeonId);
        if (prevDungeon == null || prevDungeon.State != DungeonState.Completed)
            return false;
    }
    
    // Check level requirement
    if (def.RequiredLevel > 0)
    {
        var avgLevel = saveData.Characters
            .Where(c => c.IsInActiveParty)
            .Average(c => c.Level);
        if (avgLevel < def.RequiredLevel)
            return false;
    }
    
    return true;
}
```

**UI:** Lock icon + tooltip on locked dungeons showing unlock condition.

### T2-5: Unlock Configuration (G18 — NEW IMPLEMENTATION)

Create unlock gate system:

```csharp
public interface IUnlockService
{
    bool IsUnlocked(string unlockId);
    void Unlock(string unlockId);
    bool HasUnlockCondition(string unlockId);
    UnlockCondition GetUnlockCondition(string unlockId);
}

// UnlockCondition: { QuestDefinitionId, DungeonDefinitionId, CharacterLevel, TotalDoctrineLevel, ... }
```

Define unlock configuration (example):
```
- RAID → unlocked when 3+ dungeons completed
- ASCENSION → unlocked when character reaches level 50
- PROMOTION → unlocked when character is max level + max ascension
- PET → unlocked when 10+ unique items collected
- DOCTRINE_SCREEN → unlocked at start (always available)
```

---

## Verification Gate — RESTORE_2 PASS Criteria

| Check | Method | Status |
|-------|--------|--------|
| QuestScreen opens from HUD | NOT_RUN (needs editor) | GATE |
| All 56 quests mapped to callers | PRODUCER/CONSUMER MAP | ⬜ |
| Quest → Doctrine reward integration traced | STATIC_TRACE_CONFIRMED | ⬜ |
| ClaimReward → SaveData mutation traced | STATIC_TRACE_CONFIRMED | ⬜ |
| RaidService registered + optional | STATIC_TRACE_CONFIRMED | ⬜ |
| RaidScreen opens + shows raids | NOT_RUN (needs editor) | GATE |
| Raid combat works (reuses CombatService) | STATIC_TRACE_CONFIRMED | ⬜ |
| Dungeon chain unlock gate operational | STATIC_TRACE_CONFIRMED | ⬜ |
| Locked dungeons show lock icon + tooltip | NOT_RUN (needs editor) | GATE |
| UnlockService registered + wired | STATIC_TRACE_CONFIRMED | ⬜ |
| Unlock conditions configurable | CONFIG FILE | ⬜ |
