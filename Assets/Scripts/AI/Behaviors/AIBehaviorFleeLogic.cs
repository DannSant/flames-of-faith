using UnityEngine;

namespace Game.AI.Behaviors
{
    public class FleeBehaviorState
    {
        public float strafeSign;
        public float nextStrafeFlipTime;
    }

    /// <summary>
    /// Runs away from the player while they are within the flee range. A sideways strafe is mixed in so the
    /// enemy slides along walls instead of getting pinned in a corner. Pair with a FixedUpdate movement behavior
    /// (e.g. ChaserMovement) that applies context.moveDirection.
    /// </summary>
    [CreateAssetMenu(menuName = "Behaviors/On Update/FleeLogic")]
    public class AIBehaviorFleeLogic : AIUpdateBehavior
    {
        [Tooltip("Starts fleeing when the player is closer than this distance.")]
        [SerializeField] private float fleeRange = 6f;
        [Tooltip("Stops fleeing once the player is farther than fleeRange + buffer.")]
        [SerializeField] private float buffer = 1f;
        [Tooltip("How much sideways movement is mixed into the flee direction (0 = straight away from the player).")]
        [Range(0f, 1f)]
        [SerializeField] private float strafeWeight = 0.4f;
        [Tooltip("Random interval range (seconds) for switching the strafe side.")]
        [SerializeField] private Vector2 strafeFlipInterval = new Vector2(1.5f, 3f);

        public override void Tick(BehaviorContext context)
        {
            if (context.playerTransform == null || context.enemyTransform == null)
                return;

            var state = context.GetState<FleeBehaviorState>(this);
            if (state.strafeSign == 0f || Time.time >= state.nextStrafeFlipTime)
            {
                state.strafeSign = Random.value < 0.5f ? -1f : 1f;
                state.nextStrafeFlipTime = Time.time + Random.Range(strafeFlipInterval.x, strafeFlipInterval.y);
            }

            Vector2 awayFromPlayer = context.enemyTransform.position - context.playerTransform.position;
            float distance = awayFromPlayer.magnitude;

            if (distance > fleeRange + buffer)
            {
                context.isMoving = false;
                return;
            }

            if (distance < fleeRange)
            {
                Vector2 away = distance > 0.001f ? awayFromPlayer / distance : Random.insideUnitCircle.normalized;
                Vector2 strafe = new Vector2(-away.y, away.x) * state.strafeSign;
                context.moveDirection = (away * (1f - strafeWeight) + strafe * strafeWeight).normalized;
                context.isMoving = true;
            }
        }
    }
}
