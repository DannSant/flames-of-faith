using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// Stuns whatever a <see cref="DamageSourceBase"/> damages. A sibling component so the stun is
    /// opt-in per prefab and any damage source - trap, projectile, weapon hitbox - can carry it.
    /// </summary>
    [RequireComponent(typeof(DamageSourceBase))]
    public class ApplyStunOnDamage : MonoBehaviour
    {
        [SerializeField] private float stunDuration = 2f;

        [Tooltip("Also stun enemies caught by an AreaDamageOnHit on this object, not just the one " +
            "hit directly. This component has no radius of its own - it stuns whoever got damaged, " +
            "so the reach is whatever AreaDamageOnHit's Radius is set to.")]
        [SerializeField] private bool stunAreaDamageTargets = true;

        private IDamageDealtNotifier[] notifiers;

        private void Awake()
        {
            notifiers = stunAreaDamageTargets
                ? GetComponents<IDamageDealtNotifier>()
                : new IDamageDealtNotifier[] { GetComponent<DamageSourceBase>() };
        }

        private void OnEnable()
        {
            foreach (var notifier in notifiers)
            {
                notifier.OnDamageDealtEvent += HandleDamageDealt;
            }
        }

        private void OnDisable()
        {
            foreach (var notifier in notifiers)
            {
                notifier.OnDamageDealtEvent -= HandleDamageDealt;
            }
        }

        private void HandleDamageDealt(float damage, GameObject target)
        {
            if (target == null) return;

            // Enemy variants can wrap EnemyBase as a child, so the damaged GameObject is not
            // necessarily the one holding the handler.
            var stunHandler = target.GetComponentInParent<StunHandler>();
            if (stunHandler != null)
            {
                stunHandler.ApplyStun(stunDuration);
            }
        }
    }
}
