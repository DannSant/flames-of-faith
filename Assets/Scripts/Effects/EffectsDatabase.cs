
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Effects
{
    [CreateAssetMenu(fileName = "EffectsDatabase", menuName = "Effects/EffectsDatabase")]
    [Obsolete("EffectsDatabase is deprecated, replaced by SQLite database")]
    public class EffectsDatabase : ScriptableObject
    {
        public List<Effect> availableEffects;
        public List<Effect> unlockableEffects;

        private static EffectsDatabase _instance;

        /// <summary>
        /// Loads the singleton instance from Resources if not already loaded.
        /// </summary>
        private static EffectsDatabase Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<EffectsDatabase>("Effects/EffectsDatabase");
                    if (_instance == null)
                        Debug.LogError("EffectsDatabase not found! Make sure it is placed at 'Resources/Effects/EffectsDatabase'.");
                }
                return _instance;
            }
        }

        /// <summary>
        /// Searches for an effect only in the availableEffects list.
        /// </summary>
        public static Effect GetAvailableEffectById(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;

            return Instance.availableEffects.FirstOrDefault(e => e != null && e.EffectID == id);
        }

        /// <summary>
        /// Searches for an effect in both availableEffects and unlockableEffects lists.
        /// </summary>
        public static Effect GetEffectById(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;

            // Check available effects first
            Effect found = Instance.availableEffects.FirstOrDefault(e => e != null && e.EffectID == id);
            if (found != null)
                return found;

            // Fallback: search unlockable effects
            return Instance.unlockableEffects.FirstOrDefault(e => e != null && e.EffectID == id);
        }
    }

}