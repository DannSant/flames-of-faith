using Game.Combat;
using Game.Common;
using Game.Progression;
using Game.Scene;
using Game.Utils;
using Game.Waves;
using Game.GameSettings;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Control
{
    public abstract class DashBase : MonoBehaviour, IDependentStateLoader
    {
        [SerializeField] protected float dashCooldownBase = 2f;
        [SerializeField] protected float dashSpeed = 3f;
        [SerializeField] protected float dashDuration = .2f;
        [SerializeField] protected int maxCharges = 1;

        public Action<float, float> OnDashTimerUpdated;
        public Action<int, int> OnChargesUpdated;

        protected PlayerInputHandler inputHandler;
        protected PlayerController playerController;
        protected PlayerProgression playerProgression;
        protected TrailRenderer dashTrailRenderer;
        protected CharacterVisual characterVisual;
        protected Collider2D characterCollider;
        protected PlayerHealth playerHealth;

        protected UpdateTimer dashUpdateTimer;
        protected UpdateTimer dashCooldownTimer;

        protected int currentCharges;
        private Vector2 dashDirection;

        public int MaxCharges => maxCharges;
        public int CurrentCharges => currentCharges;
        // Direction captured once when the dash starts: the current move input if any,
        // otherwise wherever the player is currently facing (mouse today, potentially a
        // gamepad look direction later) - so dashing without a move input doesn't dash in place.
        public Vector2 DashDirection => dashDirection;

        private Action<InputAction.CallbackContext> dashInputCallback;

        protected virtual void Awake()
        {
            dashTrailRenderer = GetComponent<TrailRenderer>();
            characterCollider = GetComponent<Collider2D>();
            characterVisual = GetComponentInChildren<CharacterVisual>();
            dashUpdateTimer = new UpdateTimer(dashDuration);
            dashUpdateTimer.OnEventStarted += StartDashing;
            dashUpdateTimer.OnEventComplete += EndDashing;

            dashCooldownTimer = new UpdateTimer(dashCooldownBase);
            dashCooldownTimer.OnEventComplete += OnChargeRecharged;

            currentCharges = maxCharges;
        }

        protected virtual void Start()
        {
            playerProgression = PlayerManager.Instance.GetPlayerComponent<PlayerProgression>();
            inputHandler = PlayerManager.Instance.GetPlayerComponent<PlayerInputHandler>();
            dashInputCallback = ctx => StartDashEvent();
            inputHandler.Player.Jump.performed += dashInputCallback;

            playerProgression.onDerivedStatsChanged += OnDashCooldownUpdated;
            playerController = PlayerManager.Instance.GetPlayerComponent<PlayerController>();

            playerHealth = PlayerManager.Instance.GetPlayerComponent<PlayerHealth>();
        }

        protected virtual void OnDisable()
        {
            if (playerProgression != null)
            {
                playerProgression.onDerivedStatsChanged -= OnDashCooldownUpdated;
            }

            if (inputHandler != null && dashInputCallback != null)
            {
                inputHandler.Player.Jump.performed -= dashInputCallback;
            }
        }

        protected virtual void Update()
        {
            dashUpdateTimer.UpdateEvent();
            dashCooldownTimer.UpdateEvent();
            ManageDashTimerEvent();

            // Check if dashing so we set direction in dash direction
            CheckDashDirection();
        }

        private void OnDashCooldownUpdated()
        {
            RecalculateCooldownDuration();
        }

        protected virtual void StartDashEvent()
        {
            if (playerHealth != null && playerHealth.IsDead())
            {
                return;
            }
            if (WaveSpawner.Instance != null && WaveSpawner.Instance.EndingWave == true)
            {
                return;
            }
            if (PauseManager.Instance != null && PauseManager.Instance.IsPaused)
            {
                return;
            }
            if (currentCharges <= 0)
            {
                return;
            }
            if (dashUpdateTimer.GetIsEventActive())
            {
                return;
            }

            Vector2 moveInput = inputHandler.Player.Move.ReadValue<Vector2>();
            dashDirection = moveInput.sqrMagnitude > 0.0001f ? moveInput.normalized : characterVisual.FacingDirection;

            currentCharges--;
            OnChargesUpdated?.Invoke(currentCharges, maxCharges);
            dashUpdateTimer.StartEvent();

            if (!dashCooldownTimer.GetIsEventActive())
            {
                dashCooldownTimer.StartEvent();
            }
        }

        private void OnChargeRecharged()
        {
            currentCharges = Mathf.Min(currentCharges + 1, maxCharges);
            OnChargesUpdated?.Invoke(currentCharges, maxCharges);

            if (currentCharges < maxCharges)
            {
                dashCooldownTimer.StartEvent();
            }
        }

        protected virtual void StartDashing()
        {
            characterVisual.PlayDashAnimation();
            playerController.ChangeDashMultiplier(dashSpeed);
            dashTrailRenderer.emitting = true;
            playerHealth.ToggleIsInvulnerable(true);
        }

        protected virtual void EndDashing()
        {
            playerController.ResetDashMultiplier();
            dashTrailRenderer.emitting = false;
            playerHealth.ToggleIsInvulnerable(false);
        }

        protected virtual void RecalculateCooldownDuration()
        {
            float dashCooldown = StatsCalculations.CalculateDashCooldown(playerProgression.GetStatTotal(StatType.DashCooldown), dashCooldownBase);
            dashCooldownTimer.SetEventDuration(dashCooldown);
        }

        private void ManageDashTimerEvent()
        {
            // Only surface cooldown progress once every charge is spent - a charge
            // recharging silently in the background while another is still available
            // shouldn't show as "on cooldown".
            if (currentCharges <= 0 && dashCooldownTimer.GetIsEventActive())
            {
                float timeLeft = dashCooldownTimer.GetEventTimer();
                float cooldownDuration = dashCooldownTimer.GetEventDuration();
                OnDashTimerUpdated?.Invoke(timeLeft, cooldownDuration);
            }
        }

        protected void CheckDashDirection()
        {
            if (dashUpdateTimer.GetIsEventActive())
            {
                characterVisual.SetFacingDirection(dashDirection);
            }
        }

        public bool isDashActive()
        {
            return dashUpdateTimer.GetIsEventActive();
        }

        public virtual void LoadState()
        {
            RecalculateCooldownDuration();
            currentCharges = maxCharges;
            OnChargesUpdated?.Invoke(currentCharges, maxCharges);
        }

        public virtual void SaveState()
        {
            // No need to save, it is saved on the player progression component
        }

        public virtual void ResetState()
        {
            dashCooldownTimer.SetEventDuration(dashCooldownBase);
            currentCharges = maxCharges;
            OnChargesUpdated?.Invoke(currentCharges, maxCharges);
        }
    }
}
