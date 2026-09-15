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

        private float moveSpeed = 1f;

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
        }

        private void OnDisable()
        {
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

        private void StopAttacking()
        {
            attackButtonDown = false;
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
            movement = inputHandler.Player.Move.ReadValue<Vector2>();

            bool shouldMove = movement.magnitude > 0f;
            characterVisual?.PlayMoveAnimation(shouldMove);
        }

        private void AttackInput()
        {
            if (WaveSpawner.Instance != null && WaveSpawner.Instance.EndingWave == true)
            {
                return;
            }
            if (IsAttacksPreventedByLevel())
            {
                return;
            }
            if (attackButtonDown && CanAttack())
            {
                Attack();
            }
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
            if (WaveSpawner.Instance != null && WaveSpawner.Instance.EndingWave == true)
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

        private bool CanAttack() 
        {
            if (playerHealth != null && playerHealth.IsDead())
            {
                return false; // Prevent attack if the player is dead
            }
            if (weaponManager == null) return false;
            var currentWeapon = weaponManager.GetCurrentWeapon();
            bool canAttack = !(currentWeapon.IsAttackTimerActive() || currentWeapon.IsSpecialAttackTimerActive());
            return canAttack;
        }

        private void Move()
        {
            if (playerHealth != null && playerHealth.IsDead())
            {
                rb.linearVelocity = Vector2.zero; // Stop movement immediately
                return;
            }
            // Prevent movement if the wave is ending
            if (WaveSpawner.Instance != null && WaveSpawner.Instance.EndingWave == true)
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
            if (WaveSpawner.Instance != null && WaveSpawner.Instance.EndingWave == true)
            {
                return;
            }
            if (playerDash.isDashActive()) return;
            if (weaponManager != null && weaponManager.IsAutoAttackEnabled)
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
