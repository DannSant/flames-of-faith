using System.Collections;
using UnityEngine;

namespace Game.Boss
{
    public class Act1BossBehavior : BossBehavior
    {
        private BossMovement movement;
        private Animator animator;
        private SpriteRenderer spriteRenderer;

        private bool eyesActive = false;

        private void Awake()
        {
            movement = GetComponent<BossMovement>();
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();

        }

        public override void OnPhaseOneStart()
        {
            boss.OnAbilityCasted += HandleAbilityCastPhaseOne;
            boss.OnAbilityFinished += HandleAbilityFinishedPhaseOne;
            //StartCoroutine(PhaseOneLoop());
        }

        public override void OnPhaseTwoStart()
        {
            boss.OnAbilityCasted -= HandleAbilityCastPhaseOne;
            boss.OnAbilityFinished -= HandleAbilityFinishedPhaseOne;
            StopAllCoroutines();          
            // Phase 2 movement logic later
        }

        private void HandleAbilityCastPhaseOne(BossAbilityRuntime ability)
        {
            // Move to random point when any ability is cast, and show boss if it was hidden
            //TODO only move when eyes are not active
            movement.MoveToRandomPoint();
            spriteRenderer.enabled = true;
        }

        private void HandleAbilityFinishedPhaseOne(BossAbilityRuntime ability)
        {
            //TODO Hide boss when ability finishes if eyes are not active
            spriteRenderer.enabled = false;
        }

        /*private IEnumerator PhaseOneLoop()
        {
            yield return null;
        }*/

        // Called externally by ability
        public void SetEyesActive(bool value)
        {
            eyesActive = value;
        }
    }
}