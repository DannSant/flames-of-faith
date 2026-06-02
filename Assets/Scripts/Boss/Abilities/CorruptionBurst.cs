using Game.Combat;
using System.Collections;
using UnityEngine;

namespace Game.Boss
{
    [CreateAssetMenu(menuName = "Boss/Abilities/CorruptionBurst")]
    public class CorruptionBurst : BossAbilityBase
    {
        [Header("Projectile")]
        [SerializeField] private GameObject projectilePrefab;

        [Header("Burst Settings")]
        [SerializeField] private float duration = 5f;
        [SerializeField] private float fireInterval = 0.2f;

        [SerializeField] private float spreadAngle = 45f;

        [SerializeField] private float projectileSpeed = 8f;

        [SerializeField] private int projectilesPerShot = 1;

        [Header("Damage")]
        [SerializeField] private float baseDamage = 5f;
        [SerializeField] private float damageScaling = 1f;


        public override IEnumerator Execute(BossController boss, BossAbilityRuntime bossAbilityRuntime, BossAbilityContext context)
        {
            var bossRenderer = boss.GetBossRenderer();
            var player = boss.GetPlayer();
            if (player == null)
                yield break;

            if (!string.IsNullOrEmpty(initialAnimationName)) { 
                bossRenderer.TriggerAnimation(initialAnimationName);
            }

            yield return new WaitForSeconds(delayToStart);

            if (!string.IsNullOrEmpty(animationName))
            {
                if (useTriggerAnimation)
                {
                    bossRenderer.TriggerAnimation(animationName);
                }else
                {
                    bossRenderer.SetAnimationBool(animationName, true);
                }
                
            }
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += fireInterval;

                FireBurstShot(boss, player);

                yield return new WaitForSeconds(fireInterval);
            }
            
            yield return null;

            if (!useTriggerAnimation)
            {
                bossRenderer.SetAnimationBool(animationName, false);
            }

            if (!string.IsNullOrEmpty(endAnimationName))
            {
                bossRenderer.TriggerAnimation(endAnimationName);
            }
            yield return new WaitForSeconds(delayToEnd);
        }

        private void FireBurstShot(BossController boss,Transform player)
        {
            Vector2 baseDirection =
                (player.position - boss.transform.position).normalized;



            Transform castPoint = boss.transform;

            for (int i = 0; i < projectilesPerShot; i++)
            {
                float randomAngle =Random.Range(-spreadAngle, spreadAngle);

                Vector2 finalDirection =  RotateVector(baseDirection, randomAngle);

                GameObject projectile = Instantiate(projectilePrefab, castPoint.position,Quaternion.identity);

                if (projectile.TryGetComponent(out ProjectileMovement movement))
                {
                    movement.SetDirection(finalDirection.normalized);
                }

                float damage =baseDamage + (boss.GetEnrageLevel() * damageScaling);

                if (projectile.TryGetComponent(out EnemyDamage enemyDamage))
                {
                    enemyDamage.SetDamageAmount(Mathf.RoundToInt(damage));
                }
            }


        }
        private Vector2 RotateVector(Vector2 v, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;

            float sin = Mathf.Sin(radians);
            float cos = Mathf.Cos(radians);

            return new Vector2(
                cos * v.x - sin * v.y,
                sin * v.x + cos * v.y
            );
        }
    }
}