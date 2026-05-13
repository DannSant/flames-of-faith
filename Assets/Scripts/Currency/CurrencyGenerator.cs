using Game.Common;
using Game.Scene;
using Game.Waves;
using System;
using UnityEngine;

namespace Game.Currency
{
    public class CurrencyGenerator : Singleton<CurrencyGenerator>, ISceneCleanupHandler
    {
        [SerializeField] private int minCurrencyAmount = 10;
        [SerializeField] private int maxCurrencyAmount = 20;
        [SerializeField] private int currencyPerWave = 2;
        [SerializeField] private bool shouldNotGenerate = false;

        private CurrencyWallet currencyWallet;
       

        public event Action<int> OnCurrencyGenerated;

        private void Start()
        {

            if (MainSceneController.Instance != null)
            {
                MainSceneController.Instance.OnGameplayUISetupRequested += Initialize;
            }          
           
        }

        private void OnDisable()
        {
            if (MainSceneController.Instance != null)
            {
                MainSceneController.Instance.OnGameplayUISetupRequested -= Initialize;
            }
            if (WaveSpawner.Instance != null)
            {
                WaveSpawner.Instance.OnWaveCompleteEnded -= GenerateCurrency;
            }
        }

        private void Initialize()
        {
            currencyWallet = PlayerManager.Instance.GetPlayerComponent<CurrencyWallet>();

            if (WaveSpawner.Instance == null)
            {                
                return;
            }
            WaveSpawner.Instance.OnWaveCompleteEnded += GenerateCurrency;
        }

        private void GenerateCurrency()
        {
            if (currencyWallet == null)
            {
                Debug.LogWarning("CurrencyWallet is not initialized. Cannot generate currency.");
                return;
            }
            if (OnCurrencyGenerated == null)
            {
                Debug.LogWarning("No listeners for OnCurrencyGenerated event. No currency will be generated.");
               
            }
            //Debug.Log($"shouldNotGenerate: {shouldNotGenerate}:: name {gameObject.name} {gameObject.scene}");
            if (shouldNotGenerate)
            {               
                return;
            }
            int currentWave = WaveSpawner.Instance.CurrentWaveIndex;
            int amount = UnityEngine.Random.Range(minCurrencyAmount, maxCurrencyAmount + 1) + (currentWave * currencyPerWave);
           
            currencyWallet?.AddCurrency(amount);
            OnCurrencyGenerated?.Invoke(amount);            
        }

        public void Cleanup()
        {            
            WaveSpawner.Instance.OnWaveCompleteEnded -= GenerateCurrency;
            Destroy(gameObject);
        }
    }

}