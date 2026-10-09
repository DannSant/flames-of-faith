using Game.Audio;
using Game.Combat.Projectiles;
using Game.Control;
using Game.Effects;
using Game.Progression;
using Game.Scene;
using Game.Utils;
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
        }

        private void HandleProjectileDamageDealt(float damage, GameObject target) => RaiseWeaponDamageDealt(damage, target);

        private void PlayRandomArrowSound()
        {
            var audioClip = bowAttackSounds[Random.Range(0, bowAttackSounds.Count)];
            AudioManager.Instance.PlaySFX(audioClip,true);
        }

    }

}