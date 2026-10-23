using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace Game.Enemies.Editor
{
    /// <summary>
    /// Builds the Corruptor's art from its sprite sheets: slices them, makes the 8-direction clips and the
    /// animator controller, and wires both into EnemyCorruptor.prefab. It can be run again after the art
    /// changes: it re-slices, rewrites the clips in place and rebuilds the controller.
    ///
    /// Sheets follow the other enemies' layout (Skull, DrekoEye): one row per direction, one column per
    /// frame, and the file name gives both, "Name_SS_&lt;frames&gt;f_&lt;width&gt;x&lt;height&gt;".
    /// </summary>
    public static class CorruptorArtBuilder
    {
        public const string SpriteFolder = "Assets/Sprites/Enemies/Corruptor";
        public const string AnimationFolder = "Assets/Animation/Enemies/Corruptor";
        public const string ControllerPath = AnimationFolder + "/Corruptor.controller";
        public const string PrefabPath = "Assets/Prefabs/Enemies/EnemyCorruptor.prefab";

        // Every enemy sheet is imported at 64 pixels per unit, whatever its cell size
        public const int PixelsPerUnit = 64;
        public const float FrameRate = 12f;
        // The figure sits low in its 128px cell (feet 16px above the bottom, head 27px below the top), so the
        // pivot is raised to its middle. That keeps it centered on the transform like the Skull it replaces, and
        // the variant's health bar, stun point and collider offsets still fit.
        public static readonly Vector2 Pivot = new Vector2(0.5f, 0.46f);
        // The mouth opens widest on the 4th attack frame: the projectile leaves there (AIBehaviorShooter)
        public const int AttackEventFrame = 3;
        // The attack ends on its last frame: the Corruptor holds still until then (AIBehaviorShooter.holdStillWhileShooting)
        public const string ShootStartEvent = "OnAnimationEventStart";
        public const string ShootEndEvent = "OnAnimationEventEnd";

        public const string IdleState = "Idle";
        public const string WalkState = "Walk";
        public const string AttackState = "Attack";

        // Rows of the sheets, top to bottom, as in the Skull and FlameShooter sheets
        public static readonly string[] Directions = { "s", "sw", "w", "nw", "n", "ne", "e", "se" };

        private static readonly Dictionary<string, Vector2> BlendPositions = new Dictionary<string, Vector2>
        {
            { "n", new Vector2(0, 1) }, { "ne", new Vector2(1, 1) }, { "e", new Vector2(1, 0) }, { "se", new Vector2(1, -1) },
            { "s", new Vector2(0, -1) }, { "sw", new Vector2(-1, -1) }, { "w", new Vector2(-1, 0) }, { "nw", new Vector2(-1, 1) },
        };

        public readonly struct SheetInfo
        {
            public readonly string State;
            public readonly string FileName;
            public readonly bool Loop;
            // Shot clips: the projectile leaves on AttackEventFrame and the attack ends on the last frame
            public readonly bool Shoots;

            public SheetInfo(string state, string fileName, bool loop, bool shoots = false)
            {
                State = state;
                FileName = fileName;
                Loop = loop;
                Shoots = shoots;
            }

            public string Path => SpriteFolder + "/" + FileName;
        }

        public static readonly SheetInfo[] Sheets =
        {
            new SheetInfo(IdleState, "CorruptorIdle_SS_20f_128x128.png", true),
            new SheetInfo(WalkState, "CorruptorWalk_SS_8f_128x128.png", true),
            new SheetInfo(AttackState, "CorruptorAttack_SS_8f_128x128.png", false, shoots: true),
        };

        private static readonly Regex SheetNamePattern = new Regex(@"_(\d+)f_(\d+)x(\d+)$");

        /// <summary>Reads the frame count and cell size from a sheet name like "CorruptorIdle_SS_20f_128x128".</summary>
        public static bool TryParseSheetName(string fileName, out int frames, out int cellWidth, out int cellHeight)
        {
            frames = cellWidth = cellHeight = 0;
            var match = SheetNamePattern.Match(System.IO.Path.GetFileNameWithoutExtension(fileName ?? string.Empty));
            if (!match.Success) return false;
            frames = int.Parse(match.Groups[1].Value);
            cellWidth = int.Parse(match.Groups[2].Value);
            cellHeight = int.Parse(match.Groups[3].Value);
            return frames > 0 && cellWidth > 0 && cellHeight > 0;
        }

        /// <summary>Unity's grid slicing name for a cell: numbered row by row from the top left.</summary>
        public static string SpriteName(string fileName, int row, int column, int columns) =>
            $"{System.IO.Path.GetFileNameWithoutExtension(fileName)}_{row * columns + column}";

        public static string ClipPath(string state, string direction) => $"{AnimationFolder}/{state}_{direction}.anim";

        [MenuItem("Tools/Flames of Faith/Enemies/Build Corruptor Art")]
        public static void Build()
        {
            var clips = new Dictionary<string, Dictionary<string, AnimationClip>>();
            foreach (var sheet in Sheets)
            {
                int frames = SliceSheet(sheet);
                clips[sheet.State] = BuildClips(sheet, frames);
            }

            var controller = BuildController(clips);
            WirePrefab(controller);
            AssetDatabase.SaveAssets();
            Debug.Log($"[CorruptorArtBuilder] Built {clips.Values.Sum(c => c.Count)} clips, {ControllerPath} and wired {PrefabPath}.");
        }

        /// <summary>Batchmode entry (-executeMethod Game.Enemies.Editor.CorruptorArtBuilder.RunBatch).</summary>
        public static void RunBatch() => Build();

        private static int SliceSheet(SheetInfo sheet)
        {
            if (!TryParseSheetName(sheet.FileName, out int frames, out int cellWidth, out int cellHeight))
            {
                throw new InvalidOperationException($"{sheet.FileName}: the name doesn't end in _<frames>f_<width>x<height>");
            }

            var importer = AssetImporter.GetAtPath(sheet.Path) as TextureImporter;
            if (importer == null) throw new InvalidOperationException($"No texture at {sheet.Path}");

            importer.GetSourceTextureWidthAndHeight(out int width, out int height);
            int columns = width / cellWidth;
            int rows = height / cellHeight;
            if (columns != frames || rows != Directions.Length || width % cellWidth != 0 || height % cellHeight != 0)
            {
                throw new InvalidOperationException($"{sheet.FileName} is {width}x{height}: expected {frames} columns and " +
                    $"{Directions.Length} rows of {cellWidth}x{cellHeight}");
            }

            // Same import settings as the other enemy sheets
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = PixelsPerUnit;
            importer.filterMode = FilterMode.Bilinear;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Compressed;
            // The idle sheet is 2560 wide: the default 2048 cap would shrink it and break the slicing
            importer.maxTextureSize = Mathf.Max(2048, Mathf.NextPowerOfTwo(Mathf.Max(width, height)));
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            // Full rect leaves room for the Corrupted outline drawn around the silhouette
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteExtrude = 1;
            importer.SetTextureSettings(settings);

            var factory = new SpriteDataProviderFactories();
            factory.Init();
            var provider = factory.GetSpriteEditorDataProviderFromObject(importer);
            provider.InitSpriteEditorDataProvider();

            // Keep the ids of sprites that already exist, so a re-run doesn't break references to them
            var existing = provider.GetSpriteRects().ToDictionary(r => r.name, r => r.spriteID);
            var rects = new List<SpriteRect>();
            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    string name = SpriteName(sheet.FileName, row, column, columns);
                    rects.Add(new SpriteRect
                    {
                        name = name,
                        spriteID = existing.TryGetValue(name, out var id) ? id : GUID.Generate(),
                        rect = new Rect(column * cellWidth, height - (row + 1) * cellHeight, cellWidth, cellHeight),
                        alignment = SpriteAlignment.Custom,
                        pivot = Pivot,
                    });
                }
            }
            provider.SetSpriteRects(rects.ToArray());
            var names = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            names?.SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)).ToList());
            provider.Apply();
            importer.SaveAndReimport();
            return frames;
        }

        private static Dictionary<string, AnimationClip> BuildClips(SheetInfo sheet, int frames)
        {
            var sprites = AssetDatabase.LoadAllAssetsAtPath(sheet.Path).OfType<Sprite>().ToDictionary(s => s.name);
            EnsureFolder(AnimationFolder);

            var clips = new Dictionary<string, AnimationClip>();
            var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
            for (int row = 0; row < Directions.Length; row++)
            {
                string direction = Directions[row];
                string path = ClipPath(sheet.State, direction);
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                bool isNew = clip == null;
                if (isNew) clip = new AnimationClip();
                clip.name = $"{sheet.State}_{direction}";
                clip.frameRate = FrameRate;

                // One key per frame. Unity makes a sprite clip one frame longer than its last key, so the last frame
                // gets its full time on screen without an extra key (one made it play twice).
                var keys = new ObjectReferenceKeyframe[frames];
                for (int frame = 0; frame < frames; frame++)
                {
                    string name = SpriteName(sheet.FileName, row, frame, frames);
                    if (!sprites.TryGetValue(name, out var sprite)) throw new InvalidOperationException($"Missing sprite {name}");
                    keys[frame] = new ObjectReferenceKeyframe { time = frame / FrameRate, value = sprite };
                }
                AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

                var clipSettings = AnimationUtility.GetAnimationClipSettings(clip);
                clipSettings.loopTime = sheet.Loop;
                AnimationUtility.SetAnimationClipSettings(clip, clipSettings);

                var events = sheet.Shoots
                    ? new[]
                    {
                        new AnimationEvent { time = AttackEventFrame / FrameRate, functionName = ShootStartEvent, stringParameter = "ShootProjectile" },
                        new AnimationEvent { time = (frames - 1) / FrameRate, functionName = ShootEndEvent, stringParameter = "ShootProjectile" },
                    }
                    : Array.Empty<AnimationEvent>();
                AnimationUtility.SetAnimationEvents(clip, events);

                if (isNew) AssetDatabase.CreateAsset(clip, path);
                else EditorUtility.SetDirty(clip);
                clips[direction] = clip;
            }
            return clips;
        }

        private static AnimatorController BuildController(Dictionary<string, Dictionary<string, AnimationClip>> clips)
        {
            // Emptied and refilled rather than recreated, so the prefab's reference (the asset's GUID) survives a re-run
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath)
                ?? AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            ClearController(controller);
            // The parameters EnemyAnimationController drives
            controller.AddParameter("DirectionX", AnimatorControllerParameterType.Float);
            controller.AddParameter("DirectionY", AnimatorControllerParameterType.Float);
            controller.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Shoot", AnimatorControllerParameterType.Trigger);

            var idle = AddDirectionalState(controller, IdleState, clips[IdleState]);
            var walk = AddDirectionalState(controller, WalkState, clips[WalkState]);
            var attack = AddDirectionalState(controller, AttackState, clips[AttackState]);
            var machine = controller.layers[0].stateMachine;
            machine.defaultState = idle;
            PlaceState(machine, idle, new Vector3(30, 240));
            PlaceState(machine, walk, new Vector3(330, 120));
            PlaceState(machine, attack, new Vector3(330, 360));

            // Like DrekoEye: idle and walk follow IsMoving, and a shot plays to its end from either.
            // Instant transitions: a crossfade between sprite clips only delays the switch.
            AddTransition(idle, walk, AnimatorConditionMode.If, "IsMoving", false);
            AddTransition(walk, idle, AnimatorConditionMode.IfNot, "IsMoving", false);
            AddTransition(idle, attack, AnimatorConditionMode.If, "Shoot", false);
            AddTransition(walk, attack, AnimatorConditionMode.If, "Shoot", false);
            AddTransition(attack, walk, AnimatorConditionMode.If, "IsMoving", true);
            AddTransition(attack, idle, AnimatorConditionMode.IfNot, "IsMoving", true);
            return controller;
        }

        private static void ClearController(AnimatorController controller)
        {
            var machine = controller.layers[0].stateMachine;
            foreach (var child in machine.states)
            {
                machine.RemoveState(child.state);
            }
            // Removing a state leaves its blend tree behind as a sub-asset
            foreach (var tree in AssetDatabase.LoadAllAssetsAtPath(ControllerPath).OfType<BlendTree>())
            {
                AssetDatabase.RemoveObjectFromAsset(tree);
                UnityEngine.Object.DestroyImmediate(tree, true);
            }
            controller.parameters = Array.Empty<AnimatorControllerParameter>();
        }

        private static AnimatorState AddDirectionalState(AnimatorController controller, string name, Dictionary<string, AnimationClip> clips)
        {
            var state = controller.CreateBlendTreeInController(name, out BlendTree tree, 0);
            tree.name = name;
            tree.blendType = BlendTreeType.SimpleDirectional2D;
            tree.blendParameter = "DirectionX";
            tree.blendParameterY = "DirectionY";
            foreach (string direction in new[] { "n", "ne", "e", "se", "s", "sw", "w", "nw" })
            {
                tree.AddChild(clips[direction], BlendPositions[direction]);
            }
            return state;
        }

        private static void PlaceState(AnimatorStateMachine machine, AnimatorState state, Vector3 position)
        {
            var states = machine.states;
            for (int i = 0; i < states.Length; i++)
            {
                if (states[i].state == state) states[i].position = position;
            }
            machine.states = states;
        }

        private static void AddTransition(AnimatorState from, AnimatorState to, AnimatorConditionMode mode, string parameter, bool atClipEnd)
        {
            var transition = from.AddTransition(to);
            transition.hasExitTime = atClipEnd;
            transition.exitTime = 1f;
            transition.hasFixedDuration = true;
            transition.duration = 0f;
            transition.AddCondition(mode, 0f, parameter);
        }

        private static void WirePrefab(AnimatorController controller)
        {
            // The prefab shows the first idle frame facing the camera until the animator takes over
            var idleSheet = Sheets.First(s => s.State == IdleState);
            TryParseSheetName(idleSheet.FileName, out int idleFrames, out _, out _);
            string firstIdleName = SpriteName(idleSheet.FileName, Array.IndexOf(Directions, "s"), 0, idleFrames);
            var firstIdle = AssetDatabase.LoadAllAssetsAtPath(idleSheet.Path).OfType<Sprite>().First(s => s.name == firstIdleName);

            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                var animator = root.GetComponent<Animator>();
                var renderer = root.GetComponent<SpriteRenderer>();
                if (animator == null || renderer == null) throw new InvalidOperationException($"{PrefabPath} has no Animator or SpriteRenderer on its root");

                // The placeholder art was tinted dark to tell the Corruptor apart: back to the base white.
                // Revert first: a revert reloads the instance from its recorded overrides, dropping changes made before it.
                var color = new SerializedObject(renderer).FindProperty("m_Color");
                if (color.prefabOverride)
                {
                    PrefabUtility.RevertPropertyOverride(color, InteractionMode.AutomatedAction);
                }

                animator.runtimeAnimatorController = controller;
                renderer.sprite = firstIdle;
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            string parent = System.IO.Path.GetDirectoryName(folder).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(folder));
        }
    }
}
