using System.Collections.Generic;
using UnityEngine;

namespace Game.Feel
{
    /// <summary>One hit stop or slow motion: the time scale it asks for, until a real-time moment.</summary>
    public struct TimeDilationRequest
    {
        public float scale;
        public float endTime;

        public TimeDilationRequest(float scale, float endTime)
        {
            this.scale = scale;
            this.endTime = endTime;
        }
    }

    /// <summary>
    /// Pure rules of the time scale: the lowest active request wins, a request ends at its real-time end
    /// (also while paused, so resuming after it gives 1), and a pause always means 0.
    /// </summary>
    public static class TimeDilationRules
    {
        /// <summary>The scale the active requests ask for: the lowest one, clamped to 0..1; 1 when none is active.</summary>
        public static float CurrentScale(IReadOnlyList<TimeDilationRequest> requests, float now)
        {
            float scale = 1f;
            if (requests == null) return scale;
            foreach (var request in requests)
            {
                if (request.endTime > now) scale = Mathf.Min(scale, Mathf.Clamp01(request.scale));
            }
            return scale;
        }

        /// <summary>Drops the requests that have ended; returns how many were removed.</summary>
        public static int RemoveExpired(List<TimeDilationRequest> requests, float now)
        {
            return requests == null ? 0 : requests.RemoveAll(r => r.endTime <= now);
        }

        /// <summary>The time scale to apply: 0 while paused, else the dilation.</summary>
        public static float TimeScale(bool paused, float dilation)
        {
            return paused ? 0f : Mathf.Clamp01(dilation);
        }
    }
}
