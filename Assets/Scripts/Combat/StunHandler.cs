using Game.Enemies;
using System;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// Timed "unable to act" state for an enemy, modeled on <see cref="Misc.Knockback"/>: this
    /// component owns the timer and the presentation, and the AI (<see cref="AI.Behaviors.BehaviorController"/>)
    /// voluntarily respects <see cref="IsStunned"/>.
    ///
    /// It deliberately knows nothing about what applied the stun, so a trap, a weapon, an item
    /// effect or a future Holy elemental debuff can all just call <see cref="ApplyStun"/>.
    /// </summary>
    public class StunHandler : MonoBehaviour
    {
        [Header("VFX")]
        [SerializeField] private GameObject stunVfxPrefab;

        [Tooltip("Where the stun VFX spawns - typically an empty child above the head. Sprite sizes " +
            "vary a lot between variants, so each one positions its own anchor. Falls back to this " +
            "object's origin when not assigned.")]
        [SerializeField] private Transform stunVfxAnchor;

        [Header("Settings")]
        [Tooltip("Freezes the animator so the sprite visibly locks in place for the duration.")]
        [SerializeField] private bool freezeAnimator = true;

        [Tooltip("If true, a stunned enemy also stops dealing contact damage to the player. " +
            "Off by default: a stunned enemy can't act, but its body is still a hazard.")]
        [SerializeField] private bool blockContactDamageWhileStunned = false;

        private Animator animator;
        private EnemyAnimationController animationController;
        private EnemyHealth enemyHealth;

        private float stunTimer;
        private GameObject vfxInstance;
        private float cachedAnimatorSpeed = 1f;

        public bool IsStunned { get; private set; }
        public bool BlockContactDamageWhileStunned => blockContactDamageWhileStunned;

        public event Action OnStunStarted;
        public event Action OnStunEnded;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            animationController = GetComponent<EnemyAnimationController>();
            enemyHealth = GetComponent<EnemyHealth>();
        }

        private void Update()
        {
            if (!IsStunned) return;

            // Death has its own delay before the GameObject is destroyed; a frozen animator would
            // otherwise hold the corpse mid-pose for that whole window.
            if (enemyHealth != null && enemyHealth.IsDead())
            {
                ClearStun();
                return;
            }

            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0f)
            {
                ClearStun();
            }
        }

        /// <summary>
        /// Stuns the target for at least <paramref name="duration"/> seconds. Re-applying extends an
        /// active stun instead of being swallowed, so a second source can never cut the first short.
        /// </summary>
        public void ApplyStun(float duration)
        {
            if (duration <= 0f) return;

            stunTimer = Mathf.Max(stunTimer, duration);

            if (IsStunned) return;

            IsStunned = true;
            FreezeAnimation();
            SpawnVfx();
            OnStunStarted?.Invoke();
        }

        public void ClearStun()
        {
            if (!IsStunned) return;

            IsStunned = false;
            stunTimer = 0f;
            RestoreAnimation();
            DespawnVfx();
            OnStunEnded?.Invoke();
        }

        private void FreezeAnimation()
        {
            // EnemyAnimationController writes the direction parameters every frame, and blend-tree
            // pose is parameter-driven rather than time-driven - left running, a "frozen" enemy
            // would still visibly pivot to keep tracking the player.
            if (animationController != null) animationController.enabled = false;

            if (!freezeAnimator || animator == null) return;

            cachedAnimatorSpeed = animator.speed;
            animator.speed = 0f;
        }

        private void RestoreAnimation()
        {
            if (animationController != null) animationController.enabled = true;

            if (!freezeAnimator || animator == null) return;

            animator.speed = cachedAnimatorSpeed;
        }

        private void SpawnVfx()
        {
            if (stunVfxPrefab == null || vfxInstance != null) return;

            Transform anchor = stunVfxAnchor != null ? stunVfxAnchor : transform;
            vfxInstance = Instantiate(stunVfxPrefab, anchor.position, Quaternion.identity, anchor);
        }

        private void DespawnVfx()
        {
            if (vfxInstance == null) return;

            Destroy(vfxInstance);
            vfxInstance = null;
        }

        private void OnDestroy()
        {
            if (IsStunned && freezeAnimator && animator != null)
            {
                animator.speed = cachedAnimatorSpeed;
            }
        }
    }
}
