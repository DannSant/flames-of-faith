using Game.AI.Behaviors;
using Game.Control;
using UnityEngine;

public class MeleeAnimationAttackBehavior : StateMachineBehaviour
{
    [SerializeField] private float attackDelay = 0.6f;
    [SerializeField] private float colliderDelay = 0.8f;
    [SerializeField] private string animationEventName = "Attack";
    private bool startEventFired;
    private bool endEventFired;
    //  OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        startEventFired = false;
        endEventFired = false;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (startEventFired && endEventFired)
        {
            return;
        }

        if (!startEventFired && stateInfo.normalizedTime >= attackDelay)
        {
            var behaviorController = animator.GetComponent<BehaviorController>();
            behaviorController?.OnAnimationEventStart(animationEventName);
            startEventFired = true;
        }

        if (!endEventFired && stateInfo.normalizedTime >= colliderDelay)
        {
            var behaviorController = animator.GetComponent<BehaviorController>();
            behaviorController?.OnAnimationEventEnd(animationEventName);
            endEventFired = true;
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
