using System.Collections.Generic;
using Game.AI.Steering;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>
    /// Pure tests of the enemy obstacle steering math, with their own parameters so retuning
    /// ObstacleAvoidanceSettings never breaks them.
    /// </summary>
    public class ObstacleSteeringTests
    {
        private static readonly SteeringParams Params = new SteeringParams
        {
            sampleCount = 16,
            dangerWeight = 2f,
            dangerSharpness = 2f,
            sideBiasWeight = 0.5f,
            sideCommitTime = 1f,
            stuckSpeedRatio = 0.25f,
            stuckCheckWindow = 0.5f,
        };

        private static float AngleOf(Vector2 v) => Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;

        private static List<ObstacleSample> Obstacle(Vector2 direction, float proximity)
        {
            return new List<ObstacleSample> { new ObstacleSample(direction, proximity) };
        }

        [Test]
        public void NoObstacles_KeepsTheWantedDirection()
        {
            var state = new SteeringState();

            Vector2 result = ObstacleSteering.Steer(new Vector2(3f, 0f), new List<ObstacleSample>(), state, 0f, Params);

            Assert.AreEqual(Vector2.right, result);
            Assert.AreEqual(0, state.committedSide);
        }

        [Test]
        public void ObstacleBesideOrBehind_KeepsTheWantedDirection()
        {
            var obstacles = new List<ObstacleSample>
            {
                new ObstacleSample(Vector2.up, 1f),
                new ObstacleSample(Vector2.left, 1f),
            };

            Vector2 result = ObstacleSteering.Steer(Vector2.right, obstacles, new SteeringState(), 0f, Params);

            Assert.AreEqual(Vector2.right, result);
        }

        [Test]
        public void TouchingObstacleHeadOn_TurnsAlongItButNeverBackwards()
        {
            Vector2 result = ObstacleSteering.Steer(Vector2.right, Obstacle(Vector2.right, 1f), new SteeringState(), 0f, Params);

            float angle = Mathf.Abs(AngleOf(result));
            Assert.Greater(angle, 45f, "should turn well away from a touching obstacle");
            Assert.LessOrEqual(angle, 90f, "should still not move away from the target");
        }

        [Test]
        public void FarObstacle_TurnsLessThanNearOne()
        {
            Vector2 far = ObstacleSteering.Steer(Vector2.right, Obstacle(Vector2.right, 0.3f), new SteeringState(), 0f, Params);
            Vector2 near = ObstacleSteering.Steer(Vector2.right, Obstacle(Vector2.right, 1f), new SteeringState(), 0f, Params);

            Assert.Less(Mathf.Abs(AngleOf(far)), Mathf.Abs(AngleOf(near)));
        }

        [Test]
        public void ObstacleSlightlyLeft_GoesAroundOnTheRight()
        {
            Vector2 slightlyLeft = Quaternion.Euler(0f, 0f, 10f) * Vector2.right;

            Vector2 result = ObstacleSteering.Steer(Vector2.right, Obstacle(slightlyLeft, 1f), new SteeringState(), 0f, Params);

            Assert.Less(AngleOf(result), 0f);
        }

        [Test]
        public void CommittedSide_HoldsEvenWhenTheObstacleDriftsToTheOtherSide()
        {
            var state = new SteeringState();
            Vector2 slightlyLeft = Quaternion.Euler(0f, 0f, 10f) * Vector2.right;
            Vector2 slightlyRight = Quaternion.Euler(0f, 0f, -10f) * Vector2.right;

            ObstacleSteering.Steer(Vector2.right, Obstacle(slightlyLeft, 1f), state, 0f, Params);
            Vector2 next = ObstacleSteering.Steer(Vector2.right, Obstacle(slightlyRight, 1f), state, 0.1f, Params);

            Assert.AreEqual(-1, state.committedSide);
            Assert.Less(AngleOf(next), 0f, "a small wobble of the obstacle must not flip the side");
        }

        [Test]
        public void CommittedSide_ExpiresAfterTheCommitTime()
        {
            var state = new SteeringState { committedSide = -1, commitUntil = 1f };
            Vector2 slightlyRight = Quaternion.Euler(0f, 0f, -10f) * Vector2.right;

            Vector2 result = ObstacleSteering.Steer(Vector2.right, Obstacle(slightlyRight, 1f), state, 2f, Params);

            Assert.Greater(AngleOf(result), 0f);
            Assert.AreEqual(1, state.committedSide);
        }

        [Test]
        public void Stuck_NextToAnObstacle_SwitchesSides()
        {
            var state = new SteeringState { committedSide = 1, commitUntil = 10f };

            ObstacleSteering.UpdateStuck(state, Vector2.zero, 2f, 0f, true, 0f, Params);
            bool switched = ObstacleSteering.UpdateStuck(state, new Vector2(0.05f, 0f), 2f, 0.5f, true, 0.5f, Params);

            Assert.IsTrue(switched);
            Assert.AreEqual(-1, state.committedSide);
        }

        [Test]
        public void MovingFine_OrAwayFromObstacles_IsNotStuck()
        {
            var moving = new SteeringState { committedSide = 1, commitUntil = 10f };
            ObstacleSteering.UpdateStuck(moving, Vector2.zero, 2f, 0f, true, 0f, Params);
            bool movingSwitched = ObstacleSteering.UpdateStuck(moving, new Vector2(0.9f, 0f), 2f, 0.5f, true, 0.5f, Params);

            var open = new SteeringState { committedSide = 1, commitUntil = 10f };
            ObstacleSteering.UpdateStuck(open, Vector2.zero, 2f, 0f, false, 0f, Params);
            bool openSwitched = ObstacleSteering.UpdateStuck(open, Vector2.zero, 2f, 0.5f, false, 0.5f, Params);

            Assert.IsFalse(movingSwitched);
            Assert.IsFalse(openSwitched);
            Assert.AreEqual(1, moving.committedSide);
            Assert.AreEqual(1, open.committedSide);
        }

        [Test]
        public void ProximityFromClearance_IsOneWhenTouchingAndZeroAtLookAhead()
        {
            Assert.AreEqual(1f, ObstacleSteering.ProximityFromClearance(-0.1f, 0.8f));
            Assert.AreEqual(1f, ObstacleSteering.ProximityFromClearance(0f, 0.8f));
            Assert.AreEqual(0.5f, ObstacleSteering.ProximityFromClearance(0.4f, 0.8f), 0.0001f);
            Assert.AreEqual(0f, ObstacleSteering.ProximityFromClearance(0.8f, 0.8f));
        }
    }
}
