using GuildMaster.Definitions;
using GuildMaster.Runtime.Models;

namespace GuildMaster.Runtime.Services
{
    public interface ISkillService
    {
        SkillRuntime CreateSkill(string id, SkillDefinition definition);
    }

    public class SkillService : ISkillService
    {
        public SkillRuntime CreateSkill(string id, SkillDefinition definition)
        {
            return new SkillRuntime(id);
        }
    }
}
