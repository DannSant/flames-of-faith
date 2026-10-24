
using Codice.Client.BaseCommands;
using Game.AI;
using Game.Combat;
using Game.Currency;
using Game.Effects;
using Game.GameSettings;
using Game.Progression;
using Game.Scene;
using Game.Waves;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DeveloperCheats 
{   

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

        [MenuItem("DevTools/Progression/Give 1000 Coins")]
    public static void IncreaseGive1000Coins()
    {
        var playerProgression = PlayerManager.Instance.GetPlayerComponent<CurrencyWallet>();
        if (playerProgression != null)
        {
            playerProgression.AddCurrency(1000);
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

    // --- Stats: one menu per amount, one item per stat (Cleave is retired, so it's left out) ---

    [MenuItem("DevTools/Stats/Increase +1/MaxHealth")] public static void Increase1MaxHealth() => ChangeStat(StatType.MaxHealth, 1);
    [MenuItem("DevTools/Stats/Increase +1/MeleeDamage")] public static void Increase1MeleeDamage() => ChangeStat(StatType.MeleeDamage, 1);
    [MenuItem("DevTools/Stats/Increase +1/AttackSpeed")] public static void Increase1AttackSpeed() => ChangeStat(StatType.AttackSpeed, 1);
    [MenuItem("DevTools/Stats/Increase +1/MoveSpeed")] public static void Increase1MoveSpeed() => ChangeStat(StatType.MoveSpeed, 1);
    [MenuItem("DevTools/Stats/Increase +1/DashCooldown")] public static void Increase1DashCooldown() => ChangeStat(StatType.DashCooldown, 1);
    [MenuItem("DevTools/Stats/Increase +1/GraceAffinity")] public static void Increase1GraceAffinity() => ChangeStat(StatType.GraceAffinity, 1);
    [MenuItem("DevTools/Stats/Increase +1/Armor")] public static void Increase1Armor() => ChangeStat(StatType.Armor, 1);
    [MenuItem("DevTools/Stats/Increase +1/ExperienceToLevelUpReduction")] public static void Increase1ExperienceToLevelUpReduction() => ChangeStat(StatType.ExperienceToLevelUpReduction, 1);
    [MenuItem("DevTools/Stats/Increase +1/Luck")] public static void Increase1Luck() => ChangeStat(StatType.Luck, 1);
    [MenuItem("DevTools/Stats/Increase +1/RangedDamage")] public static void Increase1RangedDamage() => ChangeStat(StatType.RangedDamage, 1);
    [MenuItem("DevTools/Stats/Increase +1/Range")] public static void Increase1Range() => ChangeStat(StatType.Range, 1);
    [MenuItem("DevTools/Stats/Increase +1/Pierce")] public static void Increase1Pierce() => ChangeStat(StatType.Pierce, 1);
    [MenuItem("DevTools/Stats/Increase +1/MagicDamage")] public static void Increase1MagicDamage() => ChangeStat(StatType.MagicDamage, 1);
    [MenuItem("DevTools/Stats/Increase +1/HealthRegen")] public static void Increase1HealthRegen() => ChangeStat(StatType.HealthRegen, 1);
    [MenuItem("DevTools/Stats/Increase +1/LifeSteal")] public static void Increase1LifeSteal() => ChangeStat(StatType.LifeSteal, 1);
    [MenuItem("DevTools/Stats/Increase +1/SkillDuration")] public static void Increase1SkillDuration() => ChangeStat(StatType.SkillDuration, 1);
    [MenuItem("DevTools/Stats/Increase +1/HealingReceived")] public static void Increase1HealingReceived() => ChangeStat(StatType.HealingReceived, 1);
    [MenuItem("DevTools/Stats/Increase +1/PickupRange")] public static void Increase1PickupRange() => ChangeStat(StatType.PickupRange, 1);
    [MenuItem("DevTools/Stats/Increase +1/MastowAffinity")] public static void Increase1MastowAffinity() => ChangeStat(StatType.MastowAffinity, 1);
    [MenuItem("DevTools/Stats/Increase +1/ShopItemDiscount")] public static void Increase1ShopItemDiscount() => ChangeStat(StatType.ShopItemDiscount, 1);
    [MenuItem("DevTools/Stats/Increase +1/Retribution")] public static void Increase1Retribution() => ChangeStat(StatType.Retribution, 1);
    [MenuItem("DevTools/Stats/Increase +1/Evasion")] public static void Increase1Evasion() => ChangeStat(StatType.Evasion, 1);

    [MenuItem("DevTools/Stats/Decrease -1/MaxHealth")] public static void Decrease1MaxHealth() => ChangeStat(StatType.MaxHealth, -1);
    [MenuItem("DevTools/Stats/Decrease -1/MeleeDamage")] public static void Decrease1MeleeDamage() => ChangeStat(StatType.MeleeDamage, -1);
    [MenuItem("DevTools/Stats/Decrease -1/AttackSpeed")] public static void Decrease1AttackSpeed() => ChangeStat(StatType.AttackSpeed, -1);
    [MenuItem("DevTools/Stats/Decrease -1/MoveSpeed")] public static void Decrease1MoveSpeed() => ChangeStat(StatType.MoveSpeed, -1);
    [MenuItem("DevTools/Stats/Decrease -1/DashCooldown")] public static void Decrease1DashCooldown() => ChangeStat(StatType.DashCooldown, -1);
    [MenuItem("DevTools/Stats/Decrease -1/GraceAffinity")] public static void Decrease1GraceAffinity() => ChangeStat(StatType.GraceAffinity, -1);
    [MenuItem("DevTools/Stats/Decrease -1/Armor")] public static void Decrease1Armor() => ChangeStat(StatType.Armor, -1);
    [MenuItem("DevTools/Stats/Decrease -1/ExperienceToLevelUpReduction")] public static void Decrease1ExperienceToLevelUpReduction() => ChangeStat(StatType.ExperienceToLevelUpReduction, -1);
    [MenuItem("DevTools/Stats/Decrease -1/Luck")] public static void Decrease1Luck() => ChangeStat(StatType.Luck, -1);
    [MenuItem("DevTools/Stats/Decrease -1/RangedDamage")] public static void Decrease1RangedDamage() => ChangeStat(StatType.RangedDamage, -1);
    [MenuItem("DevTools/Stats/Decrease -1/Range")] public static void Decrease1Range() => ChangeStat(StatType.Range, -1);
    [MenuItem("DevTools/Stats/Decrease -1/Pierce")] public static void Decrease1Pierce() => ChangeStat(StatType.Pierce, -1);
    [MenuItem("DevTools/Stats/Decrease -1/MagicDamage")] public static void Decrease1MagicDamage() => ChangeStat(StatType.MagicDamage, -1);
    [MenuItem("DevTools/Stats/Decrease -1/HealthRegen")] public static void Decrease1HealthRegen() => ChangeStat(StatType.HealthRegen, -1);
    [MenuItem("DevTools/Stats/Decrease -1/LifeSteal")] public static void Decrease1LifeSteal() => ChangeStat(StatType.LifeSteal, -1);
    [MenuItem("DevTools/Stats/Decrease -1/SkillDuration")] public static void Decrease1SkillDuration() => ChangeStat(StatType.SkillDuration, -1);
    [MenuItem("DevTools/Stats/Decrease -1/HealingReceived")] public static void Decrease1HealingReceived() => ChangeStat(StatType.HealingReceived, -1);
    [MenuItem("DevTools/Stats/Decrease -1/PickupRange")] public static void Decrease1PickupRange() => ChangeStat(StatType.PickupRange, -1);
    [MenuItem("DevTools/Stats/Decrease -1/MastowAffinity")] public static void Decrease1MastowAffinity() => ChangeStat(StatType.MastowAffinity, -1);
    [MenuItem("DevTools/Stats/Decrease -1/ShopItemDiscount")] public static void Decrease1ShopItemDiscount() => ChangeStat(StatType.ShopItemDiscount, -1);
    [MenuItem("DevTools/Stats/Decrease -1/Retribution")] public static void Decrease1Retribution() => ChangeStat(StatType.Retribution, -1);
    [MenuItem("DevTools/Stats/Decrease -1/Evasion")] public static void Decrease1Evasion() => ChangeStat(StatType.Evasion, -1);

    [MenuItem("DevTools/Stats/Increase +10/MaxHealth")] public static void Increase10MaxHealth() => ChangeStat(StatType.MaxHealth, 10);
    [MenuItem("DevTools/Stats/Increase +10/MeleeDamage")] public static void Increase10MeleeDamage() => ChangeStat(StatType.MeleeDamage, 10);
    [MenuItem("DevTools/Stats/Increase +10/AttackSpeed")] public static void Increase10AttackSpeed() => ChangeStat(StatType.AttackSpeed, 10);
    [MenuItem("DevTools/Stats/Increase +10/MoveSpeed")] public static void Increase10MoveSpeed() => ChangeStat(StatType.MoveSpeed, 10);
    [MenuItem("DevTools/Stats/Increase +10/DashCooldown")] public static void Increase10DashCooldown() => ChangeStat(StatType.DashCooldown, 10);
    [MenuItem("DevTools/Stats/Increase +10/GraceAffinity")] public static void Increase10GraceAffinity() => ChangeStat(StatType.GraceAffinity, 10);
    [MenuItem("DevTools/Stats/Increase +10/Armor")] public static void Increase10Armor() => ChangeStat(StatType.Armor, 10);
    [MenuItem("DevTools/Stats/Increase +10/ExperienceToLevelUpReduction")] public static void Increase10ExperienceToLevelUpReduction() => ChangeStat(StatType.ExperienceToLevelUpReduction, 10);
    [MenuItem("DevTools/Stats/Increase +10/Luck")] public static void Increase10Luck() => ChangeStat(StatType.Luck, 10);
    [MenuItem("DevTools/Stats/Increase +10/RangedDamage")] public static void Increase10RangedDamage() => ChangeStat(StatType.RangedDamage, 10);
    [MenuItem("DevTools/Stats/Increase +10/Range")] public static void Increase10Range() => ChangeStat(StatType.Range, 10);
    [MenuItem("DevTools/Stats/Increase +10/Pierce")] public static void Increase10Pierce() => ChangeStat(StatType.Pierce, 10);
    [MenuItem("DevTools/Stats/Increase +10/MagicDamage")] public static void Increase10MagicDamage() => ChangeStat(StatType.MagicDamage, 10);
    [MenuItem("DevTools/Stats/Increase +10/HealthRegen")] public static void Increase10HealthRegen() => ChangeStat(StatType.HealthRegen, 10);
    [MenuItem("DevTools/Stats/Increase +10/LifeSteal")] public static void Increase10LifeSteal() => ChangeStat(StatType.LifeSteal, 10);
    [MenuItem("DevTools/Stats/Increase +10/SkillDuration")] public static void Increase10SkillDuration() => ChangeStat(StatType.SkillDuration, 10);
    [MenuItem("DevTools/Stats/Increase +10/HealingReceived")] public static void Increase10HealingReceived() => ChangeStat(StatType.HealingReceived, 10);
    [MenuItem("DevTools/Stats/Increase +10/PickupRange")] public static void Increase10PickupRange() => ChangeStat(StatType.PickupRange, 10);
    [MenuItem("DevTools/Stats/Increase +10/MastowAffinity")] public static void Increase10MastowAffinity() => ChangeStat(StatType.MastowAffinity, 10);
    [MenuItem("DevTools/Stats/Increase +10/ShopItemDiscount")] public static void Increase10ShopItemDiscount() => ChangeStat(StatType.ShopItemDiscount, 10);
    [MenuItem("DevTools/Stats/Increase +10/Retribution")] public static void Increase10Retribution() => ChangeStat(StatType.Retribution, 10);
    [MenuItem("DevTools/Stats/Increase +10/Evasion")] public static void Increase10Evasion() => ChangeStat(StatType.Evasion, 10);

    [MenuItem("DevTools/Stats/Increase +50/MaxHealth")] public static void Increase50MaxHealth() => ChangeStat(StatType.MaxHealth, 50);
    [MenuItem("DevTools/Stats/Increase +50/MeleeDamage")] public static void Increase50MeleeDamage() => ChangeStat(StatType.MeleeDamage, 50);
    [MenuItem("DevTools/Stats/Increase +50/AttackSpeed")] public static void Increase50AttackSpeed() => ChangeStat(StatType.AttackSpeed, 50);
    [MenuItem("DevTools/Stats/Increase +50/MoveSpeed")] public static void Increase50MoveSpeed() => ChangeStat(StatType.MoveSpeed, 50);
    [MenuItem("DevTools/Stats/Increase +50/DashCooldown")] public static void Increase50DashCooldown() => ChangeStat(StatType.DashCooldown, 50);
    [MenuItem("DevTools/Stats/Increase +50/GraceAffinity")] public static void Increase50GraceAffinity() => ChangeStat(StatType.GraceAffinity, 50);
    [MenuItem("DevTools/Stats/Increase +50/Armor")] public static void Increase50Armor() => ChangeStat(StatType.Armor, 50);
    [MenuItem("DevTools/Stats/Increase +50/ExperienceToLevelUpReduction")] public static void Increase50ExperienceToLevelUpReduction() => ChangeStat(StatType.ExperienceToLevelUpReduction, 50);
    [MenuItem("DevTools/Stats/Increase +50/Luck")] public static void Increase50Luck() => ChangeStat(StatType.Luck, 50);
    [MenuItem("DevTools/Stats/Increase +50/RangedDamage")] public static void Increase50RangedDamage() => ChangeStat(StatType.RangedDamage, 50);
    [MenuItem("DevTools/Stats/Increase +50/Range")] public static void Increase50Range() => ChangeStat(StatType.Range, 50);
    [MenuItem("DevTools/Stats/Increase +50/Pierce")] public static void Increase50Pierce() => ChangeStat(StatType.Pierce, 50);
    [MenuItem("DevTools/Stats/Increase +50/MagicDamage")] public static void Increase50MagicDamage() => ChangeStat(StatType.MagicDamage, 50);
    [MenuItem("DevTools/Stats/Increase +50/HealthRegen")] public static void Increase50HealthRegen() => ChangeStat(StatType.HealthRegen, 50);
    [MenuItem("DevTools/Stats/Increase +50/LifeSteal")] public static void Increase50LifeSteal() => ChangeStat(StatType.LifeSteal, 50);
    [MenuItem("DevTools/Stats/Increase +50/SkillDuration")] public static void Increase50SkillDuration() => ChangeStat(StatType.SkillDuration, 50);
    [MenuItem("DevTools/Stats/Increase +50/HealingReceived")] public static void Increase50HealingReceived() => ChangeStat(StatType.HealingReceived, 50);
    [MenuItem("DevTools/Stats/Increase +50/PickupRange")] public static void Increase50PickupRange() => ChangeStat(StatType.PickupRange, 50);
    [MenuItem("DevTools/Stats/Increase +50/MastowAffinity")] public static void Increase50MastowAffinity() => ChangeStat(StatType.MastowAffinity, 50);
    [MenuItem("DevTools/Stats/Increase +50/ShopItemDiscount")] public static void Increase50ShopItemDiscount() => ChangeStat(StatType.ShopItemDiscount, 50);
    [MenuItem("DevTools/Stats/Increase +50/Retribution")] public static void Increase50Retribution() => ChangeStat(StatType.Retribution, 50);
    [MenuItem("DevTools/Stats/Increase +50/Evasion")] public static void Increase50Evasion() => ChangeStat(StatType.Evasion, 50);

    private static void ChangeStat(StatType stat, int amount)
    {
        if (PlayerManager.Instance == null) return;
        var playerProgression = PlayerManager.Instance.GetPlayerComponent<PlayerProgression>();
        if (playerProgression == null) return;

        playerProgression.UpdateStat(stat, amount);
        Debug.Log($"[DevTools] {stat} is now {playerProgression.GetStatTotal(stat)}");

        // The next level respawns the player from PlayerData, which is only written when a level ends,
        // so on the map write it now or the change is lost on entering the next level
        if (PlayerManager.Instance.IsPlayerOnMap)
        {
            PlayerManager.Instance.SaveAllPlayerComponentStates();
        }
    }

    [MenuItem("DevTools/Items/Give Glyph of Consecrate")]
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

    [MenuItem("DevTools/Items/Glyph of Holy Missiles")]
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

    [MenuItem("DevTools/Items/Ring of Vitality")]
    public static void GiveRingOfVitality()
    {
        var effectStore = PlayerManager.Instance.GetPlayerComponent<EffectStore>();
        if (effectStore != null)
        {
            string effectId = "a2331bad-46bb-46ba-b3f8-865cfd64fe31";
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

    [MenuItem("DevTools/Items/Ley Flower")]
    public static void GiveLeyFlower()
    {
        var effectStore = PlayerManager.Instance.GetPlayerComponent<EffectStore>();
        if (effectStore != null)
        {
            string effectId = "c4f5c675-8a9e-4976-80fa-52b15a072fee";
            effectStore.AddEffect(EffectsDatabaseProvider.GetAvailableEffectById(effectId));
        }

    }

    [MenuItem("DevTools/Items/Charisma Amulet")]
    public static void GiveCharismaAmulet()
    {
        var effectStore = PlayerManager.Instance.GetPlayerComponent<EffectStore>();
        if (effectStore != null)
        {
            string effectId = "1f52c934-2feb-4259-ab5e-78b75b6a6f88";
            effectStore.AddEffect(EffectsDatabaseProvider.GetAvailableEffectById(effectId));
        }

    }

    [MenuItem("DevTools/Items/Bloody Amulet")]
    public static void GiveBloodyAmulet()
    {
        var effectStore = PlayerManager.Instance.GetPlayerComponent<EffectStore>();
        if (effectStore != null)
        {
            string effectId = "416e66a7-736d-4b7f-a909-201a7c70e913";
            effectStore.AddEffect(EffectsDatabaseProvider.GetAvailableEffectById(effectId));
        }

    }

    [MenuItem("DevTools/Items/Ley Arrows")]
    public static void GiveLeyArrows()
    {
        var effectStore = PlayerManager.Instance.GetPlayerComponent<EffectStore>();
        if (effectStore != null)
        {
            string effectId = "21267774-d53f-447e-85ca-5dec3571ab96";
            effectStore.AddEffect(EffectsDatabaseProvider.GetAvailableEffectById(effectId));
        }

    }

    [MenuItem("DevTools/Items/Ley Fracture")]
    public static void GiveLeyFracture()
    {
        var effectStore = PlayerManager.Instance.GetPlayerComponent<EffectStore>();
        if (effectStore != null)
        {
            string effectId = "806ff2a0-88d4-4ee5-aeaa-c2e86c9dc72b";
            effectStore.AddEffect(EffectsDatabaseProvider.GetAvailableEffectById(effectId));
        }

    }

    
    [MenuItem("DevTools/Items/Glyph of Vengeance Daggers")]
    public static void GiveGlyphOfVengeanceDaggers()
    {
        var effectStore = PlayerManager.Instance.GetPlayerComponent<EffectStore>();
        if (effectStore != null)
        {
            string effectId = "cfc621a3-c452-497f-ad24-a61f5fb60c22";
            effectStore.AddEffect(EffectsDatabaseProvider.GetAvailableEffectById(effectId));
        }

    }

    [MenuItem("DevTools/Items/Glyph of Strafe")]
    public static void GiveGlyphOfStrafe()
    {
        var effectStore = PlayerManager.Instance.GetPlayerComponent<EffectStore>();
        if (effectStore != null)
        {
            string effectId = "f432e5fc-d77c-4ced-aef7-e4f17ba18f56";
            effectStore.AddEffect(EffectsDatabaseProvider.GetAvailableEffectById(effectId));
        }

    }

        [MenuItem("DevTools/Items/Glyph of Ether Thunder")]
    public static void GiveGlyphOfEtherThunder()
    {
        var effectStore = PlayerManager.Instance.GetPlayerComponent<EffectStore>();
        if (effectStore != null)
        {
            string effectId = "a07699b6-f08a-4a79-9d46-36c60f8c0259";
            effectStore.AddEffect(EffectsDatabaseProvider.GetAvailableEffectById(effectId));
        }

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

    [MenuItem("DevTools/Spawn Enemies/Chaser Slime")]
    public static void SpawnChaserSlime()
    {
        var waveSpawner = WaveSpawner.Instance;
        if (waveSpawner != null)
        {
            waveSpawner.SpawnEnemy(EnemyType.SlimeChaser);
        }
        else
        {
            Debug.LogError("WaveSpawner instance not found.");
        }
    }

    [MenuItem("DevTools/Player/Decimate Health")]
    public static void DecimateHealth()
    {
        var playerHealth = PlayerManager.Instance.GetPlayerComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(playerHealth.GetCurrentHealth() - 1); // Reduce health to 1
        }
        else
        {
            Debug.LogError("PlayerHealth instance not found.");
        }
    }

    [MenuItem("DevTools/Player/Full Heal")]
    public static void FullHeal()
    {
        var playerHealth = PlayerManager.Instance.GetPlayerComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.RestoreHealth();
        }
        else
        {
            Debug.LogError("PlayerHealth instance not found.");
        }
    }

    // --- Overworld map (Play mode, on the map). Same actions as the Ctrl+Shift+D cheat window. ---

    private static Game.Overworld.MapRunController MapForCheats =>
        Application.isPlaying && Game.Overworld.MapRunController.Instance != null
        && Game.Overworld.MapRunController.Instance.IsInitialized
            ? Game.Overworld.MapRunController.Instance
            : null;

    [MenuItem("DevTools/Map/Reveal full map")]
    public static void RevealFullMap() => MapForCheats?.DebugRevealAll();

    [MenuItem("DevTools/Map/Toggle free travel")]
    public static void ToggleFreeTravel()
    {
        var map = MapForCheats;
        if (map == null) return;
        map.FreeTravel = !map.FreeTravel;
        Debug.Log($"[DevTools] Free travel {(map.FreeTravel ? "ON" : "OFF")}");
    }

    [MenuItem("DevTools/Map/Beat current node")]
    public static void BeatCurrentNode() => MapForCheats?.DebugBeatCurrentNode();

    [MenuItem("DevTools/Map/Go to Act 1")]
    public static void GoToAct1() => MapForCheats?.DebugGoToAct(1);

    [MenuItem("DevTools/Map/Go to Act 2")]
    public static void GoToAct2() => MapForCheats?.DebugGoToAct(2);

    [MenuItem("DevTools/Map/Go to Act 3")]
    public static void GoToAct3() => MapForCheats?.DebugGoToAct(3);

    [MenuItem("DevTools/Map/Reveal full map", true)]
    [MenuItem("DevTools/Map/Toggle free travel", true)]
    [MenuItem("DevTools/Map/Beat current node", true)]
    [MenuItem("DevTools/Map/Go to Act 1", true)]
    [MenuItem("DevTools/Map/Go to Act 2", true)]
    [MenuItem("DevTools/Map/Go to Act 3", true)]
    private static bool ValidateMapCheats() =>
        MapForCheats != null && PlayerManager.Instance != null && PlayerManager.Instance.IsPlayerOnMap;
}
