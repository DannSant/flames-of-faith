using UnityEngine;

namespace Game.Combat {
    public class ProjectileMovement : MonoBehaviour
    {
        [SerializeField] private float speed = 10f;
        private Vector2 direction = Vector2.zero;

        public void SetDirection(Vector2 dir)
        {
            direction = dir.normalized;
        }

        private void Update()
        {
            transform.position += (Vector3)(direction * speed * Time.deltaTime);
        }
    }
}
