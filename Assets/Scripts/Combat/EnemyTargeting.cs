using System.Collections.Generic;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// The one place "which enemy is closest to this point" is answered. WeaponManager,
    /// ScepterWeapon and DebuffLeyCharge each had their own verbatim copy of this loop, and each
    /// copy carried the same two flaws.
    ///
    /// Flaw one: the overlap query is a *shape* query - it accepts a collider whose outline
    /// intersects the circle - but the ranking compared transform positions, so detection measured
    /// to the enemy's edge while selection measured to its centre. A boss whose body was touching
    /// the player lost to a slime several units away, because the boss's centre is further out.
    ///
    /// Flaw two: the physics colliders on the big enemies are much smaller than their sprites
    /// (BossAct1 draws 6x6 world units but collides as a circle of radius 0.98), so measuring to
    /// the collider still left the player closing one to two extra units before the game agreed
    /// they were in range - while against a slime, whose collider does match its sprite, the stat
    /// read true. That is the inconsistency testers reported as "less range than it says".
    /// <see cref="EnemyHealth.TargetingBodyRadius"/> is the dial that closes that gap, without
    /// touching the colliders themselves (widening those would block the player two units from the
    /// boss and change knockback, pathing and enemy attack contacts).
    /// </summary>
    public static class EnemyTargeting
    {
        /// <summary>
        /// Largest <see cref="EnemyHealth.TargetingBodyRadius"/> any enemy is expected to declare.
        /// The broad phase runs at range + this, so a big enemy is still a candidate while its
        /// collider sits outside the range but its visible body does not. Raising an enemy's body
        /// radius past this silently puts it out of reach again, so the mismatch is logged.
        /// </summary>
        public const float MaxTargetingBodyRadius = 3f;

        private static readonly List<Collider2D> overlapResults = new();
        private static ContactFilter2D contactFilter;
        private static bool contactFilterReady;

#if UNITY_EDITOR
        private static readonly HashSet<EnemyHealth> warnedAboutBodyRadius = new();
#endif

        /// <summary>
        /// The closest live, non-immune enemy whose body is within <paramref name="range"/> of
        /// <paramref name="origin"/>, measured to the enemy's surface rather than its centre.
        /// </summary>
        /// <param name="compensateForBodySize">
        /// Whether a large enemy's visible body counts as part of its reach. Off for weapons whose
        /// real hit comes from a collider rather than the acquisition radius - see
        /// <see cref="WeaponData.compensateForEnemyBodySize"/>.
        /// </param>
        /// <param name="excluded">Optional enemies to skip, e.g. a chain that has already hopped through them.</param>
        public static EnemyHealth FindClosest(
            Vector2 origin,
            float range,
            bool compensateForBodySize,
            HashSet<EnemyHealth> excluded = null)
        {
            if (range <= 0f) return null;

            EnsureContactFilter();

            // Pad the query so an enemy whose collider is outside `range` but whose body reaches
            // inside it is still a candidate. Every candidate is re-gated on the real distance
            // below, so the padding only widens the search, never the reach.
            float queryRadius = range + (compensateForBodySize ? MaxTargetingBodyRadius : 0f);

            overlapResults.Clear();
            Physics2D.OverlapCircle(origin, queryRadius, contactFilter, overlapResults);

            EnemyHealth closest = null;
            float closestDistance = Mathf.Infinity;

            foreach (var hit in overlapResults)
            {
                // GetComponent, not GetComponentInParent: only the body collider defines the
                // surface. EnemyLightDevourer parents a melee hitbox on the Enemy layer, and
                // letting that count would silently inflate that one enemy's reach -
                // TargetingBodyRadius is the explicit dial for body size instead.
                EnemyHealth enemy = hit.GetComponent<EnemyHealth>();

                if (enemy == null || enemy.IsImmune() || enemy.IsDead()) continue;
                if (excluded != null && excluded.Contains(enemy)) continue;

                float distance = EffectiveDistance(origin, hit, enemy, compensateForBodySize);
                if (distance > range) continue;

                if (distance < closestDistance)
                {
                    closest = enemy;
                    closestDistance = distance;
                }
            }

            return closest;
        }

        /// <summary>
        /// How far <paramref name="origin"/> is from the enemy's body, taking the nearer of its
        /// collider outline and its declared visible body.
        /// </summary>
        private static float EffectiveDistance(Vector2 origin, Collider2D hit, EnemyHealth enemy, bool compensateForBodySize)
        {
            // Distance to the collider outline - the same thing the overlap query tested against,
            // so ranking by it keeps detection and selection consistent. Also handles the
            // colliders' vertical offset for free (the boss's is 0.33 below its transform), which
            // a centre-minus-radius measure cannot. Returns 0 when origin is inside the collider.
            float surfaceDistance = Vector2.Distance(origin, hit.ClosestPoint(origin));

            if (!compensateForBodySize) return surfaceDistance;

            float bodyRadius = enemy.TargetingBodyRadius;
            if (bodyRadius <= 0f) return surfaceDistance;

            WarnIfBodyRadiusExceedsBroadPhase(enemy, bodyRadius);

            float bodyDistance = Vector2.Distance(origin, enemy.transform.position) - bodyRadius;

            // Min, never max: an enemy in range by its collider today stays in range, so a body
            // radius can only ever add reach.
            return Mathf.Min(surfaceDistance, bodyDistance);
        }

        private static void EnsureContactFilter()
        {
            if (contactFilterReady) return;

            // Layer names are resolved once rather than per call - GetMask does a string lookup,
            // and this runs every frame from WeaponManager's auto-attack.
            contactFilter = new ContactFilter2D { useTriggers = true };
            contactFilter.SetLayerMask(LayerMask.GetMask("Enemy", "Boss"));
            contactFilterReady = true;
        }

        private static void WarnIfBodyRadiusExceedsBroadPhase(EnemyHealth enemy, float bodyRadius)
        {
#if UNITY_EDITOR
            if (bodyRadius <= MaxTargetingBodyRadius) return;

            // This runs per candidate per frame, so warn once per enemy rather than flooding.
            if (!warnedAboutBodyRadius.Add(enemy)) return;

            Debug.LogWarning(
                $"{enemy.name} declares a TargetingBodyRadius of {bodyRadius}, larger than " +
                $"{nameof(EnemyTargeting)}.{nameof(MaxTargetingBodyRadius)} ({MaxTargetingBodyRadius}). " +
                "The broad-phase query is only padded by that constant, so the extra reach is " +
                "partly ignored - raise the constant or lower the radius.",
                enemy);
#endif
        }
    }
}
