using System;
using UnityEngine;

namespace Game.Misc
{
    public class Knockback : MonoBehaviour
    {
        [SerializeField] float knockbackTime = 1.5f;

        private Rigidbody2D rb;
        private float knockbackTimer = 0f;
       

        public event Action OnKnockbackStarted;

        public bool IsKnockbacked  {get; private set;}

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            KnockbackCheck();
        }

        private void KnockbackCheck()
        {
            if (!IsKnockbacked) return;

            
            if (knockbackTimer>0f)
            {
                knockbackTimer -= Time.deltaTime;
            }
            else if (knockbackTimer <= 0f)
            {
                // Reset the knockback state
                EndKnockback();
            }
        }

        public void ApplyKnockback(Transform damageSource, float force)
        {
            
            if (IsKnockbacked) return;
            
            StartKnockback();
            Vector2 direction = (transform.position - damageSource.position).normalized * force * rb.mass;
                    
            // Apply a force to the Rigidbody2D in the specified direction
            rb.AddForce(direction, ForceMode2D.Impulse);
        }

        private void StartKnockback()
        {
            OnKnockbackStarted?.Invoke();
            knockbackTimer = knockbackTime;
            IsKnockbacked = true;
        }

        private void EndKnockback()
        {           
            IsKnockbacked = false;
        }

    }
}
