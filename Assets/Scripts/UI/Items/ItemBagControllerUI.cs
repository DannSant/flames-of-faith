using Game.Audio;
using Game.Control;
using Game.Currency;
using Game.Effects;
using Game.GameSettings;
using Game.Items;
using Game.Scene;
using Game.Waves;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Items
{
    /// <summary>
    /// End-of-wave item bag: the player resolves every item picked up during the wave, one at a time,
    /// with Take (into the inventory) or Sell (coins). The window closes when the bag is empty and the
    /// wave end continues (level-up).
    /// Items are the Inventory's slots (EffectIconUI). The selected item (Take/Sell's target) is outlined.
    /// The shared tooltip works like the Inventory's: it shows while an item is hovered or focused and hides
    /// when the pointer or focus leaves it. With the gamepad, moving onto an item selects it.
    /// </summary>
    public class ItemBagControllerUI : MonoBehaviour
    {
        [Header("Containers")]
        [Tooltip("The grid the item slots are spawned in.")]
        [SerializeField] private Transform itemIconsContainer;
        [SerializeField] private GameObject mainPane;

        [Header("Prefabs")]
        [Tooltip("The Inventory's item slot.")]
        [SerializeField] private EffectIconUI itemIconPrefab;

        [Header("Components")]
        [SerializeField] private TextMeshProUGUI itemSellText;
        [Tooltip("{0} is the selected item's sell price.")]
        [SerializeField] private string sellFormat = "Sell ({0})";
        [SerializeField] private Button takeButton;
        [SerializeField] private Button sellButton;
        [SerializeField] private StatsPaneUI statsPaneUI;
        [Tooltip("Optional. Opened alongside, so the player sees what they already own.")]
        [SerializeField] private InventoryControllerUI inventoryUI;

        // References
        private ItemBag itemBag;
        private EffectStore effectStore;
        private CurrencyWallet currencyWallet;
        private GeneralTooltipPaneUI tooltipPane;

        // State
        private readonly List<EffectIconUI> icons = new();
        private Effect selectedItem;

        private void Start()
        {
            if (WaveEventManager.Instance)
            {
                WaveEventManager.Instance.OnItemBagAvailable += HandleItemBagAvailable;
            }

            itemBag = PlayerManager.Instance.GetPlayerComponent<ItemBag>();
            effectStore = PlayerManager.Instance.GetPlayerComponent<EffectStore>();
            currencyWallet = PlayerManager.Instance.GetPlayerComponent<CurrencyWallet>();

            takeButton.onClick.AddListener(TakeSelected);
            sellButton.onClick.AddListener(SellSelected);
            HidePanel();
        }

        private void OnDisable()
        {
            if (WaveEventManager.Instance)
            {
                WaveEventManager.Instance.OnItemBagAvailable -= HandleItemBagAvailable;
            }
        }

        private void HandleItemBagAvailable()
        {
            //Pause the game and Lower music volume
            PauseManager.Instance.SetPause(true);
            AudioManager.Instance.SetLowVolume();

            ClearUI();
            RefreshItems();
            ShowPanel();
            SelectFirstItem();

            // Open (and refresh) the panes beside it so the player can see what they need and own
            statsPaneUI?.ShowStatsWindow(null);
            inventoryUI?.ShowInventoryWindow();
        }

        /// <summary>The Sell button's label for an item's price.</summary>
        public static string FormatSellLabel(string format, int price) => string.Format(format, price);

        private void SelectFirstItem()
        {
            if (itemBag.PickedUpItems.Count > 0)
            {
                SelectItem(itemBag.PickedUpItems[0], false);
            }
            else
            {
                CloseBag();
            }
        }

        private void RefreshItems()
        {
            foreach (var item in itemBag.PickedUpItems)
            {
                var icon = Instantiate(itemIconPrefab, itemIconsContainer);
                icon.Setup(item, item.EffectIcon, 1, HandleItemPointed, HideTooltip);
                // A click happens over the item, so its tooltip shows as with a hover
                icon.GetComponent<Button>().onClick.AddListener(() => SelectItem(item, true));
                icons.Add(icon);
            }
        }

        // Hover (mouse) shows an item's tooltip; moving onto it with the gamepad also selects it
        private void HandleItemPointed(Effect item)
        {
            if (InputDeviceManager.Instance != null && InputDeviceManager.Instance.IsGamepadActive)
            {
                SelectItem(item, true);
                return;
            }
            ShowTooltip(item);
        }

        // showTooltip: only when the item is being pointed at. Opening the bag or taking / selling an item
        // selects the next one without a tooltip, like the Inventory shows none until something is hovered.
        private void SelectItem(Effect item, bool showTooltip)
        {
            selectedItem = item;
            foreach (var icon in icons)
            {
                if (icon != null) icon.SetSelected(icon.Effect == item);
            }
            if (itemSellText != null) itemSellText.text = FormatSellLabel(sellFormat, item.SellPrice);
            if (showTooltip) ShowTooltip(item);
        }

        private void TakeSelected()
        {
            if (selectedItem == null) return;
            var item = selectedItem;
            itemBag.RemoveEffectItem(item);
            effectStore.AddEffect(item);
            CheckIfItemsLeft();
        }

        private void SellSelected()
        {
            if (selectedItem == null) return;
            var item = selectedItem;
            itemBag.RemoveEffectItem(item);
            currencyWallet.AddCurrency(item.SellPrice);
            CheckIfItemsLeft();
        }

        private void CheckIfItemsLeft()
        {
            ClearUI();
            if (itemBag.IsBagEmpty())
            {
                CloseBag();
                return;
            }
            RefreshItems();
            SelectFirstItem();
        }

        private void CloseBag()
        {
            ClearUI();
            HidePanel();
            WaveEventManager.Instance.HandleWaveComplete();
        }

        // --- Shared tooltip ------------------------------------------------------------------------------

        private void ShowTooltip(Effect item)
        {
            if (item == null || !mainPane.activeInHierarchy) return;
            if (tooltipPane == null)
            {
                // UI to UI lookup (the pane lives in the same scene), cached after the first time
                tooltipPane = FindAnyObjectByType<GeneralTooltipPaneUI>();
                if (tooltipPane == null) return;
            }
            // Beside the item's slot
            LayOutNow((RectTransform)itemIconsContainer);
            var slot = icons.Find(icon => icon != null && icon.Effect == item);
            tooltipPane.ShowTooltip(item.GetDescriptionText(), EffectQualityDisplayHelper.ColorizeName(item), null,
                item.GetStatLines(), slot != null ? (RectTransform)slot.transform : null);
        }

        /// <summary>
        /// Lays the slot grid out now. Slots spawned this frame are not in their cell until the layout runs,
        /// and the tooltip is placed beside the slot (it showed above the buttons when the bag opened).
        /// The grid itself, not the window: a rebuild stops at the first object without a layout controller,
        /// and MainPanel has none.
        /// </summary>
        public static void LayOutNow(RectTransform grid) => LayoutRebuilder.ForceRebuildLayoutImmediate(grid);

        // The pointer or the gamepad focus left an item, or its slot was destroyed on a refresh
        private void HideTooltip()
        {
            tooltipPane?.HideTooltip();
        }

        // --- Panel ----------------------------------------------------------------------------------------

        private void ShowPanel()
        {
            mainPane.SetActive(true);
        }

        private void HidePanel()
        {
            selectedItem = null;
            mainPane.SetActive(false);
            HideTooltip();
        }

        private void ClearUI()
        {
            // The slots' OnDisable hides a tooltip they were showing
            selectedItem = null;
            foreach (Transform child in itemIconsContainer)
            {
                // Destroy waits for the frame's end; inactive, the grid already skips it for the new slots
                child.gameObject.SetActive(false);
                Destroy(child.gameObject);
            }
            icons.Clear();
            if (itemSellText != null) itemSellText.text = FormatSellLabel(sellFormat, 0);
        }
    }
}
