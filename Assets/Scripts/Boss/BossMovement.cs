using UnityEngine;

namespace Game.Boss
{
    public class BossMovement : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float moveRange = 4f;
        [SerializeField] private Transform pivotPoint;

        private void Start()
        {
            if (pivotPoint == null)
            {
                pivotPoint = transform; // Fallback to self if no pivot assigned
            }
        }

        public void MoveToRandomPoint()
        {          
            Vector2 center = pivotPoint.position;

            // Get a random point within a radius of 1, then scale it
            Vector2 randomPoint = center + (Random.insideUnitCircle * moveRange);
            pivotPoint.position = randomPoint;
        }
    }

}