using Game.Control;
using Game.Scene;
using Game.Waves;
using UnityEngine;

namespace Game.Progression
{
    public class ExperienceToken : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;

        private int xpAmount = 1;
        private float magnetRange = 2f;
        private Transform playerTransform;

        public void SetAmount(int amount)
        {
            xpAmount = amount;
        }

        public void SetMagnetRange(float range)
        {
            magnetRange = range;
        }

        private void Start()
        {
            //playerTransform = PlayerController.Instance.transform;
            playerTransform = PlayerManager.Instance.GetPlayerComponent<PlayerController>().transform;
        }

        private void OnEnable()
        {
            if (WaveSpawner.Instance != null)
            {
                WaveSpawner.Instance.OnWaveComplete += HandleWaveComplete;
            }
        }

        private void OnDisable()
        {
            if (WaveSpawner.Instance != null)
            {
                WaveSpawner.Instance.OnWaveComplete -= HandleWaveComplete;
            }
        }


        private void Update()
        {
            if (playerTransform == null) return;

            float distance = Vector2.Distance(transform.position, playerTransform.position);
            if (distance <= magnetRange)
            {
                Vector2 direction = (playerTransform.position - transform.position).normalized;
                transform.position += (Vector3)(direction * moveSpeed * Time.deltaTime);
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

        private void HandleWaveComplete()
        {
            Destroy(gameObject);
        }
    }
}
