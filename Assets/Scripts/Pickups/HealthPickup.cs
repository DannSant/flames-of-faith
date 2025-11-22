using Game.Combat;
using Game.Progression;
using UnityEngine;

namespace Game.Pickups
{
    public class HealthPickup : BasePickup
    {
        [SerializeField] private float healAmount = 10;
        public override void OnPickup(GameObject picker)
        {
            var playerHealth = picker.GetComponent<PlayerHealth>();
            var playerProgression = picker.GetComponent<PlayerProgression>();

            if (playerHealth.IsAtMaxHealth()) return;

            if(playerHealth != null && playerProgression != null)
            {
                int healingReceivedStat = playerProgression.GetStatTotal(StatType.HealingReceived);
                playerHealth.Heal(healAmount + healingReceivedStat);
                Destroy(gameObject);
            }

        }
    }

}