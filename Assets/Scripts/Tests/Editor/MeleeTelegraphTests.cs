using Game.AI;
using Game.AI.Behaviors;
using NUnit.Framework;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>
    /// The melee attack warning (tester feedback, 2026-10-26): a ring and a growing disc show where the hit will
    /// land, and the direction is locked when the swing starts. Since the Light Devourer's redraw (Nov 2026) the
    /// windup is drawn into the art: the attack plays at normal speed and the disc fills over the time to its hit frame.
    /// </summary>
    public class MeleeTelegraphTests
    {
        private const string LightDevourerAttack = "Assets/Resources/Behaviors/LightDevourer/LDAttack.asset";
        private const string LightDevourerController = "Assets/Animation/Enemies/LightDevourer/LightDevourer.controller";

        // --- Pure rules ----------------------------------------------------------------------------------

        [TestCase(0f, 0.8f, 0f)]
        [TestCase(0.4f, 0.8f, 0.5f)]
        [TestCase(0.8f, 0.8f, 1f)]
        [TestCase(2f, 0.8f, 1f)]
        [TestCase(0.1f, 0f, 1f)]
        public void Fill_GrowsToFullWhenTheHitLands(float elapsed, float windup, float expected)
        {
            Assert.AreEqual(expected, AttackTelegraph.FillFraction(elapsed, windup), 0.0001f);
        }

        [Test]
        public void Hitbox_SitsAlongTheLockedDirection()
        {
            // On the ground: the vertical reach is shortened by the ground ratio (IsoGroundAreaTests)
            Assert.AreEqual(new Vector2(0f, -0.5f), AIMeleeAttacker.HitboxOffset(new Vector2(0f, -3f), 1f, 0.5f));
            Assert.AreEqual(new Vector2(0.5f, 0f), AIMeleeAttacker.HitboxOffset(Vector2.right, 0.5f, 0.5f));
            Assert.AreEqual(new Vector2(0f, -1f), AIMeleeAttacker.HitboxOffset(new Vector2(0f, -3f), 1f, 1f), "Ratio 1: the old circle");
            Assert.AreEqual(Vector2.zero, AIMeleeAttacker.HitboxOffset(Vector2.zero, 1f, 0.5f));
        }

        // --- Assets --------------------------------------------------------------------------------------

        [Test]
        public void Settings_HaveTheirSprites()
        {
            var settings = Resources.Load<AttackTelegraphSettings>("AI/AttackTelegraphSettings");
            Assert.IsNotNull(settings, "Missing Resources/AI/AttackTelegraphSettings");
            Assert.IsTrue(settings.ShowTelegraphs);
            Assert.IsNotNull(settings.RingSprite, "Ring sprite not set (or not imported as a Single sprite)");
            Assert.IsNotNull(settings.FillSprite, "Fill sprite not set (or not imported as a Single sprite)");
            Assert.Greater(settings.RingColor.a, 0f, "The ring must be visible");
        }

        [Test]
        public void EveryMeleeAttacker_GivesTimeToReact()
        {
            var attackers = AssetDatabase.FindAssets("t:AIMeleeAttacker")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<AIMeleeAttacker>)
                .ToList();
            Assert.IsNotEmpty(attackers, "No melee attackers found");
            foreach (var attacker in attackers)
            {
                // The old 0.3s swing was too short to react to (tester feedback, 2026-10-26)
                Assert.GreaterOrEqual(attacker.AnimationHitTime, 0.5f,
                    $"{attacker.name}: the warning lasts until the hit frame ({attacker.AnimationHitTime}s), too short to react");
            }
        }

        [Test]
        public void LightDevourer_HitTime_MatchesItsAnimation()
        {
            var attacker = AssetDatabase.LoadAssetAtPath<AIMeleeAttacker>(LightDevourerAttack);
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(LightDevourerController);
            Assert.IsNotNull(attacker, $"Missing {LightDevourerAttack}");
            Assert.IsNotNull(controller, $"Missing {LightDevourerController}");

            var state = controller.layers[0].stateMachine.states.Select(s => s.state)
                .FirstOrDefault(s => s.behaviours.OfType<MeleeAnimationAttackBehavior>().Any());
            Assert.IsNotNull(state, "No attack state with a MeleeAnimationAttackBehavior");
            Assert.IsFalse(state.speedParameterActive, "The attack plays at normal speed: nothing slows the windup any more");
            Assert.AreEqual(1f, state.speed, "The attack plays at normal speed");

            var hitBehaviour = new SerializedObject(state.behaviours.OfType<MeleeAnimationAttackBehavior>().First());
            float attackDelay = hitBehaviour.FindProperty("attackDelay").floatValue;
            float clipLength = ClipLength(state.motion);
            Assert.AreEqual(attackDelay * clipLength, attacker.AnimationHitTime, 0.02f,
                "Animation Hit Time must equal the attack state's Attack Delay x its clip length, or the hit misses the full warning");
        }

        private static float ClipLength(Motion motion) => motion switch
        {
            AnimationClip clip => clip.length,
            BlendTree tree => tree.children.Select(c => ClipLength(c.motion)).DefaultIfEmpty(0f).Max(),
            _ => 0f,
        };
    }
}
