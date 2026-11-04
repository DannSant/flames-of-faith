using UnityEngine;

namespace Game.Feel
{
    /// <summary>Pure rules of the hit feel moments, with their tunables passed in.</summary>
    public static class GameFeelRules
    {
        /// <summary>A hit is heavy when it takes at least <paramref name="fraction"/> of max health.</summary>
        public static bool IsHeavyHit(float damage, float maxHealth, float fraction)
        {
            if (maxHealth <= 0f || damage <= 0f) return false;
            return damage >= maxHealth * fraction;
        }

        /// <summary>Whether a moment last played at <paramref name="lastPlayed"/> may play at <paramref name="now"/>.</summary>
        public static bool IsOffCooldown(float lastPlayed, float now, float cooldown)
        {
            return now - lastPlayed >= cooldown;
        }

        /// <summary>A shake or motor strength times the player's Settings slider (0 sends nothing).</summary>
        public static float Scaled(float strength, float slider)
        {
            return Mathf.Max(0f, strength) * Mathf.Clamp01(slider);
        }
    }
}
