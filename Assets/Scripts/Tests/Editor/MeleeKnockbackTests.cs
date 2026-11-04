using System.Collections.Generic;
using Game.AI.Behaviors;
using Game.Enemies;
using Game.Misc;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>
    /// Tester feedback (v0.5.2): a Light Devourer could be knocked back mid-swing, so its warning lied about where the
    /// hit lands. AIMeleeAttacker makes its Knockback immune for the swing (the same span it holds context.isShooting):
    /// from the windup to the end event, a stun or a gameplay freeze (which reaches behaviors as a stun).
    /// </summary>
    public class MeleeKnockbackTests
    {
        private readonly List<Object> created = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in created) if (obj != null) Object.DestroyImmediate(obj);
            created.Clear();
        }

        private T Track<T>(T obj) where T : Object
        {
            created.Add(obj);
            return obj;
        }

        private AIMeleeAttacker MakeAttacker()
        {
            var attacker = Track(ScriptableObject.CreateInstance<AIMeleeAttacker>());
            var so = new SerializedObject(attacker);
            so.FindProperty("maxRange").floatValue = 10f;
            so.FindProperty("attackCooldown").floatValue = 2f;
            so.ApplyModifiedPropertiesWithoutUndo();
            return attacker;
        }

        // An enemy in range with the melee hitbox child the behavior looks for. No EnemyDamage on the hitbox (that
        // would need a GameSession) and no animator (PlayAttack is skipped).
        private (BehaviorContext context, Knockback knockback, Transform source) MakeEnemy()
        {
            var enemy = Track(new GameObject("Enemy"));
            enemy.AddComponent<Rigidbody2D>().gravityScale = 0f;
            var knockback = enemy.AddComponent<Knockback>();
            var hitbox = new GameObject("MeleeCollider");
            hitbox.transform.SetParent(enemy.transform, false);
            hitbox.AddComponent<CircleCollider2D>();

            var player = Track(new GameObject("Player"));
            player.transform.position = new Vector3(2f, 0f, 0f);

            var data = Track(ScriptableObject.CreateInstance<EnemyData>());
            var context = new BehaviorContext
            {
                enemyGameObject = enemy,
                enemyTransform = enemy.transform,
                playerTransform = player.transform,
                enemyData = data,
            };
            return (context, knockback, player.transform);
        }

        [Test]
        public void Knockback_WorksWhenNotAttacking()
        {
            var (_, knockback, source) = MakeEnemy();

            Assert.IsFalse(knockback.IsImmune);
            knockback.ApplyKnockback(source, 5f);
            Assert.IsTrue(knockback.IsKnockbacked);
        }

        [Test]
        public void Swing_IgnoresKnockback_UntilItsEndEvent()
        {
            var attacker = MakeAttacker();
            var (context, knockback, source) = MakeEnemy();

            attacker.Tick(context);
            Assert.IsTrue(context.GetState<MeleeAttackerBehaviorState>(attacker).isAttacking, "The swing started");
            Assert.IsTrue(knockback.IsImmune, "Immune for the swing");

            knockback.ApplyKnockback(source, 5f);
            Assert.IsFalse(knockback.IsKnockbacked, "A hit mid-swing must not push the enemy");

            attacker.OnAnimationEventEnd(context, "Attack");
            Assert.IsFalse(knockback.IsImmune, "The end event releases it");
            knockback.ApplyKnockback(source, 5f);
            Assert.IsTrue(knockback.IsKnockbacked, "After the swing, knockback works again");
        }

        [Test]
        public void StunOrFreeze_ReleasesTheImmunity()
        {
            var attacker = MakeAttacker();
            var (context, knockback, source) = MakeEnemy();
            attacker.Tick(context);

            attacker.OnStunStateChanged(context, false);
            Assert.IsTrue(knockback.IsImmune, "Recovering from a stun isn't a release");

            attacker.OnStunStateChanged(context, true);
            Assert.IsFalse(knockback.IsImmune, "A stun (or a gameplay freeze) cuts the swing short and releases it");
            knockback.ApplyKnockback(source, 5f);
            Assert.IsTrue(knockback.IsKnockbacked);
        }

        [Test]
        public void KnockbackBeforeTheSwing_EndsWhenTheSwingStarts()
        {
            var attacker = MakeAttacker();
            var (context, knockback, source) = MakeEnemy();
            knockback.ApplyKnockback(source, 5f);
            Assert.IsTrue(knockback.IsKnockbacked);

            attacker.Tick(context);

            Assert.IsFalse(knockback.IsKnockbacked, "The swing ends the knockback, so movement takes the body back");
            Assert.IsTrue(context.isShooting, "...and holds it still, which stops the slide");
        }
    }
}
