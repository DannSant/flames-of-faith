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
        [SerializeField] private bool autoAttackEnabled = false;
        [SerializeField] private CharacterVisual characterVisual;
        private WeaponBase currentWeapon;
        private PlayerHealth playerHealth;


        public event Action<float, float> OnAttackTimerUpdated;
        public event Action<float, float> OnSpecialAttackTimerUpdated;

        public bool IsAutoAttackEnabled  { get { return autoAttackEnabled; } private set { autoAttackEnabled = value; } }

        private void Awake()
        {
            playerHealth = GetComponent<PlayerHealth>();
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
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, range, LayerMask.GetMask("Enemy","Boss"));

            EnemyHealth closest = null;
            float closestDistanceSqr = Mathf.Infinity;

            foreach (var hit in hits)
            {
                EnemyHealth enemy = hit.GetComponent<EnemyHealth>();

                if (enemy == null)
                {
                    continue; // Skip if no EnemyHealth component found
                }   

                if (enemy.IsImmune() || enemy.IsDead())
                {
                    continue; // Skip immune or dead enemies
                }

                if (enemy != null) // Optional: check if alive
                {
                    float distanceSqr = (enemy.transform.position - transform.position).sqrMagnitude;
                    if (distanceSqr < closestDistanceSqr)
                    {
                        closest = enemy;
                        closestDistanceSqr = distanceSqr;
                    }
                }
            }

            return closest;
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
            var target = FindClosestEnemyWithinRange(currentWeapon.GetWeaponRange());
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

        
    }

}