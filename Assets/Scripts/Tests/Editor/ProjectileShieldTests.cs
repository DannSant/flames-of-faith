using Game.Combat;
using Game.Combat.Projectiles;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>The Whirlwind's holy shield only absorbs enemy projectiles, never enemy bodies.</summary>
    public class ProjectileShieldTests
    {
        private GameObject go;

        [TearDown]
        public void TearDown()
        {
            if (go != null) Object.DestroyImmediate(go);
        }

        [Test]
        public void Null_IsNotAProjectile()
        {
            Assert.IsFalse(PlayerHealth.IsEnemyProjectile(null));
        }

        [Test]
        public void LegacyMovement_IsAProjectile()
        {
            go = new GameObject("Shot", typeof(ProjectileMovement));
            Assert.IsTrue(PlayerHealth.IsEnemyProjectile(go));
        }

        [Test]
        public void NewMovement_IsAProjectile()
        {
            go = new GameObject("Shot", typeof(ProjectileMoveLinear));
            Assert.IsTrue(PlayerHealth.IsEnemyProjectile(go));
        }

        [Test]
        public void SomethingWithoutMovement_IsNotAProjectile()
        {
            // e.g. a ground explosion
            go = new GameObject("Explosion", typeof(EnemyTriggerDamage));
            Assert.IsFalse(PlayerHealth.IsEnemyProjectile(go));
        }

        [Test]
        public void AnEnemyBody_IsNotAProjectile()
        {
            go = new GameObject("Enemy", typeof(ProjectileMovement));
            go.AddComponent<EnemyHealth>();
            Assert.IsFalse(PlayerHealth.IsEnemyProjectile(go));
        }
    }
}
