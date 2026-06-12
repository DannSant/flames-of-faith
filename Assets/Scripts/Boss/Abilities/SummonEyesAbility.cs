using Game.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Boss
{
    [CreateAssetMenu(menuName = "Boss/Abilities/SummonEyes")]
    public class SummonEyesAbility : BossAbilityBase
    {
        public GameObject eyePrefab;

        [SerializeField]
        private int baseEyeCount = 4;

        [SerializeField]
        private float fadeInDelay = 1.5f;

        [SerializeField]
        private float castDelay = 1.5f;

        [SerializeField]
        private float spawnStagger = 0.1f;

        [SerializeField]
        private int enemyEnrageOffset = 1;

        public override IEnumerator Execute(BossController boss, BossAbilityRuntime bossAbilityRuntime, BossAbilityContext context)
        {
            yield return new WaitForSeconds(fadeInDelay); // wait for the fade in animation to finish
            var bossRenderer = boss.GetBossRenderer();

            if (bossRenderer != null && animationName != "")
            {
                bossRenderer.TriggerAnimation(animationName);
            }
           

            yield return new WaitForSeconds(castDelay); // cast delay
           

            List<GameObject> eyes = new();
            var spawnPoints = boss.GetAddsSpawnPoints();

            int eyeCount = baseEyeCount + boss.GetEnrageLevel(); // scale with enrage level

            for (int i = 0; i < eyeCount; i++)
            {
                var spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];
                var eye = Instantiate(eyePrefab, spawn.position, Quaternion.identity);
                eyes.Add(eye);
                var addComponent = eye.GetComponent<BossAdd>();
                if (addComponent != null)
                {
                    addComponent.Initialize(boss);
                    boss.RegisterAdd(eye);
                }

                var enemyComponent = eye.GetComponent<Enemy>();
                if (enemyComponent != null) { 
                    enemyComponent.Initialize(boss.GetEnrageLevel() + enemyEnrageOffset); 
                }
                yield return new WaitForSeconds(spawnStagger); // stagger spawn
            }

            yield return null;
        }
    }
}