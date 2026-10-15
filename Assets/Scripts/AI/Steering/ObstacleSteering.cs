using System.Collections.Generic;
using Game.AI.Behaviors;
using UnityEngine;

namespace Game.AI.Steering
{
    /// <summary>An obstacle as seen from the enemy: which way it is, and how close (0 = at look-ahead range, 1 = touching).</summary>
    public struct ObstacleSample
    {
        public Vector2 direction;
        public float proximity;

        public ObstacleSample(Vector2 direction, float proximity)
        {
            this.direction = direction.normalized;
            this.proximity = proximity;
        }
    }

    /// <summary>The tunables the pure steering math needs, copied from ObstacleAvoidanceSettings.</summary>
    public struct SteeringParams
    {
        public int sampleCount;
        public float dangerWeight;
        public float dangerSharpness;
        public float sideBiasWeight;
        public float sideCommitTime;
        public float stuckSpeedRatio;
        public float stuckCheckWindow;
    }

    /// <summary>Per-enemy steering memory, kept on the BehaviorContext.</summary>
    public class SteeringState
    {
        // +1 = going around obstacles counter-clockwise from the wanted direction (to its left), -1 = clockwise, 0 = none.
        public int committedSide;
        public float commitUntil;

        public bool hasWindow;
        public Vector2 windowStart;
        public float windowTimer;
        public float windowExpectedDistance;

        public float radius = -1f;
        public Collider2D ownCollider;
    }

    /// <summary>
    /// Local obstacle avoidance for enemies (context steering). Levels have only a few small, convex obstacles
    /// (tree trunks), so a NavMesh isn't needed: each tick the wanted direction is compared against a ring of
    /// candidate directions, each scored by how well it follows the wanted one minus the danger of the obstacles
    /// it points at. The enemy commits to one side for a while so it doesn't flip-flop, and switches sides if it
    /// stops making progress. Movement behaviors keep their own logic and only pass their final direction here.
    /// </summary>
    public static class ObstacleSteering
    {
        private const float Epsilon = 0.0001f;

        private static readonly List<Collider2D> overlapResults = new();
        private static readonly List<ObstacleSample> sampleBuffer = new();

        // --- Gameplay entry point --------------------------------------------------------------------------

        /// <summary>
        /// Returns the direction to move in instead of <paramref name="desired"/>, going around obstacles.
        /// <paramref name="expectedSpeed"/> is the speed the enemy should be moving at (0 = no stuck check,
        /// e.g. a single hop), and <paramref name="deltaTime"/> the time since the previous call.
        /// </summary>
        public static Vector2 SteerAroundObstacles(BehaviorContext context, Rigidbody2D rb, Vector2 desired,
            float expectedSpeed, float deltaTime)
        {
            var settings = ObstacleAvoidanceSettings.Instance;
            if (!settings.AvoidanceEnabled || context == null || rb == null || desired.sqrMagnitude < Epsilon)
            {
                return desired;
            }

            var state = context.GetState<SteeringState>(settings);
            if (state.radius < 0f)
            {
                CacheOwnCollider(context, state, settings);
            }

            Vector2 center = state.ownCollider != null ? (Vector2)state.ownCollider.bounds.center : rb.position;
            CollectObstacles(center, state, settings, sampleBuffer);

            var p = settings.ToParams();
            UpdateStuck(state, rb.position, expectedSpeed, deltaTime, sampleBuffer.Count > 0, Time.time, p);
            Vector2 result = Steer(desired, sampleBuffer, state, Time.time, p);

            if (settings.DebugDraw)
            {
                Debug.DrawRay(center, desired.normalized, Color.white);
                Debug.DrawRay(center, result, Color.cyan);
            }
            return result;
        }

        private static void CacheOwnCollider(BehaviorContext context, SteeringState state, ObstacleAvoidanceSettings settings)
        {
            state.radius = settings.RadiusPadding;
            if (context.enemyGameObject == null) return;

            foreach (var col in context.enemyGameObject.GetComponents<Collider2D>())
            {
                if (col.isTrigger) continue;
                state.ownCollider = col;
                Vector3 extents = col.bounds.extents;
                state.radius = Mathf.Min(extents.x, extents.y) + settings.RadiusPadding;
                return;
            }
        }

        private static void CollectObstacles(Vector2 center, SteeringState state, ObstacleAvoidanceSettings settings,
            List<ObstacleSample> samples)
        {
            samples.Clear();
            float lookAhead = Mathf.Max(settings.LookAhead, Epsilon);

            var filter = new ContactFilter2D { useLayerMask = true, layerMask = settings.ObstacleMask, useTriggers = false };
            Physics2D.OverlapCircle(center, state.radius + lookAhead, filter, overlapResults);

            foreach (var hit in overlapResults)
            {
                if (hit == null || hit == state.ownCollider) continue;

                Vector2 toObstacle = hit.ClosestPoint(center) - center;
                float distance = toObstacle.magnitude;
                if (distance < Epsilon)
                {
                    // Already overlapping: ClosestPoint returns the center itself, so aim at the obstacle's middle.
                    toObstacle = (Vector2)hit.bounds.center - center;
                    distance = 0f;
                    if (toObstacle.sqrMagnitude < Epsilon) continue;
                }

                float proximity = ProximityFromClearance(distance - state.radius, lookAhead);
                if (proximity <= 0f) continue;
                samples.Add(new ObstacleSample(toObstacle, proximity));
            }
        }

        // --- Pure math (EditMode tested) ------------------------------------------------------------------

        /// <summary>1 when touching (clearance 0 or less), falling to 0 at <paramref name="lookAhead"/>.</summary>
        public static float ProximityFromClearance(float clearance, float lookAhead)
        {
            return 1f - Mathf.Clamp01(clearance / lookAhead);
        }

        /// <summary>
        /// Picks the direction to move in and keeps the side commitment in <paramref name="state"/> up to date.
        /// With no obstacle in the way it returns the wanted direction (normalized), unchanged.
        /// </summary>
        public static Vector2 Steer(Vector2 desired, IReadOnlyList<ObstacleSample> obstacles, SteeringState state,
            float time, in SteeringParams p)
        {
            if (desired.sqrMagnitude < Epsilon) return desired;
            Vector2 wanted = desired.normalized;

            if (state.committedSide != 0 && time > state.commitUntil)
            {
                state.committedSide = 0;
            }

            // No side yet: go around on the side away from whatever blocks the way the most.
            int side = state.committedSide != 0 ? state.committedSide : PreferredSide(wanted, obstacles, p);

            Vector2 result = ChooseDirection(wanted, obstacles, side, p);

            float turn = Cross(wanted, result);
            if (Mathf.Abs(turn) > 0.01f)
            {
                int resultSide = turn > 0f ? 1 : -1;
                if (state.committedSide == 0 || state.committedSide == resultSide)
                {
                    state.committedSide = resultSide;
                    state.commitUntil = time + p.sideCommitTime;
                }
            }
            return result;
        }

        /// <summary>
        /// The best of the wanted direction and <c>sampleCount</c> directions around it. Each scores
        /// <c>dot(dir, wanted) − dangerWeight × danger(dir)</c>, plus a bias toward <paramref name="side"/>
        /// that grows with how blocked the wanted direction is.
        /// </summary>
        public static Vector2 ChooseDirection(Vector2 wanted, IReadOnlyList<ObstacleSample> obstacles, int side,
            in SteeringParams p)
        {
            wanted = wanted.normalized;
            if (obstacles == null || obstacles.Count == 0) return wanted;

            float blocked = Danger(wanted, obstacles, p.dangerSharpness);
            if (blocked <= 0f) return wanted;

            Vector2 best = wanted;
            float bestScore = Score(wanted, wanted, obstacles, side, blocked, p);

            int count = Mathf.Max(4, p.sampleCount);
            float step = 360f / count;
            for (int i = 1; i < count; i++)
            {
                Vector2 dir = Rotate(wanted, step * i);
                float score = Score(dir, wanted, obstacles, side, blocked, p);
                if (score > bestScore + Epsilon)
                {
                    bestScore = score;
                    best = dir;
                }
            }
            return best;
        }

        /// <summary>
        /// Advances the stuck check. Over each <c>stuckCheckWindow</c> the enemy should cover
        /// <c>expectedSpeed × time</c>; covering less than <c>stuckSpeedRatio</c> of it next to an obstacle means
        /// the chosen side is blocked, so it switches sides. Returns true when it switched.
        /// </summary>
        public static bool UpdateStuck(SteeringState state, Vector2 position, float expectedSpeed, float deltaTime,
            bool nearObstacle, float time, in SteeringParams p)
        {
            if (expectedSpeed <= 0f || p.stuckCheckWindow <= 0f) return false;

            if (!state.hasWindow)
            {
                StartWindow(state, position);
                return false;
            }

            state.windowTimer += deltaTime;
            state.windowExpectedDistance += expectedSpeed * deltaTime;
            if (state.windowTimer < p.stuckCheckWindow) return false;

            float moved = Vector2.Distance(position, state.windowStart);
            bool stuck = nearObstacle && moved < state.windowExpectedDistance * p.stuckSpeedRatio;
            StartWindow(state, position);
            if (!stuck) return false;

            state.committedSide = state.committedSide == 0 ? 1 : -state.committedSide;
            state.commitUntil = time + p.sideCommitTime;
            return true;
        }

        private static void StartWindow(SteeringState state, Vector2 position)
        {
            state.hasWindow = true;
            state.windowStart = position;
            state.windowTimer = 0f;
            state.windowExpectedDistance = 0f;
        }

        private static float Score(Vector2 dir, Vector2 wanted, IReadOnlyList<ObstacleSample> obstacles, int side,
            float blocked, in SteeringParams p)
        {
            float score = Vector2.Dot(dir, wanted) - p.dangerWeight * Danger(dir, obstacles, p.dangerSharpness);
            if (side != 0)
            {
                score += p.sideBiasWeight * blocked * Mathf.Max(0f, Cross(wanted, dir) * side);
            }
            return score;
        }

        // How much heading in dir runs into obstacles: each counts by its proximity, fading for directions
        // that point away from it (sharpness > 1 makes it fade faster, so turns are gradual instead of 90°).
        private static float Danger(Vector2 dir, IReadOnlyList<ObstacleSample> obstacles, float sharpness)
        {
            float danger = 0f;
            foreach (var obstacle in obstacles)
            {
                float facing = Vector2.Dot(dir, obstacle.direction);
                if (facing <= 0f) continue;
                danger = Mathf.Max(danger, obstacle.proximity * Mathf.Pow(facing, Mathf.Max(1f, sharpness)));
            }
            return danger;
        }

        private static int PreferredSide(Vector2 wanted, IReadOnlyList<ObstacleSample> obstacles, in SteeringParams p)
        {
            float worst = 0f;
            int side = 1;
            foreach (var obstacle in obstacles)
            {
                float facing = Vector2.Dot(wanted, obstacle.direction);
                if (facing <= 0f) continue;
                float danger = obstacle.proximity * Mathf.Pow(facing, Mathf.Max(1f, p.dangerSharpness));
                if (danger <= worst) continue;
                worst = danger;
                // Obstacle to the left of the wanted direction → go around on the right, and vice versa.
                side = Cross(wanted, obstacle.direction) > 0f ? -1 : 1;
            }
            return side;
        }

        private static float Cross(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;

        private static Vector2 Rotate(Vector2 v, float degrees)
        {
            float rad = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
        }
    }
}
