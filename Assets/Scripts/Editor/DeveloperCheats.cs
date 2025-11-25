
using Codice.Client.BaseCommands;
using Game.Effects;
using Game.GameSettings;
using Game.Map;
using Game.Progression;
using Game.Scene;
using Game.Waves;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DeveloperCheats 
{
    private static List<MapLayer> mapToTest = null;

    [MenuItem("DevTools/Level/GoToNextLevel")]
    public static void GoToNextLevel()
    {
        PauseManager.Instance.SetPause(false);
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

    [MenuItem("DevTools/Stats/Increase Skill Duration")]
    public static void IncreaseSkillDuration()
    {
        var playerProgression = PlayerManager.Instance.GetPlayerComponent<PlayerProgression>();
        if (playerProgression != null)
        {
            playerProgression.UpdateStat(StatType.SkillDuration, 1);
        }
    }

    [MenuItem("DevTools/Items/Give holy cross")]
    public static void GiveholyCross()
    {
        var effectStore = PlayerManager.Instance.GetPlayerComponent<EffectStore>();
        if(effectStore != null)
        {
            string holyCrossEffectId = "2187437d-7198-4524-9bdb-3eeede1a728c";
            effectStore.AddEffect(EffectsDatabase.GetAvailableEffectById(holyCrossEffectId));
        }

    }

    [MenuItem("DevTools/Items/Give Blessed arrows")]
    public static void GiveBlessedArrows()
    {
        var effectStore = PlayerManager.Instance.GetPlayerComponent<EffectStore>();
        if (effectStore != null)
        {
            string effectId = "8338f0dd-7e68-4ed9-af01-aebabcc5688b";
            effectStore.AddEffect(EffectsDatabase.GetAvailableEffectById(effectId));
        }

    }

    [MenuItem("DevTools/Map/Generate Map")]
    public static void TestgenerateMap()
    {
        var actConfig = Resources.Load<ActConfig>("Levels/Acts/Act1");
        if(actConfig == null)
        {
            Debug.LogError("ActConfig not found in Resources/Levels/Acts/Act1");
            return;
        }
        MapGenerator mapGenerator = new MapGenerator();
        var map = mapGenerator.GenerateActMap(actConfig);
        Debug.Log("Generated map with " + map.Count + " layers.");
        Debug.Log(MapDebugUtils.MapToString(map));
        mapToTest = map;
    }

}
