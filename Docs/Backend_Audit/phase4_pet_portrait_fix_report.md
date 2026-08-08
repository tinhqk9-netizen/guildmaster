# Phase 4 Pet Portrait Fix

## Root cause

The pet save/runtime model stores the canonical definition id (`beetle`, `rockling`, etc.). The legacy sprite catalog stores pet portraits under the `pet_<id>` key (`pet_beetle`, `pet_rockling`, etc.). `DatabaseBuilder.EnrichPetDefinitions()` already populated `PetDefinition.IdImage` with that canonical legacy key, but both pet UI paths called `LegacySpriteRegistry.GetSprite(pet.DefinitionId)` directly. The registry performs exact key lookup and does not infer the `pet_` prefix, so both `ShelterDialog` and `PetDetailPanel` received `null` and displayed the gray fallback.

The Adventurer path does not have this mismatch because it uses `GetUnitSprite()`, which adds the `unit_` prefix before lookup. Enemy rendering follows the same prefixed-key convention.

## Fix

- Added `LegacySpriteRegistry.GetPetSprite(string idOrImageId)`, accepting either the canonical pet id or the already-enriched `pet_<id>` image key.
- Updated `ShelterDialog` to resolve `PetDefinition.IdImage` and use `GetPetSprite()`.
- Updated `PetDetailPanel` to use the identical resolution pipeline.
- No PetService, PetDefinition schema, PetSaveData, hatch logic, tier roll, or save logic was changed.

## Regression coverage

- `PetDefinitions_ResolveLegacyPortraitSprites`: all loaded pet definitions expose `pet_<id>` and resolve to a non-null catalog sprite.
- `Pet_SaveLoadKeepsDefinitionAndPortraitResolution`: creates a pet through `PetService`, serializes/reloads save data, and verifies the persisted definition still resolves to a portrait.

## Verification

- Unity compile: 0 errors; 8 existing warnings only (unused mock-test save events).
- Targeted Phase 4 content tests: 6/6 passed.
- Full EditMode suite: 215/215 passed, 0 failed, 0 skipped.
- Runtime Main scene smoke flow: pet verification generated and saved a random Rockling pet; Shelter displayed the real Rockling sprite; the live `PetDetailOverlay` was observed active with `Icon.m_Sprite = pet_rockling`, name `Rockling`, and populated pet data; the screenshot tool reported `Empty state active=False` and no orphan popup after close.

Screenshots:

- [Shelter with pet portrait](../Legacy_Audit/Asset_Gallery/phase_5e_shelter.png)
- [Pet detail flow](../Legacy_Audit/Asset_Gallery/phase_5e_pet_detail.png)

## Backup

Pre-change backup:

`D:\Tinh\Rebuild_GuildMaster\Backup\Phase4_Pet_Portrait_Fix\`

## Known limitation

No known limitation remains for pet portrait resolution. Pet data fields that are absent from the current legacy `pets.json` remain governed by the existing content-system behavior; this fix does not fabricate them. No gameplay/backend changes were needed.
