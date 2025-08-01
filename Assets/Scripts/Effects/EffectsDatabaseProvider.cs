using Game.Common;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Effects
{
    public class EffectsDatabaseProvider : Singleton<EffectsDatabaseProvider>
    {
        [SerializeField] private EffectsDatabase _database;

        private List<Effect> availableEffects;
        private List<Effect> unlockableEffects;

        protected override void Awake()
        {
            base.Awake();
            if (_database == null)
            {
                Debug.LogError("EffectsDatabase is not assigned in EffectsDatabaseProvider.");
                return;
            }
            availableEffects = _database.availableEffects;
            unlockableEffects = _database.unlockableEffects;
        }

        public List<Effect> GetAvailableEffects()
        {
            return availableEffects;
        }

        public void UnlockEffect(Effect effect)
        {
            if(availableEffects==null || unlockableEffects == null)
            {
                Debug.LogError("Effects lists are not initialized.");
                return;
            }

            var effectToUnlock = unlockableEffects.Find(e => e.EffectID == effect.EffectID);
            availableEffects.Add(effectToUnlock);
        }
    }
}