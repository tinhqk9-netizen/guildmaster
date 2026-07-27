using System.Collections.Generic;
using GuildMaster.Runtime.Services;

namespace GuildMaster.Runtime.Services
{
    public interface ITargetSelectionService
    {
        string GetAttackTargetStrategy(ICombatEntityWrapper acting);
        List<ICombatEntityWrapper> SelectTargets(ICombatEntityWrapper acting, string strategy, List<ICombatEntityWrapper> allEntities);
    }
}
