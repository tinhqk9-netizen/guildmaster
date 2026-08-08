# Phase 0 — Data Schema Mapping (Java → JSON → C# Definitions)

**Project:** D:\Tinh\Rebuild_GuildMaster
**Legacy source:** D:\Tinh\Guild Master - Idle Dungeons
**Date:** 2026-08-06
**Scope:** Data schema only. Synthesized from the existing deep-dive audits in
`Docs/Backend_Audit/*.md` (hero_definitions, item_data_parser, skills, traits, doctrine, pets,
dungeon_encounter_data, quest_system) plus a direct read of the current C# Definition classes,
the shipped `Assets/StreamingAssets/GameData/*.json`, `manifest.json`, `DatabaseBuilder.cs`,
`ItemFieldsLoader.cs`, and `SaveData.cs`. No gameplay files were opened for editing purposes —
only read for reference (`TavernService.cs`, `DoctrineService.cs`).

Serialization convention confirmed: `UnityJsonSerializer` calls raw `UnityEngine.JsonUtility`.
JsonUtility only binds **public fields**, never `{ get; set; }` properties, and matching is
**case-sensitive exact name match**. This is the root cause of several "field exists but is
always null" bugs found below, independent of missing data.

---

## 1. CharacterDefinition (`AdventurerDefinition.cs`)

Java ground truth: `Adventurer.java` + 129 `instances/*.java` subclasses, `configureStatistics()`.

| Field | Java | C# before | Status | Action |
|---|---|---|---|---|
| Base stats (Hp/Con/Int/Dex/Def/MDef) | yes | `BaseMaxHp` etc. (fields) | OK | none |
| `nameKey` (`R.string.*`) | yes | **missing** | 129/129 lost | Add `nameKey` to `DefinitionBase` (shared — see §8) |
| `idDescription` | yes | **missing** | 129/129 lost | Same as above (`descriptionKey`) |
| `imageId` / portrait | yes | **missing** | 129/129 lost | Add `ImageId` field |
| `weaponType` / `armorType` | yes | present (`WeaponType`/`ArmorType`) | OK, populated by `DatabaseBuilder.EnrichAdventurerDefinition` (hand-mapped, not parsed) | none — enrichment is out of Phase 0 scope |
| `activeSkill` / `passiveSkill` (`Skills.*` enum) | yes | present (`ActiveSkill`/`PassiveSkill`) | schema OK, values never populated by parser (parser skips `Skills.*` refs) | schema unchanged; parser fix is out of scope (needs a real Java enum-reference parser, tracked as gap) |
| `nextClasses` (List, `.add()`) | yes, `List<String>` | present (`NextClasses string[]`) | schema OK, values never populated (parser doesn't handle `.add()` calls) | schema unchanged; parser fix is a gap (see §9) |
| `potionDrinkerType` | yes, enum | present only as `ManualRuleRequired_PotionDrinkerType` (never treated as a real field) | Misclassified as "manual rule" | Add real `PotionDrinkerType` field; keep the old field name untouched (unreferenced elsewhere) to avoid any risk |
| `MaxLevel` | yes | present (`MaxLevel`) | OK | none |

**Affected files:** `AdventurerDefinition.cs` only for Phase 0. `DatabaseBuilder.cs`'s
`EnrichAdventurerDefinition` already owns WeaponType/ArmorType — not touched. Populating
NameKey/DescriptionKey/ImageId/NextClasses/Skills requires a real parser upgrade (regex → AST) —
explicitly out of scope per Step 3 ("just make sure schema can carry the data").

---

## 2. ItemDefinition (`ItemDefinition.cs`)

Java ground truth: `Item.java` + 607 `instances/*.java`. JSON: `items.json` (`data[]`, each with
top-level `price`, `rarity`, `nameKey`, `iconKey`, plus a nested `fields{}` dict of stat/meta
overrides such as `idImage`, `idName`, `idDescription`).

| Field | Java/JSON | C# before | Status | Action |
|---|---|---|---|---|
| `price` (lowercase, top-level) | yes | `Price` (PascalCase field) | **JsonUtility can't bind `price`→`Price`** (case-sensitive) → always 0 | `ItemFieldsLoader` already bypasses JsonUtility for the `fields{}` dict — extend it to also read top-level `price`/`rarity`/`notSellable` and assign directly to the correctly-cased C# fields |
| `rarity` (string enum, e.g. `"COMMON"`) | yes | `Rarity` (`int`) | type mismatch + casing | Add `RarityId` (`string`, raw value) alongside existing `Rarity` (`int`, kept for existing UI/Quest consumers — populated via best-effort ordinal mapping) |
| `notSellable` | yes | `NotSellable` (`bool`) | casing mismatch → always false | same fix as `price` |
| `fields.idImage` (per-item custom sprite, 11 items) | yes | **missing** | 11/11 lost | Add `IdImage` (`string`), populated by `ItemFieldsLoader` |
| Stat fields (Constitution/Dexterity/...) | yes, nested in `fields{}` | present, loaded by `ItemFieldsLoader` | OK | none |

**Affected files:** `ItemDefinition.cs` (new `IdImage`, `RarityId`), `ItemFieldsLoader.cs`
(extend `ItemDto`/`Apply` to read `price`, `rarity`, `notSellable`, `fields.idImage`).
`DatabaseBuilder.EnrichItemDefinition` (Category/ItemType mapping) is untouched.

Not fixed in Phase 0 (documented as a separate, already-known bug in `item_data_parser_audit.md`,
not a schema issue): `MerchantService.SellItem` signature and `InventoryService.ConsumeByDefinitionId`
lock bypass — both are gameplay-service logic, out of scope.

---

## 3. SkillDefinition (`SkillDefinition.cs`)

Java ground truth: `Skills.java` — a pure enum (227 entries), no combat data. All skill logic is
hardcoded in giant `switch` blocks in `Area.java`/`Entity.java`. `skills.json` therefore only ever
carries id/name/description keys — there is no static combat schema in Legacy to restore.

| Field | Java | C# before | Status | Action |
|---|---|---|---|---|
| `NameKey` | yes (`R.string.*`) | declared as `{ get; set; }` **property** | JsonUtility never binds properties → always null, independent of parser quality | Convert to public field |
| `DescriptionKey` | yes | same as above | same bug | Convert to public field |

No new combat fields added — Skill behavior is legitimately hardcoded in Java with no static
data to port (confirmed by `skills_audit.md` §2). Adding fields like Cooldown/Cost/DamageFormula
would be fabrication; the existing `// manualRuleRequired: ... deferredToS3Combat` comment is kept.

**Affected files:** `SkillDefinition.cs` only.

---

## 4. TraitDefinition (new — did not exist)

Java ground truth: `Trait.java` enum (20 hero traits: 3 common + 3 premium-common + 14 rare) and
`PetAbility.java` enum (13 pet abilities). No `TraitDefinition.cs` class and no `traits.json` /
`pet_abilities.json` currently exist in this project.

Per the audit, `CharacterRuntime`/`SaveData` currently collapse `traitCommon` + `traitRare` (two
independent Java fields) into a single `Trait` string, and 2 non-Legacy traits (`STOUT`,
`KEEN_EYED`) were hallucinated into `CharacterService.GetTraitMultiplier` (a gameplay file, not
touched here — flagged for a later cleanup task).

**Action:** create `TraitDefinition.cs` with just enough metadata for later phases:
`Category` (`CommonTrait` / `CommonTraitPremium` / `RareTrait` / `PetAbility`), `NameKey`,
`DescriptionKey`. No stat-modifier values are invented — Java hardcodes those in
`Adventurer.java`/`Area.java`, same situation as Skills. No JSON source file exists yet to
populate this from; the class exists so a future phase can hand-author `traits.json` from
`traits_audit.md` §1 without another schema change.

---

## 5. DoctrineDefinition (new — did not exist)

Java ground truth: abstract `Doctrine.java` + 8 concrete subclasses (Affliction, Control,
Fortitude, Grace, Illusion, Knowledge, Ruin, War), each with exactly **6 nodes** (`l1`..`l6`)
drawn from the 40-entry `DoctrineAbilityType.java` enum (own `maxLevel`/`cost`/`increasePerLevel`
per node).

Current C# has no `DoctrineDefinition` class at all — `DoctrineService.cs` hardcodes the 8
doctrine names in string switches, and `SaveData.cs` stores exactly **one summed `Level`/`Progress`
pair per doctrine** (`WarLevel`, `AfflictionLevel`, ...), which cannot represent 6 independent
nodes (confirmed in `doctrine_audit.md` — this is called out explicitly as the most severe
structural loss in the whole audit set).

**Action:** create `DoctrineDefinition.cs` with `DoctrineNodeDefinition { NodeId, AbilityType,
MaxLevel, Cost, IncreasePerLevel }` and `DoctrineDefinition : DefinitionBase { List<DoctrineNodeDefinition> Nodes }`.
No `doctrines.json` exists yet — same as Traits, this is a schema-only restore; the 8×6 catalog is
already hand-cataloged in `Reports/Decode_Final_Systems_Deep_Audit_20260729/05_Doctrine_Exact_Catalog.md`
for whoever authors the JSON in a later phase.

SaveData-side per-node storage is handled in §10 below (`DoctrineNodes`), independent of this
Definition schema.

---

## 6. PetDefinition (`PetDefinition.cs`)

Java ground truth: 7 abstract pet families (Avian/Construct/Esoteric/Insect/Reptile/Wild/Wooden) ×
3 tiers each (Common 75% / Uncommon 20% / Rare 5%) = 21 pets. **Pets have no HP/Attack/Defense and
no evolution system in Java** — they are Aura/Modifier objects assigned to a Dungeon `Area`, not
combat units, and their 13 `PetAbility` unlock at level 1/20/40/60.

Current `pets.json` only carries `id`/`className` (parser failure — `printPetType`,
`guaranteedFirstAbility`, `idName`, `idImage`, `abilityNumber` all missing). The current
`PetDefinition.cs` invented `BaseAttack`, `BaseDefense`, `BaseMaxHp`, `BaseSpeed`, `*Multiplier`,
`EvolutionDefinitionId`, `EvolutionLevel` — confirmed fabricated, no Java equivalent
(`pets_audit.md` §2, §6).

**Action, respecting the "restore Legacy schema only, do not add fabricated fields" instruction:**
- Add the real Legacy fields: `PetFamily` (string — Avian/Construct/...), `PetTier` (int, 1/2/3),
  `IdName`, `IdImage`, `GuaranteedFirstAbility` (PetAbility id), `AbilityNumber` (int).
- **Do not add any new fabricated field.** The existing fabricated fields
  (`BaseAttack`/`BaseDefense`/`BaseMaxHp`/`BaseSpeed`/multipliers/`EvolutionDefinitionId`/
  `EvolutionLevel`) are **left in place, unchanged** — removing them would break `PetService.cs`
  compilation (gameplay logic, explicitly out of scope for this task), and this task adds
  clarifying comments marking them as legacy-incorrect/kept only for compile compatibility. A
  future Pet-system rebuild task should delete them once `PetService.cs`/`PetRuntime.cs` are
  rewritten against the correct "Aura assigned to a Dungeon" architecture.

No `pets.json` values exist for the new fields yet (parser gap, same category as Traits/Doctrine)
— schema is ready to receive them once a future parser pass extracts them.

---

## 7. DungeonDefinition / RaidDefinition (`DungeonDefinition.cs`, `RaidDefinition.cs`)

Java ground truth: no JSON/data-table for encounters at all. Each Area/Raid subclass hardcodes
`rollEnemies()` — a weighted random table of 0-6 monster groups plus an "empty room" chance, and
some raids/dungeons special-case a guaranteed boss group by `event.getKey()`.

Current parser targets `listEnemies()` (a UI helper, not the roll table) producing a **flat
`EnemyIds: string[]`** with all weight/grouping/empty-room/boss information lost. `RaidDefinition`
doesn't even carry that — `raids.json` has no `EnemyIds` at all (`dungeon_encounter_data_audit.md`
§3, §6). Runtime (`DungeonService.RollEnemies`) currently picks exactly 1 random enemy from the
flat list — a gameplay bug, but out of scope to fix here.

**Action (schema only, no `rollEnemies()` port — that's Phase 1+ work):**
- `DungeonDefinition`: add `List<EncounterGroupData> EncounterGroups` (`EnemyIds` + `Weight` per
  group) and `double EmptyRoomWeight`, alongside the existing flat `EnemyIds` (kept for
  backward-compat with whatever currently reads it).
- `RaidDefinition`: add `List<string> EnemyIds` (currently completely absent), plus the same
  `EncounterGroups`/`EmptyRoomWeight` fields as Dungeon, for parity.

No parser currently populates `EncounterGroups` (extracting `rollEnemies()` requires an AST-level
Java parser the DecodeConverter doesn't have — tracked as a gap, not fixed here per the task's
explicit "you do NOT need to build a fully complete encounter/combat data pipeline" allowance).
`EnemyIds` keeps working exactly as before for any existing caller.

---

## 8. QuestDefinition (`QuestDefinition.cs`)

Java ground truth: `Quest.java`/`QuestsManager.java`, 36 quests created via `createInstance()`,
split into two **pools** (Kings Quests → Gems; Doctrine Quests → per-doctrine XP, keyed by which
doctrine an adventurer belongs to). Rarity is rolled at generation time and scales both target
progress and reward.

Two JSON files exist, neither fully wired:
- `quests.json` (56 records, manifest-loaded as category `quests`) — only `id`/`className`, no
  metadata.
- `quest_metadata.json` (**not referenced in `manifest.json` at all — never loaded today**) — has
  real `defaultRarity` and a 10-entry `targetProgressValues` array (per-rarity target) for a
  larger set of quests.

Current `QuestDefinition.cs` has `TargetProgress` and `TrueClass` declared as `{ get; set; }`
**properties** — JsonUtility never binds these, so they're always `0`/`null` regardless of data
availability (confirmed by an existing PlayMode test comment: *"quest.Definition.TargetProgress is
the JSON field and is often 0 in data files"*). Grep confirms these two members are only read in
two PlayMode tests (not part of the EditMode validation gate) — safe to change shape.

**Action:**
- Convert `TargetProgress`/`TrueClass` to public fields (bug fix, same names/types — no caller
  changes needed).
- Add `DefaultRarity` (`int`) and `TargetProgressValues` (`long[]`), sourced from
  `quest_metadata.json` via a new `QuestMetadataLoader` (same pattern as `ItemFieldsLoader`),
  wired into `DatabaseBuilder`'s `quests` category loader. `quest_metadata.json` is read directly
  through `IGameDataProvider` (it doesn't need a manifest entry — same technique already used for
  `EnemyDropTableLoader` reading extra info out of `enemies.json`'s raw text).
- Add `PoolType` (`string`: `"Kings"`/`"Doctrine"`) and `DoctrineId` (`string`) as **schema-only**
  fields — no JSON source currently encodes pool membership (it's hardcoded in
  `QuestsManager.java`'s field lists, e.g. `afflictionQuests`), so these stay null until a future
  parser pass or hand-authored mapping fills them in. This is what Phase 1's quest-reward rebuild
  needs to stop guessing Kings-vs-Doctrine from `Rarity >= 4` (the hallucination documented in
  `quest_system_audit.md` §4).

**Affected files:** `QuestDefinition.cs`, new `QuestMetadataLoader.cs`, `DatabaseBuilder.cs`
(one extra call in the `quests` category branch, no behavior change for the other categories).

---

## 9. Shared: `DefinitionBase.cs`

`nameKey` and `iconKey` appear at the **top level** of `items.json`, `adventurers.json`, and
`dungeons.json` alike (always `null` in current data, but same key name/casing across every
category), yet no `DefinitionBase` field exists to receive them. Rather than duplicate a `nameKey`
field on every Definition subclass, add both to `DefinitionBase` (lowercase, matching JSON exactly
— same convention already used for `id`/`className`/`parentClass`).

---

## 10. SaveData (Step 4 preview — see completion report for the applied diff)

- `CharacterSaveData.Trait` (single string) cannot hold Java's independent `traitCommon` +
  `traitRare` (confirmed by `TavernService.RollCommonTrait()`/`RollRareTrait()` — two separate
  roll tables, and `traits_audit.md` confirming Java's `Adventurer` really has two fields).
  Add `TraitCommon`/`TraitRare` fields; keep `Trait` for back-compat and migrate old saves in
  `NormalizeAfterLoad()`.
- Doctrine progression is one `Level`/`Progress` pair per doctrine in `SaveData` today — cannot
  hold the 6-node structure from §5. Add `List<DoctrineNodeSaveData> DoctrineNodes` (id-keyed list,
  since `JsonUtility` cannot serialize `Dictionary`), independent of the existing 8 Level/Progress
  pairs (left untouched — `DoctrineService.cs` is gameplay logic, not touched in Phase 0).
- No Bestiary "seen enemy" tracking exists anywhere in `SaveData`. Add
  `List<string> SeenEnemyIds`.

---

## Summary of Definition-class edits (Step 2)

| Class | File | Change |
|---|---|---|
| `DefinitionBase` | `Definitions/DefinitionBase.cs` | + `nameKey`, `iconKey` |
| `AdventurerDefinition` | `Definitions/AdventurerDefinition.cs` | + `ImageId`, `PotionDrinkerType` |
| `ItemDefinition` | `Definitions/ItemDefinition.cs` | + `IdImage`, `RarityId` |
| `SkillDefinition` | `Definitions/SkillDefinition.cs` | `NameKey`/`DescriptionKey` property → field |
| `TraitDefinition` | `Definitions/TraitDefinition.cs` (new) | new class |
| `DoctrineDefinition` | `Definitions/DoctrineDefinition.cs` (new) | new class + `DoctrineNodeDefinition` |
| `PetDefinition` | `Definitions/PetDefinition.cs` | + `PetFamily`, `PetTier`, `IdName`, `IdImage`, `GuaranteedFirstAbility`, `AbilityNumber`; fabricated fields commented, not removed |
| `DungeonDefinition` | `Definitions/DungeonDefinition.cs` | + `EncounterGroups`, `EmptyRoomWeight` |
| `RaidDefinition` | `Definitions/RaidDefinition.cs` | + `EnemyIds`, `EncounterGroups`, `EmptyRoomWeight` |
| `QuestDefinition` | `Definitions/QuestDefinition.cs` | property → field; + `DefaultRarity`, `TargetProgressValues`, `PoolType`, `DoctrineId` |

## Pipeline files touched (Step 3)

- `Database/ItemFieldsLoader.cs` — read top-level `price`/`rarity`/`notSellable` and
  `fields.idImage` directly (bypasses the JsonUtility casing bug for those four fields).
- `Database/QuestMetadataLoader.cs` (new) — reads `quest_metadata.json` directly, fills
  `DefaultRarity`/`TargetProgressValues` on already-loaded `QuestDefinition`s.
- `Database/DatabaseBuilder.cs` — one additional call wiring `QuestMetadataLoader` into the
  `quests` category branch; no other category's behavior changes.

## Explicit gaps left for later phases (not fixed here, by design)

1. Java-enum-reference parsing (`Skills.*`, `NextClasses.add()`, `R.string.*`) needs an AST-level
   Java parser upgrade to actually populate NameKey/DescriptionKey/ImageId/NextClasses/ActiveSkill/
   PassiveSkill — schema is ready, parser is not.
2. `rollEnemies()` weighted encounter tables (Dungeon/Raid) — 23 hand-written Java methods, no
   data table exists in Legacy at all; schema slot (`EncounterGroups`) is ready, empty until
   hand-authored or AST-extracted.
3. `traits.json` / `doctrines.json` do not exist yet — `TraitDefinition`/`DoctrineDefinition`
   classes exist with no data source; catalogs are already documented in
   `Reports/Decode_Final_Systems_Deep_Audit_20260729/05_Doctrine_Exact_Catalog.md` and
   `Docs/Backend_Audit/traits_audit.md` for whoever authors the JSON.
4. Quest pool membership (`PoolType`/`DoctrineId`) — no JSON source encodes this; Java hardcodes
   it as field-lists in `QuestsManager.java` (`afflictionQuests`, etc.). Schema slot only.
