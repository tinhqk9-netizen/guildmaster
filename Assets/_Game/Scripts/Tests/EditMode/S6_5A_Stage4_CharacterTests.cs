using System;
using NUnit.Framework;
using GuildMaster.Database;
using GuildMaster.Definitions;
using GuildMaster.Infrastructure.DataProviders;
using GuildMaster.Infrastructure.Serialization;
using GuildMaster.Runtime.Formulas;
using GuildMaster.Runtime.Models;
using GuildMaster.Runtime.Services;

namespace GuildMaster.Tests.EditMode
{
    [TestFixture]
    public class S6_5A_Stage4_CharacterTests
    {
        private GameDatabase _database;
        private ServiceContainer _container;

        [SetUp]
        public void Setup()
        {
            _database = new GameDatabase();
            var provider = new EditorExternalGameDataProvider();
            var serializer = new UnityJsonSerializer();
            var builder = new DatabaseBuilder(provider, serializer, _database);
            builder.Build();

            _container = new ServiceContainer(_database);
        }

        [Test]
        public void CalculateTotalStat_AscendedMultiplier_AppliesToConIntDexHpNotDefMdef()
        {
            var charService = _container.Character;
            var def = new AdventurerDefinition
            {
                id = "test_adventurer",
                BaseConstitution = 10,
                BaseIntelligence = 10,
                BaseDexterity = 10,
                BaseMaxHp = 100,
                BaseDefense = 10,
                BaseMagicDefense = 10,
                MaxLevel = 100
            };

            var character = new CharacterRuntime("char_1", def)
            {
                IsAscended = true,
                Level = 1
            };

            // CON: (int)(10 * 1.5) = 15
            Assert.AreEqual(15, charService.GetTotalStat(character, StatType.Constitution));
            // INT: (int)(10 * 1.5) = 15
            Assert.AreEqual(15, charService.GetTotalStat(character, StatType.Intelligence));
            // DEX: (int)(10 * 1.5) = 15
            Assert.AreEqual(15, charService.GetTotalStat(character, StatType.Dexterity));
            // HP: (int)((100 + 1 - 1) * 1.5) = 150
            Assert.AreEqual(150, charService.GetTotalStat(character, StatType.MaxHp));

            // DEF & MDEF: NO ascended multiplier! Base = 10
            Assert.AreEqual(10, charService.GetTotalStat(character, StatType.Defense));
            Assert.AreEqual(10, charService.GetTotalStat(character, StatType.MagicDefense));
        }

        [Test]
        public void CalculateTotalStat_PotionMapping_UsesCorrectPotionIndices()
        {
            var charService = _container.Character;
            var def = new AdventurerDefinition
            {
                id = "test_adventurer",
                BaseConstitution = 10,
                BaseIntelligence = 10,
                BaseDexterity = 10,
                BaseMaxHp = 100,
                BaseDefense = 10,
                BaseMagicDefense = 10,
                MaxLevel = 100
            };

            var character = new CharacterRuntime("char_2", def)
            {
                IsAscended = false,
                Level = 1,
                PotionsDrank = new int[] { 1, 2, 3, 4, 5, 6 }
                // potion[0]=1 (CON), potion[1]=2 (DEX), potion[2]=3 (INT), potion[3]=4 (HP *5), potion[4]=5 (DEF), potion[5]=6 (MDEF)
            };

            Assert.AreEqual(10 + 1, charService.GetTotalStat(character, StatType.Constitution));
            Assert.AreEqual(10 + 3, charService.GetTotalStat(character, StatType.Intelligence)); // Potion 2 used for INT!
            Assert.AreEqual(10 + 2, charService.GetTotalStat(character, StatType.Dexterity));    // Potion 1 used for DEX!
            Assert.AreEqual(100 + 4 * 5, charService.GetTotalStat(character, StatType.MaxHp));   // Potion 3 * 5 used for HP!
            Assert.AreEqual(10 + 5, charService.GetTotalStat(character, StatType.Defense));       // Potion 4 used for DEF!
            Assert.AreEqual(10 + 6, charService.GetTotalStat(character, StatType.MagicDefense));  // Potion 5 used for MDEF!
        }

        [Test]
        public void CalculateTotalStat_TraitMultiplier_AppliesTraitTableWithDecodeMathRound()
        {
            var charService = _container.Character;
            var def = new AdventurerDefinition
            {
                id = "test_adventurer",
                BaseConstitution = 10,
                BaseIntelligence = 10,
                BaseDexterity = 10,
                BaseMaxHp = 100,
                MaxLevel = 100
            };

            var character = new CharacterRuntime("char_3", def)
            {
                IsAscended = false,
                Level = 1,
                // Phase 1: trait multiplier now reads TraitCommon (Java has independent
                // traitCommon/traitRare fields; only the common trait affects base stats).
                TraitCommon = "BRUTE" // 1.15 for CON
            };

            // CON: DecodeMath.Round(10 * 1.15) = DecodeMath.Round(11.5) = (int)(11.5 + 0.0001) = 11
            Assert.AreEqual(11, charService.GetTotalStat(character, StatType.Constitution));
        }
    }
}
