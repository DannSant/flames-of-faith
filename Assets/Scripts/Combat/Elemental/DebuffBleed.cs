using Game.Misc;

namespace Game.Combat.Elemental
{
    /// <summary>
    /// Damage over time that stacks instead of refreshing its duration, and detonates for a burst
    /// proportional to the accumulated stacks when it expires.
    /// The stacking itself is handled generically by <see cref="DebuffBase"/>, driven by the
    /// stacking block on the debuff's <see cref="ElementalDebuffData"/>.
    /// </summary>
    public class DebuffBleed : DebuffBase
    {
        private float tickTimer = 0f;

        protected override void OnApplied()
        {
            tickTimer = 0f;
        }

        protected override void Tick(float deltaTime)
        {
            tickTimer += deltaTime;
            if (tickTimer < data.TickInterval) return;

            tickTimer = 0f;
            ApplyTickDamage();
        }

        private void ApplyTickDamage()
        {
            if (enemyHealth == null) return;

            float tickDamage = strength * stacks;

            DamageNumberSpawner.Instance.SpawnBleedDebuffDamageNumber(enemyHealth.transform.position, tickDamage);
            enemyHealth.TakeDamage(new DamageRequest(tickDamage, WeaponClass.Melee, false));
        }

        /// <summary>
        /// The debuff ran its course: detonate for the accumulated stacks.
        /// Note this only runs on expiry, so an enemy killed mid-bleed never bursts.
        /// </summary>
        public override void End()
        {
            if (enemyHealth == null || enemyHealth.IsDead()) return;

            DebuffStackData stacking = data.Stacking;
            float burstDamage = stacks * stacking.BurstDamagePerStack;
            if (burstDamage <= 0f) return;

            SpawnOneShotVfx(stacking.StackVfx, stacking.StackVfxLifetime);
            DamageNumberSpawner.Instance.SpawnBleedDebuffDamageNumber(enemyHealth.transform.position, burstDamage);
            enemyHealth.TakeDamage(new DamageRequest(burstDamage, WeaponClass.Melee, false));
        }
    }

}
