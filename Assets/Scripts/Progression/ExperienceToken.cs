using Game.Common;
using Game.Control;
using Game.Pickups;
using Game.Scene;
using Game.Utils;
using Game.Waves;
using UnityEngine;

namespace Game.Progression
{
    public class ExperienceToken : MonoBehaviour, ISceneCleanupHandler, IWorldPickup
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        private float xpAmount = 1f;
        private bool collected;

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
                CollectInstantly(playerXP);
            }
        }

        /// <summary>Grants this token's XP and removes it. Safe to call more than once.</summary>
        public void CollectInstantly(PlayerExperience playerXP)
        {
            if (collected || playerXP == null) return;
            collected = true;
            playerXP.AddExperience(xpAmount);
            Destroy(gameObject);
        }

        /// <summary>
        /// Pulls every token in the scene toward the player regardless of pickup range
        /// (end-of-level reward). Returns how many were found.
        /// </summary>
        public static int AttractAll(float speed)
        {
            var tokens = FindObjectsByType<ExperienceToken>(FindObjectsSortMode.None);
            foreach (var token in tokens)
            {
                var magnet = token.GetComponent<MagnetToPlayer>();
                if (magnet != null) magnet.ForceAttract(speed);
            }
            return tokens.Length;
        }

        /// <summary>Grants every remaining token in the scene immediately. Returns how many.</summary>
        public static int CollectAllInstantly(PlayerExperience playerXP)
        {
            if (playerXP == null) return 0;
            var tokens = FindObjectsByType<ExperienceToken>(FindObjectsSortMode.None);
            foreach (var token in tokens)
            {
                token.CollectInstantly(playerXP);
            }
            return tokens.Length;
        }

        public static int CountInScene()
        {
            return FindObjectsByType<ExperienceToken>(FindObjectsSortMode.None).Length;
        }

        public void Cleanup()
        {
           Destroy(gameObject);
        }
    }
}
