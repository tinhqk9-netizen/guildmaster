# Phase 2A — Combat Foundation Restoration: Audit Report

**Project:** D:\Tinh\Rebuild_GuildMaster
**Date:** 2026-08-06
**Scope:** Equipment / Combat / Doctrine — audit only, written before any Phase 2A code change.
Builds on the pre-existing `Docs/Backend_Audit/equipment_audit.md` and `doctrine_audit.md` (already
present from an earlier pass); this report consolidates those findings with direct verification
against the decompiled Java source and adds the Combat-pipeline half neither prior doc covered.

---

## 1. Equipment System

**Java source:**
- `storage/data/items/abstractClasses/Equipment.java` — base stat/modifier fields shared by all
  gear (constitution, lifesteal, threat, counterattack, criticalChance/Damage, flatDodgeChance,
  healingModifier, immunityToStatus, regeneration, retaliation, decay, initiative, alwaysHits, ...).
- `storage/data/items/abstractClasses/Weapon.java` + `Sword.java`/`Staff.java`/`Dagger.java`/`Bow.java`
  — each weapon subtype overrides `getDamageModifier(con,int,dex)` and `damageDelta()`.

**C# files:** `Definitions/ItemDefinition.cs`, `Database/ItemFieldsLoader.cs`,
`Runtime/Services/EquipmentService.cs`, `Runtime/Services/CharacterService.cs` (`GetTotalStat`).

**What was missing/wrong (confirmed against `items.json`'s real "fields" data, e.g.
`amulet_of_the_swordsman`: counterattack, `archmage_hat`: criticalDamage, `banshee_scream`:
criticalChance, `bleak_boots`: flatDodgeChance):**
- `ItemFieldsLoader.ItemFieldsDto` only declared 6 fields (constitution/dexterity/intelligence/
  defense/magicDefense/maxHp). Every other real, present-in-data Equipment modifier — lifesteal,
  lifestealWithMinion, threat, counterattack, criticalChance, criticalDamage, flatDodgeChance,
  healingModifier, immunityToStatus, regeneration, regenerationBonus, retaliationPhysical/
  MagicalDamage, bonusExperience, darknessReduction, decay, initiative, alwaysHits — was silently
  dropped. `ItemDefinition.cs` had no fields to hold them even if parsed.
- `EquipmentService`/`CharacterService.GetTotalStat` only ever read Constitution/Intelligence/
  Dexterity/Defense/MagicDefense/MaxHp off an equipped item. None of the above modifiers were fed
  into any runtime stat, meaning even if the data existed, nothing downstream would have read it
  (equipment "affecting combat" was false for everything except the 6 base stats).

## 2. Combat Formula (`CombatService.cs`)

**Java source (verified directly, not inferred):**
- `Sword.java`: `getDamageModifier(i,i2,i3) -> i` (Constitution). `damageDelta() -> 0.15`.
- `Staff.java`: `getDamageModifier -> i2` (Intelligence). `damageDelta() -> 0.05`.
- `Dagger.java`: `getDamageModifier -> i + i3` (Constitution + Dexterity). `damageDelta() -> 0.25`.
- `Bow.java`: `getDamageModifier -> i3` (Dexterity). `damageDelta() -> 0.10`.
- `Adventurer.java calculateMinAttackDamage()/calculateMaxAttackDamage()`:
  `Utils.round(damageModifier * (1 - weapon.damageDelta()))` /
  `Utils.round(damageModifier * (1 + weapon.damageDelta()))`, with SerpentBite's unique
  `damageModifier *= getThreat()` special case.
- `Entity.java rollAttackDamage()`/`applyDamage()` — the roll-and-reduce pipeline.
- `Utils.round(double)` == `(int)(d + 0.0001)` — truncation with an epsilon nudge, NOT
  round-half-up. Already correctly ported as `DecodeMath.Round` before Phase 2A.

**Current C# state before this phase (`CombatService.AdventurerWrapper.MinAttackDamage`/
`MaxAttackDamage`), confirmed by direct read:**
```csharp
case "sword":  mod = con * 1.2 + dex * 0.4; break;   // Java: mod = con, period.
case "staff":  mod = intel * 1.5; break;              // Java: mod = intel, period.
case "dagger": mod = dex * 1.2 + con * 0.3; break;    // Java: mod = con + dex.
case "bow":    mod = dex * 1.5; break;                // Java: mod = dex, period.
...
double delta = 0.2; // hardcoded for ALL weapon types — Java uses a DIFFERENT delta per type
                     // (0.15/0.05/0.25/0.10), never 0.2 for any of the 4 real weapon classes.
```
This is a **fabricated formula** with no Java source — confirmed matching `equipment_audit.md §2.2`
independently from the raw Java files rather than trusting the prior audit's conclusion alone. Every
number in it (1.2, 0.4, 1.5, 0.3, 0.2) was invented; none appear anywhere in `Sword`/`Staff`/
`Dagger`/`Bow`/`Weapon`.java. Net effect confirmed: damage was inflated ~20-50% and scaled the wrong
secondary stats for 3 of 4 weapon types.

Additional gaps found in the Combat pipeline (not previously documented):
- `Threat` was hardcoded to a flat `5` on `AdventurerWrapper` with a comment claiming
  "Adventurer.java:292" as evidence — that line does not say 5. Java's real formula
  (`Adventurer.getThreat()`) is `Math.max(1, Entity.threat(=1) + weapon+armor+accessory.getThreat()
  + (traitRare==INTIMIDATING?1:0) + doctrine.bonusThreat())`. The hardcoded 5 was unverifiable and
  is corrected to the real formula in this phase.
- No Critical hit chance/damage was rolled or applied anywhere in `ProcessTurn` — `Adventurer.
  calculateCriticalChance()`/`calculateCriticalDamage()` exist in Java and are fully computable from
  now-real Equipment data, but nothing in C# called them.
- No Lifesteal, no Dodge (`FlatDodgeChance`), no `AlwaysHits` bypass were implemented, despite the
  underlying Equipment data now being parseable per §1.
- `Regeneration` was hardcoded to `1` for every adventurer; Java's base `Entity.regeneration` is
  `0`, with the real value coming entirely from equipment (+ a rare-trait bonus not modeled).
- `RollAttackDamage`/`ApplyDamage` themselves (the roll-and-reduce math) were confirmed to already
  be a correct, faithful port of `Entity.rollAttackDamage()`/`applyDamage()` — these were NOT
  touched beyond adding the crit-multiplier/lifesteal/dodge hooks around them.

## 3. Doctrine System

**Java source (verified directly):**
- `doctrines/DoctrineAbilityType.java` — enum of exactly **40** ability types, each with its own
  `cost`/`increasePerLevel`/`maxLevel` (counted directly: 40 enum constants, confirming the task
  brief's "40 Nodes total" figure).
- `doctrines/Doctrine.java` — abstract base, 6 independent int fields `l1..l6`, one per node slot.
- `doctrines/instances/DoctrineOf{War,Affliction,Control,Fortitude,Grace,Illusion,Knowledge,Ruin}.java`
  — exactly **8** concrete doctrines, each `setupAbilities()` returning an ordered 6-element list of
  `DoctrineAbilityType` mapped to that doctrine's l1..l6 (8 × 6 = 48 slots, drawing from the 40
  distinct types with some types reused by more than one doctrine, e.g. `IMPROVED_HEALTH` appears
  in Affliction/Fortitude/Grace).

**C# state before this phase:** `DoctrineDefinition`/`DoctrineNodeDefinition` schema (Phase 0) and
`DoctrineNodeSaveData` (Phase 0, per-node `{DoctrineId, NodeId, Level}`) already existed as inert
save-data/schema slots — confirmed via `phase0_completion_report.md`. `DoctrineService.cs` itself
only ever read/wrote ONE summed `Level`/`Progress` pair per doctrine name (`WarLevel`, ...) — real
Java data, but a DIFFERENT concept: it maps to `Doctrine.bonusQuestPoints()` (an account-wide
"doctrine mastery" counter fed by `AddProgress` from Quest completions), not to any of the 40 nodes'
individual levels. No `doctrines.json` exists anywhere in the decoded data (confirmed, matches
Phase 0 finding) — the 8×6 catalog has never been authored as loadable data. `CombatService`/
`CharacterService` had zero doctrine integration prior to this phase — confirmed by grep, matching
`doctrine_audit.md`'s conclusion independently.

## 4. Plan for Phase 2A

1. **Equipment** — extend `ItemDefinition`/`ItemFieldsLoader` to parse and store every real
   Equipment modifier confirmed present in `items.json`; wire the new fields into
   `CharacterService.GetTotalStat` (int-valued: Threat/Lifesteal/Regeneration/BonusExperience/
   DarknessReduction) and a new `GetCombatModifier` (double-valued: Counterattack/CriticalChance/
   CriticalDamage/FlatDodgeChance/HealingModifier/ImmunityToStatus).
2. **Combat** — replace the fabricated weapon-modifier switch with the exact
   Sword/Staff/Dagger/Bow `getDamageModifier`+`damageDelta` mapping; fix Threat to the real formula;
   add Crit roll+multiplier, Lifesteal application, and Dodge roll to `ProcessTurn`; keep the
   existing Pet-bonus injection points as the Phase 4 extension hooks (no new Pet logic added).
3. **Doctrine** — author the 8×6 node catalog as real, Java-sourced C# data (no `doctrines.json`
   exists to load from) registered through `GameDatabase` like any other category; extend
   `DoctrineService` with per-node level get/set (using the already-prepared `DoctrineNodeSaveData`)
   without touching the existing account-wide Level/Progress pair; wire a confirmed subset of node
   effects (Threat, Lifesteal, Counterattack, CritChance, CritDamage, Dodge, StatusImmunity) into
   the Combat/Character pipeline built in step 2. Per-character doctrine ASSIGNMENT (Java: one
   `Doctrine` object per `Adventurer`) does not exist in the Rebuild save format — flagged in
   `phase1_completion_report.md` as a full Doctrine-system rebuild, out of this phase's scope — so
   node bonuses are summed account-wide as a documented, explicit simplification pending that
   larger rebuild.
