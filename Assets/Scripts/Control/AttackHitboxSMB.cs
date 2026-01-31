using UnityEngine;

namespace Game.Control
{
    public class AttackHitboxSMB : StateMachineBehaviour
    {
        public bool isSpecialAttack = false;
        private CharacterVisual characterVisual;
        public override void OnStateEnter(Animator animator,AnimatorStateInfo stateInfo, int layerIndex)
        {
            if(characterVisual == null)
            {
                characterVisual = animator.GetComponent<CharacterVisual>();
            }
                
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
        {
            if (characterVisual == null)
            {
                Debug.LogWarning("CharacterVisual component not found on the animator's GameObject.");
                return;
            }

            if (isSpecialAttack)
            {
                characterVisual.SpecialAttackEndAnimEvent();
            }
            else
            {
                characterVisual.AttackEndAnimEvent();
            }           
            
        }        
    }

}