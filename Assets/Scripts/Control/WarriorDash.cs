using Game.Combat;
using Game.Effects.Abilities;
using UnityEngine;

namespace Game.Control
{
    public class WarriorDash : DashBase
    {
        [Header("Warrior Dash - Melee Impact")]
        [Tooltip("Child GameObject with a trigger Collider2D + DamageSourceBase, toggled on for the dash's duration.")]
        [SerializeField] private GameObject dashHitbox;

        private DamageSourceBase hitboxDamage;
        // The hitbox's own values, used when the player has no Scroll of Crusader Charge.
        private int defaultDamage;
        private float defaultKnockback;

        protected override void Awake()
        {
            base.Awake();
            hitboxDamage = dashHitbox != null ? dashHitbox.GetComponent<DamageSourceBase>() : null;
            if (hitboxDamage != null)
            {
                defaultDamage = hitboxDamage.BaseDamage;
                defaultKnockback = hitboxDamage.KnockbackForce;
            }
        }

        protected override void ApplyAbilityItems()
        {
            if (hitboxDamage == null) return;

            var charge = GetAbilityValues<CrusaderChargeAbility, CrusaderChargeAbility.Values>();
            hitboxDamage.SetBaseDamage(charge != null ? charge.damage : defaultDamage);
            hitboxDamage.SetKnockbackForce(charge != null ? charge.knockbackForce : defaultKnockback);
        }

        protected override void StartDashing()
        {
            base.StartDashing();
            if (dashHitbox != null)
            {
                dashHitbox.SetActive(true);
            }
        }

        protected override void EndDashing()
        {
            base.EndDashing();
            if (dashHitbox != null)
            {
                dashHitbox.SetActive(false);
            }
        }
    }
}
