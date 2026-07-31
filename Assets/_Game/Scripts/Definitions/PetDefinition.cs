using System;

namespace GuildMaster.Definitions
{
    [Serializable]
    public class PetDefinition : DefinitionBase
    {
        public string PetName = "Unnamed Pet";
        public int BaseAttack;
        public int BaseDefense;
        public int BaseMaxHp = 50;
        public int BaseSpeed = 10;
        public float AttackMultiplier = 1.0f;
        public float DefenseMultiplier = 1.0f;
        public float HpMultiplier = 1.0f;
        public float SpeedMultiplier = 1.0f;
        public int ExpToLevel = 100;
        public string SkillDefinitionId = "";
        public string EvolutionDefinitionId = "";
        public int EvolutionLevel = 0;
        public string VisualPrefab = "";
    }
}
