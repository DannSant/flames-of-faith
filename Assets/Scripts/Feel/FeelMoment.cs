using System;
using UnityEngine;

namespace Game.Feel
{
    /// <summary>
    /// The only moments that get hit feel. Never routine hits or kills: with auto-attack, Attack Speed and the
    /// Whirlwind they would never stop. Each has one row on GameFeelSettings.
    /// </summary>
    public enum FeelMoment
    {
        HeavyHit,
        PlayerDeath,
        CorruptorKilled,
        BossPhaseTwo,
        BossKilled,
    }

    /// <summary>What a moment does: shake, hit stop, slow motion and rumble, and how often it may fire.</summary>
    [Serializable]
    public class FeelMomentRow
    {
        public FeelMoment moment;
        [Tooltip("Camera shake strength (world units at its peak). 0 = no shake.")]
        [Min(0f)] public float shakeForce;
        [Tooltip("How long the shake lasts (seconds).")]
        [Min(0f)] public float shakeDuration = 0.2f;
        [Tooltip("Hit stop: real-time seconds the game nearly freezes. 0 = none.")]
        [Min(0f)] public float hitStop;
        [Tooltip("Slow motion time scale (0.4 = 40% speed). 1 = none.")]
        [Range(0.05f, 1f)] public float slowMotionScale = 1f;
        [Tooltip("Real-time seconds of slow motion. 0 = none.")]
        [Min(0f)] public float slowMotionDuration;
        [Tooltip("Gamepad low-frequency (heavy) motor, 0..1.")]
        [Range(0f, 1f)] public float rumbleLow;
        [Tooltip("Gamepad high-frequency (light) motor, 0..1.")]
        [Range(0f, 1f)] public float rumbleHigh;
        [Tooltip("Real-time seconds the motors run. 0 = no rumble.")]
        [Min(0f)] public float rumbleDuration;
        [Tooltip("Seconds (real time) before this moment can fire again, so a burst can't chain.")]
        [Min(0f)] public float cooldown = 0.5f;
    }
}
