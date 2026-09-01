
using Game.Currency;
using Game.Effects;
using Game.Progression;
using Game.Scene;
using Game.UI;
using Game.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.RunEncounters
{
    public class ShopKeeper : MonoBehaviour
    {
        [SerializeField] private int itemCount = 10;
        [SerializeField] private float qualityScalePerStat = 2f;
        [SerializeField] private int quantityScalePerStat = 1;
        [SerializeField] private int maxItemCount = 20;
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
            var availableEffects = EffectsDatabaseProvider.Instance.GetAvailableEffects();

            int luck = playerProgression != null ? playerProgression.GetStatTotal(StatType.Luck) : 0;

            int displayCount = Mathf.Min(itemCount + luck * quantityScalePerStat, maxItemCount);

            // Roll each slot's quality independently (weighted by Luck, never fully excluding
            // any tier) rather than sorting-and-cutting, so common items can still show up
            // alongside rare ones instead of being pushed out entirely at high Luck.
            var remainingPool = new List<Effect>(availableEffects);
            var selectedEffects = new List<Effect>();

            for (int i = 0; i < displayCount && remainingPool.Count > 0; i++)
            {
                EffectQuality rolledQuality = EffectQualityScaler.RollQualityTier(luck, qualityScalePerStat, remainingPool);
                var candidates = remainingPool.Where(e => e.Quality == rolledQuality).ToList();
                var chosen = candidates[Random.Range(0, candidates.Count)];

                selectedEffects.Add(chosen);
                remainingPool.Remove(chosen);
            }

            // Shuffle final display order so it doesn't read as grouped/sorted by quality.
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
            }
        }

        public int GetDiscountedPrice(Effect effect)
        {
            int discountStat = playerProgression != null ? playerProgression.GetStatTotal(StatType.ShopItemDiscount) : 0;
            float multiplier = StatsCalculations.CalculateShopDiscountMultiplier(discountStat);
            return Mathf.Max(0, Mathf.RoundToInt(effect.BuyPrice * multiplier));
        }

        public bool BuyItem(Effect effect)
        {
            if(playerWallet == null)
            {
                Debug.LogWarning("Player wallet not found. Cannot process purchase.");
                return false;
            }

            int finalPrice = GetDiscountedPrice(effect);
            if (playerWallet.CurrencyAmount < finalPrice)
            {
                return false;
            }

            playerWallet.RemoveCurrency(finalPrice);
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