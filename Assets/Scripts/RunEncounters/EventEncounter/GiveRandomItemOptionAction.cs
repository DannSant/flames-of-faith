using Game.Effects;
using Game.Items;
using Game.Loot;
using Game.Progression;
using UnityEngine;

namespace Game.RunEncounters
{
    /// <summary>
    /// An event option's random item: rolled from the drop table's Event items (ItemProvider), with the player's
    /// bias stat (Luck by default) tilting the quality and their levels beaten deciding what's eligible.
    /// </summary>
    [CreateAssetMenu(menuName = "RunEncounters/EventOptionsActions/GiveRandomItemOptionAction", fileName = "GiveRandomItemOptionAction")]
    public class GiveRandomItemOptionAction : EventOptionActionBase
    {
        public EffectQuality GetExpectedQuality(PlayerProgression playerProgression)
        {
            int statValue = playerProgression != null ? playerProgression.GetStatTotal(biasStat) : 0;
            return ItemProvider.GetExpectedQuality(LootSource.Event, statValue);
        }

        public override string Apply(EventContext context)
        {
            var itemBag = context.playerEffectStore.GetComponent<ItemBag>();

            if (itemBag == null)
            {
                Debug.LogWarning("ItemBag component not found on player.");
                return string.Empty;
            }

            int statValue = context.playerProgression != null ? context.playerProgression.GetStatTotal(biasStat) : 0;
            Effect item = ItemProvider.RollOne(LootSource.Event, statValue);
            if (item == null)
            {
                return string.Empty;
            }

            itemBag.AddEffectItem(item);
            return $"Found item: {item.effectName}";
        }
    }
}
