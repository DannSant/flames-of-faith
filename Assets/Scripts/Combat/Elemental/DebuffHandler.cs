using Game.Misc;
using UnityEngine;

namespace Game.Combat.Elemental
{
    public class DebuffHandler : MonoBehaviour
    {
        // Keeps track of active debuffs (1 per type, no stacking)
        private readonly System.Collections.Generic.Dictionary<ElementalType, DebuffBase> activeDebuffs
            = new System.Collections.Generic.Dictionary<ElementalType, DebuffBase>();

        public bool TryToApplyDebuff(ElementalDebuffData debuffData, int debuffStrengthStat)
        {
            float chance = debuffData.ChanceToApply;
            if (UnityEngine.Random.value > chance)
            {
                return false; // Debuff application failed
            }
          
            float duration = debuffData.BaseDuration + debuffStrengthStat * debuffData.DurationStatScale;
            float strength = debuffData.BaseStrength + debuffStrengthStat * debuffData.StrengthStatScale;
            
            ApplyDebuff(debuffData.ElementalType, duration, strength);

            // Optionally instantiate VFX here using debuffData.VFX
            if(debuffData.VFX != null)
            {
                var vfx = Instantiate(debuffData.VFX, transform.position, Quaternion.identity, transform);
                if(vfx.TryGetComponent<DestroyAfterTime>(out var autoDestroy))
                {
                    autoDestroy.StartDestroyTimer(duration);
                } 
                
            }
            return true;
        }

        /// <summary>
        /// Applies or refreshes the debuff for the given element type.
        /// </summary>
        public void ApplyDebuff(ElementalType type, float duration, float strength)
        {
            if (type == ElementalType.None)
                return;

            // if debuff already exists → refresh it instead of stacking
            if (activeDebuffs.TryGetValue(type, out DebuffBase existing))
            {
                existing.Initialize(duration, strength);
                return;
            }

            // Create new debuff component depending on element
            DebuffBase newDebuff = null;

            switch (type)
            {
                case ElementalType.Fire:
                    newDebuff = gameObject.AddComponent<DebuffFire>();
                    break;

                /*case ElementalType.Frost:
                    newDebuff = gameObject.AddComponent<DebuffFrost>();
                    break;

                case ElementalType.Chaos:
                    newDebuff = gameObject.AddComponent<DebuffChaos>();
                    break;

                case ElementalType.Energy:
                    newDebuff = gameObject.AddComponent<DebuffEnergy>();
                    break;

                case ElementalType.Holy:
                    newDebuff = gameObject.AddComponent<DebuffHoly>();
                    break;*/
            }

            if (newDebuff != null)
            {
                newDebuff.Initialize(duration, strength);
                activeDebuffs[type] = newDebuff;
            }
        }
    }

}