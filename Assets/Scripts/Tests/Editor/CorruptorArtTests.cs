using System.Linq;
using Game.Enemies.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>
    /// The Corruptor's own art (built by Tools/Flames of Faith/Enemies/Build Corruptor Art) replaced the tinted
    /// Skull placeholder: its sheets are sliced into one row per direction, its clips play the right row, and
    /// the prefab uses its controller with no tint.
    /// </summary>
    public class CorruptorArtTests
    {
        [TestCase("CorruptorIdle_SS_20f_128x128.png", 20, 128, 128)]
        [TestCase("LDWalk_SS_8f_192x192", 8, 192, 192)]
        [TestCase("Wide_SS_4f_64x32.png", 4, 64, 32)]
        public void SheetName_GivesFramesAndCellSize(string fileName, int frames, int width, int height)
        {
            Assert.IsTrue(CorruptorArtBuilder.TryParseSheetName(fileName, out int f, out int w, out int h));
            Assert.AreEqual((frames, width, height), (f, w, h));
        }

        [TestCase("Corruptor.png")]
        [TestCase("Corruptor_SS_128x128.png")]
        [TestCase("Corruptor_SS_0f_128x128.png")]
        [TestCase(null)]
        public void SheetName_WithoutFramesOrSize_IsRejected(string fileName)
        {
            Assert.IsFalse(CorruptorArtBuilder.TryParseSheetName(fileName, out _, out _, out _));
        }

        [Test]
        public void SpriteName_CountsRowByRowFromTheTopLeft()
        {
            Assert.AreEqual("Sheet_0", CorruptorArtBuilder.SpriteName("Sheet.png", 0, 0, 8));
            Assert.AreEqual("Sheet_7", CorruptorArtBuilder.SpriteName("Sheet.png", 0, 7, 8));
            Assert.AreEqual("Sheet_8", CorruptorArtBuilder.SpriteName("Sheet.png", 1, 0, 8));
            Assert.AreEqual("Sheet_63", CorruptorArtBuilder.SpriteName("Sheet.png", 7, 7, 8));
        }

        [Test]
        public void Sheets_AreSlicedIntoOneCellPerFrameAndDirection()
        {
            foreach (var sheet in CorruptorArtBuilder.Sheets)
            {
                CorruptorArtBuilder.TryParseSheetName(sheet.FileName, out int frames, out int cellWidth, out int cellHeight);
                var importer = AssetImporter.GetAtPath(sheet.Path) as TextureImporter;
                Assert.IsNotNull(importer, $"Missing {sheet.Path}");
                Assert.AreEqual(SpriteImportMode.Multiple, importer.spriteImportMode, sheet.FileName);
                Assert.AreEqual(CorruptorArtBuilder.PixelsPerUnit, importer.spritePixelsPerUnit, $"{sheet.FileName}: enemy sheets are {CorruptorArtBuilder.PixelsPerUnit} PPU");
                importer.GetSourceTextureWidthAndHeight(out int width, out int height);
                Assert.GreaterOrEqual(importer.maxTextureSize, Mathf.Max(width, height), $"{sheet.FileName} is shrunk on import, the cells no longer line up");

                var sprites = AssetDatabase.LoadAllAssetsAtPath(sheet.Path).OfType<Sprite>().ToList();
                Assert.AreEqual(frames * CorruptorArtBuilder.Directions.Length, sprites.Count, $"{sheet.FileName}: {frames} frames x 8 directions");
                foreach (var sprite in sprites)
                {
                    Assert.AreEqual(new Vector2(cellWidth, cellHeight), sprite.rect.size, sprite.name);
                    Assert.AreEqual(0f, sprite.rect.x % cellWidth, sprite.name);
                    Assert.AreEqual(0f, sprite.rect.y % cellHeight, sprite.name);
                }
            }
        }

        [Test]
        public void Controller_HasAnEightDirectionTreePerSheet()
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(CorruptorArtBuilder.ControllerPath);
            Assert.IsNotNull(controller, $"Missing {CorruptorArtBuilder.ControllerPath}");
            var machine = controller.layers[0].stateMachine;
            Assert.AreEqual(CorruptorArtBuilder.IdleState, machine.defaultState?.name, "Idle is the default state");
            Assert.AreEqual(CorruptorArtBuilder.Sheets.Length, machine.states.Length, "One state per sheet");
            Assert.AreEqual(CorruptorArtBuilder.Sheets.Length,
                AssetDatabase.LoadAllAssetsAtPath(CorruptorArtBuilder.ControllerPath).OfType<BlendTree>().Count(),
                "Blend trees left behind by an earlier build");

            foreach (string parameter in new[] { "DirectionX", "DirectionY", "IsMoving", "Shoot" })
            {
                Assert.IsTrue(controller.parameters.Any(p => p.name == parameter), $"Missing parameter {parameter} (EnemyAnimationController sets it)");
            }

            foreach (var sheet in CorruptorArtBuilder.Sheets)
            {
                CorruptorArtBuilder.TryParseSheetName(sheet.FileName, out int frames, out _, out _);
                var state = machine.states.Select(s => s.state).FirstOrDefault(s => s.name == sheet.State);
                Assert.IsNotNull(state, $"No {sheet.State} state");
                var tree = state.motion as BlendTree;
                Assert.IsNotNull(tree, $"{sheet.State} isn't a blend tree");
                Assert.AreEqual(BlendTreeType.SimpleDirectional2D, tree.blendType);
                Assert.AreEqual(("DirectionX", "DirectionY"), (tree.blendParameter, tree.blendParameterY));
                Assert.AreEqual(8, tree.children.Length, $"{sheet.State}: 8 directions");
                Assert.AreEqual(8, tree.children.Select(c => c.position).Distinct().Count(), $"{sheet.State}: two clips share a direction");

                foreach (var child in tree.children)
                {
                    var clip = (AnimationClip)child.motion;
                    string direction = clip.name.Substring(clip.name.LastIndexOf('_') + 1);
                    int row = System.Array.IndexOf(CorruptorArtBuilder.Directions, direction);
                    Assert.GreaterOrEqual(row, 0, $"{clip.name}: unknown direction");
                    Assert.AreEqual(ExpectedPosition(direction), child.position, $"{clip.name} sits at the wrong blend position");
                    Assert.AreEqual(sheet.Loop, AnimationUtility.GetAnimationClipSettings(clip).loopTime, $"{clip.name} loop");

                    var binding = AnimationUtility.GetObjectReferenceCurveBindings(clip).Single();
                    var keys = AnimationUtility.GetObjectReferenceCurve(clip, binding);
                    Assert.AreEqual(frames, keys.Length, $"{clip.name}: one key per frame");
                    Assert.AreEqual(frames / CorruptorArtBuilder.FrameRate, clip.length, 1e-4f, $"{clip.name}: every frame shows for one frame time");
                    for (int frame = 0; frame < frames; frame++)
                    {
                        Assert.AreEqual(CorruptorArtBuilder.SpriteName(sheet.FileName, row, frame, frames), keys[frame].value.name,
                            $"{clip.name} frame {frame} shows another row or sheet");
                    }
                }
            }
        }

        // The shot leaves on the same frame in every direction, and the end event (on the last frame) lets the
        // Corruptor move again: without it, it would stand still until the shooter's safety timeout
        [Test]
        public void AttackClips_ShootAndEndOnTheSameFrames_AndOthersHaveNoEvents()
        {
            foreach (var sheet in CorruptorArtBuilder.Sheets)
            {
                CorruptorArtBuilder.TryParseSheetName(sheet.FileName, out int frames, out _, out _);
                foreach (string direction in CorruptorArtBuilder.Directions)
                {
                    var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(CorruptorArtBuilder.ClipPath(sheet.State, direction));
                    Assert.IsNotNull(clip, CorruptorArtBuilder.ClipPath(sheet.State, direction));
                    var events = AnimationUtility.GetAnimationEvents(clip);
                    if (!sheet.Shoots)
                    {
                        Assert.IsEmpty(events, $"{clip.name} has events");
                        continue;
                    }
                    Assert.AreEqual(2, events.Length, $"{clip.name}: one shot, one end");
                    Assert.AreEqual(CorruptorArtBuilder.ShootStartEvent, events[0].functionName, clip.name);
                    Assert.AreEqual(CorruptorArtBuilder.AttackEventFrame / CorruptorArtBuilder.FrameRate, events[0].time, 1e-4f, clip.name);
                    Assert.AreEqual(CorruptorArtBuilder.ShootEndEvent, events[1].functionName, clip.name);
                    Assert.AreEqual((frames - 1) / CorruptorArtBuilder.FrameRate, events[1].time, 1e-4f, $"{clip.name}: the end is on the last frame");
                    Assert.Less(events[1].time, clip.length, $"{clip.name}: an end event past the clip never fires");
                }
            }
        }

        [Test]
        public void Prefab_UsesTheCorruptorArt_WithoutTint()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CorruptorArtBuilder.PrefabPath);
            Assert.IsNotNull(prefab, $"Missing {CorruptorArtBuilder.PrefabPath}");
            var animator = prefab.GetComponent<Animator>();
            Assert.AreEqual(CorruptorArtBuilder.ControllerPath, AssetDatabase.GetAssetPath(animator.runtimeAnimatorController));

            var renderer = prefab.GetComponent<SpriteRenderer>();
            Assert.AreEqual(Color.white, renderer.color, "The placeholder's dark tint is back");
            Assert.IsFalse(new SerializedObject(renderer).FindProperty("m_Color").prefabOverride, "The variant overrides the base color");
            StringAssert.StartsWith(CorruptorArtBuilder.SpriteFolder, AssetDatabase.GetAssetPath(renderer.sprite), "The prefab still shows the placeholder sprite");
        }

        private static Vector2 ExpectedPosition(string direction)
        {
            float x = direction.Contains("e") ? 1 : direction.Contains("w") ? -1 : 0;
            float y = direction.Contains("n") ? 1 : direction.Contains("s") ? -1 : 0;
            return new Vector2(x, y);
        }
    }
}
