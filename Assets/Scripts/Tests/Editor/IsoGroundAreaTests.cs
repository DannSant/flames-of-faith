using Game.AI;
using Game.AI.Behaviors;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>
    /// A melee attack's area is a circle lying on the isometric ground: an ellipse half as tall as it is wide.
    /// The warning draws it and the hitbox is the same shape, so a player is hit exactly when standing inside
    /// what's drawn, in every direction.
    /// </summary>
    public class IsoGroundAreaTests
    {
        private const float Ratio = 0.5f;

        private static readonly Vector2[] EightDirections =
        {
            Vector2.up, new Vector2(1, 1), Vector2.right, new Vector2(1, -1),
            Vector2.down, new Vector2(-1, -1), Vector2.left, new Vector2(-1, 1),
        };

        // --- Pure math -----------------------------------------------------------------------------------

        [Test]
        public void Size_IsTwiceTheRadiusWide_AndRatioTimesThatTall()
        {
            Assert.AreEqual(new Vector2(2f, 1f), IsoGroundArea.Size(1f, Ratio));
            Assert.AreEqual(new Vector2(1.5f, 1.5f), IsoGroundArea.Size(0.75f, 1f), "Ratio 1 is a circle");
        }

        [TestCase(0.99f, 0f, true)]   // just inside the right side
        [TestCase(1.01f, 0f, false)]  // just outside it
        [TestCase(-0.99f, 0f, true)]  // left side
        [TestCase(-1.01f, 0f, false)]
        [TestCase(0f, 0.49f, true)]   // just inside the top: the ellipse is only half as tall
        [TestCase(0f, 0.51f, false)]  // just outside the top, though well inside the old circle
        [TestCase(0f, -0.49f, true)]  // bottom
        [TestCase(0f, -0.51f, false)]
        [TestCase(0.7f, 0.34f, true)]  // inside on a diagonal (0.49 + 0.4624 < 1)
        [TestCase(0.7f, 0.37f, false)] // outside on it (0.49 + 0.5476 > 1)
        public void Contains_FollowsTheEllipse(float x, float y, bool inside)
        {
            var center = new Vector2(3f, -2f);
            Assert.AreEqual(inside, IsoGroundArea.Contains(center, 1f, Ratio, center + new Vector2(x, y)));
        }

        [Test]
        public void Contains_WithRatioOne_IsACircle()
        {
            Assert.IsTrue(IsoGroundArea.Contains(Vector2.zero, 1f, 1f, new Vector2(0f, 0.99f)));
            Assert.IsTrue(IsoGroundArea.Contains(Vector2.zero, 1f, 1f, new Vector2(0.7f, 0.7f)));
            Assert.IsFalse(IsoGroundArea.Contains(Vector2.zero, 1f, 1f, new Vector2(0.72f, 0.72f)));
            Assert.IsFalse(IsoGroundArea.Contains(Vector2.zero, 0f, 1f, Vector2.zero), "No radius, no area");
        }

        [Test]
        public void GroundOffset_StaysTowardTheTarget_AndShortensItsVerticalReach()
        {
            Assert.AreEqual(new Vector2(1f, 0f), IsoGroundArea.GroundOffset(new Vector2(5f, 0f), 1f, Ratio), "Sideways: the full distance");
            AssertClose(new Vector2(0f, 0.5f), IsoGroundArea.GroundOffset(new Vector2(0f, 3f), 1f, Ratio), "North: half the distance, like the ground");
            AssertClose(new Vector2(0f, -0.5f), IsoGroundArea.GroundOffset(Vector2.down, 1f, Ratio), "South");
            Assert.AreEqual(Vector2.zero, IsoGroundArea.GroundOffset(Vector2.zero, 1f, Ratio));

            foreach (var direction in EightDirections)
            {
                Vector2 offset = IsoGroundArea.GroundOffset(direction, 1f, Ratio);
                Assert.AreEqual(0f, Vector3.Cross(direction, offset).z, 1e-5f, $"{direction}: the offset must point at the target");
                Assert.Greater(Vector2.Dot(direction, offset), 0f, $"{direction}: toward, not away");
                Vector2 ground = new Vector2(offset.x, offset.y / Ratio);
                Assert.AreEqual(1f, ground.magnitude, 1e-5f, $"{direction}: the distance is measured on the ground");
            }
        }

        [Test]
        public void GroundOffset_WithRatioOne_IsTheOldCircleOffset()
        {
            foreach (var direction in EightDirections)
            {
                AssertClose(direction.normalized * 0.75f, IsoGroundArea.GroundOffset(direction, 0.75f, 1f), direction.ToString());
            }
        }

        [Test]
        public void Outline_LiesOnTheEllipse()
        {
            var center = new Vector2(0.2f, -0.5f);
            var points = IsoGroundArea.Outline(center, 1.3f, Ratio, 48);
            Assert.AreEqual(48, points.Length);
            foreach (var point in points)
            {
                Vector2 d = point - center;
                Assert.AreEqual(1f, (d.x / 1.3f) * (d.x / 1.3f) + (d.y / (1.3f * Ratio)) * (d.y / (1.3f * Ratio)), 1e-4f);
            }
        }

        // --- The warning and the hitbox are the same shape -----------------------------------------------

        [Test]
        public void Warning_IsDrawnAsTheGroundEllipse()
        {
            Assert.AreEqual(new Vector3(2f, 1f, 1f), AttackTelegraph.ScaleFor(null, 2f, Ratio), "No sprite: one unit wide");

            var texture = new Texture2D(64, 64);
            var sprite = Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 32f); // 2 units wide
            try
            {
                Vector3 scale = AttackTelegraph.ScaleFor(sprite, 2f, Ratio);
                Vector2 drawn = Vector2.Scale(sprite.bounds.size, scale);
                AssertClose(IsoGroundArea.Size(1f, Ratio), drawn, "The ring covers exactly the ellipse");
            }
            finally
            {
                Object.DestroyImmediate(sprite);
                Object.DestroyImmediate(texture);
            }
        }

        [Test]
        public void Hitbox_IsTheDrawnEllipse_InEveryDirection()
        {
            var enemy = new GameObject("Enemy");
            try
            {
                var hitboxObject = new GameObject("MeleeCollider");
                hitboxObject.transform.SetParent(enemy.transform, false);
                var circle = hitboxObject.AddComponent<CircleCollider2D>();
                circle.radius = 0.5f;
                circle.offset = new Vector2(0f, -0.5f);

                var state = new MeleeAttackerBehaviorState();
                var hitbox = AIMeleeAttacker.CreateGroundHitbox(circle, Ratio, state) as PolygonCollider2D;
                Assert.IsNotNull(hitbox, "A circle becomes a polygon outlining the ground ellipse");
                Assert.IsFalse(circle.enabled, "The authored circle must not hit too");
                Assert.IsTrue(hitbox.isTrigger, "A solid hitbox shoves the player out of it on top of the knockback");
                Assert.AreEqual((0.5f, new Vector2(0f, -0.5f)), (state.areaRadius, state.areaOffset), "The warning uses the authored circle");
                Assert.AreSame(hitbox, AIMeleeAttacker.CreateGroundHitbox(hitbox, Ratio, state), "Other shapes are kept as they are");

                // The hitbox moved toward the player in each direction: the warning's ellipse and the polygon agree on
                // points just inside and just outside its top, bottom and sides
                foreach (var direction in EightDirections)
                {
                    Vector2 position = AIMeleeAttacker.HitboxOffset(direction, 1f, Ratio);
                    Vector2 center = position + state.areaOffset;
                    foreach (var edge in new[] { Vector2.up, Vector2.down, Vector2.left, Vector2.right, new Vector2(0.7071f, 0.7071f) })
                    {
                        Vector2 onEdge = center + new Vector2(edge.x * state.areaRadius, edge.y * state.areaRadius * Ratio);
                        foreach (float scale in new[] { 0.97f, 1.03f })
                        {
                            Vector2 point = center + (onEdge - center) * scale;
                            bool drawn = IsoGroundArea.Contains(center, state.areaRadius, Ratio, point);
                            bool hit = InsidePolygon(hitbox.points, point - position);
                            Assert.AreEqual(scale < 1f, drawn, $"{direction} {edge} x{scale}: the drawn ellipse");
                            Assert.AreEqual(drawn, hit, $"{direction} {edge} x{scale}: the hitbox differs from the warning");
                        }
                    }
                }
            }
            finally
            {
                Object.DestroyImmediate(enemy);
            }
        }

        [Test]
        public void Settings_PutTheGroundRatioOnTheAsset()
        {
            var settings = Resources.Load<AttackTelegraphSettings>("AI/AttackTelegraphSettings");
            Assert.IsNotNull(settings);
            Assert.AreEqual(0.5f, settings.GroundRatio, 1e-5f, "Level grids are isometric with 1 x 0.5 cells");
        }

        private static bool InsidePolygon(Vector2[] polygon, Vector2 point)
        {
            bool inside = false;
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                if ((polygon[i].y > point.y) != (polygon[j].y > point.y) &&
                    point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x)
                {
                    inside = !inside;
                }
            }
            return inside;
        }

        private static void AssertClose(Vector2 expected, Vector2 actual, string message)
        {
            Assert.AreEqual(expected.x, actual.x, 1e-5f, message);
            Assert.AreEqual(expected.y, actual.y, 1e-5f, message);
        }
    }
}
