using System.Collections.Generic;
using GuildMaster.Definitions.Enums;
using GuildMaster.Runtime.Models;

namespace GuildMaster.Runtime.Services
{
    public interface ICombatService
    {
        CombatResult ProcessTurn(List<CharacterRuntime> adventurers, List<EnemyRuntime> enemies, out string nextActingEntityId);
    }
}
