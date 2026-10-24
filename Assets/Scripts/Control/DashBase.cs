using Game.Combat;
using Game.Common;
using Game.Effects;
using Game.Effects.Abilities;
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
        // Loaded from the class's CharacterClassData in Awake (the balance sheet's Classes table)
        protected float dashCooldownBase = 2f;
        public float DashCooldownBase => dashCooldownBase;
        protected float dashSpeed = 3f;
        protected float dashDuration = .2f;
        protected int maxCharges = 1;

        [Header("Steering")]
        [Tooltip("Lets the move input change the dash direction while dashing. With no move input the dash " +
            "keeps its last direction, so dashing from a standstill still goes where the player faces.")]
        [SerializeField] protected bool allowSteering = false;
        [Tooltip("Max degrees per second the dash can turn while steering. 0 = instant (snaps to the move input).")]
        [SerializeField] protected float steeringTurnRate = 0f;

        public Action<float, float> OnDashTimerUpdated;
        private readonly CooldownProgressReporter dashCooldownReporter = new CooldownProgressReporter();
        public Action<int, int> OnChargesUpdated;
        public event Action onDashStarted;
        public event Action onDashEnded;

        protected PlayerInputHandler inputHandler;
        protected PlayerController playerController;
        protected PlayerProgression playerProgression;
        protected TrailRenderer dashTrailRenderer;
        protected CharacterVisual characterVisual;
        protected Collider2D characterCollider;
        protected PlayerHealth playerHealth;
        private EffectStore effectStore;

        protected UpdateTimer dashUpdateTimer;
        protected UpdateTimer dashCooldownTimer;

        protected int currentCharges;
        private Vector2 dashDirection;

        public int MaxCharges => maxCharges;
        public int CurrentCharges => currentCharges;
        // Direction captured when the dash starts: the current move input if any, otherwise
        // wherever the player is currently facing - so dashing without a move input doesn't
        // dash in place. With allowSteering, the move input keeps updating it mid-dash.
        public Vector2 DashDirection => dashDirection;

        private Action<InputAction.CallbackContext> dashInputCallback;

        protected virtual void Awake()
        {
            LoadClassValues();
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

        private void LoadClassValues()
        {
            var classManager = GetComponentInParent<PlayerClassManager>();
            var classData = classManager != null ? classManager.ClassData : null;
            if (classData == null)
            {
                Debug.LogWarning($"{GetType().Name}: no class data found, using the default dash values.", this);
                return;
            }
            dashCooldownBase = classData.dashCooldown;
            dashSpeed = classData.dashSpeed;
            dashDuration = classData.dashDuration;
            maxCharges = classData.dashMaxCharges;
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

        // Subscribed in OnEnable, not Start: the kit is reset or loaded right after the player is
        // spawned, before Start runs, and the charges must be right from the start of the level.
        protected virtual void OnEnable()
        {
            if (effectStore == null) effectStore = GetComponent<EffectStore>();
            if (effectStore != null)
            {
                effectStore.OnEffectsChanged -= ApplyAbilityItems;
                effectStore.OnEffectsChanged += ApplyAbilityItems;
            }
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

            if (effectStore != null)
            {
                effectStore.OnEffectsChanged -= ApplyAbilityItems;
            }
        }

        protected virtual void Update()
        {
            dashUpdateTimer.UpdateEvent();
            dashCooldownTimer.UpdateEvent();
            ManageDashTimerEvent();
            UpdateSteering();

            // Check if dashing so we set direction in dash direction
            CheckDashDirection();
        }

        // Only a non-zero move input steers: releasing the keys mid-dash keeps the current
        // direction instead of stopping in place (the bug the start-of-dash capture fixed).
        private void UpdateSteering()
        {
            if (!allowSteering || !dashUpdateTimer.GetIsEventActive()) return;

            Vector2 moveInput = inputHandler.Player.Move.ReadValue<Vector2>();
            if (moveInput.sqrMagnitude <= 0.0001f) return;

            Vector2 desired = moveInput.normalized;
            if (steeringTurnRate <= 0f)
            {
                dashDirection = desired;
                return;
            }

            dashDirection = ((Vector2)Vector3.RotateTowards(
                dashDirection, desired, steeringTurnRate * Mathf.Deg2Rad * Time.deltaTime, 0f)).normalized;
        }

        private void OnDashCooldownUpdated()
        {
            RecalculateCooldownDuration();
        }

        /// <summary>
        /// Reads this class's dash ability item (Glyph) before each dash, so an upgrade applies
        /// from the next dash. Runs before the dash timer starts, so a new duration counts for this
        /// dash. Without the item, a dash keeps its own values.
        /// </summary>
        protected virtual void ApplyAbilityItems() { }

        /// <summary>The tier values of the owned ability <typeparamref name="TAbility"/>, or null without the item.</summary>
        protected TValues GetAbilityValues<TAbility, TValues>()
            where TAbility : AbilityBehavior<TValues>
            where TValues : class, new()
        {
            if (effectStore == null && PlayerManager.Instance != null)
            {
                effectStore = PlayerManager.Instance.GetPlayerComponent<EffectStore>();
            }
            if (effectStore == null || !effectStore.TryGetAbility(out TAbility ability, out int tier)) return null;
            return ability.GetTier(tier);
        }

        protected virtual void StartDashEvent()
        {
            ApplyAbilityItems();

            if (playerHealth != null && playerHealth.IsDead())
            {
                return;
            }
            // On the overworld map, Submit (A) also triggers Jump: without this the dash fires
            // when entering a level, and the Augur's flame dash leaves flames on the map.
            if (playerController != null && playerController.IsInputDisabled)
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
            onDashStarted?.Invoke();
        }

        protected virtual void EndDashing()
        {
            playerController.ResetDashMultiplier();
            dashTrailRenderer.emitting = false;
            playerHealth.ToggleIsInvulnerable(false);
            onDashEnded?.Invoke();
        }

        protected virtual void RecalculateCooldownDuration()
        {
            float dashCooldown = PlayerStatSettings.Instance.GetDashCooldown(playerProgression.GetStatTotal(StatType.DashCooldown), dashCooldownBase);
            dashCooldownTimer.SetEventDuration(dashCooldown);
        }

        private void ManageDashTimerEvent()
        {
            // Only surface cooldown progress once every charge is spent - a charge
            // recharging silently in the background while another is still available
            // shouldn't show as "on cooldown". A final 0 is sent when it stops showing, so the sweep clears.
            dashCooldownReporter.Tick(currentCharges <= 0 && dashCooldownTimer.GetIsEventActive(),
                dashCooldownTimer.GetEventTimer(), dashCooldownTimer.GetEventDuration(), OnDashTimerUpdated);
        }

        protected void CheckDashDirection()
        {
            if (dashUpdateTimer.GetIsEventActive())
            {
                characterVisual.SetFacingDirection(dashDirection);
            }
        }

        /// <summary>Changes how long a dash lasts (e.g. from an ability item). Applies from the next dash.</summary>
        public void SetDashDuration(float duration)
        {
            dashDuration = Mathf.Max(0.01f, duration);
            dashUpdateTimer?.SetEventDuration(dashDuration);
        }

        /// <summary>
        /// Changes the number of dash charges (e.g. from an ability item). Raising the max grants
        /// the new charges at once; lowering it drops the charges above the new max.
        /// </summary>
        public void SetMaxCharges(int charges)
        {
            int newMax = Mathf.Max(1, charges);
            if (newMax == maxCharges) return;

            int gained = Mathf.Max(0, newMax - maxCharges);
            maxCharges = newMax;
            currentCharges = Mathf.Min(currentCharges + gained, maxCharges);

            if (currentCharges < maxCharges && dashCooldownTimer != null && !dashCooldownTimer.GetIsEventActive())
            {
                dashCooldownTimer.StartEvent();
            }
            OnChargesUpdated?.Invoke(currentCharges, maxCharges);
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
