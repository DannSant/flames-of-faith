using Game.Combat.Projectiles;
using UnityEngine;

namespace Game.UI
{
    public class RandomDirectionInitializer : MonoBehaviour
    {
        [SerializeField] private float minAngle = 0f;
        [SerializeField] private float maxAngle = 360f;

        private void Start()
        {
            var movement = GetComponent<ProjectileMovementBase>();
            if (movement == null) return;

            float angle = Random.Range(minAngle, maxAngle);
            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad),
                                            Mathf.Sin(angle * Mathf.Deg2Rad));

            movement.Initialize(direction);
        }
    }
}
