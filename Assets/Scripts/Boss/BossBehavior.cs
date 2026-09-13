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

        /// <summary>
        /// Animation that makes the boss vanish - reused for both a mid-fight hide and, on player
        /// death, making the boss disappear for good.
        /// </summary>
        public abstract string GetFadeOutAnimationName();

        public abstract void OnAnimationEvent(string eventName);
    }
}
