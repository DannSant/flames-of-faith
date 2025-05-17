using Game.Common;
using Game.Misc;
using Game.Progression;
using UnityEngine;
using UnityEngine.InputSystem;
using Game.Scene;
namespace Game.Control
{
    public class PlayerController : Singleton<PlayerController>
    {
        [SerializeField] private float baseMoveSpeed = 5f;
        [SerializeField] private float baseMoveScale = 0.25f;

        private float moveSpeed = 1f;

        //private InputSystem_Actions inputActions;
        private PlayerInputHandler inputHandler;
        private Vector2 movement;
        private Rigidbody2D rb;
        private Animator animator;
        private Knockback knockback;

        private Vector2 defaultPosition;

        public bool FacingLeft { get { return facingLeft; } set { facingLeft = value; } }
        public float DashMultiplier { get; private set; }

        private bool facingLeft = false;

        protected override void Awake()
        {
            base.Awake(); 
                          
            //inputActions = new InputSystem_Actions();
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            knockback = GetComponent<Knockback>();
            DashMultiplier = 1;
            defaultPosition = transform.position;
        }

        private void Start()
        {
            inputHandler = PlayerInputHandler.Instance;
            PlayerProgression.Instance.onStatUpdated += PlayerController_onStatUpdatedEvent;
            SetMoveSpeed();
            if (MainSceneController.Instance != null)
            {
                MainSceneController.Instance.OnGameplayResetRequested += ResetPlayerPosition;
            }
        }

        private void OnDisable()
        {
            PlayerProgression.Instance.onStatUpdated -= PlayerController_onStatUpdatedEvent;
            if (MainSceneController.Instance != null)
            {
                MainSceneController.Instance.OnGameplayResetRequested -= ResetPlayerPosition;
            }
        }

        private void Update()
        {
            PlayerInput();
            AdjustPlayerFacingDirection();          
        }

        private void FixedUpdate()
        {
            Move();
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
            moveSpeed = StatsCalculations.CalculateMoveSpeed(PlayerProgression.Instance.GetStatTotal(StatType.MoveSpeed), baseMoveSpeed, baseMoveScale);
        }

        private void PlayerInput()
        {
            if (knockback.IsKnockbacked) {
                movement = Vector2.zero; // Prevent input during knockback
                return;
            }
            movement = inputHandler.Player.Move.ReadValue<Vector2>();

            if (movement.magnitude > 0f)
            {
                animator.SetBool("Move", true);
            }else
            {
                animator.SetBool("Move", false);
            }

           
        }

        private void Move()
        {
            if (knockback.IsKnockbacked) return; // Prevent input during knockback
            rb.MovePosition(rb.position + movement * (CalculateMoveSpeed() * Time.fixedDeltaTime));
        }

        private void AdjustPlayerFacingDirection()
        {
            if (Camera.main == null)
            {
                return;
            }
            Vector3 mousePosition = Input.mousePosition;          
            Vector3 facingDirection = Camera.main.ScreenToWorldPoint(mousePosition);
            facingDirection.z = 0; // Ensure z is 0 for 2D

            Vector3 direction = (facingDirection - transform.position).normalized;
            animator.SetFloat("DirectionX", direction.x);
            animator.SetFloat("DirectionY", direction.y);
            
            if (facingDirection.x < direction.x)
            {
                FacingLeft = true;
            }else
            {
                FacingLeft = false;
            }
            
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




    }
}
