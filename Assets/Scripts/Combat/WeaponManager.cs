using Game.Common;
using UnityEngine;
using Game.Control;
using System;
using Game.Utils;
using Game.Waves;
using Game.GameSettings;
using Game.Scene;
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

        private WeaponBase currentWeapon;
        private PlayerHealth playerHealth;
        private PlayerController playerController;


        public event Action<float, float> OnAttackTimerUpdated;
        public event Action<float, float> OnSpecialAttackTimerUpdated;

        public bool IsAutoAttackEnabled  { get { return autoAttackEnabled; } private set { autoAttackEnabled = value; } }

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
                return;
            }
            ManageAttackTimer();
            ManageAutoAttack();
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
                var target = FindClosestEnemyWithinRange(currentWeapon.GetWeaponRange());
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
            var target = autoAttackEnabled
                ? FindClosestEnemyWithinRange(currentWeapon.GetWeaponRange())
                : FindEnemyInAimCone(currentWeapon.GetWeaponRange());
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