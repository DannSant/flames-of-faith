using UnityEngine;

namespace Game.Misc
{
    public class OrbitingObject : MonoBehaviour, IOrbitInitializer
    {
        [SerializeField] private float orbitRadius = 1.5f;
        [SerializeField] private float orbitSpeed = 90f; // degrees per second
        private Transform orbitTarget;
        private float currentAngle = 0f;
       
        public void InitializeOrbit(Transform target, float angleOffset = 0f)
        {
            orbitTarget = target;
            currentAngle = 0f;
            this.currentAngle = angleOffset;

            if (orbitTarget != null)
            {
               
                Vector2 offset = new Vector2(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle)) * orbitRadius;
                transform.position = orbitTarget.position + (Vector3)offset;
            }
        }
        private void Update()
        {
            if (orbitTarget == null) return;

            currentAngle += orbitSpeed * Time.deltaTime;
            float angleRad = currentAngle * Mathf.Deg2Rad;
            Vector2 offset = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * orbitRadius;
            transform.position = orbitTarget.position + (Vector3)offset;
        }
    }

}