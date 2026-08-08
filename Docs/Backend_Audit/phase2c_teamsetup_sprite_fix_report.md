# Phase 2C Team Setup Sprite Fix

## Root cause

`Assets/StreamingAssets/GameData/adventurers.json` stores `AdventurerDefinition.ImageId` as a
complete legacy catalog key, for example `unit_footman`.

`LegacySpriteRegistry.GetUnitSprite(...)` accepts the raw character id, for example `footman`,
and adds the `unit_` prefix itself. The previous shared resolver passed `ImageId` directly to
`GetUnitSprite`, producing a lookup for `unit_unit_footman`. Team Setup then received a null
sprite after `AddPortrait` was changed to use that resolver. Adventurers still worked because
its existing path uses `Definition.id` directly.

## Fix

The character sprite resolver now:

1. Uses `LegacySpriteRegistry.GetSprite(ImageId)` when `ImageId` already starts with `unit_`.
2. Uses `LegacySpriteRegistry.GetUnitSprite(ImageId)` when it is a raw id.
3. Falls back to `LegacySpriteRegistry.GetUnitSprite(Definition.id)`.

The same resolver behavior is applied to Team Setup and the existing combat/expedition card
pipeline. No layout, backend, combat logic, or enemy rendering was changed.

## Files modified

- `Assets/_Game/Scripts/Runtime/UI/Dungeon/DungeonsTabController.cs`
- `Assets/_Game/Scripts/Runtime/UI/Dungeon/DungeonScreen.cs`

Backup before editing:

`D:\Tinh\Rebuild_GuildMaster\Backup\Phase2C_TeamSetup_SpriteFix\`

## Verification

- Unity compile: PASS, 0 warnings.
- Full EditMode: `190/190 passed`, `0 failed`, `0 skipped`.
- PlayMode dungeon test: attempted, but MCP Unity returned
  `ECONNREFUSED 127.0.0.1:8090` before execution.

Manual verification when Unity MCP is reachable:

1. Open Dungeons.
2. Open Team Setup and confirm every hero row has a portrait.
3. Start the dungeon.
4. Confirm combat/expedition hero portraits remain visible.
5. Confirm enemy sprites remain unchanged.
