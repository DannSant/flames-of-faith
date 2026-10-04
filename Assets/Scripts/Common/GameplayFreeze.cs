using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Common
{
    /// <summary>
    /// A "semi-pause" for short cinematic moments (e.g. the camera focusing on the Corruptor): time keeps running so
    /// animations play, but enemy behaviors flagged to stop, player input and damage to the player are suspended.
    /// Several sources can freeze at once; gameplay resumes once every source has released it.
    /// </summary>
    public static class GameplayFreeze
    {
        private static readonly HashSet<object> sources = new();

        public static bool IsActive => sources.Count > 0;

        public static event Action<bool> OnFreezeChanged;

        public static void Begin(object source)
        {
            if (source == null || !sources.Add(source)) return;
            if (sources.Count == 1)
            {
                OnFreezeChanged?.Invoke(true);
            }
        }

        public static void End(object source)
        {
            if (source == null || !sources.Remove(source)) return;
            if (sources.Count == 0)
            {
                OnFreezeChanged?.Invoke(false);
            }
        }

        // Static state survives scene loads (and play sessions when domain reload is disabled)
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnLoad()
        {
            sources.Clear();
            OnFreezeChanged = null;
        }
    }
}
