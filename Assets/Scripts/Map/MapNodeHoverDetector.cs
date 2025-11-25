using System;
using UnityEngine;

namespace Game.Map
{
    public class MapNodeHoverDetector : MonoBehaviour
    {
        public event Action<MapNodeSprite> OnHoverEnter;
        public event Action<MapNodeSprite> OnHoverExit;

        private bool isHovering;
        private MapNodeSprite node;

        private void Awake()
        {
            node = GetComponent<MapNodeSprite>();
        }

        public void SetHoverState(bool hovering)
        {          
            if (hovering && !isHovering)
            {
                isHovering = true;
                OnHoverEnter?.Invoke(node);
            }
            else if (!hovering && isHovering)
            {
                isHovering = false;
                OnHoverExit?.Invoke(node);
            }
        }
    }
}
