# 07. Workshop & Recipes Parity
**Status**: STATIC_AUDIT_COMPLETE_RUNTIME_PENDING

| System | Feature | Expected | Unity current | Status | Unity evidence | Decode/design evidence | Caller/wiring | Player impact | Runtime verification |
|---|---|---|---|---|---|---|---|---|---|
| Workshop | Queuing | Timers, Offline progress | Missing queue loop | PRESENT_BUT_NOT_WIRED | `Scripts\Runtime\Services\CraftService.cs` | Designed Replacement | `CraftScreen.cs` | High | YES |
| Recipes | Exact list | Designed list | Missing | DESIGNED_REPLACEMENT_MISSING | `Scripts\Definitions\RecipeDefinition.cs` | Designed Replacement | N/A | High | NO |
