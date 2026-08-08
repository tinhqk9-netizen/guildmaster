using System;

namespace GuildMaster.Definitions
{
    /// <summary>
    /// Java: Trait.java (20 hero traits: 3 common + 3 premium-common + 14 rare) and
    /// PetAbility.java (13 pet abilities) — both pure enums, no static data beyond name/
    /// description keys. All stat/combat effects are hardcoded in Adventurer.java/Area.java
    /// (same situation as Skills — see SkillDefinition.cs).
    ///
    /// This class did not exist before Phase 0. CharacterRuntime/SaveData currently collapse
    /// Java's independent traitCommon + traitRare into a single string — see SaveData.cs
    /// (CharacterSaveData.TraitCommon/TraitRare) for the save-side half of this restore.
    ///
    /// No traits.json exists yet to populate this from; see
    /// Docs/Backend_Audit/traits_audit.md and Docs/Backend_Audit/phase0_schema_mapping.md §4.
    /// </summary>
    public enum TraitCategory
    {
        CommonTrait,
        CommonTraitPremium,
        RareTrait,
        PetAbility
    }

    [Serializable]
    public class TraitDefinition : DefinitionBase
    {
        public TraitCategory Category;
        public string NameKey;
        public string DescriptionKey;
    }
}
