using Game.Misc;
using UnityEngine;

namespace Game.Combat {
    public class EnemyTriggerDamage : MonoBehaviour
    {

       private int damageAmount = 1; 
        
        public void SetDamageAmount(int value)
        {
            damageAmount = Mathf.Max(1, value); // Ensure it's always at least 1
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            ProcessDamageToPlayer(collision);
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