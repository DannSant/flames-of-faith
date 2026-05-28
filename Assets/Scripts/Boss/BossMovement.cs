using Game.Control;
using Game.Scene;
using UnityEngine;

namespace Game.Boss
{
    public enum FacingDirection
    {
        N, NE, E, SE, S, SW, W, NW
    }

    public class BossMovement : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float moveRange = 4f;
        [SerializeField] private Transform pivotPoint;

        private Transform player;
        private Animator animator;

        private Vector2? targetPosition;
        private bool isCastingAbility;       

        // Properties
        public Vector2 Direction { get; private set; }
        public bool HasTargetPosition => targetPosition.HasValue;
        public bool HasReachedDestination => !targetPosition.HasValue;
        public bool IsMoving => targetPosition.HasValue;

        private void Awake()
        {
            animator = GetComponentInChildren<Animator>();
        }

        private void Start()
        {
            if (pivotPoint == null)
            {
                pivotPoint = transform; // Fallback to self if no pivot assigned
            }
            player = PlayerManager.Instance.GetPlayerComponent<PlayerController>().transform;
        }

        private void Update()
        {
            if (player == null)
                return;

            if (animator == null)
            {
                Debug.LogWarning("Animator not found on BossMovement.");
                return;
            }


            if (targetPosition.HasValue && !isCastingAbility)
            {
                MoveTowardsTarget();
            }
            else
            {
                FacePlayer();
            }
        }

        private void MoveTowardsTarget()
        {
            Vector2 current =  transform.position;

            Vector2 target = targetPosition.Value;

            Vector2 direction = (target - current).normalized;

            transform.position = Vector2.MoveTowards(current,target, moveSpeed * Time.deltaTime);

            UpdateFacing(direction);

            float distance =Vector2.Distance(current, target);
            animator.SetBool("Move", true);

            if (distance <= 0.05f)
            {
                transform.position = target;
                targetPosition = null;
                animator.SetBool("Move", false);
            }
        }

        private void FacePlayer()
        {
            //Face the player if the boss is not moving
            Vector2 directionToPlayer = (player.position - transform.position).normalized;

            UpdateFacing(directionToPlayer);
        }

        private void UpdateFacing(Vector2 direction)
        {
            animator.SetFloat("DirectionX", direction.x);
            animator.SetFloat("DirectionY", direction.y);


            Direction = direction;
        }

        public void MoveTo(Vector2 position)
        {
            targetPosition = position;
        }

        public void SetCasting(bool value)
        {
            isCastingAbility = value;
        }

        public void StopMovement()
        {
            targetPosition = null;
        }

        public bool IsCasting()
        {
            return isCastingAbility;
        }



        public void TeleportToRandomPoint()
        {          
            Vector2 center = pivotPoint.position;

            // Get a random point within a radius of 1, then scale it
            Vector2 randomPoint = center + (Random.insideUnitCircle * moveRange);
            transform.position = randomPoint;
        }

        public FacingDirection GetFacingDirection(Vector2 dir)
        {
            dir.Normalize();

            float diagonalThreshold = 0.35f;

            bool right = dir.x > diagonalThreshold;
            bool left = dir.x < -diagonalThreshold;
            bool up = dir.y > diagonalThreshold;
            bool down = dir.y < -diagonalThreshold;

            // Diagonals first
            if (up && right) return FacingDirection.NE;
            if (up && left) return FacingDirection.NW;
            if (down && right) return FacingDirection.SE;
            if (down && left) return FacingDirection.SW;

            // Cardinals
            if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            {
                return dir.x > 0
                    ? FacingDirection.E
                    : FacingDirection.W;
            }

            return dir.y > 0
                ? FacingDirection.N
                : FacingDirection.S;
        }

        public void OnDrawGizmos()
        {
            
            Gizmos.DrawWireSphere(transform.position, moveRange);
        }
    }

}