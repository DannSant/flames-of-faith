using Game.Progression;
using UnityEngine;

public static class StatDisplayNameHelper
{
    public static string GetDisplayName(StatType statType)
    {
        return statType switch
        {
            StatType.MaxHealth => "Max Health",
            StatType.MeleeDamage => "Melee Damage",
            StatType.AttackSpeed => "Attack Speed",
            StatType.MoveSpeed => "Move Speed",
            StatType.DashCooldown => "Dash Recovery speed",
            StatType.MaxGrace => "Max Grace",
            StatType.Armor => "Armor",
            StatType.ExperienceToLevelUpReduction => "Experience Reduction",
            _ => statType.ToString()
        };
    }
}
