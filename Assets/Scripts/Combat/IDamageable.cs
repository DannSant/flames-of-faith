using Game.Combat;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Combat
{
    public class DamageRequest
    {
        public float baseDamage;
        public WeaponClass originWeaponClass;
        public bool canTriggerLifeSteal = true;
        public bool silent = false; // Skips hit/death SFX, used for mass/forced kills
        public int energyChainDepth = 0; // How many Energy debuff jumps this damage is already the result of
        public HashSet<EnemyHealth> energyChainVisited = null; // Enemies already hit in this Energy chain, to prevent revisiting

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

        public bool IsDead();
    }

}