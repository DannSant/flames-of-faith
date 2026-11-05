using Game.Combat;
using System.Collections;
using UnityEngine;

namespace Game.AI
{
    public enum ExplotionType {
        Projectile,
        Radius
    }
    public class EnemyExplotion : MonoBehaviour
    {
        [Header("Explotion Settings")]
        [SerializeField] private ExplotionType explotionType;
        [Tooltip("Delay before explotion in seconds. Should match animation time if there is one")]
        [SerializeField] private float delayBeforeExplotion=0.5f;

        [Header("Projectile Settings")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float projectileCount;

        [Header("Radius Settings")]
        [SerializeField] private float explotionRadius;

        [Header("VFX Settings")]
        [SerializeField] private GameObject explotionVFX;

        private float damage;
        private bool damageSet;

        /// <summary>
        /// The damage each spawned projectile deals. Whoever spawns this object must call it: the on-death explosion
        /// behavior and the boss's Bolt of Darkness do. Without it the projectiles deal no damage.
        /// </summary>
        public void Initialize(float damage)
        {
            this.damage = damage;
            damageSet = true;
        }

        public float Damage => damage;

        private void Start()
        {
            if(explotionType == ExplotionType.Projectile)
            {
                StartCoroutine(ProjectileExplotionRoutine());
            }
            else if (explotionType == ExplotionType.Radius)
            {
                StartCoroutine(RadiousExplotionRoutine());
            }
        }

        private IEnumerator ProjectileExplotionRoutine()
        {
            yield return new WaitForSeconds(delayBeforeExplotion);

            if (!damageSet)
            {
                Debug.LogWarning($"{name}: exploded without Initialize(damage), so its projectiles deal no damage.", this);
            }

            float angleStep = 360f / projectileCount;
            float randomOffset = Random.Range(0f, 360f);
            for (int i = 0; i < projectileCount; i++)
            {
                float angle = randomOffset + i * angleStep;
                float rad = angle * Mathf.Deg2Rad;
                Vector2 direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;
                GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                if (CorruptedDamageSource.IsCorrupted(gameObject))
                {
                    CorruptedDamageSource.Mark(projectile);
                }
              
                var projectileDamage = projectile.GetComponent<EnemyTriggerDamage>();
                if (projectileDamage != null)
                {
                    projectileDamage.SetDamageAmount(damage);
                }
                if (projectile.TryGetComponent(out ProjectileMovement movement)) {
                    movement.SetDirection(direction);
                }
            }

            Destroy(gameObject);



        }

        private IEnumerator RadiousExplotionRoutine()
        {
            yield return new WaitForSeconds(delayBeforeExplotion);

        }
    }

}