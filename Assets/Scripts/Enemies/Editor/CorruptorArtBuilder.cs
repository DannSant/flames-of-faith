using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Game.Enemies.Editor
{
    /// <summary>
    /// The Corruptor's art, built by the shared <see cref="EnemySheetArtBuilder"/>: slices its sheets, makes the
    /// 8-direction clips and the animator controller, and wires both into EnemyCorruptor.prefab. It can be run
    /// again after the art changes; a rerun with nothing new leaves every file as it was.
    /// </summary>
    public static class CorruptorArtBuilder
    {
        public const string SpriteFolder = "Assets/Sprites/Enemies/Corruptor";
        public const string AnimationFolder = "Assets/Animation/Enemies/Corruptor";
        public const string ControllerPath = AnimationFolder + "/Corruptor.controller";
        public const string PrefabPath = "Assets/Prefabs/Enemies/EnemyCorruptor.prefab";

        public const int PixelsPerUnit = EnemySheetArtBuilder.PixelsPerUnit;
        public const float FrameRate = EnemySheetArtBuilder.FrameRate;
        // The figure sits low in its 128px cell (feet 16px above the bottom, head 27px below the top), so the
        // pivot is raised to its middle. That keeps it centered on the transform like the Skull it replaces, and
        // the variant's health bar, stun point and collider offsets still fit.
        public static readonly Vector2 Pivot = new Vector2(0.5f, 0.46f);
        // The mouth opens widest on the 4th attack frame: the projectile leaves there (AIBehaviorShooter)
        public const int AttackEventFrame = 3;
        // The attack ends on its last frame: the Corruptor holds still until then (AIBehaviorShooter.holdStillWhileShooting)
        public const string ShootStartEvent = "OnAnimationEventStart";
        public const string ShootEndEvent = "OnAnimationEventEnd";

        public const string IdleState = EnemySheetArtBuilder.IdleState;
        public const string WalkState = EnemySheetArtBuilder.WalkState;
        public const string AttackState = EnemySheetArtBuilder.AttackState;

        public static string[] Directions => EnemySheetArtBuilder.Directions;

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

        public static bool TryParseSheetName(string fileName, out int frames, out int cellWidth, out int cellHeight) =>
            EnemySheetArtBuilder.TryParseSheetName(fileName, out frames, out cellWidth, out cellHeight);

        public static string SpriteName(string fileName, int row, int column, int columns) =>
            EnemySheetArtBuilder.SpriteName(fileName, row, column, columns);

        public static string ClipPath(string state, string direction) => Definition.ClipPath(state, direction);

        public static EnemyArtDefinition Definition => new EnemyArtDefinition
        {
            Name = "Corruptor",
            SpriteFolder = SpriteFolder,
            AnimationFolder = AnimationFolder,
            ControllerPath = ControllerPath,
            PrefabPath = PrefabPath,
            Pivot = Pivot,
            Sheets = Sheets.Select(s => new EnemySheet(s.State, s.FileName, s.Loop, s.Shoots
                ? new[]
                {
                    new ClipEvent(AttackEventFrame, ShootStartEvent, "ShootProjectile"),
                    new ClipEvent(ClipEvent.LastFrame, ShootEndEvent, "ShootProjectile"),
                }
                : new ClipEvent[0])).ToArray(),
            AttackTrigger = "Shoot",
            ConfigurePrefab = RevertPlaceholderTint,
        };

        [MenuItem("Tools/Flames of Faith/Enemies/Build Corruptor Art")]
        public static void Build() => EnemySheetArtBuilder.Build(Definition);

        /// <summary>Batchmode entry (-executeMethod Game.Enemies.Editor.CorruptorArtBuilder.RunBatch).</summary>
        public static void RunBatch() => Build();

        // The placeholder art was tinted dark to tell the Corruptor apart: back to the base white
        private static void RevertPlaceholderTint(GameObject root)
        {
            var color = new SerializedObject(root.GetComponent<SpriteRenderer>()).FindProperty("m_Color");
            if (color.prefabOverride)
            {
                PrefabUtility.RevertPropertyOverride(color, InteractionMode.AutomatedAction);
            }
        }
    }
}
