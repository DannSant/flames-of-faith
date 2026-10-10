using Game.Common;
using UnityEngine;
using Game.Control;
using System;
using Game.Utils;
using Game.Waves;
using Game.GameSettings;
using Game.Scene;
using Game.UI;
namespace Game.Combat
{
    public class WeaponManager :MonoBehaviour, IDependentStateLoader, IInitializeAfterStateReady
    {
        [SerializeField] private WeaponBase startingWeapon;
        [Tooltip("Fallback only: the live value comes from SettingsManager.AutoAttackEnabled when it exists.")]
        [SerializeField] private bool autoAttackEnabled = false;
        [SerializeField] private CharacterVisual characterVisual;

        [Header("Manual Aim Assist (auto-attack off)")]
        [Tooltip("Half-angle, in degrees, of the cone around the right stick's aim that snaps a manual attack onto an enemy in range. 0 = pure free aim.")]
        [SerializeField] private float aimAssistAngleGamepad = 20f;
        [Tooltip("Same as above for mouse aim, which is precise enough to need less help. 0 = pure free aim.")]
        [SerializeField] private float aimAssistAngleMouse = 8f;

        [Header("Target Lock")]
        [Tooltip("Search range when the lock input is pressed with no target, as a multiple of the weapon range.")]
        [SerializeField] private float lockOnRangeMultiplier = 1f;

        private WeaponBase currentWeapon;
        private PlayerHealth playerHealth;
        private PlayerController playerController;

        // The marked target (triangle marker, lock-on, special attacks). Separate from the
        // weapon's own per-shot target, which manual aim assist can set without marking anything.
        private EnemyHealth currentTarget;
        private bool lockActive;
        // How the current lock was made: toggled locks end when their target dies, held locks
        // move on to the nearest enemy while the input is still held.
        private bool lockIsToggled;
        // The enemy currently showing the lock marker (only a locked target shows it).
        private EnemyHealth markedEnemy;

        public event Action<float, float> OnAttackTimerUpdated;
        public event Action<float, float> OnSpecialAttackTimerUpdated;
        public event Action<EnemyHealth> OnTargetChanged;
        public event Action<bool> OnLockChanged;

        public bool IsAutoAttackEnabled  { get { return autoAttackEnabled; } private set { autoAttackEnabled = value; } }
        public EnemyHealth CurrentTarget => currentTarget;
        public bool IsLockActive => lockActive;

        private void Awake()
        {
            playerHealth = GetComponent<PlayerHealth>();
            playerController = GetComponent<PlayerController>();
            if (startingWeapon != null)
            {
                EquipWeapon(startingWeapon);
            }
        }

        public void InitializeAfterStateReady()
        {
            if (currentWeapon != null)
            {
                currentWeapon.Initialize(GetComponentInChildren<CharacterVisual>());
            }

            if (SettingsManager.Instance != null)
            {
                autoAttackEnabled = SettingsManager.Instance.AutoAttackEnabled;
                SettingsManager.Instance.OnAutoAttackChanged -= HandleAutoAttackChanged;
                SettingsManager.Instance.OnAutoAttackChanged += HandleAutoAttackChanged;
            }
        }

        private void OnDisable()
        {
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.OnAutoAttackChanged -= HandleAutoAttackChanged;
            }
        }

        private void HandleAutoAttackChanged(bool enabled)
        {
            autoAttackEnabled = enabled;
        }

        private void Update()
        {
            if (playerHealth != null && playerHealth.IsDead())
            {
                SetLockActive(false, false);
                SetCurrentTarget(null);
                RefreshLockMarker();
                return;
            }
            ManageAttackTimer();
            UpdateCurrentTarget();
            RefreshLockMarker();
            ManageAutoAttack();
        }

        /// <summary>
        /// Hold mode: held state of the lock-on input, pushed by PlayerController every frame.
        /// </summary>
        public void SetLockHeld(bool held)
        {
            if (held == lockActive && !lockIsToggled) return;
            SetLockActive(held, false);
        }

        /// <summary>
        /// Toggle mode: one press of the lock-on input. Locks the current target, or the nearest
        /// enemy in lock range; does nothing if there's none. Pressed again, it unlocks.
        /// </summary>
        public void ToggleLock()
        {
            if (lockActive)
            {
                SetLockActive(false, false);
                return;
            }
            if (currentWeapon == null) return;

            var target = EnemyTargeting.IsTargetable(currentTarget)
                ? currentTarget
                : FindClosestEnemyWithinRange(currentWeapon.GetWeaponRange() * lockOnRangeMultiplier);
            if (target == null) return;

            SetCurrentTarget(target);
            SetLockActive(true, true);
        }

        private void SetLockActive(bool active, bool toggled)
        {
            lockIsToggled = active && toggled;
            if (lockActive == active) return;
            lockActive = active;
            OnLockChanged?.Invoke(lockActive);
        }

        /// <summary>
        /// Locked: keep the target, even out of range. If it dies, a toggled lock ends and a held
        /// lock moves to the nearest enemy in lock range.
        /// Not locked: with auto-attack on the target is the nearest enemy in range (or none),
        /// and with auto-attack off there is no target.
        /// </summary>
        private void UpdateCurrentTarget()
        {
            if (currentWeapon == null)
            {
                SetCurrentTarget(null);
                return;
            }

            float range = currentWeapon.GetWeaponRange();
            if (lockActive)
            {
                if (EnemyTargeting.IsTargetable(currentTarget)) return;

                if (!lockIsToggled)
                {
                    SetCurrentTarget(FindClosestEnemyWithinRange(range * lockOnRangeMultiplier));
                    return;
                }
                SetLockActive(false, false);
            }

            SetCurrentTarget(autoAttackEnabled ? FindClosestEnemyWithinRange(range) : null);
        }

        private void SetCurrentTarget(EnemyHealth target)
        {
            // Unity's null: a destroyed enemy compares equal to null. Normalise it, and compare by
            // reference so a destroyed current target is still replaced (and the event fires).
            if (target == null) target = null;
            if (ReferenceEquals(target, currentTarget)) return;

            currentTarget = target;
            OnTargetChanged?.Invoke(currentTarget);
        }

        /// <summary>The marker only shows on a locked target, so the player can see the lock.</summary>
        private void RefreshLockMarker()
        {
            EnemyHealth shouldShow = lockActive ? currentTarget : null;
            if (shouldShow == null) shouldShow = null;
            if (ReferenceEquals(shouldShow, markedEnemy)) return;

            SetMarkerVisible(markedEnemy, false);
            markedEnemy = shouldShow;
            SetMarkerVisible(markedEnemy, true);
        }

        private static void SetMarkerVisible(EnemyHealth enemy, bool visible)
        {
            if (enemy == null) return;
            var marker = enemy.GetComponentInChildren<TargetMarkerUI>(true);
            if (marker == null) return;
            if (visible) marker.Show(); else marker.Hide();
        }

        /// <summary>
        /// The marked target if it can be attacked right now (within weapon range), else null.
        /// </summary>
        private EnemyHealth GetMarkedTargetInRange()
        {
            if (currentWeapon == null) return null;
            return EnemyTargeting.IsWithinRange(transform.position, currentTarget, currentWeapon.GetWeaponRange(),
                currentWeapon.ShouldCompensateForEnemyBodySize)
                ? currentTarget
                : null;
        }

        private void ManageAutoAttack()
        {
            if(WaveSpawner.Instance != null && WaveSpawner.Instance.EndingWave == true)
            {
                return;
            }
            if (PauseManager.Instance != null && PauseManager.Instance.IsPaused)
            {
                return;
            }
            // Some level types (shop, campfire, event, treasure...) have no combat and
            // don't pause the game, so this blocks auto-attack there too.
            if (GameSession.Instance != null && GameSession.Instance.currentLevel != null && GameSession.Instance.currentLevel.preventAttacks)
            {
                return;
            }
            if (characterVisual.IsSpecialAttackAnimationPlaying)
            {
                return;
            }   
            if (autoAttackEnabled && currentWeapon != null && !currentWeapon.IsAttackTimerActive())
            {
                // Attacks the marked target, so a locked enemy is the one shot at.
                var target = GetMarkedTargetInRange();
                if (target != null)
                {                    
                    currentWeapon.SetTarget(target);
                    currentWeapon.Attack();
                }
            }
        }

        private EnemyHealth FindClosestEnemyWithinRange(float range)
        {
            // The range passed in already includes the player's Attack Range contribution
            // (see WeaponBase.GetWeaponRange). This used to add the stat again on top, which
            // doubled it - and gated it on weaponClass, so a thrown melee weapon could never
            // benefit. Both concerns now live on WeaponData (isRangeBased / rangeScale).
            //
            // The search itself lives in EnemyTargeting, which measures to the enemy's body rather
            // than its transform - this used to rank by centre distance, so a boss standing on top
            // of the player lost to a slime several units away.
            return EnemyTargeting.FindClosest(
                transform.position,
                range,
                currentWeapon != null && currentWeapon.ShouldCompensateForEnemyBodySize);
        }

        private EnemyHealth FindEnemyInAimCone(float range)
        {
            if (playerController == null) return null;

            bool gamepad = InputDeviceManager.Instance != null && InputDeviceManager.Instance.IsGamepadActive;
            return EnemyTargeting.FindInAimCone(
                transform.position,
                range,
                currentWeapon != null && currentWeapon.ShouldCompensateForEnemyBodySize,
                playerController.GetAimDirection(),
                gamepad ? aimAssistAngleGamepad : aimAssistAngleMouse);
        }

        public void EquipWeapon(WeaponBase weapon)
        {
            currentWeapon = weapon;
            currentWeapon?.SetTargetProvider(() => currentTarget);
        }

        private void ManageAttackTimer()
        {
            if (currentWeapon.IsAttackTimerActive())
            {
                OnAttackTimerUpdated?.Invoke(currentWeapon.GetAttackTimer(), currentWeapon.GetAttackTimerDuration());
            }

            if (currentWeapon.IsSpecialAttackTimerActive())
            {
                OnSpecialAttackTimerUpdated?.Invoke(currentWeapon.GetSpecialAttackTimer(), currentWeapon.GetSpecialAttackTimerDuration());
            }
        }

        public void Attack()
        {
            if (currentWeapon == null) return;

            // Re-resolve the target every manual attack so a stale in-range target
            // from auto-attack can't be used once the enemy is out of range.
            // With auto-attack off the player is aiming, so only an enemy near the aim counts;
            // with none, the target is null and ranged weapons fire freely along the aim.
            // A locked target in range always wins.
            var target = lockActive ? GetMarkedTargetInRange() : null;
            if (target == null)
            {
                target = autoAttackEnabled
                    ? FindClosestEnemyWithinRange(currentWeapon.GetWeaponRange())
                    : FindEnemyInAimCone(currentWeapon.GetWeaponRange());
            }
            currentWeapon.SetTarget(target);
            currentWeapon.Attack();
        }
        public void SpecialAttack() => currentWeapon?.SpecialAttack();
        public EnemyHealth GetCurrentTarget() => currentWeapon?.GetTarget();

        public WeaponBase GetCurrentWeapon() => currentWeapon;

        public void LoadState()
        {
           currentWeapon.SetupAttackSpeedVariables();
        }

        public void SaveState()
        {
           //no need to save, it is saved in the player progress component
        }

        public void ResetState()
        {
           //initialized on the weapon base
        }

#if UNITY_EDITOR
        // The acquisition radius, so it can be compared by eye against an enemy's
        // EnemyHealth.targetingBodyRadius gizmo while tuning that value in Play mode.
        private void OnDrawGizmosSelected()
        {
            if (currentWeapon == null) return;

            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.9f);
            Gizmos.DrawWireSphere(transform.position, currentWeapon.GetWeaponRange());
        }
#endif
    }

}