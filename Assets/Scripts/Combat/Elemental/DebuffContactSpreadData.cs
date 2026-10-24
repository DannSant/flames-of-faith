using UnityEngine;

namespace Game.Combat.Elemental
{
    /// <summary>
    /// Contact propagation config for a debuff. Authored inline on <see cref="ElementalDebuffData"/>.
    /// When enabled, an affected target spreads the debuff to other targets it physically collides with.
    /// </summary>
    [System.Serializable]
    public class DebuffContactSpreadData
    {
        [Tooltip("An affected enemy spreads the debuff to enemies it touches.")]
        [SerializeField] private bool enabled = false;
        [Tooltip("Chance (0 to 1) that a touch spreads the debuff.")]
        [Range(0f, 1f)]
        [SerializeField] private float chance = 0.35f;
        [Tooltip("Seconds between spread attempts from one affected enemy.")]
        [SerializeField] private float cooldown = 0.75f; // seconds between spread attempts from one affected target
        [Tooltip("How many hops away from the first enemy it can spread.")]
        [SerializeField] private int maxGenerations = 3; // how many hops away from the original application
        [Tooltip("Each hop keeps this share of the duration (0.8 = 80%).")]
        [Range(0f, 1f)]
        [SerializeField] private float durationFalloff = 0.8f;
        [Tooltip("Each hop keeps this share of the strength (0.8 = 80%).")]
        [Range(0f, 1f)]
        [SerializeField] private float strengthFalloff = 0.8f;

        public bool Enabled => enabled;
        public float Chance => chance;
        public float Cooldown => cooldown;
        public int MaxGenerations => maxGenerations;
        public float DurationFalloff => durationFalloff;
        public float StrengthFalloff => strengthFalloff;
    }

}
