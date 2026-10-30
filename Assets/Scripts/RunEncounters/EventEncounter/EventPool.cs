using System.Collections.Generic;

namespace Game.RunEncounters
{
    /// <summary>
    /// Picks the event for an event node. Unseen events first; once every event has been seen, any event except
    /// a once-per-Pilgrimage one that was. The pick is deterministic from the run seed and the node id, so leaving
    /// a level (Save &amp; Exit, a crash) and coming back gives the same event instead of a re-roll. Pure.
    /// </summary>
    public static class EventPool
    {
        /// <summary>The id an event is remembered by in the run's seen list.</summary>
        public static string IdOf(EventEncounterData data) =>
            data == null ? null : string.IsNullOrWhiteSpace(data.eventName) ? data.name : data.eventName;

        public static EventEncounterData Pick(IReadOnlyList<EventEncounterData> pool, ICollection<string> seen, int seed, string nodeId)
        {
            var candidates = new List<EventEncounterData>();
            if (pool == null) return null;

            foreach (var e in pool)
            {
                if (e != null && (seen == null || !seen.Contains(IdOf(e)))) candidates.Add(e);
            }

            if (candidates.Count == 0)
            {
                foreach (var e in pool)
                {
                    if (e != null && !e.oncePerPilgrimage) candidates.Add(e);
                }
            }

            if (candidates.Count == 0) return null;

            uint hash = StableHash($"{seed}:{nodeId}");
            return candidates[(int)(hash % (uint)candidates.Count)];
        }

        /// <summary>FNV-1a: the same on every platform and run, unlike string.GetHashCode.</summary>
        public static uint StableHash(string text)
        {
            uint hash = 2166136261;
            if (text == null) return hash;
            foreach (char c in text)
            {
                hash ^= c;
                hash *= 16777619;
            }
            return hash;
        }
    }
}
