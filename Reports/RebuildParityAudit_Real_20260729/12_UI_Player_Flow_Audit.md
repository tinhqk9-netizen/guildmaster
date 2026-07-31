# 12. UI Player Flow Audit
**Status**: STATIC_AUDIT_COMPLETE_RUNTIME_PENDING

| System | Feature | Expected | Unity current | Status | Unity evidence | Decode/design evidence | Caller/wiring | Player impact | Runtime verification |
|---|---|---|---|---|---|---|---|---|---|
| Flow | Tavern -> Recruit | Screen, Button, Backend | Screen exists, backend partial | PARTIAL_MATCH | `Scripts\Runtime\UI\Tavern\TavernScreen.cs` | None | `TavernService.cs` | High | YES |
| Flow | Crafting | Valid queue display | Missing logic | PRESENT_BUT_NOT_PLAYER_USABLE | `Scripts\Runtime\UI\Craft\CraftScreen.cs` | None | `CraftService.cs` | High | YES |
