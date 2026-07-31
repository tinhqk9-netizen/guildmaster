# 05. Quest Parity
**Status**: STATIC_AUDIT_COMPLETE_RUNTIME_PENDING

| System | Feature | Expected | Unity current | Status | Unity evidence | Decode/design evidence | Caller/wiring | Player impact | Runtime verification |
|---|---|---|---|---|---|---|---|---|---|
| Quest | Definitions | 56 Quest definitions | Present but data missing | DESIGNED_REPLACEMENT_MISSING | `Scripts\Definitions\QuestDefinition.cs` | Decode Phase 2 | N/A | High | NO |
| Quest | Progress tracking | Caller increments | Missing | MISSING_IN_UNITY | `Scripts\Runtime\Services\QuestService.cs` | Decode Phase 2 | N/A | High | NO |
