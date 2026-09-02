using System.Collections.Generic;
using UnityEngine;

namespace Game.Combat
{
    public static class DamageTypeColorHelper
    {
        private const string ConfigResourcePath = "Combat/DamageTypeColorConfig";

        private static readonly Dictionary<string, WeaponClass> metaTagMap = new()
        {
            { "_melee_damage_", WeaponClass.Melee },
            { "_ranged_damage_", WeaponClass.Ranged },
            { "_magic_damage_", WeaponClass.Magic },
        };

        private static readonly Dictionary<WeaponClass, string> displayLabels = new()
        {
            { WeaponClass.Melee, "Melee Damage" },
            { WeaponClass.Ranged, "Ranged Damage" },
            { WeaponClass.Magic, "Magic Damage" },
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

                string label = displayLabels[tagEntry.Value];
                string replacement = $"<color=#{GetHex(tagEntry.Value)}>{label}</color>";
                text = text.Replace(tagEntry.Key, replacement);
            }

            return text;
        }
    }
}
