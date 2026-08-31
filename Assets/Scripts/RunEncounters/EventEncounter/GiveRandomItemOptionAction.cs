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

        public EffectQuality GetExpectedQuality(PlayerProgression playerProgression)
        {
            int statValue = playerProgression != null ? playerProgression.GetStatTotal(biasStat) : 0;
            return EffectQualityScaler.GetExpectedQualityTier(statValue, qualityScalePerStat);
        }

        public override string Apply(EventContext context)
        {
            var itemBag = context.playerEffectStore.GetComponent<ItemBag>();

            if(itemBag == null)
            {
                Debug.LogWarning("ItemBag component not found on player.");
                return string.Empty;
            }

            if (possibleItems.Count == 0)
            {
                return string.Empty;
            }

            int statValue = context.playerProgression != null ? context.playerProgression.GetStatTotal(biasStat) : 0;
            var pool = possibleItems.Where(entry => entry.item != null).Select(entry => entry.item);
            EffectQuality rolledQuality = EffectQualityScaler.RollQualityTier(statValue, qualityScalePerStat, pool);

            var tierEntries = possibleItems.Where(entry => entry.item != null && entry.item.Quality == rolledQuality).ToList();
            if (tierEntries.Count == 0)
            {
                return string.Empty;
            }

            float totalWeight = 0f;
            foreach (var entry in tierEntries)
            {
                totalWeight += entry.weight;
            }
            float randomValue = Random.Range(0f, totalWeight);
            float cumulativeWeight = 0f;
            foreach (var entry in tierEntries)
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