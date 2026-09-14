using System.Collections.Generic;
using UnityEngine;

namespace Game.Effects
{
    /// <summary> What a <see cref="StackScalingRule"/> scales when an effect is stacked. </summary>
    public enum StackScalingTarget
    {
        SpawnCount,
        Damage,
        Bounces,
        Lifetime,
        Speed,
        Pierce
    }

    public enum StackScalingMode
    {
        /// <summary> 1 + perStack * extraStacks - a flat share added per extra copy. </summary>
        Additive,
        /// <summary> (1 + perStack) ^ extraStacks - compounds, so late stacks are worth more. </summary>
        Multiplicative
    }

    /// <summary>
    /// One authored scaling rule on an <see cref="EffectBehavior"/>: "each extra copy of this
    /// relic scales X by Y". Authored inline on the behavior asset, mirroring
    /// <see cref="Game.Combat.Elemental.DebuffStackData"/>.
    ///
    /// Every rule resolves to 1 (identity) at a single unstacked copy, so a component can apply
    /// the result unconditionally without checking whether the effect is stacked at all.
    /// </summary>
    [System.Serializable]
    public class StackScalingRule
    {
        [SerializeField] private StackScalingTarget target = StackScalingTarget.Damage;
        [Tooltip("Magnitude contributed by each copy BEYOND the first. 0.25 additive = +25% per extra copy.")]
        [SerializeField] private float perStack = 1f;
        [SerializeField] private StackScalingMode mode = StackScalingMode.Additive;
        [Tooltip("Upper bound on the resulting multiplier. 0 = uncapped.")]
        [SerializeField] private float maxTotal = 0f;

        public StackScalingTarget Target => target;

        public float Resolve(int stackCount)
        {
            int extraStacks = Mathf.Max(0, stackCount - 1);

            float result = mode == StackScalingMode.Multiplicative
                ? Mathf.Pow(1f + perStack, extraStacks)
                : 1f + perStack * extraStacks;

            if (maxTotal > 0f)
            {
                result = Mathf.Min(result, maxTotal);
            }

            return result;
        }
    }

    /// <summary>
    /// A behavior's rule list resolved against the live stack count, handed to each spawned object.
    /// Querying a target nothing scales returns 1, so consumers never branch on whether a rule exists.
    /// </summary>
    public readonly struct StackScaling
    {
        public static readonly StackScaling Identity = new StackScaling(null, 1);

        private readonly List<StackScalingRule> rules;
        private readonly int stackCount;

        public StackScaling(List<StackScalingRule> rules, int stackCount)
        {
            this.rules = rules;
            this.stackCount = stackCount;
        }

        public int StackCount => stackCount;

        public float Get(StackScalingTarget target)
        {
            if (rules == null)
            {
                return 1f;
            }

            for (int i = 0; i < rules.Count; i++)
            {
                if (rules[i] != null && rules[i].Target == target)
                {
                    return rules[i].Resolve(stackCount);
                }
            }

            return 1f;
        }
    }
}
