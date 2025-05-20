using Game.Control;
using Game.Progression;
using Game.Scene;
using Game.Utils;
using UnityEngine;
using static Game.Progression.PlayerProgression;

namespace Game.Combat 
{
    public class SwordWeapon : WeaponBase
    {
        [SerializeField] private GameObject weaponColliderObject;
        [SerializeField] private GameObject specialAttackCollider;
        [SerializeField] private DamageSource mainDamageSource;
        [SerializeField] private DamageSource specialDamageSource;
       
        private CharacterVisual characterVisual;
        private PlayerController playerController;
        private PlayerGrace playerGrace;
        private PlayerProgression playerProgression;
        private WeaponManager weaponManager;
        private bool attackButtonDown = false;
        private int specialAttackCost = 3;

        private UpdateTimer attackTimer;
        private UpdateTimer specialAttackTimer;

        public override void Initialize(CharacterVisual characterVisual)
        {
            
            this.characterVisual = characterVisual;
            weaponColliderObject.SetActive(false);
            specialAttackCollider.SetActive(false);

            attackTimer = new UpdateTimer(1);
            specialAttackTimer = new UpdateTimer(1);

            playerController = PlayerManager.Instance.GetPlayerComponent<PlayerController>();
            playerGrace = PlayerManager.Instance.GetPlayerComponent<PlayerGrace>();
            playerProgression = PlayerManager.Instance.GetPlayerComponent<PlayerProgression>();
            weaponManager = PlayerManager.Instance.GetPlayerComponent<WeaponManager>();

            SetupAttackSpeedVariables();

            mainDamageSource.WeaponData = weaponData;
            specialDamageSource.WeaponData = specialWeaponData;

            playerProgression.onStatUpdated += OnStatUpdated;
            characterVisual.OnAttackEndAnimEvent += CharacterVisual_OnAttackEndAnimEvent;
            characterVisual.OnSpecialAttackEndAnimEvent += CharacterVisual_OnSpecialAttackEndAnimEvent;
            
        }
        

        private void OnDisable()
        {
            playerProgression.onStatUpdated -= OnStatUpdated;
            characterVisual.OnAttackEndAnimEvent -= CharacterVisual_OnAttackEndAnimEvent;
            characterVisual.OnSpecialAttackEndAnimEvent -= CharacterVisual_OnSpecialAttackEndAnimEvent;
          
        }

        private void Update()
        {
            ProcessAttackTimers();
            AttackUpdate();
            if (weaponManager.IsAutoAttackEnabled)
            {
                WeaponColliderLookAtTarget();
            }else
            {
                WeaponColliderLookAtMouse();
            }
            
        }

        private void OnStatUpdated(StatType statType, int value)
        {
            if (statType == StatType.AttackSpeed)
            {
                SetupAttackSpeedVariables();
            }
        }

        private void SetupAttackSpeedVariables()
        {
            float attackDelay = StatsCalculations.CalculateAttackDelay(
                playerProgression.GetStatTotal(StatType.AttackSpeed),
                weaponData.attackCooldownBase,
                weaponData.attackSpeedScale);
            attackTimer.SetEventDuration(attackDelay);

            float specialAttackDelay = StatsCalculations.CalculateAttackDelay(
                playerProgression.GetStatTotal(StatType.AttackSpeed),
                specialWeaponData.attackCooldownBase,
                specialWeaponData.attackSpeedScale);
            specialAttackTimer.SetEventDuration(specialAttackDelay);
        }

        public override void Attack()
        {
            if (!attackTimer.GetIsEventActive())
            {
                weaponColliderObject.SetActive(true);

                characterVisual.PlayAttackAnimation();               
                attackTimer.StartEvent();
            }
        }

        public override void SpecialAttack()
        {
            if (specialAttackTimer.GetIsEventActive()) return;
            if (playerGrace.CurrentGrace <= specialAttackCost) return;

            playerGrace.RemoveGrace(specialAttackCost);

            characterVisual.PlayAttackSpecialAnimation();
            specialAttackTimer.StartEvent();
            specialAttackCollider.SetActive(true);
        }

        private void EndSpecialAttack()
        {
            specialAttackCollider.SetActive(false);
        }

        private void AttackUpdate()
        {
            if (attackButtonDown && !attackTimer.GetIsEventActive())
            {
                Attack();
            }
        }

        private void WeaponColliderLookAtTarget()
        {
            if (currentTarget == null) return;

            Vector2 direction = (currentTarget.transform.position - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            weaponColliderObject.transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        private void WeaponColliderLookAtMouse()
        {
            Vector2 mousePosition = playerController.GetMouseWorldPosition();
            Vector2 direction = (mousePosition - (Vector2)transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            weaponColliderObject.transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        private void ProcessAttackTimers()
        {
            attackTimer.UpdateEvent();
            specialAttackTimer.UpdateEvent();
        }

        private void CharacterVisual_OnAttackEndAnimEvent()
        {
            weaponColliderObject.SetActive(false);
        }

        private void CharacterVisual_OnSpecialAttackEndAnimEvent()
        {
           specialAttackCollider.SetActive(false);
        }


        public override float GetWeaponRange() => 3f;

        public override bool IsAttackTimerActive() => attackTimer.GetIsEventActive();
        public override bool IsSpecialAttackTimerActive() => specialAttackTimer.GetIsEventActive(); 
        public override float GetAttackTimer() => attackTimer.GetEventTimer();
        public override float GetSpecialAttackTimer() => specialAttackTimer.GetEventTimer();
        public override float GetAttackTimerDuration() => attackTimer.GetEventDuration();
        public override float GetSpecialAttackTimerDuration() => specialAttackTimer.GetEventDuration();
    }

}