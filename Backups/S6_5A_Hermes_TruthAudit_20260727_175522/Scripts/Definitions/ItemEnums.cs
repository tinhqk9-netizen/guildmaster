namespace GuildMaster.Definitions
{
    public enum ItemCategory
    {
        None,
        Weapon,
        Armor,
        Accessory,
        Consumable,
        Material // Maps to Upgrade and basic Items
    }

    public enum EquipmentSlot
    {
        Weapon,
        Armor,
        Accessory
    }

    public enum StatType
    {
        MaxHp,
        Constitution,
        Intelligence,
        Dexterity,
        Defense,
        MagicDefense,
        Threat,
        Counterattack,
        Lifesteal,
        BonusExperience,
        DarknessReduction,
        DarknessDamageAmplification,
        RetaliationPhysicalDamage,
        RetaliationMagicalDamage,
        HealingModifier,
        ImmunityToStatus,
        Regeneration,
        CriticalDamage,
        CriticalChance,
        FlatDodgeChance,
        Decay,
        ExaltInspireBonusTurns,
        OnFireBonusDamage,
        FreezeBonusDamage,
        PoisonBonus,
        LivingCompanionBonusDamage,
        RegenerationBonus
    }
}
