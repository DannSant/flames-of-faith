using System.Collections.Generic;
using Game.Progression;
using UnityEngine;

namespace Game.Combat
{
    public static class DamageTypeColorHelper
    {
        private const string ConfigResourcePath = "Combat/DamageTypeColorConfig";

        private static readonly Dictionary<string, StatType> metaTagMap = new()
        {
            { "_melee_damage_", StatType.MeleeDamage },
            { "_ranged_damage_", StatType.RangedDamage },
            { "_magic_damage_", StatType.MagicDamage },
        };

        private static readonly Dictionary<StatType, WeaponClass> damageStatToWeaponClass = new()
        {
            { StatType.MeleeDamage, WeaponClass.Melee },
            { StatType.RangedDamage, WeaponClass.Ranged },
            { StatType.MagicDamage, WeaponClass.Magic },
        };

        private static DamageTypeColorConfig config;
        private static bool configLoadAttempted;

        private static DamageTypeColorConfig Config
        {
            get
            {
                if (!configLoadAttempted)
                {
                    configLoadAttempted = true;
                    config = Resources.Load<DamageTypeColorConfig>(ConfigResourcePath);
                    if (config == null)
                    {
                        Debug.LogWarning($"DamageTypeColorConfig not found at Resources/{ConfigResourcePath}. Falling back to default colors.");
                    }
                }
                return config;
            }
        }

        public static Color GetColor(WeaponClass damageType)
        {
            if (Config != null)
            {
                return Config.GetColor(damageType);
            }

            return damageType switch
            {
                WeaponClass.Melee => new Color32(0xE7, 0x4C, 0x3C, 0xFF),
                WeaponClass.Ranged => new Color32(0x2E, 0xCC, 0x71, 0xFF),
                WeaponClass.Magic => new Color32(0x93, 0x59, 0xE8, 0xFF),
                _ => Color.white
            };
        }

        public static string GetHex(WeaponClass damageType) => ColorUtility.ToHtmlStringRGB(GetColor(damageType));

        /// <summary>
        /// True if this stat is one of the three damage-type stats (Melee/Ranged/Magic Damage)
        /// that should be color-coded to match its damage number color.
        /// </summary>
        public static bool TryGetWeaponClass(StatType statType, out WeaponClass weaponClass) =>
            damageStatToWeaponClass.TryGetValue(statType, out weaponClass);

        public static string ColorizeMetaTags(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            foreach (var tagEntry in metaTagMap)
            {
                if (!text.Contains(tagEntry.Key))
                {
                    continue;
                }

                WeaponClass weaponClass = damageStatToWeaponClass[tagEntry.Value];
                string label = StatDisplayNameHelper.GetDisplayName(tagEntry.Value);
                string replacement = $"<color=#{GetHex(weaponClass)}>{label}</color>";
                text = text.Replace(tagEntry.Key, replacement);
            }

            return text;
        }
    }
}
