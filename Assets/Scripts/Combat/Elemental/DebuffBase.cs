using UnityEngine;

namespace Game.Combat.Elemental
{
    public abstract class DebuffBase : MonoBehaviour
    {
        protected EnemyHealth enemyHealth;
       
        protected float duration;
        protected float strength; // how strong is this effect

        protected virtual void Awake()
        {
            enemyHealth = GetComponent<EnemyHealth>();            
        }

        /// <summary>
        /// Called right after AddComponent() by the debuff applicator.
        /// </summary>
        public virtual void Initialize(float duration, float strength)
        {
            this.duration = duration;
            this.strength = strength;
        }

        /// <summary>
        /// Called when the debuff is overwritten or ended.
        /// </summary>
        public virtual void End() { }

        protected virtual void Update()
        {
            duration -= Time.deltaTime;
            if (duration <= 0f)
            {
                End();
                Destroy(this);
            }
        }
    }

}