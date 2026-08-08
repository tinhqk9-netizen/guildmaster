using System;

namespace GuildMaster.Definitions
{
    [Serializable]
    public class PetDefinition : DefinitionBase
    {
        // ── Legacy-accurate fields (Phase 0 restore) ────────────────────────────────────
        // Java: 7 abstract pet families (Avian/Construct/Esoteric/Insect/Reptile/Wild/Wooden),
        // each with 3 tiers (1=Common 75%, 2=Uncommon 20%, 3=Rare 5%). Pets in Legacy have no
        // combat stats — they're an Aura/Modifier assigned to a Dungeon Area, not a fighting
        // unit. See Docs/Backend_Audit/pets_audit.md.
        public string PetFamily = "";
        public int PetTier;
        public string IdName = "";
        public string IdImage = "";
        // Java: guaranteedFirstAbility — the PetAbility id granted at level 1.
        public string GuaranteedFirstAbility = "";
        // Java: abilityNumber — how many of the pet's abilities are active at its current tier.
        public int AbilityNumber;

        // ── LEGACY-INCORRECT: kept only for compile compatibility ──────────────────────
        // These fields do not exist in Java (Pet has no HP/Attack/Defense/Speed and no
        // evolution system — confirmed in pets_audit.md §2, §6). They are NOT removed here
        // because PetService.cs (gameplay logic, out of scope for this task) depends on them.
        // A future Pet-system rebuild should delete these once PetService/PetRuntime are
        // rewritten against the correct "Aura assigned to a Dungeon" architecture.
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
