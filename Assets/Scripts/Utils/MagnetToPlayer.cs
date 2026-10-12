using Game.Control;
using Game.Pickups;
using Game.Progression;
using Game.Scene;
using UnityEngine;

namespace Game.Utils {
    public class MagnetToPlayer : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;

        private Transform playerTransform;
        private PlayerProgression playerProgression;
        private BasePickup pickupObject;
        private bool forceAttract;
        private float forcedSpeed;

        private void Start()
        {
            playerTransform = PlayerManager.Instance.GetPlayerComponent<PlayerController>().transform;
            playerProgression = PlayerManager.Instance.GetPlayerComponent<PlayerProgression>();
            pickupObject = GetComponent<BasePickup>();
        }

        /// <summary>
        /// From now on, fly to the player at <paramref name="speed"/> from any distance
        /// (e.g. collecting every XP gem at the end of a level).
        /// </summary>
        public void ForceAttract(float speed)
        {
            forceAttract = true;
            forcedSpeed = Mathf.Max(moveSpeed, speed);
        }

        private void Update()
        {
            if (forceAttract)
            {
                if (playerTransform == null) return;
                transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, forcedSpeed * Time.deltaTime);
                return;
            }

            if (!ShouldPullTowardsPlayer()) return;

            float distance = Vector2.Distance(transform.position, playerTransform.position);
            // Range numbers live on PlayerStatSettings (shared with the Pickup Range tooltip).
            float totalRange = PlayerStatSettings.Instance.GetPickupRange(playerProgression.GetStatTotal(StatType.PickupRange));
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