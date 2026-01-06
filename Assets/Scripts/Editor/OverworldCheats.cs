using Game.GameSettings;
using Game.Overworld;
using Game.Waves;
using UnityEditor;
using UnityEngine;

public class OverworldCheats 
{
    [MenuItem("DevTools/Overworld/Clear current level")]
    public static void GoToNextLevel()
    {
        if(MapRunController.Instance != null)
        {
            MapRunController.Instance.OnLevelCleared();
        }else
        {
            Debug.LogWarning("MapRunController instance not found. Execute this method while on LevelSelection scene");
        }
    }
}
