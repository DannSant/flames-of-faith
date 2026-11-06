using Game.Control;
using Game.Overworld;
using Game.Scene;
using NUnit.Framework;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>
    /// The pilgrim on the overworld map (Nov 2026, BACKLOG 42): the played class's animated pilgrim walks along
    /// the drawn path to the next node. The walk math, the look gameplay pushes for each class, and the scene.
    /// </summary>
    public class MapPilgrimTests
    {
        private const float Eps = 1e-4f;

        // --- Walk math -------------------------------------------------------------------------------------

        [Test]
        public void Path_FollowsTheEdgeAsDrawn_ReversedWhenWalkedBackwards()
        {
            var edge = new[] { new Vector2(0, 0), new Vector2(1, 1), new Vector2(2, 0) };
            CollectionAssert.AreEqual(edge, MapWalk.PathBetween(edge, new Vector2(0, 0), new Vector2(2, 0)));
            CollectionAssert.AreEqual(edge.Reverse(), MapWalk.PathBetween(edge, new Vector2(2, 0), new Vector2(0, 0)));
            CollectionAssert.AreEqual(new[] { new Vector2(0, 0), new Vector2(3, 4) }, MapWalk.PathBetween(null, new Vector2(0, 0), new Vector2(3, 4)),
                "No drawn edge: a straight line");
        }

        [Test]
        public void PositionAndHeading_AlongThePath()
        {
            var path = new[] { new Vector2(0, 0), new Vector2(3, 0), new Vector2(3, 4) };
            Assert.AreEqual(7f, MapWalk.Length(path), Eps);
            Assert.AreEqual(new Vector2(0, 0), MapWalk.PointAt(path, -1f));
            Assert.AreEqual(new Vector2(1.5f, 0), MapWalk.PointAt(path, 1.5f));
            Assert.AreEqual(new Vector2(3, 2), MapWalk.PointAt(path, 5f));
            Assert.AreEqual(new Vector2(3, 4), MapWalk.PointAt(path, 99f), "Clamped to the end");
            Assert.AreEqual(Vector2.right, MapWalk.DirectionAt(path, 1f));
            Assert.AreEqual(Vector2.up, MapWalk.DirectionAt(path, 4f));
            Assert.AreEqual(Vector2.up, MapWalk.DirectionAt(path, 99f), "Past the end it keeps the last heading");
            Assert.AreEqual(Vector2.zero, MapWalk.DirectionAt(new[] { Vector2.one, Vector2.one }, 0f));
        }

        [Test]
        public void Duration_FollowsTheSpeed_ButLongPathsAreCapped()
        {
            Assert.AreEqual(1f, MapWalk.Duration(3f, 3f, 1.2f), Eps);
            Assert.AreEqual(1.2f, MapWalk.Duration(30f, 3f, 1.2f), Eps, "A long edge doesn't drag");
            Assert.AreEqual(0f, MapWalk.Duration(0f, 3f, 1.2f));
        }

        [Test]
        public void Facing_IsTheDirectionOfTravel_ElseThePreviousOne()
        {
            Assert.AreEqual(new Vector2(0.6f, 0.8f), MapWalk.Facing(new Vector2(3, 4), Vector2.down));
            Assert.AreEqual(Vector2.left, MapWalk.Facing(Vector2.zero, Vector2.left), "Standing still keeps the facing");
        }

        // --- The look gameplay pushes ----------------------------------------------------------------------

        [TestCase("Assets/Prefabs/Player/PlayerCrusader.prefab")]
        [TestCase("Assets/Prefabs/Player/PlayerSentinel.prefab")]
        [TestCase("Assets/Prefabs/Player/PlayerAugur.prefab")]
        public void EveryClass_GivesTheMapItsAnimatedPilgrim(string prefabPath)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Assert.IsNotNull(prefab, prefabPath);
            var look = PlayerManager.BuildMapPilgrimLook(prefab);
            Assert.IsNotNull(look, $"{prefab.name}: no body sprite for the map");

            var classData = prefab.GetComponent<Game.Progression.PlayerClassManager>().ClassData;
            Assert.AreSame(classData.animatorController, look.controller, "The map plays the class data's animator, so new art arrives with it");
            Assert.IsNotNull(look.sprite);

            var visual = prefab.GetComponentInChildren<CharacterVisual>(true);
            Assert.AreEqual(visual.transform.lossyScale, look.scale, "The pilgrim is the same size as in a level");
            Assert.Less(look.feetOffset.y, 0f, "The feet are below the sprite's pivot");
            Assert.Less(look.feetOffset.magnitude, 1.5f, "The feet are on the body");

            // The marker drives these: idle facing the last heading, walking while it moves
            var controller = (AnimatorController)look.controller;
            var parameters = controller.parameters.ToDictionary(p => p.name, p => p.type);
            Assert.AreEqual(AnimatorControllerParameterType.Float, parameters["DirectionX"]);
            Assert.AreEqual(AnimatorControllerParameterType.Float, parameters["DirectionY"]);
            Assert.AreEqual(AnimatorControllerParameterType.Bool, parameters["Move"]);
        }

        // --- Scene -----------------------------------------------------------------------------------------

        [Test]
        public void Scene_MarkerStandsOnTheNode_AndWalksAtASaneSpeed()
        {
            string yaml = System.IO.File.ReadAllText("Assets/Scenes/LevelSelector.unity");
            StringAssert.Contains("playerOffset: {x: 0, y: 0}", yaml, "The marker's position is the pilgrim's feet: no offset from the node");

            var go = new GameObject("Renderer");
            try
            {
                var renderer = go.AddComponent<OverworldMapRenderer>();
                var so = new SerializedObject(renderer);
                float speed = so.FindProperty("walkSpeed").floatValue;
                float cap = so.FindProperty("maxWalkDuration").floatValue;
                Assert.That(speed, Is.InRange(0.5f, 20f));
                Assert.That(cap, Is.InRange(0.2f, 3f), "Long edges must not drag");
                Assert.IsFalse(renderer.IsWalking);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
