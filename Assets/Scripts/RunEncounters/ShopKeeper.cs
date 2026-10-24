
using Game.Control;
using Game.Currency;
using Game.Effects;
using Game.Loot;
using Game.Progression;
using Game.Scene;
using Game.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.RunEncounters
{
    public class ShopKeeper : MonoBehaviour
    {
        // The stock comes from the drop table's Shop items, and its size from LootSettings (ItemProvider)
        public List<Effect> items;

        public event System.Action<bool, List<Effect>, ShopKeeper> onShopWindowToggle;
        public event System.Action onShopWindowOpened;
        public event System.Action onInventoryWindowRefresh;

        private CurrencyWallet playerWallet;
        private EffectStore effectStore;
        private PlayerProgression playerProgression;
        private StatsPaneUI statsPaneUI;

        private void Start()
        {
            // PlayerProgression's stats aren't populated (LoadState) until after Start() runs
            // for scene objects, so wait for the signal that gameplay setup has fully completed.
            if (MainSceneController.Instance != null)
            {
                MainSceneController.Instance.OnGameplayInitialSetup += InitializeShopItems;
            }
        }

        private void OnDisable()
        {
            if (MainSceneController.Instance != null)
            {
                MainSceneController.Instance.OnGameplayInitialSetup -= InitializeShopItems;
            }

            // The shop only closes by leaving the level, and the player persists across levels,
            // so the block must be released when this scene unloads.
            SetPlayerInputBlocked(false);
        }

        private void InitializeShopItems()
        {
            effectStore = PlayerManager.Instance.GetPlayerComponent<EffectStore>();
            playerWallet = PlayerManager.Instance.GetPlayerComponent<CurrencyWallet>();
            playerProgression = PlayerManager.Instance.GetPlayerComponent<PlayerProgression>();
            statsPaneUI = FindAnyObjectByType<StatsPaneUI>();
            BuildItemList();
        }

        private void BuildItemList()
        {
            // Each slot rolls its quality on its own (weighted by Luck, never fully excluding a quality) rather than
            // sorting and cutting, so common items still show up next to rare ones at high Luck. Items never repeat.
            // This runs inside the level load, so ItemProvider never throws: a short pool just gives a shorter shop.
            int luck = playerProgression != null ? playerProgression.GetStatTotal(StatType.Luck) : 0;
            int displayCount = LootSettings.Instance.GetShopItemCount(luck);
            var selectedEffects = ItemProvider.Roll(LootSource.Shop, luck, displayCount, true);

            // Shuffle the display order so it doesn't read as grouped by quality
            items = selectedEffects.OrderBy(x => Random.value).ToList();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.GetComponent<CurrencyWallet>())
            {
                onShopWindowToggle?.Invoke(true, items, this);
                onShopWindowOpened?.Invoke();

                // Open (and refresh) the stats window so the player can see what they need.
                statsPaneUI?.ShowStatsWindow(null);

                // Browsing is modal until Continue: no walking or dashing around the shop.
                SetPlayerInputBlocked(true);
            }
        }

        private void SetPlayerInputBlocked(bool blocked)
        {
            if (PlayerManager.Instance == null) return;
            var inputHandler = PlayerManager.Instance.GetPlayerComponent<PlayerInputHandler>();
            if (inputHandler == null) return;

            if (blocked)
            {
                inputHandler.AddGameplayBlock(this);
            }
            else
            {
                inputHandler.RemoveGameplayBlock(this);
            }
        }

        public int GetDiscountedPrice(Effect effect)
        {
            int discountStat = playerProgression != null ? playerProgression.GetStatTotal(StatType.ShopItemDiscount) : 0;
            float multiplier = PlayerStatSettings.Instance.GetShopDiscountMultiplier(discountStat);
            return Mathf.Max(0, Mathf.RoundToInt(effect.BuyPrice * multiplier));
        }

        /// <summary>Whether the player has enough coins for the item at its discounted price.</summary>
        public bool CanAfford(Effect effect) =>
            playerWallet != null && effect != null && playerWallet.CurrencyAmount >= GetDiscountedPrice(effect);

        public bool BuyItem(Effect effect)
        {
            if(playerWallet == null)
            {
                Debug.LogWarning("Player wallet not found. Cannot process purchase.");
                return false;
            }

            if (!CanAfford(effect))
            {
                return false;
            }

            playerWallet.RemoveCurrency(GetDiscountedPrice(effect));
            items.Remove(effect);

            if(effectStore != null)
            {
                effectStore.AddEffect(effect);
            }

            onInventoryWindowRefresh?.Invoke();


            return true;
        }

        public List<Effect> GetAvailableItems()
        {
            return items;
        }
    }

}