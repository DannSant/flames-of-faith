using Game.AI.Behaviors;
using Game.Enemies;
using Game.Overworld;
using Game.Scene;
using Game.Waves;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>
    /// Guards against data that looks live but isn't (the Oct 2026 cleanup for the data-driven pilot): every
    /// enemy behavior, level and wave database is actually used, every scene a level loads is in the build,
    /// and each enemy's speed is in the field its movement reads (EnemyData.moveSpeed for walkers,
    /// hopForce for hoppers; the old speedBase meant both).
    /// </summary>
    public class DataHygieneTests
    {
        private static List<T> LoadAssets<T>(string folder) where T : Object =>
            AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] { folder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<T>)
                .Where(a => a != null)
                .ToList();

        private static HashSet<string> DependenciesOf(IEnumerable<string> paths) =>
            new(paths.SelectMany(p => AssetDatabase.GetDependencies(p, true)));

        private static IEnumerable<string> PrefabPaths(string folder) =>
            AssetDatabase.FindAssets("t:Prefab", new[] { folder }).Select(AssetDatabase.GUIDToAssetPath);

        private static List<string> BuildScenePaths() =>
            EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToList();

        [Test]
        public void EveryEnemyBehaviorAsset_IsUsedByAnEnemy()
        {
            var used = DependenciesOf(PrefabPaths("Assets/Prefabs/Enemies"));
            var unused = AssetDatabase.FindAssets("", new[] { "Assets/Resources/Behaviors" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => p.EndsWith(".asset") && !used.Contains(p))
                .ToList();
            Assert.IsEmpty(unused, "Enemy behavior assets no enemy prefab uses (delete them, or they look tunable):\n" +
                string.Join("\n", unused));
        }

        [Test]
        public void EveryLevel_IsOnAMap()
        {
            var mapPaths = AssetDatabase.FindAssets("t:MapDefinition", new[] { "Assets/Resources/Overworld" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .ToList();
            Assert.IsNotEmpty(mapPaths, "No MapDefinition assets");
            var used = DependenciesOf(mapPaths);

            var unused = AssetDatabase.FindAssets("t:LevelData", new[] { "Assets/Resources/Levels" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => !used.Contains(p))
                .ToList();
            Assert.IsEmpty(unused, "LevelData no map uses:\n" + string.Join("\n", unused));
        }

        [Test]
        public void EveryWaveDatabase_IsUsedByABuildScene()
        {
            var used = DependenciesOf(BuildScenePaths());
            var unused = AssetDatabase.FindAssets("t:WaveDatabase", new[] { "Assets/Resources/Waves" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => !used.Contains(p))
                .ToList();
            Assert.IsEmpty(unused, "Wave databases no scene in the build uses:\n" + string.Join("\n", unused));
        }

        [Test]
        public void EveryBuildScene_ExistsAndEveryLevelSceneIsInTheBuild()
        {
            var buildScenes = BuildScenePaths();
            var missing = buildScenes.Where(p => !File.Exists(p)).ToList();
            Assert.IsEmpty(missing, "Build list entries whose scene file is gone:\n" + string.Join("\n", missing));

            var buildSceneNames = new HashSet<string>(buildScenes.Select(Path.GetFileNameWithoutExtension));
            var notInBuild = LoadAssets<LevelData>("Assets/Resources/Levels")
                .Where(l => !buildSceneNames.Contains(l.SceneName))
                .Select(l => $"{l.name} loads \"{l.SceneName}\"")
                .ToList();
            Assert.IsEmpty(notInBuild, "Levels whose scene isn't in the build list:\n" + string.Join("\n", notInBuild));
        }

        [Test]
        public void EveryEnemy_HasTheSpeedItsMovementReads()
        {
            var enemies = LoadAssets<EnemyData>("Assets/Resources/Enemies");
            Assert.IsNotEmpty(enemies);

            var problems = new List<string>();
            foreach (var enemy in enemies.Where(e => e.enemyPrefab != null))
            {
                var behaviors = AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(enemy.enemyPrefab), true)
                    .Select(AssetDatabase.LoadAssetAtPath<AIBehavior>)
                    .Where(b => b != null)
                    .ToList();

                bool hops = behaviors.Any(b => b is AIBehaviorChaseOnAnimIntervals || b is AIBehaviorRoamOnAnimIntervals);
                bool walks = behaviors.Any(b => b is AIBehaviorChaserMovement || b is AIBehaviorMoveToSnapshot);

                if (hops && enemy.hopForce <= 0f) problems.Add($"{enemy.name} hops but its hopForce is {enemy.hopForce}");
                if (walks && enemy.moveSpeed <= 0f) problems.Add($"{enemy.name} walks but its moveSpeed is {enemy.moveSpeed}");
                if (!hops && enemy.hopForce > 0f) problems.Add($"{enemy.name} has hopForce {enemy.hopForce} but never hops");
                if (!walks && enemy.moveSpeed > 0f) problems.Add($"{enemy.name} has moveSpeed {enemy.moveSpeed} but never walks");
            }
            Assert.IsEmpty(problems, string.Join("\n", problems));
        }
    }
}
