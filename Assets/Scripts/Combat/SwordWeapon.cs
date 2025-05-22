using Game.Control;
using Game.Misc;
using Game.Progression;
using Game.Scene;
using Game.Utils;
using UnityEngine;
using static Game.Progression.PlayerProgression;

namespace Game.Combat 
{
    public class SwordWeapon : WeaponBase
    {
        [SerializeField] private GameObject weaponColliderObject;
        [SerializeField] private GameObject specialAttackCollider;
        [SerializeField] private DamageSource mainDamageSource;
        [SerializeField] private DamageSource specialDamageSource;

        public override void Initialize(CharacterVisual characterVisual)
        {
            base.Initialize(characterVisual);

            weaponColliderObject.SetActive(false);
            specialAttackCollider.SetActive(false);

            mainDamageSource.WeaponData = weaponData;
            mainDamageSource.OnDamageDealt += OnDamageDealt;
            specialDamageSource.WeaponData = specialWeaponData;
            specialDamageSource.OnDamageDealt += OnSpecialDamageDealt;

            //characterVisual.OnAttackEndAnimEvent += OnAttackEndAnimEvent;
            //characterVisual.OnSpecialAttackEndAnimEvent += OnSpecialAttackEndAnimEvent;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            //characterVisual.OnAttackEndAnimEvent -= OnAttackEndAnimEvent;
            //characterVisual.OnSpecialAttackEndAnimEvent -= OnSpecialAttackEndAnimEvent;
            mainDamageSource.OnDamageDealt -= OnDamageDealt;
            specialDamageSource.OnDamageDealt -= OnSpecialDamageDealt;
        }

        protected override void Update()
        {
            base.Update();

            if (weaponManager.IsAutoAttackEnabled)
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
                weaponColliderObject.SetActive(true);
                characterVisual.PlayAttackAnimation();
                attackTimer.StartEvent();
                CleaveDamage();
            }
        }

        public override void SpecialAttack()
        {
            if (specialAttackTimer.GetIsEventActive()) return;
            if (playerGrace.CurrentGrace <= specialAttackCost) return;

            playerGrace.RemoveGrace(specialAttackCost);
            characterVisual.PlayAttackSpecialAnimation();
            specialAttackTimer.StartEvent();
            specialAttackCollider.SetActive(true);
        }

        protected override void OnAttackAnimationPlayed()
        {
            weaponColliderObject.SetActive(false);
        }

        protected override void OnSpecialAttackAnimationPlayed()
        {
            specialAttackCollider.SetActive(false);
        }

        private void OnDamageDealt(int damage, int graceGenerated)
        {
            GrantGrace(graceGenerated);
        }

        private void OnSpecialDamageDealt(int damage, int graceGenerated)
        {
            GrantGrace(graceGenerated);
        }

        private void LookAtTarget()
        {
            if (currentTarget == null) return;
            Vector2 direction = (currentTarget.transform.position - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            weaponColliderObject.transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        private void LookAtMouse()
        {
            Vector2 mousePosition = playerController.GetMouseWorldPosition();
            Vector2 direction = (mousePosition - (Vector2)transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            weaponColliderObject.transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        public override float GetWeaponRange()
        {
            return weaponData.rangeBase;
        }

        private void CleaveDamage()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, weaponData.rangeBase, LayerMask.GetMask("Enemy"));
            int cleaveDamage = playerProgression.GetStatTotal(StatType.Cleave);
            foreach (var hit in hits)
            {
                Health enemy = hit.GetComponent<Health>();
                if (enemy != null) 
                { 
                    enemy.TakeDamage(cleaveDamage);
                }
                Knockback knockback = hit.GetComponent<Knockback>();
                if (knockback != null)
                {
                    var playerTransform = PlayerManager.Instance.GetPlayerComponent<PlayerController>().transform;
                    knockback.ApplyKnockback(playerTransform, weaponData.knockbackForce);
                }
            }
        }
    }

}