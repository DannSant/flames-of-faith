using Game.Currency;
using Game.Scene;
using Game.Waves;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    public class InventoryControllerUI : MonoBehaviour
    {
        [SerializeField] private GameObject titleObject;
        [SerializeField] private GameObject mainPanel;

        [SerializeField] private TextMeshProUGUI currencyText;

        private void Start()
        {
            TogglePanel(false);
            WaveSpawner waveSpawner = WaveSpawner.Instance;
            if (waveSpawner != null)
            {
                waveSpawner.OnWaveComplete += ShowInventoryWindow;
                waveSpawner.OnWaveStarted += HideInventoryWindow;
            }
        }

        

        private void OnDisable()
        {
            WaveSpawner waveSpawner = WaveSpawner.Instance;
            if (waveSpawner != null)
            {
                waveSpawner.OnWaveComplete -= ShowInventoryWindow;
                waveSpawner.OnWaveStarted -= HideInventoryWindow;
            }
        }

        private void TogglePanel(bool value)
        {
            titleObject.SetActive(value);
            mainPanel.SetActive(value);
        }

        public void ShowInventoryWindow()
        {
            // Show the inventory panel
            TogglePanel(true);

             CurrencyWallet currencyWallet = PlayerManager.Instance.GetPlayerComponent<CurrencyWallet>();
            currencyText.text = currencyWallet != null ? currencyWallet.CurrencyAmount.ToString() : "0";

        }

        public void HideInventoryWindow(int _)
        {
            // Hide the inventory panel
            TogglePanel(false);
        }
    }
}
