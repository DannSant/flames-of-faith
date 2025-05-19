using Game.Combat;
using Game.Common;
using Game.Progression;
using Game.Utils;
using System;
using UnityEngine;

namespace Game.Control
{
    public class Dash : Singleton<Dash>
    {
        [SerializeField] private float dashSpeed = 3f;

        public Action<float, float> OnDashTimerUpdated;

        //private InputSystem_Actions inputActions;
        private PlayerInputHandler inputHandler;
        private TrailRenderer dashTrailRenderer;
        private CharacterVisual characterVisual;
        private Collider2D characterCollider;

        private float dashDuration = .1f;
        private float dashCooldownBase = 2f;
        
        private UpdateTimer dashUpdateTimer;
        private UpdateTimer dashCooldownTimer;

        protected override void Awake()
        {
            base.Awake();
            dashTrailRenderer = GetComponent<TrailRenderer>();
            characterCollider = GetComponent<Collider2D>();
            characterVisual = GetComponentInChildren<CharacterVisual>();
            dashUpdateTimer = new UpdateTimer(dashDuration);
            dashUpdateTimer.OnEventStarted += StartDashing;
            dashUpdateTimer.OnEventComplete += EndDashing;

            dashCooldownTimer = new UpdateTimer(dashCooldownBase);
        }        

        private void Start()
        {
            inputHandler = PlayerInputHandler.Instance;
            inputHandler.Player.Jump.performed += ctx => StartDashEvent();

            PlayerProgression.Instance.onStatUpdated += Dash_onStatUpdatedEvent;
        }

        private void OnDisable()
        {
            PlayerProgression.Instance.onStatUpdated -= Dash_onStatUpdatedEvent;
        }


        private void Update()
        {
            dashUpdateTimer.UpdateEvent();
            dashCooldownTimer.UpdateEvent();
            ManageDashTimerEvent();
            //TODO review this later, it should disable collider for invincibility frames but still collide with obstacles
            //CheckAndToggleColliderOnInvincibility();
        }

        private void Dash_onStatUpdatedEvent(StatType statType, int value)
        {

            if (statType == StatType.DashCooldown)
            {
                SetupAttackSpeedVariables();
            }
        }

        private void StartDashEvent()
        {  
            if (dashCooldownTimer.GetIsEventActive())
            {               
                return;
            }
            dashCooldownTimer.StartEvent();
            dashUpdateTimer.StartEvent();
        }

        private void StartDashing()
        {
            characterVisual.PlayDashAnimation();
            PlayerController.Instance.ChangeDashMultiplier(dashSpeed);
            dashTrailRenderer.emitting = true;
        }

        private void EndDashing()
        {           
            PlayerController.Instance.ResetDashMultiplier();
            dashTrailRenderer.emitting = false;
        }

        private void SetupAttackSpeedVariables()
        {
            float dashCooldown = StatsCalculations.CalculateDashCooldown(PlayerProgression.Instance.GetStatTotal(StatType.DashCooldown), dashCooldownBase);
            dashCooldownTimer.SetEventDuration(dashCooldown);           

        }

        private void ManageDashTimerEvent() 
        { 
            if(dashCooldownTimer.GetIsEventActive())
            {
                float timeLeft = dashCooldownTimer.GetEventTimer();
                float cooldownDuration = dashCooldownTimer.GetEventDuration();
                OnDashTimerUpdated?.Invoke(timeLeft, cooldownDuration);
            }
        }

        private void CheckAndToggleColliderOnInvincibility()
        {
            if (dashUpdateTimer.GetIsEventActive())
            {
                characterCollider.enabled = false; // Disable collider during invincibility
            }
            else
            {
                characterCollider.enabled = true; // Enable collider when not invincible
            }
        }


    }
}
