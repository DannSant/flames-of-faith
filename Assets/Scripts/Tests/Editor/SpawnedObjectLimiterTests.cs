using System.Collections.Generic;
using Game.Common;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public class SpawnedObjectLimiterTests
    {
        private readonly List<GameObject> created = new();

        [TearDown]
        public void TearDown()
        {
            foreach (var go in created)
            {
                if (go != null) Object.DestroyImmediate(go);
            }
            created.Clear();
        }

        private GameObject Make(string name)
        {
            var go = new GameObject(name);
            created.Add(go);
            return go;
        }

        [Test]
        public void UnderTheLimit_NothingIsEvicted()
        {
            var limiter = new SpawnedObjectLimiter(3);

            Assert.IsEmpty(limiter.Register(Make("a")));
            Assert.IsEmpty(limiter.Register(Make("b")));
            Assert.IsEmpty(limiter.Register(Make("c")));
            Assert.AreEqual(3, limiter.Count);
        }

        [Test]
        public void OverTheLimit_TheOldestIsEvicted()
        {
            var limiter = new SpawnedObjectLimiter(3);
            var first = Make("first");
            limiter.Register(first);
            limiter.Register(Make("second"));
            limiter.Register(Make("third"));

            var evicted = limiter.Register(Make("fourth"));

            Assert.AreEqual(1, evicted.Count);
            Assert.AreSame(first, evicted[0]);
            Assert.AreEqual(3, limiter.Count);
        }

        [Test]
        public void EvictionKeepsGoingOldestFirst()
        {
            var limiter = new SpawnedObjectLimiter(2);
            var a = Make("a");
            var b = Make("b");
            limiter.Register(a);
            limiter.Register(b);

            Assert.AreSame(a, limiter.Register(Make("c"))[0]);
            Assert.AreSame(b, limiter.Register(Make("d"))[0]);
        }

        [Test]
        public void ObjectsDestroyedElsewhere_FreeTheirSlot()
        {
            var limiter = new SpawnedObjectLimiter(3);
            var triggered = Make("triggered");
            limiter.Register(triggered);
            limiter.Register(Make("b"));
            limiter.Register(Make("c"));

            Object.DestroyImmediate(triggered); // e.g. a trap that went off

            Assert.AreEqual(2, limiter.Count);
            Assert.IsEmpty(limiter.Register(Make("d")), "the destroyed one no longer counts");
        }

        [Test]
        public void LimitOfOne_AlwaysKeepsOnlyTheNewest()
        {
            var limiter = new SpawnedObjectLimiter(1);
            var a = Make("a");
            limiter.Register(a);

            var evicted = limiter.Register(Make("b"));

            Assert.AreSame(a, evicted[0]);
            Assert.AreEqual(1, limiter.Count);
        }

        [Test]
        public void LoweringTheLimit_EvictsTheExtraOnTheNextRegister()
        {
            var limiter = new SpawnedObjectLimiter(3);
            limiter.Register(Make("a"));
            limiter.Register(Make("b"));
            limiter.Register(Make("c"));

            limiter.SetMaxCount(2);
            var evicted = limiter.Register(Make("d"));

            Assert.AreEqual(2, evicted.Count);
            Assert.AreEqual(2, limiter.Count);
        }

        [Test]
        public void LimitBelowOne_IsTreatedAsOne()
        {
            var limiter = new SpawnedObjectLimiter(0);
            limiter.Register(Make("a"));

            Assert.AreEqual(1, limiter.Register(Make("b")).Count);
        }
    }
}
