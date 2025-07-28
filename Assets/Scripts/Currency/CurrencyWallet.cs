using Game.Common;
using Game.Scene;
using UnityEngine;

namespace Game.Currency
{
    public class CurrencyWallet : MonoBehaviour, IPrimaryStateLoader
    {
        private int currencyAmount = 0;

        public event System.Action<int> OnCurrencyChanged;
        //public event System.Action<int> OnCurrencyDisplayRequest;


        public int CurrencyAmount {
            get => currencyAmount;            
        }

        public void AddCurrency(int amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning("Cannot add negative currency amount.");
                return;
            }
            currencyAmount += amount;
            OnCurrencyChanged?.Invoke(currencyAmount);
            
        }

        public void LoadState()
        {
            currencyAmount = GameSession.Instance.LoadCurrencyAmount();
            OnCurrencyChanged?.Invoke(currencyAmount);
        }

        public void ResetState()
        {
            currencyAmount = 0;
        }

        public void SaveState()
        {
            GameSession.Instance.SaveCurrencyAmount(currencyAmount);
        }
    }

}