using Game.Effects;
using Game.Items;
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
            var itemBag = context.playerEffectStore.GetComponent<ItemBag>();

            if(itemBag == null)
            {
                Debug.LogWarning("ItemBag component not found on player.");
                return;
            }

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
                    //context.playerEffectStore.AddEffect(entry.item);
                    itemBag.AddEffectItem(entry.item);
                    break;
                }
            }
        }
    }
}