using Game.Combat;
using Game.Control;
using Game.Effects.Abilities;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>
    /// The Warrior's dash (Crusader Charge) hitbox. Enemies only have a solid feet collider, and the
    /// player's solid body collides with them, so a dash hitbox that doesn't fully contain the body
    /// gets stopped by the bodies colliding before its trigger overlaps the enemy: the charge then
    /// deals no damage at some angles. These tests guard that geometry and the Melee Damage scaling.
    /// </summary>
    public class WarriorDashTests
    {
        private const string WarriorPrefab = "Assets/Prefabs/Player/PlayerWarrior.prefab";
        private const string ChargeAbilityPath = "Assets/Resources/Effects/EffectBehaviors/CrusaderChargeAbility.asset";

        // How far the hitbox must reach past the body on every side. Contacts are solved a little
        // apart (Physics2D contact offset), so touching bodies must still leave the trigger overlapping.
        private const float MinHitboxMargin = 0.05f;

        // --- Capsule containment math ------------------------------------------------------

        private struct Capsule
        {
            public Vector2 a, b; // segment end points
            public float radius;
        }

        private static Capsule ToCapsule(Vector2 center, Vector2 size, CapsuleDirection2D direction)
        {
            bool vertical = direction == CapsuleDirection2D.Vertical;
            float radius = (vertical ? size.x : size.y) * 0.5f;
            float half = Mathf.Max(0f, (vertical ? size.y : size.x) * 0.5f - radius);
            Vector2 axis = vertical ? Vector2.up : Vector2.right;
            return new Capsule { a = center - axis * half, b = center + axis * half, radius = radius };
        }

        private static float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float t = ab.sqrMagnitude > 0f ? Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude) : 0f;
            return Vector2.Distance(p, a + ab * t);
        }

        /// <summary>
        /// How far <paramref name="outer"/> reaches past <paramref name="inner"/> at its closest point;
        /// negative when inner sticks out. Distance to a segment is convex, so the inner segment's end
        /// points are its worst case.
        /// </summary>
        private static float ContainmentMargin(Capsule outer, Capsule inner)
        {
            float worst = Mathf.Max(DistanceToSegment(inner.a, outer.a, outer.b), DistanceToSegment(inner.b, outer.a, outer.b));
            return outer.radius - inner.radius - worst;
        }

        [Test]
        public void ContainmentMargin_SameCenter_IsTheRadiusDifference()
        {
            var outer = ToCapsule(Vector2.zero, new Vector2(1f, 0.6f), CapsuleDirection2D.Horizontal);
            var inner = ToCapsule(Vector2.zero, new Vector2(0.6f, 0.4f), CapsuleDirection2D.Horizontal);
            Assert.AreEqual(0.1f, ContainmentMargin(outer, inner), 1e-4f);
        }

        [Test]
        public void ContainmentMargin_NarrowerOrOffset_IsNegative()
        {
            var body = ToCapsule(Vector2.zero, new Vector2(0.6f, 0.4f), CapsuleDirection2D.Horizontal);
            // The old hitbox shape: tall and narrow, centered above the feet.
            var tall = ToCapsule(new Vector2(0f, 0.3f), new Vector2(0.5f, 1f), CapsuleDirection2D.Vertical);
            Assert.Less(ContainmentMargin(tall, body), 0f);
        }

        // --- The real prefab ---------------------------------------------------------------

        private static (GameObject prefab, GameObject hitbox) LoadDash()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(WarriorPrefab);
            Assert.IsNotNull(prefab, $"Missing prefab {WarriorPrefab}");
            var dash = prefab.GetComponent<WarriorDash>();
            Assert.IsNotNull(dash, "PlayerWarrior has no WarriorDash");
            var hitbox = new SerializedObject(dash).FindProperty("dashHitbox")?.objectReferenceValue as GameObject;
            Assert.IsNotNull(hitbox, "WarriorDash.dashHitbox is not set");
            return (prefab, hitbox);
        }

        /// <summary>The collider as a capsule in the prefab root's space.</summary>
        private static Capsule InRootSpace(CapsuleCollider2D collider, Transform root)
        {
            Vector2 center = root.InverseTransformPoint(collider.transform.TransformPoint(collider.offset));
            Vector3 rootScale = root.lossyScale, scale = collider.transform.lossyScale;
            Vector2 size = new Vector2(collider.size.x * Mathf.Abs(scale.x / rootScale.x),
                                       collider.size.y * Mathf.Abs(scale.y / rootScale.y));
            return ToCapsule(center, size, collider.direction);
        }

        [Test]
        public void Hitbox_ContainsTheSolidBody()
        {
            var (prefab, hitbox) = LoadDash();

            var hitboxCollider = hitbox.GetComponent<CapsuleCollider2D>();
            Assert.IsNotNull(hitboxCollider, "The dash hitbox needs a CapsuleCollider2D (this test only measures capsules)");
            Assert.IsTrue(hitboxCollider.isTrigger, "The dash hitbox collider must be a trigger");

            var body = prefab.GetComponent<CapsuleCollider2D>();
            Assert.IsNotNull(body, "PlayerWarrior has no CapsuleCollider2D body on its root");
            Assert.IsFalse(body.isTrigger, "The player's body collider is expected to be solid");

            float margin = ContainmentMargin(InRootSpace(hitboxCollider, prefab.transform), InRootSpace(body, prefab.transform));
            Assert.GreaterOrEqual(margin, MinHitboxMargin,
                "The dash hitbox must contain the player's body with a margin, or a charge into an enemy is " +
                "stopped by the bodies colliding before the hitbox touches it");
        }

        [Test]
        public void Hitbox_IsMelee()
        {
            var (_, hitbox) = LoadDash();
            var damage = hitbox.GetComponent<DamageSourceBase>();
            Assert.IsNotNull(damage, "The dash hitbox has no DamageSourceBase");
            Assert.AreEqual(WeaponClass.Melee, damage.WeaponClass, "The dash scales with Melee Damage only as WeaponClass.Melee");
            Assert.Greater(damage.AttackScale, 0f, "The hitbox's own Melee Damage share (used without the Scroll)");
        }

        [Test]
        public void CrusaderCharge_EveryTierScalesWithMeleeDamage()
        {
            var charge = AssetDatabase.LoadAssetAtPath<CrusaderChargeAbility>(ChargeAbilityPath);
            Assert.IsNotNull(charge, $"Missing {ChargeAbilityPath}");
            Assert.Greater(charge.TierCount, 0);
            for (int i = 0; i < charge.TierCount; i++)
            {
                Assert.Greater(charge.Tiers[i].meleeDamageScale, 0f, $"Crusader Charge tier {i} has no Melee Damage share");
                if (i > 0)
                {
                    Assert.GreaterOrEqual(charge.Tiers[i].meleeDamageScale, charge.Tiers[i - 1].meleeDamageScale,
                        $"Crusader Charge tier {i} scales less than tier {i - 1}");
                }
            }
        }
    }
}
