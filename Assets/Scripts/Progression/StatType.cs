using UnityEngine;

namespace Game.Progression {
    public enum StatType 
    {
        MaxHealth, 
        MeleeDamage, 
        AttackSpeed, 
        MoveSpeed, 
        DashCooldown, 
        GraceAffinity, 
        Armor, 
        ExperienceToLevelUpReduction,
        Luck,
        RangedDamage,
        Range,
        Pierce, // was PierceAmount (saves map the old name via StatTypeNames)
        MagicDamage,
        Cleave,
        HealthRegen,
        LifeSteal, 
        SkillDuration,
        HealingReceived,
        PickupRange,
        MastowAffinity,
        ShopItemDiscount,
        Retribution,
        Evasion,
    }

}