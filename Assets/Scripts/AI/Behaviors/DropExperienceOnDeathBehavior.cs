using Game.AI;
using Game.AI.Behaviors;
using Game.Enemies;
using Game.Progression;
using Game.Scene;
using UnityEngine;

namespace Game.AI.Behaviors
{

    [CreateAssetMenu(menuName = "Behaviors/On Death/Drop Exp On Death")]
    public class DropExperienceOnDeathBehavior : AIDeathBehavior
    {
        [SerializeField] private GameObject experienceTokenPrefab;
        [SerializeField] private float dropChance = 0.8f;

        
       
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


            if (Random.value <= dropChance)
            {
                var token = Instantiate(experienceTokenPrefab, enemy.transform.position, Quaternion.identity);
                var experience = token.GetComponent<ExperienceToken>();
                int bonusXpPerLevel = GameSession.Instance.LevelsBeaten * enemyData.xpPerLevel;

                if (experience != null)
                {
                    experience.SetAmount(enemyData.xpBase + bonusXpPerLevel);
                }
            }
        }
    }
}
