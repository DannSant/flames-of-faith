using Game.Control;
using Game.Progression;
using Game.Scene;
using Game.Utils;
using System;
using UnityEngine;

namespace Game.Combat
{
    [Obsolete]
    public class Sword : MonoBehaviour
    {       
        [SerializeField] private GameObject weaponColliderObject;
        [SerializeField] private CircleCollider2D specialAttackCollider;      
        [SerializeField] private WeaponDamageSource mainDamageSource;
        [SerializeField] private WeaponDamageSource specialDamageSource;
        [Tooltip("Weapon data")]
        [SerializeField] private WeaponData weaponData;
        [SerializeField] private WeaponData specialWeaponData;

        private PlayerInputHandler inputHandler;
        //private InputSystem_Actions inputActions;
        private Animator animator;
        private Animator playerAnimator;
        private PolygonCollider2D weaponCollider;      
        private SpriteRenderer weaponSpriteRenderer;
        private bool attackButtonDown = false;
        private int specialAttackCost = 3;

        //Timers
        private UpdateTimer attackTimer;
        private UpdateTimer specialAttackTimer;
        

        private void Awake()
        {         
            animator = GetComponent<Animator>();
            weaponCollider = weaponColliderObject.GetComponent<PolygonCollider2D>();
            weaponSpriteRenderer = GetComponent<SpriteRenderer>();
            weaponSpriteRenderer.enabled = false;
            attackTimer = new UpdateTimer(1);
            specialAttackTimer = new UpdateTimer(1);
            SetupAttackSpeedVariables();
            

            mainDamageSource.WeaponData = weaponData;
            specialDamageSource.WeaponData = specialWeaponData;
        }


        private void OnDisable()
        {
           
            //PlayerProgression.Instance.onStatUpdated -= Sword_onStatUpdatedEvent;
            //PlayerAnimEventsHandler.Instance.OnSpecialAttackEnded -= EndSpecialAttack;
        }

        private void Start()
        {
            inputHandler = PlayerManager.Instance.GetPlayerComponent<PlayerInputHandler>();
            inputHandler.Player.Attack.performed += ctx => StartAttacking();
            inputHandler.Player.Attack.canceled += ctx => StopAttacking();

            inputHandler.Player.Special.performed += ctx => StartSpecialAttack();

            //playerAnimator = PlayerController.Instance.GetComponent<Animator>();

            //PlayerProgression.Instance.onStatUpdated += Sword_onStatUpdatedEvent;
            //PlayerAnimEventsHandler.Instance.OnSpecialAttackEnded += EndSpecialAttack;

        }

        private void Sword_onStatUpdatedEvent(StatType statType, int value)
        {
           
            if (statType == StatType.AttackSpeed)
            {
                SetupAttackSpeedVariables();
            }
        }

        private void Update()
        {
            ProcessAttackTimers();
            AttackUpdate();
            WeaponColliderLookAtMouse();
        }

        private void StartAttacking()
        {
            attackButtonDown = true;
        }

        private void StopAttacking()
        {
            attackButtonDown = false;
        }

        private void SetupAttackSpeedVariables() {
            //float attackDelay = StatsCalculations.CalculateAttackDelay(PlayerProgression.Instance.GetStatTotal(StatType.AttackSpeed),weaponData.attackCooldownBase, weaponData.attackSpeedScale);
            //attackTimer.SetEventDuration(attackDelay);

            //float specialAttackDelay = StatsCalculations.CalculateAttackDelay(PlayerProgression.Instance.GetStatTotal(StatType.AttackSpeed), specialWeaponData.attackCooldownBase, specialWeaponData.attackSpeedScale);
           //specialAttackTimer.SetEventDuration(specialAttackDelay);
            
        }

        private void StartSpecialAttack()
        {
            //Check cooldown
            if(specialAttackTimer.GetIsEventActive())
            {
                return;
            }

            //check cost
            /*if(PlayerGrace.Instance.CurrentGrace<=specialAttackCost)
            {
                return;
            }
            PlayerGrace.Instance.RemoveGrace(specialAttackCost);*/
            playerAnimator.SetTrigger("SpecialAttack");
            specialAttackTimer.StartEvent();
            specialAttackCollider.enabled = true;
        }

        private void EndSpecialAttack() 
        {
            specialAttackCollider.enabled = false;
        }

        private void AttackUpdate()
        {           
            if (attackButtonDown && CanAttack())
            {
                Attack();
            }
        }

        private void Attack() {
            weaponCollider.enabled = true;
            weaponSpriteRenderer.enabled = true;
            animator.SetTrigger("Attack");           
            attackTimer.StartEvent();
        }

        private void WeaponColliderLookAtMouse()
        {
            /*Vector2 mousePosition = PlayerController.Instance.GetMouseWorldPosition(); // Get the mouse position in world space
            Vector2 direction = (mousePosition - (Vector2)transform.position).normalized;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

           weaponColliderObject.transform.rotation = Quaternion.Euler(0, 0, angle);*/


        }

        private bool CanAttack()
        {
            return !attackTimer.GetIsEventActive();           
        }

        private void ProcessAttackTimers()
        {
            attackTimer.UpdateEvent();
            specialAttackTimer.UpdateEvent();
        }

        public void AttackFinishedAnimEvent()
        {            
            weaponCollider.enabled = false;
            weaponSpriteRenderer.enabled = false;
        }

        public bool IsAttackTimerActive()
        {
            return attackTimer.GetIsEventActive();
        }

        public bool IsSpecialAttackTimerActive()
        {
            return specialAttackTimer.GetIsEventActive();
        }

        public float GetAttackTimer() { 
            return attackTimer.GetEventTimer();
        }

        public float GetSpecialAttackTimer()
        {
            return specialAttackTimer.GetEventTimer();
        }

        public float GetAttackTimerDuration()
        {
            return attackTimer.GetEventDuration();
        }

        public float GetSpecialAttackTimerDuration()
        {
            return specialAttackTimer.GetEventDuration();
        }

    }
}
