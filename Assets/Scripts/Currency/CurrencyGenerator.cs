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

        private CurrencyWallet currencyWallet;
        private WaveSpawner waveSpawner;

        public event Action<int> OnCurrencyGenerated;

        private void Start()
        {
            waveSpawner = WaveSpawner.Instance;
            if (waveSpawner == null)
            {
                Debug.LogError("WaveSpawner instance not found.");
                return;
            }
            waveSpawner.OnWaveComplete += GenerateCurrency;

            currencyWallet = PlayerManager.Instance.GetPlayerComponent<CurrencyWallet>();
        }

        private void GenerateCurrency()
        {
            int currentWave = waveSpawner.CurrentWaveIndex;
            int amount = UnityEngine.Random.Range(minCurrencyAmount, maxCurrencyAmount + 1) + (currentWave * currencyPerWave);
            currencyWallet?.AddCurrency(amount);
            OnCurrencyGenerated?.Invoke(amount);            
        }

        public void Cleanup()
        {
            waveSpawner.OnWaveComplete -= GenerateCurrency;
        }
    }

}