using System;
using System.Collections.Generic;
using System.Linq;
using GuildMaster.Definitions.Enums;
using GuildMaster.Runtime.Formulas;
using GuildMaster.Runtime.Models;

namespace GuildMaster.Runtime.Services
{
    public class CombatService : ICombatService
    {
        private readonly ICharacterService _characterService;
        private readonly IPetService _petService;
        private readonly Random _random = new Random();

        public CombatService(ICharacterService characterService = null, IPetService petService = null)
        {
            _characterService = characterService;
            _petService = petService;
        }

        public CombatResult ProcessTurn(List<CharacterRuntime> adventurers, List<EnemyRuntime> enemies, out string nextActingEntityId)
        {
            nextActingEntityId = null;

            if (adventurers.All(a => a.CurrentHp <= 0)) return CombatResult.Defeat;
            if (enemies.All(e => e.IsDead)) return CombatResult.Victory;

            var allEntities = new List<ICombatEntityWrapper>();
            allEntities.AddRange(adventurers.Where(a => a.CurrentHp > 0).Select(a => new AdventurerWrapper(a, _characterService, _petService)));
            allEntities.AddRange(enemies.Where(e => !e.IsDead).Select(e => new EnemyWrapper(e)));

            // Java decode: Area.processCombat() iterates ALL entities in turn order.
            // Each alive entity acts once per combat round (regen → mana → attack).
            var sorted = allEntities.OrderByDescending(e => e.IsInitiative).ThenByDescending(e => e.Dexterity).ToList();

            foreach (var acting in sorted)
            {
                // Skip dead entities (may have died earlier this round)
                if (acting.CurrentHp <= 0) continue;

                nextActingEntityId = acting.Id;

                // Resolve status (Regen)
                acting.CurrentHp = Math.Min(acting.MaxHp, acting.CurrentHp + acting.Regeneration);
                
                // Increase mana
                if (!string.IsNullOrEmpty(acting.ActiveSkillId))
                {
                    if (acting.CurrentMana >= 100)
                    {
                        acting.CurrentMana = 0;
                    }
                    else
                    {
                        acting.CurrentMana = Math.Min(100, acting.CurrentMana + acting.ManaRegen);
                    }
                }

                // Target opposite team via weighted threat selection (parity with Area.selectEnemyTarget)
                var potentialTargets = sorted.Where(e => e.IsAdventurer != acting.IsAdventurer && e.CurrentHp > 0).ToList();
                ICombatEntityWrapper target = null;
                if (potentialTargets.Count > 0)
                {
                    var weightedPool = new List<ICombatEntityWrapper>();
                    foreach (var p in potentialTargets)
                    {
                        int threat = Math.Max(1, p.Threat);
                        for (int i = 0; i < threat; i++)
                        {
                            weightedPool.Add(p);
                        }
                    }
                    
                    target = weightedPool[_random.Next(weightedPool.Count)];
                }

                if (target != null)
                {
                    double rawDamage = RollAttackDamage(acting);
                    ApplyDamage(target, rawDamage, acting.IsMagic, 0, 0.0);
                }

                // Early termination if combat is decided
                if (adventurers.All(a => a.CurrentHp <= 0)) return CombatResult.Defeat;
                if (enemies.All(e => e.IsDead)) return CombatResult.Victory;
            }

            if (adventurers.All(a => a.CurrentHp <= 0)) return CombatResult.Defeat;
            if (enemies.All(e => e.IsDead)) return CombatResult.Victory;

            return CombatResult.None;
        }

        /// <summary>
        /// Port of <c>Entity.rollAttackDamage()</c>:
        /// <c>min + random() * (max - min)</c>, where a "rolls three times" attacker takes the
        /// best of three rolls.
        ///
        /// Evidence: Entity.rollAttackDamage (DAD) — see
        /// Reports/S6_5A/S6_5A_Claude_DungeonCombatLoot_Audit_Report.md
        /// </summary>
        public double RollAttackDamage(ICombatEntityWrapper attacker)
        {
            if (attacker == null) return 0d;

            int min = attacker.MinAttackDamage;
            int max = attacker.MaxAttackDamage;
            if (max < min) max = min;

            double roll = _random.NextDouble();
            if (attacker.RollsDamageThreeTimes)
            {
                roll = Math.Max(Math.Max(roll, _random.NextDouble()), _random.NextDouble());
            }

            return (roll * (max - min)) + min;
        }

        public int ApplyDamage(ICombatEntityWrapper target, double rawDamage, bool isMagic, int barrier = 0, double armorIgnored = 0.0)
        {
            if (target == null || rawDamage <= 0) return 0;
            if (target.IsAdventurer == false) barrier = 0; // Enemy has no barrier

            // [PARTIAL] Java flat reduction includes Exalt status (+5), DragonBlood (+ MaxLevel/5), Ascended (+ MaxLevel/10).
            // Currently runtime model lacks StatusEffects and Traits. Ported Base + Constitution.
            int defStat = isMagic ? target.MagicDefense : target.Defense;
            double reduction = Math.Min(1.0, (1.0 - armorIgnored) * 0.01 * defStat);

            int flatReduction = target.FlatDamageReduction;

            double reducedDamage = (1.0 - reduction) * rawDamage - flatReduction - barrier;
            int result = DecodeMath.Round(Math.Max(1.0, reducedDamage));

            if (target.CurrentShield >= result)
            {
                target.CurrentShield -= result;
            }
            else
            {
                int leftover = result - target.CurrentShield;
                target.CurrentShield = 0;
                target.CurrentHp = Math.Max(0, target.CurrentHp - leftover);
            }

            return result;
        }
    }

    public interface ICombatEntityWrapper
    {
        string Id { get; }
        bool IsAdventurer { get; }
        int CurrentHp { get; set; }
        int CurrentMana { get; set; }
        int CurrentShield { get; set; }
        int MaxHp { get; }
        int Regeneration { get; }
        int ManaRegen { get; }
        int Dexterity { get; }
        int Constitution { get; }
        int Defense { get; }
        int MagicDefense { get; }
        bool IsInitiative { get; }
        string ActiveSkillId { get; }
        
        int FlatDamageReduction { get; }

        /// <summary>Attack range feeding <c>rollAttackDamage()</c>.</summary>
        int MinAttackDamage { get; }
        int MaxAttackDamage { get; }
        bool IsMagic { get; }
        bool RollsDamageThreeTimes { get; }

        /// <summary>Experience this entity awards when killed (enemies only).</summary>
        int ExpGiven { get; }

        int Threat { get; }
    }

    public class AdventurerWrapper : ICombatEntityWrapper
    {
        private CharacterRuntime _character;
        private ICharacterService _charService;
        private IPetService _petService;
        public AdventurerWrapper(CharacterRuntime character, ICharacterService charService = null, IPetService petService = null)
        {
            _character = character;
            _charService = charService;
            _petService = petService;
        }
        
        public string Id => _character.InstanceId;
        public bool IsAdventurer => true;
        public int CurrentHp { get => _character.CurrentHp; set => _character.CurrentHp = value; }
        public int CurrentMana { get => _character.CurrentMana; set => _character.CurrentMana = value; }
        public int CurrentShield { get => _character.CurrentShield; set => _character.CurrentShield = value; }
        
        public int MaxHp => _charService != null ? _charService.GetTotalStat(_character, GuildMaster.Definitions.StatType.MaxHp) : _character.Definition.BaseMaxHp;
        public int Regeneration => 1;
        public int ManaRegen => 10;
        public int Dexterity => _charService != null ? _charService.GetTotalStat(_character, GuildMaster.Definitions.StatType.Dexterity) : _character.Definition.BaseDexterity;
        public int Constitution => _charService != null ? _charService.GetTotalStat(_character, GuildMaster.Definitions.StatType.Constitution) : _character.Definition.BaseConstitution;
        public int Defense => _charService != null ? _charService.GetTotalStat(_character, GuildMaster.Definitions.StatType.Defense) : _character.Definition.BaseDefense;
        public int MagicDefense => _charService != null ? _charService.GetTotalStat(_character, GuildMaster.Definitions.StatType.MagicDefense) : _character.Definition.BaseMagicDefense;
        public bool IsInitiative => false;
        public string ActiveSkillId => _character.ActiveSkillId;

        public int FlatDamageReduction
        {
            get
            {
                // Entity.java calculateFlatDamageReduction
                int reduction = Constitution / 8;
                // [PARTIAL] Statuses (Exalt +5) missing from runtime.
                // Adventurer.java calculateFlatDamageReduction
                // [PARTIAL] Traits (DragonBlood, Ascended) missing from runtime.
                return reduction;
            }
        }

        public int MinAttackDamage
        {
            get
            {
                var weapon = _character.Weapon;
                if (weapon == null || weapon.Definition == null) return 1;

                double con = Constitution;
                double dex = Dexterity;
                double intel = _charService != null ? _charService.GetTotalStat(_character, GuildMaster.Definitions.StatType.Intelligence) : _character.Definition.BaseIntelligence;

                double mod = 0;
                string parentClass = weapon.Definition.parentClass ?? "";

                switch (parentClass.ToLowerInvariant())
                {
                    case "sword":
                        mod = con * 1.2 + dex * 0.4;
                        break;
                    case "staff":
                        mod = intel * 1.5;
                        break;
                    case "dagger":
                        mod = dex * 1.2 + con * 0.3;
                        break;
                    case "bow":
                        mod = dex * 1.5;
                        break;
                    default:
                        mod = (con + dex + intel) / 2.0;
                        break;
                }

                if (weapon.Definition.id == "serpent_bite")
                {
                    mod *= Threat;
                }

                double delta = 0.2; // 20% default delta
                int minDmg = (int)Math.Round(mod * (1.0 - delta));
                if (_petService != null)
                {
                    minDmg += (int)_petService.GetAttackBonus(_character.InstanceId);
                }
                return Math.Max(1, minDmg);
            }
        }

        public int MaxAttackDamage
        {
            get
            {
                var weapon = _character.Weapon;
                if (weapon == null || weapon.Definition == null) return 1;

                double con = Constitution;
                double dex = Dexterity;
                double intel = _charService != null ? _charService.GetTotalStat(_character, GuildMaster.Definitions.StatType.Intelligence) : _character.Definition.BaseIntelligence;

                double mod = 0;
                string parentClass = weapon.Definition.parentClass ?? "";

                switch (parentClass.ToLowerInvariant())
                {
                    case "sword":
                        mod = con * 1.2 + dex * 0.4;
                        break;
                    case "staff":
                        mod = intel * 1.5;
                        break;
                    case "dagger":
                        mod = dex * 1.2 + con * 0.3;
                        break;
                    case "bow":
                        mod = dex * 1.5;
                        break;
                    default:
                        mod = (con + dex + intel) / 2.0;
                        break;
                }

                if (weapon.Definition.id == "serpent_bite")
                {
                    mod *= Threat;
                }

                double delta = 0.2;
                int maxDmg = (int)Math.Round(mod * (1.0 + delta));
                if (_petService != null)
                {
                    maxDmg += (int)_petService.GetAttackBonus(_character.InstanceId);
                }
                return Math.Max(1, maxDmg);
            }
        }

        public bool IsMagic => false;
        public bool RollsDamageThreeTimes => false;
        public int ExpGiven => 0;
        public int Threat => 5; // Base adventurer threat (from Java Adventurer.java:292)
    }

    public class EnemyWrapper : ICombatEntityWrapper
    {
        private EnemyRuntime _enemy;
        public EnemyWrapper(EnemyRuntime enemy) => _enemy = enemy;

        public string Id => _enemy.InstanceId;
        public bool IsAdventurer => false;
        public int CurrentHp { get => _enemy.CurrentHp; set => _enemy.CurrentHp = value; }
        public int CurrentMana { get => _enemy.CurrentMana; set => _enemy.CurrentMana = value; }
        public int CurrentShield { get => _enemy.CurrentShield; set => _enemy.CurrentShield = value; }

        public int MaxHp => _enemy.Definition.BaseMaxHp;
        public int Regeneration => 0;
        public int ManaRegen => 10;
        public int Dexterity => _enemy.Definition.BaseDexterity;
        public int Constitution => _enemy.Definition.BaseConstitution;
        public int Defense => _enemy.Definition.BaseDefense;
        public int MagicDefense => _enemy.Definition.BaseMagicDefense;
        public bool IsInitiative => false;
        public string ActiveSkillId => _enemy.ActiveSkillId;

        public int FlatDamageReduction
        {
            get
            {
                // Entity.java calculateFlatDamageReduction
                int reduction = Constitution / 8;
                // [PARTIAL] Statuses (Exalt +5) missing from runtime.
                // [PARTIAL] LegateHadrian (+15) and TheExiled (+40) enemy-specific overrides missing since enemy subtypes are unified here.
                return reduction;
            }
        }

        public int MinAttackDamage => _enemy.Definition.MinDamage;
        public int MaxAttackDamage => _enemy.Definition.MaxDamage;
        public bool IsMagic => _enemy.Definition.IsMagic;
        public bool RollsDamageThreeTimes => false;
        public int ExpGiven => _enemy.Definition.ExpGiven;
        public int Threat => 1; // Default enemy threat
    }
}
