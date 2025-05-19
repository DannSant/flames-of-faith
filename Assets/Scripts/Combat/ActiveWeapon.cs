using Game.Common;
using Game.Control;
using System;
using UnityEngine;

namespace Game.Combat
{
    [Obsolete]
    public class ActiveWeapon : Singleton<ActiveWeapon>
    {
        [SerializeField] private Sword sword;

        //public event Action<float,float> OnAttackTimerUpdated;
        //public event Action<float,float> OnSpecialAttackTimerUpdated;

        protected override void Awake()
        {
            base.Awake();
            
        }

        private void Update()
        {
            MouseFollowWithOffset();           

        }        

        private void MouseFollowWithOffset()
        {
            Vector2 mousePosition = PlayerController.Instance.GetMouseWorldPosition(); // Get the mouse position in world space
            Vector2 direction = (mousePosition - (Vector2)transform.position).normalized;
            transform.right = direction; // Rotate the sword to face the mouse position
            Vector2 scale = transform.localScale;
            if (direction.x < 0)
            {
                scale.y = -1;
            }
            else if (direction.x > 0)
            {
                scale.y = 1;
            }
            transform.localScale = scale;

        }        
    }
}
