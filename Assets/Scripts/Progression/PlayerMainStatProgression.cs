using Game.Combat;
using Game.Control;
using Game.Misc;
using UnityEngine;

namespace Game.Progression
{
    /// <summary>
    /// Grants the player +1 to their class's main stat (see CharacterClassData.mainStat /
    /// levelsPerMainStatPoint) every N level-ups, via PlayerProgression.UpdateStat so the bonus
    /// persists like any other stat gain.
    /// </summary>
    public class PlayerMainStatProgression : MonoBehaviour
    {
        private PlayerExperience playerExperience;
        private PlayerProgression playerProgression;
        private CharacterVisual characterVisual;

        // The very first onLevelUp firing after this component is enabled is always a "sync to
        // current state" broadcast from PlayerExperience.ResetState/LoadState, not a real gain -
        // it's used only to establish a baseline so that broadcast never (re-)grants a point that
        // was already accounted for in the stats we just loaded.
        private int? lastSeenLevel;

        private void Awake()
        {
            playerExperience = GetComponent<PlayerExperience>();
            playerProgression = GetComponent<PlayerProgression>();
            characterVisual = GetComponentInChildren<CharacterVisual>();
        }

        private void Start()
        {
            if (playerExperience != null)
            {
                playerExperience.onLevelUp += HandleLevelUp;
            }
        }

        private void OnDisable()
        {
            if (playerExperience != null)
            {
                playerExperience.onLevelUp -= HandleLevelUp;
            }
        }

        private void HandleLevelUp(int newLevel, int newXPRequired)
        {
            if (lastSeenLevel.HasValue && newLevel > lastSeenLevel.Value)
            {
                GrantPointsIfEarned(lastSeenLevel.Value, newLevel);
            }
            lastSeenLevel = newLevel;
        }

        private void GrantPointsIfEarned(int previousLevel, int newLevel)
        {
            var classData = characterVisual != null ? characterVisual.CharacterData : null;
            if (classData == null || classData.levelsPerMainStatPoint <= 0) return;

            // Level-number-aware rather than a simple per-scene counter, so the cadence stays
            // correct regardless of what level this component happened to start observing from
            // (e.g. after a retry or a level transition mid-run).
            int pointsBefore = previousLevel / classData.levelsPerMainStatPoint;
            int pointsAfter = newLevel / classData.levelsPerMainStatPoint;
            int pointsEarned = pointsAfter - pointsBefore;
            if (pointsEarned <= 0) return;


            DamageTypeColorHelper.TryGetWeaponClass(classData.mainStat, out WeaponClass weaponClass);
            string statGainedText = $"{pointsEarned} {classData.mainStat} gained!";
            DamageNumberSpawner.Instance.SpawnStatGainedByWeaponClass(transform.position, statGainedText, weaponClass);

            playerProgression.UpdateStat(classData.mainStat, pointsEarned);
        }
    }
}
