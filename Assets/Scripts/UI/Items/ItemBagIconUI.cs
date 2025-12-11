using Game.Effects;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Items
{
    public class ItemBagIconUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;

        public void Initialize(Effect item)
        {            
            if (iconImage == null) return;
            iconImage.sprite = item.EffectIcon;
        }
    }

}