# 04. Dungeon, Raid & Loot Parity
**Status**: STATIC_AUDIT_COMPLETE_RUNTIME_PENDING

| System | Feature | Expected | Unity current | Status | Unity evidence | Decode/design evidence | Caller/wiring | Player impact | Runtime verification |
|---|---|---|---|---|---|---|---|---|---|
| Dungeon | Progression | 250 tick threshold | Present | MATCHES_DECODE | `Scripts\Runtime\Services\DungeonService.cs` | Decode Phase 2 | `GameLoopService.cs` | High | YES |
| Raid | Waves & Boss | Full wave structure | Not implemented | MISSING_IN_UNITY | `Scripts\Definitions\RaidDefinition.cs` (Data only) | Decode Phase 2 | N/A | High | NO |
| Loot | Drop tables | Scale 1000 | Present | MATCHES_DECODE | `Scripts\Runtime\Services\LootService.cs` | Decode Phase 2 | `CombatService.cs` | High | NO |
