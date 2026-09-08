using UnityEngine;

namespace Game.Combat
{
    [RequireComponent(typeof(DamageSourceBase))]
    public class DestroyObjectOnDamage : MonoBehaviour
    {
        [Tooltip("Delay before destroying the object after it deals damage, so an animation/VFX has time to play.")]
        [SerializeField] private float destroyDelay = 0.5f;

        private DamageSourceBase damageSource;
        private bool hasBeenTriggered = false;

        private void Awake()
        {
            damageSource = GetComponent<DamageSourceBase>();
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
            if (hasBeenTriggered) return;
            hasBeenTriggered = true;

            Destroy(gameObject, destroyDelay);
        }
    }
}
