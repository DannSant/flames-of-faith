
using Game.Currency;
using Game.Effects;
using Game.Progression;
using Game.Scene;
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
        public List<Effect> items;

        public event System.Action<bool, List<Effect>> onShopWindowToggle;
        public event System.Action onShopWindowOpened;
        public event System.Action onInventoryWindowRefresh;

        private CurrencyWallet playerWallet;
        private EffectStore effectStore;
        private PlayerProgression playerProgression;

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
            BuildItemList();
        }

        private void BuildItemList()
        {
            var availableEffects = EffectsDatabaseProvider.Instance.GetAvailableEffects();

            int luck = playerProgression != null ? playerProgression.GetStatTotal(StatType.Luck) : 0;
            //Debug.Log(playerProgression==null ? "null" : playerProgression.name);
            EffectQuality maxQuality = EffectQualityScaler.GetMaxEligibleQuality(luck, qualityScalePerStat);
            var eligibleEffects = EffectQualityScaler.FilterByMaxQuality(availableEffects, maxQuality);

            int displayCount = itemCount + luck * quantityScalePerStat;

            // Fill from the highest eligible quality down, spilling into lower tiers only once
            // a tier is exhausted; ThenBy randomizes which items are picked within the same quality.
            var selectedEffects = eligibleEffects
                .OrderByDescending(e => e.Quality)
                .ThenBy(e => Random.value)
                .Take(displayCount)
                .ToList();

            //Debug.Log($"ShopKeeper: Selected {selectedEffects.Count} items for display. Max Quality: {maxQuality}, Luck: {luck}");
            //Debug.Log($"Items {string.Join(", ", selectedEffects.Select(e => e.effectName))}");

            // Shuffle again so the shop display isn't visibly grouped by quality.
            items = selectedEffects.OrderBy(x => Random.value).ToList();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {          
            if (collision.GetComponent<CurrencyWallet>())
            {
                onShopWindowToggle?.Invoke(true, items);
                onShopWindowOpened?.Invoke();
            }
        }

        public bool BuyItem(Effect effect)
        {
            if(playerWallet == null)
            {
                Debug.LogWarning("Player wallet not found. Cannot process purchase.");
                return false;
            }
            if (playerWallet.CurrencyAmount < effect.BuyPrice)
            {
                return false;
            }

            playerWallet.RemoveCurrency(effect.BuyPrice);
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