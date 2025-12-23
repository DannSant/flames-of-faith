using Game.Combat;
using UnityEngine;

public interface IDamageable 
{
    public void TakeDamage(float damage, WeaponClass weaponClass);
    public bool ShouldSpawnDamageNumber();
    public bool ShouldSpawnEffectObject();
}
