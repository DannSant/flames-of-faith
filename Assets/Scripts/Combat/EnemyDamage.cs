using Game.AI;
using Game.Control;
using Game.Misc;
using UnityEngine;

namespace Game.Combat {
    public class EnemyDamage : MonoBehaviour
    {
        
        [SerializeField] private bool shouldApplyKnockback = true;
        [SerializeField] private float knockbackForce = 15f;

        private float damageAmount = 1f;
        private Transform attacker;

        private void Awake()
        {
            // The enemy this hitbox belongs to: the knockback pushes away from where it stands, not from the hitbox
            var body = GetComponentInParent<Rigidbody2D>();
            attacker = body != null ? body.transform : transform;
        }

        public void SetDamageAmount(float amount)
        {
            damageAmount = amount;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            ProcessDamageToPlayer(collision.collider);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            ProcessDamageToPlayer(collision);
        }

        private void ProcessDamageToPlayer(Collider2D collision)
        {
            var health = collision.GetComponent<PlayerHealth>();            
            if (health != null)
            {
                health.TakeDamage(damageAmount, gameObject);
            }

            var knockback = collision.GetComponent<Knockback>();
            
            if (shouldApplyKnockback && knockback!=null)
            {              
                knockback.ApplyKnockbackFrom(IsoGround.PointOf(attacker != null ? attacker : transform), knockbackForce);
            }
        }
    }
}