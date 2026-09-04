using Game.Combat;
using Game.Scene;
using Game.UI;
using UnityEngine;

namespace Game.RunEncounters
{
    public class CampfireGraceGenerator : MonoBehaviour
    {
        [SerializeField] private int graceAmount = 3;
        [SerializeField] private int healAmount = 3;

        private bool isActive = true;
        private const string PLAYER_TAG = "Player";

        private StatsPaneUI statsPaneUI;

        public event System.Action<bool> onPlayerEntersCampfire;
        public int GraceAmount { get { return graceAmount; } }
        public int HealAmount { get { return healAmount; } }

        private void Start()
        {
            // The UI scene loads after this level's scene (see MainSceneController.LoadGameplayRoutine),
            // so StatsPaneUI doesn't exist yet if we look for it here. Wait for the signal that
            // gameplay setup (both scenes loaded) has fully completed, same as ShopKeeper does.
            if (MainSceneController.Instance != null)
            {
                MainSceneController.Instance.OnGameplayInitialSetup += CacheStatsPaneUI;
            }
        }

        private void OnDisable()
        {
            if (MainSceneController.Instance != null)
            {
                MainSceneController.Instance.OnGameplayInitialSetup -= CacheStatsPaneUI;
            }
        }

        private void CacheStatsPaneUI()
        {
            statsPaneUI = FindAnyObjectByType<StatsPaneUI>();
            if (statsPaneUI == null)
            {
                Debug.LogWarning("CampfireGraceGenerator: StatsPaneUI not found after gameplay initial setup.");
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if(!collision.CompareTag(PLAYER_TAG)) return;

            GenerateGrace(collision);
            Heal(collision);

            onPlayerEntersCampfire?.Invoke(true);
            isActive = false;

            // Open (and refresh) the stats window so the player can see what they need.
            statsPaneUI?.ShowStatsWindow(null);
        }

        private void GenerateGrace(Collider2D collision)
        {
            if (!isActive) return;
            var playerGrace = collision.GetComponent<PlayerGrace>();

            if (playerGrace == null)
            {
                Debug.LogWarning("PlayerGrace component not found on player.");
                return;
            }

            if (playerGrace != null && !playerGrace.IsAtMaxGrace())
            {               
                playerGrace.AddGrace(graceAmount);                
            }
        }

        private void Heal(Collider2D collision)
        {
            if (!isActive) return;
            var playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth == null)
            {
                Debug.LogWarning("PlayerHealth component not found on player.");
                return;
            }
            if (playerHealth != null && !playerHealth.IsAtMaxHealth())
            {               
                playerHealth.Heal(healAmount);                
            }
        }

    }

}