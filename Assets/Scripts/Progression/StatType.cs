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
        Cleave, // retired (Oct 2026): nothing grants or reads it; kept so the stats stored by index don't shift
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