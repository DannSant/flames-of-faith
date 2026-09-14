using Game.Effects;
using UnityEngine;

namespace Game.Misc
{
    public class DestroyAfterTime : MonoBehaviour, IStackScalable
    {
        [SerializeField] private float timeToDestroy = 1f;
        [SerializeField] private bool autoStartTimer = true;

        public void ApplyStackScaling(StackScaling scaling)
        {
            timeToDestroy *= scaling.Get(StackScalingTarget.Lifetime);
        }

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
