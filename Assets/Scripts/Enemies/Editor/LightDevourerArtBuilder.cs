using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Game.Enemies.Editor
{
    /// <summary>
    /// The Light Devourer's art (the art team's redraw, Nov 2026), built by the shared <see cref="EnemySheetArtBuilder"/>.
    /// Its attack is a melee swing: the hit and its end come from the attack state's MeleeAnimationAttackBehavior
    /// (AIMeleeAttacker), not from clip events.
    /// </summary>
    public static class LightDevourerArtBuilder
    {
        public const string SpriteFolder = "Assets/Sprites/Enemies/LightDevourer";
        public const string AnimationFolder = "Assets/Animation/Enemies/LightDevourer";
        public const string ControllerPath = AnimationFolder + "/LightDevourer.controller";
        public const string PrefabPath = "Assets/Prefabs/Enemies/EnemyLightDevourer.prefab";

        // The feet sit 168-185px down the 192px cell, where the old art's did, so the centered pivot keeps the
        // figure on the base prefab's collider and shadow
        public static readonly Vector2 Pivot = new Vector2(0.5f, 0.5f);

        // The blow lands on the 9th attack frame (chosen by the owner, Nov 2026): the slash at its widest
        public const int AttackHitFrame = 8;
        // The hitbox stays on through the slash (frames 9 and 10) and goes off as the recovery frame starts
        public const int AttackEndFrame = 10;
        public const string AttackTrigger = "Attack";

        public static readonly EnemySheet[] Sheets =
        {
            new EnemySheet(EnemySheetArtBuilder.IdleState, "LDIdle_SS_16f_192x192.png", true),
            new EnemySheet(EnemySheetArtBuilder.WalkState, "LDWalk_SS_8f_192x192.png", true),
            new EnemySheet(EnemySheetArtBuilder.AttackState, "LDAttack_SS_11f_192x192.png", false),
        };

        /// <summary>Normalized time of a frame's start in a clip of <paramref name="frames"/> frames (MeleeAnimationAttackBehavior's delays).</summary>
        public static float NormalizedFrameTime(int frame, int frames) => frames <= 0 ? 0f : (float)frame / frames;

        public static EnemyArtDefinition Definition => new EnemyArtDefinition
        {
            Name = "LightDevourer",
            SpriteFolder = SpriteFolder,
            AnimationFolder = AnimationFolder,
            ControllerPath = ControllerPath,
            PrefabPath = PrefabPath,
            Pivot = Pivot,
            // The idle sheet is 3072 wide and the attack 2112: the default 2048 blurred every frame
            MinMaxTextureSize = 4096,
            Sheets = Sheets,
            AttackTrigger = AttackTrigger,
            ConfigureAttackState = ConfigureAttackState,
        };

        [MenuItem("Tools/Flames of Faith/Enemies/Build Light Devourer Art")]
        public static void Build() => EnemySheetArtBuilder.Build(Definition);

        /// <summary>Batchmode entry (-executeMethod Game.Enemies.Editor.LightDevourerArtBuilder.RunBatch).</summary>
        public static void RunBatch() => Build();

        private static void ConfigureAttackState(AnimatorController controller, AnimatorState attack)
        {
            EnemySheetArtBuilder.TryParseSheetName(Sheets[Sheets.Length - 1].FileName, out int frames, out _, out _);
            var hit = attack.AddStateMachineBehaviour<MeleeAnimationAttackBehavior>();
            var serialized = new SerializedObject(hit);
            serialized.FindProperty("attackDelay").floatValue = NormalizedFrameTime(AttackHitFrame, frames);
            serialized.FindProperty("colliderDelay").floatValue = NormalizedFrameTime(AttackEndFrame, frames);
            serialized.FindProperty("animationEventName").stringValue = "Attack";
            serialized.ApplyModifiedPropertiesWithoutUndo();

            // Still slowed to the warning's Telegraph Time (AIMeleeAttacker) until that goes
            controller.AddParameter(new AnimatorControllerParameter
            {
                name = "AttackSpeed",
                type = AnimatorControllerParameterType.Float,
                defaultFloat = 1f,
            });
            attack.speedParameterActive = true;
            attack.speedParameter = "AttackSpeed";
        }
    }
}
