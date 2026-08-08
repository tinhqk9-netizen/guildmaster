# Phase 4.6 — Raid System Full Legacy Restoration

Date: 2026-08-07  
Project: `D:\Tinh\Rebuild_GuildMaster`

## Scope and backup

The implementation was limited to the Legacy raid backend, persisted raid state, the existing raid detail view, and regression tests. No Phase 5 work, global UI redesign, or new gameplay mechanic was started.

Backup created before code changes:

`D:\Tinh\Rebuild_GuildMaster\Backup\Phase4.6_Raid_Full_Restoration\`

The pre-code source/data mapping is recorded in:

`D:\Tinh\Rebuild_GuildMaster\Docs\Backend_Audit\phase4_6_raid_precode_audit.md`

## Legacy source audited

The audit used the decoded Java files under:

`D:\Tinh\Guild Master - Idle Dungeons\sources\it\paranoidsquirrels\idleguildmaster\storage\data\places\raids\`

and `storage/data/places/Event.java`, including all twelve raid classes:

`AncientGraveDigging`, `CelestialMothership`, `DivineArcheology`, `ImperialRescue`, `Kaunis`, `SleepingPlanet`, `TheCultistRebels`, `TheDireDescent`, `TheDreadfulAscent`, `TheLostExpedition`, `TheSlimePond`, and `TheTower`.

The source evidence was cross-checked against the decoded `raids.json`, `enemies.json`, and `items.json` records. Java source class names are resolved to the canonical Unity IDs at the database boundary; encounter logic does not use a flat random enemy list as its primary path.

## Restored implementation

### Encounter and room progression

- `RaidContentCatalog` now restores the fixed Legacy encounter progress for all twelve raids, including multi-enemy rooms, empty room gaps, and boss rooms.
- Event-driven encounter branches are represented by `LegacyEventKeys` and resolved from persisted event state:
  - `halls_exploration`
  - `halls_skeleton_door`
  - `lost_expedition_trapdoor`
  - `pyramid_door_open`
- Slime Pond uses the Java weights `.695 / .100 / .100 / .100 / .005` for Slime, FireSlime, ElectricSlime, FrozenSlime, and VoidSlime.
- Cultist Rebels restores the Java empty/LesserTitan/five-Crusader event branch and Skeleton Key transition.
- Lost Expedition restores the 20% trapdoor branch, living-party damage, and event encounter sequence.
- Divine Archeology restores the Constitution 200 pyramid gate.
- Tower restores its progress-specific resurrection/heal rooms and darkness 50 at progress 31.

### Boss callbacks and unique drops

- Kabar kills the Necroliths and increments `and_stay_dead` once.
- Reinforced Door kills the Gcss and records the source-backed Bestiary discovery callback.
- Primordial Titan increments `endless_agony`.
- Claris/Thorvus increment `botched_ritual` once after both event targets are dead.
- Avatar of the Ancient increments `eldritch_horror`.
- Slime King increments `regicide`.
- Unique boss rewards remain in the real enemy drop tables. Encounter-level unique gates prevent replaying a boss whose corresponding item is already owned, including both Divine Archeology rewards:
  - Eyes of the Swordsman
  - Divine Zygote
  - Evo 23 Vial
  - Skeleton Key
  - Serpent Lunge
  - Serpent Staff
- Achievement callbacks are not fabricated: the current Unity backend has no AchievementService/save field. These Java achievement unlocks are documented as a known limitation rather than silently pretending to unlock them.

### Persistence

`SaveData.ActiveRaid` now persists:

- raid definition and room/progress position;
- event key, event progress, and event outcome;
- complete/failed state;
- party instance IDs and current HP/mana/shield;
- enemy HP/mana/shield and status effects;
- pending rewards and recent raid log.

Older saves remain compatible because `ActiveRaid` is nullable and `NormalizeAfterLoad()` initializes its nested lists when present. Raid defeat, turn-cap, room transition, event transition, reward collection, and abandon paths update the save state.

### Existing Raid UI integration

`RaidsTabController` keeps the existing legacy-styled hub/detail layout and now shows backend truth for an active run:

- current room/progress;
- boss-room state;
- event key/progress or `EVENT NONE`;
- current darkness;
- pending reward count and persisted event outcome.

Start, Fight, Collect Rewards, Abandon, and Back continue to use `IRaidService`. No fake raid data or second UI state machine was added.

## Files created or modified for Phase 4.6

Created:

- `Assets\_Game\Scripts\Database\RaidContentCatalog.cs`
- `Assets\_Game\Scripts\Runtime\Models\RaidRuntime.cs`
- `Assets\_Game\Scripts\Runtime\Services\RaidService.cs`
- `Assets\_Game\Scripts\Tests\EditMode\Phase4_6_RaidRestorationTests.cs`
- `Assets\_Game\Scripts\Tests\PlayMode\Phase4_6_RaidRuntimeSmokeTests.cs`
- `Docs\Backend_Audit\phase4_6_raid_precode_audit.md`
- `Docs\Backend_Audit\phase4_6_raid_full_restoration_report.md`

Modified:

- `Assets\_Game\Scripts\Definitions\RaidDefinition.cs`
- `Assets\_Game\Scripts\Runtime\Save\SaveData.cs`
- `Assets\_Game\Scripts\Runtime\Services\ServiceContainer.cs`
- `Assets\_Game\Scripts\Runtime\UI\Raid\RaidsTabController.cs`

## Verification

### Compile

Unity script recompilation: **PASS — 0 errors, 0 warnings on the final production/test compile.**

### EditMode

- Phase 4.6 targeted suite: **6/6 passed**.
- Full EditMode suite after final changes: **213/213 passed, 0 failed, 0 skipped**.

The targeted suite verifies:

- all twelve raid IDs and encounter enemy resolution;
- decoded unlock gates;
- regular multi-enemy Kaunis start;
- Cultist event creation and persistence;
- Tower boss spawn at progress 8;
- active raid room/event/party/enemy save-load round-trip;
- encounter-level unique reward mapping, including both Divine rewards.

### PlayMode

A dedicated smoke test was created at:

`Assets\_Game\Scripts\Tests\PlayMode\Phase4_6_RaidRuntimeSmokeTests.cs`

It covers a regular raid, Cultist event raid, and Tower boss raid from the loaded Main scene. It was not executable through the current MCP session because the Unity MCP endpoint returned `ECONNREFUSED 127.0.0.1:8090` on the PlayMode request, while the Unity Editor process remained running. This is a tool-connectivity limitation, not a compile or EditMode failure. The test is ready to run from Unity Test Runner after the MCP endpoint is available.

## Known limitations

1. Java achievement unlock callbacks cannot be persisted because the current Unity project has no AchievementService or achievement save schema. No fabricated achievement state was introduced.
2. Raid event narrative strings are represented as state/log keys and outcomes; localization text parity is outside this backend restoration task.
3. Manual PlayMode/UI smoke confirmation remains pending the Unity Test Runner/MCP transport becoming available.

Phase 4.6 stops here. Phase 5 was not started.
