using UnityEngine;

namespace Game.Combat.Elemental
{
    /// <summary>
    /// Stacking config for a debuff. Authored inline on <see cref="ElementalDebuffData"/>.
    /// When enabled, re-applying the debuff adds a stack instead of refreshing its duration.
    /// </summary>
    [System.Serializable]
    public class DebuffStackData
    {
        [SerializeField] private bool enabled = false;
        [SerializeField] private int maxStacks = 5;
        [Tooltip("If false, adding a stack leaves the remaining duration untouched.")]
        [SerializeField] private bool refreshDurationOnStack = false;
        [Tooltip("Damage dealt when the debuff expires, multiplied by the accumulated stacks.")]
        [SerializeField] private float burstDamagePerStack = 5f;
        [Tooltip("One-shot VFX played every time a stack is gained, and again on the final burst.")]
        [SerializeField] private GameObject stackVfx;
        [SerializeField] private float stackVfxLifetime = 0.5f;

        public bool Enabled => enabled;
        public int MaxStacks => maxStacks;
        public bool RefreshDurationOnStack => refreshDurationOnStack;
        public float BurstDamagePerStack => burstDamagePerStack;
        public GameObject StackVfx => stackVfx;
        public float StackVfxLifetime => stackVfxLifetime;
    }

}
