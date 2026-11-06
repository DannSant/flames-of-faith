using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace Game.Enemies.Editor
{
    /// <summary>An animation event placed on a frame of every direction's clip of a sheet.</summary>
    public readonly struct ClipEvent
    {
        /// <summary>Use as <see cref="Frame"/> for the sheet's last frame, whatever its frame count.</summary>
        public const int LastFrame = -1;

        public readonly int Frame;
        public readonly string FunctionName;
        public readonly string StringParameter;

        public ClipEvent(int frame, string functionName, string stringParameter = "")
        {
            Frame = frame;
            FunctionName = functionName;
            StringParameter = stringParameter ?? string.Empty;
        }

        /// <summary>The 0-based frame the event sits on, for a sheet of <paramref name="frames"/> frames.</summary>
        public int ResolveFrame(int frames) => Frame == LastFrame ? frames - 1 : Frame;
    }

    /// <summary>One sheet of an enemy: one action (an animator state), one row per direction.</summary>
    public sealed class EnemySheet
    {
        public readonly string State;
        public readonly string FileName;
        public readonly bool Loop;
        public readonly ClipEvent[] Events;

        public EnemySheet(string state, string fileName, bool loop, params ClipEvent[] events)
        {
            State = state;
            FileName = fileName;
            Loop = loop;
            Events = events ?? Array.Empty<ClipEvent>();
        }
    }

    /// <summary>Everything that differs between two enemies' art: where it lives, its sheets, and how its attack is wired.</summary>
    public sealed class EnemyArtDefinition
    {
        public string Name;
        public string SpriteFolder;
        public string AnimationFolder;
        public string ControllerPath;
        public string PrefabPath;
        /// <summary>The sprites' pivot, normalized in the cell: it decides where the figure stands on the transform.</summary>
        public Vector2 Pivot = new Vector2(0.5f, 0.5f);
        /// <summary>Lowest Max Size the sheets are imported at (raised further if a sheet is wider).</summary>
        public int MinMaxTextureSize = 2048;
        /// <summary>One sheet per action: Idle, Walk and Attack (<see cref="EnemySheetArtBuilder"/>'s state names).</summary>
        public EnemySheet[] Sheets;
        /// <summary>The trigger EnemyAnimationController sets to start the attack ("Shoot" or "Attack").</summary>
        public string AttackTrigger;
        /// <summary>Adds the attack state's behaviours (e.g. MeleeAnimationAttackBehavior) and any parameter they need. Optional.</summary>
        public Action<AnimatorController, AnimatorState> ConfigureAttackState;
        /// <summary>Extra prefab changes, made before the controller and first frame are set. Optional.</summary>
        public Action<GameObject> ConfigurePrefab;

        public string SheetPath(EnemySheet sheet) => SpriteFolder + "/" + sheet.FileName;
        public string ClipPath(string state, string direction) => $"{AnimationFolder}/{state}_{direction}.anim";
    }

    /// <summary>
    /// Builds an enemy's art from its sprite sheets: slices them, makes the 8-direction clips and the animator
    /// controller, and wires both into the prefab. Shared by every redrawn enemy (the Corruptor, the Light
    /// Devourer...); each one only describes itself in an <see cref="EnemyArtDefinition"/>.
    ///
    /// It can be run again after the art changes. Sprite ids, clips and the controller's GUID survive a rerun, and
    /// a rerun with nothing new leaves every file as it was: the controller is built in a scratch copy first and the
    /// real one is only emptied and refilled when something differs (a rebuild would give every state new file ids).
    ///
    /// Sheets: one row per direction (S, SW, W, NW, N, NE, E, SE top to bottom), one column per frame, and the file
    /// name gives both, "Name_SS_&lt;frames&gt;f_&lt;width&gt;x&lt;height&gt;".
    /// </summary>
    public static class EnemySheetArtBuilder
    {
        // Every enemy sheet is imported at 64 pixels per unit, whatever its cell size
        public const int PixelsPerUnit = 64;
        public const float FrameRate = 12f;

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

        /// <summary>The Max Size a sheet needs so it isn't shrunk on import.</summary>
        public static int RequiredMaxTextureSize(int width, int height, int minimum) =>
            Mathf.Max(minimum, Mathf.NextPowerOfTwo(Mathf.Max(width, height)));

        /// <returns>True when the controller had to be rebuilt.</returns>
        public static bool Build(EnemyArtDefinition definition)
        {
            var clips = new Dictionary<string, Dictionary<string, AnimationClip>>();
            foreach (var sheet in definition.Sheets)
            {
                int frames = SliceSheet(definition, sheet);
                clips[sheet.State] = BuildClips(definition, sheet, frames);
            }

            var (controller, rebuilt) = BuildController(definition, clips);
            WirePrefab(definition, controller);
            AssetDatabase.SaveAssets();
            Debug.Log($"[EnemySheetArtBuilder] {definition.Name}: built {clips.Values.Sum(c => c.Count)} clips, " +
                $"{definition.ControllerPath} ({(rebuilt ? "rebuilt" : "unchanged")}) and wired {definition.PrefabPath}.");
            return rebuilt;
        }

        private static int SliceSheet(EnemyArtDefinition definition, EnemySheet sheet)
        {
            string path = definition.SheetPath(sheet);
            if (!TryParseSheetName(sheet.FileName, out int frames, out int cellWidth, out int cellHeight))
            {
                throw new InvalidOperationException($"{sheet.FileName}: the name doesn't end in _<frames>f_<width>x<height>");
            }

            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new InvalidOperationException($"No texture at {path}");

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
            // A sheet wider than the cap is shrunk on import: every frame goes blurry and the slicing breaks
            int maxSize = RequiredMaxTextureSize(width, height, definition.MinMaxTextureSize);
            importer.maxTextureSize = maxSize;
            // A platform override would cap it again
            foreach (string platform in new[] { "Standalone" })
            {
                var platformSettings = importer.GetPlatformTextureSettings(platform);
                if (platformSettings.overridden && platformSettings.maxTextureSize < maxSize)
                {
                    platformSettings.maxTextureSize = maxSize;
                    importer.SetPlatformTextureSettings(platformSettings);
                }
            }
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
                        pivot = definition.Pivot,
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

        private static Dictionary<string, AnimationClip> BuildClips(EnemyArtDefinition definition, EnemySheet sheet, int frames)
        {
            string sheetPath = definition.SheetPath(sheet);
            var sprites = AssetDatabase.LoadAllAssetsAtPath(sheetPath).OfType<Sprite>().ToDictionary(s => s.name);
            EnsureFolder(definition.AnimationFolder);

            var clips = new Dictionary<string, AnimationClip>();
            var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
            for (int row = 0; row < Directions.Length; row++)
            {
                string direction = Directions[row];
                string path = definition.ClipPath(sheet.State, direction);
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

                var events = sheet.Events
                    .Select(e => new AnimationEvent { time = e.ResolveFrame(frames) / FrameRate, functionName = e.FunctionName, stringParameter = e.StringParameter })
                    .ToArray();
                AnimationUtility.SetAnimationEvents(clip, events);

                if (isNew) AssetDatabase.CreateAsset(clip, path);
                else EditorUtility.SetDirty(clip);
                clips[direction] = clip;
            }
            return clips;
        }

        private static (AnimatorController controller, bool rebuilt) BuildController(EnemyArtDefinition definition,
            Dictionary<string, Dictionary<string, AnimationClip>> clips)
        {
            var existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(definition.ControllerPath);
            if (existing != null)
            {
                // Build a scratch copy and compare: refilling the real one gives every state, tree and transition a
                // new file id, so a rerun with nothing new would still rewrite the whole file
                string scratchPath = AssetDatabase.GenerateUniqueAssetPath($"{definition.AnimationFolder}/_{definition.Name}_scratch.controller");
                try
                {
                    var scratch = AnimatorController.CreateAnimatorControllerAtPath(scratchPath);
                    FillController(definition, scratch, scratchPath, clips);
                    if (Describe(scratch) == Describe(existing)) return (existing, false);
                }
                finally
                {
                    AssetDatabase.DeleteAsset(scratchPath);
                }
            }

            // Emptied and refilled rather than recreated, so the prefab's reference (the asset's GUID) survives a re-run
            var controller = existing ?? AnimatorController.CreateAnimatorControllerAtPath(definition.ControllerPath);
            FillController(definition, controller, definition.ControllerPath, clips);
            EditorUtility.SetDirty(controller);
            return (controller, true);
        }

        private static void FillController(EnemyArtDefinition definition, AnimatorController controller, string path,
            Dictionary<string, Dictionary<string, AnimationClip>> clips)
        {
            ClearController(controller, path);
            // The parameters EnemyAnimationController drives
            controller.AddParameter("DirectionX", AnimatorControllerParameterType.Float);
            controller.AddParameter("DirectionY", AnimatorControllerParameterType.Float);
            controller.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);
            controller.AddParameter(definition.AttackTrigger, AnimatorControllerParameterType.Trigger);

            var idle = AddDirectionalState(controller, IdleState, clips[IdleState]);
            var walk = AddDirectionalState(controller, WalkState, clips[WalkState]);
            var attack = AddDirectionalState(controller, AttackState, clips[AttackState]);
            definition.ConfigureAttackState?.Invoke(controller, attack);
            var machine = controller.layers[0].stateMachine;
            machine.defaultState = idle;
            PlaceState(machine, idle, new Vector3(30, 240));
            PlaceState(machine, walk, new Vector3(330, 120));
            PlaceState(machine, attack, new Vector3(330, 360));

            // Idle and walk follow IsMoving, and an attack plays to its end from either.
            // Instant transitions: a crossfade between sprite clips only delays the switch.
            AddTransition(idle, walk, AnimatorConditionMode.If, "IsMoving", false);
            AddTransition(walk, idle, AnimatorConditionMode.IfNot, "IsMoving", false);
            AddTransition(idle, attack, AnimatorConditionMode.If, definition.AttackTrigger, false);
            AddTransition(walk, attack, AnimatorConditionMode.If, definition.AttackTrigger, false);
            AddTransition(attack, walk, AnimatorConditionMode.If, "IsMoving", true);
            AddTransition(attack, idle, AnimatorConditionMode.IfNot, "IsMoving", true);
        }

        private static void ClearController(AnimatorController controller, string path)
        {
            var machine = controller.layers[0].stateMachine;
            foreach (var child in machine.states)
            {
                machine.RemoveState(child.state);
            }
            foreach (var transition in machine.anyStateTransitions)
            {
                machine.RemoveAnyStateTransition(transition);
            }
            foreach (var transition in machine.entryTransitions)
            {
                machine.RemoveEntryTransition(transition);
            }
            // Removing a state leaves its blend tree (and an old hand-made controller's strays) behind as sub-assets
            foreach (var tree in AssetDatabase.LoadAllAssetsAtPath(path).OfType<BlendTree>())
            {
                AssetDatabase.RemoveObjectFromAsset(tree);
                UnityEngine.Object.DestroyImmediate(tree, true);
            }
            controller.parameters = Array.Empty<AnimatorControllerParameter>();
        }

        /// <summary>
        /// Everything about a controller that the game or the builder cares about, as text: parameters, states and
        /// their positions, blend trees, behaviours and transitions. Two controllers with the same text are the same.
        /// </summary>
        public static string Describe(AnimatorController controller)
        {
            var text = new StringBuilder();
            foreach (var p in controller.parameters)
            {
                text.AppendLine($"param {p.name} {p.type} {p.defaultFloat} {p.defaultInt} {p.defaultBool}");
            }
            text.AppendLine($"layers {controller.layers.Length}");
            var machine = controller.layers[0].stateMachine;
            text.AppendLine($"default {machine.defaultState?.name}");
            text.AppendLine($"submachines {machine.stateMachines.Length}");
            foreach (var transition in machine.anyStateTransitions) text.AppendLine("any " + DescribeTransition(transition));
            foreach (var transition in machine.entryTransitions) text.AppendLine($"entry {transition.destinationState?.name}");
            foreach (var child in machine.states.OrderBy(s => s.state.name, StringComparer.Ordinal))
            {
                var state = child.state;
                text.AppendLine($"state {state.name} at {child.position} speed {state.speed} " +
                    $"{state.speedParameterActive}:{state.speedParameter} wd {state.writeDefaultValues} tag {state.tag}");
                DescribeMotion(text, state.motion, "  ");
                foreach (var behaviour in state.behaviours)
                {
                    text.AppendLine($"  behaviour {behaviour.GetType().FullName} {EditorJsonUtility.ToJson(behaviour)}");
                }
                foreach (var transition in state.transitions) text.AppendLine("  " + DescribeTransition(transition));
            }
            text.AppendLine($"trees {AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(controller)).OfType<BlendTree>().Count()}");
            return text.ToString();
        }

        private static void DescribeMotion(StringBuilder text, Motion motion, string indent)
        {
            switch (motion)
            {
                case BlendTree tree:
                    text.AppendLine($"{indent}tree {tree.name} {tree.blendType} {tree.blendParameter} {tree.blendParameterY}");
                    foreach (var child in tree.children)
                    {
                        text.AppendLine($"{indent}  child at {child.position} x{child.timeScale} {child.threshold}");
                        DescribeMotion(text, child.motion, indent + "    ");
                    }
                    break;
                case AnimationClip clip:
                    text.AppendLine($"{indent}clip {AssetDatabase.GetAssetPath(clip)}");
                    break;
                default:
                    text.AppendLine($"{indent}motion {(motion == null ? "none" : motion.name)}");
                    break;
            }
        }

        private static string DescribeTransition(AnimatorStateTransition transition) =>
            $"-> {transition.destinationState?.name} exit {transition.hasExitTime}:{transition.exitTime} " +
            $"duration {transition.hasFixedDuration}:{transition.duration} offset {transition.offset} " +
            $"interrupt {transition.interruptionSource} " +
            string.Join(",", transition.conditions.Select(c => $"{c.mode} {c.parameter} {c.threshold}"));

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

        private static void WirePrefab(EnemyArtDefinition definition, AnimatorController controller)
        {
            // The prefab shows the first idle frame facing the camera until the animator takes over
            var idleSheet = definition.Sheets.First(s => s.State == IdleState);
            TryParseSheetName(idleSheet.FileName, out int idleFrames, out _, out _);
            string firstIdleName = SpriteName(idleSheet.FileName, Array.IndexOf(Directions, "s"), 0, idleFrames);
            var firstIdle = AssetDatabase.LoadAllAssetsAtPath(definition.SheetPath(idleSheet)).OfType<Sprite>().First(s => s.name == firstIdleName);

            var root = PrefabUtility.LoadPrefabContents(definition.PrefabPath);
            try
            {
                var animator = root.GetComponent<Animator>();
                var renderer = root.GetComponent<SpriteRenderer>();
                if (animator == null || renderer == null) throw new InvalidOperationException($"{definition.PrefabPath} has no Animator or SpriteRenderer on its root");

                // Before the controller and sprite: a revert there reloads the instance from its recorded overrides
                definition.ConfigurePrefab?.Invoke(root);

                animator.runtimeAnimatorController = controller;
                renderer.sprite = firstIdle;
                PrefabUtility.SaveAsPrefabAsset(root, definition.PrefabPath);
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
