using UnityEngine;

namespace Game.Control
{
    public class WarriorDash : DashBase
    {
        [Header("Warrior Dash - Melee Impact")]
        [Tooltip("Child GameObject with a trigger Collider2D + DamageSourceBase, toggled on for the dash's duration.")]
        [SerializeField] private GameObject dashHitbox;

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
