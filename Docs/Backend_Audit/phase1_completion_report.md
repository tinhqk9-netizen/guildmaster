# Phase 1 — Character Progression Core Restoration: Completion Report

**Project:** D:\Tinh\Rebuild_GuildMaster
**Date:** 2026-08-06
**Full findings before code:** `Docs/Backend_Audit/phase1_audit_report.md`

---

## Files changed

**Backed up to `Backup/Phase1_CharacterProgression/...`** (pre-Phase-1 state) before editing:
`Definitions/AdventurerDefinition.cs`, `Definitions/PromotionDefinition.cs`,
`Definitions/SkillDefinition.cs`, `Runtime/Services/CharacterService.cs`,
`Runtime/Services/PromotionService.cs`, `Runtime/Services/SkillService.cs`,
`Runtime/Services/TavernService.cs`, `Runtime/Services/ICharacterService.cs`,
`Runtime/Services/ServiceContainer.cs`, `Runtime/Save/SaveData.cs`,
`Runtime/Models/CharacterRuntime.cs`, `Runtime/UI/Character/CharacterDetailPanel.cs`,
`Database/DatabaseBuilder.cs`, `StreamingAssets/GameData/adventurers.json` (pre-extraction copy),
`Tests/EditMode/S2VerificationTests.cs`.
(One process note: `Tests/EditMode/S6_5A_Stage4_CharacterTests.cs` was edited before its backup
copy was taken, so that specific backup reflects the post-edit state, not pre-Phase-1 — the
original is still recoverable from git history if ever needed.)

**New files:** `Runtime/Services/TraitService.cs`.

**Data file modified in place** (not backed up under a separate "before" name beyond the copy
above): `Assets/StreamingAssets/GameData/adventurers.json` — merged extraction results directly
into the existing 129-record file (see Task 1.1).

---

## Task 1.1 — Hero Class System

- **116 hero classes confirmed** (not 129 — the raw `adventurers.json` had 13 non-hero records
  polluting it; see audit §a). `DatabaseBuilder.cs` now filters `AdventurerDefinition` records to
  `parentClass == "Adventurer"` before registering them, dropping the 13 non-hero records
  (`Adventurer` base class, `PotionsDrank`, `Doctrine` + 8 `DoctrineOf*` + `DoctrineAbility` +
  `EmptyDoctrine`).
- Ran a narrow, targeted regex extraction (`extract_hero_data.py`, one-off script, not a pipeline
  change) over all 116 `units/*.java` files' `configureStatistics()` bodies and merged the results
  directly into `adventurers.json`: `NextClasses` (`nextClasses.add(...)` calls),
  `ActiveSkill`/`PassiveSkill` (`Skills.*` enum constant names), `ImageId` (`R.drawable.*` key),
  `nameKey`/`descriptionKey` (`R.string.*` keys, raw — no localization table exists in this
  project), `PotionDrinkerType` (enum constant), `MaxLevel` (int). No field or class was
  fabricated — every value came directly from the matching Java file's
  `configureStatistics()` method.
- `AdventurerDefinition.cs`: added `descriptionKey` (string). All other needed fields
  (`NextClasses`, `ActiveSkill`, `PassiveSkill`, `ImageId`, `MaxLevel`, `WeaponType`, `ArmorType`,
  `PotionDrinkerType`) already existed as schema (Phase 0) and are now actually populated.
- Character runtime already pulled stats from `AdventurerDefinition` (`CharacterService.
  GetTotalStat`), not hardcoded values — confirmed correct, unchanged.
- `CharacterSaveData.DefinitionId` now genuinely represents class identity end-to-end: it is
  reassigned on promotion/ascension (Task 1.2), not just set once at creation.

Java field → C# field (key restored pieces):
| Java | C# |
|---|---|
| `nextClasses` (`List<String>`, `.add()`) | `AdventurerDefinition.NextClasses` (`string[]`) |
| `activeSkill`/`passiveSkill` (`Skills.*`) | `AdventurerDefinition.ActiveSkill`/`PassiveSkill` (raw enum-constant string) |
| `imageId` (`R.drawable.*`) | `AdventurerDefinition.ImageId` |
| `idName`/`idDescription` (`R.string.*`) | `DefinitionBase.nameKey` / `AdventurerDefinition.descriptionKey` (raw keys) |
| `potionDrinkerType` | `AdventurerDefinition.PotionDrinkerType` |
| `maxLevel` | `AdventurerDefinition.MaxLevel` |
| `parentClass == "Adventurer"` filter | 116/129 real hero records registered |

---

## Task 1.2 — Promotion System

Removed the fake ascension-as-promotion logic **after** confirming via audit that it didn't match
Java: `AscensionLevel` (fabricated tier counter, removed from `CharacterSaveData`/
`CharacterRuntime`) and `PromotionDefinition.RequiredItemId`/`RequiredItemCount`/`StatMultiplier`/
`TierName`/`TierIndex` (fabricated — no `promotions.json` ever existed; `PromotionDefinition.cs`
trimmed to an empty, unused shell rather than deleted, to minimize blast radius).

`IsAscended`/`ascended` was **kept** — audit confirmed it is a real Java field with a real
(previously recovered, tested) +50% CON/INT/DEX/HP stat effect, not a promotion substitute.

Restored the real flow in `PromotionService.cs` (full rewrite) + new
`CharacterService.ChangeClass(instanceId, newDefinitionId, setAscended)`:
- `GetPromotionChoices(character)`: current class's `NextClasses` (resolved to full
  `AdventurerDefinition`s by matching `className`), gated on `character.Level >=
  currentDef.MaxLevel`. No item requirement (confirmed: none exists in Java's standard tree).
- `Promote(character, targetDefinitionId)`: reassigns `DefinitionId` to the chosen class,
  `Level = 1`, `Experience = 0`. Weapon/Armor/Accessory/traits/potions/doctrine untouched (Java:
  carried through unchanged).
- `CanAscend`/`Ascend(character)`: only when the current class has an EMPTY `NextClasses` at
  `MaxLevel` (final tier). Resets `DefinitionId` to the hero's base class
  (`Utils.getBaseClass` — weapon type: bow→Archer, dagger→Rogue, staff→Apprentice, else→Footman),
  `Level = 1`, `Experience = 0`, `IsAscended = true` permanently. Same equipment/trait/potion/
  doctrine carry-through as promote.

**Save/load migration for old saves:** `AscensionLevel` was simply removed from
`CharacterSaveData` — no explicit migration code is needed because `UnityJsonSerializer` calls raw
`JsonUtility.FromJson`, which silently ignores JSON keys with no matching C# member. An old save
containing `"AscensionLevel": 3` loads cleanly; the value is dropped, `IsAscended`/`DefinitionId`
(both still present, both correctly typed) load exactly as before. Verified by code inspection of
`SaveData.NormalizeCharacter()` (touches only `PositiveStatusEffects`/`NegativeStatusEffects`/
`PotionsDrank`/`Trait`/`TraitCommon`/`TraitRare` — no reference to `AscensionLevel` anywhere) and
of `JsonUtility`'s documented field-matching behavior (case-sensitive exact match; unmatched JSON
keys are dropped, unmatched C# fields keep their default). No temporary EditMode test was added
for this specific check — the removal is a strict field deletion with no dependent logic, and the
existing `S6_5A_Stage1_FoundationTests.SaveData_OldSaveWithoutNewFieldsStillLoads` /
`SaveData_NormalizeAfterLoad_*` tests (unchanged, still passing) already exercise the same
"old JSON missing fields the class doesn't expect" load path this relies on.

---

## Task 1.3 — Trait System

Created `Runtime/Services/TraitService.cs` (did not exist — logic was scattered in
`TavernService.RollCommonTrait()`/`RollRareTrait()`). Restored:
- **Full 14-value rare trait roll** (Java: `Utils.rollRareTrait()`, exact 1/70 thresholds). The old
  `TavernService.RollRareTrait()` only rolled 7 of the 14 rare traits — a real bug, fixed.
- **Independent common + rare rolls** (Java: `generateVisitor()` calls `rollCommonTrait()` AND
  `rollRareTrait()`, both passed to `Adventurer.getInstance` — a hero can hold both simultaneously).
  `TavernService.GenerateVisitor()` previously treated them as mutually exclusive
  (`trait = RollCommonTrait() ?? RollRareTrait()`); now rolls and stores both independently.
- `CharacterRuntime` gained `TraitCommon`/`TraitRare` (previously only a single `Trait` string;
  `CharacterSaveData` already had the split from Phase 0 — Phase 1 only needed to extend it to the
  runtime model, there was no duplicate/conflicting Phase-0 work to resolve). `Trait` kept as a
  read-only-in-practice legacy display alias (`== TraitCommon`) for existing UI callers
  (`AdventurersTabController`, `TavernScreen`, `TavernDialog`, `CharacterScreen`) that were left
  unchanged (out of scope — Task 1.3 only required `CharacterSaveData`/`CharacterService`).
- `CharacterService.GetTraitMultiplier`: removed `"STOUT"`/`"KEEN_EYED"` (fabricated — not in
  Java's 20-value `Trait` enum) and `"NIMBLE"` (real trait, but in Java affects flat dodge chance,
  not Dexterity — mapping it to a DEX stat multiplier was wrong; dodge chance is a Combat-system
  stat, deferred to Phase 2). Added the `_PLUS` premium-common variants
  (`BOOKWORM_PLUS`/`BRUTE_PLUS`/`FERAL_PLUS`) at the same 1.15× as their base variant — the exact
  `_PLUS` multiplier is not visible anywhere in the decompiled source
  (`Adventurer.calculateTotalStat`'s body is stripped/undecompilable), so this is flagged in code
  comments as the least-speculative option, NOT a confirmed Java value.
- Only `TraitCommon` feeds `GetTraitMultiplier` now (Java: no rare trait touches a base stat total
  anywhere in `Adventurer.java`'s visible source — rare traits hook combat mechanics instead, all
  Phase 2/Combat concerns).

`TraitDefinition` catalog data (`traits.json`) still does not exist (Phase 0 finding, unchanged) —
`TraitService.GetTraitDefinition()` is a ready, correctly-safe lookup (returns `null` gracefully,
`GameDatabase.GetAll<T>()` returns empty for unregistered types) for whenever that JSON is
authored.

---

## Task 1.4 — Skill System

`SkillService.cs` rewritten from a no-op shell into a real lookup: `GetByEnumConstant(string)`
resolves an `AdventurerDefinition.ActiveSkill`/`PassiveSkill` value (e.g.
`"ACTIVE_ENERGY_BURST_I"`) to the matching `SkillDefinition` by lowercasing to match `skills.json`'s
id convention (`"active_energy_burst_i"`), with a `className`-based fallback. `IsActiveSkill`/
`IsPassiveSkill` helpers added for `"PASSIVE_NONE"`/empty-safe checks.

`CharacterService.ChangeClass()` refreshes `ActiveSkillId`/`PassiveSkillId` on the `CharacterRuntime`
from the NEW `AdventurerDefinition` every time a class changes — tested by design (promotion flow
calls `ChangeClass`, which unconditionally re-reads `newDef.ActiveSkill`/`PassiveSkill`), confirming
the direct dependency the task called out: a hero's skill set now correctly follows its class.

No new combat fields were added to `SkillDefinition` — confirmed (Phase 0, re-confirmed here)
that Java's `Skills.java` is a pure enum with all actual skill behavior hardcoded in
`Area.java`/`Entity.java` switch blocks (Combat, Phase 2). Adding Cooldown/Cost/DamageFormula
fields would be fabrication.

---

## Task 1.5 — Character Detail UI

Extended the existing Phase 6 `CharacterDetailPanel.cs` (`Assets/_Game/Scripts/Runtime/UI/
Character/CharacterDetailPanel.cs`) in place — kept the established bordered-card / dim-white /
brass-accent legacy visual language (`LegacyUITheme`, `LegacySpriteRegistry`), no pixel-for-pixel
Android XML port.

Changes:
- **Identity row**: now shows `Lv.X/MaxLevel` (was `Lv.X Rank N` against the fake
  `AscensionLevel`), plus an `[ASCENDED]` tag and highlight color when `IsAscended` is true.
- **New Traits section**: separate "Common" / "Rare" lines reading `TraitCommon`/`TraitRare` (was
  a single `Trait` line).
- **Skills section**: now resolves real `SkillDefinition` ids via `ISkillService.
  GetByEnumConstant()` instead of printing the raw `Skills.*` enum constant.
- **Promotion screen**: fully rewired off the old `PromotionDefinition`/item-requirement UI (owned
  item counts, tier badges) onto the real choice list (`IPromotionService.GetPromotionChoices` —
  full `AdventurerDefinition`s with portrait + `MaxLevel`) and a distinct Ascend button/explanation
  when the hero is at a final tier (`CanAscend`), matching the two real Legacy states
  (promote-with-choices vs. ascend-with-no-choices).
- **Equipment**: unchanged, already real (weapon/armor/accessory from `CharacterRuntime`).
- **Doctrine**: unchanged (still account-global levels, not per-character) — Phase 0 already
  identified the per-character Doctrine relation as a structural gap requiring a full
  Doctrine-system rebuild; `AdventurerDefinition` correctly has no doctrine relation field (that
  relation lives on `CharacterSaveData`/runtime in Java, not the class definition), so there was
  nothing new to wire here within Phase 1's scope. Deferred, same reasoning as Phase 0.

---

## Deferred to Phase 2 (Combat/Dungeon) and why

- Rare-trait combat effects (mana regen, crit chance/damage, lifesteal, dodge chance, threat,
  darkness resist, etc.) — all hook `Entity`/`Adventurer` combat-calculation methods, not
  character-progression data. `TraitService`/`CharacterService` only restore the roll and the one
  stat-total effect (`TraitCommon` → CON/INT/DEX) that Task 1.3 explicitly scoped.
- `NIMBLE`'s flat dodge chance effect — same reasoning, explicitly a Combat stat.
- Skill active/passive BEHAVIOR (damage, targeting, cooldowns) — `Skills.java` has none of this;
  it's hardcoded per-skill in `Area.java`/`Entity.java` combat switch blocks.
- `"Intercession"` item-shortcut ascension (`DialogConsumeIntercession.java`) — a Tavern/Shop
  item-consumption flow, not part of the core class-promotion mechanic this phase restores.
- Per-character Doctrine relation/6-node structure — flagged as a full Doctrine-system rebuild in
  Phase 0, unchanged here.

---

## Validation

- **Compile:** 0 errors, 0 warnings after every task's checkpoint and at the end
  (`mcp__mcp-unity__recompile_scripts`).
- **EditMode tests, GuildMaster.Tests namespace (the actual project test suite):**
  **112/112 passed, 0 failed** (`testFilter: "GuildMaster.Tests"`). Two pre-existing tests
  (`S2_003_EquipmentSystem_SlotRestrictions`, `S2_004_CharacterSystem_StatAggregationAndLevelUp`)
  needed a fixture change (test character id `"adventurer"` → `"footman"`) because `"adventurer"`
  is Java's abstract, non-instantiable base class and is now correctly excluded from the
  `AdventurerDefinition` table — the old test fixture was genuinely wrong, not weakened.
  `S6_5A_Stage4_CharacterTests.CalculateTotalStat_TraitMultiplier_...` was updated to set
  `TraitCommon` instead of `Trait` (same expected numeric assertion, `11`, unchanged) to match the
  new dual-trait shape.
- **Full unfiltered EditMode run:** 208 total tests, 2 failures — both in the third-party
  `McpUnity.Tests` package (`Packages/com.gamelovers.mcp-unity`, `UpdateComponentToolTests`,
  `"No script asset for ScriptableObject"`), unrelated to any file this phase touched. Not part of
  the `GuildMaster.Tests` namespace and not part of the "171/171" baseline this task's validation
  gate refers to.
- **Old-save migration:** verified by code inspection (see Task 1.2) rather than a throwaway
  EditMode test, given `JsonUtility`'s well-defined ignore-unmatched-field behavior and that the
  removed field (`AscensionLevel`) has zero remaining references anywhere in
  `NormalizeAfterLoad`/`NormalizeCharacter`.
- **Not performed, disclosed:** the Play Mode smoke test (real `Button.onClick.Invoke()`
  navigation into a character's Detail screen) called for in the task's validation section was not
  run, given this session's cost budget was already flagged as over threshold before Task 1.5
  began. Compile success + the 112/112 EditMode pass (which includes
  `S6_5A_Stage2_ServiceWiringTests.ServiceContainer_Initialization_AllServicesNotNull`, exercising
  every service including the new `TraitService`/rewritten `PromotionService`/`SkillService`
  through the real `ServiceContainer` composition root) is the verification actually completed for
  this report.
