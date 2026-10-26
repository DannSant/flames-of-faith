using Game.Combat;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>
    /// How an invulnerable enemy (the boss's first phase) reads to the player (tester feedback, 2026-10-25): no
    /// transparency on the boss, and IMMUNE instead of a 0 on a hit, at most once per interval so fast repeated
    /// hits don't stack a pile of them.
    /// </summary>
    public class ImmuneFeedbackTests
    {
        private const string BossPath = "Assets/Prefabs/Enemies/BossAct1.prefab";

        [Test]
        public void FirstHit_ShowsImmune()
        {
            Assert.IsTrue(EnemyHealth.ShouldShowImmuneNumber(0f, float.NegativeInfinity, 0.4f));
        }

        [TestCase(10.1f, false)]
        [TestCase(10.39f, false)]
        [TestCase(10.45f, true)]
        [TestCase(11f, true)]
        public void LaterHits_ShowImmune_OncePerInterval(float now, bool shows)
        {
            Assert.AreEqual(shows, EnemyHealth.ShouldShowImmuneNumber(now, 10f, 0.4f));
        }

        [Test]
        public void NoInterval_ShowsEveryHit()
        {
            Assert.IsTrue(EnemyHealth.ShouldShowImmuneNumber(10f, 10f, 0f));
        }

        [Test]
        public void Boss_IsOpaqueWhileInvulnerable()
        {
            var boss = AssetDatabase.LoadAssetAtPath<GameObject>(BossPath);
            Assert.IsNotNull(boss, $"Missing {BossPath}");
            var renderer = boss.GetComponentInChildren<Game.Boss.BossRenderer>(true);
            Assert.IsNotNull(renderer, "BossAct1 has no BossRenderer");
            var toggle = new SerializedObject(renderer).FindProperty("enableFirstPhaseTransparency");
            Assert.IsNotNull(toggle, "BossRenderer.enableFirstPhaseTransparency was renamed");
            Assert.IsFalse(toggle.boolValue,
                "Testers disliked the see-through boss in its first phase; invulnerability is shown another way");
        }
    }
}
