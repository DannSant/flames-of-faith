using Game.Combat;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Boss
{
    public class Act1BossBehavior : BossBehavior
    {
        [SerializeField] string fadeOutAnim = "FadeOut";
        [SerializeField] string damageAnim = "Damage";
        private BossMovement movement;

        [SerializeField] private List<Transform> patrolPoints = new List<Transform>();  

        private bool isOnPhase2 = false;
        private Transform currentPatrolPoint;
        private EnemyHealth bossHealth;


        private void Awake()
        {
            movement = GetComponent<BossMovement>();
            bossHealth = GetComponent<EnemyHealth>();

            // The boss starts hidden and untargetable; BossRenderer.Awake handles both.

            bossHealth.onDeath += HandleBossDeath;
        }

        private void OnDisable()
        {
            bossHealth.onDeath -= HandleBossDeath;
        }

        private void Update()
        {
           
            if (isOnPhase2)
            {
                Phase2MovementUpdate();
            }

        }

        private void Phase2MovementUpdate()
        {
            if (movement == null)
            {
               
                return;
            }

            // Do not issue movement while casting
            if (boss.IsCastingAbility())
            {
               
                return;
            }

            // If already moving → nothing to do
            if (movement.IsMoving)
            {
               
                return;
            }          
            
            if(bossHealth != null && bossHealth.IsDead())
            {
                return;
            }

            SelectNextPatrolPoint();
        }

        private void SelectNextPatrolPoint()
        {
            //Debug.Log("Selecting next patrol point...");
            if (patrolPoints.Count == 0)
                return;

            List<Transform> validPoints =
                patrolPoints
                .Where(p => p != currentPatrolPoint)
                .ToList();

            if (validPoints.Count == 0)
                validPoints = patrolPoints;

            currentPatrolPoint = validPoints[Random.Range(0, validPoints.Count)];

            //Debug.Log($"Boss moving to patrol point: {currentPatrolPoint.name}");

            movement.MoveTo(currentPatrolPoint.position);
        }

        public override void OnPhaseOneStart()
        {
            boss.OnAbilityCasted += HandleAbilityCastPhaseOne;          
            boss.OnAllAddsDeath += HandleAllAddsDead;           
        }

        public override void OnPhaseTwoStart()
        {
            boss.OnAbilityCasted -= HandleAbilityCastPhaseOne;            
            boss.OnAllAddsDeath -= HandleAllAddsDead;
            StopAllCoroutines();

            // Enable phase 2 movement
            isOnPhase2 = true;

        }

        private void HandleAbilityCastPhaseOne(BossAbilityRuntime ability)
        {         
         
            var abilityData = ability.GetBossAbility();
            var metadata = abilityData.abilityMetadata;
            if (metadata == null)
            {
                return;
            }

            // Move to random point when any ability is cast, and show boss if it was hidden   
            if (metadata.Contains("moveToRandomPoint")) {
                movement.TeleportToRandomPoint();                
            }

        }

        public override void OnAnimationEvent(string eventName)
        {
            if (eventName == "FadeOutEnd")
            {
                HandleFadeoutAnimationEnd();
            }

            if (eventName == "FadeInStart")
            {
                HandleFadeInAnimationStart();
            }
        }

        public void HandleFadeInAnimationStart()
        {
            bossRenderer.SetVisible(true);
        }

        public void HandleFadeoutAnimationEnd() {
            // SetVisible also drops the collider, so the player can't hit an invisible boss.
            bossRenderer.SetVisible(false);
        }

        private IEnumerator HandleAllAddsDeadRoutine()
        {
            bossRenderer.TriggerAnimation(fadeOutAnim);
            yield return null;
        }

        private void HandleAllAddsDead()
        {
            StartCoroutine(HandleAllAddsDeadRoutine());
        }

        private void HandleBossDeath()
        {
            // Targetable, not visible: the boss stays on screen for its death animation but must
            // stop taking hits.
            bossRenderer.SetTargetable(false);

            //Search for summoned objects and destroy them
            var summonedObjects = Object.FindObjectsByType<SummonedObject>(FindObjectsSortMode.None);
            foreach (var obj in summonedObjects)
            {
                if (obj.shouldBeDestroyedOnBossDeath())
                {
                    Destroy(obj.gameObject);
                }
            }
        }

        public override string GetPhaseTransitionAnimationName()
        {
            return damageAnim;
        }

        public override string GetFadeOutAnimationName()
        {
            return fadeOutAnim;
        }
    }
}