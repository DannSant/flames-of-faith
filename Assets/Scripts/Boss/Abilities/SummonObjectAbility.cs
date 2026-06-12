using Game.Combat;
using Game.Combat.Projectiles;
using System.Collections;
using UnityEngine;

namespace Game.Boss
{
    [Tooltip("Summons an object at a specified location.")]
    [CreateAssetMenu(menuName = "Boss/Abilities/SummonObject")]
    public class SummonObjectAbility : BossAbilityBase
    {
        [SerializeField] private GameObject objectPrefab;
        [SerializeField] private float baseDamage = 10;
        [SerializeField] private float castDelay = 1.5f;

        public override IEnumerator Execute(BossController boss, BossAbilityRuntime bossAbilityRuntime, BossAbilityContext context)
        {
            var bossRenderer = boss.GetBossRenderer();
            var player = boss.GetPlayer();

            if (bossRenderer != null && animationName != "")
            {
                bossRenderer.TriggerAnimation(animationName);
            }

            yield return new WaitForSeconds(castDelay); // cast delay

            var dir = (player.position - boss.transform.position).normalized;
            var castPoint = boss.GetCurrentCastPoint(dir);

            GameObject projectile = Instantiate(objectPrefab, castPoint.position, Quaternion.identity);
            Vector2 direction = (player.position - castPoint.position).normalized;

            if (projectile.TryGetComponent(out ProjectileMovementBase movement))
            {
                movement.Initialize(direction);
                movement.SetTarget(player);
            }

            if (projectile.TryGetComponent(out EnemyDamage damage))
            {
                int damageAmount = Mathf.RoundToInt(baseDamage);
                damage.SetDamageAmount(damageAmount);
            }

            if (projectile.TryGetComponent(out EnemyTriggerDamage damageTrigger))
            {
                int damageAmount = Mathf.RoundToInt(baseDamage);
                damageTrigger.SetDamageAmount(damageAmount);
            }
        }
    }

}