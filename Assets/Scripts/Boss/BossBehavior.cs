using UnityEngine;

namespace Game.Boss
{
    public abstract class BossBehavior : MonoBehaviour
    {
        protected BossController boss;

        public virtual void Initialize(BossController bossController)
        {
            boss = bossController;
        }

        public abstract void OnPhaseOneStart();
        public abstract void OnPhaseTwoStart();

        public abstract void OnAnimationEvent(string eventName);
    }
}
