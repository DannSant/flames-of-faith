using System.Collections.Generic;
using UnityEngine;

namespace Game.Combat.Elemental
{
    public class DebuffHandler : MonoBehaviour
    {
        // Keeps track of active debuffs (1 per type, no stacking)
        private readonly Dictionary<ElementalType, DebuffBase> activeDebuffs = new Dictionary<ElementalType, DebuffBase>();

        private EnemyHealth enemyHealth;

        private void Awake()
        {
            enemyHealth = GetComponent<EnemyHealth>();
        }

        public bool TryToApplyDebuff(ElementalDebuffData debuffData, int debuffStrengthStat)
        {
            if (debuffData == null) return false;

            float chance = debuffData.ChanceToApply;
            if (Random.value > chance)
            {
                return false; // Debuff application failed
            }

            float duration = debuffData.BaseDuration + debuffStrengthStat * debuffData.DurationStatScale;
            float strength = debuffData.BaseStrength + debuffStrengthStat * debuffData.StrengthStatScale;

            ApplyDebuff(debuffData, duration, strength);
            return true;
        }

        /// <summary>
        /// Applies or refreshes the debuff described by <paramref name="debuffData"/>.
        /// <paramref name="generation"/> is 0 for a direct application and increases with each propagation hop.
        /// </summary>
        public void ApplyDebuff(ElementalDebuffData debuffData, float duration, float strength, int generation = 0)
        {
            if (debuffData == null) return;

            ElementalType type = debuffData.ElementalType;
            if (type == ElementalType.None) return;
            if (enemyHealth != null && enemyHealth.IsDead()) return;

            // if debuff already exists → refresh it instead of stacking.
            // The null check matters: an expired debuff Destroy()s itself, leaving a fake-null entry behind.
            if (activeDebuffs.TryGetValue(type, out DebuffBase existing) && existing != null)
            {
                existing.Initialize(debuffData, duration, strength, generation);
                return;
            }

            // Create new debuff component depending on element
            DebuffBase newDebuff = null;

            switch (type)
            {
                case ElementalType.Fire:
                    newDebuff = gameObject.AddComponent<DebuffFire>();
                    break;

                case ElementalType.Frost:
                    newDebuff = gameObject.AddComponent<DebuffFrost>();
                    break;

                case ElementalType.LeyCharge:
                    newDebuff = gameObject.AddComponent<DebuffLeyCharge>();
                    break;

                /*case ElementalType.Chaos:
                    newDebuff = gameObject.AddComponent<DebuffChaos>();
                    break;

                case ElementalType.Holy:
                    newDebuff = gameObject.AddComponent<DebuffHoly>();
                    break;*/
            }

            if (newDebuff != null)
            {
                newDebuff.Initialize(debuffData, duration, strength, generation);
                activeDebuffs[type] = newDebuff;
            }
        }

        /// <summary>
        /// Called by a debuff right before it destroys itself, so the slot can be reused.
        /// </summary>
        public void NotifyDebuffEnded(DebuffBase debuff)
        {
            if (debuff == null) return;

            ElementalType type = debuff.ElementalType;
            // Only clear the slot if it is still owned by this debuff, never one that already replaced it.
            if (activeDebuffs.TryGetValue(type, out DebuffBase current) && current == debuff)
            {
                activeDebuffs.Remove(type);
            }
        }
    }

}
