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
        public void Initialize(float damage)
        {
            this.damage = damage;
        }

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

            float angleStep = 360f / projectileCount;
            float randomOffset = Random.Range(0f, 360f);
            for (int i = 0; i < projectileCount; i++)
            {
                float angle = randomOffset + i * angleStep;
                float rad = angle * Mathf.Deg2Rad;
                Vector2 direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;
                GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
              
                var projectileDamage = projectile.GetComponent<EnemyTriggerDamage>();
                if (projectileDamage != null)
                {
                    projectileDamage.SetDamageAmount(Mathf.CeilToInt(damage));
                }
                if (projectile.TryGetComponent(out ProjectileMovement movement)) {
                    movement.SetDirection(direction);
                }
                  
                Destroy(gameObject);
            }



        }

        private IEnumerator RadiousExplotionRoutine()
        {
            yield return new WaitForSeconds(delayBeforeExplotion);

        }
    }

}