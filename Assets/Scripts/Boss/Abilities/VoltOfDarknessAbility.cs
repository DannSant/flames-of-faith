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

                if (projectile.TryGetComponent(out EnemyDamage damage))
                {
                    int damageAmount = Mathf.RoundToInt(baseDamage + (boss.GetEnrageLevel() * damageScaling));
                    damage.SetDamageAmount(damageAmount);
                }

                if (projectile.TryGetComponent(out EnemyTriggerDamage damageTrigger))
                {
                    int damageAmount = Mathf.RoundToInt(baseDamage + (boss.GetEnrageLevel() * damageScaling));
                    damageTrigger.SetDamageAmount(damageAmount);
                }
            }

            yield return new WaitForSeconds(ability.delayToStart);
        }
    }
}