using System;
using GuildMaster.Definitions;

namespace GuildMaster.Runtime.Models
{
    public class SkillRuntime
    {
        public string Id { get; set; }
        // manualRuleRequired: CooldownLeft
        // deferredToS3Combat
        
        public SkillRuntime(string id)
        {
            Id = id;
        }
    }
}
