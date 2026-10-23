using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

        // Debug: how many times this component subscribed to onDamageTaken (should stay 1)
        private int subscriptions;

        protected override void Awake()
        {
            base.Awake();
            chainVfxPrefab = Resources.Load<GameObject>("VFX/ChainLightningBolt");
        }

        private void OnEnable()
        {
            if (enemyHealth != null)
            {
                enemyHealth.onDamageTaken += HandleDamageTaken;
                subscriptions++;
            }
        }

        private void OnDisable()
        {
            if (enemyHealth != null)
            {
                enemyHealth.onDamageTaken -= HandleDamageTaken;
                subscriptions--;
            }
        }

        protected override void OnApplied()
        {
            Log($"{Label(enemyHealth)} charged (duration {duration:0.##} s, strength {strength:0.##})");
        }

        private void HandleDamageTaken(DamageRequest sourceDamage)
        {
            if (Logging)
            {
                string chain = sourceDamage.leyChargeChainVisited != null
                    ? $"chain {ChainId(sourceDamage.leyChargeChainVisited)}"
                    : "new chain";
                Log($"{Label(enemyHealth)} took {sourceDamage.baseDamage:0.##} {sourceDamage.originWeaponClass} " +
                    $"(depth {sourceDamage.leyChargeChainDepth}/{maxChainDepth}, {chain}, lifesteal {sourceDamage.canTriggerLifeSteal}) " +
                    $"- Ley components on it: {GetComponents<DebuffLeyCharge>().Length}, subscriptions: {subscriptions}");
            }

            if (enemyHealth == null || enemyHealth.IsDead())
            {
                Log($"  {Label(enemyHealth)} is dead: no strike");
                return;
            }
            if (sourceDamage.leyChargeChainDepth >= maxChainDepth)
            {
                Log($"  {Label(enemyHealth)} stops the chain: depth {sourceDamage.leyChargeChainDepth} reached the max {maxChainDepth}");
                return;
            }

            var visited = sourceDamage.leyChargeChainVisited ?? new HashSet<EnemyHealth>();
            visited.Add(enemyHealth);

            // compensateForBodySize: true - a chain hop should reach a big enemy's body for the same
            // reason an attack should, and nothing here is gated on a collider landing the hit.
            EnemyHealth nearest = EnemyTargeting.FindClosest(
                transform.position, chainRadius, compensateForBodySize: true, excluded: visited);
            if (nearest == null)
            {
                Log($"  {Label(enemyHealth)}: no unvisited enemy within {chainRadius} (chain {ChainId(visited)}, {visited.Count} visited)");
                return;
            }

            visited.Add(nearest);

            var debuffHandler = nearest.GetComponent<DebuffHandler>();
            bool targetWasCharged = debuffHandler != null && debuffHandler.GetStacks(ElementalType.LeyCharge) > 0;
            Log($"  {Label(enemyHealth)} strikes {Label(nearest)} for {strength:0.##} " +
                $"(depth {sourceDamage.leyChargeChainDepth + 1}, chain {ChainId(visited)}, target already charged: {targetWasCharged})");

            var chainDamage = new DamageRequest(strength, WeaponClass.Magic, false)
            {
                leyChargeChainDepth = sourceDamage.leyChargeChainDepth + 1,
                leyChargeChainVisited = visited
            };
            nearest.TakeDamage(chainDamage);
            DamageNumberSpawner.Instance.SpawnLeyChargeDebuffDamageNumber(nearest.transform.position, strength);

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

        // --- Debug log (ElementalDebuffData.LogDebug on the Ley Charge asset) ---------------------------------

        private bool Logging => data != null && data.LogDebug;

        private void Log(string message)
        {
            if (!Logging) return;
            // The context object selects the enemy when the log line is clicked; the stack trace shows the caller
            Debug.Log($"[LeyCharge f{Time.frameCount}] {message}", this);
        }

        private static string Label(EnemyHealth enemy) =>
            enemy != null ? $"{enemy.name}#{enemy.GetEntityId()}" : "(none)";

        // One visited set per chain, so its identity names the chain
        private static int ChainId(HashSet<EnemyHealth> visited) => RuntimeHelpers.GetHashCode(visited);
    }
}
