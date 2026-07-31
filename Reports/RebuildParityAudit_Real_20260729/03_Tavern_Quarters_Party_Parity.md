# 03. Tavern, Quarters & Party Parity
**Status**: STATIC_AUDIT_COMPLETE_RUNTIME_PENDING

| System | Feature | Expected | Unity current | Status | Unity evidence | Decode/design evidence | Caller/wiring | Player impact | Runtime verification |
|---|---|---|---|---|---|---|---|---|---|
| Tavern | Refresh / Recruitment | Timer generation, Recruit action | Partial implementation | PARTIAL_MATCH | `Scripts\Runtime\Services\TavernService.cs` | Decode Phase 2 | `TavernScreen.cs` | High | YES |
| Quarters | Capacity / Send Away | Unlimited/Limited limits | No strict enforcement | CONTRADICTS_DECODE | `Scripts\Runtime\Services\CharacterService.cs` | Decode Phase 2 | N/A | Low | NO |
| Party | Formation (4 Adv + 1 Pet) | 4 + 1 restrictions | 4 slots enforced, Pets missing | PARTIAL_MATCH | `Scripts\Runtime\Services\DungeonService.cs` | Decode Phase 3 | `DungeonScreen.cs` | High | NO |
