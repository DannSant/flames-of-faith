using System.Collections.Generic;
using Game.Misc;
using Game.VFX;
using UnityEngine;

namespace Game.Combat.Elemental
{
    public class DebuffLeyCharge : DebuffBase
    {
        [SerializeField] private float chainRadius = 6f;
        [SerializeField] private int maxChainDepth = 3;

        private GameObject chainVfxPrefab;

        protected override void Awake()
        {
            base.Awake();
            chainVfxPrefab = Resources.Load<GameObject>("VFX/ChainLightningBolt");
        }

        private void OnEnable()
        {
            if (enemyHealth != null)
                enemyHealth.onDamageTaken += HandleDamageTaken;
        }

        private void OnDisable()
        {
            if (enemyHealth != null)
                enemyHealth.onDamageTaken -= HandleDamageTaken;
        }

        /// <summary>
        /// A charged enemy strikes when it is hit (an attack or a chain hop below the max depth), never on
        /// a debuff's own damage (a burning enemy struck its closest neighbour on every 0.5 s burn tick) or
        /// a silent forced kill (the wave end clearing the enemies left).
        /// </summary>
        public static bool ShouldStrike(DamageRequest sourceDamage, int maxChainDepth)
        {
            if (sourceDamage == null || sourceDamage.isDamageOverTime || sourceDamage.silent) return false;
            return sourceDamage.leyChargeChainDepth < maxChainDepth;
        }

        private void HandleDamageTaken(DamageRequest sourceDamage)
        {
            if (enemyHealth == null || enemyHealth.IsDead()) return;
            if (!ShouldStrike(sourceDamage, maxChainDepth)) return;

            var visited = sourceDamage.leyChargeChainVisited ?? new HashSet<EnemyHealth>();
            visited.Add(enemyHealth);

            // compensateForBodySize: true - a chain hop should reach a big enemy's body for the same
            // reason an attack should, and nothing here is gated on a collider landing the hit.
            EnemyHealth nearest = EnemyTargeting.FindClosest(
                transform.position, chainRadius, compensateForBodySize: true, excluded: visited);
            if (nearest == null) return;

            visited.Add(nearest);

            var chainDamage = new DamageRequest(strength, WeaponClass.Magic, false)
            {
                leyChargeChainDepth = sourceDamage.leyChargeChainDepth + 1,
                leyChargeChainVisited = visited
            };
            nearest.TakeDamage(chainDamage);
            DamageNumberSpawner.Instance.SpawnLeyChargeDebuffDamageNumber(nearest.transform.position, strength);

            var debuffHandler = nearest.GetComponent<DebuffHandler>();
            if (debuffHandler != null)
                debuffHandler.ApplyDebuff(data, duration, strength);

            SpawnChainVfx(transform.position, nearest.transform.position);
        }

        private void SpawnChainVfx(Vector3 from, Vector3 to)
        {
            if (chainVfxPrefab == null) return;
            var vfx = Instantiate(chainVfxPrefab, from, Quaternion.identity);
            if (vfx.TryGetComponent<ChainLightningBolt>(out var bolt))
            {
                bolt.Init(from, to);
            }
        }
    }
}
