using UnityEngine;

namespace Game.Misc
{
    public class DriftingOrbit : MonoBehaviour
    {
        [SerializeField] private float orbitRadius = 0.5f;
        [SerializeField] private float orbitSpeed = 180f; // degrees per second
        [SerializeField] private float startAngleOffset = 0f;
        [SerializeField] private float radiusDriftPerCycle = 0.5f;

        private Vector3 centerPosition;
        private float currentAngle;
        private float currentRadius;

        private void Start()
        {
            centerPosition = transform.position;
            currentAngle = startAngleOffset;
            currentRadius = orbitRadius;
        }

        private void Update()
        {
            float previousAngle = currentAngle;
            currentAngle += orbitSpeed * Time.deltaTime;

            int cyclesCompleted = Mathf.FloorToInt(currentAngle / 360f) - Mathf.FloorToInt(previousAngle / 360f);
            if (cyclesCompleted > 0)
            {
                currentRadius += radiusDriftPerCycle * cyclesCompleted;
            }

            float angleRad = currentAngle * Mathf.Deg2Rad;
            Vector2 offset = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * currentRadius;
            transform.position = centerPosition + (Vector3)offset;
        }
    }
}
