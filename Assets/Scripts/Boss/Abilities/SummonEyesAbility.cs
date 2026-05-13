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
            var animator = boss.GetAnimator();

            if (animator != null && animationName != "")
            {
                animator.SetTrigger(animationName);
            }

            yield return new WaitForSeconds(1f); // cast delay

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
            }
            //context.activeAddsCount = eyes.Count;

            /*float spamTimer = 3f;

            while (eyes.Exists(e => e != null))
            {
                spamTimer -= Time.deltaTime;

                if (spamTimer <= 0f)
                {
                    // reuse volt logic? (later we can inject abilities)
                    spamTimer = 3f;
                }

                yield return null;
            }

            context.activeAddsCount = 0;

           boss.IncreaseEnrageLevel();*/
            yield return null;
        }
    }
}