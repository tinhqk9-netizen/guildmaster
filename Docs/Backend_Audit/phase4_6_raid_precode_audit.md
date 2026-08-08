# Phase 4.6 — Raid Legacy Audit Before Code

Date: 2026-08-07

## Evidence audited

- Legacy Java raid classes:
  `D:\Tinh\Guild Master - Idle Dungeons\sources\it\paranoidsquirrels\idleguildmaster\storage\data\places\raids\`
- Legacy event state:
  `D:\Tinh\Guild Master - Idle Dungeons\sources\it\paranoidsquirrels\idleguildmaster\storage\data\places\Event.java`
- Legacy raid UI:
  `...\ui\raids\RaidsFragment.java`
- Unity raid data and implementation:
  `Assets/StreamingAssets/GameData/raids.json`
  `Assets/_Game/Scripts/Database/RaidContentCatalog.cs`
  `Assets/_Game/Scripts/Runtime/Services/RaidService.cs`
  `Assets/_Game/Scripts/Runtime/Models/RaidRuntime.cs`
  `Assets/_Game/Scripts/Runtime/UI/Raid/RaidsTabController.cs`

## Current Unity gap

The existing Unity `RaidService` starts an in-memory `RaidRuntime`, loads a static `Rooms` list, resolves combat and rewards, then drops the runtime on reload. It has no persisted active raid, event key/progress, event outcome, pending-room state, boss side-effect callback, or enemy-state persistence. `RaidsTabController` can start/fight/collect the current generic flow, but does not expose event/boss state from a persisted runtime.

## Legacy mapping

Unlock values below are from `Assets/StreamingAssets/GameData/raids.json`. Encounter and event values are from each named Java class's `rollEnemies()` and `triggerEvent()` methods. `Room progress` is the Java `Area.progress`, not merely the ordinal position in the current Unity room list.

| Raid ID | Unlock | Java encounter / boss evidence | Event or side effect evidence | Current Unity status |
|---|---|---|---|---|
| `ancient_grave_digging` | `eternal_battlefield >= 150` | Encounters at progress 3, 4, 6, 8, 9, 11; boss `KabarTheRotten` with `Necrolith` | `kill_KabarTheRotten` sets every `Necrolith` HP to 0, increments `andStayDead`, unlocks Necromancer achievement | Static room data exists; kill callback/persistence missing |
| `celestial_mothership` | `barren_wastelands >= 180` | Progress 2, 3, 4, 5, 6, 8, 9, 12, 15; progress 17 `LegateHadrian` unless unique drop already exists | `kill_ReinforcedDoor` kills all `Gcss`; boss callback records `Gcss` and `ReinforcedDoor` as seen | Static room data exists; special kill callback/persistence missing |
| `divine_archeology` | `the_desert >= 150` | Progress 2, 4, 5, 6, 9 `ShaKireFirstSwordsman`; progress 12 `ShaTheHiddenGod` only after event key 1 | At progress 12, living-party CON >= 200 opens `PYRAMID_DOOR_OPEN`; otherwise raid terminates. Unique-drop gates are checked in Java | Current catalog loses progress/event gate |
| `imperial_rescue` | `the_golden_city >= 150` | Progress 1, 2, 3, 6, 7, 9, 11; progress 14 `EmperorClovisXXVIII` unless `SkeletonKey` unique drop exists | `kill_EmperorClovisXXVIII` unlocks Rescue Team achievement | Static room data exists; callback/persistence missing |
| `kaunis` | no dungeon gate in JSON | Progress 1, 6, 9, 10, 11, 12, 16; boss group at 16: `ChiefScientistAva`, `KingAino`, `FirstMinisterAtos` | Killing any council member unlocks Council achievement | Current catalog has incomplete order and no callback |
| `sleeping_planet` | no dungeon gate in JSON | Progress 5, 8, 10, 12; boss `Singularity` at 14 | `kill_Singularity` unlocks Unity achievement | Static room data exists; callback/persistence missing |
| `the_cultist_rebels` | `frostbite_peaks >= 150` | Event-driven: Halls exploration produces empty/LesserTitan/5x Crusader; event progress 14 gives `Claris` + `Thorvus`; event key 2 progress 2 gives `PrimordialTitan` | `HALLS_EXPLORATION`, optional `HALLS_SKELETON_DOOR` when party has Skeleton Key; random event outcomes and progress; kill callbacks increment quests/achievements | Current static rooms cannot represent event-driven sequence |
| `the_dire_descent` | `lost_lands >= 100` | Progress 5 gives `HeraldXavi`, `HeraldMaya`, `HeraldShoran` unless `SerpentLunge` already obtained | Progress 6 unlocks Core achievement; progress 8 terminates | Static room data exists; unique-drop gate/callback missing |
| `the_dreadful_ascent` | `obsidian_mines >= 100` | Progress 2, 3, 4, 5, 8; progress 10 `KasimirTheSeer`; progress 11 `HeraldKali` unless `SerpentStaff` already obtained | `kill_HeraldKali` unlocks Seer achievement | Static room data exists; progress ordering/callback missing |
| `the_lost_expedition` | `obsidian_mines >= 220` | Normal progress 2, 4, 8, 9, 10, 14; event progress 5 and 7 add Lost Miners plus `TekeliLiFirstApostle` | Room 11 has 20% `LOST_EXPEDITION_TRAPDOOR`; event progress 1 applies 40 damage to living party, progress advances to 8; kill callbacks unlock quest/achievement | Current static rooms cannot represent event state or fall damage |
| `the_slime_pond` | `enchanted_forest >= 150` | Progress 2–4 rolls `progress + 1` slimes with Java weights 69.5/10/10/10/0.5%; progress 6 `SlimeKing` | `kill_SlimeKing` increments Regicide and unlocks Royal Pudding achievement | Current catalog loses weighted encounter and callback |
| `the_tower` | no dungeon gate in JSON | Bosses at progress 8, 12, 16, 22, 26, 31, 35: Lazarus, Phoenix, HeadlessKnight, Ultraslime, TheExiled, TheAncient, TheMachine | At progress 10/14/18/24/28/33, living heroes heal and one dead hero is resurrected; darkness is 50 at progress 31; final kill unlocks Tower achievement | Current catalog has boss list but no heal/resurrection/darkness/event persistence |

## Event model evidence

`Event.java` stores a string-derived event key and integer progress. The source constants include `HALLS_EXPLORATION`, `HALLS_SKELETON_DOOR`, `PYRAMID_DOOR_OPEN`, and `LOST_EXPEDITION_TRAPDOOR`. This is the minimum state that must survive save/load. No choice dialog mechanic was found in the raid classes audited; the Cultist random branches are automatic outcomes, not player choices.

## Reward evidence

The current catalog has explicit unique reward mappings for `celestial_mothership`, `divine_archeology`, `imperial_rescue`, `the_dire_descent`, and `the_dreadful_ascent`. Java additionally guards boss spawning with `Utils.gotUniqueDrop(...)` for the corresponding unique item. Regular enemy drops come from each resolved `EnemyDefinition.DropTable`. No new reward IDs will be fabricated.

## Planned implementation boundary

The restoration will add only source-backed raid state and callbacks:

1. Progress/event state and active raid save/load.
2. Source-backed encounter resolution, including weighted Slime Pond and event-driven Cultist/Lost Expedition branches.
3. Source-backed boss/kill callbacks and Tower/Trapdoor effects where the current Unity models expose the required state.
4. UI binding to real raid runtime state only.
5. Regression tests for all 12 definitions, event branches, boss callbacks, rewards, and reload.

Mechanics that require missing global systems (Android achievement UI, Java logger localization strings, or unique-drop history not represented in the current save model) will be recorded as limitations rather than fabricated.
