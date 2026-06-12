using Game.Common;
using UnityEngine;
using Game.Control;
using System;
using Game.Progression;
using Game.Utils;
using Game.Waves;
namespace Game.Combat
{
    public class WeaponManager :MonoBehaviour, IDependentStateLoader, IInitializeAfterStateReady
    {
        [SerializeField] private WeaponBase startingWeapon;
        [SerializeField] private bool autoAttackEnabled = false;
        [SerializeField] private CharacterVisual characterVisual;
        private WeaponBase currentWeapon;
        private PlayerProgression playerProgression;
       

        public event Action<float, float> OnAttackTimerUpdated;
        public event Action<float, float> OnSpecialAttackTimerUpdated;

        public bool IsAutoAttackEnabled  { get { return autoAttackEnabled; } private set { autoAttackEnabled = value; } }

        private void Awake()
        {
            playerProgression = GetComponent<PlayerProgression>();
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
            ManageAttackTimer();
            ManageAutoAttack();
        }

        private void ManageAutoAttack()
        {
            if(WaveSpawner.Instance != null && WaveSpawner.Instance.EndingWave == true)
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
            bool isRangeWeapon = currentWeapon.GetWeaponData().weaponClass == WeaponClass.Ranged || currentWeapon.GetWeaponData().weaponClass == WeaponClass.Magic;
            int additionalRange = isRangeWeapon ? playerProgression.GetStatTotal(StatType.Range) : 0;
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, range + additionalRange, LayerMask.GetMask("Enemy"));

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

        public void Attack() => currentWeapon?.Attack();
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