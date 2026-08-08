using System;
using System.Collections.Generic;
using System.Linq;
using GuildMaster.Database;
using GuildMaster.Definitions;

namespace GuildMaster.Runtime.Services
{
    /// <summary>
    /// Restores Legacy's trait rolling and lookup (Docs/Backend_Audit/phase1_audit_report.md).
    ///
    /// Java ground truth: Utils.rollCommonTrait()/rollRareTrait() (Utils.java), each an
    /// independent roll — a recruited adventurer can have a common trait AND a rare trait AT
    /// THE SAME TIME (Adventurer.getInstance(class, id, lvl, exp, weapon, armor, accessory,
    /// rollCommonTrait(), rollRareTrait(), ...) passes both). Trait.java has exactly 20 values:
    /// 3 common (BOOKWORM/BRUTE/FERAL, 13.33% each, 40% chance of any) + 3 "_PLUS" premium-common
    /// variants (not rollable via the normal Tavern roll in Java — no rollCommonTraitPlus() call
    /// site exists in Utils.java; they appear only pre-authored on specific NPCs, e.g.
    /// DialogShop.java's "imperialVanguard" units) + 14 rare (1/70 ~=1.4286% each, 20% chance of
    /// any of the 14).
    /// </summary>
    public interface ITraitService
    {
        /// <summary>Java: Utils.rollCommonTrait(). Returns null if no trait rolled (60% of the time).</summary>
        string RollCommonTrait();

        /// <summary>Java: Utils.rollRareTrait(). Returns null if no trait rolled (80% of the time).</summary>
        string RollRareTrait();

        /// <summary>Definition lookup for a trait id, or null if no traits.json-sourced catalog entry exists yet (see phase0_schema_mapping.md §4 — TraitDefinition schema exists, catalog JSON does not).</summary>
        TraitDefinition GetTraitDefinition(string traitId);
    }

    public class TraitService : ITraitService
    {
        private readonly GameDatabase _database;
        private readonly Random _random = new Random();

        public TraitService(GameDatabase database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
        }

        public string RollCommonTrait()
        {
            double rand = _random.NextDouble();
            if (rand < 0.13333333333333333d) return "BOOKWORM";
            if (rand < 0.26666666666666666d) return "BRUTE";
            if (rand < 0.4d) return "FERAL";
            return null;
        }

        public string RollRareTrait()
        {
            double rand = _random.NextDouble();
            if (rand < 0.014285714285714285d) return "EMPATHETIC";
            if (rand < 0.028571428571428574d) return "GIFTED";
            if (rand < 0.04285714285714286d) return "INTIMIDATING";
            if (rand < 0.05714285714285715d) return "FOCUSED";
            if (rand < 0.07142857142857144d) return "DRAGON_BLOOD";
            if (rand < 0.08571428571428572d) return "CURSED";
            if (rand < 0.1d) return "REACTIVE";
            if (rand < 0.1142857142857143d) return "NOCTURNAL";
            if (rand < 0.1285714285714286d) return "MINDFUL";
            if (rand < 0.14285714285714288d) return "TROLL_BLOOD";
            if (rand < 0.15714285714285717d) return "RUTHLESS";
            if (rand < 0.17142857142857143d) return "BLESSED";
            if (rand < 0.18571428571428572d) return "ALERT";
            if (rand < 0.2d) return "NIMBLE";
            return null;
        }

        public TraitDefinition GetTraitDefinition(string traitId)
        {
            if (string.IsNullOrEmpty(traitId)) return null;
            return _database.GetAll<TraitDefinition>()
                .FirstOrDefault(t => string.Equals(t.id, traitId, StringComparison.OrdinalIgnoreCase)
                                    || string.Equals(t.className, traitId, StringComparison.OrdinalIgnoreCase));
        }
    }
}
