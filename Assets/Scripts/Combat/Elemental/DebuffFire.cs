using Game.Misc;

namespace Game.Combat.Elemental
{
    public class DebuffFire : DebuffBase
    {
        private float tickTimer = 0f;

        protected override void Tick(float deltaTime)
        {
            tickTimer += deltaTime;
            if (tickTimer >= data.TickInterval)
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
            DamageNumberSpawner.Instance.SpawnFireDebuffDamageNumber(enemyHealth.transform.position, finalDamage);

            enemyHealth.TakeDamage(new DamageRequest(finalDamage, WeaponClass.Magic, false));
        }

        protected override void OnApplied()
        {
            tickTimer = 0f;
        }

        public override void End()
        {

        }
    }

}
