using Game.Effects;
using System.Collections.Generic;
using UnityEngine;

namespace Game.RunEncounters
{
    [System.Serializable]
    public class ItemOptionEntry
    {
        public Effect item;
        public float weight = 1f;
    }
    [CreateAssetMenu(menuName = "RunEncounters/EventOptionsActions/GiveRandomItemOptionAction", fileName = "GiveRandomItemOptionAction")]
    public class GiveRandomItemOptionAction : EventOptionActionBase
    {
        [SerializeField] private List<ItemOptionEntry> possibleItems = new();

        public override void Apply(EventContext context)
        {

            float totalWeight = 0f;
            foreach (var entry in possibleItems)
            {
                totalWeight += entry.weight;
            }
            float randomValue = Random.Range(0f, totalWeight);
            float cumulativeWeight = 0f;
            foreach (var entry in possibleItems)
            {
                cumulativeWeight += entry.weight;
                if (randomValue <= cumulativeWeight)
                {
                    context.playerEffectStore.AddEffect(entry.item);
                    break;
                }
            }
        }
    }
}