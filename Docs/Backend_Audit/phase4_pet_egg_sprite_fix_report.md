# Phase 4 — Pet Egg Sprite Fix

Date: 2026-08-07

## Root cause

The egg slot UI was using the canonical item definition ID directly:

`ShelterDialog.CreateEggSlot()` → `LegacySpriteRegistry.GetSprite(egg.Definition.id)`

The actual data and catalog use different Legacy naming order:

| Item definition ID | Item `IdImage` | Catalog key |
|---|---|---|
| `avian_egg` | `egg_avian` | `egg_avian` |
| `construct_egg` | `egg_construct` | `egg_construct` |
| `esoteric_egg` | `egg_esoteric` | `egg_esoteric` |
| `insect_egg` | `egg_insect` | `egg_insect` |
| `reptile_egg` | `egg_reptile` | `egg_reptile` |
| `wild_egg` | `egg_wild` | `egg_wild` |
| `wooden_egg` | `egg_wooden` | `egg_wooden` |

The imported catalog contains `egg_*` keys, not `*_egg` keys. The registry's underscore-normalized fallback cannot solve a word-order mismatch (`avianegg` is not `eggavian`), so the Shelter icon received `null` and displayed the gray fallback.

Evidence:

- `D:/Tinh/Rebuild_GuildMaster/Assets/StreamingAssets/GameData/items.json`
- `D:/Tinh/Rebuild_GuildMaster/Assets/Resources/LegacySpriteCatalog.asset`
- `D:/Tinh/Guild Master - Idle Dungeons/sources/it/paranoidsquirrels/idleguildmaster/storage/data/items/instances/AvianEgg.java`

## Fix

### Generic resolver

Modified:

- `D:/Tinh/Rebuild_GuildMaster/Assets/_Game/Scripts/Runtime/UI/Legacy/LegacySpriteRegistry.cs`

Added `GetEggSprite(string itemIdOrImageId)`:

- accepts `egg_avian`/`egg_construct` image keys;
- accepts canonical IDs such as `avian_egg` and converts them generically to `egg_avian`;
- falls back to the original key when necessary;
- does not hardcode individual eggs.

### Shelter binding

Modified:

- `D:/Tinh/Rebuild_GuildMaster/Assets/_Game/Scripts/Runtime/UI/Headquarters/ShelterDialog.cs`

Egg slots now use `ItemDefinition.IdImage` first and the canonical-ID resolver fallback second. `Image.sprite` is assigned from the same helper used by the regression test.

No changes were made to PetService, hatch logic, PetDefinition, SaveData, inventory consumption or UI layout.

## Regression tests

Modified:

- `D:/Tinh/Rebuild_GuildMaster/Assets/_Game/Scripts/Tests/EditMode/Phase4_ContentRestorationTests.cs`

Added `PetEggDefinitions_ResolveShelterSpritesThroughGenericResolver`, which:

- loads every hatchable pet egg definition;
- verifies the actual `IdImage` path resolves;
- verifies the canonical `*_egg` fallback resolves;
- calls the same `ShelterDialog.ResolveEggSprite` boundary used before assigning `Image.sprite`.

Results:

- Egg regression: **1/1 passed**.
- Phase 4 suite: **11/11 passed**.
- Full EditMode suite: **220/220 passed**, 0 failed, 0 skipped.
- Unity recompile: **0 compile errors**; 8 existing unused mock-save-event warnings remain outside this fix.

## Backup

`D:/Tinh/Rebuild_GuildMaster/Backup/Phase4_Pet_Egg_Sprite_Fix/`

Backed up before editing:

- `ShelterDialog.cs`
- `LegacySpriteRegistry.cs`
- `Phase4_ContentRestorationTests.cs`

## Runtime expectation

After refreshing the scene or entering Shelter, every hatchable egg slot should receive its `egg_*` Sprite and no longer show the gray missing-sprite fallback. The Hatch button and PetService flow are unchanged.
