using Game.Audio;
using Game.Combat.Projectiles;
using Game.Control;
using Game.Effects;
using Game.Progression;
using Game.Scene;
using Game.Utils;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.InferenceEngine.Tokenization.PostProcessors.Templating;
using UnityEngine;

namespace Game.Combat
{
    public class BowWeapon : WeaponBase
    {
        [Header("Sound Effects")]
        [SerializeField] private List<AudioClip> bowAttackSounds = new();

        [Header("Projectile")]
        [SerializeField] private Transform projectileSpawnTransform;

        [Header("Special: Barrage On Target")]
        [Tooltip("Extra arrows fired at the marked target after the special's burst. Nothing extra without a target.")]
        [SerializeField] private int barrageArrowCount = 6;
        [Tooltip("Seconds between barrage arrows.")]
        [SerializeField] private float barrageInterval = 0.06f;
        [Tooltip("Each barrage arrow leaves at a random angle up to this many degrees off the target, then homes in.")]
        [SerializeField] private float barrageSpreadAngle = 15f;
        [Tooltip("Barrage arrow damage as a fraction of the special's base damage.")]
        [SerializeField] private float barrageDamageMultiplier = 0.5f;
        [Tooltip("Enemies a barrage arrow passes through (plus the Pierce stat).")]
        [SerializeField] private int barragePierce = 0;
        [Tooltip("Homing arrow prefab for the barrage (needs ProjectileMovementBase + DamageSourceBase).")]
        [SerializeField] private GameObject barrageArrowPrefab;

        private Coroutine barrageRoutine;

        public event System.Action<DamageSourceBase> onBowAttackLaunched;
        public event System.Action<DamageSourceBase> onBowSpecialAttackLaunched;

        private Vector3 targetPosition;
        private bool hasPendingTarget;

        public override void Attack()
        {
            if (!attackTimer.GetIsEventActive())
            {
                // No target is fine: the arrow is fired along the aim instead (see OnAttackAnimationPlayed).
                hasPendingTarget = currentTarget != null;
                if (hasPendingTarget)
                {
                    targetPosition = currentTarget.transform.position;
                }
                PlayRandomArrowSound();
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
          
            Vector2 spawnPos = projectileSpawnTransform.position;
            Vector2 direction = hasPendingTarget
                ? ((Vector2)targetPosition - spawnPos).normalized
                : GetFreeAimDirection();

            int pierceAmount = weaponData.pierceAmount + playerProgression.GetStatTotal(StatType.PierceAmount);

            var go = Instantiate(weaponData.projectilePrefab, spawnPos, Quaternion.identity);

            var move = go.GetComponent<ProjectileMovementBase>();
            move.Initialize(direction);
            if (!hasPendingTarget)
            {
                // Targeted arrows keep the prefab's lifetime; only free-aim ones are capped to the range.
                move.SetMaxTravelDistance(GetFreeAimMaxDistance());
            }

            var damage = go.GetComponent<DamageSourceBase>();
            damage.Initialize(weaponData.baseDamage, pierceAmount, null, weaponData.weaponClass,weaponData);
            damage.OnDamageDealtEvent += HandleProjectileDamageDealt;

            onBowAttackLaunched?.Invoke(damage);
        }

        protected override void OnSpecialAttackAnimationPlayed()
        {
            playerHealth.ToggleIsInvulnerable(false);
            int numProjectiles = specialWeaponData.projectileAmount;
            //int numProjectiles = Random.Range(6, 11);
            float angleStep = 360f / numProjectiles;
            float randomOffset = Random.Range(0f, 360f);
            Vector2 spawnPos = transform.position;

            //int damageAmount = specialWeaponData.baseDamage + playerProgression.GetStatTotal(StatType.RangedDamage) * specialWeaponData.attackScale;
            int pierceAmount = specialWeaponData.pierceAmount + playerProgression.GetStatTotal(StatType.PierceAmount);

            for (int i = 0; i < numProjectiles; i++)
            {
                float angle = randomOffset + i * angleStep;
                float rad = angle * Mathf.Deg2Rad;
                Vector2 direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;

                var go = Instantiate(specialWeaponData.projectilePrefab, spawnPos, Quaternion.identity);

                var move = go.GetComponent<ProjectileMovementBase>();
                move.Initialize(direction);

                var damage = go.GetComponent<DamageSourceBase>();
                damage.Initialize(specialWeaponData.baseDamage, pierceAmount, null, specialWeaponData.weaponClass, specialWeaponData);
                damage.OnDamageDealtEvent += HandleProjectileDamageDealt;
            }

            var target = GetMarkedTarget();
            if (target != null && barrageArrowPrefab != null && barrageArrowCount > 0)
            {
                if (barrageRoutine != null) StopCoroutine(barrageRoutine);
                barrageRoutine = StartCoroutine(BarrageRoutine(target));
            }
        }

        /// <summary>
        /// Fires the barrage at the target captured when the special went off, even if the
        /// marked target changes meanwhile. Arrows left after it dies fly straight.
        /// </summary>
        private IEnumerator BarrageRoutine(EnemyHealth target)
        {
            int damageAmount = Mathf.Max(1, Mathf.RoundToInt(specialWeaponData.baseDamage * barrageDamageMultiplier));
            int pierceAmount = barragePierce + playerProgression.GetStatTotal(StatType.PierceAmount);
            Vector2 lastTargetPosition = target.transform.position;

            for (int i = 0; i < barrageArrowCount; i++)
            {
                bool targetAlive = EnemyTargeting.IsTargetable(target);
                if (targetAlive) lastTargetPosition = target.transform.position;

                Vector2 spawnPos = projectileSpawnTransform.position;
                Vector2 toTarget = (lastTargetPosition - spawnPos).normalized;
                float spread = Random.Range(-barrageSpreadAngle, barrageSpreadAngle);
                Vector2 direction = Quaternion.Euler(0f, 0f, spread) * toTarget;

                var go = Instantiate(barrageArrowPrefab, spawnPos, Quaternion.identity);

                var move = go.GetComponent<ProjectileMovementBase>();
                move.Initialize(direction);
                if (targetAlive)
                {
                    move.SetTarget(target.transform);
                }

                var damage = go.GetComponent<DamageSourceBase>();
                damage.Initialize(damageAmount, pierceAmount, null, specialWeaponData.weaponClass, specialWeaponData);
                damage.OnDamageDealtEvent += HandleProjectileDamageDealt;

                yield return new WaitForSeconds(barrageInterval);
            }
            barrageRoutine = null;
        }

        private void HandleProjectileDamageDealt(float damage, GameObject target) => RaiseWeaponDamageDealt(damage, target);

        private void PlayRandomArrowSound()
        {
            var audioClip = bowAttackSounds[Random.Range(0, bowAttackSounds.Count)];
            AudioManager.Instance.PlaySFX(audioClip,true);
        }

    }

}