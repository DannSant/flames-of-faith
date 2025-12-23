using Game.Audio;
using Game.Control;
using Game.Misc;
using Game.Progression;
using Game.Scene;
using Game.Utils;
using System.Collections.Generic;
using UnityEngine;
using static Game.Progression.PlayerProgression;

namespace Game.Combat 
{
    public class SwordWeapon : WeaponBase
    {
        [Header("Colliders")]
        [SerializeField] private GameObject weaponColliderObject;
        [SerializeField] private Transform pivotPoint;
        [SerializeField] private float orbitRadius = 0.5f;
        [SerializeField] private GameObject specialAttackCollider;
        [SerializeField] private WeaponDamageSource mainDamageSource;
        [SerializeField] private WeaponDamageSource specialDamageSource;

        [Header("Visual Effects")]
        [SerializeField] private GameObject specialAttackVFXPrefab;

        [Header("Sound Effects")]
        [SerializeField] private List<AudioClip> swordAttackSounds = new();        

        public override void Initialize(CharacterVisual characterVisual)
        {
            base.Initialize(characterVisual);

            weaponColliderObject.SetActive(false);
            specialAttackCollider.SetActive(false);

            mainDamageSource.WeaponData = weaponData;
            mainDamageSource.OnDamageDealt += OnDamageDealt;
            specialDamageSource.WeaponData = specialWeaponData;
            specialDamageSource.OnDamageDealt += OnSpecialDamageDealt;
            
        }

        protected override void OnDisable()
        {
            base.OnDisable();            
            mainDamageSource.OnDamageDealt -= OnDamageDealt;
            specialDamageSource.OnDamageDealt -= OnSpecialDamageDealt;
        }

        protected override void Update()
        {
            base.Update();

            if (weaponManager.GetCurrentTarget()!=null)
            {
                LookAtTarget();
            }
            else
            {
                LookAtMouse();
            }
        }

        public override void Attack()
        {
            if (!attackTimer.GetIsEventActive())
            {
                PlayRandomSwordSound();
                characterVisual.PlayAttackAnimation();
                attackTimer.StartEvent();               
            }
        }

        public override void SpecialAttack()
        {
            if (specialAttackTimer.GetIsEventActive()) return;

            characterVisual.PlayAttackSpecialAnimation();
            specialAttackTimer.StartEvent();
            specialAttackCollider.SetActive(true);
            effectStore?.Trigger(Effects.EffectTrigger.OnSpecialAttack);
        }

        protected override void OnAttackAnimationStarted()
        {
            weaponColliderObject.SetActive(true);
            CleaveDamage();
        }

        protected override void OnAttackAnimationPlayed()
        {
            weaponColliderObject.SetActive(false);
        }

        protected override void OnSpecialAttackAnimationPlayed()
        {           
            specialAttackCollider.SetActive(false);
        }

        private void OnDamageDealt(float damage, GameObject target)
        {
            Instantiate(specialAttackVFXPrefab, target.transform.position, Quaternion.identity);
            //GrantGrace(graceGenerated);
        }

        private void OnSpecialDamageDealt(float damage, GameObject target)
        {
            if (specialAttackVFXPrefab != null)
            {
                Instantiate(specialAttackVFXPrefab, target.transform.position, Quaternion.identity);
            }
            //GrantGrace(graceGenerated);
        }

        private void LookAtTarget()
        {
            if (currentTarget == null) return;
            if (weaponColliderObject == null) return;
           
            Vector2 direction = (currentTarget.transform.position - transform.position).normalized;
            OrbitCollider(direction);


        }

        private void LookAtMouse()
        {
            if(playerController == null) return;
            if(weaponColliderObject == null) return;
          
            Vector2 mousePosition = playerController.GetMouseWorldPosition();
            Vector2 direction = (mousePosition - (Vector2)transform.position).normalized;

            //float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            //weaponColliderObject.transform.rotation = Quaternion.Euler(0, 0, angle);
            OrbitCollider(direction);

        }

        private void OrbitCollider(Vector2 direction)
        {
           
            float currentAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            float angleRad = currentAngle * Mathf.Deg2Rad;
            Vector2 offset = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * orbitRadius;
            weaponColliderObject.transform.position = pivotPoint.position + (Vector3)offset;
        }

        public override float GetWeaponRange()
        {
            return weaponData.rangeBase;
        }

        private void CleaveDamage()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, weaponData.rangeBase, LayerMask.GetMask("Enemy"));
            float cleaveDamage = playerProgression.GetStatTotal(StatType.Cleave);
            foreach (var hit in hits)
            {
                EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
                if (cleaveDamage > 0 && enemy != null) 
                { 
                    enemy.TakeDamage(cleaveDamage, weaponData.weaponClass);
                }
                Knockback knockback = hit.GetComponent<Knockback>();
                if (cleaveDamage> 0 && knockback != null)
                {
                    var playerTransform = PlayerManager.Instance.GetPlayerComponent<PlayerController>().transform;
                    knockback.ApplyKnockback(playerTransform, weaponData.knockbackForce);
                }
            }
        }

        private void PlayRandomSwordSound()
        {
            var audioClip =swordAttackSounds[Random.Range(0, swordAttackSounds.Count)];
            AudioManager.Instance.PlaySFX(audioClip);
        }

        public WeaponDamageSource GetDamageSource()
        {
            return mainDamageSource;
        }

        public WeaponDamageSource GetSpecialDamageSource() 
        {
            return specialDamageSource;
        }
    }

}