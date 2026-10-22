using Game.Combat;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>
    /// The player faces the weapon's per-shot target (and the sword orbits toward it) only while that enemy
    /// can still be hit. It used to stay set after the player walked away, so the character kept looking at
    /// an enemy across the level until another one came close.
    /// </summary>
    public class WeaponTargetTests
    {
        private const float Range = 3f;

        private static EnemyHealth Enemy(Vector2 position, int health = 10)
        {
            // No collider: the range check measures to the transform
            var enemy = new GameObject("Enemy").AddComponent<EnemyHealth>();
            enemy.transform.position = position;
            enemy.SetMaxHealth(health);
            return enemy;
        }

        [Test]
        public void AnEnemyInRange_IsFaced()
        {
            var enemy = Enemy(new Vector2(2f, 0f));
            try
            {
                Assert.AreSame(enemy, WeaponManager.TargetIfInRange(Vector2.zero, enemy, Range, false));
            }
            finally { Object.DestroyImmediate(enemy.gameObject); }
        }

        [Test]
        public void AnEnemyLeftBehind_IsNoLongerFaced()
        {
            var enemy = Enemy(new Vector2(20f, 0f));
            try
            {
                Assert.IsNull(WeaponManager.TargetIfInRange(Vector2.zero, enemy, Range, false),
                    "Out of weapon range: facing goes back to the aim");
            }
            finally { Object.DestroyImmediate(enemy.gameObject); }
        }

        [Test]
        public void ADeadEnemy_IsNoLongerFaced()
        {
            var enemy = Enemy(new Vector2(1f, 0f), health: 0);
            try
            {
                Assert.IsNull(WeaponManager.TargetIfInRange(Vector2.zero, enemy, Range, false));
            }
            finally { Object.DestroyImmediate(enemy.gameObject); }
        }

        [Test]
        public void NoTarget_IsNull()
        {
            Assert.IsNull(WeaponManager.TargetIfInRange(Vector2.zero, null, Range, false));
        }
    }
}
