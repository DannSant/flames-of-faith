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
        // Set by the spawner (the scepter, from the Glyph of Fire Tornadoes); none = the explosion deals no damage
        private DamageProfile? explosionDamage;

        /// <summary>The damage of the explosion this spawns (e.g. from an ability item's tier).</summary>
        public void SetExplosionDamage(DamageProfile damage) => explosionDamage = damage;

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
            if (explosionDamage.HasValue && explosionInstance.TryGetComponent<DamageSourceBase>(out var source))
            {
                explosionDamage.Value.ApplyTo(source);
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
