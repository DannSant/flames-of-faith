using Game.RunEncounters;
using Game.Saving;
using Game.Scene;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>
    /// No event repeats in a Pilgrimage: the pick skips seen events, is the same for the same run and node (so
    /// leaving and continuing doesn't re-roll), falls back once the pool is used up (never to a once-only event),
    /// and the seen list survives the run save.
    /// </summary>
    public class EventPoolTests
    {
        private const string EventsFolder = "Assets/Resources/RunEncounters/EventEncounter";

        private static EventEncounterData Event(string id, bool once = false)
        {
            var e = ScriptableObject.CreateInstance<EventEncounterData>();
            e.name = id + "Encounter";
            e.eventName = id;
            e.oncePerPilgrimage = once;
            return e;
        }

        private static List<EventEncounterData> Pool() =>
            new() { Event("a"), Event("b"), Event("c"), Event("liah", once: true) };

        [Test]
        public void Pick_SkipsSeenEvents()
        {
            var pool = Pool();
            var seen = new HashSet<string> { "a", "b", "liah" };
            for (int node = 0; node < 20; node++)
            {
                Assert.AreEqual("c", EventPool.Pick(pool, seen, 42, $"n{node}").eventName);
            }
        }

        [Test]
        public void Pick_IsTheSameForTheSameRunAndNode()
        {
            var pool = Pool();
            var first = EventPool.Pick(pool, new HashSet<string>(), 1234, "node_7");
            for (int i = 0; i < 5; i++)
            {
                Assert.AreSame(first, EventPool.Pick(pool, new HashSet<string>(), 1234, "node_7"));
            }
        }

        [Test]
        public void Pick_VariesAcrossNodesAndSeeds()
        {
            var pool = Pool();
            var picked = new HashSet<string>();
            for (int seed = 0; seed < 50; seed++)
            {
                picked.Add(EventPool.Pick(pool, null, seed, "node_1").eventName);
            }
            Assert.AreEqual(4, picked.Count, "Every event should come up for some seed");
        }

        [Test]
        public void UsedUpPool_RepeatsEvents_ButNeverAOnceOnlyOne()
        {
            var pool = Pool();
            var seen = new HashSet<string> { "a", "b", "c", "liah" };
            for (int seed = 0; seed < 50; seed++)
            {
                var e = EventPool.Pick(pool, seen, seed, "node");
                Assert.IsNotNull(e);
                Assert.AreNotEqual("liah", e.eventName);
            }
        }

        [Test]
        public void SingleEventList_AlwaysGivesItsEvent()
        {
            var shrine = Event("holyAltar");
            var list = new List<EventEncounterData> { shrine };
            Assert.AreSame(shrine, EventPool.Pick(list, new HashSet<string> { "holyAltar" }, 9, "node"));
            Assert.IsNull(EventPool.Pick(new List<EventEncounterData>(), null, 9, "node"));
        }

        [Test]
        public void IdOf_FallsBackToTheAssetName()
        {
            var e = Event("x");
            e.eventName = "";
            Assert.AreEqual("xEncounter", EventPool.IdOf(e));
        }

        [Test]
        public void StableHash_IsFixed()
        {
            // FNV-1a of "" and "a": a change here would re-roll every saved run's events
            Assert.AreEqual(2166136261u, EventPool.StableHash(""));
            Assert.AreEqual(0xE40C292Cu, EventPool.StableHash("a"));
        }

        [Test]
        public void EveryEvent_HasAUniqueId()
        {
            var ids = AssetDatabase.FindAssets("t:EventEncounterData", new[] { EventsFolder })
                .Select(g => AssetDatabase.LoadAssetAtPath<EventEncounterData>(AssetDatabase.GUIDToAssetPath(g)))
                .Select(EventPool.IdOf)
                .ToList();
            Assert.IsNotEmpty(ids);
            Assert.AreEqual(ids.Count, ids.Distinct().Count(), $"Duplicate event ids: {string.Join(", ", ids)}");
        }

        [Test]
        public void SeenEvents_SurviveTheRunSave()
        {
            var go = new GameObject("GameSessionTest");
            try
            {
                var session = go.AddComponent<GameSession>();
                session.ResetPlayerData();
                session.MarkEventSeen("arcaneBox");
                session.MarkEventSeen("liah");
                session.MarkEventSeen("arcaneBox");

                var json = JsonUtility.ToJson(session.ToSaveData());
                var loaded = JsonUtility.FromJson<RunSaveData>(json);
                CollectionAssert.AreEqual(new[] { "arcaneBox", "liah" }, loaded.seenEvents);

                session.Initialize();
                Assert.IsEmpty(session.SeenEvents, "A new run forgets the seen events");

                session.ApplySaveData(loaded);
                CollectionAssert.AreEqual(new[] { "arcaneBox", "liah" }, session.SeenEvents);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void OlderSave_WithoutSeenEvents_LoadsEmpty()
        {
            var loaded = JsonUtility.FromJson<RunSaveData>("{\"saveVersion\":2,\"levelsBeaten\":3}");
            Assert.IsNotNull(loaded.seenEvents);
            Assert.IsEmpty(loaded.seenEvents);
        }
    }
}
