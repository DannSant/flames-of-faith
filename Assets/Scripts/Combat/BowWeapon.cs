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

        public override void Attack()
        {
            if (!attackTimer.GetIsEventActive())
            {
                if(currentTarget == null) return;
                targetPosition = currentTarget.transform.position;
                PlayRandomArrowSound();
                characterVisual.PlayAttackAnimation();
                attackTimer.StartEvent();
            }
        }

        public override void SpecialAttack()
        {
            if (specialAttackTimer.GetIsEventActive()) return;           

            playerHealth.IsInvulnerable = true;
            characterVisual.PlayAttackSpecialAnimation();
            specialAttackTimer.StartEvent();
            effectStore?.Trigger(Effects.EffectTrigger.OnSpecialAttack);
        }

        protected override void OnAttackAnimationPlayed()
        {
          
            //if (currentTarget == null) return;
            Vector2 spawnPos = projectileSpawnTransform.position;
            Vector2 targetPos = targetPosition;//currentTarget.transform.position;
            Vector2 direction = (targetPos - spawnPos).normalized;
            
            int pierceAmount = weaponData.pierceAmount + playerProgression.GetStatTotal(StatType.PierceAmount);

            var go = Instantiate(weaponData.projectilePrefab, spawnPos, Quaternion.identity);

            var move = go.GetComponent<ProjectileMovementBase>();
            move.Initialize(direction);

            var damage = go.GetComponent<DamageSourceBase>();
            damage.Initialize(weaponData.baseDamage, weaponData.pierceAmount, null, weaponData.weaponClass,weaponData);

            onBowAttackLaunched?.Invoke(damage);
        }

        protected override void OnSpecialAttackAnimationPlayed()
        {
            playerHealth.IsInvulnerable = false;
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
                damage.Initialize(specialWeaponData.baseDamage, specialWeaponData.pierceAmount, null, specialWeaponData.weaponClass, specialWeaponData);
            }
        }

        private void OnDamageDealt(float damage, int graceGenerated, GameObject target)
        {
            /*if (graceGenerated > 0)
            {
                playerGrace.AddGrace(graceGenerated);
            }*/
        }

        public override float GetWeaponRange()
        {
            float playerRange = playerProgression.GetStatTotal(StatType.Range);
            return weaponData.rangeBase + playerRange;
        }

        private void PlayRandomArrowSound()
        {
            var audioClip = bowAttackSounds[Random.Range(0, bowAttackSounds.Count)];
            AudioManager.Instance.PlaySFX(audioClip,true);
        }

    }

}