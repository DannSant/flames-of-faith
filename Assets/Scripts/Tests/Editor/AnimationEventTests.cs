using Game.AI.Behaviors;
using Game.Common;
using Game.Control;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>
    /// Animation events on the clips of a blend tree fire once per clip, in the same frame. The Warrior's attack
    /// is an 8-direction blend tree, so its start event ran the cleave 8 times per swing; DrekoEye and
    /// FlameShooter shot 8 stacked volleys. CharacterVisual (player) and BehaviorController (enemies) forward
    /// each event once per frame.
    /// </summary>
    public class AnimationEventTests
    {
        // Animation event methods that go through a once-per-frame gate
        private static readonly HashSet<string> GatedEvents = new()
        {
            "AttackStartAnimEvent", "AttackEndAnimEvent", "SpecialAttackStartAnimEvent", "SpecialAttackEndAnimEvent", // CharacterVisual
            "OnAnimationEventStart", "OnAnimationEventEnd", // BehaviorController
        };

        [Test]
        public void OncePerFrame_PassesOncePerFrame()
        {
            var gate = new OncePerFrame();
            Assert.IsTrue(gate.TryPass(5));
            Assert.IsFalse(gate.TryPass(5), "Second call in the same frame");
            Assert.IsFalse(gate.TryPass(5));
            Assert.IsTrue(gate.TryPass(6), "Next frame");
        }

        [Test]
        public void CharacterVisual_ForwardsEachEventOncePerFrame()
        {
            var go = new GameObject("CharacterVisual");
            try
            {
                var visual = go.AddComponent<CharacterVisual>();
                int starts = 0, ends = 0, specialStarts = 0, specialEnds = 0;
                visual.OnAttackStartAnimEvent += () => starts++;
                visual.OnAttackEndAnimEvent += () => ends++;
                visual.OnSpecialAttackStartAnimEvent += () => specialStarts++;
                visual.OnSpecialAttackEndAnimEvent += () => specialEnds++;

                // The 8 clips of the attack blend tree, all firing in this frame
                for (int i = 0; i < 8; i++)
                {
                    visual.AttackStartAnimEvent();
                    visual.AttackEndAnimEvent();
                    visual.SpecialAttackStartAnimEvent();
                    visual.SpecialAttackEndAnimEvent();
                }

                Assert.AreEqual(1, starts, "Attack start (runs the swing)");
                Assert.AreEqual(1, ends, "Attack end");
                Assert.AreEqual(1, specialStarts, "Special attack start");
                Assert.AreEqual(1, specialEnds, "Special attack end");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void BehaviorController_ForwardsEachEventOncePerFrame()
        {
            var go = new GameObject("Enemy");
            var receiver = ScriptableObject.CreateInstance<CountingAnimationEventReceiver>();
            try
            {
                var controller = go.AddComponent<BehaviorController>();
                controller.updateBehaviors = new List<AIUpdateBehavior> { receiver };
                controller.fixedUpdateBehaviors = new List<AIFixedUpdateBehavior>();
                controller.collisionBehaviors = new List<AICollisionBehavior>();
                controller.triggerBehaviors = new List<AITriggerBehavior>();
                controller.deathBehaviors = new List<AIDeathBehavior>();

                // DrekoEye's 8 shoot clips, all firing in this frame
                for (int i = 0; i < 8; i++)
                {
                    controller.OnAnimationEventStart("ShootProjectile");
                    controller.OnAnimationEventEnd("ShootProjectile");
                }
                controller.OnAnimationEventStart("Hop");

                Assert.AreEqual(1, receiver.Count(receiver.Starts, "ShootProjectile"), "One volley per shot");
                Assert.AreEqual(1, receiver.Count(receiver.Ends, "ShootProjectile"));
                Assert.AreEqual(1, receiver.Count(receiver.Starts, "Hop"), "Another event in the same frame still goes through");
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(receiver);
            }
        }

        [Test]
        public void EventsRepeatedAcrossABlendTree_AreGatedAndFireInTheSameFrame()
        {
            var controllers = AssetDatabase.FindAssets("t:AnimatorController", new[] { "Assets/Animation" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<AnimatorController>)
                .Where(c => c != null)
                .ToList();
            Assert.IsNotEmpty(controllers);

            int checkedGroups = 0;
            foreach (var controller in controllers)
            {
                foreach (var tree in controller.layers.SelectMany(l => BlendTrees(l.stateMachine)))
                {
                    var clips = Clips(tree).Distinct().ToList();
                    var groups = clips
                        .SelectMany(clip => AnimationUtility.GetAnimationEvents(clip).Select(e => (clip, e)))
                        .GroupBy(x => (x.e.functionName, x.e.stringParameter))
                        .Where(g => g.Select(x => x.clip).Distinct().Count() > 1);

                    foreach (var group in groups)
                    {
                        checkedGroups++;
                        string where = $"{controller.name} / {tree.name}: {group.Key.functionName}({group.Key.stringParameter})";
                        Assert.IsTrue(GatedEvents.Contains(group.Key.functionName),
                            $"{where} is on several clips of a blend tree, so it fires once per clip: route it through a once-per-frame gate");

                        // Blend tree clips play in sync by normalized time; equal normalized times fire in the same frame
                        float first = group.First().e.time / group.First().clip.length;
                        foreach (var (clip, e) in group)
                        {
                            Assert.AreEqual(first, e.time / clip.length, 0.001f, $"{where}: {clip.name} fires at another moment than the others");
                        }
                    }
                }
            }
            Assert.Greater(checkedGroups, 0, "The Warrior attack, DrekoEye and FlameShooter blend trees should be checked");
        }

        private static IEnumerable<BlendTree> BlendTrees(AnimatorStateMachine machine)
        {
            foreach (var state in machine.states)
            {
                if (state.state.motion is BlendTree tree)
                {
                    yield return tree;
                }
            }
            foreach (var child in machine.stateMachines)
            {
                foreach (var tree in BlendTrees(child.stateMachine)) yield return tree;
            }
        }

        private static IEnumerable<AnimationClip> Clips(BlendTree tree)
        {
            foreach (var child in tree.children)
            {
                if (child.motion is AnimationClip clip) yield return clip;
                else if (child.motion is BlendTree nested)
                {
                    foreach (var nestedClip in Clips(nested)) yield return nestedClip;
                }
            }
        }
    }

    /// <summary>Counts the animation events a BehaviorController forwards (test only, never saved as an asset).</summary>
    public class CountingAnimationEventReceiver : AIUpdateBehavior, IAnimationEventReceiver
    {
        public readonly Dictionary<string, int> Starts = new();
        public readonly Dictionary<string, int> Ends = new();

        public override void Tick(BehaviorContext context) { }

        public void OnAnimationEventStart(BehaviorContext context, string eventName) => Add(Starts, eventName);

        public void OnAnimationEventEnd(BehaviorContext context, string eventName) => Add(Ends, eventName);

        public int Count(Dictionary<string, int> counts, string eventName) =>
            counts.TryGetValue(eventName, out int count) ? count : 0;

        private static void Add(Dictionary<string, int> counts, string eventName) =>
            counts[eventName] = (counts.TryGetValue(eventName, out int count) ? count : 0) + 1;
    }
}
