using Game.Effects;
using Game.Items;
using Game.Scene;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Pickups
{
    [System.Serializable]
    public struct ItemRandomConfig
    {
        public Effect itemEffect;
        [Range(0f, 100f)]
        public float weight;
    }

    public class RandomItemPickup : BasePickup
    {
        [SerializeField] private List<ItemRandomConfig> possibleItems;      

        private ItemBag itemBag;

        protected override void Start()
        {
            base.Start();
            itemBag = PlayerManager.Instance.GetPlayerComponent<ItemBag>();
        }

        public override bool CanBePickedUp(GameObject picker)
        {
            return true;
        }

        public override void OnPickup(GameObject picker)
        {
            if(itemBag==null)
            {
                Debug.LogWarning("ItemBag component not found on player.");
                return;
            }

            if (possibleItems == null || possibleItems.Count == 0)
            {
                Debug.LogWarning("No available items to pick up.");
                return;
            }

            Effect selected = GetWeightedRandomEffect();
            if (selected == null)
            {
                Debug.LogWarning("RandomItemPickup: Weighted selection returned null.");
                return;
            }

            itemBag.AddEffectItem(selected);
            Destroy(gameObject);

        }

        private Effect GetWeightedRandomEffect()
        {
            float totalWeight = 0f;

            // Sum all valid weights
            foreach (var cfg in possibleItems)
            {
                if (cfg.weight > 0f && cfg.itemEffect != null)
                    totalWeight += cfg.weight;
            }

            if (totalWeight <= 0f)
            {
                Debug.LogWarning("RandomItemPickup: Total weight <= 0.");
                return possibleItems[0].itemEffect;
            }

            float randomValue = Random.value * totalWeight;
            float cumulative = 0f;

            foreach (var cfg in possibleItems)
            {
                if (cfg.weight <= 0f || cfg.itemEffect == null)
                    continue;

                cumulative += cfg.weight;

                if (randomValue <= cumulative)
                    return cfg.itemEffect;
            }

            // Fallback (rare floating-point edge case)
            return possibleItems[possibleItems.Count - 1].itemEffect;
        }
    }

}