using System.Linq;
using Game.AI.Behaviors;
using Game.Boss;
using Game.Control;
using Game.Misc;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>
    /// Owner play-test, Nov 2026:
    /// - a player standing north of an enemy drew on top of it: enemies sat on sorting order 3, the player and the trees
    ///   on 10, and the renderer's Y sort (Transparency Sort Axis 0,1,0) only orders sprites of the same layer and order;
    /// - the Light Devourer's hit pushed the player a little or across half the level: its hitbox was solid (physics
    ///   shoved the player out of it), the push added to the player's velocity, and it ran from the hitbox's transform.
    /// </summary>
    public class KnockbackAndSortingTests
    {
        private const float Ratio = 0.5f;

        // --- Knockback ----------------------------------------------------------------------------------------

        [Test]
        public void PushVelocity_IsTheForceAlongTheDirection()
        {
            Assert.AreEqual(new Vector2(3f, 0f), Knockback.PushVelocity(new Vector2(0.2f, 0f), 3f));
            Assert.AreEqual(3f, Knockback.PushVelocity(new Vector2(-1f, 1f), 3f).magnitude, 1e-5f);
            Assert.AreEqual(Vector2.zero, Knockback.PushVelocity(Vector2.zero, 3f), "No direction, no push");
        }

        [Test]
        public void GroundPush_IsShorterUpAndDownTheScreen()
        {
            Assert.AreEqual(new Vector2(3f, 0f), Knockback.GroundPushVelocity(Vector2.right * 5f, 3f, Ratio));
            Vector2 up = Knockback.GroundPushVelocity(Vector2.up * 0.1f, 3f, Ratio);
            Assert.AreEqual(0f, up.x, 1e-5f);
            Assert.AreEqual(1.5f, up.y, 1e-5f, "Straight up the screen: the same ground distance, half the screen distance");
            Assert.AreEqual(new Vector2(0f, -3f), Knockback.GroundPushVelocity(Vector2.down, 3f, 1f), "Ratio 1: as on screen");
        }

        [Test]
        public void ConsistentPush_SetsTheSpeed_AndStopsWhenTheKnockbackEnds()
        {
            var body = new GameObject("Body");
            try
            {
                var rb = body.AddComponent<Rigidbody2D>();
                rb.gravityScale = 0f;
                var knockback = body.AddComponent<Knockback>();
                var serialized = new SerializedObject(knockback);
                serialized.FindProperty("consistentPush").boolValue = true;
                serialized.ApplyModifiedPropertiesWithoutUndo();

                rb.linearVelocity = new Vector2(-20f, 7f); // whatever it was doing, e.g. pushed out of a collider
                knockback.ApplyKnockbackFrom(new Vector2(-1f, 0f), 3f);
                Assert.IsTrue(knockback.IsKnockbacked);
                Assert.AreEqual(3f, rb.linearVelocity.x, 1e-4f, "The push replaces the velocity instead of adding to it");
                Assert.AreEqual(0f, rb.linearVelocity.y, 1e-4f);

                knockback.ApplyKnockbackFrom(new Vector2(0f, -1f), 30f);
                Assert.AreEqual(3f, rb.linearVelocity.x, 1e-4f, "A second hit during the knockback doesn't push again");

                knockback.SetImmune(true); // ends the knockback, like its timer running out
                Assert.IsFalse(knockback.IsKnockbacked);
                Assert.AreEqual(Vector2.zero, rb.linearVelocity, "The push stops when the knockback ends: no slide across the level");
            }
            finally
            {
                Object.DestroyImmediate(body);
            }
        }

        [Test]
        public void Player_UsesTheConsistentPush()
        {
            var player = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/PlayerBase.prefab");
            var knockback = player.GetComponent<Knockback>();
            Assert.IsNotNull(knockback);
            Assert.IsTrue(new SerializedObject(knockback).FindProperty("consistentPush").boolValue);
        }

        // --- Sorting ------------------------------------------------------------------------------------------

        [Test]
        public void EveryCharacterBody_SortsWithTheTrees()
        {
            var tree = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Oak_Tree.prefab").GetComponentInChildren<SpriteRenderer>();
            Assert.IsNotNull(tree, "Oak_Tree has no sprite");

            int checkedBodies = 0;
            foreach (string path in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Enemies", "Assets/Prefabs/Player" })
                         .Select(AssetDatabase.GUIDToAssetPath))
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                SpriteRenderer body = null;
                if (prefab.GetComponent<BehaviorController>() != null) body = prefab.GetComponent<SpriteRenderer>();
                else if (prefab.GetComponent<BossController>() != null) body = prefab.GetComponentsInChildren<SpriteRenderer>(true).OrderByDescending(r => r.sortingOrder).FirstOrDefault();
                else if (prefab.GetComponentInChildren<CharacterVisual>(true) != null) body = prefab.GetComponentInChildren<CharacterVisual>(true).GetComponent<SpriteRenderer>();
                if (body == null) continue;

                checkedBodies++;
                Assert.AreEqual(tree.sortingLayerID, body.sortingLayerID, $"{prefab.name}: another sorting layer than the trees");
                Assert.AreEqual(tree.sortingOrder, body.sortingOrder,
                    $"{prefab.name}: order {body.sortingOrder}, the trees and characters are on {tree.sortingOrder}. The Y sort only " +
                    "orders sprites on the same order, so it would draw over or under everyone wherever it stands.");
            }
            Assert.Greater(checkedBodies, 8, "Found too few characters: the prefab folders moved?");
        }
    }
}
