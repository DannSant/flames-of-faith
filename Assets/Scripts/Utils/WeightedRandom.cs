using System.Collections.Generic;
using System;
using UnityEngine;

namespace Game.Utils
{
    public static class WeightedRandom 
    {
        public static T Roll<T>(
        IList<T> items,
        Func<T, float> weightSelector,
        System.Random rng)
        {
            float totalWeight = 0f;

            foreach (var item in items)
            {
                float w = weightSelector(item);
                if (w > 0)
                    totalWeight += w;
            }

            if (totalWeight <= 0f)
            {
                Debug.LogError("[WeightedRandom] Total weight is zero.");
                return default;
            }

            double roll = rng.NextDouble() * totalWeight;
            float cumulative = 0f;

            foreach (var item in items)
            {
                float w = weightSelector(item);
                if (w <= 0)
                    continue;

                cumulative += w;
                if (roll <= cumulative)
                    return item;
            }

            // Fallback (floating point safety)
            return items[^1];
        }
    }

}