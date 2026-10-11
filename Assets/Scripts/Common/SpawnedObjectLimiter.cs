using System.Collections.Generic;
using UnityEngine;

namespace Game.Common
{
    /// <summary>
    /// Keeps at most N spawned objects alive, oldest first out (e.g. the Archer's dash traps).
    /// Objects destroyed by other means (a trap that went off) are dropped from the count, so
    /// they don't take up a slot.
    /// </summary>
    public class SpawnedObjectLimiter
    {
        private readonly LinkedList<GameObject> alive = new();
        private int maxCount;

        public SpawnedObjectLimiter(int maxCount)
        {
            SetMaxCount(maxCount);
        }

        /// <summary>How many tracked objects are still alive.</summary>
        public int Count
        {
            get
            {
                Prune();
                return alive.Count;
            }
        }

        public void SetMaxCount(int value)
        {
            maxCount = Mathf.Max(1, value);
        }

        /// <summary>
        /// Tracks a newly spawned object and returns the ones that must now be removed to stay
        /// within the limit, oldest first. The caller destroys them.
        /// </summary>
        public List<GameObject> Register(GameObject spawned)
        {
            Prune();
            var evicted = new List<GameObject>();
            if (spawned == null) return evicted;

            alive.AddLast(spawned);
            while (alive.Count > maxCount)
            {
                evicted.Add(alive.First.Value);
                alive.RemoveFirst();
            }
            return evicted;
        }

        public void Clear()
        {
            alive.Clear();
        }

        private void Prune()
        {
            var node = alive.First;
            while (node != null)
            {
                var next = node.Next;
                // Unity's null: true for destroyed objects too.
                if (node.Value == null) alive.Remove(node);
                node = next;
            }
        }
    }
}
