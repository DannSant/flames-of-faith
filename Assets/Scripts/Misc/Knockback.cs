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

        /// <summary>
        /// While set, knockback is ignored (damage and debuffs still land). A melee enemy sets it for its swing, so a
        /// hit can't push it off the spot its warning shows. Whoever sets it must clear it on every exit path.
        /// </summary>
        public bool IsImmune { get; private set; }

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

        /// <summary>
        /// Turns the immunity on or off. Turning it on also ends a knockback already playing out, so the movement
        /// behaviors take the body back and the attack's hold-still stops the slide.
        /// </summary>
        public void SetImmune(bool immune)
        {
            IsImmune = immune;
            if (immune && IsKnockbacked) EndKnockback();
        }

        public void ApplyKnockback(Transform damageSource, float force)
        {
            
            if (IsKnockbacked || IsImmune) return;
            if (rb == null) rb = GetComponent<Rigidbody2D>();
            if (rb == null) return;
            
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
            knockbackTimer = 0f;
        }

    }
}
