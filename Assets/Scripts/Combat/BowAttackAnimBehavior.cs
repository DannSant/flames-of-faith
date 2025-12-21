using Game.Control;
using UnityEngine;

namespace Game.Combat
{
    public class BowAttackAnimBehavior : StateMachineBehaviour
    {
        [SerializeField] private bool isSpecialAttack = false;
        [SerializeField] private float attackDelay = 0.6f;
        private bool hasFired;
        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            hasFired = false;

        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!hasFired && stateInfo.normalizedTime >= attackDelay)
            {
                hasFired = true;

                var attack = animator.GetComponent<CharacterVisual>();
                if (isSpecialAttack)
                {
                    attack?.SpecialAttackEndAnimEvent();
                }
                else {
                    attack?.AttackEndAnimEvent();
                }
               
            }
        }

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    
        //}

        // OnStateMove is called right after Animator.OnAnimatorMove()
        //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    // Implement code that processes and affects root motion
        //}

        // OnStateIK is called right after Animator.OnAnimatorIK()
        //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    // Implement code that sets up animation IK (inverse kinematics)
        //}
    }

}