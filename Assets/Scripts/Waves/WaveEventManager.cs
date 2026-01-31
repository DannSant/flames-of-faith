using Game.Common;
using Game.Items;
using Game.Progression;
using Game.Scene;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Waves
{
    public class WaveEventManager : Singleton<WaveEventManager>
    {

        public event Action OnUpgradeOptionsAvailable;
        public event Action OnNoUpgradeAvailable;
        public event Action OnItemBagAvailable;

        private ItemBag playerItemBag;

        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            WaveSpawner.Instance.OnWaveCompleteEnded += HandleWaveComplete;
            playerItemBag = PlayerManager.Instance.GetPlayerComponent<ItemBag>();
        }

        private void OnDestroy()
        {
            if (WaveSpawner.Instance != null)
            {
                WaveSpawner.Instance.OnWaveCompleteEnded -= HandleWaveComplete;
            }
        }

        /// <summary>
        /// This method will be called when a wave is completed and we want to handle the logic that decides which components we want to trigger.
        /// We can also subscribe directly to the WaveSpawner's OnWaveComplete event, but having this intermediary method allows us to have control over the flow on the different components that need to be notified.
        /// </summary>
        public void HandleWaveComplete()
        {          
            // If bag is not empty, trigger item bag event and return
            if (!playerItemBag.IsBagEmpty())
            {
                OnItemBagAvailable?.Invoke();
                return;
            }

            // Otherwise, trigger upgrade options event

            int levelsGained = UpgradeManager.Instance.CalculateLevelsGainedInWave();            
            if (levelsGained > 0)
            {
                UpgradeManager.Instance.RecordLastLevel();
                UpgradeManager.Instance.BuildStatsUpgradeOptions();
                OnUpgradeOptionsAvailable?.Invoke();
            }
            else
            {
                UpgradeManager.Instance.RecordLastLevel();
                UpgradeManager.Instance.OnNoUpgradeOptionAvailable();
                OnNoUpgradeAvailable?.Invoke();
            }
        }


    }
}