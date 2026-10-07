using Game.Progression;
using Game.Waves;
using UnityEngine;

namespace Game.AI.Behaviors
{

    [CreateAssetMenu(menuName = "Behaviors/On Death/Drop Exp On Death")]
    public class DropExperienceOnDeathBehavior : AIDeathBehavior
    {
        [SerializeField] private GameObject experienceTokenPrefab;

        public override void OnDeath(BehaviorContext context)
        {
            if (!Application.isPlaying) return;


            if (experienceTokenPrefab == null)
            {
                Debug.LogWarning("ExperienceToken prefab not assigned.");
                return;
            }

            var enemy = context.enemyGameObject;
            var enemyData = context.enemyData;

            if (enemy == null || enemyData == null)
            {
                Debug.LogWarning("[DropExp] Missing enemy context.");
                return;
            }

            var settings = ExperienceSettings.Instance;
            if (Random.value > settings.DropChance)
            {
                return;
            }

            var token = Instantiate(experienceTokenPrefab, enemy.transform.position, Quaternion.identity);
            var experience = token.GetComponent<ExperienceToken>();
            if (experience == null)
            {
                return;
            }

            float baseXp = WaveSpawner.Instance != null && WaveSpawner.Instance.CurrentWaveBaseXp > 0f
                ? WaveSpawner.Instance.CurrentWaveBaseXp
                : settings.FallbackXp;

            var denomination = WaveExperienceCalculator.RollDenomination(enemyData.xpTier, settings);
            float multiplier = denomination != null ? denomination.multiplier : 1f;
            Sprite sprite = denomination != null ? denomination.sprite : null;

            experience.Setup(baseXp * multiplier, sprite);

            if (WaveSpawner.Instance != null)
            {
                WaveSpawner.Instance.RegisterExperienceDrop(baseXp * multiplier);
            }
        }
    }
}
