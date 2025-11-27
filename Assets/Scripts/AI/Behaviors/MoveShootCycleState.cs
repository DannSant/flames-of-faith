using UnityEngine;

namespace Game.AI.Behaviors
{
    [System.Serializable]
    public class MoveShootCycleState
    {
        public bool hasTarget = false;
        public Vector2 targetPoint;

        public bool reachedPoint = false;
        public bool isShooting = false;

        public void ResetCycle()
        {
            hasTarget = false;
            reachedPoint = false;
            isShooting = false;
        }
    }
}
