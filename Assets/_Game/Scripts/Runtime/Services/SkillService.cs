using System;
using System.Linq;
using GuildMaster.Database;
using GuildMaster.Definitions;
using GuildMaster.Runtime.Models;

namespace GuildMaster.Runtime.Services
{
    /// <summary>
    /// Java ground truth: Skills.java is a pure 227-value enum (ACTIVE_*/PASSIVE_*), no combat
    /// data of its own — all skill behavior is hardcoded in Area.java/Entity.java switch blocks
    /// (Combat, Phase 2, out of scope here). Each AdventurerDefinition declares its class's
    /// ActiveSkill/PassiveSkill as the raw Skills enum constant name (e.g. "ACTIVE_ENERGY_BURST_I",
    /// "PASSIVE_NONE") — restored in Phase 1 by extracting every unit's configureStatistics().
    /// skills.json ids are the lowercased enum name (e.g. "active_energy_burst_i").
    /// </summary>
    public interface ISkillService
    {
        SkillRuntime CreateSkill(string id, SkillDefinition definition);

        /// <summary>Looks up a SkillDefinition from an AdventurerDefinition.ActiveSkill/PassiveSkill enum-constant value (case-insensitive; "PASSIVE_NONE"/"ACTIVE_NONE"/empty return null).</summary>
        SkillDefinition GetByEnumConstant(string skillsEnumConstant);

        bool IsActiveSkill(string skillsEnumConstant);
        bool IsPassiveSkill(string skillsEnumConstant);
    }

    public class SkillService : ISkillService
    {
        private readonly GameDatabase _database;

        public SkillService(GameDatabase database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
        }

        public SkillRuntime CreateSkill(string id, SkillDefinition definition)
        {
            return new SkillRuntime(id);
        }

        public SkillDefinition GetByEnumConstant(string skillsEnumConstant)
        {
            if (string.IsNullOrEmpty(skillsEnumConstant)) return null;
            if (skillsEnumConstant.EndsWith("_NONE", StringComparison.OrdinalIgnoreCase)) return null;

            string id = skillsEnumConstant.ToLowerInvariant();
            _database.TryGet<SkillDefinition>(id, out var def);
            if (def != null) return def;

            // Fall back to className match in case casing/format ever diverges from id.
            return _database.GetAll<SkillDefinition>()
                .FirstOrDefault(s => string.Equals(s.className, skillsEnumConstant, StringComparison.OrdinalIgnoreCase));
        }

        public bool IsActiveSkill(string skillsEnumConstant) =>
            !string.IsNullOrEmpty(skillsEnumConstant) && skillsEnumConstant.StartsWith("ACTIVE_", StringComparison.OrdinalIgnoreCase);

        public bool IsPassiveSkill(string skillsEnumConstant) =>
            !string.IsNullOrEmpty(skillsEnumConstant) && skillsEnumConstant.StartsWith("PASSIVE_", StringComparison.OrdinalIgnoreCase);
    }
}
