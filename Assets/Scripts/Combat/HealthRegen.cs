using Game.Common;
using Game.Progression;
using Game.Scene;
using Game.Waves;
using UnityEngine;

namespace Game.Combat
{
    public class HealthRegen : MonoBehaviour, IMapComponentDisabler
    {
        private PlayerProgression playerProgression;
        private PlayerHealth playerHealth;

        private float regenTimer = 0f;
        private LevelData levelData;

        private void Awake()
        {
            playerProgression = GetComponent<PlayerProgression>();
            playerHealth = GetComponent<PlayerHealth>();
            
        }

        void Update()
        {
            if (levelData == null)
            {
                levelData = FindLevelData();
                return;
            }
            if (levelData.preventHealthRegen) 
            {
                //Debug.Log("levelData.preventHealthRegen");
                return;
            }

            // Regen stops while the wave-end sequence waits on the player (the wave summary isn't paused, so
            // waiting there would heal for free) and during semi-pauses, where the player can't take damage either
            bool waveEnding = WaveSpawner.Instance != null && WaveSpawner.Instance.EndingWave;
            if (waveEnding || GameplayFreeze.IsActive) return;
           
            float hpRegen = playerProgression.GetStatTotal(StatType.HealthRegen);
            if (hpRegen <= 0f) return;

            float interval = CalculateHealInterval(hpRegen);
            regenTimer += Time.deltaTime;

            if (regenTimer >= interval)
            {
                playerHealth.Heal(1, applyHealingReceivedStat: false);
                regenTimer = 0f;
            }
        }

        // There is no regen on the map, and no LevelSettings to find there either - without this the lookup above
        // would search the scene every frame. The player is respawned for each level, so it starts enabled again.
        public void DisableComponentsOnMap()
        {
            enabled = false;
        }

        private LevelData FindLevelData()
        {
            var levelSettings = FindAnyObjectByType<LevelSettings>();
            if (levelSettings != null)
            {
                return levelSettings.LevelData;
            }
            return null;
        }

        private float CalculateHealInterval(float hpRegen)
        {
            if (hpRegen <= 0f) return Mathf.Infinity;
            return 5f / (1f + ((hpRegen - 1f) / 2.25f));
        }
    }
        
 }
