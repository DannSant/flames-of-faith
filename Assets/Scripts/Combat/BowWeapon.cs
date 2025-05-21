using Game.Combat.Projectiles;
using Game.Control;
using Game.Progression;
using Game.Scene;
using Game.Utils;
using UnityEngine;

namespace Game.Combat
{
    public class BowWeapon : WeaponBase
    {
        private CharacterVisual characterVisual;
        private PlayerController playerController;
        private PlayerGrace playerGrace;
        private PlayerProgression playerProgression;
        private WeaponManager weaponManager;
        private bool attackButtonDown = false;
        private int specialAttackCost = 3;
        private UpdateTimer attackTimer;
        private UpdateTimer specialAttackTimer;
        public override void Initialize(CharacterVisual characterVisual)
        {
            this.characterVisual = characterVisual;
            playerController = PlayerManager.Instance.GetPlayerComponent<PlayerController>();
            playerGrace = PlayerManager.Instance.GetPlayerComponent<PlayerGrace>();
            playerProgression = PlayerManager.Instance.GetPlayerComponent<PlayerProgression>();
            weaponManager = PlayerManager.Instance.GetPlayerComponent<WeaponManager>();
          

            attackTimer = new UpdateTimer(1);
            specialAttackTimer = new UpdateTimer(1);

            SetupAttackSpeedVariables();

            playerProgression.onStatUpdated += OnStatUpdated;
            characterVisual.OnAttackEndAnimEvent += OnAttackAnimationPlayed;
            characterVisual.OnSpecialAttackEndAnimEvent += OnSpecialAttackAnimationPlayed;
        }

        private void Update()
        {
            ProcessAttackTimers();                   

        }

        private void ProcessAttackTimers()
        {
            attackTimer.UpdateEvent();
            specialAttackTimer.UpdateEvent();
        }

        private void AttackUpdate()
        {
            if (attackButtonDown && !attackTimer.GetIsEventActive())
            {
                Attack();
            }
        }

        private void OnStatUpdated(StatType statType, int value)
        {
            if (statType == StatType.AttackSpeed)
            {
                SetupAttackSpeedVariables();
            }
        }

        private void SetupAttackSpeedVariables()
        {
            float attackDelay = StatsCalculations.CalculateAttackDelay(
                playerProgression.GetStatTotal(StatType.AttackSpeed),
                weaponData.attackCooldownBase,
                weaponData.attackSpeedScale);
            attackTimer.SetEventDuration(attackDelay);

            float specialAttackDelay = StatsCalculations.CalculateAttackDelay(
                playerProgression.GetStatTotal(StatType.AttackSpeed),
                specialWeaponData.attackCooldownBase,
                specialWeaponData.attackSpeedScale);
            specialAttackTimer.SetEventDuration(specialAttackDelay);
        }

        public override void Attack()
        {
            if (!attackTimer.GetIsEventActive())
            {               
                //Start animation. Projectil will spawn on animation event
                characterVisual.PlayAttackAnimation();
                attackTimer.StartEvent();
            }
        }

        public override void SpecialAttack()
        {
            if (specialAttackTimer.GetIsEventActive()) return;
            if (playerGrace.CurrentGrace <= specialAttackCost) return;

            playerGrace.RemoveGrace(specialAttackCost);

            //Start animation. Projectil will spawn on animation event
            characterVisual.PlayAttackSpecialAnimation();
            specialAttackTimer.StartEvent();
           
        }

        private void OnAttackAnimationPlayed()
        {
            if (currentTarget == null) return;
            Vector2 spawnPos = transform.position;
            Vector2 targetPos = currentTarget.transform.position;
            Vector2 direction = (targetPos - spawnPos).normalized;

            int damageAmount = weaponData.baseDamage + playerProgression.GetStatTotal(StatType.RangedDamage) * weaponData.attackScale;
            int pierceAmount = weaponData.pierceAmount + playerProgression.GetStatTotal(StatType.PierceAmount);

            //spawn projectile
            var projectile = Instantiate(weaponData.projectilePrefab, transform.position, Quaternion.identity);
            projectile.Initialize(direction, damageAmount, pierceAmount, 5f, false);
            projectile.ConfigureAfterSpawn();
            projectile.OnDamageDealtEvent += OnDamageDealt;
        }

        private void OnSpecialAttackAnimationPlayed()
        {
            int numProjectiles = Random.Range(6, 11); // Random between 6 and 10
            float angleStep = 360f / numProjectiles;
            float randomOffset = Random.Range(0f, 360f); // Small offset to vary the spread

            Vector2 spawnPos = transform.position;

            int damageAmount = specialWeaponData.baseDamage + playerProgression.GetStatTotal(StatType.RangedDamage) * specialWeaponData.attackScale;
            int pierceAmount = specialWeaponData.pierceAmount + playerProgression.GetStatTotal(StatType.PierceAmount);

            for (int i = 0; i < numProjectiles; i++)
            {
                float angle = randomOffset + i * angleStep;
                float rad = angle * Mathf.Deg2Rad;

                Vector2 direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;

                var projectile = Instantiate(specialWeaponData.projectilePrefab, spawnPos, Quaternion.identity);
                projectile.Initialize(direction, damageAmount, pierceAmount, 5f, false);
                projectile.ConfigureAfterSpawn();
            }
        }

        private void OnDamageDealt(int damage, int graceGenerated)
        {
            if (graceGenerated > 0)
            {
                playerGrace.AddGrace(graceGenerated);
            }
        }
        

        public override float GetWeaponRange()
        {  
            float playerRange = playerProgression.GetStatTotal(StatType.Range);
            return weaponData.rangeBase + playerRange;
        }

        public override bool IsAttackTimerActive() => attackTimer.GetIsEventActive();
        public override bool IsSpecialAttackTimerActive() => specialAttackTimer.GetIsEventActive();
        public override float GetAttackTimer() => attackTimer.GetEventTimer();
        public override float GetSpecialAttackTimer() => specialAttackTimer.GetEventTimer();
        public override float GetAttackTimerDuration() => attackTimer.GetEventDuration();
        public override float GetSpecialAttackTimerDuration() => specialAttackTimer.GetEventDuration();

        
    }

}