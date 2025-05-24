using Game.Progression;
using UnityEngine;

namespace Game.Combat
{
    public class HealthRegen : MonoBehaviour
    {
         private PlayerProgression playerProgression;
         private PlayerHealth playerHealth;

        private float regenTimer = 0f;

        private void Awake()
        {
            playerProgression = GetComponent<PlayerProgression>();
            playerHealth = GetComponent<PlayerHealth>();
        }

        void Update()
        {
            float hpRegen = playerProgression.GetStatTotal(StatType.HealthRegen);
            if (hpRegen <= 0f) return;

            float interval = CalculateHealInterval(hpRegen);
            regenTimer += Time.deltaTime;

            if (regenTimer >= interval)
            {
                playerHealth.Heal(1);
                regenTimer = 0f;
            }
        }

        private float CalculateHealInterval(float hpRegen)
        {
            if (hpRegen <= 0f) return Mathf.Infinity;
            return 5f / (1f + ((hpRegen - 1f) / 2.25f));
        }
    }
}
