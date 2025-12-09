using UnityEngine;

namespace Game.Misc
{
    public class DestroyAfterTime : MonoBehaviour
    {
        [SerializeField] private float timeToDestroy = 1f;
        [SerializeField] private bool autoStartTimer = true;

        private void Start()
        {
            // Destroy the GameObject after the specified time
            if (autoStartTimer)
            {
                StartDestroyTimer(timeToDestroy);
            }
            
        }

        public void StartDestroyTimer(float time)
        {
            Destroy(gameObject, time);
        }
    }
}
