# 08. Merchant, Market & Shop Parity
**Status**: STATIC_AUDIT_COMPLETE_RUNTIME_PENDING

| System | Feature | Expected | Unity current | Status | Unity evidence | Decode/design evidence | Caller/wiring | Player impact | Runtime verification |
|---|---|---|---|---|---|---|---|---|---|
| Merchant | Restock | Timer-based | No automatic scheduler | PRESENT_BUT_NOT_WIRED | `Scripts\Runtime\Services\MerchantService.cs` | Decode Phase 3 | `MerchantScreen.cs` | High | YES |
| Market | Listings | Listing timer, proceed | Missing completely | MISSING_IN_UNITY | `Scripts\Runtime\Services\MerchantService.cs` | Decode Phase 3 | N/A | High | NO |
