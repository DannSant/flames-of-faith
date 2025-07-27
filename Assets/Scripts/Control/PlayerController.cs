using Game.Common;
using Game.Misc;
using Game.Progression;
using UnityEngine;
using UnityEngine.InputSystem;
using Game.Scene;
using Game.Combat;
namespace Game.Control
{
    public class PlayerController : MonoBehaviour, IDependentStateLoader
    {
        [SerializeField] private float defaultMoveSpeed = 1f;
        [SerializeField] private float baseMoveSpeed = 5f;
        [SerializeField] private float baseMoveScale = 0.25f;

        private float moveSpeed = 1f;

        private CharacterVisual characterVisual;
        private PlayerProgression playerProgression;
        private PlayerInputHandler inputHandler;
        private WeaponManager weaponManager;
        private Vector2 movement;
        private Rigidbody2D rb;       
        private Knockback knockback;        

        private Vector2 defaultPosition;
        private bool attackButtonDown = false;

        public bool FacingLeft { get { return facingLeft; } set { facingLeft = value; } }
        public float DashMultiplier { get; private set; }

        private bool facingLeft = false;

        private void Awake()
        {                                
           
            rb = GetComponent<Rigidbody2D>();           
            knockback = GetComponent<Knockback>();
            characterVisual = GetComponentInChildren<CharacterVisual>();
            DashMultiplier = 1;
            defaultPosition = transform.position;

            if (characterVisual == null)
            {
                Debug.LogError("CharacterVisual component not found on PlayerController.");
            }
        }

        private void Start()
        {           
            weaponManager = PlayerManager.Instance.GetPlayerComponent<WeaponManager>();
            playerProgression = PlayerManager.Instance.GetPlayerComponent<PlayerProgression>();            
            playerProgression.onStatUpdated += PlayerController_onStatUpdatedEvent;
            SetMoveSpeed();
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
            playerProgression.onStatUpdated -= PlayerController_onStatUpdatedEvent;
            if (MainSceneController.Instance != null)
            {
                MainSceneController.Instance.OnGameplayInitialSetup -= ResetPlayerPosition;
            }


        }

        private void Update()
        {
            MovementInput();
            AttackInput();
            AdjustPlayerFacingDirection();          
        }

        private void FixedUpdate()
        {
            Move();
        }

        private void StartAttacking()
        {
            attackButtonDown = true;
        }

        private void StopAttacking()
        {
            attackButtonDown = false;
        }

        private void PlayerController_onStatUpdatedEvent(StatType statType, int value)
        {
            if (statType == StatType.MoveSpeed)
            {
                SetMoveSpeed();
            }
        }

        private void ResetPlayerPosition() { 
            transform.position = defaultPosition; 
        }

        private void SetMoveSpeed() 
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
            if (attackButtonDown)
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
            weaponManager.SpecialAttack();
        }

        private void Move()
        {
            if (knockback.IsKnockbacked) return; // Prevent input during knockback
            rb.MovePosition(rb.position + movement * (CalculateMoveSpeed() * Time.fixedDeltaTime));
        }

        private void AdjustPlayerFacingDirection()
        {
            if (weaponManager != null && weaponManager.IsAutoAttackEnabled)
            {
                EnemyHealth target = weaponManager.GetCurrentTarget();
                if (target != null)
                {
                    Vector3 targetPosition = target.transform.position;
                    FacingLeft = (targetPosition.x < transform.position.x);
                    characterVisual?.SetFacingDirection(FacingLeft);
                    return;
                }
            }

            // Default to mouse-based facing
            if (Camera.main == null) return;

            Vector3 mousePosition = Input.mousePosition;
            Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
            worldMousePosition.z = 0;

            FacingLeft = (worldMousePosition.x < transform.position.x);
            characterVisual?.SetFacingDirection(FacingLeft);
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
            SetMoveSpeed();
        }

        public void SaveState()
        {
            // No need to save, it is saved on the player progression component
        }

        public void ResetState()
        {
            moveSpeed = defaultMoveSpeed;
        }
    }
}
