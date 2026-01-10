using Game.Combat;
using UnityEngine;

namespace Game.Campfire
{
    public class CampfireGraceGenerator : MonoBehaviour
    {
        [SerializeField] private int graceAmount = 3;
        [SerializeField] private int healAmount = 3;

        private bool isActive = true;
        private const string PLAYER_TAG = "Player";

        public event System.Action<bool> onPlayerEntersCampfire;
        public int GraceAmount { get { return graceAmount; } }
        public int HealAmount { get { return healAmount; } }

        private void OnTriggerEnter2D(Collider2D collision)
        {            
            if(!collision.CompareTag(PLAYER_TAG)) return;
            Debug.Log("CampfireGraceGenerator OnTriggerEnter2D");
            GenerateGrace(collision);
            Heal(collision);
           
            onPlayerEntersCampfire?.Invoke(true);
            isActive = false;
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
                Debug.Log("Generating Grace at Campfire.");
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
                Debug.Log("Generating Health at Campfire.");
                playerHealth.Heal(healAmount);
                
            }
        }

    }

}