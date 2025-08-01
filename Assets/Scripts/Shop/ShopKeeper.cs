
using Game.Currency;
using Game.Effects;
using Game.Scene;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Shop
{
    public class ShopKeeper : MonoBehaviour
    {
        public List<Effect> items;

        public event System.Action<bool, List<Effect>> onShopWindowToggle;
        public event System.Action onShopWindowOpened;
        public event System.Action onInventoryWindowRefresh;

        private CurrencyWallet playerWallet;
        private EffectStore effectStore;

        private void Awake()
        {
            BuildItemList();
        }

        private void Start()
        {
            effectStore = PlayerManager.Instance.GetPlayerComponent<EffectStore>();
            playerWallet = PlayerManager.Instance.GetPlayerComponent<CurrencyWallet>();
        }

        private void BuildItemList()
        {
            var availableEffects = EffectsDatabaseProvider.Instance.GetAvailableEffects();
            items = availableEffects.OrderBy(x => Random.value).Take(5).ToList();
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