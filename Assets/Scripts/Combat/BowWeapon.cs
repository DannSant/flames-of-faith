using Game.Audio;
using Game.Combat.Projectiles;
using Game.Control;
using Game.Effects;
using Game.Effects.Abilities;
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
        [Tooltip("Extra arrows fired at the marked target after the special's burst. Nothing extra without a target. Glyph of Barrage overrides this.")]
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

        // Glyph of Holy Shot / Glyph of Barrage, read when firing. Null = the bow's own data.
        private HolyShotAbility.Values HolyShot => GetAbilityValues<HolyShotAbility, HolyShotAbility.Values>();
        private BarrageAbility.Values Barrage => GetAbilityValues<BarrageAbility, BarrageAbility.Values>();

        public event System.Action<DamageSourceBase> onBowAttackLaunched;
        public event System.Action<DamageSourceBase> onBowSpecialAttackLaunched;

        private Vector3 targetPosition;
        private bool hasPendingTarget;
        private EnemyHealth pendingTarget;

        public override void Attack()
        {
            if (!attackTimer.GetIsEventActive())
            {
                // No target is fine: the arrow is fired along the aim instead (see OnAttackAnimationPlayed).
                hasPendingTarget = currentTarget != null;
                pendingTarget = currentTarget;
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

            // Aim where the target is now (it moved during the wind-up); the arrow then steers
            // toward it in flight. A target that died meanwhile: aim at its last known position.
            bool targetAlive = hasPendingTarget && EnemyTargeting.IsTargetable(pendingTarget);
            if (targetAlive)
            {
                targetPosition = pendingTarget.transform.position;
            }
            Vector2 direction = hasPendingTarget
                ? ((Vector2)targetPosition - spawnPos).normalized
                : GetFreeAimDirection();

            int pierceAmount = weaponData.pierceAmount + PierceSettings.Instance.GetExtraPierce(playerProgression.GetStatTotal(StatType.Pierce));

            var go = Instantiate(weaponData.projectilePrefab, spawnPos, Quaternion.identity);

            var move = go.GetComponent<ProjectileMovementBase>();
            move.Initialize(direction);
            if (targetAlive)
            {
                // Course correction (only if the arrow prefab has steering enabled).
                move.SetTarget(pendingTarget.transform);
            }
            if (!hasPendingTarget)
            {
                // Targeted arrows keep the prefab's lifetime; only free-aim ones are capped to the range.
                move.SetMaxTravelDistance(GetFreeAimMaxDistance());
            }

            var holyShot = HolyShot;
            var damage = go.GetComponent<DamageSourceBase>();
            int baseDamage = holyShot?.baseDamage ?? weaponData.baseDamage;
            damage.Initialize(baseDamage, pierceAmount, null, weaponData.weaponClass,weaponData);
            damage.OnDamageDealtEvent += HandleProjectileDamageDealt;

            onBowAttackLaunched?.Invoke(damage);

            if (holyShot != null && holyShot.extraArrows > 0)
            {
                int extraDamage = Mathf.Max(1, Mathf.RoundToInt(baseDamage * holyShot.extraArrowDamageMultiplier));
                for (int i = 1; i <= holyShot.extraArrows; i++)
                {
                    float angle = GetConeAngle(i, holyShot.coneAngle);
                    FireConeArrow(spawnPos, Quaternion.Euler(0f, 0f, angle) * direction, extraDamage, pierceAmount);
                }
            }
        }

        /// <summary>
        /// Angle of the <paramref name="index"/>-th extra arrow (1-based) of an upgraded Holy Shot:
        /// alternating sides of the main arrow, one more step out every two arrows (+1, -1, +2, -2...).
        /// </summary>
        public static float GetConeAngle(int index, float step)
        {
            if (index <= 0) return 0f;
            int side = index % 2 == 1 ? 1 : -1;
            int ring = (index + 1) / 2;
            return side * ring * step;
        }

        // An extra Holy Shot arrow: same damage and pierce as the main one, but it flies straight
        // (no steering) for the weapon's range, and doesn't count as an attack for on-hit items
        // (onBowAttackLaunched), so those trigger once per volley.
        private void FireConeArrow(Vector2 spawnPos, Vector2 direction, int baseDamage, int pierceAmount)
        {
            var go = Instantiate(weaponData.projectilePrefab, spawnPos, Quaternion.identity);

            var move = go.GetComponent<ProjectileMovementBase>();
            move.Initialize(direction.normalized);
            move.SetMaxTravelDistance(GetFreeAimMaxDistance());

            var damage = go.GetComponent<DamageSourceBase>();
            damage.Initialize(baseDamage, pierceAmount, null, weaponData.weaponClass, weaponData);
            damage.OnDamageDealtEvent += HandleProjectileDamageDealt;
        }

        protected override void OnSpecialAttackAnimationPlayed()
        {
            playerHealth.ToggleIsInvulnerable(false);
            var barrage = Barrage;
            int numProjectiles = barrage?.ringArrowCount ?? specialWeaponData.projectileAmount;
            int specialBaseDamage = barrage?.baseDamage ?? specialWeaponData.baseDamage;
            int targetArrowCount = barrage?.targetArrowCount ?? barrageArrowCount;
            //int numProjectiles = Random.Range(6, 11);
            float angleStep = 360f / numProjectiles;
            float randomOffset = Random.Range(0f, 360f);
            Vector2 spawnPos = transform.position;

            //int damageAmount = specialWeaponData.baseDamage + playerProgression.GetStatTotal(StatType.RangedDamage) * specialWeaponData.attackScale;
            int pierceAmount = specialWeaponData.pierceAmount + PierceSettings.Instance.GetExtraPierce(playerProgression.GetStatTotal(StatType.Pierce));

            for (int i = 0; i < numProjectiles; i++)
            {
                float angle = randomOffset + i * angleStep;
                float rad = angle * Mathf.Deg2Rad;
                Vector2 direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;

                var go = Instantiate(specialWeaponData.projectilePrefab, spawnPos, Quaternion.identity);

                var move = go.GetComponent<ProjectileMovementBase>();
                move.Initialize(direction);

                var damage = go.GetComponent<DamageSourceBase>();
                damage.Initialize(specialBaseDamage, pierceAmount, null, specialWeaponData.weaponClass, specialWeaponData);
                damage.OnDamageDealtEvent += HandleProjectileDamageDealt;
            }

            var target = GetMarkedTarget();
            if (target != null && barrageArrowPrefab != null && targetArrowCount > 0)
            {
                if (barrageRoutine != null) StopCoroutine(barrageRoutine);
                barrageRoutine = StartCoroutine(BarrageRoutine(target, specialBaseDamage, targetArrowCount));
            }
        }

        /// <summary>
        /// Fires the barrage at the target captured when the special went off, even if the
        /// marked target changes meanwhile. Arrows left after it dies fly straight.
        /// </summary>
        private IEnumerator BarrageRoutine(EnemyHealth target, int specialBaseDamage, int arrowCount)
        {
            int damageAmount = Mathf.Max(1, Mathf.RoundToInt(specialBaseDamage * barrageDamageMultiplier));
            int pierceAmount = barragePierce + PierceSettings.Instance.GetExtraPierce(playerProgression.GetStatTotal(StatType.Pierce));
            Vector2 lastTargetPosition = target.transform.position;

            for (int i = 0; i < arrowCount; i++)
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