using Game.Combat.Projectiles;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public class ProjectileSteeringTests
    {
        private const float AngleTolerance = 0.01f;

        private static float AngleOf(Vector2 v) => Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;

        [Test]
        public void Steer_TurnsByAtMostTheLimit()
        {
            // Target 90 degrees to the left, limit 10 degrees.
            Vector2 result = ProjectileSteering.Steer(Vector2.right, Vector2.up, 10f);

            Assert.AreEqual(10f, AngleOf(result), AngleTolerance);
            Assert.AreEqual(1f, result.magnitude, 0.0001f);
        }

        [Test]
        public void Steer_TurnsClockwiseForTargetsOnTheRight()
        {
            Vector2 result = ProjectileSteering.Steer(Vector2.right, Vector2.down, 10f);

            Assert.AreEqual(-10f, AngleOf(result), AngleTolerance);
        }

        [Test]
        public void Steer_NeverOvershoots_WhenTheTargetIsWithinTheLimit()
        {
            Vector2 toTarget = Quaternion.Euler(0f, 0f, 5f) * Vector2.right;

            Vector2 result = ProjectileSteering.Steer(Vector2.right, toTarget * 3f, 30f);

            Assert.AreEqual(5f, AngleOf(result), AngleTolerance);
        }

        [Test]
        public void Steer_TargetStraightAhead_KeepsTheDirection()
        {
            Vector2 result = ProjectileSteering.Steer(Vector2.right, new Vector2(4f, 0f), 45f);

            Assert.AreEqual(0f, AngleOf(result), AngleTolerance);
        }

        [Test]
        public void Steer_TargetDirectlyBehind_TurnsByTheLimit_AndStaysIn2D()
        {
            Vector2 result = ProjectileSteering.Steer(Vector2.right, Vector2.left, 20f);

            Assert.AreEqual(20f, Mathf.Abs(AngleOf(result)), AngleTolerance);
            Assert.AreEqual(1f, result.magnitude, 0.0001f);
        }

        [Test]
        public void Steer_ZeroVectors_AreHandled()
        {
            Assert.AreEqual(Vector2.right, ProjectileSteering.Steer(Vector2.right * 2f, Vector2.zero, 30f));
            Assert.AreEqual(Vector2.up, ProjectileSteering.Steer(Vector2.zero, Vector2.up * 5f, 30f));
        }

        [Test]
        public void Steer_ZeroOrNegativeLimit_DoesNotTurn()
        {
            Assert.AreEqual(0f, AngleOf(ProjectileSteering.Steer(Vector2.right, Vector2.up, 0f)), AngleTolerance);
            Assert.AreEqual(0f, AngleOf(ProjectileSteering.Steer(Vector2.right, Vector2.up, -5f)), AngleTolerance);
        }

        [Test]
        public void Steer_RepeatedSteps_ConvergeOnTheTarget()
        {
            // 120 deg/s at the physics step (0.02 s) = 2.4 deg per step: 90 deg takes ~38 steps.
            Vector2 direction = Vector2.right;
            for (int i = 0; i < 50; i++)
            {
                direction = ProjectileSteering.Steer(direction, Vector2.up, 120f * 0.02f);
            }

            Assert.AreEqual(90f, AngleOf(direction), AngleTolerance);
        }

        [TestCase(0f, true)]
        [TestCase(45f, true)]
        [TestCase(89f, true)]
        [TestCase(91f, false)]
        [TestCase(180f, false)]
        public void SteeringCone_StopsOnceTheTargetIsBehind(float targetAngle, bool expected)
        {
            Vector2 toTarget = Quaternion.Euler(0f, 0f, targetAngle) * Vector2.right;

            Assert.AreEqual(expected, ProjectileSteering.IsWithinSteeringCone(Vector2.right, toTarget, 90f));
        }

        [Test]
        public void SteeringCone_ZeroVectors_DoNotSteer()
        {
            Assert.IsFalse(ProjectileSteering.IsWithinSteeringCone(Vector2.right, Vector2.zero, 90f));
            Assert.IsFalse(ProjectileSteering.IsWithinSteeringCone(Vector2.zero, Vector2.up, 90f));
        }
    }
}
