using Game.Scene;
using UnityEngine;

namespace Game.Feel
{
    /// <summary>
    /// Pure rules of the low-health warning: when it is on, how strong it is and how it pulses.
    /// Health is passed as a share of max health; the tunables come from <see cref="GameFeelSettings"/>.
    /// </summary>
    public static class LowHealthRules
    {
        /// <summary>Current health as a share of max health; 0 when max is 0 or less.</summary>
        public static float HealthFraction(float current, float max)
        {
            if (max <= 0f) return 0f;
            return Mathf.Clamp01(current / max);
        }

        /// <summary>
        /// Whether the warning is on. It turns on at or below <paramref name="enter"/> and, once on,
        /// stays on until health rises above <paramref name="exit"/>. Never on at 0 health: the death screen takes over.
        /// </summary>
        public static bool IsActive(bool wasActive, float fraction, float enter, float exit)
        {
            if (fraction <= 0f) return false;
            return wasActive ? fraction <= Mathf.Max(enter, exit) : fraction <= enter;
        }

        /// <summary>
        /// 0..1 strength of an active warning: <paramref name="minIntensity"/> at the enter threshold
        /// (and above it, while the warning holds until the exit threshold), rising to 1 at <paramref name="fullAt"/>.
        /// </summary>
        public static float Intensity(float fraction, float enter, float fullAt, float minIntensity)
        {
            if (enter <= fullAt) return fraction <= enter ? 1f : minIntensity;
            float t = Mathf.InverseLerp(enter, fullAt, fraction);
            return Mathf.Lerp(minIntensity, 1f, t);
        }

        /// <summary>Pulses per second for an intensity: faster when stronger.</summary>
        public static float PulseRate(float intensity, float rateMin, float rateMax)
        {
            return Mathf.Lerp(rateMin, rateMax, Mathf.Clamp01(intensity));
        }

        /// <summary>
        /// Vignette alpha at a point of the pulse (<paramref name="phase"/> in beats; the top of a beat is at whole numbers).
        /// It swings between <paramref name="floor"/> × peak and the peak, where peak = intensity × maxAlpha × visibility.
        /// </summary>
        public static float PulseAlpha(float phase, float intensity, float maxAlpha, float floor, float visibility)
        {
            float wave = 0.5f + 0.5f * Mathf.Cos(phase * 2f * Mathf.PI);
            float peak = Mathf.Clamp01(intensity) * maxAlpha * Mathf.Clamp01(visibility);
            return peak * Mathf.Lerp(Mathf.Clamp01(floor), 1f, wave);
        }

        /// <summary>The warning only shows where the player fights: combat and boss levels.</summary>
        public static bool ShowsInLevel(LevelType type)
        {
            return type == LevelType.Combat || type == LevelType.Boss;
        }
    }
}
