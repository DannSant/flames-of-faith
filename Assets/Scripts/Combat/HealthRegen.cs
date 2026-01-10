using Game.Common;
using Game.Progression;
using Game.Scene;
using UnityEngine;

namespace Game.Combat
{
    public class HealthRegen : MonoBehaviour
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
           
            float hpRegen = playerProgression.GetStatTotal(StatType.HealthRegen);
            if (hpRegen <= 0f) return;

            float interval = CalculateHealInterval(hpRegen);
            regenTimer += Time.deltaTime;

            if (regenTimer >= interval)
            {
                playerHealth.Heal(1);
                regenTimer = 0f;
            }
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
