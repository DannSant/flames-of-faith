using Game.Audio;
using Game.Control;
using Game.Effects.Abilities;
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
        [Tooltip("Shown around the player while an upgraded Glyph of Whirlwind's holy shield is up.")]
        [SerializeField] private GameObject projectileShieldVFXPrefab;
        [Tooltip("Where the shield effect sits relative to the player's position (at the feet). Raise Y to center it on the body.")]
        [SerializeField] private Vector3 projectileShieldVFXOffset = new Vector3(0f, 0.3f, 0f);

        private GameObject activeShieldVfx;

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

        // Glyph of Crusader Strike sets the basic attack's damage and bleed chance; Glyph of
        // Whirlwind sets the special's damage. Without them the WeaponData values are used.
        protected override void ApplyAbilityItems()
        {
            if (mainDamageSource == null || specialDamageSource == null) return;

            var strike = GetAbilityValues<CrusaderStrikeAbility, CrusaderStrikeAbility.Values>();
            mainDamageSource.BaseDamageOverride = strike?.baseDamage;
            mainDamageSource.DebuffChanceBonus = strike != null ? strike.bleedChanceBonus : 0f;

            var whirlwind = GetAbilityValues<WhirlwindAbility, WhirlwindAbility.Values>();
            specialDamageSource.BaseDamageOverride = whirlwind?.baseDamage;
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

            if (weaponManager!= null && weaponManager.GetCurrentTarget()!=null)
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
            if(characterVisual == null) return;
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

            playerHealth.ToggleIsInvulnerable(true);
            characterVisual.PlayAttackSpecialAnimation();
            specialAttackTimer.StartEvent();
            specialAttackCollider.SetActive(true);
            RaiseProjectileShield();
            effectStore?.Trigger(Effects.EffectTrigger.OnSpecialAttack);
        }

        // Upgraded Glyph of Whirlwind: a holy shield that absorbs enemy projectiles (PlayerHealth)
        private void RaiseProjectileShield()
        {
            var whirlwind = GetAbilityValues<WhirlwindAbility, WhirlwindAbility.Values>();
            if (whirlwind == null || whirlwind.shieldDuration <= 0f || playerHealth == null) return;

            playerHealth.ActivateProjectileShield(whirlwind.shieldDuration, whirlwind.absorbHealFraction);

            if (projectileShieldVFXPrefab == null) return;
            if (activeShieldVfx != null) Destroy(activeShieldVfx);
            activeShieldVfx = Instantiate(projectileShieldVFXPrefab, playerHealth.transform);
            activeShieldVfx.transform.localPosition = projectileShieldVFXOffset;
            Destroy(activeShieldVfx, whirlwind.shieldDuration);
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
            playerHealth.ToggleIsInvulnerable(false);
            specialAttackCollider.SetActive(false);
        }

        private void OnDamageDealt(float damage, GameObject target)
        {
            Instantiate(specialAttackVFXPrefab, target.transform.position, Quaternion.identity);
            RaiseWeaponDamageDealt(damage, target);
        }

        private void OnSpecialDamageDealt(float damage, GameObject target)
        {
            if (specialAttackVFXPrefab != null)
            {
                Instantiate(specialAttackVFXPrefab, target.transform.position, Quaternion.identity);
            }
            RaiseWeaponDamageDealt(damage, target);
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
          
            Vector2 direction = playerController.GetAimDirection();

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

        private void CleaveDamage()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, weaponData.rangeBase, LayerMask.GetMask("Enemy"));
            float cleaveDamage = playerProgression.GetStatTotal(StatType.Cleave);
            foreach (var hit in hits)
            {
                EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
                if (cleaveDamage > 0 && enemy != null) 
                { 
                    enemy.TakeDamage(new DamageRequest(cleaveDamage, weaponData.weaponClass,false));
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