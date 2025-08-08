using Game.Progression;
using Game.Scene;
using Game.Waves;
using UnityEditor;
using UnityEngine;

public class DeveloperCheats 
{
    [MenuItem("DevTools/Level/GoToNextLevel")]
    public static void GoToNextLevel()
    {
        WaveSpawner.Instance.GoToNextLevel();
    }

    [MenuItem("DevTools/Stats/Increase Max Health")]
    public static void IncreaseMaxHealth()
    {
        var playerProgression = PlayerManager.Instance.GetPlayerComponent<PlayerProgression>();
        if (playerProgression != null)
        {
            playerProgression.UpdateStat(StatType.MaxHealth, 1);
        }
    }

    [MenuItem("DevTools/Stats/Increase Health Regen")]
    public static void IncreaseHealthRegen()
    {
        var playerProgression = PlayerManager.Instance.GetPlayerComponent<PlayerProgression>();
        if (playerProgression!=null)
        {
            playerProgression.UpdateStat(StatType.HealthRegen, 1);
        }
    }

    
}
