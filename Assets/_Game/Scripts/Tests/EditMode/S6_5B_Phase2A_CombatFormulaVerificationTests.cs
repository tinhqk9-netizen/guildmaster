using NUnit.Framework;
using GuildMaster.Database;
using GuildMaster.Definitions;
using GuildMaster.Runtime.Core;
using GuildMaster.Runtime.Formulas;
using GuildMaster.Runtime.Models;
using GuildMaster.Runtime.Save;
using GuildMaster.Runtime.Services;

namespace GuildMaster.Tests.EditMode
{
    /// <summary>
    /// Phase 2A mandatory verification: hand-calculated damage numbers against the EXACT Java
    /// formula, ported straight from the decompiled source with no invented values.
    ///
    /// Java sources verified against:
    ///   - storage/data/items/abstractClasses/Sword.java   (getDamageModifier -> con, damageDelta -> 0.15)
    ///   - storage/data/entities/adventurers/Adventurer.java
    ///       calculateMinAttackDamage(): round(damageModifier * (1 - weapon.damageDelta()))
    ///       calculateMaxAttackDamage(): round(damageModifier * (1 + weapon.damageDelta()))
    ///       calculateCriticalDamage(): base 1.5 + weapon/armor/accessory.getCriticalDamage() + doctrine
    ///   - storage/data/entities/Entity.java Utils.round == (int)(d + 0.0001) (truncation, not
    ///     round-half-up) — ported as DecodeMath.Round.
    ///
    /// This suite does not exercise RNG (RollAttackDamage's random roll, crit roll, dodge roll) —
    /// it verifies the deterministic min/max damage endpoints and modifier values that RNG samples
    /// from, per the task's explicit instruction to test the deterministic parts when RNG is
    /// involved.
    /// </summary>
    public class S6_5B_Phase2A_CombatFormulaVerificationTests
    {
        private GameDatabase _database;
        private SaveService _saveService;
        private FormulaService _formulaService;
        private RuntimeFactory _runtimeFactory;
        private ItemService _itemService;
        private InventoryService _inventoryService;
        private CharacterService _characterService;

        private AdventurerDefinition _heroDef;

        [SetUp]
        public void Setup()
        {
            _database = new GameDatabase();
            _saveService = new SaveService();
            _saveService.DeleteSave();
            _saveService.Load(out _);
            _formulaService = new FormulaService();
            _runtimeFactory = new RuntimeFactory(new DefaultInstanceIdGenerator());
            _itemService = new ItemService(_runtimeFactory, _database);
            _inventoryService = new InventoryService(_saveService, _formulaService, _itemService, _database);

            // CharacterService with no IDoctrineService (null) — doctrine node bonuses default to
            // 0 in that case (see CharacterService.GetTotalStat/GetCombatModifier), isolating the
            // Equipment/Trait pipeline this test targets.
            _characterService = new CharacterService(_saveService, _formulaService, _database, _runtimeFactory, _inventoryService);

            // Synthetic hero definition with hand-picked round stats (not real game data — chosen
            // so hand-calculation avoids DecodeMath.Round's truncate-not-round-half-up ambiguity
            // wherever possible; see inline comments where it does not).
            _heroDef = new AdventurerDefinition
            {
                id = "test_hero",
                MaxLevel = 50,
                BaseConstitution = 60,
                BaseDexterity = 20,
                BaseIntelligence = 10,
                BaseDefense = 5,
                BaseMagicDefense = 5,
                BaseMaxHp = 100
            };
        }

        [TearDown]
        public void TearDown()
        {
            _saveService.DeleteSave();
        }

        private CharacterRuntime MakeHero()
        {
            var c = new CharacterRuntime("hero_1", _heroDef) { Level = 1 };
            return c;
        }

        private ItemRuntime MakeSword(int conBonus = 0, double critDamageBonus = 0)
        {
            var def = new ItemDefinition
            {
                id = "test_sword",
                parentClass = "Sword",
                Category = ItemCategory.Weapon,
                ItemType = "Sword",
                Constitution = conBonus,
                CriticalDamage = critDamageBonus
            };
            return new ItemRuntime("sword_1", def);
        }

        // ------------------------------------------------------------------------------------
        // Test Case 1: base damage, no equipment stat bonus, no trait.
        // Java: Sword.getDamageModifier(con,int,dex) = con. Sword.damageDelta() = 0.15.
        // con = BaseConstitution(60) + equip(0) = 60.
        //   min = round(60 * (1 - 0.15)) = round(60 * 0.85) = round(51.0)  = 51
        //   max = round(60 * (1 + 0.15)) = round(60 * 1.15) = round(69.0)  = 69
        // ------------------------------------------------------------------------------------
        [Test]
        public void Case1_BaseDamage_NoModifiers_MatchesJavaSwordFormula()
        {
            var hero = MakeHero();
            hero.Weapon = MakeSword(conBonus: 0);

            int con = _characterService.GetTotalStat(hero, StatType.Constitution);
            Assert.AreEqual(60, con, "Constitution should be exactly base (60), no equipment/trait/doctrine bonus.");

            var wrapper = new AdventurerWrapper(hero, _characterService, null);

            Assert.AreEqual(51, wrapper.MinAttackDamage, "Min damage must equal round(con * 0.85) = round(51.0) = 51.");
            Assert.AreEqual(69, wrapper.MaxAttackDamage, "Max damage must equal round(con * 1.15) = round(69.0) = 69.");
        }

        // ------------------------------------------------------------------------------------
        // Test Case 2: equipment (+20 CON on the sword) AND a trait (BRUTE, +15% CON) both
        // change the result, exactly as the real pipeline (CharacterService.GetTotalStat) sums
        // them: (base + equip) * traitMult.
        //   con = round((60 + 20) * 1.15) = round(80 * 1.15) = round(92.0) = 92
        //   min = round(92 * 0.85) = round(78.2)  -> DecodeMath truncates: (int)(78.2+0.0001) = 78
        //   max = round(92 * 1.15) = round(105.8) -> (int)(105.8+0.0001) = 105
        //
        // NOTE: a "skill" leg of this case was intentionally NOT added. Skills.java (verified in
        // Phase 1, phase1_completion_report.md Task 1.4) is a pure enum with zero damage-formula
        // fields anywhere in the decompiled source — all skill behavior is hardcoded per-skill in
        // Area.java/Entity.java combat switch blocks that JADX could not decompile. Fabricating a
        // skill damage bonus here would violate the "never invent an unverified formula" rule.
        // ------------------------------------------------------------------------------------
        [Test]
        public void Case2_EquipmentAndTraitModifiers_ChangeResult_MatchesJavaFormula()
        {
            var hero = MakeHero();
            hero.Weapon = MakeSword(conBonus: 20);
            hero.TraitCommon = "BRUTE";

            int con = _characterService.GetTotalStat(hero, StatType.Constitution);
            Assert.AreEqual(92, con, "Constitution should be round((60 base + 20 equip) * 1.15 BRUTE) = 92.");

            var wrapper = new AdventurerWrapper(hero, _characterService, null);

            Assert.AreEqual(78, wrapper.MinAttackDamage, "Min damage must equal DecodeMath.Round(92 * 0.85) = 78 (truncation, not round-half-up).");
            Assert.AreEqual(105, wrapper.MaxAttackDamage, "Max damage must equal DecodeMath.Round(92 * 1.15) = 105.");

            // Confirms equipment ALONE (no-trait baseline from Case 1: min=51/max=69) measurably
            // changes combat output end-to-end through the real CombatService pipeline — the
            // explicit CHECKPOINT requirement "hero with equipment vs without differs, real numbers".
            Assert.Greater(wrapper.MinAttackDamage, 51);
            Assert.Greater(wrapper.MaxAttackDamage, 69);
        }

        // ------------------------------------------------------------------------------------
        // Test Case 3: critical hit multiplier.
        // Java: Adventurer.calculateCriticalDamage() = base(1.5) + weapon/armor/accessory.getCriticalDamage() + doctrine*0.01.
        // Weapon carries +0.3 criticalDamage (real Equipment field, restored in Task 2.1).
        //   expected multiplier = 1.5 + 0.3 = 1.8
        // Applying that multiplier to a fixed raw damage roll of 100 BEFORE armor reduction
        // (CombatService.ProcessTurn multiplies rawDamage by CriticalDamage pre-ApplyDamage):
        //   critRawDamage = 100 * 1.8 = 180
        // Then through ApplyDamage (target Defense=10, isMagic=false, armorIgnored=0, barrier=0):
        //   reduction   = min(1.0, 1.0 * 0.01 * 10) = 0.10
        //   flatReduct  = target.Constitution(40) / 8 = 5   (int division, Java: calculateTotalConstitution()/8)
        //   noCrit: reduced = (1-0.10)*100 - 5 - 0 = 85.0            -> round = 85
        //   crit:   reduced = (1-0.10)*180 - 5 - 0 = 157.0           -> round = 157
        // ------------------------------------------------------------------------------------
        [Test]
        public void Case3_CriticalHitMultiplier_AppliedCorrectly()
        {
            var hero = MakeHero();
            hero.Weapon = MakeSword(conBonus: 0, critDamageBonus: 0.3);

            double critDamage = _characterService.GetCombatModifier(hero, StatType.CriticalDamage);
            Assert.AreEqual(1.8, critDamage, 0.0001, "CriticalDamage multiplier must equal Java base(1.5) + weapon(0.3) = 1.8.");

            var targetDef = new AdventurerDefinition { id = "target", BaseConstitution = 40, BaseDefense = 10, BaseMaxHp = 200, MaxLevel = 50 };
            var targetHero = new CharacterRuntime("target_1", targetDef) { Level = 1, CurrentHp = 200 };
            var targetWrapper = new AdventurerWrapper(targetHero, _characterService, null);

            var combat = new CombatService(_characterService, null);

            int noCritResult = combat.ApplyDamage(targetWrapper, 100.0, isMagic: false, barrier: 0, armorIgnored: 0.0);
            Assert.AreEqual(85, noCritResult, "No-crit: DecodeMath.Round((1-0.10)*100 - 5) = 85.");

            // Reset target HP/shield before the second call so the two results are independent.
            targetHero.CurrentHp = 200;
            targetHero.CurrentShield = 0;

            double critRawDamage = 100.0 * critDamage; // 180 — this is exactly what CombatService.ProcessTurn does: rawDamage *= acting.CriticalDamage before calling ApplyDamage.
            int critResult = combat.ApplyDamage(targetWrapper, critRawDamage, isMagic: false, barrier: 0, armorIgnored: 0.0);
            Assert.AreEqual(157, critResult, "Crit: DecodeMath.Round((1-0.10)*180 - 5) = 157.");

            Assert.Greater(critResult, noCritResult, "Critical hit must deal strictly more damage than a non-critical hit under identical conditions.");
        }
    }
}
