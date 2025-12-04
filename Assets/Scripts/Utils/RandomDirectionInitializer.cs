using Game.Combat.Projectiles;
using UnityEngine;

namespace Game.UI
{
    public class RandomDirectionInitializer : MonoBehaviour
    {
        [SerializeField] private float minAngle = 0f;
        [SerializeField] private float maxAngle = 360f;
        [SerializeField] private float initialSpeedOverride = 0f; // 0 = use movement component default

        private void Start()
        {
            var movement = GetComponent<ProjectileMovementBase>();
            if (movement == null) return;

            float angle = Random.Range(minAngle, maxAngle);
            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad),
                                            Mathf.Sin(angle * Mathf.Deg2Rad));

            movement.Initialize(direction);

            // Optional: override speed on Linear movement
            if (initialSpeedOverride > 0 && movement is ProjectileMoveLinear linear)
            {
                linear.SetSpeed(initialSpeedOverride);
            }
        }
    }
}
