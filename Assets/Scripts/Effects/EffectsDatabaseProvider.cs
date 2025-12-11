using Game.Common;
using Game.Metaprogression;
using Game.Scene;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Effects
{
    public class EffectsDatabaseProvider : Singleton<EffectsDatabaseProvider>, IMetaProgressionStateLoader
    {
     

        private List<Effect> availableEffects;
        private List<Effect> unlockableEffects;

        private List<string> unlockedEffects = null;

        protected override void Awake()
        {
            base.Awake();          
           
        }

        public void Initialize()
        {
            //Build the effect lists from the database, this is the default state

            BuildEffectListsFromDatabase();
            //If this is null, it means we are starting fresh and need to build the lists from the SO database
            if (unlockedEffects == null)
            {
                unlockedEffects = new List<string>();                
            }else
            {
                BuildListFromProgression();
            }
        }

        private void BuildEffectListsFromDatabase()
        {
            var allEfects = Resources.LoadAll<Effect>("Effects").ToList(); ;

            availableEffects = allEfects.FindAll(Effect => Effect.UnlockedByDefault);
            unlockableEffects = allEfects.FindAll(Effect => !Effect.UnlockedByDefault);
        }

        private void BuildListFromProgression() {
            foreach (var effectID in unlockedEffects)
            {
                UnlockEffect(effectID);
            }
        }

        public List<Effect> GetAvailableEffects()
        {
            return availableEffects;
        }

        public void UnlockEffect(string effectId)
        {
            if(availableEffects==null || unlockableEffects == null)
            {
                Debug.LogError("Effects lists are not initialized.");
                return;
            }

            var effectToUnlock = unlockableEffects.Find(e => e.EffectID == effectId);
            availableEffects.Add(effectToUnlock);

            // Add to unlocked effects list for persistence if not already present
            if (!unlockedEffects.Contains(effectId))
            {
                unlockedEffects.Add(effectId);
            }
        }

        public static Effect GetAvailableEffectById(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;

            return Instance.availableEffects.FirstOrDefault(e => e != null && e.EffectID == id);
        }

        public void LoadState(MetaState state)
        {
            //Initialize state from meta progression. This will be null if no progression has been made yet.
            unlockedEffects = state.unlockedEffects;          
        }

        public void SaveState()
        {
            MetaProgressionManager.Instance.SaveUnlockedEffects(unlockedEffects);
        }

        public void ResetState()
        {
            unlockedEffects = null;
        }

    }
}