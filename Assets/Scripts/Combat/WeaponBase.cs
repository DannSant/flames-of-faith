using Game.Control;
using Game.Effects;
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
        protected EnemyHealth currentTarget;

        protected CharacterVisual characterVisual;
        protected PlayerController playerController;      
        protected PlayerProgression playerProgression;
        protected WeaponManager weaponManager;
        protected EffectStore effectStore;
        protected PlayerHealth playerHealth;

        protected UpdateTimer attackTimer;
        protected UpdateTimer specialAttackTimer;

        protected int specialAttackCost = 3;
        protected bool attackButtonDown = false;

        private void Start()
        {
            attackTimer = new UpdateTimer(1);
            specialAttackTimer = new UpdateTimer(1);
            playerController = PlayerManager.Instance.GetPlayerComponent<PlayerController>();
            playerProgression = PlayerManager.Instance.GetPlayerComponent<PlayerProgression>();
            weaponManager = PlayerManager.Instance.GetPlayerComponent<WeaponManager>();
            effectStore = PlayerManager.Instance.GetPlayerComponent<EffectStore>();
            playerHealth = PlayerManager.Instance.GetPlayerComponent<PlayerHealth>();
        }

        /**
         * Executed on InitializeAfterStateReady
         * */
        public virtual void Initialize(CharacterVisual characterVisual)
        {
            this.characterVisual = characterVisual;

            SetupAttackSpeedVariables();

            playerProgression.onDerivedStatsChanged += OnDerivedStatUpdated;
            characterVisual.OnAttackStartAnimEvent += OnAttackAnimationStarted;
            characterVisual.OnAttackEndAnimEvent += OnAttackAnimationPlayed;

            characterVisual.OnSpecialAttackStartAnimEvent += OnSpecialAttackAnimationStarted;
            characterVisual.OnSpecialAttackEndAnimEvent += OnSpecialAttackAnimationPlayed;
        }

        protected virtual void Update()
        {
            ProcessAttackTimers();
            AttackUpdate();
        }

        protected virtual void OnDisable()
        {
            if(playerProgression != null)
            {
                playerProgression.onDerivedStatsChanged -= OnDerivedStatUpdated;
            }
                
            if(characterVisual != null)
            {
                characterVisual.OnAttackStartAnimEvent -= OnAttackAnimationStarted;
                characterVisual.OnAttackEndAnimEvent -= OnAttackAnimationPlayed;
                characterVisual.OnSpecialAttackEndAnimEvent -= OnSpecialAttackAnimationPlayed;
                characterVisual.OnSpecialAttackStartAnimEvent -= OnSpecialAttackAnimationStarted;
            }
            
        }

        public virtual void SetupAttackSpeedVariables()
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

        protected virtual void OnDerivedStatUpdated()
        {
            SetupAttackSpeedVariables();
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

        public virtual void SetTarget(EnemyHealth target)
        {
            currentTarget = target;
        }

        public virtual EnemyHealth GetTarget() => currentTarget;

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

        protected virtual void OnAttackAnimationStarted() { }
        protected virtual void OnAttackAnimationPlayed() { }
        protected virtual void OnSpecialAttackAnimationStarted() { }
        protected virtual void OnSpecialAttackAnimationPlayed() { }
    }
}
