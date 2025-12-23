using Game.Misc;
using System;
using UnityEngine;

namespace Game.AI
{
    [Obsolete("use behaviors instead")]
    public class EnemyPathfinding : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        private Rigidbody2D rb;
        private Vector2 moveDir;
        private Knockback knockback;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            knockback = GetComponent<Knockback>();
        }

        private void FixedUpdate()
        {
            // Prevent movement during knockback
            if (knockback.IsKnockbacked) return; 

            rb.MovePosition(rb.position + moveDir * (moveSpeed * Time.fixedDeltaTime));
        }

        public void MoveTo(Vector2 targetPosition)
        {
            moveDir = targetPosition;
        }
    }
}
