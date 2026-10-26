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
    /// land, the direction is locked when the swing starts, and the attack animation is slowed so the hit lands
    /// when the disc is full (Telegraph Time).
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
        public void Animation_IsSlowedSoTheHitLandsAtTelegraphTime()
        {
            float speed = AIMeleeAttacker.AttackAnimationSpeed(0.35f, 0.8f);
            Assert.AreEqual(0.4375f, speed, 0.0001f);
            Assert.AreEqual(0.8f, 0.35f / speed, 0.0001f, "The hit frame lands at Telegraph Time");
            Assert.AreEqual(1f, AIMeleeAttacker.AttackAnimationSpeed(0.35f, 0f), "No telegraph time: normal speed");
        }

        [Test]
        public void Hitbox_SitsAlongTheLockedDirection()
        {
            Assert.AreEqual(new Vector2(0f, -1f), AIMeleeAttacker.HitboxOffset(new Vector2(0f, -3f), 1f));
            Assert.AreEqual(new Vector2(0.5f, 0f), AIMeleeAttacker.HitboxOffset(Vector2.right, 0.5f));
            Assert.AreEqual(Vector2.zero, AIMeleeAttacker.HitboxOffset(Vector2.zero, 1f));
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
        public void EveryMeleeAttacker_WarnsLongerThanTheOldSwing()
        {
            var attackers = AssetDatabase.FindAssets("t:AIMeleeAttacker")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<AIMeleeAttacker>)
                .ToList();
            Assert.IsNotEmpty(attackers, "No melee attackers found");
            foreach (var attacker in attackers)
            {
                Assert.Greater(attacker.TelegraphTime, attacker.AnimationHitTime,
                    $"{attacker.name}: the warning must last longer than the swing used to (it was {attacker.AnimationHitTime}s, too short to react)");
            }
        }

        [Test]
        public void LightDevourer_HitTime_MatchesItsAnimation()
        {
            var attacker = AssetDatabase.LoadAssetAtPath<AIMeleeAttacker>(LightDevourerAttack);
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(LightDevourerController);
            Assert.IsNotNull(attacker, $"Missing {LightDevourerAttack}");
            Assert.IsNotNull(controller, $"Missing {LightDevourerController}");

            Assert.IsTrue(controller.parameters.Any(p => p.name == "AttackSpeed" && p.type == AnimatorControllerParameterType.Float),
                "The controller needs the AttackSpeed float parameter");

            var state = controller.layers[0].stateMachine.states.Select(s => s.state)
                .FirstOrDefault(s => s.behaviours.OfType<MeleeAnimationAttackBehavior>().Any());
            Assert.IsNotNull(state, "No attack state with a MeleeAnimationAttackBehavior");
            Assert.IsTrue(state.speedParameterActive && state.speedParameter == "AttackSpeed",
                "The attack state's speed must follow AttackSpeed, or the windup isn't slowed");

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
