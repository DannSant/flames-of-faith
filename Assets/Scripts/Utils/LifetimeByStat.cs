using Game.Combat;
using Game.Effects;
using Game.Progression;
using Game.Scene;
using UnityEngine;

namespace Game.Utils
{
    public class LifetimeByStat : MonoBehaviour, IStackScalable
    {
        [Header("Lifetime Settings")]
        [Tooltip("Used only without a DamageSourceData: a player damage source takes its lifetime from the asset.")]
        [SerializeField] private float baseDurationTime = 3f;
        [Tooltip("Used only without a DamageSourceData (Lifetime Per Skill Duration on the asset).")]
        [SerializeField] private float durationScalingFactor = 1f;
        [SerializeField] private StatType durationStat = StatType.SkillDuration;

        private float stackLifetimeMultiplier = 1f;

        private void Awake()
        {
            if (TryGetComponent(out DamageSourceBase source) && source.Data != null && source.Data.Lifetime > 0f)
            {
                baseDurationTime = source.Data.Lifetime;
                durationScalingFactor = source.Data.LifetimePerSkillDuration;
            }
        }

        // Spawned by an effect that scales lifetime: pushed in right after Instantiate, which is
        // before Start, so the duration computed below already accounts for it.
        public void ApplyStackScaling(StackScaling scaling)
        {
            stackLifetimeMultiplier = scaling.Get(StackScalingTarget.Lifetime);
        }

        private void Start()
        {
            var progression = PlayerManager.Instance.GetPlayerComponent<PlayerProgression>();
            if (progression == null)
            {
                Debug.LogWarning("LifetimeByStat: No PlayerProgression found, using base duration.");
                Destroy(gameObject, baseDurationTime * stackLifetimeMultiplier);
                return;
            }

            float extra = progression.GetStatTotal(durationStat);
            float totalDuration = (baseDurationTime + (extra * durationScalingFactor)) * stackLifetimeMultiplier;

            Destroy(gameObject, totalDuration);
        }
    }

}