using Game.Combat;
using Game.Progression;
using Game.Scene;
using UnityEngine;

namespace Game.Pickups
{
    public class HealthPickup : BasePickup
    {
        [SerializeField] private float baseAmount = 5;
        [SerializeField] private float amountPerLevel = 2;

        public override bool CanBePickedUp(GameObject picker)
        {
            var playerHealth = picker.GetComponent<PlayerHealth>();
            if (playerHealth.IsAtMaxHealth())
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public override void OnPickup(GameObject picker)
        {
            var playerHealth = picker.GetComponent<PlayerHealth>();
            var playerProgression = picker.GetComponent<PlayerProgression>();

            if (playerHealth.IsAtMaxHealth()) return;

            if(playerHealth != null && playerProgression != null)
            {
               int levelHealthBonus = GameSession.Instance.LevelsBeaten * (int)amountPerLevel;
                playerHealth.Heal(baseAmount + levelHealthBonus);
                Destroy(gameObject);
            }

        }
    }

}