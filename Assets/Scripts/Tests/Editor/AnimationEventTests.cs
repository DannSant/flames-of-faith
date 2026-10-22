using Game.Common;
using Game.Control;
using NUnit.Framework;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>
    /// Animation events on the clips of a blend tree fire once per clip, in the same frame. The Warrior's
    /// attack is an 8-direction blend tree, so its start event ran the cleave 8 times per swing (8x cleave
    /// damage and knockback, 8 Ley Charge chains per charged enemy). CharacterVisual forwards each event once
    /// per frame.
    /// </summary>
    public class AnimationEventTests
    {
        private const string WarriorAnimationFolder = "Assets/Animation/PlayerWarriorIso";

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

                Assert.AreEqual(1, starts, "Attack start (runs the cleave)");
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
        public void WarriorAttackClips_FireTheirStartEventInTheSameFrame()
        {
            // The once-per-frame gate relies on every directional clip firing its copy at the same moment
            var clips = Directory.GetFiles(WarriorAnimationFolder, "Attack_*.anim")
                .Select(path => AssetDatabase.LoadAssetAtPath<AnimationClip>(path.Replace('\\', '/')))
                .ToList();
            Assert.AreEqual(8, clips.Count, "The 8 directional attack clips");

            float? length = null, startTime = null;
            foreach (var clip in clips)
            {
                Assert.IsNotNull(clip);
                var starts = AnimationUtility.GetAnimationEvents(clip).Where(e => e.functionName == "AttackStartAnimEvent").ToList();
                Assert.AreEqual(1, starts.Count, $"{clip.name}: one AttackStartAnimEvent");

                length ??= clip.length;
                startTime ??= starts[0].time;
                Assert.AreEqual(length.Value, clip.length, 0.0001f, $"{clip.name}: same length as the others");
                Assert.AreEqual(startTime.Value, starts[0].time, 0.0001f, $"{clip.name}: start event at the same time as the others");
            }
        }
    }
}
