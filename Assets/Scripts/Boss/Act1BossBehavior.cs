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


        private void Awake()
        {
            movement = GetComponent<BossMovement>();
            bossCollider = GetComponent<Collider2D>();
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
            // Move to random point when any ability is cast, and show boss if it was hidden   
           
            var abilityData = ability.GetBossAbility();
            var metadata = abilityData.abilityMetadata;
            if (metadata == null)
            {
                return;
            }


            if (metadata.Contains("moveToRandomPoint")) {
                movement.TeleportToRandomPoint();                
            }

            if (metadata.Contains("showSpriteOnAbilityStart"))
            {               
                bossRenderer.ToggleSprite(true);
                bossCollider.enabled = true;
            }


        }

        public override void OnAnimationEvent(string eventName)
        {
            if (eventName == "FadeOutEnd")
            {
                HandleFadeoutAnimationEnd();
            }
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

       public override string GetPhaseTransitionAnimationName()
        {
            return damageAnim;
        }
    }
}