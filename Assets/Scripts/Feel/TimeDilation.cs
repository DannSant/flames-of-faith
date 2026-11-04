using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Feel
{
    /// <summary>
    /// Hit stops and slow motions. It only holds the requests: PauseManager is the one place that writes
    /// Time.timeScale, as paused ? 0 : CurrentScale. Durations are real time, and every scene load or unload
    /// clears the requests, so a slow motion never carries over.
    /// </summary>
    public static class TimeDilation
    {
        private static readonly List<TimeDilationRequest> requests = new List<TimeDilationRequest>();

        private static float Now => Time.realtimeSinceStartup;

        /// <summary>The scale the active requests ask for (1 when none).</summary>
        public static float CurrentScale
        {
            get
            {
                TimeDilationRules.RemoveExpired(requests, Now);
                return TimeDilationRules.CurrentScale(requests, Now);
            }
        }

        /// <summary>Asks for <paramref name="scale"/> (0 = hit stop) for <paramref name="duration"/> real-time seconds.</summary>
        public static void Request(float scale, float duration)
        {
            if (duration <= 0f) return;
            requests.Add(new TimeDilationRequest(scale, Now + duration));
        }

        /// <summary>Ends every request now.</summary>
        public static void Clear()
        {
            requests.Clear();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            requests.Clear();
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneUnloaded -= HandleSceneUnloaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
            SceneManager.sceneUnloaded += HandleSceneUnloaded;
        }

        private static void HandleSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode) => Clear();
        private static void HandleSceneUnloaded(UnityEngine.SceneManagement.Scene scene) => Clear();
    }
}
