using System;
using System.Collections.Generic;
using GuildMaster.Definitions;

namespace GuildMaster.Runtime.Models
{
    public class CharacterRuntime
    {
        public string InstanceId { get; set; }
        public string DefinitionId { get; set; }
        public AdventurerDefinition Definition { get; set; }
        public int Level { get; set; } = 1;
        public int Experience { get; set; } = 0;
        public ItemRuntime Weapon { get; set; }
        public ItemRuntime Armor { get; set; }
        public ItemRuntime Accessory { get; set; }

        public int CurrentHp { get; set; }
        public int CurrentMana { get; set; }
        public int CurrentShield { get; set; }

        public List<StatusEffectRuntime> PositiveStatusEffects { get; set; } = new List<StatusEffectRuntime>();
        public List<StatusEffectRuntime> NegativeStatusEffects { get; set; } = new List<StatusEffectRuntime>();

        // Java: Adventurer.ascended. See CharacterSaveData.IsAscended for the full explanation of
        // what this does and doesn't mean. Phase 1 removed the fabricated AscensionLevel counter
        // that used to stand in for real class promotion (see PromotionService.cs).
        public bool IsAscended { get; set; }

        // Java: Adventurer.traitCommon / traitRare — two independent slots a hero can hold at
        // once (Phase 1). Trait is kept only as a legacy display alias for old callers.
        public string TraitCommon { get; set; } = string.Empty;
        public string TraitRare { get; set; } = string.Empty;
        public string Trait { get; set; } = string.Empty;
        public int[] PotionsDrank { get; set; } = new int[6];

        public string ActiveSkillId { get; set; }
        public string PassiveSkillId { get; set; }

        public CharacterRuntime(string instanceId, AdventurerDefinition definition)
        {
            InstanceId = instanceId;
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            
            Level = 1;
            Experience = 0;
            // CurrentHp will be initialized properly when Combat/Status systems are integrated
            CurrentHp = 100; 
        }
    }
}
