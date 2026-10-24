
using Game.Combat;
using Game.Common;
using Game.Effects;
using Game.Scene;
using Game.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Progression {
    public class PlayerProgression : MonoBehaviour, IInitializeAfterStateReady, IPrimaryStateLoader
    {       

        private Dictionary<StatType, int> currentStats = new Dictionary<StatType, int>();

        public delegate void OnStatUpdated(StatType statType, int value);
        public event OnStatUpdated onStatUpdated;
        public event Action onDerivedStatsChanged;

        private Dictionary<StatType, float> cachedFinalStats = new();
        // Stat totals before stat dependency rules: what the rules read from.
        private readonly Dictionary<StatType, float> preBonusStats = new();
        private bool statsDirty = true;

        private EffectStore effectStore;
        private PlayerGrace playerGrace;

        private void Awake()
        {
            effectStore = GetComponent<EffectStore>();
            effectStore.OnEffectsChanged += HandleEffectsChanged;

            playerGrace = GetComponent<PlayerGrace>();
            playerGrace.onGraceChanged += OnGraceChanged;
        }

        private void Start()
        {
           
            if (MainSceneController.Instance != null)
            {               
                MainSceneController.Instance.OnGameplayStateResetRequested += ResetProgression;
            }

        }

        private void OnDisable()
        {
            effectStore.OnEffectsChanged -= HandleEffectsChanged;
            playerGrace.onGraceChanged -= OnGraceChanged;
            if (MainSceneController.Instance != null)
            {
                MainSceneController.Instance.OnGameplayStateResetRequested -= ResetProgression;
            }
        }

        private void RecalculateFinalStats()
        {
            cachedFinalStats = FinalStatsCalculator.Calculate(
                currentStats,
                effectStore.GetAllStatModifiers(),
                GetCorruptionPenalty,
                effectStore.GetAllStatDependencies(),
                preBonusStats);

            statsDirty = false;
        }

        /// <summary>
        /// The final stat totals as they would be with <paramref name="extraPoints"/> more points
        /// of <paramref name="stat"/> (e.g. a level-up choice), through the same math as the real
        /// stats: % item bonuses, corruption and stat bonus rules included. Changes nothing.
        /// </summary>
        public Dictionary<StatType, float> PreviewFinalStats(StatType stat, int extraPoints)
        {
            var baseStats = new Dictionary<StatType, int>(currentStats);
            baseStats.TryGetValue(stat, out int current);
            baseStats[stat] = current + extraPoints;

            return FinalStatsCalculator.Calculate(
                baseStats,
                effectStore.GetAllStatModifiers(),
                GetCorruptionPenalty,
                effectStore.GetAllStatDependencies());
        }

        /// <summary>
        /// How much one item's stat dependency rules currently add to <paramref name="stat"/>
        /// (for the stats pane's sources view). Uses the same pre-bonus totals as the real stat.
        /// </summary>
        public float GetStatDependencyBonus(Effect effect, int count, StatType stat)
        {
            if (effect == null || !effect.HasStatDependencies) return 0f;
            if (statsDirty) RecalculateFinalStats();

            float total = 0f;
            foreach (var rule in effect.StatDependencies)
            {
                if (rule == null || rule.targetStat != stat) continue;
                preBonusStats.TryGetValue(rule.sourceStat, out float source);
                total += StatDependencyCalculator.CalculateRuleBonus(rule, source, count);
            }
            return total;
        }

       

        /// <summary>
        /// How much negative Grace (Corrupted) currently reduces a stat. 0 while Grace is 0 or above.
        /// </summary>
        /// <summary>
        /// What the stat gives right now ("17% less damage taken"), or null for stats with nothing
        /// to compute. Pass <paramref name="points"/> to preview another value (level-up choices).
        /// </summary>
        public string GetStatBenefitText(StatType stat, int? points = null)
        {
            return StatBenefitDescriber.Describe(stat, points ?? GetStatTotal(stat), BuildStatBenefitContext());
        }

        /// <summary>
        /// The settings and player components behind the stat formulas, so the descriptions use
        /// exactly the numbers gameplay uses.
        /// </summary>
        public StatBenefitContext BuildStatBenefitContext()
        {
            var ctx = new StatBenefitContext
            {
                damageSettings = Game.Enemies.EnemyDamageSettings.Instance,
                pierceSettings = PierceSettings.Instance,
                statSettings = PlayerStatSettings.Instance,
                experienceSettings = ExperienceSettings.Instance,
                corruptionSettings = CorruptionSettings.Instance,
                meleeDamage = GetStatTotal(StatType.MeleeDamage),
            };

            var weapon = GetComponent<WeaponManager>()?.GetCurrentWeapon();
            var weaponData = weapon != null ? weapon.GetWeaponData() : null;
            if (weaponData != null)
            {
                ctx.hasWeapon = true;
                ctx.attackCooldownBase = weaponData.attackCooldownBase;
                ctx.attackSpeedScale = weaponData.attackSpeedScale;
                ctx.weaponUsesRange = weaponData.isRangeBased;
                ctx.rangeBase = weaponData.rangeBase;
                ctx.rangeScale = weaponData.rangeScale;

                var specialData = weapon.GetSpecialWeaponData();
                if (specialData != null)
                {
                    ctx.hasSpecial = true;
                    ctx.specialCooldownBase = specialData.attackCooldownBase;
                    ctx.specialAttackSpeedScale = specialData.attackSpeedScale;
                }
            }

            var dash = GetComponent<Game.Control.DashBase>();
            if (dash != null) ctx.dashCooldownBase = dash.DashCooldownBase;

            if (GetComponent<Game.Control.PlayerController>() != null)
            {
                ctx.moveSpeedBase = ctx.statSettings.BaseMoveSpeed;
                ctx.moveSpeedScale = ctx.statSettings.MoveSpeedPerPoint;
            }

            var retribution = GetComponent<RetributionHandler>();
            if (retribution != null)
            {
                ctx.hasRetribution = true;
                ctx.retribution = retribution.Curve;
            }

            return ctx;
        }

        public float GetCorruptionPenalty(StatType stat)
        {
            if (playerGrace == null || !playerGrace.IsCorrupted || StatUpgradeDatabase.Instance == null)
            {
                return 0f;
            }

            StatData statData = StatUpgradeDatabase.Instance.GetStatData(stat);
            if (statData == null || !statData.AffectedByCorruption)
            {
                return 0f;
            }
            return playerGrace.CorruptedLevel * statData.CorruptionReduceFactor;
        }

        private void ResetProgression()
        {
            currentStats.Clear();            
            foreach (var statData in StatUpgradeDatabase.Instance.GetStatsConfig())
            {               
                currentStats.Add(statData.StatType, statData.InitialValue);
                onStatUpdated?.Invoke(statData.StatType, statData.InitialValue);
            }
            statsDirty = true;
        }

        // Method to update the player's stat
        public void UpdateStat(StatType statType, int value)
        {
            if (currentStats.ContainsKey(statType))
            {
                // Update the stat value
                currentStats[statType] += value;

                // Mark stats as dirty
                statsDirty = true;

                var newStat = GetStatTotal(statType, true);
                // Trigger the event to notify subscribers
                onStatUpdated?.Invoke(statType, newStat);

                //Trigger event for derived stats change
                onDerivedStatsChanged?.Invoke(); 

                //Debug.Log($"Updated Stat {statType} by {value}. New total: {newStat}");

            }
        }

        public Dictionary<StatType, int> GetAllCurrentStats()
        {
           
            Dictionary<StatType, int> result = new Dictionary<StatType, int>();
            foreach (var stat in currentStats)
            {               
                result.Add(stat.Key, GetStatTotal(stat.Key));
            }
            return result;
        }

        private void OnGraceChanged(float _, float __)
        {
            statsDirty = true;
            onDerivedStatsChanged?.Invoke();
        }

        private void HandleEffectsChanged()
        {
            statsDirty = true;
            onDerivedStatsChanged?.Invoke();
        }

        // Method to retrieve a stat total value
        public int GetStatTotal(StatType statType, bool forceRecalculate = false)
        {
            return Mathf.FloorToInt(GetFinalStat(statType, forceRecalculate));
        }

        public float GetFinalStat(StatType stat, bool forceRecalculate=false)
        {
           
            if (statsDirty || forceRecalculate)
            {
                RecalculateFinalStats();
            }

            return cachedFinalStats.TryGetValue(stat, out float value) ? value : 0f;
        }

       

        public void InitializeAfterStateReady()
        {
          // not used
        }

        public void ResetState()
        {
            //State is reset in ResetProgression called from MainSceneController
        }

        public void SaveState()
        {
            GameSession.Instance.SaveStats(currentStats);
        }

        public void LoadState()
        {
            statsDirty = true;
            currentStats = new Dictionary<StatType, int>(GameSession.Instance.PlayerData.savedStats);

            // A run saved before a stat existed (e.g. Evasion) lacks it; give it its initial value.
            if (StatUpgradeDatabase.Instance != null)
            {
                StatBackfill.FillMissing(currentStats, StatUpgradeDatabase.Instance.GetStatsConfig()
                    .Select(s => new KeyValuePair<StatType, int>(s.StatType, s.InitialValue)));
            }

        }
    }
}
