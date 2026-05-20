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
        
        public int baseEyeCount = 4;

        public override IEnumerator Execute(BossController boss, BossAbilityRuntime bossAbilityRuntime, BossAbilityContext context)
        {
            yield return new WaitForSeconds(1.5f); // wait for the fade in animation to finish
            var bossRenderer = boss.GetBossRenderer();

            if (bossRenderer != null && animationName != "")
            {
                bossRenderer.TriggerAnimation(animationName);
            }
           

            yield return new WaitForSeconds(1.5f); // cast delay
           

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
                    enemyComponent.Initialize(boss.GetEnrageLevel()+1); 
                }
                yield return new WaitForSeconds(0.1f); // stagger spawn
            }

            yield return null;
        }
    }
}