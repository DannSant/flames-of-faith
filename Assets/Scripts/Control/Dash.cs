using Game.Combat;
using Game.Common;
using Game.Progression;
using Game.Scene;
using Game.Utils;
using System;
using UnityEngine;

namespace Game.Control
{
    public class Dash : MonoBehaviour, IDependentStateLoader
    {
        [SerializeField] private float defaultCooldown = 2f;
        [SerializeField] private float dashSpeed = 3f;

        public Action<float, float> OnDashTimerUpdated;

        private PlayerInputHandler inputHandler;
        private PlayerController playerController;
        private PlayerProgression playerProgression;
        private TrailRenderer dashTrailRenderer;
        private CharacterVisual characterVisual;
        private Collider2D characterCollider;

        private float dashDuration = .2f;
        private float dashCooldownBase = 2f;
        
        private UpdateTimer dashUpdateTimer;
        private UpdateTimer dashCooldownTimer;


        private void Awake()
        {          
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
            playerProgression = PlayerManager.Instance.GetPlayerComponent<PlayerProgression>();
            inputHandler = PlayerManager.Instance.GetPlayerComponent<PlayerInputHandler>();
            inputHandler.Player.Jump.performed += ctx => StartDashEvent();

            playerProgression.onStatUpdated += Dash_onStatUpdatedEvent;
            playerController = PlayerManager.Instance.GetPlayerComponent<PlayerController>();
        }

        private void OnDisable()
        {
            playerProgression.onStatUpdated -= Dash_onStatUpdatedEvent;
        }


        private void Update()
        {
            dashUpdateTimer.UpdateEvent();
            dashCooldownTimer.UpdateEvent();
            ManageDashTimerEvent();
            //TODO review this later, it should disable collider for invincibility frames but still collide with obstacles
            //CheckAndToggleColliderOnInvincibility();

            // Check if dashing so we set direction in dash direction
            CheckDashDirection();
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
            playerController.ChangeDashMultiplier(dashSpeed);
            dashTrailRenderer.emitting = true;
        }

        private void EndDashing()
        {
            //PlayerController.Instance.ResetDashMultiplier();
            playerController.ResetDashMultiplier();
            dashTrailRenderer.emitting = false;
        }

        private void SetupAttackSpeedVariables()
        {
            float dashCooldown = StatsCalculations.CalculateDashCooldown(playerProgression.GetStatTotal(StatType.DashCooldown), dashCooldownBase);
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

        private void CheckDashDirection()
        {
            if (dashUpdateTimer.GetIsEventActive()) { 
                characterVisual.SetFacingDirection(inputHandler.Player.Move.ReadValue<Vector2>());
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

        public bool isDashActive()
        {
            return dashUpdateTimer.GetIsEventActive();
        }

        public void LoadState()
        {
            SetupAttackSpeedVariables();
        }

        public void SaveState()
        {
            // No need to save, it is saved on the player progression component
        }

        public void ResetState()
        {
            dashCooldownTimer.SetEventDuration(defaultCooldown);
        }
    }
}
