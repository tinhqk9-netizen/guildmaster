# Production New Player Starting State

## Scope

This change only adjusts first-time save initialization. Existing gameplay, economy formulas,
tutorial fields, equipment ownership, pets, raids, progression and UI systems remain unchanged.

## Previous root cause

- `SaveData.CreateDefault()` started with `Money = 20`.
- `UIRuntimeBootstrap` detected an empty save by character/visitor counts and created four random
  heroes, temporarily raising quarters capacity.
- The old boot path did not represent the requested single Footman + first non-Footman visitor
  onboarding state.

## Production flow after change

1. `SaveService.Load()` creates `SaveData.CreateDefault()` when no save file exists and reports
   `SaveLoadResult.FreshNewGame`.
2. `UIRuntimeBootstrap` calls `NewPlayerStateInitializer` only for that load result. Existing
   saves, backup-loaded saves and corrupted-save recovery are not re-seeded.
3. The initializer calls `ITavernService.CreateInitialStartingHero()`.
4. `TavernService` generates a Footman visitor and sends it through normal `RecruitGuest()` and
   `CharacterService.RecruitCharacter()` flow. The starter weapon is materialized through the
   existing `AddEquippedItem` ownership path and is hidden from visible inventory.
5. The Footman is added to Party 0 so it is immediately available to expedition/team setup.
6. `ITavernService.GenerateInitialVisitor()` makes one equal-probability roll across Rogue,
   Archer and Apprentice, then uses the normal trait and starter-weapon visitor pipeline.
7. The initialized state is saved immediately. Subsequent Tavern generation uses the unchanged
   normal pool, including Footman.

## Files changed

- `Assets/_Game/Scripts/Runtime/Save/SaveData.cs`
- `Assets/_Game/Scripts/Runtime/Services/ITavernService.cs`
- `Assets/_Game/Scripts/Runtime/Services/TavernService.cs`
- `Assets/_Game/Scripts/Runtime/Boot/UIRuntimeBootstrap.cs`
- `Assets/_Game/Scripts/Runtime/Boot/NewPlayerStateInitializer.cs`
- `Assets/_Game/Scripts/Runtime/Tools/Developer/NewPlayerStateResetter.cs`
- `Assets/_Game/Scripts/Tests/EditMode/ProductionNewPlayerStateTests.cs`

## Expected fresh state

- Gold: `100`
- Owned characters: exactly one `footman`
- Party 0: the starting Footman
- Footman: starter weapon equipped; no visible inventory duplicate
- Tavern visitors: exactly one visitor; class is Rogue, Archer or Apprentice
- Workshop, market, pets, active dungeon and active raid: empty/null
- Existing `TutorialStep` field remains present and is not used to force Tavern generation

## Backup

`D:\Tinh\Rebuild_GuildMaster\Backup\Production_New_Player_Starting_State\`

## Verification

- Unity script recompilation: **0 errors**, 16 existing test-mock unused-event warnings.
- New production starting-state tests: **3/3 passed**.
- Full EditMode suite: **240/240 passed**, 0 failed, 0 skipped.

The APK was not built in this change. A normal APK build will execute this initializer on its
first run because it uses the runtime `SaveService`/`Application.persistentDataPath` pipeline; an
Editor `save.json` is not packaged into the APK.
