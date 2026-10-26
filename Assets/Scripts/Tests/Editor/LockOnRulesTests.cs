using Game.Combat;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>
    /// The lock-on tap (tester feedback, 2026-10-26): a tap locks, then cycles clockwise through the enemies in
    /// lock range, and only releases the lock once no enemy is in range (the player walked away).
    /// </summary>
    public class LockOnRulesTests
    {
        // --- What a tap does --------------------------------------------------------------------------------

        [TestCase(false, 3, 3, LockTap.Lock, TestName = "NotLocked_EnemiesNearby_Locks")]
        [TestCase(false, 0, 0, LockTap.Nothing, TestName = "NotLocked_NoEnemies_DoesNothing")]
        [TestCase(true, 3, 2, LockTap.Cycle, TestName = "Locked_OthersNearby_Cycles")]
        [TestCase(true, 0, 0, LockTap.Unlock, TestName = "Locked_WalkedAway_Unlocks")]
        [TestCase(true, 1, 0, LockTap.Nothing, TestName = "Locked_OnlyTheLockedEnemyNearby_KeepsTheLock")]
        public void Tap(bool locked, int nearby, int others, LockTap expected)
        {
            Assert.AreEqual(expected, LockOnRules.DecideTap(locked, nearby, others));
        }

        // --- Clockwise cycling ------------------------------------------------------------------------------

        [Test]
        public void Cycle_GoesClockwise()
        {
            // Current target straight up; candidates right, down, left: clockwise from up is right first
            var others = new[] { new Vector2(-1f, 0f), new Vector2(0f, -1f), new Vector2(1f, 0f) };
            Assert.AreEqual(2, LockOnRules.NextClockwise(Vector2.zero, new Vector2(0f, 1f), others));
        }

        [Test]
        public void Cycle_VisitsEveryEnemyBeforeComingBack()
        {
            var enemies = new[] { new Vector2(0f, 1f), new Vector2(1f, 0f), new Vector2(0f, -1f), new Vector2(-1f, 0f) };
            int current = 0;
            var visited = new System.Collections.Generic.List<int> { current };
            for (int step = 0; step < 3; step++)
            {
                var others = new System.Collections.Generic.List<Vector2>();
                var indices = new System.Collections.Generic.List<int>();
                for (int i = 0; i < enemies.Length; i++)
                {
                    if (i == current) continue;
                    others.Add(enemies[i]);
                    indices.Add(i);
                }
                current = indices[LockOnRules.NextClockwise(Vector2.zero, enemies[current], others)];
                visited.Add(current);
            }
            CollectionAssert.AreEqual(new[] { 0, 1, 2, 3 }, visited, "Up, right, down, left");
        }

        [Test]
        public void Cycle_IsMeasuredAroundThePlayer_NotTheWorldOrigin()
        {
            var player = new Vector2(10f, 10f);
            var others = new[] { new Vector2(9f, 10f), new Vector2(11f, 10f) };
            Assert.AreEqual(1, LockOnRules.NextClockwise(player, new Vector2(10f, 11f), others), "Right of the player comes first");
        }

        [Test]
        public void Cycle_WithNoOtherEnemy_HasNoNext()
        {
            Assert.AreEqual(-1, LockOnRules.NextClockwise(Vector2.zero, Vector2.up, new Vector2[0]));
        }
    }
}
