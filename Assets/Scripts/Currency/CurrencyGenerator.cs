using Game.Common;
using Game.Scene;
using Game.Waves;
using System;
using UnityEngine;

namespace Game.Currency
{
    public class CurrencyGenerator : Singleton<CurrencyGenerator>
    {
        [SerializeField] private int minCurrencyAmount = 10;
        [SerializeField] private int maxCurrencyAmount = 20;
        [SerializeField] private int currencyPerWave = 2;

        private CurrencyWallet currencyWallet;
        //private WaveSpawner waveSpawner;

        public event Action<int> OnCurrencyGenerated;

        private void Start()
        {

            if (MainSceneController.Instance != null)
            {
                MainSceneController.Instance.OnGameplayUISetupRequested += Initialize;
            }          
           
        }

        private void Initialize()
        {
            currencyWallet = PlayerManager.Instance.GetPlayerComponent<CurrencyWallet>();

            if (WaveSpawner.Instance == null)
            {
                Debug.LogError("WaveSpawner instance not found.");
                return;
            }
            WaveSpawner.Instance.OnWaveComplete += GenerateCurrency;
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
            int currentWave = WaveSpawner.Instance.CurrentWaveIndex;
            int amount = UnityEngine.Random.Range(minCurrencyAmount, maxCurrencyAmount + 1) + (currentWave * currencyPerWave);
            currencyWallet?.AddCurrency(amount);
            OnCurrencyGenerated?.Invoke(amount);            
        }

        public void Cleanup()
        {
            Debug.Log("CurrencyGenerator.Cleanup");
            WaveSpawner.Instance.OnWaveComplete -= GenerateCurrency;
        }
    }

}