using System.Collections.Generic;
using System.Linq;
using Game.Effects;
using UnityEngine;

namespace Game.Utils
{
    // Converts a stat value (typically Luck) into the highest EffectQuality tier
    // eligible to be rewarded/stocked, and filters an Effect pool down to it.
    public static class EffectQualityScaler
    {
        public static EffectQuality GetMaxEligibleQuality(int statValue, float scalePerQualityStep)
        {
            if (scalePerQualityStep <= 0f)
            {
                return EffectQuality.Legendary;
            }

            int maxIndex = Mathf.FloorToInt(statValue / scalePerQualityStep);
            int highestIndex = System.Enum.GetValues(typeof(EffectQuality)).Length - 1;
            return (EffectQuality)Mathf.Clamp(maxIndex, 0, highestIndex);
        }

        public static List<Effect> FilterByMaxQuality(IEnumerable<Effect> effects, EffectQuality maxQuality)
        {
            return effects.Where(e => e != null && e.Quality <= maxQuality).ToList();
        }
    }
}
