using Game.Combat;
using Game.Common;
using Game.Effects.Abilities;
using UnityEngine;

namespace Game.Control
{
    public class ArcherDash : DashBase
    {
        [Header("Archer Dash - Trap")]
        [Tooltip("Prefab with a trigger Collider2D + DamageSourceBase + TriggerAnimationOnDamage + DestroyObjectOnDamage, spawned at the dash's start position. Traps don't expire; they break when triggered.")]
        [SerializeField] private GameObject trapPrefab;
        [Tooltip("Most traps that can be set at once. Setting another one removes the oldest. Scroll of Trap Step overrides this.")]
        [Min(1)]
        [SerializeField] private int maxActiveTraps = 3;

        private SpawnedObjectLimiter trapLimiter;

        // The dash's own values, used when the player has no Scroll of Trap Step.
        private float defaultDashDuration;
        // Null = keep the trap prefab's own damage.
        private int? trapDamageOverride;
        private int currentMaxTraps;

        protected override void Awake()
        {
            base.Awake();
            defaultDashDuration = dashDuration;
            currentMaxTraps = maxActiveTraps;
        }

        protected override void ApplyAbilityItems()
        {
            var trapStep = GetAbilityValues<TrapStepAbility, TrapStepAbility.Values>();
            SetDashDuration(trapStep != null ? trapStep.dashDuration : defaultDashDuration);
            trapDamageOverride = trapStep?.trapDamage;
            currentMaxTraps = trapStep != null ? trapStep.maxTraps : maxActiveTraps;
        }

        protected override void StartDashing()
        {
            base.StartDashing();
            if (trapPrefab == null) return;

            trapLimiter ??= new SpawnedObjectLimiter(currentMaxTraps);
            trapLimiter.SetMaxCount(currentMaxTraps);

            var trap = Instantiate(trapPrefab, transform.position, Quaternion.identity);
            if (trapDamageOverride.HasValue)
            {
                var damage = trap.GetComponentInChildren<DamageSourceBase>();
                if (damage != null) damage.SetBaseDamage(trapDamageOverride.Value);
            }

            foreach (var oldTrap in trapLimiter.Register(trap))
            {
                Destroy(oldTrap);
            }
        }
    }
}
