using Game.Combat;
using Game.Effects;
using UnityEngine;

namespace Game.Misc
{
    public class DestroyAfterTime : MonoBehaviour, IStackScalable
    {
        [Tooltip("Seconds. A player damage source takes its lifetime from its DamageSourceData instead.")]
        [SerializeField] private float timeToDestroy = 1f;
        [SerializeField] private bool autoStartTimer = true;

        // Before stack scaling, which is pushed in right after Instantiate.
        private void Awake()
        {
            if (TryGetComponent(out DamageSourceBase source) && source.Data != null && source.Data.Lifetime > 0f)
            {
                timeToDestroy = source.Data.Lifetime;
            }
        }

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
