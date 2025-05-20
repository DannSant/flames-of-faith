
using Game.Combat;
using Game.Scene;
using UnityEngine;

namespace Game.UI
{
    public class DeathscreenController : MonoBehaviour
    {
        [SerializeField] private GameObject deathScreenPanel;
        private void Start()
        {
            deathScreenPanel.SetActive(false);           
            /*if (PlayerHealth.Instance != null) {
                PlayerHealth.Instance.onDeath += ShowDeathScreen;
            }*/
            var playerHealth = PlayerManager.Instance.GetPlayerComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.onDeath += ShowDeathScreen;
            }
            
        }

        private void OnDisable()
        {            
            var playerHealth = PlayerManager.Instance.GetPlayerComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.onDeath -= ShowDeathScreen;
            }
        }

        private void ShowDeathScreen() 
        {
            deathScreenPanel.SetActive(true);
        }

        public void OnRetryButtonClicked() 
        { 
            MainSceneController.Instance.LoadGameplay();
        }

        public void OnMainMenuButtonClicked()
        {
            MainSceneController.Instance.LoadMainMenu();
        }
    }

}