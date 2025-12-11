using Game.Effects;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Items
{
    public class ItemBag : MonoBehaviour
    {
        [SerializeField] private List<Effect> pickedUpItems;

        public List<Effect> PickedUpItems { get => pickedUpItems; }

        // Events for item added and removed.
        // Params are the new added or removed item and the current list of items.
        public event System.Action<Effect,List<Effect>> OnItemAdded;
        public event System.Action<Effect, List<Effect>> OnItemRemoved;

        public void AddEffectItem(Effect effect)
        {
            if (effect == null)
            {
                Debug.LogWarning("Attempted to add a null effect to ItemBag.");
                return;
            }
            pickedUpItems.Add(effect);           
            OnItemAdded?.Invoke(effect, pickedUpItems);
        }

        public void RemoveEffectItem(Effect effect)
        {
            if (effect == null)
            {
                Debug.LogWarning("Attempted to remove a null effect from ItemBag.");
                return;
            }
            pickedUpItems.Remove(effect);
            OnItemRemoved?.Invoke(effect, pickedUpItems);
        }

        public bool IsBagEmpty()
        {
            return pickedUpItems.Count == 0;
        }
    }
}