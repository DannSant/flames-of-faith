using Game.Scene;
using Game.Waves;
using UnityEngine;

namespace Game.Combat
{
    public class CorruptionGenerator : MonoBehaviour
    {
        [SerializeField] private LevelSettings currentLevelSettings;
        private PlayerCorruption playerCorruption;
        private LevelData currentLevelData;        

        private void Start()
        {
            if (WaveSpawner.Instance != null)
            {
                WaveSpawner.Instance.OnWaveGroupFinished += HandleEndOfLevel;
            }

            playerCorruption = PlayerManager.Instance.GetPlayerComponent<PlayerCorruption>();

            if (currentLevelSettings != null) 
            {
                currentLevelData = currentLevelSettings.LevelData;
            }else
            {
                Debug.LogWarning("Current Level Settings is not assigned in CorruptionGenerator.");
            }
            
        }

        private void HandleEndOfLevel()
        {
            // Increase corruption when a wave group is finished
            if(playerCorruption != null && currentLevelData!=null)
            {
                //Debug.Log($"Level Data {currentLevelData}");
                playerCorruption.AddCorruption(currentLevelData.corruptionIncrease);
            }else
            {
                Debug.LogWarning("PlayerCorruption or LevelData is not assigned in CorruptionGenerator.");
            }
        }
    }

}