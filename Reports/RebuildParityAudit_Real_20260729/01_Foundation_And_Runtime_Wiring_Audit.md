# 01. Foundation & Runtime Wiring Audit
**Status**: STATIC_AUDIT_COMPLETE_RUNTIME_PENDING

This report is based on extracting class definitions and exact paths from the Unity project source code, verifying the presence of foundational systems.

## Bootstrap & Service Creation
| System | Feature | Expected | Unity current | Status | Unity evidence | Decode/design evidence | Caller/wiring | Player impact | Runtime verification |
|---|---|---|---|---|---|---|---|---|---|
| Foundation | Bootstrapper | Initialize Core Services | Exists in Scripts | PRESENT_BUT_NOT_WIRED | `Scripts\Runtime\Boot\Bootstrapper.cs` | Decode Audit Phase 1 | `UIRuntimeBootstrap.cs` | High | Required |
| Foundation | Service Locator | Provide dependencies | `ServiceContainer.cs` exists | MATCHES_DECODE | `Scripts\Runtime\Services\ServiceContainer.cs` | Decode Phase 1 | Constructor injection | High | NO |

## Save & Offline
| System | Feature | Expected | Unity current | Status | Unity evidence | Decode/design evidence | Caller/wiring | Player impact | Runtime verification |
|---|---|---|---|---|---|---|---|---|---|
| Foundation | Save/Load | Full atomic JSON save | `SaveData.cs` and `ActiveDungeonSaveData.cs` | PARTIAL_MATCH | `Scripts\Runtime\Save\SaveData.cs` | Decode Phase 3 | `GameLoopRunner.cs` | High | YES |
| Foundation | Offline | TrueTime / Negative time check | `OfflineProgressService.cs` exists | MATCHES_DECODE | `Scripts\Runtime\Services\OfflineProgressService.cs` | Decode Phase 3 | `GameLoopService.cs` | High | YES |

## Event & Tick
| System | Feature | Expected | Unity current | Status | Unity evidence | Decode/design evidence | Caller/wiring | Player impact | Runtime verification |
|---|---|---|---|---|---|---|---|---|---|
| Foundation | Game Loop | Tick events for logic | `GameLoopRunner.cs`, `IGameLoopService.cs` | MATCHES_DECODE | `Scripts\Runtime\Core\GameLoopRunner.cs` | Decode Phase 1 | Unity `Update()` | High | YES |

## Tests
| System | Feature | Expected | Unity current | Status | Unity evidence | Decode/design evidence | Caller/wiring | Player impact | Runtime verification |
|---|---|---|---|---|---|---|---|---|---|
| Tests | EditMode Baseline | 82 tests, 5 fail | TESTS_NOT_RUN | TESTS_NOT_RUN | `Scripts\Tests\EditMode\*.cs` | User baseline | N/A | None | YES |
| Tests | PlayMode Baseline | 4 pass | TESTS_NOT_RUN | TESTS_NOT_RUN | `Scripts\Tests\PlayMode\*.cs` | User baseline | N/A | None | YES |
| Tests | Total Found | At least 87 attributes | 87 attributes parsed | MATCHES_DECODE | `Scripts\Tests\GuildMasterAutomatedTestsRunner.cs` | User baseline | N/A | None | NO |
