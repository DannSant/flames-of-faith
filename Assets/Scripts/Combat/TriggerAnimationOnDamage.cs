using UnityEngine;

namespace Game.Combat
{
    [RequireComponent(typeof(DamageSourceBase))]
    public class TriggerAnimationOnDamage : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private string triggerName = "Hit";

        private DamageSourceBase damageSource;

        private void Awake()
        {
            damageSource = GetComponent<DamageSourceBase>();
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }

        private void OnEnable()
        {
            damageSource.OnDamageDealtEvent += HandleDamageDealt;
        }

        private void OnDisable()
        {
            damageSource.OnDamageDealtEvent -= HandleDamageDealt;
        }

        private void HandleDamageDealt(float damage, GameObject target)
        {
            if (animator != null)
            {
                animator.SetTrigger(triggerName);
            }
        }
    }
}
