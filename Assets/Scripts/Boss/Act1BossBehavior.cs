using System.Collections;
using UnityEngine;

namespace Game.Boss
{
    public class Act1BossBehavior : BossBehavior
    {
        [SerializeField] string fadeOutAnim = "FadeOut";
        [SerializeField] string damageAnim = "Damage";
        private BossMovement movement;
        //private Animator animator;
       

        private bool eyesActive = false;

        private void Awake()
        {
            movement = GetComponent<BossMovement>();
            //animator = GetComponentInChildren<Animator>();
           // spriteRenderer = GetComponentInChildren<SpriteRenderer>();

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
            // Phase 2 movement logic later

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
            }


        }

        /*private void HandleAbilityFinishedPhaseOne(BossAbilityRuntime ability)
        {
            var abilityData = ability.GetBossAbility();
            var metadata = abilityData.abilityMetadata;
            if (metadata == null)
            {
                return;
            }
            if (metadata.Contains("hideSpriteOnAbilityEnd"))
            {
                spriteRenderer.enabled = false;
            }
           
        }*/

        public override void OnAnimationEvent(string eventName)
        {
            if (eventName == "FadeOutEnd")
            {
                HandleFadeoutAnimationEnd();
            }
        }

        public void HandleFadeoutAnimationEnd() {
            bossRenderer.ToggleSprite(false);
            //TODO: disable hitbox here if we add one, or disable damage dealing component

        }

        private IEnumerator HandleAllAddsDeadRoutine()
        {
            bossRenderer.TriggerAnimation(fadeOutAnim);
            yield return null;
            //yield return new WaitForSeconds(1f);
            // Hide boss again when all adds are dead
            //bossRenderer.ToggleSprite(false);
        }

        private void HandleAllAddsDead()
        {
            StartCoroutine(HandleAllAddsDeadRoutine());
        }

        /*private IEnumerator PhaseOneLoop()
        {
            yield return null;
        }*/

       public override string GetPhaseTransitionAnimationName()
        {
            return damageAnim;
        }
    }
}