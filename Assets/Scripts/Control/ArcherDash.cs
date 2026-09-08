using UnityEngine;

namespace Game.Control
{
    public class ArcherDash : DashBase
    {
        [Header("Archer Dash - Trap")]
        [Tooltip("Prefab with a trigger Collider2D + DamageSourceBase + TriggerAnimationOnDamage + DestroyObjectOnDamage (+ DestroyAfterTime as a never-hit safety net), spawned at the dash's start position.")]
        [SerializeField] private GameObject trapPrefab;

        protected override void StartDashing()
        {
            base.StartDashing();
            if (trapPrefab != null)
            {
                Instantiate(trapPrefab, transform.position, Quaternion.identity);
            }
        }
    }
}
