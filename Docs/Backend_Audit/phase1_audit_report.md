# Phase 1 — Character Progression Core: Audit Report (Step 0)

**Project:** D:\Tinh\Rebuild_GuildMaster
**Legacy source:** D:\Tinh\Guild Master - Idle Dungeons
**Date:** 2026-08-06
**Scope:** Written BEFORE any Phase 1 implementation code, per the task's Step 0 requirement.

---

## (a) Hero class count: 116 confirmed, NOT 129

`storage/data/entities/adventurers/units/*.java` in the Legacy source contains exactly **116**
`.java` files, every one of them `public class X extends Adventurer`. This is the real, verified
hero class count (spec's "116" is correct).

However, the Rebuild's `adventurers.json` (as produced by the existing DecodeConverter) contains
**129** records. The extra 13 are NOT hero classes — the Python parser walks the whole
`entities/adventurers` Java package tree and also picked up:
- `Adventurer.java` itself (the abstract base class — `public abstract class Adventurer extends
  Entity`, never directly instantiable; `Adventurer.getInstance()` only reflects into concrete
  `units/*.java` subclasses)
- `PotionsDrank.java`
- `Doctrine.java`, `DoctrineAbility.java`, `EmptyDoctrine.java`, and the 8 `DoctrineOf*.java`
  subclasses

All 116 real hero records have `parentClass == "Adventurer"` (confirmed by grepping every record);
all 13 polluting records have `parentClass` in `{"Entity", null, "Doctrine"}`. This is an exact,
verified filter with no false positives/negatives (checked against a hand count of both sets).

**Action taken:** `DatabaseBuilder.cs`'s `AdventurerDefinition` load branch now does
`list.RemoveAll(d => d.parentClass != "Adventurer")` before registering the collection. Two
pre-existing EditMode tests (`S2_003_EquipmentSystem_SlotRestrictions`,
`S2_004_CharacterSystem_StatAggregationAndLevelUp`) used `"adventurer"` (the now-correctly-removed
abstract base) as a stand-in test character id; both were updated to use `"footman"` (a real class)
— fixing a genuinely wrong test fixture, not weakening an assertion.

---

## (b) `AscensionLevel`/`IsAscended`: partially real, partially fabricated

`Adventurer.java` (Legacy) has a **real** field: `protected boolean ascended;` with `isAscended()`/
`setAscended()`. It is not fake. It:
- Unlocks Doctrine (`canPickDoctrine() { return this.ascended; }`)
- Gates a `DRAGON_BLOOD`/`TROLL_BLOOD` trait bonus (+maxLevel/5, +9 more if ascended)
- Feeds `Formulas.experienceToNextLevel(level, ascended)` (Combat/Formula concern, untouched here)
- A previously recovered (not in this audit — from `S6_5A_Stage4_CharacterTests`, which predates
  Phase 1) +50% CON/INT/DEX/HP (NOT DEF/MDEF) stat multiplier. `Adventurer.calculateTotalStat(int)`
  is NOT decompilable (the APK strips its body — `"Method not decompiled"` in the JADX output), so
  this exact number cannot be re-verified against Java source directly, but it is an existing,
  tested rule that this audit found no reason to contradict. **Kept unchanged.**

What IS fabricated: **`AscensionLevel`** (an `int` "promotion tier counter" on `CharacterSaveData`/
`CharacterRuntime`) and its use in `PromotionService`/`PromotionDefinition`. Java has **no**
per-tier data table at all — `promotions.json` never existed in this project (confirmed: no file,
no `manifest.json` entry), so `_database.GetAll<PromotionDefinition>()` always returned an empty
collection and the entire promotion feature was dead code in practice. `AscensionLevel` never
changed `character.Definition`/`DefinitionId` — a promoted character kept the SAME
`AdventurerDefinition` forever, just with a counter that produced a fake `+10%-per-tier` stat
multiplier. This does not match Java in any way.

**Real Java promotion** (`DialogEntityDetail.dialogAdventurerPromotion/promote/ascend`,
`DialogPromotionChoices.java`):
```
if (adventurer.getMaxLevel() < 45) promote(adventurer);   // pick a NextClasses entry
else ascend(adventurer);                                   // final tier -> reset to base class
```
- **Promote**: choices = `AdventurerDefinition.NextClasses` (declarative per class, e.g.
  `Apprentice -> ["LightDisciple", "Adept"]`). Selecting one calls
  `Adventurer.getInstance(newClass, id, level=1, exp=0, weapon, armor, accessory, traitCommon,
  traitRare, potionsDrank, doctrine, isAscended)` — i.e. **the class identity (DefinitionId)
  actually changes**, Level/Experience reset to 1/0, and weapon/armor/accessory/traits/potions/
  doctrine/ascended-flag all carry through unchanged.
- **Ascend**: only reachable from a final-tier class (empty `NextClasses`, `MaxLevel == 45` for
  all observed final classes, e.g. `Balrog`). Resets `DefinitionId` to
  `Utils.getBaseClass(adventurer)` — derived from **weapon type**: `bow -> Archer`,
  `dagger -> Rogue`, `staff -> Apprentice`, else `-> Footman` — Level/Experience reset to 1/0, and
  sets `ascended = true` permanently (equipment/traits/potions/doctrine carry through unchanged,
  same as promote).

**Action taken:** `AscensionLevel` removed from `CharacterSaveData`/`CharacterRuntime`.
`PromotionService` rewritten to operate directly on `AdventurerDefinition.NextClasses`/`MaxLevel`
(no data table). `IsAscended`/`ascended` KEPT (real field, real recovered stat effect).
`CharacterService.ChangeClass()` (new) performs the actual `DefinitionId` reassignment for both
promote and ascend paths.

---

## (c) Promotion requirement: level only, no item — for the STANDARD tree

Confirmed by reading `DialogEntityDetail.promote()`/`ascend()`/`DialogPromotionChoices.java`: no
item is consumed anywhere in the standard promotion or ascension flow. The condition is purely
`character.Level >= currentDefinition.MaxLevel`.

One exception exists but is **out of scope for Phase 1**: `DialogConsumeIntercession.java` lets a
player consume an `"Intercession"` item to instantly set `ascended = true` on any non-ascended
hero WITHOUT going through the class tree or resetting level/class at all. This is a Tavern/Shop
item-consumption flow (Phase 4 territory per the Legacy_Audit phase report structure), not part of
`PromotionService`'s core class-change logic — noted here so it is not lost, but not implemented.

**Confirms the task's suspicion exactly**: the OLD C# `PromotionDefinition.RequiredItemId`/
`RequiredItemCount` (gating the STANDARD promotion tree on an item) was fabricated and has been
removed. It never had data behind it anyway (no `promotions.json`).

---

## (d) Skill-unlock-by-class wiring: schema existed, was never populated

`AdventurerDefinition.ActiveSkill`/`PassiveSkill` (string) already existed as schema slots (Phase
0), but were always `null` — the parser never resolved `Skills.ACTIVE_*`/`Skills.PASSIVE_*` enum
references. `SkillService` was a no-op shell (`CreateSkill` just wrapped an id, no lookup).

**Action taken:** a narrow, targeted regex extraction (not a full AST parser — out of scope per
Phase 0's explicit note) was run once over all 116 `units/*.java` files' `configureStatistics()`
bodies to pull `this.activeSkill = Skills.X;` / `this.passiveSkill = Skills.X;` (plus
`nextClasses.add(...)`, `imageId`, `idName`, `idDescription`, `potionDrinkerType`, `maxLevel`) and
merge the extracted values directly into `adventurers.json`. All 116 records now carry real
`NextClasses`/`ActiveSkill`/`PassiveSkill`/`ImageId`/`nameKey`/`descriptionKey`/`PotionDrinkerType`/
`MaxLevel`. `SkillService.GetByEnumConstant()` (new) resolves an `AdventurerDefinition.ActiveSkill`
value (e.g. `"ACTIVE_ENERGY_BURST_I"`) to the matching `SkillDefinition` (id
`"active_energy_burst_i"`, lowercased — confirmed against `skills.json`'s id convention).
`CharacterService.ChangeClass()` refreshes `ActiveSkillId`/`PassiveSkillId` on the runtime whenever
a character's class changes, so promotion correctly swaps skill sets.

---

## Trait system: two more real bugs found (Task 1.3 territory)

`Trait.java` (Legacy enum) has exactly 20 values: 3 common (`BOOKWORM`/`BRUTE`/`FERAL`) + 3
`_PLUS` premium-common variants + 14 rare (`EMPATHETIC`...`NIMBLE`). Cross-checking the existing
`CharacterService.GetTraitMultiplier` switch against this enum found:
- `"STOUT"` and `"KEEN_EYED"` handled there **do not exist in `Trait.java` at all** — confirmed
  fabricated (already flagged in `Docs/Backend_Audit/traits_audit.md`, now actually removed).
- `"NIMBLE"` is real, but in Java it only grants `+0.08` flat dodge chance
  (`Adventurer.calculateTotalFlatDodgeChance`) — it never touches Dexterity. Mapping it to a DEX
  stat multiplier (as the old code did) was also wrong. Flat dodge chance is a Combat-system stat,
  deferred to Phase 2.
- `Utils.rollRareTrait()` (Java) rolls among all **14** rare traits (1/70 ≈ 1.4286% each, 20% total
  chance of any). `TavernService.RollRareTrait()` only rolled among the first **7**
  (`EMPATHETIC`...`REACTIVE`), missing `NOCTURNAL`/`MINDFUL`/`TROLL_BLOOD`/`RUTHLESS`/`BLESSED`/
  `ALERT`/`NIMBLE` entirely — a real, separate bug, now fixed (`TraitService.RollRareTrait`, exact
  Java thresholds).
- `Utils.java`'s `generateVisitor()` rolls `traitCommon` and `traitRare` **independently** for
  every random guest (`rollCommonTrait()` AND `rollRareTrait()`, both passed to
  `Adventurer.getInstance`) — a guest can have both, either, or neither. The old
  `TavernService.GenerateVisitor()` treated them as mutually exclusive (`trait =
  RollCommonTrait() ?? RollRareTrait()`), which was wrong. Fixed.

Phase 0 already added `TraitCommon`/`TraitRare` directly on `CharacterSaveData` (not a separate
"SaveData-level" vs "character-level" split — they're the same class; there is no duplication to
resolve). `CharacterRuntime` did NOT have the split (still had a single `Trait` string) — added in
Phase 1, with `Trait` kept only as a legacy display alias (`== TraitCommon`).

---

## Doctrine-per-character relation

`AdventurerDefinition` has no doctrine relation field (correctly — Java's per-instance
`Adventurer.doctrine` field is a `CharacterSaveData`/runtime concern, not a class-definition
concern). Current `SaveData` only stores ACCOUNT-GLOBAL doctrine levels
(`WarLevel`/`AfflictionLevel`/...), not one `Doctrine` object per hero as Java has — this
structural gap was already flagged in `Docs/Backend_Audit/phase0_schema_mapping.md` §5/§10 as
"the most severe structural loss in the whole audit set" and is a full Doctrine-system rebuild,
explicitly out of scope for Phase 1 (Character Progression). The Character Detail UI's Doctrine
section continues to show the existing account-global data; a note documents this deferral.

---

## Files audited (read-only) this step

`AdventurerDefinition.cs`, `CharacterService.cs`, `CharacterRuntime.cs`, `SaveData.cs`
(`CharacterSaveData`), `PromotionService.cs`, `PromotionDefinition.cs` (found — existed but fake),
`TraitService.cs` (did not exist — trait logic lived in `TavernService.RollCommonTrait/
RollRareTrait`), `SkillDefinition.cs`, `SkillService.cs` (existed, no-op), `TavernService.cs`,
`CharacterDetailPanel.cs`, `RuntimeFactory.cs`, `ServiceContainer.cs`, `GameDatabase.cs`,
`DatabaseBuilder.cs`, plus the Legacy Java files: `Adventurer.java`, `Trait.java`, `Utils.java`,
`DialogPromotionChoices.java`, `DialogEntityDetail.java`, `DialogConsumeIntercession.java`,
`DialogConsumeEvo23.java`, `DialogConsumePotionOfRejuvenation.java`, and all 116
`units/*.java` files (mechanically, via script, for field extraction).
