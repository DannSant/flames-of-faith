using Game.Common;
using UnityEngine;

namespace Game.Control
{
    public class ArcherDash : DashBase
    {
        [Header("Archer Dash - Trap")]
        [Tooltip("Prefab with a trigger Collider2D + DamageSourceBase + TriggerAnimationOnDamage + DestroyObjectOnDamage, spawned at the dash's start position. Traps don't expire; they break when triggered.")]
        [SerializeField] private GameObject trapPrefab;
        [Tooltip("Most traps that can be set at once. Setting another one removes the oldest.")]
        [Min(1)]
        [SerializeField] private int maxActiveTraps = 3;

        private SpawnedObjectLimiter trapLimiter;

        protected override void StartDashing()
        {
            base.StartDashing();
            if (trapPrefab == null) return;

            trapLimiter ??= new SpawnedObjectLimiter(maxActiveTraps);
            trapLimiter.SetMaxCount(maxActiveTraps);

            var trap = Instantiate(trapPrefab, transform.position, Quaternion.identity);
            foreach (var oldTrap in trapLimiter.Register(trap))
            {
                Destroy(oldTrap);
            }
        }
    }
}
