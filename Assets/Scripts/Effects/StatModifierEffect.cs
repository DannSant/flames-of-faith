using Game.Progression;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.STP;

namespace Game.Effects
{
    [CreateAssetMenu(fileName = "StatModifierEffect", menuName = "Effects/StatModifierEffect")]
    public class StatModifierEffect : Effect
    {
        [SerializeField] private List<StatValuePair> statModifiers = new ();
        [SerializeField] private ModifierType modifierType = ModifierType.Flat;

        public override List<StatModifier> StatModifiers
        {

            get
            {
                List<StatModifier> list = new();

                foreach (var pair in statModifiers)
                {
                    // Default: flat modifier
                    list.Add(new StatModifier
                    {
                        stat = pair.statType,
                        type = modifierType,
                        value = pair.value
                    });
                }

                return list;
            }
        }

    }
}
