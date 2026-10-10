using Game.Audio;
using Game.Combat.Projectiles;
using Game.Common;
using Game.Progression;
using Game.Waves;
using System.Collections;
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

        [Header("Special: Explosions On Target")]
        [Tooltip("Explosion spawned on the marked target during the special. Default: FireballExplotion.")]
        [SerializeField] private GameObject targetExplosionPrefab;
        [Tooltip("Seconds between explosions. The first one goes off as soon as the special is cast.")]
        [SerializeField] private float targetExplosionInterval = 2.5f;
        [Tooltip("Seconds the explosions keep going after the special is cast.")]
        [SerializeField] private float targetExplosionDurationBase = 3f;
        [Tooltip("Extra seconds per point of the Skill Duration stat (same idea as the tornado's LifetimeByStat).")]
        [SerializeField] private float targetExplosionDurationPerSkillDuration = 1f;
        [Tooltip("Explosion damage as a fraction of the special's base damage.")]
        [SerializeField] private float targetExplosionDamageMultiplier = 0.5f;

        private Coroutine targetExplosionsRoutine;

        public event System.Action<DamageSourceBase> onScepterAttackLaunched;
        public event System.Action<DamageSourceBase> onScepterSpecialAttackLaunched;

        private EnemyHealth pendingAttackTarget;

        public override void Attack()
        {
            if (!attackTimer.GetIsEventActive())
            {
                // May be null: the fireball is then fired along the aim instead (see OnAttackAnimationPlayed).
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
            // A target that died during the wind-up no longer drops the shot: it goes out along the aim.
            bool hasTarget = pendingAttackTarget != null && !pendingAttackTarget.IsDead();

            Vector2 spawnPos = projectileSpawnTransform.position;
            Vector2 direction = hasTarget
                ? ((Vector2)pendingAttackTarget.transform.position - spawnPos).normalized
                : GetFreeAimDirection();

            var go = Instantiate(weaponData.projectilePrefab, spawnPos, Quaternion.identity);

            var move = go.GetComponent<ProjectileMovementBase>();
            move.Initialize(direction);
            if (hasTarget)
            {
                move.SetTarget(pendingAttackTarget.transform);
            }
            else
            {
                // With no target the homing movement flies straight, so cap it to the range.
                move.SetMaxTravelDistance(GetFreeAimMaxDistance());
            }

            var damage = go.GetComponent<DamageSourceBase>();
            damage.Initialize(weaponData.baseDamage, weaponData.pierceAmount, null, weaponData.weaponClass, weaponData);
            damage.OnDamageDealtEvent += HandleProjectileDamageDealt;

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

            if (targetExplosionPrefab != null)
            {
                if (targetExplosionsRoutine != null) StopCoroutine(targetExplosionsRoutine);
                targetExplosionsRoutine = StartCoroutine(TargetExplosionsRoutine());
            }
        }

        /// <summary>
        /// Every interval, for a fixed duration, explodes on whatever the marked target is at that
        /// moment (it follows target changes). Ticks with no target are skipped. Time doesn't
        /// count during a gameplay freeze or the wave-end sequence.
        /// </summary>
        private IEnumerator TargetExplosionsRoutine()
        {
            float duration = targetExplosionDurationBase
                + playerProgression.GetStatTotal(StatType.SkillDuration) * targetExplosionDurationPerSkillDuration;
            float elapsed = 0f;
            float untilNext = 0f;

            while (elapsed < duration)
            {
                bool frozen = GameplayFreeze.IsActive || (WaveSpawner.Instance != null && WaveSpawner.Instance.EndingWave);
                if (!frozen)
                {
                    if (untilNext <= 0f)
                    {
                        SpawnTargetExplosion();
                        untilNext = targetExplosionInterval;
                    }
                    elapsed += Time.deltaTime;
                    untilNext -= Time.deltaTime;
                }
                yield return null;
            }
            targetExplosionsRoutine = null;
        }

        private void SpawnTargetExplosion()
        {
            var target = GetMarkedTarget();
            if (target == null) return;

            var go = Instantiate(targetExplosionPrefab, target.transform.position, Quaternion.identity);
            var damage = go.GetComponent<DamageSourceBase>();
            if (damage == null) return;

            int damageAmount = Mathf.Max(1, Mathf.RoundToInt(specialWeaponData.baseDamage * targetExplosionDamageMultiplier));
            damage.Initialize(damageAmount, damage.PierceCount, null, specialWeaponData.weaponClass, specialWeaponData);
            damage.OnDamageDealtEvent += HandleProjectileDamageDealt;
        }

        private void SpawnSpecialProjectile(Vector2 spawnPos)
        {
            int pierceAmount = specialWeaponData.pierceAmount + playerProgression.GetStatTotal(StatType.PierceAmount);
            EnemyHealth target = EnemyTargeting.FindClosest(
                spawnPos, specialAttackTargetSearchRadius, ShouldSpecialCompensateForEnemyBodySize);
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
            damage.OnDamageDealtEvent += HandleProjectileDamageDealt;

            onScepterSpecialAttackLaunched?.Invoke(damage);
        }

        private void HandleProjectileDamageDealt(float damage, GameObject target) => RaiseWeaponDamageDealt(damage, target);

        private void PlayRandomScepterSound()
        {
            if (scepterAttackSounds.Count == 0) return;
            var audioClip = scepterAttackSounds[Random.Range(0, scepterAttackSounds.Count)];
            AudioManager.Instance.PlaySFX(audioClip, true);
        }
    }
}
