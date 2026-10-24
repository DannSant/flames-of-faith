using Game.Effects;
using Game.Items;
using Game.Loot;
using Game.Scene;
using UnityEngine;

namespace Game.Pickups
{
    /// <summary>
    /// The random item an enemy sometimes drops. The item is rolled when it's picked up, from the drop table's Enemy
    /// Pickup items (ItemProvider), with the player's Luck and levels beaten at that moment.
    /// </summary>
    public class RandomItemPickup : BasePickup
    {
        private ItemBag itemBag;

        protected override void Start()
        {
            base.Start();
            itemBag = PlayerManager.Instance.GetPlayerComponent<ItemBag>();
        }

        public override bool CanBePickedUp(GameObject picker)
        {
            return true;
        }

        public override void OnPickup(GameObject picker)
        {
            if (itemBag == null)
            {
                Debug.LogWarning("ItemBag component not found on player.");
                return;
            }

            Effect selected = ItemProvider.RollOne(LootSource.EnemyPickup, ItemProvider.GetPlayerStat());
            if (selected != null)
            {
                itemBag.AddEffectItem(selected);
            }
            Destroy(gameObject);
        }
    }

}
