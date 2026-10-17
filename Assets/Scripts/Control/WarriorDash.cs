using Game.Combat;
using Game.Effects.Abilities;
using UnityEngine;

namespace Game.Control
{
    public class WarriorDash : DashBase
    {
        [Header("Warrior Dash - Melee Impact")]
        [Tooltip("Child GameObject with a trigger Collider2D + DamageSourceBase (WeaponClass Melee), toggled on for " +
            "the dash's duration. Its collider must contain the player's solid body collider with a margin: enemies " +
            "only have a solid feet collider, so a smaller hitbox is stopped by the bodies colliding and never hits.")]
        [SerializeField] private GameObject dashHitbox;

        private DamageSourceBase hitboxDamage;
        // The hitbox's own values, used when the player has no Scroll of Crusader Charge.
        private int defaultDamage;
        private float defaultKnockback;
        private float defaultAttackScale;

        protected override void Awake()
        {
            base.Awake();
            hitboxDamage = dashHitbox != null ? dashHitbox.GetComponent<DamageSourceBase>() : null;
            if (hitboxDamage != null)
            {
                defaultDamage = hitboxDamage.BaseDamage;
                defaultKnockback = hitboxDamage.KnockbackForce;
                defaultAttackScale = hitboxDamage.AttackScale;
            }
        }

        protected override void ApplyAbilityItems()
        {
            if (hitboxDamage == null) return;

            var charge = GetAbilityValues<CrusaderChargeAbility, CrusaderChargeAbility.Values>();
            hitboxDamage.SetBaseDamage(charge != null ? charge.damage : defaultDamage);
            hitboxDamage.SetKnockbackForce(charge != null ? charge.knockbackForce : defaultKnockback);
            // The hitbox is WeaponClass.Melee, so its attack scale is its share of Melee Damage.
            hitboxDamage.SetAttackScale(charge != null ? charge.meleeDamageScale : defaultAttackScale);
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
