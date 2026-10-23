using System.Collections.Generic;
using Game.AI.Behaviors;
using Game.Enemies;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>
    /// AIBehaviorShooter's Hold Still While Shooting: the Corruptor walks, so it stops for each shot (from the attack's
    /// start to its end event) by setting context.isShooting, which ChaserMovement holds position for. The FlameShooter
    /// floats and shares both behavior types, so with the toggle off nothing may change for it.
    /// </summary>
    public class ShooterHoldStillTests
    {
        private const string CorruptorShoot = "Assets/Resources/Behaviors/CorruptorAct1/CorruptorAct1Shoot.asset";
        private const string FlameShoot = "Assets/Resources/Behaviors/FlameShooter/FlameShoot.asset";

        private readonly List<Object> created = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in created) Object.DestroyImmediate(obj);
            created.Clear();
        }

        private T Track<T>(T obj) where T : Object
        {
            created.Add(obj);
            return obj;
        }

        private AIBehaviorShooter MakeShooter(bool holdStill)
        {
            var shooter = Track(ScriptableObject.CreateInstance<AIBehaviorShooter>());
            var so = new SerializedObject(shooter);
            so.FindProperty("maxRange").floatValue = 10f;
            so.FindProperty("shootCooldown").floatValue = 2f;
            so.FindProperty("holdStillWhileShooting").boolValue = holdStill;
            so.FindProperty("maxHoldTime").floatValue = 1.5f;
            so.ApplyModifiedPropertiesWithoutUndo();
            return shooter;
        }

        // An enemy in range of the player, with the fire point the shooter looks for. No animator: PlayShoot is skipped.
        private BehaviorContext MakeContext()
        {
            var enemy = Track(new GameObject("Enemy"));
            enemy.AddComponent<Rigidbody2D>().gravityScale = 0f;
            new GameObject("FirePoint").transform.SetParent(enemy.transform, false);
            var player = Track(new GameObject("Player"));
            player.transform.position = new Vector3(3f, 0f, 0f);

            var data = Track(ScriptableObject.CreateInstance<EnemyData>());
            data.moveSpeed = 2f;
            return new BehaviorContext
            {
                enemyGameObject = enemy,
                enemyTransform = enemy.transform,
                playerTransform = player.transform,
                enemyData = data,
            };
        }

        [Test]
        public void HoldOff_NeverMarksAShot()
        {
            var shooter = MakeShooter(holdStill: false);
            var context = MakeContext();

            shooter.Tick(context);

            Assert.AreEqual(2f, context.GetState<ShooterBehaviorState>(shooter).shootTimer, "It did shoot (the cooldown started)");
            Assert.IsFalse(context.isShooting, "A shooter that doesn't hold still marked the shot: the FlameShooter would stop");
        }

        [Test]
        public void HoldOff_EndEventLeavesAnotherBehaviorsShotAlone()
        {
            var shooter = MakeShooter(holdStill: false);
            var context = MakeContext();
            context.isShooting = true; // e.g. a ShootBurst in progress

            shooter.OnAnimationEventEnd(context, "ShootProjectile");
            shooter.OnStunStateChanged(context, true);

            Assert.IsTrue(context.isShooting);
        }

        [Test]
        public void HoldOn_HoldsFromTheShotToItsEndEvent()
        {
            var shooter = MakeShooter(holdStill: true);
            var context = MakeContext();

            shooter.Tick(context);
            Assert.IsTrue(context.isShooting, "Starting the shot holds the enemy still");

            shooter.Tick(context);
            Assert.IsTrue(context.isShooting, "Still holding mid-attack");

            shooter.OnAnimationEventEnd(context, "ShootProjectile");
            Assert.IsFalse(context.isShooting, "The attack's end releases it");
        }

        [Test]
        public void HoldOn_StunReleases()
        {
            var shooter = MakeShooter(holdStill: true);
            var context = MakeContext();
            shooter.Tick(context);

            shooter.OnStunStateChanged(context, false);
            Assert.IsTrue(context.isShooting, "Recovering from a stun isn't a release");

            shooter.OnStunStateChanged(context, true);
            Assert.IsFalse(context.isShooting, "A stun (or gameplay freeze) cuts the attack short");
        }

        [Test]
        public void HoldOn_ReleasesAfterMaxHoldTime_WhenTheEndEventNeverComes()
        {
            var shooter = MakeShooter(holdStill: true);
            var context = MakeContext();
            shooter.Tick(context);

            var state = context.GetState<ShooterBehaviorState>(shooter);
            Assert.AreEqual(Time.time + 1.5f, state.holdUntil, 0.1f, "The safety timeout is maxHoldTime after the shot");
            state.holdUntil = Time.time - 0.01f; // the safety timeout ran out

            shooter.Tick(context);
            Assert.IsFalse(context.isShooting);
        }

        [Test]
        public void ChaserMovement_HoldsPositionDuringAShot()
        {
            var movement = Track(ScriptableObject.CreateInstance<AIBehaviorChaserMovement>());
            var so = new SerializedObject(movement);
            so.FindProperty("avoidObstacles").boolValue = false;
            so.ApplyModifiedPropertiesWithoutUndo();

            var context = MakeContext();
            var rb = context.enemyTransform.GetComponent<Rigidbody2D>();
            context.isMoving = true;
            context.moveDirection = Vector2.right;

            movement.FixedTick(context);
            Assert.AreEqual(new Vector2(2f, 0f), rb.linearVelocity, "Moves when not shooting");

            context.isShooting = true;
            movement.FixedTick(context);
            Assert.AreEqual(Vector2.zero, rb.linearVelocity, "Stands still while shooting");
            Assert.IsTrue(context.isMoving, "isMoving belongs to the logic behavior");
        }

        [Test]
        public void Corruptor_HoldsStillToShoot_FlameShooterDoesNot()
        {
            Assert.IsTrue(HoldsStill(CorruptorShoot), "The Corruptor walks: it must stop to shoot");
            Assert.IsFalse(HoldsStill(FlameShoot), "The FlameShooter floats and shoots on the move");
        }

        private static bool HoldsStill(string path)
        {
            var asset = AssetDatabase.LoadAssetAtPath<AIBehaviorShooter>(path);
            Assert.IsNotNull(asset, $"Missing {path}");
            return new SerializedObject(asset).FindProperty("holdStillWhileShooting").boolValue;
        }
    }
}
