using Game.AI;
using Game.AI.Behaviors;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>
    /// Distances on the isometric ground, feet to feet (tester report, Nov 2026): the Light Devourer attacking from the
    /// south drew its area on its own body and never hit, because its stop distance and range were measured on screen,
    /// where a player straight above it is only half as far as one beside it, and its area orbited from the sprite's middle.
    /// </summary>
    public class IsoGroundTests
    {
        private const float Ratio = 0.5f;
        private const string LightDevourerPrefab = "Assets/Prefabs/Enemies/EnemyLightDevourer.prefab";
        private const string LightDevourerAttack = "Assets/Resources/Behaviors/LightDevourer/LDAttack.asset";
        private const string LightDevourerChase = "Assets/Resources/Behaviors/LightDevourer/LDChase.asset";
        private static readonly string[] PlayerPrefabs =
        {
            "Assets/Prefabs/Player/PlayerWarrior.prefab", "Assets/Prefabs/Player/PlayerArcher.prefab", "Assets/Prefabs/Player/PlayerAugur.prefab",
        };

        private static readonly Vector2[] EightDirections =
        {
            Vector2.up, new Vector2(1, 1), Vector2.right, new Vector2(1, -1),
            Vector2.down, new Vector2(-1, -1), Vector2.left, new Vector2(-1, 1),
        };

        // --- Pure math -----------------------------------------------------------------------------------

        [Test]
        public void GroundDistance_CountsVerticalScreenDistanceDouble()
        {
            Assert.AreEqual(1f, IsoGroundArea.GroundDistance(Vector2.zero, new Vector2(1f, 0f), Ratio), 1e-5f, "Sideways: as on screen");
            Assert.AreEqual(1f, IsoGroundArea.GroundDistance(Vector2.zero, new Vector2(0f, 0.5f), Ratio), 1e-5f, "Half a unit up the screen is a unit of ground");
            Assert.AreEqual(1f, IsoGroundArea.GroundDistance(new Vector2(2f, 3f), new Vector2(2f, 2.5f), Ratio), 1e-5f, "Down is the same as up");
            Assert.AreEqual(5f, IsoGroundArea.GroundDistance(Vector2.zero, new Vector2(3f, 2f), Ratio), 1e-5f, "(3, 4) on the ground");
            Assert.AreEqual(0.7071068f, IsoGroundArea.GroundDistance(Vector2.zero, new Vector2(0.5f, 0.5f), 1f), 1e-5f, "Ratio 1: screen distance");
        }

        [Test]
        public void GroundDistance_AgreesWithTheDrawnArea()
        {
            // A point is inside a ground circle exactly when its ground distance to the center is within the radius
            foreach (var direction in EightDirections)
            {
                Vector2 edge = IsoGroundArea.GroundOffset(direction, 1f, Ratio);
                Assert.AreEqual(1f, IsoGroundArea.GroundDistance(Vector2.zero, edge, Ratio), 1e-5f, direction.ToString());
                Assert.IsTrue(IsoGroundArea.Contains(Vector2.zero, 1f, Ratio, edge * 0.98f), direction.ToString());
                Assert.IsFalse(IsoGroundArea.Contains(Vector2.zero, 1f, Ratio, edge * 1.02f), direction.ToString());
            }
        }

        [Test]
        public void AreaCenter_OrbitsFromTheFeet_TheSameGroundDistanceEverySide()
        {
            var feet = new Vector2(4f, -2f);
            foreach (var direction in EightDirections)
            {
                Vector2 center = AIMeleeAttacker.AreaCenter(feet, direction, 1f, Ratio);
                Assert.AreEqual(1f, IsoGroundArea.GroundDistance(feet, center, Ratio), 1e-5f, $"{direction}: one unit of ground from the feet");

                // A player whose feet are anywhere from Orbit Radius - radius to + radius away on the ground is inside
                foreach (float playerDistance in new[] { 0.55f, 1f, 1.45f })
                {
                    Vector2 playerFeet = feet + IsoGroundArea.GroundOffset(direction, playerDistance, Ratio);
                    Assert.IsTrue(IsoGroundArea.Contains(center, 0.5f, Ratio, playerFeet), $"{direction} at {playerDistance}: missed");
                }
            }
        }

        // --- Ground points ------------------------------------------------------------------------------

        [Test]
        public void PointOf_IsTheSolidBodyCollider_NotTheTriggers()
        {
            var character = new GameObject("Character");
            try
            {
                character.transform.position = new Vector3(3f, 5f, 0f);
                var trigger = character.AddComponent<BoxCollider2D>();
                trigger.isTrigger = true;
                trigger.offset = new Vector2(0f, 2f);
                Assert.AreEqual(new Vector2(3f, 5f), IsoGround.PointOf(character.transform), "No solid body: the position");

                var body = character.AddComponent<CapsuleCollider2D>();
                body.offset = new Vector2(0.1f, -0.5f);
                Assert.AreEqual(new Vector2(3.1f, 4.5f), IsoGround.PointOf(character.transform));
                Assert.AreSame(body, IsoGround.BodyOf(character.transform));

                body.enabled = false;
                Assert.AreEqual(new Vector2(3f, 5f), IsoGround.PointOf(character.transform), "A disabled body doesn't count");
            }
            finally
            {
                Object.DestroyImmediate(character);
            }
        }

        // --- The Light Devourer reaches the player from every side -----------------------------------------

        [Test]
        public void LightDevourer_MeasuresOnTheGround()
        {
            var chase = AssetDatabase.LoadAssetAtPath<AIBehaviorChaserLogic>(LightDevourerChase);
            Assert.IsNotNull(chase, LightDevourerChase);
            Assert.IsTrue(chase.MeasureOnGround, "On screen it stops twice as far (on the ground) above or below the player as beside it");
        }

        // With the real colliders: where the enemy ends up next to the player (its stop distance, or touching when the
        // bodies meet first), the player is within Max Range and inside the hit area, straight above, below and beside it
        [Test]
        public void LightDevourer_ReachesThePlayer_AboveBelowAndBeside()
        {
            var attack = AssetDatabase.LoadAssetAtPath<AIMeleeAttacker>(LightDevourerAttack);
            var chase = AssetDatabase.LoadAssetAtPath<AIBehaviorChaserLogic>(LightDevourerChase);
            var enemy = AssetDatabase.LoadAssetAtPath<GameObject>(LightDevourerPrefab);
            Assert.IsNotNull(attack, LightDevourerAttack);
            Assert.IsNotNull(enemy, LightDevourerPrefab);
            float ratio = AttackTelegraphSettings.Instance.GroundRatio;
            var enemyBody = IsoGround.BodyOf(enemy.transform);
            Assert.IsNotNull(enemyBody, "The Light Devourer needs a solid body collider: its feet are measured from it");
            var area = enemy.transform.Find("MeleeCollider").GetComponent<CircleCollider2D>();
            Assert.IsNotNull(area, "MeleeCollider's circle sets the area's radius");

            Assert.LessOrEqual(attack.MaxRange, attack.OrbitRadius + area.radius,
                "Max Range past the area's far edge: the enemy swings at a player it can't reach");

            foreach (string path in PlayerPrefabs)
            {
                var player = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var playerBody = IsoGround.BodyOf(player.transform);
                Assert.IsNotNull(playerBody, $"{path}: no solid body collider");

                foreach (var axis in new[] { Vector2.up, Vector2.down, Vector2.right, Vector2.left })
                {
                    // Bodies touching along this axis, center to center (the ground points), in ground units
                    Vector2 gap = HalfExtents(enemyBody) + HalfExtents(playerBody);
                    float touching = axis.x != 0f ? gap.x : gap.y / ratio;
                    float stopsAt = Mathf.Max(touching, chase.MinRange);
                    string where = $"{player.name}, enemy reaching {axis}";

                    Assert.LessOrEqual(stopsAt, attack.MaxRange, $"{where}: it stops {stopsAt:0.00} away on the ground and never swings (Max Range {attack.MaxRange})");
                    Vector2 playerFeet = IsoGroundArea.GroundOffset(axis, stopsAt, ratio);
                    Vector2 center = AIMeleeAttacker.AreaCenter(Vector2.zero, axis, attack.OrbitRadius, ratio);
                    Assert.IsTrue(IsoGroundArea.Contains(center, area.radius, ratio, playerFeet),
                        $"{where}: the player's feet ({stopsAt:0.00} away) are outside the area (Orbit Radius {attack.OrbitRadius}, radius {area.radius})");
                }
            }
        }

        private static Vector2 HalfExtents(Collider2D collider) => collider switch
        {
            CapsuleCollider2D capsule => capsule.size * 0.5f,
            BoxCollider2D box => box.size * 0.5f,
            CircleCollider2D circle => Vector2.one * circle.radius,
            _ => Vector2.zero,
        };
    }
}
