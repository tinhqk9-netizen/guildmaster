# 15. Consolidated Parity Matrix
**Status**: STATIC_AUDIT_COMPLETE_RUNTIME_PENDING

| System | Feature/Rule | Source Type | Expected | Unity Current | Status | Dependency | Player Impact | Evidence | Runtime Check | Suggested Restore Phase |
|---|---|---|---|---|---|---|---|---|---|---|
| Core | Basic Combat | DECODE_PROVEN | Turn-based | Implemented | MATCHES_DECODE | None | High | CombatSystem.cs | YES | RESTORE_1_CORE_LOOP |
| Progression | Quests | DECODE_PROVEN | 56 exact | None | MISSING_IN_UNITY | Database | High | No QuestData | NO | RESTORE_2_QUEST_RAID_PROGRESSION |
| Economy | Workshop | DESIGNED_FOR_REBUILD | Timer queues | Not wired | PRESENT_BUT_NOT_WIRED | UI | Medium | Workshop.cs | YES | RESTORE_3_ECONOMY |
| Pets | Shelter | DESIGNED_FOR_REBUILD | Storage | None | DESIGNED_REPLACEMENT_MISSING | Pets | Medium | None | NO | RESTORE_4_DESIGNED_SYSTEMS |
| Save | Save migration | DECODE_INFERRED | Schema v2 | None | MISSING_IN_UNITY | SaveManager | High | SaveManager.cs | YES | RESTORE_5_SAVE_OFFLINE_UI_POLISH |
