using UnityEngine;

namespace Game.Boss {
    public class BossAnimationEventHandler : MonoBehaviour
    {
        [SerializeField] private BossController bossController;
        [SerializeField] private BossBehavior bossBehavior;

        public void HandleAnimationEvent(string eventName)
        {
            bossBehavior?.OnAnimationEvent(eventName);
        }

    }

}