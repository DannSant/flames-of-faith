using Game.Combat;
using UnityEngine;

namespace Game.Enemies
{
    public enum EnemyDamageKind
    {
        Contact,
        Projectile
    }

    /// <summary>
    /// Single source of truth for how much damage an enemy attack deals to the player.
    ///
    /// Every attack has a damage tier on EnemyData, and EnemyDamageSettings says how many hits of
    /// each tier a player with the *expected* stats survives at this point of the run:
    ///
    ///   progress   = levelsBeaten + (wave - 1) * waveProgressStep
    ///   expected   = expected max health and armor at that progress (designer curve)
    ///   afterArmor = expected max health / hits to kill for the tier
    ///   raw        = afterArmor / armor multiplier of the expected armor
    ///
    /// PlayerHealth then applies the player's real armor with the same ArmorMitigation math, so a
    /// player with exactly the expected stats dies in exactly that many hits, and one with more
    /// health or armor than expected outscales the enemies. The real player's stats are never
    /// read here on purpose.
    ///
    /// Kept free of GameSession/BehaviorContext on purpose - callers pass the progress values in,
    /// so the formula can be tested and previewed in the Editor.
    /// </summary>
    public static class EnemyDamageCalculator
    {
        public static float Calculate(EnemyData enemyData, int waveNumber, int levelsBeaten, EnemyDamageKind kind)
        {
            return Calculate(enemyData, waveNumber, levelsBeaten, kind, EnemyDamageSettings.Instance);
        }

        public static float Calculate(EnemyData enemyData, int waveNumber, int levelsBeaten, EnemyDamageKind kind,
            EnemyDamageSettings settings)
        {
            if (enemyData == null) return 0f;

            float progress = GetProgress(waveNumber, levelsBeaten, settings);
            return CalculateForTier(enemyData.GetDamageTier(kind), progress, settings);
        }

        /// <summary>
        /// Raw (pre-armor) damage for a tier at a point of the run. Tier 0 or below deals nothing.
        /// </summary>
        public static float CalculateForTier(float tier, float progress, EnemyDamageSettings settings)
        {
            if (tier <= 0f) return 0f;

            ExpectedPlayerStats expected = GetExpectedStats(progress, settings);
            float hits = GetHitsToKill(tier, settings);
            if (hits <= 0f || expected.maxHealth <= 0f) return settings.MinDamage;

            float afterArmor = expected.maxHealth / hits;
            float armorMultiplier = ArmorMitigation.Multiplier(expected.armor,
                settings.ArmorEffectivenessConstant, settings.MinDamagePercent);
            float raw = afterArmor / Mathf.Max(0.0001f, armorMultiplier);

            return Mathf.Max(settings.MinDamage, raw);
        }

        /// <summary>
        /// Run progress in levels: each beaten level counts 1, each wave inside the current level a fraction.
        /// </summary>
        public static float GetProgress(int waveNumber, int levelsBeaten, EnemyDamageSettings settings)
        {
            return Mathf.Max(0, levelsBeaten) + Mathf.Max(0, waveNumber - 1) * settings.WaveProgressStep;
        }

        /// <summary>
        /// Expected player stats at a progress value, interpolated linearly between curve rows
        /// and clamped to the first/last row.
        /// </summary>
        public static ExpectedPlayerStats GetExpectedStats(float progress, EnemyDamageSettings settings)
        {
            var curve = settings.ExpectedCurve;
            if (curve == null || curve.Count == 0) return new ExpectedPlayerStats(progress, 0f, 0f);

            if (progress <= curve[0].levelsBeaten) return WithProgress(curve[0], progress);

            for (int i = 1; i < curve.Count; i++)
            {
                ExpectedPlayerStats from = curve[i - 1];
                ExpectedPlayerStats to = curve[i];
                if (progress > to.levelsBeaten) continue;

                float span = to.levelsBeaten - from.levelsBeaten;
                float t = span > 0f ? (progress - from.levelsBeaten) / span : 1f;
                return new ExpectedPlayerStats(
                    progress,
                    Mathf.Lerp(from.maxHealth, to.maxHealth, t),
                    Mathf.Lerp(from.armor, to.armor, t));
            }

            return WithProgress(curve[curve.Count - 1], progress);
        }

        /// <summary>
        /// Hits a player with the expected stats survives from an attack of this tier.
        /// Fractional tiers interpolate between neighbours, tiers below 1 scale tier 1 down
        /// (tier 0.5 takes twice the hits), tiers above the table are clamped to the last entry.
        /// Returns +infinity for tier 0 or below.
        /// </summary>
        public static float GetHitsToKill(float tier, EnemyDamageSettings settings)
        {
            if (tier <= 0f) return float.PositiveInfinity;

            var table = settings.TierHitsToKill;
            if (table == null || table.Count == 0) return 0f;

            if (tier < 1f) return table[0] / tier;

            float index = tier - 1f;
            if (index >= table.Count - 1) return table[table.Count - 1];

            int lower = Mathf.FloorToInt(index);
            return Mathf.Lerp(table[lower], table[lower + 1], index - lower);
        }

        /// <summary>
        /// How many hits of rawDamage a player with these stats survives before dying, using the
        /// same armor math as PlayerHealth. Returns int.MaxValue when the hit deals nothing.
        /// </summary>
        public static int SimulateHitsToDie(float playerMaxHealth, float playerArmor, float rawDamage,
            EnemyDamageSettings settings)
        {
            float perHit = ArmorMitigation.Apply(rawDamage, playerArmor,
                settings.ArmorEffectivenessConstant, settings.MinDamagePercent);
            if (perHit <= 0f) return int.MaxValue;

            // The small tolerance keeps float error from turning an exact 25.0 into 26 hits.
            return Mathf.Max(1, Mathf.CeilToInt(playerMaxHealth / perHit - 0.0001f));
        }

        private static ExpectedPlayerStats WithProgress(ExpectedPlayerStats stats, float progress)
        {
            return new ExpectedPlayerStats(progress, stats.maxHealth, stats.armor);
        }
    }
}
