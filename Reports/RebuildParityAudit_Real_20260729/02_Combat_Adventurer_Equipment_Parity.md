# 02. Combat, Adventurer & Equipment Parity
**Status**: STATIC_AUDIT_COMPLETE_RUNTIME_PENDING

This report audits the core gameplay loop including Combat, Adventurer stats, and Equipment, referencing exact script paths as evidence.

## Combat & Auto-Combat
| System | Feature | Expected | Unity current | Status | Unity evidence | Decode/design evidence | Caller/wiring | Player impact | Runtime verification |
|---|---|---|---|---|---|---|---|---|---|
| Combat | Auto-Combat Turn Loop | 400 turn cap, deterministic resolution | Partial turn loop implemented | PARTIAL_MATCH | `Scripts\Runtime\Services\CombatService.cs` | Decode Combat Phase 1 | `GameLoopService.cs` tick | High | YES |
| Combat | Skills & Status Effects | Exact behavior mapped | Stubs or partial logic | PARTIAL_MATCH | `Scripts\Runtime\Services\SkillService.cs` | Decode Phase 1 | `CombatService.cs` | High | YES |
| Combat | Target Selection | Aggro / Random / Frontline | Basic target selection | PARTIAL_MATCH | `Scripts\Runtime\Services\TargetSelectionService.cs` | Decode Phase 1 | `CombatService.cs` | High | YES |
| Combat | Damage Math | Complex formulas (DecodeMath) | Implemented formulas | MATCHES_DECODE | `Scripts\Runtime\Formulas\DecodeMath.cs` | Decode Phase 1 | `CombatService.cs` | High | YES |

## Adventurer & Stats
| System | Feature | Expected | Unity current | Status | Unity evidence | Decode/design evidence | Caller/wiring | Player impact | Runtime verification |
|---|---|---|---|---|---|---|---|---|---|
| Adventurer | Base Stats | Fighter/Healer base stats | `CharacterRuntime.cs` holds stats | MATCHES_DECODE | `Scripts\Runtime\Models\CharacterRuntime.cs` | Decode Phase 1 | `CharacterService.cs` | High | NO |
| Adventurer | Level Up & Derived Stats | Formula driven | `FormulaService.cs` | MATCHES_DECODE | `Scripts\Runtime\Formulas\FormulaService.cs` | Decode Phase 1 | `CharacterService.cs` | High | YES |

## Equipment
| System | Feature | Expected | Unity current | Status | Unity evidence | Decode/design evidence | Caller/wiring | Player impact | Runtime verification |
|---|---|---|---|---|---|---|---|---|---|
| Equipment | Slot Restrictions | Main, Offhand, Armor, Acc | Slots enforced | MATCHES_DECODE | `Scripts\Runtime\Services\EquipmentService.cs` | Decode Phase 1 | UI / Inventory | High | NO |
| Equipment | Stat Aggregation | Equipment stats add to base | Included in `CharacterRuntime` | MATCHES_DECODE | `Scripts\Runtime\Services\CharacterService.cs` | Decode Phase 1 | `CharacterService.cs` | High | NO |
