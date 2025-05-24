using Game.Common;
using UnityEngine;
using Game.Control;
using System;
namespace Game.Combat
{
    public class WeaponManager :MonoBehaviour
    {
        [SerializeField] private WeaponBase startingWeapon;
        [SerializeField] private bool autoAttackEnabled = false;
        private WeaponBase currentWeapon;

        public event Action<float, float> OnAttackTimerUpdated;
        public event Action<float, float> OnSpecialAttackTimerUpdated;

        public bool IsAutoAttackEnabled  { get { return autoAttackEnabled; } private set { autoAttackEnabled = value; } }

        private void Awake()
        {           
            if (startingWeapon != null)
            {
                EquipWeapon(startingWeapon);                
            }
        }

        private void Start()
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
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, range, LayerMask.GetMask("Enemy"));

            EnemyHealth closest = null;
            float closestDistanceSqr = Mathf.Infinity;

            foreach (var hit in hits)
            {
                EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
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
    }

}