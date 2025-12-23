using Game.Misc;
using UnityEngine;

namespace Game.Combat.Elemental
{
    public class DebuffFire : DebuffBase
    {
        //[SerializeField] private float baseTickDamage = 2f;
        [SerializeField] private float tickInterval = 0.5f;

        private float tickTimer = 0f;

        protected override void Update()
        {
            base.Update(); // handles duration countdown

            tickTimer += Time.deltaTime;
            if (tickTimer >= tickInterval)
            {
                tickTimer = 0f;
                ApplyTickDamage();
            }
        }

        private void ApplyTickDamage()
        {
            if (enemyHealth == null) return;

            float finalDamage = strength;

            // spawn damage numbers
            DamageNumberSpawner.Instance.SpawnFireDebuffDamageNumber(enemyHealth.transform.position,finalDamage );

            enemyHealth.TakeDamage(new DamageRequest(finalDamage, WeaponClass.Magic, false));
        }

        public override void Initialize(float duration, float strength)
        {
            base.Initialize(duration, strength);
            tickTimer = 0f;
        }

        public override void End()
        {
            
        }
    }

}