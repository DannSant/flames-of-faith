using Game.Combat;
using Game.Effects;
using Game.Effects.Abilities;
using Game.Scene;
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

        protected override void StartDashing()
        {
            base.StartDashing();
            ApplyChargeAbility();
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

        // Read on every dash, so an upgraded scroll applies from the next dash.
        private void ApplyChargeAbility()
        {
            if (hitboxDamage == null) return;

            CrusaderChargeAbility.Values charge = null;
            var store = PlayerManager.Instance != null ? PlayerManager.Instance.GetPlayerComponent<EffectStore>() : null;
            if (store != null && store.TryGetAbility(out CrusaderChargeAbility ability, out int tier))
            {
                charge = ability.GetTier(tier);
            }

            hitboxDamage.SetBaseDamage(charge != null ? charge.damage : defaultDamage);
            hitboxDamage.SetKnockbackForce(charge != null ? charge.knockbackForce : defaultKnockback);
        }
    }
}
