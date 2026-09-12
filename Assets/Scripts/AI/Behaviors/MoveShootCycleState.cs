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

        // Handle to the running burst so it can be cancelled deterministically (e.g. on a stun)
        // instead of relying on the routine's finally block to run when it is stopped.
        [System.NonSerialized] public Coroutine shootRoutine;

        public void ResetCycle()
        {
            hasTarget = false;
            reachedPoint = false;
            isShooting = false;
            shootRoutine = null;
        }
    }
}
