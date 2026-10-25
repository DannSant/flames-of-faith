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
        [Tooltip("Most traps that can be set at once. Setting another one removes the oldest. Glyph of Trap Step overrides this.")]
        [Min(1)]
        [SerializeField] private int maxActiveTraps = 3;

        private SpawnedObjectLimiter trapLimiter;

        // The dash's own values, used when the player has no Glyph of Trap Step.
        private float defaultDashDuration;
        // The trap's damage from the Glyph of Trap Step; none without it (every Archer kit has it).
        private DamageProfile trapDamage;
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
            trapDamage = trapStep != null ? new DamageProfile(trapStep.trapDamage, dashDamageStat, trapStep.trapStatScale) : default;
            currentMaxTraps = trapStep != null ? trapStep.maxTraps : maxActiveTraps;
        }

        protected override void StartDashing()
        {
            base.StartDashing();
            if (trapPrefab == null) return;

            trapLimiter ??= new SpawnedObjectLimiter(currentMaxTraps);
            trapLimiter.SetMaxCount(currentMaxTraps);

            var trap = Instantiate(trapPrefab, transform.position, Quaternion.identity);
            trapDamage.ApplyTo(trap.GetComponentInChildren<DamageSourceBase>());

            foreach (var oldTrap in trapLimiter.Register(trap))
            {
                Destroy(oldTrap);
            }
        }
    }
}
