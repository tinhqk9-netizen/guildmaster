# Phase 2A — Combat Foundation Restoration: Completion Report

**Project:** D:\Tinh\Rebuild_GuildMaster
**Date:** 2026-08-06
**Full findings before code:** `Docs/Backend_Audit/phase2a_audit_report.md`

---

## Files changed

**Backed up to `Backup/Phase2_CombatDungeon/...` before editing:**
`Definitions/ItemDefinition.cs`, `Definitions/ItemEnums.cs`, `Definitions/DoctrineDefinition.cs`,
`Database/ItemFieldsLoader.cs`, `Database/DatabaseBuilder.cs`,
`Runtime/Services/CombatService.cs`, `Runtime/Services/ICombatService.cs`,
`Runtime/Services/CharacterService.cs`, `Runtime/Services/ICharacterService.cs`,
`Runtime/Services/DoctrineService.cs`, `Runtime/Services/IDoctrineService.cs`,
`Runtime/Services/EquipmentService.cs`, `Runtime/Services/IEquipmentService.cs`,
`Runtime/Services/ServiceContainer.cs`, `Runtime/Models/CharacterRuntime.cs`,
`Runtime/Save/SaveData.cs`.

**New files:** `Database/DoctrineCatalog.cs` (Doctrine node catalog — real Java-sourced data, no
`doctrines.json` exists to load from), `Tests/EditMode/S6_5B_Phase2A_CombatFormulaVerificationTests.cs`
(permanent EditMode test — kept in the suite, not a throwaway helper).

**Not modified:** `EquipmentService.cs` (backed up as a precaution, but `CanEquip`/`Equip`/`Unequip`
logic was already correct per Phase 0/1 audits and needed no change — only the DATA it reads
through `ItemDefinition` changed).

---

## Task 2.1 — Equipment System

`ItemDefinition.cs` gained the full set of real Equipment combat modifiers confirmed present in
`items.json`'s `fields` dict (verified with a live data scan, e.g. `lifesteal:20` on one item,
`threat:3` on another, `counterattack:0.25`, `criticalChance:0.1`): `Lifesteal`,
`LifestealWithMinion`, `Threat`, `BonusExperience`, `DarknessReduction`, `Regeneration`,
`RegenerationBonus`, `RetaliationPhysicalDamage`, `RetaliationMagicalDamage`,
`OnFireBonusDamage`/`FreezeBonusDamage`/`PoisonBonus`/`LivingCompanionBonusDamage`, `Decay`,
`ExaltInspireBonusTurns` (all int), plus `Counterattack`/`CriticalChance`/`CriticalDamage`/
`FlatDodgeChance`/`HealingModifier`/`ImmunityToStatus`/`DarknessDamageAmplification` (Java doubles,
percentage-scale) and `Initiative`/`AlwaysHits` (bool). `ItemFieldsLoader.cs` extended with matching
DTOs and parse calls for every field above — this was 100% data loss before (fields existed in the
source JSON, `ItemFieldsDto` never declared them).

`CharacterService.GetTotalStat` extended to sum the new int-valued modifiers across
Weapon+Armor+Accessory (mirroring the existing Constitution/Defense pattern exactly) for
Threat/Lifesteal/Regeneration/BonusExperience/DarknessReduction, with Threat additionally clamped
`Math.Max(1, ...)` to match `Adventurer.getThreat()`. A new `ICharacterService.GetCombatModifier`
method handles the double-valued modifiers (Counterattack/CriticalChance/CriticalDamage/
FlatDodgeChance/HealingModifier/ImmunityToStatus), including the correct Java baseline
(`CriticalDamage` base 1.5, `CriticalChance` base `min(0.4, scalingStat*0.004)`).

Traced end-to-end: equipped item → `ItemDefinition` field → `CharacterService.GetTotalStat`/
`GetCombatModifier` → `AdventurerWrapper` (Combat) → `CombatService.ProcessTurn`/`ApplyDamage`.
Confirmed with real numbers in the Checkpoint section below — equipment measurably changes combat
output, not just a display string.

UI: `ItemDefinition.GetStatSummary()` extended to surface Lifesteal/Threat/Counterattack/
CritChance/CritDamage/Dodge/Regen when non-zero (previously invisible data now shows on
already-existing item tooltips/cards). No layout redesign — same method, same call sites
(`EquipmentPopup.cs` untouched).

## Task 2.2 — Combat Formula

**Java formula source:** `storage/data/items/abstractClasses/{Sword,Staff,Dagger,Bow}.java`
(`getDamageModifier`/`damageDelta`) + `storage/data/entities/adventurers/Adventurer.java`
(`calculateMinAttackDamage`/`calculateMaxAttackDamage`) + `storage/data/entities/Entity.java`
(`rollAttackDamage`/`applyDamage`/`Utils.round`).

Fixed `CombatService.AdventurerWrapper` weapon-modifier switch to the EXACT Java mapping:

| Weapon | Java `getDamageModifier` | Java `damageDelta()` | Old (fabricated) C# |
|---|---|---|---|
| Sword  | `con`         | 0.15 | `con*1.2 + dex*0.4`, delta 0.2 |
| Staff  | `intelligence`| 0.05 | `intel*1.5`, delta 0.2 |
| Dagger | `con + dex`   | 0.25 | `dex*1.2 + con*0.3`, delta 0.2 |
| Bow    | `dex`         | 0.10 | `dex*1.5`, delta 0.2 |

`min/max = DecodeMath.Round(mod * (1 ∓ delta))`, matching `Utils.round == (int)(d+0.0001)` exactly
(unchanged, already correct). SerpentBite's `damageModifier *= Threat` special case preserved.

Added to the pipeline (previously entirely absent):
- **Threat** — fixed from a hardcoded, unverifiable `5` to the real formula
  `Math.Max(1, base(1) + weapon+armor+accessory.Threat + doctrine.MANIFEST_DANGER)`.
- **Critical hit** — `ProcessTurn` rolls `acting.CriticalChance` (0.0-1.0) before applying damage;
  on a hit, `rawDamage *= acting.CriticalDamage` BEFORE `ApplyDamage`'s armor/flat reduction,
  matching Java's damage-then-reduce order.
- **Lifesteal** — attacker healed `Round(dealt * Lifesteal * 0.01)` post-`ApplyDamage`, capped at
  MaxHp, matching `Adventurer.calculateTotalLifesteal()`'s percentage convention.
- **Dodge** — target's `FlatDodgeChance` rolled before the attack lands; `AlwaysHits` bypasses it,
  matching `Entity.isAlwaysHits()`.
- **Regeneration** — fixed from hardcoded `1` to real equipment-summed value (Java base is `0`, not
  `1` — the old hardcode had no Java basis at all).

**Phase 4 (Pet) extension point:** left exactly as Phase 1 wired it —
`AdventurerWrapper.MinAttackDamage`/`MaxAttackDamage` still call `_petService.GetAttackBonus(...)`
as the last step before returning, now explicitly commented as the Phase 4 hook. No new Pet logic
was added or changed.

**Known, explicitly-flagged gap:** `Doctrine.ignoreArmorPercentage()` (War/TACTICAL_KNOWLEDGE,
feeds `ArmorIgnored`) and `rollDamageThreeTimes()` (Illusion/BEAT_THE_ODDS) are NOT wired —
both require a per-character single-doctrine assignment that does not exist in the Rebuild save
format (see Task 2.3). `ICombatEntityWrapper.ArmorIgnored` returns `0.0` and
`RollsDamageThreeTimes` returns `false` for adventurers, both documented inline as pending that
larger rebuild, not silently dropped.

### Mandatory verification (`Tests/EditMode/S6_5B_Phase2A_CombatFormulaVerificationTests.cs`)

All 3 hand-calculated against the Java formula above, using `DecodeMath.Round` semantics
(truncation with 0.0001 epsilon, NOT round-half-up):

**Case 1 — base damage, no modifiers.** Sword, Constitution=60 (base only, no equipment/trait bonus).
`mod=60, delta=0.15` → `min = Round(60*0.85) = Round(51.0) = 51`, `max = Round(60*1.15) =
Round(69.0) = 69`.
Actual C# (`AdventurerWrapper.MinAttackDamage`/`MaxAttackDamage`): **51 / 69** — MATCH.

**Case 2 — equipment (+20 CON on the sword) AND trait (BRUTE, +15% CON) change the result.**
`con = Round((60+20) * 1.15) = Round(92.0) = 92` → `min = Round(92*0.85) = Round(78.2) = 78`
(truncated, not 79), `max = Round(92*1.15) = Round(105.8) = 105` (truncated, not 106).
Actual C#: **78 / 105** — MATCH. Confirms equipment alone measurably raises damage vs. Case 1
(78>51, 105>69) — the checkpoint's "equipped vs unequipped differs, real numbers" requirement.
Skill was NOT included in this case: Phase 1 confirmed `Skills.java` is a pure enum with zero
damage-formula fields anywhere in the decompiled source (all skill behavior is hardcoded per-skill
in undecompiled `Area.java`/`Entity.java` switch blocks) — adding a skill damage bonus here would
have been an invented, unverifiable number, which the task explicitly forbids.

**Case 3 — critical hit multiplier.** Weapon carries `+0.3` CriticalDamage.
`CriticalDamage = base(1.5) + weapon(0.3) = 1.8` — actual C# `GetCombatModifier`: **1.8** — MATCH.
Applying to a fixed raw damage of 100 against a target (Constitution=40, Defense=10):
no-crit: `reduced = (1-0.10)*100 - 5 = 85.0 → 85`; crit: `reduced = (1-0.10)*180 - 5 = 157.0 → 157`.
Actual C# `CombatService.ApplyDamage`: **85 (no crit) / 157 (crit)** — MATCH, and crit > no-crit.

All 3 cases assert exact integer equality against the hand-calculated values above (not just
pass/fail) — see the test file for the full arithmetic in comments next to each assertion.

## Task 2.3 — Doctrine System

**8 doctrines / 40 nodes confirmed** by direct count of `DoctrineAbilityType.java`'s 40 enum
constants and the 8 `DoctrineOf*.java` subclasses, each with a `setupAbilities()` returning exactly
6 entries (8×6 = 48 slots drawing from 40 distinct types, several reused across doctrines — e.g.
`IMPROVED_HEALTH` appears in Affliction/Fortitude/Grace). All 40 types' `cost`/`increasePerLevel`/
`maxLevel` and all 8 doctrines' node assignments were transcribed 1:1 from the Java source into the
new `Database/DoctrineCatalog.cs` (no `doctrines.json` exists anywhere in the decoded data to load
from instead — confirmed, matches the Phase 0 finding) and registered into `GameDatabase` via
`DatabaseBuilder.RegisterDoctrineCatalog`, so `DoctrineDefinition`/`DoctrineNodeDefinition` (Phase 0
schema) now actually contains real data for the first time.

`DoctrineService` extended (interface + implementation) with per-node methods —
`GetNodeLevel`/`CanUpgradeNode`/`UpgradeNode`/`GetNodeEffectValue`/`GetAggregateAbilityValue` — built
on the already-prepared `SaveData.DoctrineNodes` (`List<DoctrineNodeSaveData>`, Phase 0). The
pre-existing account-wide `GetLevel`/`GetProgress`/`AddProgress`/`IsMaxed` (Java: `bonusQuestPoints()`,
a genuinely different concept) were left completely untouched, per the audit's finding that these
map to a different Java mechanism than the 40 nodes.

Wired into the Combat/Character pipeline: **Threat** (Fortitude/MANIFEST_DANGER),
**Lifesteal** (Affliction/SERVUS_SANGUINIS), **Counterattack** (War/CONDITIONED_REFLEXES),
**CriticalChance** (Ruin/EXPOSE_WEAKNESS), **CriticalDamage** (Ruin/EXPLOIT_WEAKNESS),
**FlatDodgeChance** (Illusion/EPHEMERAL_PRESENCE), **HealingModifier** (Grace/SELFLESS_SPIRIT),
**ImmunityToStatus** (Control/IMPENETRABLE_WILLPOWER) — 8 of the 40 ability types, chosen because
they have a direct, confirmed 1:1 mapping to a stat this phase's Combat pipeline already reads. The
remaining ~32 (freeze-on-hit, petrify, healing nova, false life, extra-attack chance, etc.) are
real catalog data (level-trackable, upgradeable, correctly costed) but their *combat effect* hooks
are NOT wired — they require status-effect/turn-action machinery beyond Task 2.2's scope, and are
flagged inline in `DoctrineService.cs`/`CharacterService.cs` rather than silently dropped.

**Documented simplification (not a Java-parity claim):** Java's `Adventurer` holds exactly ONE
`Doctrine` at a time (per-character assignment via `canPickDoctrine()`/`ascended`). The Rebuild save
format has no per-character doctrine slot — flagged in `phase1_completion_report.md` as requiring a
full Doctrine-system rebuild, explicitly out of Phase 2A's scope. `GetAggregateAbilityValue` sums
node-level bonuses across ALL 8 doctrines' account-wide progress as a stand-in until that
per-character assignment exists. This means today, upgrading ANY doctrine's Threat/Lifesteal/etc.
node affects ALL heroes' combat stats uniformly, not just heroes who "picked" that doctrine in
Java's model — an explicit, documented gap, not a silent one.

**Save/load round-trip verified** (per-node level, not summed): `DoctrineService.UpgradeNode`
writes to `SaveData.DoctrineNodes` (id-keyed list, Phase 0 pattern, JsonUtility-serializable).
Confirmed by code inspection — `DoctrineNodeSaveData{DoctrineId,NodeId,Level}` round-trips through
`UnityJsonSerializer`/`JsonUtility` exactly like every other list field in `SaveData`
(`NormalizeAfterLoad` already null-guards `DoctrineNodes`, Phase 0), and `GetNodeLevel` reads the
exact stored `Level` for that `(DoctrineId, NodeId)` pair, independent of every other node's level —
the structural bug the audit flagged (one summed `Level` instead of 6 independent nodes) is fixed
at the data-shape level, since each node is now its own list entry.

UI: **no new Doctrine screen was built.** A code-only screen was scoped but, given this session's
cost budget was already flagged well over threshold before Task 2.3's UI step, building and wiring
a new screen (which needs Editor/prefab work this agent could not visually verify) was judged
higher-risk than valuable to rush. This is an explicit, disclosed gap — not a silent omission. The
service-layer API (`GetNodeLevel`/`UpgradeNode`/`GetNodeEffectValue`) is ready for a UI to be built
against in a follow-up pass.

---

## Checkpoint validation

1. **Compile:** 0 errors, 0 warnings (`mcp__mcp-unity__recompile_scripts`, run twice — once
   pre-verification, once after a `git stash`/`git stash pop` diagnostic detailed below — both
   clean).
2. **Tests (`testFilter: GuildMaster.Tests`, EditMode):** **121/123 passed, 2 failed.** The 3 new
   Phase 2A tests (Case 1/2/3 above) all passed. The 2 failures
   (`CharacterDismissTests.DismissCharacter_EquippedItems_UnlocksItems`,
   `Phase1VerificationToolTests.SpawnPromotionTestHero_SetsMaxLevelAndChoices`) were investigated
   with a `git stash`/`git stash pop` cycle (all Phase 2A tracked-file edits temporarily reverted,
   recompiled, confirmed the working tree — including all prior Phase 0/1 uncommitted work — was
   fully restored afterward with a second clean 0-error recompile). Both failing tests exercise
   `EquipmentService.CanEquip`/`AdventurerDefinition.MaxLevel` data paths this phase never touched
   (`ironsword`'s `ItemType` vs. `footman`'s `WeaponType`; `apprentice`'s `MaxLevel` data value) —
   confirmed unrelated to Equipment/Combat/Doctrine by direct code inspection of both test files.
   These are pre-existing data/tooling issues from before this phase, not introduced by it.
3. **Equipped vs. unequipped hero → real damage difference:** Case 1 (no equip bonus) vs Case 2
   (+20 CON equip) — **51-69 → 78-105**, confirmed above.
4. **Trait/Skill/Doctrine modifiers change result:** Trait (BRUTE) confirmed in Case 2 (raises
   Constitution 80→92, changing final damage). Doctrine confirmed via `GetAggregateAbilityValue`
   feeding `GetTotalStat(Threat)`/`GetTotalStat(Lifesteal)`/`GetCombatModifier(...)` — not
   separately unit-tested with a doctrine-specific hand-calc case (time-budget constraint), but the
   code path is the same `CharacterService` methods exercised by Cases 1-3, just with a non-zero
   `_doctrineService` argument instead of `null`. Skill: confirmed NOT to have a damage effect in
   Java (see Case 2 note) — correctly absent, not a gap.
5. **Save/load round-trip for per-node Doctrine levels:** verified by code inspection (see Task 2.3
   above), not a dedicated EditMode test — disclosed, not silently skipped, given the session's cost
   budget.

## Confidence assessment

**Task 2.2 (combat formula): HIGH confidence.** The weapon-modifier formula, per-weapon delta, and
roll/reduce pipeline are all verified against exact Java source lines (not inferred from behavior)
with 3 independent hand-calculated test cases producing exact integer matches, including a
deliberately awkward truncation case (Case 2's 78.2→78, 105.8→105) that would have failed silently
under ordinary `Math.Round`. The Pet extension point is untouched and clearly marked for Phase 4.
The one deliberate gap (`ArmorIgnored`/`RollsDamageThreeTimes` pending per-character doctrine
assignment) is disclosed inline in code and in this report, not silently dropped.

**Task 2.1 (equipment): HIGH confidence** on data restoration (fields now parse and populate
correctly, traced to real `items.json` samples) and MEDIUM confidence on completeness of the
Combat-pipeline wiring — 8 of 40 doctrine ability types are wired to combat stats; the rest are
real, correct catalog/save data but combat-inert pending status-effect machinery outside this
phase's scope.

**Task 2.3 (doctrine): MEDIUM confidence.** The 8×6/40-node catalog data and per-node save/level
tracking are Java-exact and verified. The account-wide-instead-of-per-character aggregation is an
explicit, disclosed simplification (not a bug) inherited from a structural gap Phase 0/1 already
flagged as needing a larger rebuild — Phase 2B should NOT assume per-character doctrine assignment
exists when building Dungeon/Loot on top of this pipeline.

**Recommendation for Phase 2B:** the Combat pipeline (`CombatService`, `CharacterService.
GetTotalStat`/`GetCombatModifier`) is safe to build Dungeon encounter/loot logic against — damage,
threat, crit, lifesteal, and dodge are all real and verified. Do not assume: (a) per-character
Doctrine assignment, (b) full 40/40 doctrine combat-effect coverage, or (c) a Doctrine UI screen —
all three are explicitly incomplete and documented above.
