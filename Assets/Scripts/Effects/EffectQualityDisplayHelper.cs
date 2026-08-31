using UnityEngine;

namespace Game.Effects
{
    public static class EffectQualityDisplayHelper
    {
        public static Color GetQualityColor(EffectQuality quality)
        {
            return quality switch
            {
                EffectQuality.Common => new Color32(0x9D, 0x9D, 0x9D, 0xFF),
                EffectQuality.Uncommon => new Color32(0xFF, 0xFF, 0xFF, 0xFF),
                EffectQuality.Rare => new Color32(0x2E, 0xCC, 0x71, 0xFF),
                EffectQuality.Heroic => new Color32(0x33, 0x99, 0xFF, 0xFF),
                EffectQuality.Epic => new Color32(0xA3, 0x35, 0xEE, 0xFF),
                EffectQuality.Legendary => new Color32(0xFF, 0xD7, 0x00, 0xFF),
                _ => Color.white
            };
        }
    }
}
