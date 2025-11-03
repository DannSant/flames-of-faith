using Game.Audio;
using Game.Combat.Projectiles;
using Game.Control;
using Game.Progression;
using Game.Scene;
using Game.Utils;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Game.Combat
{
    public class BowWeapon : WeaponBase
    {
        [Header("Sound Effects")]
        [SerializeField] private List<AudioClip> bowAttackSounds = new();

        public event System.Action<ProjectileBase> onBowAttackLaunched;
        public event System.Action<ProjectileBase> onBowSpecialAttackLaunched;

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
           // if (playerGrace.CurrentGrace <= specialAttackCost) return;

            //playerGrace.RemoveGrace(specialAttackCost);
            characterVisual.PlayAttackSpecialAnimation();
            specialAttackTimer.StartEvent();
        }

        protected override void OnAttackAnimationPlayed()
        {
            //if (currentTarget == null) return;
            Vector2 spawnPos = transform.position;
            Vector2 targetPos = targetPosition;//currentTarget.transform.position;
            Vector2 direction = (targetPos - spawnPos).normalized;

            int damageAmount = weaponData.baseDamage + playerProgression.GetStatTotal(StatType.RangedDamage) * weaponData.attackScale;
            int pierceAmount = weaponData.pierceAmount + playerProgression.GetStatTotal(StatType.PierceAmount);

            var projectile = Instantiate(weaponData.projectilePrefab, transform.position, Quaternion.identity);
            projectile.Initialize(direction, damageAmount, pierceAmount, 5f, false, weaponData.graceGenerated);
            projectile.ConfigureAfterSpawn();
            projectile.OnDamageDealtEvent += OnDamageDealt;
            onBowAttackLaunched?.Invoke(projectile);
        }

        protected override void OnSpecialAttackAnimationPlayed()
        {
            int numProjectiles = Random.Range(6, 11);
            float angleStep = 360f / numProjectiles;
            float randomOffset = Random.Range(0f, 360f);
            Vector2 spawnPos = transform.position;

            int damageAmount = specialWeaponData.baseDamage + playerProgression.GetStatTotal(StatType.RangedDamage) * specialWeaponData.attackScale;
            int pierceAmount = specialWeaponData.pierceAmount + playerProgression.GetStatTotal(StatType.PierceAmount);

            for (int i = 0; i < numProjectiles; i++)
            {
                float angle = randomOffset + i * angleStep;
                float rad = angle * Mathf.Deg2Rad;
                Vector2 direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;

                var projectile = Instantiate(specialWeaponData.projectilePrefab, spawnPos, Quaternion.identity);
                projectile.Initialize(direction, damageAmount, pierceAmount, 5f, false, specialWeaponData.graceGenerated);
                projectile.ConfigureAfterSpawn();
                onBowSpecialAttackLaunched?.Invoke(projectile);
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