using Game.Control;
using Game.Progression;
using Game.Scene;
using Game.Utils;
using UnityEngine;

namespace Game.Combat
{

    public abstract class WeaponBase : MonoBehaviour
    {
        [SerializeField] protected WeaponData weaponData;
        [SerializeField] protected WeaponData specialWeaponData;
        protected Health currentTarget;

        protected CharacterVisual characterVisual;
        protected PlayerController playerController;
        protected PlayerGrace playerGrace;
        protected PlayerProgression playerProgression;
        protected WeaponManager weaponManager;

        protected UpdateTimer attackTimer;
        protected UpdateTimer specialAttackTimer;

        protected int specialAttackCost = 3;
        protected bool attackButtonDown = false;

        public virtual void Initialize(CharacterVisual characterVisual)
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

        protected virtual void Update()
        {
            ProcessAttackTimers();
            AttackUpdate();
        }

        protected virtual void OnDisable()
        {
            playerProgression.onStatUpdated -= OnStatUpdated;
            characterVisual.OnAttackEndAnimEvent -= OnAttackAnimationPlayed;
            characterVisual.OnSpecialAttackEndAnimEvent -= OnSpecialAttackAnimationPlayed;
        }

        protected virtual void SetupAttackSpeedVariables()
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

        protected virtual void OnStatUpdated(StatType statType, int value)
        {
            if (statType == StatType.AttackSpeed)
            {
                SetupAttackSpeedVariables();
            }
        }

        protected virtual void ProcessAttackTimers()
        {
            attackTimer.UpdateEvent();
            specialAttackTimer.UpdateEvent();
        }

        protected virtual void AttackUpdate()
        {
            if (attackButtonDown && !attackTimer.GetIsEventActive())
            {
                Attack();
            }
        }

        public virtual void SetAttackButtonDown(bool value)
        {
            attackButtonDown = value;
        }

        public virtual void SetTarget(Health target)
        {
            currentTarget = target;
        }

        public virtual Health GetTarget() => currentTarget;

        public abstract void Attack();
        public abstract void SpecialAttack();

        public abstract float GetWeaponRange();
        public WeaponData GetWeaponData() => weaponData;

        public virtual bool IsAttackTimerActive() => attackTimer.GetIsEventActive();
        public virtual bool IsSpecialAttackTimerActive() => specialAttackTimer.GetIsEventActive();
        public virtual float GetAttackTimer() => attackTimer.GetEventTimer();
        public virtual float GetSpecialAttackTimer() => specialAttackTimer.GetEventTimer();
        public virtual float GetAttackTimerDuration() => attackTimer.GetEventDuration();
        public virtual float GetSpecialAttackTimerDuration() => specialAttackTimer.GetEventDuration();

        protected void GrantGrace(int amount)
        {
            if (amount > 0)
            {
                playerGrace.AddGrace(amount);
            }
        }

        protected virtual void OnAttackAnimationPlayed() { }
        protected virtual void OnSpecialAttackAnimationPlayed() { }
    }
}
