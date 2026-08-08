using System;

namespace GuildMaster.Definitions
{
    [Serializable]
    public class SkillDefinition : DefinitionBase
    {
        // Phase 0: JsonUtility only binds public fields, never { get; set; } properties, so
        // these were always null regardless of parser output. Converted to fields to fix that.
        public string NameKey;
        public string DescriptionKey;

        // manualRuleRequired: Cooldown, Cost, Level, TargetRule, DamageFormula
        // deferredToS3Combat
    }
}
