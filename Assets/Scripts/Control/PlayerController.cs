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
namespace Game.Control
{
    public class PlayerController : MonoBehaviour, IDependentStateLoader, IInitializeAfterStateReady, IMapComponentDisabler
    {
        [SerializeField] private float defaultMoveSpeed = 1f;
        [SerializeField] private float baseMoveSpeed = 5f;
        [SerializeField] private float baseMoveScale = 0.25f;

        private float moveSpeed = 1f;

        private Dash playerDash;
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
            playerDash = GetComponent<Dash>();
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
            if (attackButtonDown && CanAttack())
            {
                Attack();
            }
        }

        private void Attack()
        {           
            weaponManager.Attack();
        }

        private void StartSpecialAttack()
        {
            if (WaveSpawner.Instance != null && WaveSpawner.Instance.EndingWave == true)
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
            if (weaponManager == null) return false;
            var currentWeapon = weaponManager.GetCurrentWeapon();
            bool canAttack = !(currentWeapon.IsAttackTimerActive() || currentWeapon.IsSpecialAttackTimerActive());
            return canAttack;
        }

        private void Move()
        {
            // Prevent movement if the wave is ending
            if (WaveSpawner.Instance != null && WaveSpawner.Instance.EndingWave == true)
            {
                return;
            }
            if (knockback.IsKnockbacked) return; // Prevent input during knockback
            rb.MovePosition(rb.position + movement * (CalculateMoveSpeed() * Time.fixedDeltaTime));
        }

        private void AdjustPlayerFacingDirection()
        {
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

            // Default to mouse-based facing
            if (Camera.main == null) return;
            if (playerDash.isDashActive()) return;

            Vector3 mousePosition = Input.mousePosition;
            Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
            worldMousePosition.z = 0;
            Vector3 targetPosition = worldMousePosition - transform.position;           
            characterVisual?.SetFacingDirection(targetPosition.normalized);
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
    }
}
