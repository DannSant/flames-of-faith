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

        public FacingDirection CurrentFacing { get; private set; }

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

            CurrentFacing = GetFacingDirection(directionToPlayer);
        }



        public void TeleportToRandomPoint()
        {          
            Vector2 center = pivotPoint.position;

            // Get a random point within a radius of 1, then scale it
            Vector2 randomPoint = center + (Random.insideUnitCircle * moveRange);
            transform.position = randomPoint;
        }

        private FacingDirection GetFacingDirection(Vector2 dir)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            // Convert angle to 0-360
            if (angle < 0)
                angle += 360f;

            // 8-direction sectors (45 degrees each)

            if (angle >= 337.5f || angle < 22.5f)
                return FacingDirection.E;

            if (angle >= 22.5f && angle < 67.5f)
                return FacingDirection.NE;

            if (angle >= 67.5f && angle < 112.5f)
                return FacingDirection.N;

            if (angle >= 112.5f && angle < 157.5f)
                return FacingDirection.NW;

            if (angle >= 157.5f && angle < 202.5f)
                return FacingDirection.W;

            if (angle >= 202.5f && angle < 247.5f)
                return FacingDirection.SW;

            if (angle >= 247.5f && angle < 292.5f)
                return FacingDirection.S;

            return FacingDirection.SE;
        }

        public void OnDrawGizmos()
        {
            
            Gizmos.DrawWireSphere(transform.position, moveRange);
        }
    }

}