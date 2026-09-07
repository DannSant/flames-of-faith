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

        private void HandleDamageTaken(DamageRequest sourceDamage)
        {
            if (enemyHealth == null || enemyHealth.IsDead()) return;
            if (sourceDamage.leyChargeChainDepth >= maxChainDepth) return;

            var visited = sourceDamage.leyChargeChainVisited ?? new HashSet<EnemyHealth>();
            visited.Add(enemyHealth);

            EnemyHealth nearest = FindNearestEnemy(visited);
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

        private EnemyHealth FindNearestEnemy(HashSet<EnemyHealth> excluded)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, chainRadius, LayerMask.GetMask("Enemy", "Boss"));

            EnemyHealth closest = null;
            float closestDistanceSqr = Mathf.Infinity;

            foreach (var hit in hits)
            {
                EnemyHealth candidate = hit.GetComponent<EnemyHealth>();
                if (candidate == null || candidate.IsImmune() || candidate.IsDead()) continue;
                if (excluded.Contains(candidate)) continue;

                float distanceSqr = ((Vector2)candidate.transform.position - (Vector2)transform.position).sqrMagnitude;
                if (distanceSqr < closestDistanceSqr)
                {
                    closest = candidate;
                    closestDistanceSqr = distanceSqr;
                }
            }

            return closest;
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
