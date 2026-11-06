using System.Linq;
using Game.Enemies.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>
    /// The Light Devourer's redrawn art (Tools/Flames of Faith/Enemies/Build Light Devourer Art, the shared
    /// EnemySheetArtBuilder): the sheets import sharp and are named after their real frame counts, every clip
    /// plays all the frames of its own row at 12 fps, and the controller keeps its GUID, its three directional
    /// trees and the melee hit behaviour on the attack.
    /// </summary>
    public class LightDevourerArtTests
    {
        // The controller's GUID: the prefab and any override refer to it, so a rebuild must keep it
        private const string ControllerGuid = "d6b556c8d0deb894097a99ea2178679f";

        [TestCase("LDIdle_SS_16f_192x192.png", 16)]
        [TestCase("LDWalk_SS_8f_192x192.png", 8)]
        [TestCase("LDAttack_SS_11f_192x192.png", 11)]
        public void Sheet_IsNamedAfterItsRealFrameCount_AndImportsSharp(string fileName, int frames)
        {
            string path = LightDevourerArtBuilder.SpriteFolder + "/" + fileName;
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            Assert.IsNotNull(importer, $"Missing {path}");
            importer.GetSourceTextureWidthAndHeight(out int width, out int height);
            Assert.IsTrue(EnemySheetArtBuilder.TryParseSheetName(fileName, out int named, out int cellWidth, out int cellHeight));
            Assert.AreEqual(frames, named);
            Assert.AreEqual(frames * cellWidth, width, $"{fileName}: the name's frame count doesn't match the sheet's width");
            Assert.AreEqual(EnemySheetArtBuilder.Directions.Length * cellHeight, height, $"{fileName}: 8 rows");

            Assert.GreaterOrEqual(importer.maxTextureSize, Mathf.Max(width, height), $"{fileName} is shrunk on import: every frame goes blurry");
            var standalone = importer.GetPlatformTextureSettings("Standalone");
            if (standalone.overridden)
            {
                Assert.GreaterOrEqual(standalone.maxTextureSize, Mathf.Max(width, height), $"{fileName}: the Standalone override shrinks it");
            }
            Assert.AreEqual(EnemySheetArtBuilder.PixelsPerUnit, importer.spritePixelsPerUnit, fileName);
            Assert.AreEqual(FilterMode.Bilinear, importer.filterMode, fileName);
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            Assert.AreEqual(SpriteMeshType.FullRect, settings.spriteMeshType, $"{fileName}: Full Rect leaves room for the Corrupted outline");

            var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToList();
            Assert.AreEqual(frames * EnemySheetArtBuilder.Directions.Length, sprites.Count, $"{fileName}: {frames} frames x 8 directions");
            foreach (var sprite in sprites)
            {
                Assert.AreEqual(new Vector2(cellWidth, cellHeight), sprite.rect.size, sprite.name);
            }
        }

        [Test]
        public void EverySheet_IsBuilt()
        {
            CollectionAssert.AreEquivalent(
                new[] { EnemySheetArtBuilder.IdleState, EnemySheetArtBuilder.WalkState, EnemySheetArtBuilder.AttackState },
                LightDevourerArtBuilder.Sheets.Select(s => s.State));
        }

        [Test]
        public void Clips_PlayEveryFrameOfTheirOwnRow_At12Fps_WithoutEvents()
        {
            var definition = LightDevourerArtBuilder.Definition;
            foreach (var sheet in LightDevourerArtBuilder.Sheets)
            {
                EnemySheetArtBuilder.TryParseSheetName(sheet.FileName, out int frames, out _, out _);
                for (int row = 0; row < EnemySheetArtBuilder.Directions.Length; row++)
                {
                    string clipPath = definition.ClipPath(sheet.State, EnemySheetArtBuilder.Directions[row]);
                    var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
                    Assert.IsNotNull(clip, $"Missing {clipPath}");
                    Assert.AreEqual(EnemySheetArtBuilder.FrameRate, clip.frameRate, clip.name);
                    Assert.AreEqual(sheet.Loop, AnimationUtility.GetAnimationClipSettings(clip).loopTime, $"{clip.name} loop");
                    // The hit comes from the attack state's MeleeAnimationAttackBehavior, not from clip events
                    Assert.IsEmpty(AnimationUtility.GetAnimationEvents(clip), $"{clip.name} has events");

                    var binding = AnimationUtility.GetObjectReferenceCurveBindings(clip).Single();
                    var keys = AnimationUtility.GetObjectReferenceCurve(clip, binding);
                    Assert.AreEqual(frames, keys.Length, $"{clip.name}: one key per frame, no duplicated closing key");
                    Assert.AreEqual(frames / EnemySheetArtBuilder.FrameRate, clip.length, 1e-4f, clip.name);
                    for (int frame = 0; frame < frames; frame++)
                    {
                        Assert.AreEqual(EnemySheetArtBuilder.SpriteName(sheet.FileName, row, frame, frames), keys[frame].value.name,
                            $"{clip.name} frame {frame} shows another row or sheet");
                    }
                }
            }
        }

        [Test]
        public void Controller_KeepsItsGuid_AndHasThreeDirectionalTrees()
        {
            Assert.AreEqual(ControllerGuid, AssetDatabase.AssetPathToGUID(LightDevourerArtBuilder.ControllerPath),
                "The controller was recreated: its GUID changed");
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(LightDevourerArtBuilder.ControllerPath);
            var machine = controller.layers[0].stateMachine;
            Assert.AreEqual(EnemySheetArtBuilder.IdleState, machine.defaultState?.name);
            Assert.AreEqual(3, machine.states.Length, "Idle, Walk and Attack only");
            Assert.AreEqual(3, AssetDatabase.LoadAllAssetsAtPath(LightDevourerArtBuilder.ControllerPath).OfType<BlendTree>().Count(),
                "Blend trees left behind by an earlier build");

            foreach (string parameter in new[] { "DirectionX", "DirectionY", "IsMoving", LightDevourerArtBuilder.AttackTrigger })
            {
                Assert.IsTrue(controller.parameters.Any(p => p.name == parameter), $"Missing parameter {parameter} (EnemyAnimationController sets it)");
            }

            foreach (var child in machine.states)
            {
                var tree = child.state.motion as BlendTree;
                Assert.IsNotNull(tree, $"{child.state.name} isn't a blend tree");
                Assert.AreEqual(BlendTreeType.SimpleDirectional2D, tree.blendType);
                Assert.AreEqual(("DirectionX", "DirectionY"), (tree.blendParameter, tree.blendParameterY));
                Assert.AreEqual(8, tree.children.Select(c => c.position).Distinct().Count(), $"{child.state.name}: 8 directions");
            }

            var attack = machine.states.Select(s => s.state).Single(s => s.name == EnemySheetArtBuilder.AttackState);
            Assert.IsTrue(machine.states.Select(s => s.state).Where(s => s != attack)
                    .All(s => s.transitions.Any(t => t.destinationState == attack && t.conditions.Any(c => c.parameter == LightDevourerArtBuilder.AttackTrigger))),
                "The Attack trigger must reach the attack from Idle and Walk");
            Assert.AreEqual(1, attack.behaviours.OfType<MeleeAnimationAttackBehavior>().Count(), "The attack needs one MeleeAnimationAttackBehavior");
        }

        [Test]
        public void AttackHit_LandsOnTheChosenFrame_AndEndsAfterIt()
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(LightDevourerArtBuilder.ControllerPath);
            var attack = controller.layers[0].stateMachine.states.Select(s => s.state).Single(s => s.name == EnemySheetArtBuilder.AttackState);
            var hit = new SerializedObject(attack.behaviours.OfType<MeleeAnimationAttackBehavior>().Single());
            float attackDelay = hit.FindProperty("attackDelay").floatValue;
            float colliderDelay = hit.FindProperty("colliderDelay").floatValue;

            Assert.AreEqual(8f / 11f, attackDelay, 1e-4f, "The blow lands on the 9th of 11 frames");
            Assert.Greater(colliderDelay, attackDelay, "The hitbox goes off after it goes on");
            Assert.Less(colliderDelay, 1f, "The end must fire before the attack leaves its state, or the hitbox stays on");
        }

        [Test]
        public void Prefab_UsesTheController_AndTheNewArt()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(LightDevourerArtBuilder.PrefabPath);
            Assert.IsNotNull(prefab, $"Missing {LightDevourerArtBuilder.PrefabPath}");
            Assert.AreEqual(LightDevourerArtBuilder.ControllerPath, AssetDatabase.GetAssetPath(prefab.GetComponent<Animator>().runtimeAnimatorController));
            StringAssert.StartsWith(LightDevourerArtBuilder.SpriteFolder + "/LD", AssetDatabase.GetAssetPath(prefab.GetComponent<SpriteRenderer>().sprite));
        }

        [TestCase(0, 11, 0f)]
        [TestCase(8, 11, 8f / 11f)]
        [TestCase(3, 0, 0f)]
        public void NormalizedFrameTime_IsTheFramesStart(int frame, int frames, float expected)
        {
            Assert.AreEqual(expected, LightDevourerArtBuilder.NormalizedFrameTime(frame, frames), 1e-5f);
        }
    }
}
