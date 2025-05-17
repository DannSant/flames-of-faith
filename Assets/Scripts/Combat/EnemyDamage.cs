using Game.AI;
using Game.Control;
using Game.Misc;
using UnityEngine;

namespace Game.Combat {
    public class EnemyDamage : MonoBehaviour
    {
        
        [SerializeField] private bool shouldApplyKnockback = true;
        [SerializeField] private float knockbackForce = 15f;

        private int damageAmount = 1;

        public void SetDamageAmount(int amount)
        {
            damageAmount = amount;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            ProcessDamageToPlayer(collision.collider);
        }

        private void ProcessDamageToPlayer(Collider2D collision)
        {
            PlayerHealth health = collision.GetComponent<PlayerHealth>();
            
            if (health != null)
            {
                health.TakeDamage(damageAmount);                
            }
        }
    }
}