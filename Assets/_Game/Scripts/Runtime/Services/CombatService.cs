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
        private readonly Random _random = new Random();

        public CombatResult ProcessTurn(List<CharacterRuntime> adventurers, List<EnemyRuntime> enemies, out string nextActingEntityId)
        {
            nextActingEntityId = null;

            if (adventurers.All(a => a.CurrentHp <= 0)) return CombatResult.Defeat;
            if (enemies.All(e => e.IsDead)) return CombatResult.Victory;

            var allEntities = new List<ICombatEntityWrapper>();
            allEntities.AddRange(adventurers.Where(a => a.CurrentHp > 0).Select(a => new AdventurerWrapper(a)));
            allEntities.AddRange(enemies.Where(e => !e.IsDead).Select(e => new EnemyWrapper(e)));

            var sorted = allEntities.OrderByDescending(e => e.IsInitiative).ThenByDescending(e => e.Dexterity).ToList();
            var acting = sorted.First();
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

            // Target opposite team
            ICombatEntityWrapper target = null;
            if (acting.IsAdventurer)
                target = sorted.FirstOrDefault(e => !e.IsAdventurer && e.CurrentHp > 0);
            else
                target = sorted.FirstOrDefault(e => e.IsAdventurer && e.CurrentHp > 0);

            if (target != null)
            {
                double rawDamage = RollAttackDamage(acting);
                ApplyDamage(target, rawDamage, acting.IsMagic, 0, 0.0);
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

            int defStat = isMagic ? target.MagicDefense : target.Defense;
            double reduction = Math.Min(1.0, (1.0 - armorIgnored) * 0.01 * defStat);

            int flatReduction = target.Constitution / 8; // Flat damage reduction rule

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

        /// <summary>Attack range feeding <c>rollAttackDamage()</c>.</summary>
        int MinAttackDamage { get; }
        int MaxAttackDamage { get; }
        bool IsMagic { get; }
        bool RollsDamageThreeTimes { get; }

        /// <summary>Experience this entity awards when killed (enemies only).</summary>
        int ExpGiven { get; }
    }

    public class AdventurerWrapper : ICombatEntityWrapper
    {
        private CharacterRuntime _character;
        public AdventurerWrapper(CharacterRuntime character) => _character = character;
        
        public string Id => _character.InstanceId;
        public bool IsAdventurer => true;
        public int CurrentHp { get => _character.CurrentHp; set => _character.CurrentHp = value; }
        public int CurrentMana { get => _character.CurrentMana; set => _character.CurrentMana = value; }
        public int CurrentShield { get => _character.CurrentShield; set => _character.CurrentShield = value; }
        
        public int MaxHp => _character.Definition.BaseMaxHp;
        public int Regeneration => 1;
        public int ManaRegen => 10;
        public int Dexterity => _character.Definition.BaseDexterity;
        public int Constitution => _character.Definition.BaseConstitution;
        public int Defense => _character.Definition.BaseDefense;
        public int MagicDefense => _character.Definition.BaseMagicDefense;
        public bool IsInitiative => false;
        public string ActiveSkillId => _character.ActiveSkillId;

        // Adventurer damage comes from the equipped weapon in the decode. Until the weapon
        // damage-modifier port lands, an unarmed adventurer deals the decode's unarmed
        // minimum of 1 (Adventurer.calculateMinAttackDamage returns 1 when weapon == null).
        public int MinAttackDamage => 1;
        public int MaxAttackDamage => 1;
        public bool IsMagic => false;
        public bool RollsDamageThreeTimes => false;
        public int ExpGiven => 0;
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

        public int MinAttackDamage => _enemy.Definition.MinDamage;
        public int MaxAttackDamage => _enemy.Definition.MaxDamage;
        public bool IsMagic => _enemy.Definition.IsMagic;
        public bool RollsDamageThreeTimes => false;
        public int ExpGiven => _enemy.Definition.ExpGiven;
    }
}
