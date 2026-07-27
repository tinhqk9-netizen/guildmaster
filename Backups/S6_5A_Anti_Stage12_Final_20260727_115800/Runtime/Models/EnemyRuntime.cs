using System;
using System.Collections.Generic;
using GuildMaster.Definitions;

namespace GuildMaster.Runtime.Models
{
    public class EnemyRuntime
    {
        public string InstanceId { get; set; }
        public string DefinitionId { get; set; }
        public EnemyDefinition Definition { get; set; }
        
        public int CurrentHp { get; set; }
        public int CurrentMana { get; set; }
        public int CurrentShield { get; set; }

        public List<StatusEffectRuntime> PositiveStatusEffects { get; set; } = new List<StatusEffectRuntime>();
        public List<StatusEffectRuntime> NegativeStatusEffects { get; set; } = new List<StatusEffectRuntime>();

        public string ActiveSkillId { get; set; }
        public string PassiveSkillId { get; set; }
        
        public bool IsDead => CurrentHp <= 0;

        public EnemyRuntime(string instanceId, EnemyDefinition definition)
        {
            InstanceId = instanceId;
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            DefinitionId = definition.id;
            
            CurrentHp = definition.BaseMaxHp;
            CurrentMana = 0;
            CurrentShield = 0;
            ActiveSkillId = definition.ActiveSkillId;
            PassiveSkillId = definition.PassiveSkillId;
        }
    }
}
