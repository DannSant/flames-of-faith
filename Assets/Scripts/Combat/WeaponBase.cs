using Game.Control;
using UnityEngine;

namespace Game.Combat
{
    public abstract class WeaponBase : MonoBehaviour
    {
        [SerializeField] protected WeaponData weaponData;
        [SerializeField] protected WeaponData specialWeaponData;

        protected Health currentTarget;

        public abstract void Attack();
        public abstract void SpecialAttack();

        public virtual void Initialize(CharacterVisual characterVisual ) { }

        public virtual void SetTarget(Health target)
        {
            currentTarget = target;
        }
        public virtual Health GetTarget() => currentTarget;

        public abstract bool IsAttackTimerActive();
        public abstract bool IsSpecialAttackTimerActive();
        public abstract float GetAttackTimer();
        public abstract float GetSpecialAttackTimer();
        public abstract float GetAttackTimerDuration();
        public abstract float GetSpecialAttackTimerDuration();
        public abstract float GetWeaponRange();
        
    }
}
