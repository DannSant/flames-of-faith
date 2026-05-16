using Game.Control;
using Game.Scene;
using UnityEngine;

namespace Game.Boss
{
    public class BossMovement : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float moveRange = 4f;
        [SerializeField] private Transform pivotPoint;

        private Transform player;
        private Animator animator;

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
            Vector2 directionToPlayer = player.position - pivotPoint.position;
            animator.SetFloat("DirectionX", directionToPlayer.x);
            animator.SetFloat("DirectionY", directionToPlayer.y);
        }



        public void MoveToRandomPoint()
        {          
            Vector2 center = pivotPoint.position;

            // Get a random point within a radius of 1, then scale it
            Vector2 randomPoint = center + (Random.insideUnitCircle * moveRange);
            pivotPoint.position = randomPoint;
        }

        public void OnDrawGizmos()
        {
            
            Gizmos.DrawWireSphere(transform.position, moveRange);
        }
    }

}