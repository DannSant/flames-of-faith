using Game.Combat;
using Game.Misc;
using Game.Progression;
using UnityEngine;

namespace Game.Effects.EffectBehaviors
{
    /// <summary>
    /// Grants +1 of a stat every N level-ups, as a permanent stat gain (PlayerProgression.UpdateStat).
    /// Used by the class Emblems for the class's main damage stat.
    ///
    /// The level the player has when the item is added is the baseline. PlayerExperience is a
    /// primary state loader, so it is already reset or loaded by then, and loading a run never
    /// re-grants points that are already in the saved stats.
    /// </summary>
    [CreateAssetMenu(menuName = "Effects/Behaviors/Stat On Level Up")]
    public class MainStatOnLevelUpBehavior : EffectBehavior
    {
        [Tooltip("The stat gained on level-up (e.g. Melee Damage for the Warrior).")]
        [SerializeField] private StatType stat = StatType.MeleeDamage;
        [Tooltip("+1 to the stat every this many level-ups. 1 = every level.")]
        [Min(1)]
        [SerializeField] private int levelsPerPoint = 1;

        private class State
        {
            public PlayerExperience experience;
            public PlayerProgression progression;
            public int lastSeenLevel;
            public PlayerExperience.OnLevelUp handler;
        }

        public override void Initialize(EffectBehaviorContext context, Effect effect)
        {
            base.Initialize(context, effect);

            var state = GetState<State>();
            Unsubscribe(state);

            state.experience = ownerObject.GetComponent<PlayerExperience>();
            state.progression = ownerObject.GetComponent<PlayerProgression>();
            if (state.experience == null || state.progression == null) return;

            state.lastSeenLevel = state.experience.GetCurrentLevel();
            state.handler = (newLevel, _) => HandleLevelUp(state, newLevel);
            state.experience.onLevelUp += state.handler;
        }

        public override void Cleanup()
        {
            if (context != null)
            {
                Unsubscribe(GetState<State>());
            }
        }

        private static void Unsubscribe(State state)
        {
            if (state.experience != null && state.handler != null)
            {
                state.experience.onLevelUp -= state.handler;
            }
            state.handler = null;
        }

        private void HandleLevelUp(State state, int newLevel)
        {
            int points = StatsCalculations.CalculateMainStatPointsEarned(state.lastSeenLevel, newLevel, levelsPerPoint);
            // A level going down (a reset broadcast) just moves the baseline.
            state.lastSeenLevel = newLevel;
            if (points <= 0) return;

            if (DamageNumberSpawner.Instance != null)
            {
                DamageTypeColorHelper.TryGetWeaponClass(stat, out WeaponClass weaponClass);
                string text = $"{points} {StatDisplayNameHelper.GetDisplayName(stat)} gained!";
                DamageNumberSpawner.Instance.SpawnStatGainedByWeaponClass(state.progression.transform.position, text, weaponClass);
            }

            state.progression.UpdateStat(stat, points);
        }
    }
}
