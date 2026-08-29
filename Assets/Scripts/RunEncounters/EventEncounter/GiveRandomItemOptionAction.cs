using Game.Effects;
using Game.Items;
using Game.Progression;
using Game.Utils;
using System.Collections.Generic;
using System.Linq;
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
        [SerializeField] private float qualityScalePerStat = 2f;

        public List<ItemOptionEntry> PossibleItems => possibleItems;

        public EffectQuality GetMaxEligibleQuality(PlayerProgression playerProgression)
        {
            int statValue = playerProgression != null ? playerProgression.GetStatTotal(biasStat) : 0;
            return EffectQualityScaler.GetMaxEligibleQuality(statValue, qualityScalePerStat);
        }

        public override string Apply(EventContext context)
        {
            var itemBag = context.playerEffectStore.GetComponent<ItemBag>();

            if(itemBag == null)
            {
                Debug.LogWarning("ItemBag component not found on player.");
                return string.Empty;
            }

            EffectQuality maxQuality = GetMaxEligibleQuality(context.playerProgression);
            var eligibleItems = possibleItems.Where(entry => entry.item != null && entry.item.Quality <= maxQuality).ToList();

            if (eligibleItems.Count == 0)
            {
                return string.Empty;
            }

            float totalWeight = 0f;
            foreach (var entry in eligibleItems)
            {
                totalWeight += entry.weight;
            }
            float randomValue = Random.Range(0f, totalWeight);
            float cumulativeWeight = 0f;
            foreach (var entry in eligibleItems)
            {
                cumulativeWeight += entry.weight;
                if (randomValue <= cumulativeWeight)
                {
                    //context.playerEffectStore.AddEffect(entry.item);
                    itemBag.AddEffectItem(entry.item);
                    return $"Found item: {entry.item.effectName}";
                }
            }

            return string.Empty;
        }
    }
}