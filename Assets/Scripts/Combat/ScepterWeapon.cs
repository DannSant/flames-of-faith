using Game.Audio;
using Game.Combat.Projectiles;
using Game.Progression;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Combat
{
    public class ScepterWeapon : WeaponBase
    {
        [Header("Sound Effects")]
        [SerializeField] private List<AudioClip> scepterAttackSounds = new();

        [Header("Projectile")]
        [SerializeField] private Transform projectileSpawnTransform;

        [Header("Special Attack")]
        [Tooltip("Distance from the player each of the 4 special projectiles (N/S/E/W) spawns at.")]
        [SerializeField] private float specialProjectileOffset = 0.75f;
        [Tooltip("Radius used to find the closest target for each special projectile, relative to that projectile's own spawn position.")]
        [SerializeField] private float specialAttackTargetSearchRadius = 15f;

        public event System.Action<DamageSourceBase> onScepterAttackLaunched;
        public event System.Action<DamageSourceBase> onScepterSpecialAttackLaunched;

        private EnemyHealth pendingAttackTarget;

        public override void Attack()
        {
            if (!attackTimer.GetIsEventActive())
            {
                if (currentTarget == null) return;
                pendingAttackTarget = currentTarget;
                PlayRandomScepterSound();
                characterVisual.PlayAttackAnimation();
                attackTimer.StartEvent();
            }
        }

        public override void SpecialAttack()
        {
            if (specialAttackTimer.GetIsEventActive()) return;

            playerHealth.ToggleIsInvulnerable(true);
            characterVisual.PlayAttackSpecialAnimation();
            specialAttackTimer.StartEvent();
            effectStore?.Trigger(Effects.EffectTrigger.OnSpecialAttack);
        }

        protected override void OnAttackAnimationPlayed()
        {
            if (pendingAttackTarget == null || pendingAttackTarget.IsDead()) return;

            Vector2 spawnPos = projectileSpawnTransform.position;
            Vector2 direction = ((Vector2)pendingAttackTarget.transform.position - spawnPos).normalized;

            var go = Instantiate(weaponData.projectilePrefab, spawnPos, Quaternion.identity);

            var move = go.GetComponent<ProjectileMovementBase>();
            move.Initialize(direction);
            move.SetTarget(pendingAttackTarget.transform);

            var damage = go.GetComponent<DamageSourceBase>();
            damage.Initialize(weaponData.baseDamage, weaponData.pierceAmount, null, weaponData.weaponClass, weaponData);

            onScepterAttackLaunched?.Invoke(damage);
        }

        protected override void OnSpecialAttackAnimationPlayed()
        {
            playerHealth.ToggleIsInvulnerable(false);

            Vector2 originPos = transform.position;

            SpawnSpecialProjectile(originPos + Vector2.up * specialProjectileOffset);
            SpawnSpecialProjectile(originPos + Vector2.down * specialProjectileOffset);
            SpawnSpecialProjectile(originPos + Vector2.left * specialProjectileOffset);
            SpawnSpecialProjectile(originPos + Vector2.right * specialProjectileOffset);
        }

        private void SpawnSpecialProjectile(Vector2 spawnPos)
        {
            int pierceAmount = specialWeaponData.pierceAmount + playerProgression.GetStatTotal(StatType.PierceAmount);
            EnemyHealth target = FindClosestEnemyToPosition(spawnPos, specialAttackTargetSearchRadius);
            Vector2 direction = target != null
                ? ((Vector2)target.transform.position - spawnPos).normalized
                : Random.insideUnitCircle.normalized;

            var go = Instantiate(specialWeaponData.projectilePrefab, spawnPos, Quaternion.identity);

            var move = go.GetComponent<ProjectileMovementBase>();
            move.Initialize(direction);
            if (target != null)
            {
                move.SetTarget(target.transform);
            }

            var damage = go.GetComponent<DamageSourceBase>();
            damage.Initialize(specialWeaponData.baseDamage, pierceAmount, null, specialWeaponData.weaponClass, specialWeaponData);

            onScepterSpecialAttackLaunched?.Invoke(damage);
        }

        private EnemyHealth FindClosestEnemyToPosition(Vector2 position, float radius)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(position, radius, LayerMask.GetMask("Enemy", "Boss"));

            EnemyHealth closest = null;
            float closestDistanceSqr = Mathf.Infinity;

            foreach (var hit in hits)
            {
                EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
                if (enemy == null || enemy.IsImmune() || enemy.IsDead())
                {
                    continue;
                }

                float distanceSqr = ((Vector2)enemy.transform.position - position).sqrMagnitude;
                if (distanceSqr < closestDistanceSqr)
                {
                    closest = enemy;
                    closestDistanceSqr = distanceSqr;
                }
            }

            return closest;
        }

        public override float GetWeaponRange()
        {
            float playerRange = playerProgression.GetStatTotal(StatType.Range);
            return weaponData.rangeBase + playerRange;
        }

        private void PlayRandomScepterSound()
        {
            if (scepterAttackSounds.Count == 0) return;
            var audioClip = scepterAttackSounds[Random.Range(0, scepterAttackSounds.Count)];
            AudioManager.Instance.PlaySFX(audioClip, true);
        }
    }
}
