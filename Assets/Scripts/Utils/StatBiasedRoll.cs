using Game.Progression;
using UnityEngine;

namespace Game.Utils
{
    // Rolls a value within a range, skewed toward the high end as chance grows.
    // Use a range that itself straddles the desired outcomes (e.g. -10..10) to
    // combine "chance of a good result" and "chance of a bad result" in one roll.
    public static class StatBiasedRoll
    {
        public static float Roll(Vector2 range, float baseChance, PlayerProgression playerProgression, StatType statType = StatType.Luck)
        {
            if (range.y < range.x)
            {
                Debug.LogError($"[StatBiasedRoll] Invalid range ({range.x}, {range.y}): range.y must be >= range.x. Swapping to recover.");
                (range.x, range.y) = (range.y, range.x);
            }

            float chanceScore = Mathf.Max(0f, baseChance + GetStat(playerProgression, statType));

            // Exponent shrinks toward 0 as chanceScore grows, skewing a uniform roll toward range.y.
            float exponent = 1f / (1f + chanceScore / 100f);
            float t = Mathf.Pow(Random.value, exponent);

            return Mathf.Floor(Mathf.Lerp(range.x, range.y, t));
        }

        private static int GetStat(PlayerProgression playerProgression, StatType statType)
        {
            if (playerProgression == null)
            {
                return 0;
            }

            return playerProgression.GetAllCurrentStats().TryGetValue(statType, out int value) ? value : 0;
        }
    }
}
