using System;

namespace GuildMaster.Definitions
{
    [Serializable]
    public class SkillDefinition : DefinitionBase
    {
        public string NameKey { get; set; }
        public string DescriptionKey { get; set; }

        // manualRuleRequired: Cooldown, Cost, Level, TargetRule, DamageFormula
        // deferredToS3Combat
    }
}
