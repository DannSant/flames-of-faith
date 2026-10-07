using Game.Common;
using Game.Control;
using Game.Pickups;
using Game.Scene;
using Game.Waves;
using UnityEngine;

namespace Game.Progression
{
    public class ExperienceToken : MonoBehaviour, ISceneCleanupHandler, IWorldPickup
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        private float xpAmount = 1f;

        private void Awake()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }
        }

        public void SetAmount(float amount)
        {
            xpAmount = amount;
        }

        /// <summary>
        /// Sets the XP this token grants and its denomination sprite. A null sprite keeps the prefab's sprite.
        /// </summary>
        public void Setup(float amount, Sprite sprite)
        {
            xpAmount = amount;
            if (sprite != null && spriteRenderer != null)
            {
                spriteRenderer.sprite = sprite;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var playerXP = other.GetComponent<PlayerExperience>();
            if (playerXP != null)
            {
                playerXP.AddExperience(xpAmount);
                Destroy(gameObject);
            }
        }

        public void Cleanup()
        {
           Destroy(gameObject);
        }
    }
}
