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

        public override void Apply(GameObject config, EffectStore effectStore) 
        { 
            base.Apply(config, effectStore);
            var playerProgression = config.GetComponent<PlayerProgression>();
            AddStatModifierToPlayerProgression(playerProgression);
        }

        public override void UpdateEffect(GameObject config)
        {
            base.UpdateEffect(config);
            var playerProgression = config.GetComponent<PlayerProgression>();
            AddStatModifierToPlayerProgression(playerProgression);
        }

        private void AddStatModifierToPlayerProgression(PlayerProgression playerProgression)
        {
           
            if (playerProgression == null)
            {
                Debug.LogWarning("StatModifierEffect: PlayerProgression component not found on the target GameObject.");
                return;
            }

            foreach (var modifier in statModifiers)
            {
                playerProgression.UpdateExtraStat(modifier.statType, modifier.value);
            }
        }

    }
}
