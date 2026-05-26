using Game.Combat;
using UnityEngine;

namespace Game.Combat
{
    public class DamageRequest
    {
        public float baseDamage;
        public WeaponClass originWeaponClass;
        public bool canTriggerLifeSteal = true;

        public DamageRequest(float baseDamage, WeaponClass weaponClass, bool canTriggerLifeSteal)
        {
            this.baseDamage = baseDamage;
            this.originWeaponClass = weaponClass;
            this.canTriggerLifeSteal = canTriggerLifeSteal;
        }
    }
    public interface IDamageable
    {
        public void TakeDamage(DamageRequest damageRequest);
        public bool ShouldSpawnDamageNumber();
        public bool ShouldSpawnEffectObject();

        public bool IsImmune();
    }

}