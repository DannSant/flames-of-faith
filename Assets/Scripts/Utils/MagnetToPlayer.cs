using Game.Control;
using Game.Pickups;
using Game.Progression;
using Game.Scene;
using UnityEngine;

namespace Game.Utils {
    public class MagnetToPlayer : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float baseRange = 2f;
        [SerializeField] private float statScale = 0.1f;

        private Transform playerTransform;
        private PlayerProgression playerProgression;
        private BasePickup pickupObject;
        private void Start()
        {         
            playerTransform = PlayerManager.Instance.GetPlayerComponent<PlayerController>().transform;
            playerProgression = PlayerManager.Instance.GetPlayerComponent<PlayerProgression>();
            pickupObject = GetComponent<BasePickup>();
        }
        private void Update()
        {
            if (!ShouldPullTowardsPlayer()) return;

            float distance = Vector2.Distance(transform.position, playerTransform.position);
            float additionalRange = playerProgression.GetStatTotal(StatType.PickupRange) * statScale;
            float totalRange = baseRange + additionalRange;
            if (distance <= totalRange)
            {
                Vector2 direction = (playerTransform.position - transform.position).normalized;
                transform.position += (Vector3)(direction * moveSpeed * Time.deltaTime);
            }
        }

        private bool ShouldPullTowardsPlayer()
        {
            if (pickupObject == null) return true;
            if (playerTransform == null) return false;
            var playerController = playerTransform.GetComponent<PlayerController>();
            if (playerController == null) return false;
            return pickupObject.CanBePickedUp(playerController.gameObject);
        }
    }

}