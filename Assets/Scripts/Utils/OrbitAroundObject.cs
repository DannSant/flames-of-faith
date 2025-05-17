using UnityEngine;

namespace Game.Utils
{
    public class OrbitAroundObject : MonoBehaviour
    {
        [SerializeField] private Transform pivotObject;
        [SerializeField] private float maxAllowedDistance = 5f;
        private void Update()
        {
            Move();
        }

        private void Move()
        {
            if (pivotObject == null)
            {
                Debug.LogWarning("Pivot Object is not assigned!");
                return;
            }

            // Get mouse position in world space
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = Vector3.Distance(Camera.main.transform.position, pivotObject.position);
            Vector3 targetPosition = Camera.main.ScreenToWorldPoint(mousePosition);

            // Calculate direction from pivot to target
            Vector3 direction = targetPosition - pivotObject.position;
            float distance = direction.magnitude;

            // Clamp distance to max allowed distance
            if (distance > maxAllowedDistance)
            {
                direction = direction.normalized * maxAllowedDistance;
            }

            // Set new position
            transform.position = pivotObject.position + direction;
        }
    }

}