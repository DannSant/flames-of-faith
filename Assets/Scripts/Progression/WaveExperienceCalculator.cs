using Game.Waves;
using UnityEngine;

namespace Game.Progression
{
    public struct WaveExperienceEstimate
    {
        public float ExpectedSpawns;
        public float ExpectedDrops;
        public float TargetXp;
        public float BaseXp;
    }

    /// <summary>
    /// Splits the XP a wave should give across the drops the wave is expected to produce,
    /// and rolls the denomination of each dropped token.
    /// </summary>
    public static class WaveExperienceCalculator
    {
        /// <param name="experienceReductionPoints">Total of the ExperienceToLevelUpReduction stat.</param>
        public static WaveExperienceEstimate CalculateBaseXp(WaveData waveData, int playerLevel, float experienceReductionPoints,
            PlayerExperience playerExperience, ExperienceSettings settings)
        {
            float expectedSpawns;
            if (waveData.expectedEnemyCountOverride > 0)
            {
                expectedSpawns = waveData.expectedEnemyCountOverride;
            }
            else
            {
                float blendedCooldown = Mathf.Lerp(waveData.regularCooldown, waveData.longCooldown, settings.CooldownBlend);
                expectedSpawns = waveData.waveDuration / Mathf.Max(0.1f, blendedCooldown);
            }

            float expectedDrops = Mathf.Max(1f, expectedSpawns * settings.DropChance * settings.ExpectedKillRatio);
            // A flat bonus per point: the same extra share of a level at every level, unlike bending the XP curve,
            // which grows exponentially with level
            float reductionBonus = 1f + Mathf.Max(0f, experienceReductionPoints) * settings.XpBonusPerReductionPoint;
            float targetXp = playerExperience.GetXPRequired(playerLevel) * settings.LevelsPerWaveTarget * reductionBonus;

            return new WaveExperienceEstimate
            {
                ExpectedSpawns = expectedSpawns,
                ExpectedDrops = expectedDrops,
                TargetXp = targetXp,
                BaseXp = targetXp / expectedDrops
            };
        }

        /// <summary>
        /// Starts at tier 1 and keeps promoting up to the enemy's tier, stopping at the first failed roll.
        /// </summary>
        public static ExperienceTokenDenomination RollDenomination(int enemyTier, ExperienceSettings settings)
        {
            var result = settings.GetDenomination(1);
            for (int tier = 2; tier <= enemyTier; tier++)
            {
                var next = settings.GetDenomination(tier);
                if (next == null || Random.value > next.promoteChance)
                {
                    break;
                }
                result = next;
            }
            return result;
        }
    }
}
