using UnityEngine;

namespace Game.Boss
{
    public abstract class BossBehavior : MonoBehaviour
    {
        protected BossController boss;
        protected BossRenderer bossRenderer;

        public virtual void Initialize(BossController bossController)
        {
            boss = bossController;
            bossRenderer = bossController.GetComponent<BossRenderer>();

        }



        public abstract void OnPhaseOneStart();
        public abstract void OnPhaseTwoStart();

        public abstract string GetPhaseTransitionAnimationName();

        public abstract void OnAnimationEvent(string eventName);
    }
}
