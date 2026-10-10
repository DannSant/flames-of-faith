using Game.Misc;
using UnityEngine;

namespace Game.Combat {
    public class EnemyTriggerDamage : MonoBehaviour
    {

       private float damageAmount = 1f;
        
        public void SetDamageAmount(float value)
        {
            damageAmount = Mathf.Max(0f, value);
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
                health.TakeDamage(damageAmount, gameObject);
            }
        }
    }

}