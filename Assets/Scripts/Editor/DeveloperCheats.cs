
using Codice.Client.BaseCommands;
using Game.AI;
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

    [MenuItem("DevTools/Level/Skip wave")]
    public static void SkipWave()
    {
        PauseManager.Instance.SetPause(false);
        WaveSpawner.Instance.ReduceWaveTimer();
    }

    [MenuItem("DevTools/Progression/Give 5 XP")]
    public static void IncreaseGive5XP()
    {
        var playerProgression = PlayerManager.Instance.GetPlayerComponent<PlayerExperience>();
        if (playerProgression != null)
        {
            playerProgression.AddExperience(5);
        }
    }

    [MenuItem("DevTools/Progression/Give 20 XP")]
    public static void IncreaseGive20XP()
    {
        var playerProgression = PlayerManager.Instance.GetPlayerComponent<PlayerExperience>();
        if (playerProgression != null)
        {
            playerProgression.AddExperience(20);
        }
    }

    [MenuItem("DevTools/Progression/Give 200 XP")]
    public static void IncreaseGive200XP()
    {
        var playerProgression = PlayerManager.Instance.GetPlayerComponent<PlayerExperience>();
        if (playerProgression != null)
        {
            playerProgression.AddExperience(200);
        }
    }

    [MenuItem("DevTools/Stats/Increase Max Health")]
    public static void IncreaseMaxHealth()
    {
        var playerProgression = PlayerManager.Instance.GetPlayerComponent<PlayerProgression>();
        if (playerProgression != null)
        {
            playerProgression.UpdateStat(StatType.MaxHealth, 30);
        }
    }

    [MenuItem("DevTools/Stats/Increase Health Regen")]
    public static void IncreaseHealthRegen()
    {
        var playerProgression = PlayerManager.Instance.GetPlayerComponent<PlayerProgression>();
        if (playerProgression!=null)
        {
            playerProgression.UpdateStat(StatType.HealthRegen, 10);
        }
    }

    [MenuItem("DevTools/Stats/Increase Melee Damage")]
    public static void IncreaseMeleeDamage()
    {
        var playerProgression = PlayerManager.Instance.GetPlayerComponent<PlayerProgression>();
        if (playerProgression != null)
        {
            playerProgression.UpdateStat(StatType.MeleeDamage, 10);
        }
    }

    [MenuItem("DevTools/Stats/Increase Ranged Damage")]
    public static void IncreaseRangedDamage()
    {
        var playerProgression = PlayerManager.Instance.GetPlayerComponent<PlayerProgression>();
        if (playerProgression != null)
        {
            playerProgression.UpdateStat(StatType.RangedDamage, 10);
        }
    }

    [MenuItem("DevTools/Stats/Increase Skill Duration")]
    public static void IncreaseSkillDuration()
    {
        var playerProgression = PlayerManager.Instance.GetPlayerComponent<PlayerProgression>();
        if (playerProgression != null)
        {
            playerProgression.UpdateStat(StatType.SkillDuration, 10);
        }
    }

    [MenuItem("DevTools/Stats/Increase Lifesteal")]
    public static void IncreaseSkillLifesteal()
    {
        var playerProgression = PlayerManager.Instance.GetPlayerComponent<PlayerProgression>();
        if (playerProgression != null)
        {
            playerProgression.UpdateStat(StatType.LifeSteal, 10);
        }
    }

    [MenuItem("DevTools/Items/Give Concecrate")]
    public static void GiveholyCross()
    {
        var effectStore = PlayerManager.Instance.GetPlayerComponent<EffectStore>();
        if(effectStore != null)
        {
            string holyCrossEffectId = "849c2af3-bc0d-4fca-a444-20c03195458d";
            effectStore.AddEffect(EffectsDatabaseProvider.GetAvailableEffectById(holyCrossEffectId));
        }

    }

    [MenuItem("DevTools/Items/Holy Orb")]
    public static void GiveHolyOrb()
    {
        var effectStore = PlayerManager.Instance.GetPlayerComponent<EffectStore>();
        if (effectStore != null)
        {
            string holyCrossEffectId = "e79ea8a3-ed55-499e-b87e-ed5edab1e9d3";
            effectStore.AddEffect(EffectsDatabaseProvider.GetAvailableEffectById(holyCrossEffectId));
        }

    }

    [MenuItem("DevTools/Items/Holy Missiles")]
    public static void GiveHolyMissiles()
    {
        var effectStore = PlayerManager.Instance.GetPlayerComponent<EffectStore>();
        if (effectStore != null)
        {
            string holyCrossEffectId = "2bf80c37-2cb1-4792-b7b1-39ff7ac47490";
            effectStore.AddEffect(EffectsDatabaseProvider.GetAvailableEffectById(holyCrossEffectId));
        }

    }

    [MenuItem("DevTools/Items/Give Blessed arrows")]
    public static void GiveBlessedArrows()
    {
        var effectStore = PlayerManager.Instance.GetPlayerComponent<EffectStore>();
        if (effectStore != null)
        {
            string effectId = "2d72897d-a44f-47ab-bd4f-cba9e0acd0d8";
            effectStore.AddEffect(EffectsDatabaseProvider.GetAvailableEffectById(effectId));
        }

    }
    [MenuItem("DevTools/Items/Ancient Talisman Of Power")]
    public static void GiveTalismanOfPower()
    {
        var effectStore = PlayerManager.Instance.GetPlayerComponent<EffectStore>();
        if (effectStore != null)
        {
            string effectId = "976d761f-eb94-4764-94aa-546b0c5b1f92";
            effectStore.AddEffect(EffectsDatabaseProvider.GetAvailableEffectById(effectId));
        }

    }
    [MenuItem("DevTools/Items/Crusader Blessing")]
    public static void GiveCrusaderBlessing()
    {
        var effectStore = PlayerManager.Instance.GetPlayerComponent<EffectStore>();
        if (effectStore != null)
        {
            string effectId = "91b5f1c5-d818-4dd7-90b4-5a04c82c5209";
            effectStore.AddEffect(EffectsDatabaseProvider.GetAvailableEffectById(effectId));
        }

    }

    [MenuItem("DevTools/Items/Cursed Coin")]
    public static void GiveCursedCoing()
    {
        var effectStore = PlayerManager.Instance.GetPlayerComponent<EffectStore>();
        if (effectStore != null)
        {
            string effectId = "38e26abe-c8c3-410c-a47f-0734f9604b77";
            effectStore.AddEffect(EffectsDatabaseProvider.GetAvailableEffectById(effectId));
        }

    }
    [MenuItem("DevTools/Items/Goblet of Sacrifice")]
    public static void GiveGobletOfSacrifice()
    {
        var effectStore = PlayerManager.Instance.GetPlayerComponent<EffectStore>();
        if (effectStore != null)
        {
            string effectId = "e03ebe22-0327-4bb0-9e9a-240e896e6565";
            effectStore.AddEffect(EffectsDatabaseProvider.GetAvailableEffectById(effectId));
        }

    }

    [MenuItem("DevTools/Items/Heavy Plate Armor")]
    public static void GiveHeavyPlateArmor()
    {
        var effectStore = PlayerManager.Instance.GetPlayerComponent<EffectStore>();
        if (effectStore != null)
        {
            string effectId = "60a1824a-1ffe-4866-8493-6d28d5e32c2d";
            effectStore.AddEffect(EffectsDatabaseProvider.GetAvailableEffectById(effectId));
        }

    }

    [MenuItem("DevTools/Items/Swords of Fire")]
    public static void GiveSwordOfFire()
    {
        var effectStore = PlayerManager.Instance.GetPlayerComponent<EffectStore>();
        if (effectStore != null)
        {
            string effectId = "f91c7609-c550-47cb-acf4-7c44a4ce5528";
            effectStore.AddEffect(EffectsDatabaseProvider.GetAvailableEffectById(effectId));
        }

    }

    [MenuItem("DevTools/Items/Sacred Writtings")]
    public static void GiveSacredWrittings()
    {
        var effectStore = PlayerManager.Instance.GetPlayerComponent<EffectStore>();
        if (effectStore != null)
        {
            string effectId = "8064e3c0-a56e-4051-bcc4-53ff6e120290";
            effectStore.AddEffect(EffectsDatabaseProvider.GetAvailableEffectById(effectId));
        }

    }

    [MenuItem("DevTools/Items/Frost Arrows")]
    public static void GiveFrostArrows()
    {
        var effectStore = PlayerManager.Instance.GetPlayerComponent<EffectStore>();
        if (effectStore != null)
        {
            string effectId = "fea26ff8-e38c-4869-8d81-faba2e6fa69e";
            effectStore.AddEffect(EffectsDatabaseProvider.GetAvailableEffectById(effectId));
        }

    }

    [MenuItem("DevTools/Items/Fire Arrows")]
    public static void GiveFireArrows()
    {
        var effectStore = PlayerManager.Instance.GetPlayerComponent<EffectStore>();
        if (effectStore != null)
        {
            string effectId = "222676b1-9b64-480a-83c6-885b121930c2";
            effectStore.AddEffect(EffectsDatabaseProvider.GetAvailableEffectById(effectId));
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

    [MenuItem("DevTools/Spawn Enemies/Flame Shooter")]
    public static void SpawnFlameShooter()
    {
        var waveSpawner = WaveSpawner.Instance;
        if (waveSpawner != null)
        {
            waveSpawner.SpawnEnemy(EnemyType.FlameShooter);
        }
        else 
        {
            Debug.LogError("WaveSpawner instance not found.");
        }
    }

    [MenuItem("DevTools/Spawn Enemies/Dreko Eye")]
    public static void SpawnDrekoEye()
    {
        var waveSpawner = WaveSpawner.Instance;
        if (waveSpawner != null)
        {
            waveSpawner.SpawnEnemy(EnemyType.DrekoEye);
        }
        else
        {
            Debug.LogError("WaveSpawner instance not found.");
        }
    }

    [MenuItem("DevTools/Spawn Enemies/Light Devourer")]
    public static void SpawnLightDevourer()
    {
        var waveSpawner = WaveSpawner.Instance;
        if (waveSpawner != null)
        {
            waveSpawner.SpawnEnemy(EnemyType.LightDevourerAttacker);
        }
        else
        {
            Debug.LogError("WaveSpawner instance not found.");
        }
    }

    [MenuItem("DevTools/Spawn Enemies/Explosive Slime")]
    public static void SpawnExplosiveSlime()
    {
        var waveSpawner = WaveSpawner.Instance;
        if (waveSpawner != null)
        {
            waveSpawner.SpawnEnemy(EnemyType.SlimeExplosive);
        }
        else
        {
            Debug.LogError("WaveSpawner instance not found.");
        }
    }

}
