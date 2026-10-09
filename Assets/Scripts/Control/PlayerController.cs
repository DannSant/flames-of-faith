using Game.Common;
using Game.Misc;
using Game.Progression;
using UnityEngine;
using UnityEngine.InputSystem;
using Game.Scene;
using Game.Combat;
using Game.Saving;
using Game.Utils;
using Game.Waves;
using Game.GameSettings;
namespace Game.Control
{
    public class PlayerController : MonoBehaviour, IDependentStateLoader, IInitializeAfterStateReady, IMapComponentDisabler
    {
        [SerializeField] private float defaultMoveSpeed = 1f;
        [SerializeField] private float baseMoveSpeed = 5f;
        [SerializeField] private float baseMoveScale = 0.25f;
        [Tooltip("Right-stick magnitude needed before it overrides the facing direction.")]
        [SerializeField] private float aimDeadzone = 0.25f;
        [Tooltip("Debug: while attack is held, logs whatever is stopping the primary attack each time it changes.")]
        [SerializeField] private bool logAttackBlocks = false;

        private float moveSpeed = 1f;
        private string lastAttackBlockReason;

        private DashBase playerDash;
        private CharacterVisual characterVisual;
        private PlayerProgression playerProgression;
        private PlayerInputHandler inputHandler;
        private WeaponManager weaponManager;
        private Vector2 movement;
        private Rigidbody2D rb;       
        private Knockback knockback;  
        private PlayerHealth playerHealth;

        private Vector2 defaultPosition;
        private bool attackButtonDown = false;
        private InputAction toggleAutoAttackAction;

        public bool FacingLeft { get { return facingLeft; } set { facingLeft = value; } }
        public float DashMultiplier { get; private set; }
        public Vector2 DefaultPosition { get { return defaultPosition; } set { defaultPosition = value; } }

        private bool facingLeft = false;
        private bool disabledInput = false;

        private void Awake()
        {                                
           
            rb = GetComponent<Rigidbody2D>();           
            knockback = GetComponent<Knockback>();
            playerHealth = GetComponent<PlayerHealth>();
            characterVisual = GetComponentInChildren<CharacterVisual>();
            playerDash = GetComponent<DashBase>();
            DashMultiplier = 1;
            disabledInput = false;

            if (characterVisual == null)
            {
                Debug.LogError("CharacterVisual component not found on PlayerController.");
            }
        }

        private void Start()
        {           
            weaponManager = PlayerManager.Instance.GetPlayerComponent<WeaponManager>();
            playerProgression = PlayerManager.Instance.GetPlayerComponent<PlayerProgression>();            
            playerProgression.onDerivedStatsChanged += PlayerController_onStatUpdatedEvent;
           
            if (MainSceneController.Instance != null)
            {
                MainSceneController.Instance.OnGameplayInitialSetup += ResetPlayerPosition;
            }

            inputHandler = PlayerManager.Instance.GetPlayerComponent<PlayerInputHandler>();
            inputHandler.Player.Attack.performed += ctx => StartAttacking();
            inputHandler.Player.Attack.canceled += ctx => StopAttacking();
            inputHandler.Player.Special.performed += ctx => StartSpecialAttack();

            // Looked up by name so this doesn't depend on the generated wrapper having been
            // regenerated with the action yet.
            toggleAutoAttackAction = inputHandler.Player.Get().FindAction("ToggleAutoAttack", throwIfNotFound: false);
            if (toggleAutoAttackAction != null)
            {
                toggleAutoAttackAction.performed += HandleToggleAutoAttack;
            }
            else
            {
                Debug.LogWarning("PlayerController: no Player/ToggleAutoAttack input action, the auto-attack hotkey is disabled.");
            }
        }

        private void OnDisable()
        {
            if (toggleAutoAttackAction != null)
            {
                toggleAutoAttackAction.performed -= HandleToggleAutoAttack;
            }
            playerProgression.onDerivedStatsChanged -= PlayerController_onStatUpdatedEvent;
            if (MainSceneController.Instance != null)
            {
                MainSceneController.Instance.OnGameplayInitialSetup -= ResetPlayerPosition;
            }


        }

        private void Update()
        {
            if (playerHealth.IsDead()) return;
            if (disabledInput) {return;}
            // Update() still runs while Time.timeScale is 0, so pause must be checked
            // explicitly - otherwise mouse-look keeps working while everything else freezes.
            if (PauseManager.Instance != null && PauseManager.Instance.IsPaused) return;

            MovementInput();
            AttackInput();
            AdjustPlayerFacingDirection();          
        }

        private void FixedUpdate()
        {
            Move();
        }

        public void InitializeAfterStateReady()
        {
            RecalculateMoveSpeed();
        }

        private void StartAttacking()
        {
            attackButtonDown = true;
        }

        // Also fires when the Player map is disabled while attack is held (menus, GameplayFreeze...).
        // The Attack action has initialStateCheck on, so a button still held when the map comes
        // back is picked up again without a re-press.
        private void StopAttacking()
        {
            if (logAttackBlocks && attackButtonDown)
            {
                Debug.Log($"[PlayerController] Primary attack released (map enabled: {inputHandler.Player.enabled})", this);
            }
            attackButtonDown = false;
        }

        // Ctrl / RT. Goes through SettingsManager so the choice persists and the settings menu,
        // WeaponManager and the HUD indicator all hear about it from the one place.
        private void HandleToggleAutoAttack(InputAction.CallbackContext ctx)
        {
            if (disabledInput) return;
            if (playerHealth != null && playerHealth.IsDead()) return;
            if (PauseManager.Instance != null && PauseManager.Instance.IsPaused) return;
            if (SettingsManager.Instance == null) return;

            SettingsManager.Instance.SetAutoAttackEnabled(!SettingsManager.Instance.AutoAttackEnabled);
        }

        private void PlayerController_onStatUpdatedEvent()
        {
            RecalculateMoveSpeed();
        }

        private void ResetPlayerPosition() { 
            transform.position = defaultPosition; 
        }

        private void RecalculateMoveSpeed() 
        { 
            moveSpeed = StatsCalculations.CalculateMoveSpeed(playerProgression.GetStatTotal(StatType.MoveSpeed), baseMoveSpeed, baseMoveScale);           
        }

        private void MovementInput()
        {
            if (knockback.IsKnockbacked) {
                movement = Vector2.zero; // Prevent input during knockback
                return;
            }
            // The player is held in place while the wave ends, so drop the input too -
            // otherwise the walk animation plays while standing still.
            movement = IsWaveEnding() ? Vector2.zero : inputHandler.Player.Move.ReadValue<Vector2>();

            bool shouldMove = movement.magnitude > 0f;
            characterVisual?.PlayMoveAnimation(shouldMove);
        }

        private void AttackInput()
        {
            if (IsWaveEnding())
            {
                LogAttackBlock(attackButtonDown ? "wave ending" : null);
                return;
            }
            if (IsAttacksPreventedByLevel())
            {
                LogAttackBlock(attackButtonDown ? "level prevents attacks" : null);
                return;
            }
            if (!attackButtonDown)
            {
                lastAttackBlockReason = null;
                return;
            }

            string blockReason = GetAttackBlockReason();
            if (blockReason == null)
            {
                Attack();
            }
            LogAttackBlock(blockReason);
        }

        private void LogAttackBlock(string reason)
        {
            if (!logAttackBlocks || reason == lastAttackBlockReason) return;
            lastAttackBlockReason = reason;
            // The attack timer is the normal gap between shots, so it's not worth a line.
            if (reason != null && reason != AttackTimerReason)
            {
                Debug.Log($"[PlayerController] Primary attack held but blocked: {reason}", this);
            }
        }

        private bool IsWaveEnding()
        {
            return WaveSpawner.Instance != null && WaveSpawner.Instance.EndingWave;
        }

        // Some level types (shop, campfire, event, treasure...) have no combat and don't
        // pause the game, so clicking their UI can otherwise bleed through as an attack input.
        private bool IsAttacksPreventedByLevel()
        {
            return GameSession.Instance != null
                && GameSession.Instance.currentLevel != null
                && GameSession.Instance.currentLevel.preventAttacks;
        }

        private void Attack()
        {           
            weaponManager.Attack();
        }

        private void StartSpecialAttack()
        {
            if (playerHealth != null && playerHealth.IsDead())
            {
                return;
            }
            if (IsWaveEnding())
            {
                return;
            }
            if (PauseManager.Instance != null && PauseManager.Instance.IsPaused)
            {
                return;
            }
            if (IsAttacksPreventedByLevel())
            {
                return;
            }
            /*if (characterVisual.IsAttackAnimationPlaying)
            {
                return; // Prevent special attack if normal attack animation is playing
            }*/
            weaponManager.SpecialAttack();
        }

        private const string AttackTimerReason = "attack cooldown";

        // Null when the primary attack may fire, otherwise why not (for logAttackBlocks).
        private string GetAttackBlockReason()
        {
            if (playerHealth != null && playerHealth.IsDead()) return "player dead";
            if (weaponManager == null) return "no WeaponManager";
            var currentWeapon = weaponManager.GetCurrentWeapon();
            if (currentWeapon == null) return "no weapon equipped";
            if (currentWeapon.IsAttackTimerActive()) return AttackTimerReason;
            // Only the special's animation blocks, not its cooldown (that used to lock the primary
            // out for the whole special cooldown). The animation still has to: an Attack trigger
            // would cut it before its end event, which spawns the special's projectiles and ends
            // its invulnerability. Same rule as WeaponManager.ManageAutoAttack.
            if (characterVisual != null && characterVisual.IsSpecialAttackAnimationPlaying) return "special attack animation playing";
            return null;
        }

        private void Move()
        {
            if (playerHealth != null && playerHealth.IsDead())
            {
                rb.linearVelocity = Vector2.zero; // Stop movement immediately
                return;
            }
            // Prevent movement if the wave is ending
            if (IsWaveEnding())
            {
                rb.linearVelocity = Vector2.zero; // Stop movement immediately
                return;
            }
            if (knockback.IsKnockbacked) return; // Prevent input during knockback

            // While dashing, move along the direction captured when the dash started
            // rather than the live move input - otherwise dashing without holding a
            // movement key would dash in place.
            Vector2 moveDirection = playerDash.isDashActive() ? playerDash.DashDirection : movement;
            rb.MovePosition(rb.position + moveDirection * (CalculateMoveSpeed() * Time.fixedDeltaTime));
        }

        private void AdjustPlayerFacingDirection()
        {
            if (IsWaveEnding())
            {
                return;
            }
            if (playerDash.isDashActive()) return;
            // With auto-attack off, only face the target while an attack on it is playing (an
            // aim-assisted manual shot); otherwise the player's own aim drives facing.
            if (weaponManager != null && (weaponManager.IsAutoAttackEnabled || IsAttackInProgress()))
            {
                EnemyHealth target = weaponManager.GetCurrentTarget();
                if (target != null)
                {
                    Vector3 attackLookAtPosition = target.transform.position - transform.position;
                   
                    characterVisual?.SetFacingDirection(attackLookAtPosition.normalized);
                    return;
                }
            }

            if (IsGamepadActive())
            {
                AdjustFacingFromGamepad();
                return;
            }

            // Default to mouse-based facing
            if (Camera.main == null) return;

            Vector3 mousePosition = Input.mousePosition;
            Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
            worldMousePosition.z = 0;
            Vector3 targetPosition = worldMousePosition - transform.position;           
            characterVisual?.SetFacingDirection(targetPosition.normalized);
        }

        // Twin-stick facing: right stick aims; with it idle, face the movement direction;
        // with both idle, keep the last facing so a standing dash goes where the indicator points.
        private void AdjustFacingFromGamepad()
        {
            Vector2 stick = ReadGamepadLook();
            if (stick.magnitude > aimDeadzone)
            {
                characterVisual?.SetFacingDirection(stick.normalized);
            }
            else if (movement.sqrMagnitude > 0.0001f)
            {
                characterVisual?.SetFacingDirection(movement.normalized);
            }
        }

        // Look is bound to both <Pointer>/position and <Gamepad>/rightStick. Reading the action
        // directly lets the pointer's pixel position (huge magnitude) win the conflict
        // resolution, so only read the gamepad controls bound to it.
        private Vector2 ReadGamepadLook()
        {
            Vector2 best = Vector2.zero;
            foreach (var control in inputHandler.Player.Look.controls)
            {
                if (control.device is Gamepad && control is InputControl<Vector2> vectorControl)
                {
                    Vector2 value = vectorControl.ReadValue();
                    if (value.sqrMagnitude > best.sqrMagnitude) best = value;
                }
            }
            return best;
        }

        private bool IsAttackInProgress()
        {
            var weapon = weaponManager != null ? weaponManager.GetCurrentWeapon() : null;
            return weapon != null && weapon.IsAttackTimerActive();
        }

        private bool IsGamepadActive()
        {
            return InputDeviceManager.Instance != null && InputDeviceManager.Instance.IsGamepadActive;
        }

        private float CalculateMoveSpeed()
        {
            float speed = moveSpeed;

            speed *= DashMultiplier;

            return speed;
        }

        public Vector2 GetMouseWorldPosition()
        {
            Vector3 mousePosition = inputHandler.Player.Look.ReadValue<Vector2>();         
            mousePosition.z = 0;
            if (Camera.main == null) 
            {
                return Vector2.zero;
            }
            return Camera.main.ScreenToWorldPoint(mousePosition);
        }

        // World-space aim direction for the active input scheme. Prefer this over
        // GetMouseWorldPosition: the Look action is also bound to the right stick, whose
        // value is not a screen position.
        public Vector2 GetAimDirection()
        {
            if (IsGamepadActive())
            {
                // Read the live input before falling back to facing: while a manual attack is
                // locked onto a target, facing is held on it, and aiming from facing would keep
                // re-locking the same enemy however the stick is pushed.
                Vector2 stick = ReadGamepadLook();
                if (stick.magnitude > aimDeadzone) return stick.normalized;
                if (movement.sqrMagnitude > 0.0001f) return movement.normalized;
                return characterVisual != null ? characterVisual.FacingDirection : Vector2.right;
            }
            return (GetMouseWorldPosition() - (Vector2)transform.position).normalized;
        }

        public void ChangeDashMultiplier(float multiplier)
        {
            DashMultiplier = multiplier;
        }

        public void ResetDashMultiplier()
        {
            DashMultiplier = 1;
        }

        public void LoadState()
        {
            RecalculateMoveSpeed();
        }

        public void SaveState()
        {
            // No need to save, it is saved on the player progression component
        }

        public void ResetState()
        {
            moveSpeed = defaultMoveSpeed;
        }

        public void DisableComponentsOnMap()
        {
            disabledInput = true;
        }

        public void EnableInput()
        {
            disabledInput = false;
        }
    }
}
