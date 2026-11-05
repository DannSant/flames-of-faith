using Game.AI;
using Game.Combat;
using System.Collections;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

namespace Game.Boss
{
    [CreateAssetMenu(menuName = "Boss/Abilities/VoltOfDarkness")]
    public class VoltOfDarknessAbility : BossAbilityBase
    {
        public GameObject projectilePrefab;

        [SerializeField]
        private float projectileSpeed = 6f;

        [SerializeField]
        private float baseDamage = 10f;

        [SerializeField]
        private float damageScaling = .5f;

        [SerializeField]
        private int projectileNumberScaling = 1;

        [SerializeField]
        private int maxProjectileCount = 5;

        [SerializeField]
        private int baseProjectileCount = 1;

        [SerializeField]
        private float animationWindup = 0.95f;

        [Tooltip("Share of the bolt's damage each smaller projectile deals when the bolt splits (its EnemyExplotion).")]
        [Range(0f, 1f)]
        [SerializeField]
        private float fragmentDamageFraction = 0.5f;

        public float FragmentDamageFraction => fragmentDamageFraction;

        /// <summary>Damage of each projectile the bolt splits into.</summary>
        public static float FragmentDamage(float boltDamage, float fraction) =>
            Mathf.Max(0f, boltDamage) * Mathf.Clamp01(fraction);

        public override IEnumerator Execute(BossController boss, BossAbilityRuntime bossAbilityRuntime, BossAbilityContext context)
        {
            var bossRenderer = boss.GetBossRenderer();
            var player = boss.GetPlayer();

            var ability = bossAbilityRuntime.GetBossAbility();

            if (player == null) yield break;


            yield return new WaitForSeconds(ability.delayToStart);

            int projectilesToSpawn = Mathf.Min(baseProjectileCount + (boss.GetEnrageLevel() * projectileNumberScaling), maxProjectileCount);

            for (int i = 0; i < projectilesToSpawn; i++) {
                var dir = (player.position - boss.transform.position).normalized;
                var castPoint = boss.GetCurrentCastPoint(dir);
                //Debug.Log($"castPoint {castPoint.name} - Boss:{boss.transform.position}- Player:{player.position}- direction {dir}");

                // Trigger animation
                if (bossRenderer != null && animationName != "")
                {
                    bossRenderer.TriggerAnimation(animationName);
                }

                // Wait for animation windup
                yield return new WaitForSeconds(animationWindup);

                GameObject projectile = Instantiate(projectilePrefab, castPoint.position, Quaternion.identity);
                Vector2 direction = (player.position - castPoint.position).normalized;

                if (projectile.TryGetComponent(out ProjectileMovement movement))
                    movement.SetDirection(direction);

                int damageAmount = boss.GetAbilityDamage(baseDamage + (boss.GetEnrageLevel() * damageScaling));

                if (projectile.TryGetComponent(out EnemyDamage damage))
                {
                    damage.SetDamageAmount(damageAmount);
                }

                if (projectile.TryGetComponent(out EnemyTriggerDamage damageTrigger))
                {
                    damageTrigger.SetDamageAmount(damageAmount);
                }

                // The bolt splits into smaller projectiles later; they take their damage from here, or they deal none
                if (projectile.TryGetComponent(out EnemyExplotion explosion))
                {
                    explosion.Initialize(FragmentDamage(damageAmount, fragmentDamageFraction));
                }
            }

            yield return new WaitForSeconds(ability.delayToStart);
        }
    }
}