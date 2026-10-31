using System.Collections.Generic;
using System.Linq;
using Game.Control;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>
    /// A primary attack's animation speeds up with its cooldown, so the hit or shot always lands. Every class's
    /// controller must follow: a new controller that forgets the parameter brings the bug back.
    /// </summary>
    public class AttackAnimationSpeedTests
    {
        private const string Parameter = "AttackSpeed";

        [TestCase(1.5f, 1.5f, 1f)]
        [TestCase(1.5f, 0.75f, 2f)]
        [TestCase(2f, 0.5f, 4f)]
        public void Speed_IsBaseCooldownOverCurrentCooldown(float baseCooldown, float cooldown, float expected)
        {
            Assert.AreEqual(expected, StatsCalculations.CalculateAttackAnimationSpeed(baseCooldown, cooldown), 0.0001f);
        }

        [Test]
        public void Speed_NeverDropsBelowNormal_WithNegativeAttackSpeed()
        {
            float cooldown = StatsCalculations.CalculateAttackDelay(-20f, 1.5f, 0.075f);

            Assert.Greater(cooldown, 1.5f);
            Assert.AreEqual(1f, StatsCalculations.CalculateAttackAnimationSpeed(1.5f, cooldown));
        }

        public static IEnumerable<CharacterClassData> Classes() => AssetDatabase.FindAssets("t:CharacterClassData")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<CharacterClassData>);

        [Test]
        public void EveryClass_PrimaryAttackState_FollowsTheParameter()
        {
            var classes = Classes().ToList();
            Assert.IsNotEmpty(classes, "No classes found");

            foreach (var data in classes)
            {
                var controller = data.animatorController as AnimatorController;
                Assert.IsNotNull(controller, $"{data.name}: no AnimatorController");

                var parameter = controller.parameters.FirstOrDefault(p => p.name == Parameter);
                Assert.IsNotNull(parameter, $"{controller.name} needs the {Parameter} float parameter");
                Assert.AreEqual(AnimatorControllerParameterType.Float, parameter.type, $"{controller.name}: {Parameter} must be a float");
                Assert.AreEqual(1f, parameter.defaultFloat, $"{controller.name}: {Parameter} must default to 1, or attacks freeze until it's set");

                var attack = StateTriggeredBy(controller, "Attack");
                Assert.IsNotNull(attack, $"{controller.name}: no state entered by the Attack trigger");
                Assert.IsTrue(attack.speedParameterActive && attack.speedParameter == Parameter,
                    $"{controller.name}: {attack.name}'s speed must follow {Parameter}, or high Attack Speed cuts the hit off");
                Assert.AreEqual(1f, attack.speed, $"{controller.name}: {attack.name}'s base speed");
            }
        }

        // The class data's controller is only swapped in depending on Awake order, so the prefab's must be the same one
        [Test]
        public void EveryClass_DataAndPrefab_UseTheSameController()
        {
            foreach (var data in Classes())
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/Player/Player{data.name}.prefab");
                Assert.IsNotNull(prefab, $"No prefab for {data.name}");
                var animator = prefab.GetComponentInChildren<CharacterVisual>(true).GetComponent<Animator>();
                Assert.AreEqual(animator.runtimeAnimatorController, data.animatorController, $"{data.name}: the class data and the prefab disagree");
            }
        }

        [Test]
        public void EveryClass_SpecialAttackState_KeepsItsSpeed()
        {
            foreach (var data in Classes())
            {
                var controller = (AnimatorController)data.animatorController;
                var special = StateTriggeredBy(controller, "SpecialAttack");
                Assert.IsNotNull(special, $"{controller.name}: no state entered by the SpecialAttack trigger");
                Assert.IsFalse(special.speedParameterActive, $"{controller.name}: a special's timing carries gameplay, so it stays at speed 1");
            }
        }

        /// <summary>The state an Any State transition on this trigger leads to.</summary>
        public static AnimatorState StateTriggeredBy(AnimatorController controller, string trigger) =>
            controller.layers[0].stateMachine.anyStateTransitions
                .FirstOrDefault(t => t.conditions.Any(c => c.parameter == trigger))?.destinationState;
    }
}
