using Game.Currency;
using Game.Effects;
using Game.Items;
using Game.Scene;
using Game.Waves;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Items
{
    public class ItemBagControllerUI : MonoBehaviour
    {
        [Header("Containers")]
        [SerializeField] private Transform itemIconsContainer;
        [SerializeField] private Transform itemDescriptionContainer;
        [SerializeField] private GameObject mainPane;

        [Header("Prefabs")]
        [SerializeField] private GameObject itemIconPrefab;

        [Header("Components")]
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI itemNameDesc;
        [SerializeField] private TextMeshProUGUI itemSellText;
        [SerializeField] private Button takeButton;
        [SerializeField] private Button sellButton;

        // References
        private ItemBag itemBag;
        private EffectStore effectStore;
        private CurrencyWallet currencyWallet;

        // State
        private int selectedItemIndex = -1;

        private void Start()
        {
            WaveEventManager.Instance.OnItemBagAvailable += HandleItemBagAvailable;
            itemBag = PlayerManager.Instance.GetPlayerComponent<ItemBag>();
            effectStore = PlayerManager.Instance.GetPlayerComponent<EffectStore>();
            currencyWallet = PlayerManager.Instance.GetPlayerComponent<CurrencyWallet>();
            HidePanel();
        }

        private void HandleItemBagAvailable()
        {
            var items = itemBag.PickedUpItems;

            // Clear existing UI
            ClearUI();

            // Populate UI with current items
            foreach (var item in items) {
                var buttonItem = Instantiate(itemIconPrefab, itemIconsContainer);
                buttonItem.GetComponent<ItemBagIconUI>().Initialize(item);
                var button = buttonItem.GetComponent<Button>();
                button.onClick.AddListener(() =>
                {
                    SelectIcon(items.IndexOf(item));
                });
            }

            ShowPanel();
            SelectIcon(0);
        }


        private void SelectIcon(int index)
        {
            selectedItemIndex = index;
            var item = itemBag.PickedUpItems[index];
            itemNameText.text = item.EffectName;
            itemNameDesc.text = item.Description;
            itemSellText.text = $"Sell ({item.SellPrice})";

            takeButton.onClick.AddListener(() =>
            {
                itemBag.RemoveEffectItem(item);
                effectStore.AddEffect(item);
                ClearUI();
                CheckIfItemsLeft();
            });

            sellButton.onClick.AddListener(() =>
            {
                itemBag.RemoveEffectItem(item);
                currencyWallet.AddCurrency(item.SellPrice);
                ClearUI();
                CheckIfItemsLeft();
            });

            itemDescriptionContainer.gameObject.SetActive(true);
        }

        private void CheckIfItemsLeft()
        {
            if (itemBag.IsBagEmpty())
            {
                HidePanel();
                WaveEventManager.Instance.HandleWaveComplete();
            }
        }

        private void ShowPanel()
        {
            mainPane.gameObject.SetActive(true);
        }

        private void HidePanel()
        {
            mainPane.gameObject.SetActive(false);
        }

        private void ClearUI()
        {
            
            foreach (Transform child in itemIconsContainer)
            {
                var button = child.GetComponent<Button>();
                button.onClick.RemoveAllListeners();
                Destroy(child.gameObject);
            }
            itemNameText.text = "";
            itemNameDesc.text = "";
            itemSellText.text = "Sell";
            takeButton.onClick.RemoveAllListeners();
            sellButton.onClick.RemoveAllListeners();
            itemDescriptionContainer.gameObject.SetActive(false);
        }
    }
}
