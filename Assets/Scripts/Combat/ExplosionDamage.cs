using Game.Misc;
using UnityEngine;

namespace Game.Combat
{
    [RequireComponent(typeof(DamageSourceBase))]
    public class ExplosionDamage : MonoBehaviour
    {
        [Header("Explosion")]
        [SerializeField] private GameObject explosionVfxPrefab;
        [Tooltip("Short-lived prefab with its own DamageSourceBase + a large trigger collider, used to damage everything around the primary target.")]
        [SerializeField] private GameObject explosionDamagePrefab;
        [SerializeField] private float explosionDamageLifetime = 0.1f;

        private DamageSourceBase damageSource;
        private bool hasExploded = false;
        // Null = the explosion prefab's own damage
        private int? explosionDamageOverride;

        /// <summary>Base damage of the explosion this spawns (e.g. from an ability item's tier).</summary>
        public void SetExplosionDamage(int baseDamage) => explosionDamageOverride = baseDamage;

        private void Awake()
        {
            damageSource = GetComponent<DamageSourceBase>();
        }

        private void OnEnable()
        {
            damageSource.OnDamageDealtEvent += HandleDamageDealt;
        }

        private void OnDisable()
        {
            damageSource.OnDamageDealtEvent -= HandleDamageDealt;
        }

        private void HandleDamageDealt(float damage, GameObject target)
        {
            if (hasExploded) return;
            hasExploded = true;

            Explode(target.transform.position);
        }

        private void Explode(Vector3 position)
        {
            if (explosionVfxPrefab != null)
            {
                Instantiate(explosionVfxPrefab, position, Quaternion.identity);
            }

            if (explosionDamagePrefab == null) return;

            var explosionInstance = Instantiate(explosionDamagePrefab, position, Quaternion.identity);
            if (explosionDamageOverride.HasValue && explosionInstance.TryGetComponent<DamageSourceBase>(out var explosionDamage))
            {
                explosionDamage.SetBaseDamage(explosionDamageOverride.Value);
            }
            if (explosionInstance.TryGetComponent<DestroyAfterTime>(out var destroyAfterTime))
            {
                destroyAfterTime.StartDestroyTimer(explosionDamageLifetime);
            }
            else
            {
                Destroy(explosionInstance, explosionDamageLifetime);
            }
        }
    }
}
