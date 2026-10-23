using Game.AI;
using Game.Combat;
using Game.Enemies;
using Game.Progression;
using Game.Waves;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace Game.BalanceData.Editor
{
    /// <summary>
    /// Read-only tables computed from the balance data with the game's own formulas (EnemyDamageCalculator,
    /// Enemy.CalculateHealth, ExperienceSettings), so the designer sees what the numbers do without Unity. Written to
    /// BalanceData/Reports on every export and import; editing them changes nothing.
    /// </summary>
    public static class BalanceReports
    {
        private const int XpLevels = 30;
        private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

        private static string F(float value, string format = "0.##") => value.ToString(format, Invariant);

        public static List<BalanceTable> All()
        {
            var damage = BalanceTablesLoad<EnemyDamageSettings>(BalanceTables.EnemyDamageSettingsPath);
            var experience = BalanceTablesLoad<ExperienceSettings>(BalanceTables.ExperienceSettingsPath);
            var enemies = BalanceTables.LoadAll<EnemyData>(BalanceTables.EnemiesFolder).OrderBy(e => e.enemyType).ToList();
            int lastLevel = damage.ExpectedCurve.Count > 0 ? Mathf.CeilToInt(damage.ExpectedCurve[damage.ExpectedCurve.Count - 1].levelsBeaten) : 0;
            int maxWaves = Mathf.Max(1, BalanceTables.LoadAll<WaveDatabase>(BalanceTables.WavesFolder).Select(d => d.waves.Count).DefaultIfEmpty(1).Max());

            return new List<BalanceTable>
            {
                EnemyDamage(damage, enemies, lastLevel, maxWaves),
                EnemyHealth(enemies, lastLevel, maxWaves),
                Experience(experience),
            };
        }

        private static T BalanceTablesLoad<T>(string resourcesPath) where T : Object
        {
            var asset = Resources.Load<T>(resourcesPath);
            if (asset == null) throw new System.IO.FileNotFoundException($"Missing Resources/{resourcesPath}");
            return asset;
        }

        /// <summary>Each enemy attack's damage per hit, and the hits the expected player survives, at every point of the run.</summary>
        public static BalanceTable EnemyDamage(EnemyDamageSettings settings, List<EnemyData> enemies, int lastLevel, int maxWaves)
        {
            var table = new BalanceTable("EnemyDamage", new[]
            {
                "Levels Beaten", "Wave", "Expected Max Health", "Expected Armor", "Enemy", "Attack", "Tier", "Damage Per Hit", "Hits To Die",
            });
            for (int level = 0; level <= lastLevel; level++)
            {
                for (int wave = 1; wave <= maxWaves; wave++)
                {
                    float progress = EnemyDamageCalculator.GetProgress(wave, level, settings);
                    var expected = EnemyDamageCalculator.GetExpectedStats(progress, settings);
                    foreach (var enemy in enemies)
                    {
                        foreach (var kind in new[] { EnemyDamageKind.Contact, EnemyDamageKind.Projectile })
                        {
                            float tier = enemy.GetDamageTier(kind);
                            if (tier <= 0f) continue;
                            float raw = EnemyDamageCalculator.Calculate(enemy, wave, level, kind, settings);
                            int hits = EnemyDamageCalculator.SimulateHitsToDie(expected.maxHealth, expected.armor, raw, settings);
                            table.AddRow(level.ToString(), wave.ToString(), F(expected.maxHealth), F(expected.armor),
                                enemy.enemyType.ToString(), kind.ToString(), F(tier), F(raw), hits.ToString());
                        }
                    }
                }
            }
            return table;
        }

        /// <summary>Each enemy's health at every point of the run, normal and Corrupted.</summary>
        public static BalanceTable EnemyHealth(List<EnemyData> enemies, int lastLevel, int maxWaves)
        {
            float corruptedBonus = CorruptionSettings.Instance.CorruptedHealthBonus;
            var table = new BalanceTable("EnemyHealth", new[] { "Enemy", "Levels Beaten", "Wave", "Health", "Corrupted Health" });
            foreach (var enemy in enemies)
            {
                for (int level = 0; level <= lastLevel; level++)
                {
                    for (int wave = 1; wave <= maxWaves; wave++)
                    {
                        table.AddRow(enemy.enemyType.ToString(), level.ToString(), wave.ToString(),
                            Enemy.CalculateHealth(enemy, wave, level, enemy.alwaysCorrupted, corruptedBonus).ToString(),
                            Enemy.CalculateHealth(enemy, wave, level, true, corruptedBonus).ToString());
                    }
                }
            }
            return table;
        }

        /// <summary>XP needed for each level and in total.</summary>
        public static BalanceTable Experience(ExperienceSettings settings)
        {
            var table = new BalanceTable("Experience", new[] { "Level", "XP To Next Level", "Total XP To Reach Level" });
            int total = 0;
            for (int level = 1; level <= XpLevels; level++)
            {
                int required = settings.GetXPRequired(level);
                table.AddRow(level.ToString(), required.ToString(), total.ToString());
                total += required;
            }
            return table;
        }
    }
}
