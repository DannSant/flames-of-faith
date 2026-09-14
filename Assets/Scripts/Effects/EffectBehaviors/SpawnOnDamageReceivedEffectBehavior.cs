using Game.Combat;
using Game.Scene;
using UnityEngine;

namespace Game.Effects.EffectBehaviors
{
    [CreateAssetMenu(menuName = "Effects/Behaviors/Spawn On Damage Received")]
    public class SpawnOnDamageReceivedEffectBehavior : EffectBehavior
    {
        [Header("Spawn Settings")]
        [SerializeField] private GameObject prefabToSpawn;

        [Tooltip("Upper bound on how many instances can spawn per proc. Each one from 0 to this count independently rolls chanceToSpawn.")]
        [SerializeField] private int maxSpawnCount = 5;

        [SerializeField] private Vector2 spawnOffset = Vector2.zero;

        [SerializeField] private bool randomRotation = true;
        [SerializeField] private bool randomPositionOffset = false;

        [Range(0f, 1f)]
        [SerializeField] private float chanceToSpawn = 0.3f;

        [Header("Cooldown")]
        [Tooltip("Minimum time between procs, so a burst of hits taken at once can't roll this repeatedly.")]
        [SerializeField] private float cooldown = 1.5f;

        // Per-owner runtime state - see EffectBehaviorContext for why this can't live directly
        // on this ScriptableObject (the cooldown timestamp in particular would otherwise persist
        // across runs, since this asset is a single shared instance reused for the whole session).
        private class RuntimeState
        {
            public PlayerHealth playerHealth;
            public float nextAvailableTime;
        }

        public override void Initialize(EffectBehaviorContext context, Effect parentEffect)
        {
            base.Initialize(context, parentEffect);

            var state = GetState<RuntimeState>();
            state.playerHealth = PlayerManager.Instance.GetPlayerComponent<PlayerHealth>();
            if (state.playerHealth == null)
                return;

            state.playerHealth.onDamageTaken += HandleDamageTaken;
        }

        public override void Cleanup()
        {
            var state = GetState<RuntimeState>();
            if (state.playerHealth != null)
                state.playerHealth.onDamageTaken -= HandleDamageTaken;
        }

        private void HandleDamageTaken(float damageAmount, GameObject attacker)
        {
            var state = GetState<RuntimeState>();

            if (Time.time < state.nextAvailableTime)
                return;

            state.nextAvailableTime = Time.time + cooldown;

            // Number of rolls depends on effect stack count
            int finalMaxSpawnCount = maxSpawnCount * storeOwner.GetEffectMultiplierConfig(parentEffect.EffectID).count;

            for (int i = 0; i < finalMaxSpawnCount; i++)
            {
                if (Random.value <= chanceToSpawn)
                {
                    SpawnInstance();
                }
            }
        }

        private void SpawnInstance()
        {
            Quaternion rot = randomRotation
                ? Quaternion.Euler(0, 0, Random.Range(0, 360))
                : Quaternion.identity;

            Vector2 spawnPos = (Vector2)ownerObject.transform.position + spawnOffset;
            if (randomPositionOffset)
            {
                Vector2 randomOffsetVec = new Vector2(
                    Random.Range(-spawnOffset.x, spawnOffset.x),
                    Random.Range(-spawnOffset.y, spawnOffset.y)
                );
                spawnPos += randomOffsetVec;
            }

            GameObject instance = Instantiate(prefabToSpawn, spawnPos, rot);

            if (instance.TryGetComponent<IEffectMultiplier>(out var multiplier))
            {
                multiplier.SetEffectID(parentEffect.EffectID);
            }
        }
    }
}
