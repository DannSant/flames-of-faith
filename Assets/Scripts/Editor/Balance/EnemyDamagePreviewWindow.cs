using System.Collections.Generic;
using Game.Enemies;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.Balance
{
    /// <summary>
    /// Tuning table for enemy -> player damage. Rows are points of the run (levels beaten and wave),
    /// columns are damage tiers or enemy attacks. Each cell shows the raw damage and how many hits
    /// the expected player survives, and optionally how many a custom player (e.g. stacked armor)
    /// survives. Everything goes through EnemyDamageCalculator, so it always matches the game.
    /// </summary>
    public class EnemyDamagePreviewWindow : EditorWindow
    {
        private enum ColumnMode { Tiers, Enemies }

        private const float RowLabelWidth = 150f;
        private const float CellWidth = 120f;

        private EnemyDamageSettings settings;
        private ColumnMode mode = ColumnMode.Tiers;
        private int wavesPerLevel = 2;
        private bool showCustomPlayer;
        private float customMaxHealth = 60f;
        private float customArmor = 5f;
        private Vector2 scroll;

        private readonly List<(string label, EnemyData enemy, EnemyDamageKind kind)> enemyColumns = new();

        [MenuItem("Tools/Flames of Faith/Enemy Damage Preview")]
        public static void Open()
        {
            GetWindow<EnemyDamagePreviewWindow>("Enemy Damage Preview");
        }

        private void OnEnable()
        {
            if (settings == null)
            {
                settings = Resources.Load<EnemyDamageSettings>("Combat/EnemyDamageSettings");
            }
            RefreshEnemies();
        }

        private void OnFocus()
        {
            RefreshEnemies();
        }

        private void RefreshEnemies()
        {
            enemyColumns.Clear();
            foreach (var enemy in Resources.LoadAll<EnemyData>("Enemies"))
            {
                if (enemy.contactDamageTier > 0f) enemyColumns.Add(($"{enemy.name}\nContact T{enemy.contactDamageTier:0.##}", enemy, EnemyDamageKind.Contact));
                if (enemy.projectileDamageTier > 0f) enemyColumns.Add(($"{enemy.name}\nProjectile T{enemy.projectileDamageTier:0.##}", enemy, EnemyDamageKind.Projectile));
            }
        }

        private void OnGUI()
        {
            settings = (EnemyDamageSettings)EditorGUILayout.ObjectField("Settings", settings, typeof(EnemyDamageSettings), false);
            if (settings == null)
            {
                EditorGUILayout.HelpBox("Assign an EnemyDamageSettings asset (Resources/Combat/EnemyDamageSettings).", MessageType.Info);
                return;
            }
            if (settings.ExpectedCurve.Count == 0 || settings.TierHitsToKill.Count == 0)
            {
                EditorGUILayout.HelpBox("The expected curve and the tier table both need at least one row.", MessageType.Warning);
                return;
            }

            mode = (ColumnMode)GUILayout.Toolbar((int)mode, new[] { "By tier", "By enemy" });
            wavesPerLevel = EditorGUILayout.IntSlider("Waves per level", wavesPerLevel, 1, 6);
            showCustomPlayer = EditorGUILayout.Toggle("Compare a custom player", showCustomPlayer);
            if (showCustomPlayer)
            {
                EditorGUI.indentLevel++;
                customMaxHealth = EditorGUILayout.FloatField("Max health", customMaxHealth);
                customArmor = EditorGUILayout.FloatField("Armor", customArmor);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.HelpBox(
                "Each cell: raw damage per hit · hits the expected player survives" +
                (showCustomPlayer ? " · hits the custom player survives." : "."),
                MessageType.None);

            scroll = EditorGUILayout.BeginScrollView(scroll);
            DrawHeader();

            var curve = settings.ExpectedCurve;
            int lastLevel = Mathf.CeilToInt(curve[curve.Count - 1].levelsBeaten);
            for (int level = 0; level <= lastLevel; level++)
            {
                for (int wave = 1; wave <= wavesPerLevel; wave++)
                {
                    DrawRow(level, wave);
                }
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Progress (expected)", EditorStyles.boldLabel, GUILayout.Width(RowLabelWidth));
            if (mode == ColumnMode.Tiers)
            {
                for (int tier = 1; tier <= settings.TierHitsToKill.Count; tier++)
                {
                    GUILayout.Label($"Tier {tier}\n({settings.TierHitsToKill[tier - 1]:0.#} hits)", EditorStyles.boldLabel, GUILayout.Width(CellWidth));
                }
            }
            else
            {
                foreach (var column in enemyColumns)
                {
                    GUILayout.Label(column.label, EditorStyles.boldLabel, GUILayout.Width(CellWidth));
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawRow(int level, int wave)
        {
            float progress = EnemyDamageCalculator.GetProgress(wave, level, settings);
            var expected = EnemyDamageCalculator.GetExpectedStats(progress, settings);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"L{level} W{wave}  ({expected.maxHealth:0.#} HP, {expected.armor:0.#} AR)", GUILayout.Width(RowLabelWidth));
            if (mode == ColumnMode.Tiers)
            {
                for (int tier = 1; tier <= settings.TierHitsToKill.Count; tier++)
                {
                    DrawCell(EnemyDamageCalculator.CalculateForTier(tier, progress, settings), expected);
                }
            }
            else
            {
                foreach (var column in enemyColumns)
                {
                    DrawCell(EnemyDamageCalculator.Calculate(column.enemy, wave, level, column.kind, settings), expected);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawCell(float raw, ExpectedPlayerStats expected)
        {
            int expectedHits = EnemyDamageCalculator.SimulateHitsToDie(expected.maxHealth, expected.armor, raw, settings);
            string text = $"{raw:0.##} · {expectedHits}";
            if (showCustomPlayer)
            {
                int customHits = EnemyDamageCalculator.SimulateHitsToDie(customMaxHealth, customArmor, raw, settings);
                text += $" · {customHits}";
            }
            GUILayout.Label(text, GUILayout.Width(CellWidth));
        }
    }
}
