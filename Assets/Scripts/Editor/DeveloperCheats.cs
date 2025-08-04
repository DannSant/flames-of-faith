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
}
