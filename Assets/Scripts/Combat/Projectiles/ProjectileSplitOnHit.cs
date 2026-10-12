using Game.Progression;
using Game.Scene;
using UnityEngine;

namespace Game.Combat.Projectiles
{
    /// <summary>
    /// Pierce stat bonus for linear arrows: each enemy hit has a chance (PierceSettings) to break
    /// off a second arrow of the same type, at reduced damage, in a random direction. Split arrows
    /// never split again. Put it only on linear player projectiles (the Archer's arrows, Strafe).
    /// </summary>
    [RequireComponent(typeof(DamageSourceBase))]
    public class ProjectileSplitOnHit : MonoBehaviour
    {
        private DamageSourceBase damageSource;
        private bool isSplit;

        private void Awake()
        {
            damageSource = GetComponent<DamageSourceBase>();
            damageSource.OnDamageDealtEvent += HandleDamageDealt;
        }

        private void OnDestroy()
        {
            if (damageSource != null)
            {
                damageSource.OnDamageDealtEvent -= HandleDamageDealt;
            }
        }

        private void HandleDamageDealt(float damage, GameObject target)
        {
            if (isSplit || damage <= 0f) return;

            var progression = PlayerManager.Instance != null
                ? PlayerManager.Instance.GetPlayerComponent<PlayerProgression>()
                : null;
            if (progression == null) return;

            var settings = PierceSettings.Instance;
            float chance = settings.GetSplitChance(progression.GetStatTotal(StatType.Pierce));
            if (chance <= 0f || Random.value >= chance) return;

            Split(target, settings);
        }

        private void Split(GameObject hitTarget, PierceSettings settings)
        {
            var clone = Instantiate(gameObject, transform.position, Quaternion.identity);

            var cloneSplit = clone.GetComponent<ProjectileSplitOnHit>();
            if (cloneSplit != null) cloneSplit.isSplit = true;

            var cloneDamage = clone.GetComponent<DamageSourceBase>();
            int splitDamage = Mathf.Max(1, Mathf.RoundToInt(damageSource.BaseDamage * settings.SplitDamageMultiplier));
            cloneDamage.SetBaseDamage(splitDamage);
            cloneDamage.ConfigurePierce(true, 1);
            // It spawns inside the enemy just hit; don't let it hit that one again.
            cloneDamage.IgnoreTarget(hitTarget);

            var move = clone.GetComponent<ProjectileMovementBase>();
            if (move != null)
            {
                Vector2 direction = Random.insideUnitCircle;
                if (direction.sqrMagnitude < 0.0001f) direction = Vector2.right;
                move.Initialize(direction.normalized);
                move.SetTarget(null); // no course correction
                move.SetMaxTravelDistance(settings.SplitMaxTravelDistance);
            }
        }
    }
}
