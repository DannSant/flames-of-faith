using Game.Progression;
using Game.Scene;
using UnityEngine;

namespace Game.Utils
{
    public class LifetimeByStat : MonoBehaviour
    {
        [Header("Lifetime Settings")]
        [SerializeField] private float baseDurationTime = 3f;
        [SerializeField] private StatType durationStat = StatType.SkillDuration;

        private void Start()
        {
            var progression = PlayerManager.Instance.GetPlayerComponent<PlayerProgression>();
            if (progression == null)
            {
                Debug.LogWarning("LifetimeByStat: No PlayerProgression found, using base duration.");
                Destroy(gameObject, baseDurationTime);
                return;
            }

            float extra = progression.GetStatTotal(durationStat);
            float totalDuration = baseDurationTime + extra;

            Destroy(gameObject, totalDuration);
        }
    }

}