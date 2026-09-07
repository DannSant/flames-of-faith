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
        [SerializeField] private bool enabled = false;
        [Range(0f, 1f)]
        [SerializeField] private float chance = 0.35f;
        [SerializeField] private float cooldown = 0.75f; // seconds between spread attempts from one affected target
        [SerializeField] private int maxGenerations = 3; // how many hops away from the original application
        [Range(0f, 1f)]
        [SerializeField] private float durationFalloff = 0.8f;
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
