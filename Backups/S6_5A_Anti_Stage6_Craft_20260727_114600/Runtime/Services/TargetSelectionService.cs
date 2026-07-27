using System;
using System.Collections.Generic;
using System.Linq;

namespace GuildMaster.Runtime.Services
{
    public class TargetSelectionService : ITargetSelectionService
    {
        private readonly Random _random = new Random();

        public string GetAttackTargetStrategy(ICombatEntityWrapper acting)
        {
            if (acting == null) return "random_enemy";
            // Recovered Rule TS-01
            return "random_enemy";
        }

        public List<ICombatEntityWrapper> SelectTargets(ICombatEntityWrapper acting, string strategy, List<ICombatEntityWrapper> allEntities)
        {
            var results = new List<ICombatEntityWrapper>();
            if (acting == null || allEntities == null || allEntities.Count == 0) return results;

            bool isAdventurer = acting.IsAdventurer;
            var allies = allEntities.Where(e => e.IsAdventurer == isAdventurer && e.CurrentHp > 0).ToList();
            var enemies = allEntities.Where(e => e.IsAdventurer != isAdventurer && e.CurrentHp > 0).ToList();

            switch (strategy)
            {
                case "all":
                    results.AddRange(allEntities.Where(e => e.CurrentHp > 0));
                    break;
                case "all_allies":
                    results.AddRange(allies);
                    break;
                case "all_enemies":
                    results.AddRange(enemies);
                    break;
                case "all_except_self":
                    results.AddRange(allEntities.Where(e => e.Id != acting.Id && e.CurrentHp > 0));
                    break;
                case "lowest_absolute_ally":
                    var lowestAbsAlly = allies.OrderBy(a => a.CurrentHp).FirstOrDefault();
                    if (lowestAbsAlly != null) results.Add(lowestAbsAlly);
                    break;
                case "lowest_absolute_enemy":
                    var lowestAbsEnemy = enemies.OrderBy(e => e.CurrentHp).FirstOrDefault();
                    if (lowestAbsEnemy != null) results.Add(lowestAbsEnemy);
                    break;
                case "lowest_relative_ally":
                    var lowestRelAlly = allies.OrderBy(a => (double)a.CurrentHp / Math.Max(1, a.MaxHp)).FirstOrDefault();
                    if (lowestRelAlly != null) results.Add(lowestRelAlly);
                    break;
                case "lowest_relative_enemy":
                    var lowestRelEnemy = enemies.OrderBy(e => (double)e.CurrentHp / Math.Max(1, e.MaxHp)).FirstOrDefault();
                    if (lowestRelEnemy != null) results.Add(lowestRelEnemy);
                    break;
                case "lowest_shield_ally":
                    var lowestShieldAlly = allies.OrderBy(a => a.CurrentShield).FirstOrDefault();
                    if (lowestShieldAlly != null) results.Add(lowestShieldAlly);
                    break;
                case "random_ally":
                    if (allies.Count > 0) results.Add(allies[_random.Next(allies.Count)]);
                    break;
                case "random_ally_except_self":
                    var alliesExceptSelf = allies.Where(a => a.Id != acting.Id).ToList();
                    if (alliesExceptSelf.Count > 0) results.Add(alliesExceptSelf[_random.Next(alliesExceptSelf.Count)]);
                    break;
                case "random_enemy":
                default:
                    if (enemies.Count > 0) results.Add(enemies[_random.Next(enemies.Count)]);
                    break;
            }

            return results;
        }
    }
}
