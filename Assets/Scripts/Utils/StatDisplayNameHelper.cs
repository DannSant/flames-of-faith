using Game.Progression;
using UnityEngine;

public static class StatDisplayNameHelper
{
    public static string GetDisplayName(StatType statType)
    {
        return statType switch
        {
            StatType.MaxHealth => "Health",
            StatType.MeleeDamage => "Melee Damage",
            StatType.AttackSpeed => "Attack Speed",
            StatType.MoveSpeed => "Move Speed",
            StatType.DashCooldown => "Dash Recovery speed",
            StatType.MaxGrace => "Max Grace",
            StatType.Armor => "Armor",
            StatType.ExperienceToLevelUpReduction => "Experience Reduction",
            StatType.Luck => "Luck",
            StatType.RangedDamage => "Ranged Damage",
            StatType.MagicDamage => "Magic Damage",
            StatType.PierceAmount => "Pierce Targets",
            StatType.HealthRegen => "Health Regen",
            StatType.LifeSteal => "Life Steal",
            StatType.SkillDuration => "Skill Duration",
            StatType.HealingReceived => "Healing Received",
            StatType.PickupRange => "Pickup Range",
            StatType.Range => "Attack Range",
            StatType.MastowAffinity => "Mastow Affinity",
            _ => statType.ToString()
        };
    }
}
