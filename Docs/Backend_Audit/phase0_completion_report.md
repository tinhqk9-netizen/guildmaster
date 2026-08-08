# Phase 0 — Data Foundation Rebuild: Completion Report

**Project:** D:\Tinh\Rebuild_GuildMaster
**Date:** 2026-08-06
**Scope:** Data schema only (Definition classes, data pipeline, SaveData shape). No gameplay
logic, no UI, no CombatService/DungeonService/Quest-reward changes.

Full field-by-field audit is in `Docs/Backend_Audit/phase0_schema_mapping.md` (written before any
code change, per Step 1). This report covers what was actually applied.

---

## Files changed

**Backed up to `Backup/Phase0_DataFoundation/...` before editing:**
`Definitions/DefinitionBase.cs`, `Definitions/AdventurerDefinition.cs`, `Definitions/ItemDefinition.cs`,
`Definitions/SkillDefinition.cs`, `Definitions/QuestDefinition.cs`, `Definitions/PetDefinition.cs`,
`Definitions/DungeonDefinition.cs`, `Definitions/RaidDefinition.cs`, `Definitions/EnemyDefinition.cs`,
`Database/ItemFieldsLoader.cs`, `Database/DatabaseBuilder.cs`, `Runtime/Save/SaveData.cs`.

**New files (no backup needed):**
`Definitions/TraitDefinition.cs`, `Definitions/DoctrineDefinition.cs`, `Database/QuestMetadataLoader.cs`.

---

## Definition schema changes (Step 2)

**`DefinitionBase.cs`** — added `nameKey`, `iconKey` (shared across every category's JSON top
level; previously had no field to land in).

**`AdventurerDefinition.cs`** — added `ImageId` (string), `PotionDrinkerType` (string, a real
simple field — the old `ManualRuleRequired_PotionDrinkerType` was left in place unused so nothing
else needs to change).

**`ItemDefinition.cs`** — added `RarityId` (string, raw Java rarity value) and `IdImage` (string,
per-item custom sprite override). Existing `Price`/`Rarity`/`NotSellable` fields kept as-is
(type/name unchanged, so no caller breaks) but are now actually populated (see pipeline section).

**`SkillDefinition.cs`** — `NameKey`/`DescriptionKey` converted from `{ get; set; }` properties to
public fields. This was a pure bug fix: `UnityJsonSerializer` calls raw `JsonUtility.FromJson`,
which never binds properties, so these were always `null` regardless of parser quality.

**`QuestDefinition.cs`** — `TargetProgress`/`TrueClass` converted from properties to fields (same
property-binding bug as Skills; verified only 2 PlayMode tests reference them, both compatible
with a field of the same name/type). Added `DefaultRarity` (int), `TargetProgressValues` (long[]),
`PoolType` (string, schema-only — no data source yet), `DoctrineId` (string, schema-only).

**`PetDefinition.cs`** — added the real Legacy fields: `PetFamily`, `PetTier`, `IdName`, `IdImage`,
`GuaranteedFirstAbility`, `AbilityNumber`. Per the explicit instruction, **no new fabricated field
was added.** The pre-existing fabricated fields (`BaseAttack`/`BaseDefense`/`BaseMaxHp`/`BaseSpeed`/
`*Multiplier`/`EvolutionDefinitionId`/`EvolutionLevel`) were **left in place, only commented** as
legacy-incorrect — removing them would break `PetService.cs` (gameplay logic, out of scope) and
the constraint is "do not add," not "must remove what's already there."

**`DungeonDefinition.cs`** / **`RaidDefinition.cs`** — added `EncounterGroups` (new
`EncounterGroupData { EnemyIds, Weight }`) and `EmptyRoomWeight` to both, matching Java's
`rollEnemies()` shape (weighted groups + empty-room chance). `RaidDefinition` also gained
`EnemyIds`, which was completely absent before. The old flat `EnemyIds` list on `DungeonDefinition`
is kept for backward compatibility with whatever currently reads it.

**`TraitDefinition.cs` (new)** — `Category` (enum: CommonTrait/CommonTraitPremium/RareTrait/
PetAbility), `NameKey`, `DescriptionKey`. Did not exist before Phase 0.

**`DoctrineDefinition.cs` (new)** — `Nodes: List<DoctrineNodeDefinition>`, each node carrying
`NodeId`, `AbilityType`, `MaxLevel`, `Cost`, `IncreasePerLevel`. Did not exist before Phase 0.

**`EnemyDefinition.cs`** — removed its own duplicate `nameKey` field (now inherited from
`DefinitionBase`); this was a one-line cleanup to kill a `CS0108` hide-inherited-member warning
introduced by the `DefinitionBase` change, not a functional change.

---

## Data pipeline fixes (Step 3)

**`Database/ItemFieldsLoader.cs`** — extended to read `price` (long), `rarity` (string),
`notSellable` (bool) from the top level of each `items.json` record, and `fields.idImage` (string)
from the nested fields dict — all previously silently dropped because JsonUtility's case-sensitive
binding never matches `price`→`Price` etc. Values are now assigned directly to `ItemDefinition`'s
correctly-cased fields, plus a best-effort `RarityId`→`Rarity` (int) mapping
(`COMMON`=0, `UNCOMMON`=1, `RARE`=2, `EPIC`=3, `LEGENDARY`=4; only `COMMON` appears in the current
data sample, other tiers mapped defensively for when fuller data lands).

**`Database/QuestMetadataLoader.cs` (new)** — reads `quest_metadata.json` directly through
`IGameDataProvider` (same technique as `ItemFieldsLoader`/`EnemyDropTableLoader`) and fills
`DefaultRarity`/`TargetProgressValues` onto the already-loaded `QuestDefinition` list.
`quest_metadata.json` was not referenced in `manifest.json` at all before this change and was
therefore never loaded by anything.

**`Database/DatabaseBuilder.cs`** — one added branch in the generic `LoadCategory<T>` method,
gated on `typeof(T) == typeof(QuestDefinition)`, calling `QuestMetadataLoader.Apply`. No other
category's load path was touched.

### Explicit pipeline gaps left for later phases
1. Java-enum-reference parsing (`Skills.*`, `NextClasses.add()`, `R.string.*`/`R.drawable.*`)
   needs an AST-level Java parser upgrade — the current regex-based converter can't extract these.
   Affects `AdventurerDefinition.NameKey/DescriptionKey/ImageId/NextClasses/ActiveSkill/PassiveSkill`
   and most `nameKey`/`iconKey` fields project-wide. Schema is ready; parser is not.
2. `rollEnemies()` weighted encounter tables — 23 hand-written Java methods with no data-table
   equivalent in Legacy at all. `EncounterGroups` is a ready, empty schema slot; populating it
   needs either an AST parser or hand-authored data, tracked as a Phase 1+ task.
3. `traits.json` / `doctrines.json` do not exist. `TraitDefinition`/`DoctrineDefinition` classes
   are ready with no data source. Catalogs are already hand-documented in
   `Reports/Decode_Final_Systems_Deep_Audit_20260729/05_Doctrine_Exact_Catalog.md` and
   `Docs/Backend_Audit/traits_audit.md`.
4. Quest `PoolType`/`DoctrineId` — no JSON source encodes pool membership; Java hardcodes it as
   field-lists in `QuestsManager.java`. Schema slot only.

---

## SaveData structures added (Step 4)

**`CharacterSaveData`** — added `TraitCommon`, `TraitRare` (both `string`, default empty). The old
`Trait` field is kept, and `NormalizeAfterLoad()` → `NormalizeCharacter()` migrates it on load: if
both new fields are empty and `Trait` is set, the value is routed into `TraitCommon` if it matches
one of the six known common-trait ids (`BOOKWORM`/`BRUTE`/`FERAL` + `_PLUS` variants), otherwise
into `TraitRare`. This is lossless because `TavernService`'s existing roll logic only ever
assigns `Trait` from exactly one of `RollCommonTrait()`/`RollRareTrait()`, never both. Neither
`TavernService.cs` nor any character-creation logic was modified.

**`SaveData`** — added `DoctrineNodes: List<DoctrineNodeSaveData>` (new
`DoctrineNodeSaveData { DoctrineId, NodeId, Level }`, an id-keyed list since JsonUtility can't
serialize `Dictionary`, matching the existing `MerchantOfferSaveData` pattern). The existing
per-doctrine `Level`/`Progress` pairs (`WarLevel`, `AfflictionLevel`, ...) were left untouched —
`DoctrineService.cs` reads/writes them and is gameplay logic, out of scope. `DoctrineNodes` is a
save-data slot only; nothing reads or writes it yet.

**`SaveData`** — added `SeenEnemyIds: List<string>` for future Bestiary discovery tracking. No
discovery logic wired up.

**`NormalizeAfterLoad()`** — extended with null-checks for `DoctrineNodes` and `SeenEnemyIds`,
following the exact same pattern already used for every other list field in that method, so older
saves load these as empty collections rather than null.

---

## What was deliberately left undone (and why)

- **Pet combat stats / evolution fields not removed** — `BaseAttack`/`BaseDefense`/`BaseMaxHp`/
  `BaseSpeed`/multipliers/`EvolutionDefinitionId`/`EvolutionLevel` on `PetDefinition` are confirmed
  fabricated (no Java equivalent — pets are Dungeon-assigned auras, not combat units). They were
  not deleted because `PetService.cs` (gameplay logic) depends on them and deleting them would
  require rewriting that service, which is explicitly out of scope for this task. Flagged in code
  comments for a future Pet-system rebuild.
- **`traits.json` / `doctrines.json` not authored** — the new `TraitDefinition`/`DoctrineDefinition`
  classes have no data file to load from yet. Both catalogs (20 traits + 13 pet abilities, 8
  doctrines × 6 nodes) are already fully cataloged in existing audit docs; authoring the actual
  JSON is a data-entry task for a later phase, not a schema task.
- **`rollEnemies()` weighted tables not extracted** — genuinely requires either an AST-level Java
  parser (the current converter is regex-based) or manual transcription of 23 Java methods. The
  `EncounterGroups` schema slot exists and the current flat `EnemyIds` still works exactly as
  before, so no phase-1 caller is broken by this gap.
- **Quest pool (`PoolType`/`DoctrineId`) not populated** — same reasoning; Java encodes this as
  field-lists inside `QuestsManager.java`, not a resolvable JSON key.
- **`MerchantService.SellItem`/`InventoryService.ConsumeByDefinitionId` bugs from
  `item_data_parser_audit.md` §3 not fixed** — both are gameplay-service logic bugs (wrong
  instance-vs-definition id usage, missing `IsLocked` check), unrelated to data schema, explicitly
  out of scope for this task.

---

## Dependencies / expectations for Phase 1

1. **Parser upgrade is the real unlock.** Almost every "missing" field in this audit (hero
   name/skills/promotions, item images, pet metadata) is schema-complete now but empty because the
   Python `DecodeConverter` uses regex, not an AST. Phase 1 (or an earlier dedicated task) should
   upgrade `Tools/DecodeConverter/src/java_parser.py` to resolve `R.string.*`/`R.drawable.*`
   references, `Enum.CONSTANT` assignments, and `list.add(...)` calls before any hero/skill/promotion
   gameplay work can use real data instead of placeholders.
2. **Doctrine and Trait data must be hand-authored or AST-extracted** before `DoctrineService.cs`/
   `CharacterService.GetTraitMultiplier` can be rebuilt against real per-node/per-trait data instead
   of hardcoded switches. The catalogs are documented; only the JSON is missing.
3. **`DoctrineNodes`/`TraitCommon`+`TraitRare`/`SeenEnemyIds` are inert save-data slots.** Phase 1
   (or a dedicated Doctrine/Trait/Bestiary task) must wire `DoctrineService.cs`,
   `TavernService.cs`, and a new Bestiary service against them — none of that logic was touched
   here.
4. **`EncounterGroups` on Dungeon/Raid is empty** until Java's `rollEnemies()` is ported (AST parse
   or manual transcription). `DungeonService.RollEnemies` should keep using the flat `EnemyIds`
   until then; switching to `EncounterGroups` is a Phase 1+ task, not Phase 0.
5. **Pet system needs a full rebuild, not incremental fixes.** `PetDefinition` now carries the
   correct Legacy-shaped fields alongside the fabricated ones. A future task should: (a) author
   `pets.json` with the new fields, (b) rewrite `PetService.cs`/`PetRuntime.cs` around "Pet assigned
   to a Dungeon Area" instead of "Pet equipped to a Hero", and (c) delete the fabricated stat/
   evolution fields once nothing references them.

---

## Validation

- **Compile:** clean after each checkpoint. Final recompile: **0 errors, 0 warnings**
  (`mcp__mcp-unity__recompile_scripts`).
- **EditMode tests:** **171/171 passed, 0 failed, 0 skipped** (`mcp__mcp-unity__run_tests`,
  `testMode: EditMode`) — matches the required baseline exactly, no regressions.
