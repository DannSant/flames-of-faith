using Game.Combat;
using System.Collections;
using UnityEngine;

namespace Game.Boss
{
    [CreateAssetMenu(menuName = "Boss/Abilities/VoltOfDarkness")]
    public class VoltOfDarknessAbility : BossAbilityBase
    {
        public GameObject projectilePrefab;
        public float projectileSpeed = 6f;
        public float baseDamage = 10;
        public float damageScaling = .5f;
        public int projectileNumberScaling = 1;


        public override IEnumerator Execute(BossController boss, BossAbilityRuntime bossAbilityRuntime, BossAbilityContext context)
        {
            var bossRenderer = boss.GetBossRenderer();
            var player = boss.GetPlayer();
            var castPoint = boss.GetCurrentCastPoint();
            Debug.Log($"castPoint {castPoint.name}");
            var ability = bossAbilityRuntime.GetBossAbility();

            if (player == null) yield break;


            yield return new WaitForSeconds(ability.delayToStart);

            int projectilesToSpawn = 1 + (boss.GetEnrageLevel() * projectileNumberScaling);

            for (int i = 0; i < projectilesToSpawn; i++) {
                // Trigger animation
                if (bossRenderer != null && animationName != "")
                {
                    bossRenderer.TriggerAnimation(animationName);
                }

                // Wait for animation windup
                yield return new WaitForSeconds(.95f);

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