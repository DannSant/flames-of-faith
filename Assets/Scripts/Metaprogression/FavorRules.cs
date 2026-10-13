using System.Collections.Generic;

namespace Game.Metaprogression
{
    /// <summary>Pure Favor math, shared by gameplay and tests.</summary>
    public static class FavorRules
    {
        /// <summary>Favors for a finished wave: only when Grace ends the wave above 0.</summary>
        public static int CalculateWaveFavors(float graceAfterWave, int favorsPerPositiveGraceWave)
        {
            return graceAfterWave > 0f ? System.Math.Max(0, favorsPerPositiveGraceWave) : 0;
        }

        /// <summary>
        /// Favors a class can spend: its own, or every class's together when the pool is shared.
        /// </summary>
        public static int CalculateSpendable(IReadOnlyDictionary<string, int> favorsByClass, string characterClass, bool sharedPool)
        {
            if (favorsByClass == null) return 0;
            if (!sharedPool)
            {
                return favorsByClass.TryGetValue(characterClass, out int own) ? own : 0;
            }

            int total = 0;
            foreach (var amount in favorsByClass.Values) total += amount;
            return total;
        }

        /// <summary>
        /// Takes <paramref name="cost"/> Favors. From a shared pool it takes from the spending class
        /// first, then from the others in order. Returns false (and changes nothing) if there aren't enough.
        /// </summary>
        public static bool TrySpend(Dictionary<string, int> favorsByClass, string characterClass, int cost, bool sharedPool)
        {
            if (favorsByClass == null || cost < 0) return false;
            if (CalculateSpendable(favorsByClass, characterClass, sharedPool) < cost) return false;

            int remaining = cost;
            remaining = TakeFrom(favorsByClass, characterClass, remaining);
            if (sharedPool)
            {
                foreach (var key in new List<string>(favorsByClass.Keys))
                {
                    if (remaining == 0) break;
                    if (key == characterClass) continue;
                    remaining = TakeFrom(favorsByClass, key, remaining);
                }
            }
            return true;
        }

        private static int TakeFrom(Dictionary<string, int> favorsByClass, string key, int remaining)
        {
            if (remaining <= 0 || !favorsByClass.TryGetValue(key, out int available)) return remaining;
            int taken = System.Math.Min(available, remaining);
            favorsByClass[key] = available - taken;
            return remaining - taken;
        }
    }
}
