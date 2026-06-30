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
        private Collider2D bossCollider;

        [SerializeField] private List<Transform> patrolPoints = new List<Transform>();  

        private bool isOnPhase2 = false;
        private Transform currentPatrolPoint;
        private EnemyHealth bossHealth;


        private void Awake()
        {
            movement = GetComponent<BossMovement>();
            bossCollider = GetComponent<Collider2D>();
            bossHealth = GetComponent<EnemyHealth>();

            bossCollider.enabled = false;

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
            bossRenderer.ToggleSprite(true);
            bossCollider.enabled = true;
        }

        public void HandleFadeoutAnimationEnd() {
            bossRenderer.ToggleSprite(false);
            bossCollider.enabled = false; // Disable collider to prevent player from hitting invisible boss

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
            //Disable collider
            bossCollider.enabled = false;

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
    }
}