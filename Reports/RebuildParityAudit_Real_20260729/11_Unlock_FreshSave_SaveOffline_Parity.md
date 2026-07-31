# 11. Unlock, FreshSave & Offline Parity
**Status**: STATIC_AUDIT_COMPLETE_RUNTIME_PENDING

| System | Feature | Expected | Unity current | Status | Unity evidence | Decode/design evidence | Caller/wiring | Player impact | Runtime verification |
|---|---|---|---|---|---|---|---|---|---|
| Fresh Save | Initial Data | Correct starter setup | Basic initialization | PARTIAL_MATCH | `Scripts\Runtime\Save\SaveData.cs` | Decode Phase 3 | `GameLoopRunner.cs` | High | YES |
| Offline | Catch-up | Max 12 hours | Negative check present | MATCHES_DECODE | `Scripts\Runtime\Services\OfflineProgressService.cs` | Decode Phase 3 | `GameLoopService.cs` | High | YES |
