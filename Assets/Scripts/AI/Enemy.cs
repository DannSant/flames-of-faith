using Game.Combat;
using UnityEngine;

namespace Game.AI {
    public class Enemy : MonoBehaviour
    {
        [SerializeField]
        private EnemyType enemyType;

        private EnemyAIBase ai;
        private EnemyHealth health;
        private EnemyDamage damage;

        private void Awake()
        {
            ai = GetComponent<EnemyAIBase>();
            health = GetComponent<EnemyHealth>();
            damage = GetComponent<EnemyDamage>();
        }

        public void Initialize(int waveNumber)
        {
            int calculatedDamage = 1 + (waveNumber - 1);
            int calculatedHealth = 5 + ((waveNumber - 1) * 2);

            damage.SetDamageAmount(calculatedDamage);
            health.SetMaxHealth(calculatedHealth);
            ai.SetProjectileDamage(calculatedDamage); // Shooter will use this value

            //Debug.Log($"Enemy initialized (Wave {waveNumber}) - Damage: {calculatedDamage}, Health: {calculatedHealth}");
        }
    }

}