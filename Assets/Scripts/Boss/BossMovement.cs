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

        //public FacingDirection CurrentFacing { get; private set; }

        public Vector2 Direction { get; private set; }

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
            if (animator == null) { 
                Debug.LogWarning("Animator not found on BossMovement."); 
                return;
            }
            //Face the player if the boss is not moving
            Vector2 directionToPlayer = (player.position - transform.position).normalized;

           
            animator.SetFloat("DirectionX", directionToPlayer.x);
            animator.SetFloat("DirectionY", directionToPlayer.y);

          

            //CurrentFacing = GetFacingDirection(directionToPlayer);
            Direction = directionToPlayer;
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