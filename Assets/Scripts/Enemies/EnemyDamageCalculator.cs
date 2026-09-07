using UnityEngine;

namespace Game.Enemies
{
    public enum EnemyDamageKind
    {
        Contact,
        Projectile
    }

    /// <summary>
    /// Single source of truth for how enemy damage scales with run progress.
    ///
    /// Damage grows with the wave number within a level and with how many levels have been beaten
    /// in the run. Projectiles carry their own base and per-wave values but deliberately share
    /// damagePerLevel with contact damage.
    ///
    /// Kept free of GameSession/BehaviorContext on purpose - callers pass the progress values in,
    /// so the formula can be reasoned about (and tuned) in one place.
    /// </summary>
    public static class EnemyDamageCalculator
    {
        public static int Calculate(EnemyData enemyData, int waveNumber, int levelsBeaten, EnemyDamageKind kind)
        {
            if (enemyData == null) return 1;

            // Both multipliers are 1-based: wave 1 of the first level already counts as one step
            // of each, so the numbers authored on EnemyData read the way they are named.
            int waveMultiplier = Mathf.Max(1, waveNumber);
            int levelMultiplier = Mathf.Max(1, levelsBeaten + 1);

            bool isProjectile = kind == EnemyDamageKind.Projectile;

            int baseDamage = isProjectile ? enemyData.projectileDamageBase : enemyData.damageBase;
            int perWave = isProjectile ? enemyData.projectileDamagePerWave : enemyData.damagePerWave;

            int total = baseDamage
                        + perWave * waveMultiplier
                        + enemyData.damagePerLevel * levelMultiplier;

            return Mathf.Max(1, total);
        }
    }
}
