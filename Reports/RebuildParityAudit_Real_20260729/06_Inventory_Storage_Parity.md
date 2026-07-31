# 06. Inventory & Storage Parity
**Status**: STATIC_AUDIT_COMPLETE_RUNTIME_PENDING

| System | Feature | Expected | Unity current | Status | Unity evidence | Decode/design evidence | Caller/wiring | Player impact | Runtime verification |
|---|---|---|---|---|---|---|---|---|---|
| Inventory | Stack limit | Max 9999 | Implemented | MATCHES_DECODE | `Scripts\Runtime\Services\InventoryService.cs` | Decode Phase 2 | `LootService.cs` | High | YES |
| Storage | Safe box | Exists | Missing | MISSING_IN_UNITY | `Scripts\Runtime\Services\InventoryService.cs` | Decode Phase 3 | N/A | Low | NO |
