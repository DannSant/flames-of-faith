using Game.AI;
using Game.Combat;
using Game.Effects;
using Game.Effects.Abilities;
using Game.Enemies;
using Game.Loot;
using Game.Progression;
using Game.Waves;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace Game.BalanceData.Editor
{
    /// <summary>
    /// Read-only tables computed from the balance data with the game's own formulas (EnemyDamageCalculator,
    /// Enemy.CalculateHealth, ExperienceSettings, LootRoller), so the designer sees what the numbers do without Unity. Written to
    /// BalanceData/Reports on every export and import; editing them changes nothing.
    /// </summary>
    public static class BalanceReports
    {
        private const int XpLevels = 30;
        private static readonly int[] LootLuck = { -5, 0, 5, 10 };
        private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

        private static string F(float value, string format = "0.##") => value.ToString(format, Invariant);

        public static List<BalanceTable> All()
        {
            var damage = BalanceTablesLoad<EnemyDamageSettings>(BalanceTables.EnemyDamageSettingsPath);
            var experience = BalanceTablesLoad<ExperienceSettings>(BalanceTables.ExperienceSettingsPath);
            var enemies = BalanceTables.LoadAll<EnemyData>(BalanceTables.EnemiesFolder).OrderBy(e => e.enemyType).ToList();
            int lastLevel = damage.ExpectedCurve.Count > 0 ? Mathf.CeilToInt(damage.ExpectedCurve[damage.ExpectedCurve.Count - 1].levelsBeaten) : 0;
            int maxWaves = Mathf.Max(1, BalanceTables.LoadAll<WaveDatabase>(BalanceTables.WavesFolder).Select(d => d.waves.Count).DefaultIfEmpty(1).Max());

            return new List<BalanceTable>
            {
                EnemyDamage(damage, enemies, lastLevel, maxWaves),
                EnemyHealth(enemies, lastLevel, maxWaves),
                Experience(experience),
                LootOdds(Resources.Load<ItemDropTable>(ItemDropTable.ResourcePath), Resources.Load<LootSettings>(LootSettings.ResourcePath), lastLevel),
                ItemDamage(ItemDamageInputs()),
            };
        }

        private static T BalanceTablesLoad<T>(string resourcesPath) where T : Object
        {
            var asset = Resources.Load<T>(resourcesPath);
            if (asset == null) throw new System.IO.FileNotFoundException($"Missing Resources/{resourcesPath}");
            return asset;
        }

        /// <summary>Each enemy attack's damage per hit, and the hits the expected player survives, at every point of the run.</summary>
        public static BalanceTable EnemyDamage(EnemyDamageSettings settings, List<EnemyData> enemies, int lastLevel, int maxWaves)
        {
            var table = new BalanceTable("EnemyDamage", new[]
            {
                "Levels Beaten", "Wave", "Expected Max Health", "Expected Armor", "Enemy", "Attack", "Tier", "Damage Per Hit", "Hits To Die",
            });
            for (int level = 0; level <= lastLevel; level++)
            {
                for (int wave = 1; wave <= maxWaves; wave++)
                {
                    float progress = EnemyDamageCalculator.GetProgress(wave, level, settings);
                    var expected = EnemyDamageCalculator.GetExpectedStats(progress, settings);
                    foreach (var enemy in enemies)
                    {
                        foreach (var kind in new[] { EnemyDamageKind.Contact, EnemyDamageKind.Projectile })
                        {
                            float tier = enemy.GetDamageTier(kind);
                            if (tier <= 0f) continue;
                            float raw = EnemyDamageCalculator.Calculate(enemy, wave, level, kind, settings);
                            int hits = EnemyDamageCalculator.SimulateHitsToDie(expected.maxHealth, expected.armor, raw, settings);
                            table.AddRow(level.ToString(), wave.ToString(), F(expected.maxHealth), F(expected.armor),
                                enemy.enemyType.ToString(), kind.ToString(), F(tier), F(raw), hits.ToString());
                        }
                    }
                }
            }
            return table;
        }

        /// <summary>Each enemy's health at every point of the run, normal and Corrupted.</summary>
        public static BalanceTable EnemyHealth(List<EnemyData> enemies, int lastLevel, int maxWaves)
        {
            float corruptedBonus = CorruptionSettings.Instance.CorruptedHealthBonus;
            var table = new BalanceTable("EnemyHealth", new[] { "Enemy", "Levels Beaten", "Wave", "Health", "Corrupted Health" });
            foreach (var enemy in enemies)
            {
                for (int level = 0; level <= lastLevel; level++)
                {
                    for (int wave = 1; wave <= maxWaves; wave++)
                    {
                        table.AddRow(enemy.enemyType.ToString(), level.ToString(), wave.ToString(),
                            Enemy.CalculateHealth(enemy, wave, level, enemy.alwaysCorrupted, corruptedBonus).ToString(),
                            Enemy.CalculateHealth(enemy, wave, level, true, corruptedBonus).ToString());
                    }
                }
            }
            return table;
        }

        /// <summary>XP needed for each level and in total.</summary>
        public static BalanceTable Experience(ExperienceSettings settings)
        {
            var table = new BalanceTable("Experience", new[] { "Level", "XP To Next Level", "Total XP To Reach Level" });
            int total = 0;
            for (int level = 1; level <= XpLevels; level++)
            {
                int required = settings.GetXPRequired(level);
                table.AddRow(level.ToString(), required.ToString(), total.ToString());
                total += required;
            }
            return table;
        }

        /// <summary>
        /// For each source, levels beaten and Luck: how many items can drop and the chance of each quality, with the
        /// roll's own math (LootRoller). Items locked by default count as locked.
        /// </summary>
        public static BalanceTable LootOdds(ItemDropTable table, LootSettings settings, int lastLevel)
        {
            var qualities = System.Enum.GetNames(typeof(EffectQuality));
            var report = new BalanceTable("LootOdds", new[] { "Source", "Levels Beaten", "Luck", "Eligible Items" }
                .Concat(qualities.Select(q => q + " %")).ToArray());
            if (table == null || settings == null) return report;

            int maxMin = table.Entries.Where(e => e != null).Select(e => e.minLevelsBeaten).DefaultIfEmpty(0).Max();
            int levels = Mathf.Max(lastLevel, maxMin);
            foreach (LootSource source in System.Enum.GetValues(typeof(LootSource)))
            {
                var sourceSettings = settings.GetSource(source);
                for (int level = 0; level <= levels; level++)
                {
                    var eligible = LootRoller.GetEligible(table.Entries, source, level, e => e.UnlockedByDefault);
                    foreach (int luck in LootLuck)
                    {
                        var odds = LootRoller.GetTierOdds(eligible, sourceSettings, luck);
                        report.AddRow(new[] { source.ToString(), level.ToString(), luck.ToString(), eligible.Count.ToString() }
                            .Concat(odds.Select(o => F(o * 100f, "0.#"))).ToArray());
                    }
                }
            }
            return report;
        }

        private static readonly int[] DamageStatPoints = { 0, 10, 20 };

        /// <summary>What one spawned attack deals before Grace: base, the damage stat it scales with, and how much.</summary>
        public struct DamageInput
        {
            public string source;
            public string name;
            public int tier;
            public int baseDamage;
            public WeaponClass damageStat;
            public float statScale;
            /// <summary>Damage multiplier at 2 copies of the item (stack scaling); 1 for abilities.</summary>
            public float twoStackMultiplier;
        }

        /// <summary>
        /// The damage of every item behavior and of the dash and explosion abilities whose prefab damage moved to
        /// their spawner, at 0 / 10 / 20 points of their damage stat and 1 / 2 copies:
        /// floor((base + points x scale) x stack multiplier), at least 1, as DamageCalculator computes it at 0 Grace.
        /// </summary>
        public static BalanceTable ItemDamage(List<DamageInput> inputs)
        {
            var table = new BalanceTable("ItemDamage", new[] { "Source", "Name", "Tier", "Damage Stat", "Stat Points", "Copies", "Damage" });
            foreach (var input in inputs.OrderBy(i => i.source, System.StringComparer.Ordinal).ThenBy(i => i.name, System.StringComparer.Ordinal).ThenBy(i => i.tier))
            {
                foreach (int points in DamageStatPoints)
                {
                    int copiesMax = input.source == "Item" ? 2 : 1;
                    for (int copies = 1; copies <= copiesMax; copies++)
                    {
                        float multiplier = copies == 1 ? 1f : input.twoStackMultiplier;
                        int damage = Mathf.Max(1, Mathf.FloorToInt((input.baseDamage + points * input.statScale) * multiplier));
                        table.AddRow(input.source, input.name, input.tier.ToString(), input.damageStat.ToString(), points.ToString(),
                            copies.ToString(), damage.ToString());
                    }
                }
            }
            return table;
        }

        // --- Where each attack's damage comes from (the damage-ownership move changes only this part) ---------------

        private static DamageSourceData SourceData(string name) =>
            Resources.Load<DamageSourceData>(BalanceTables.DamageSourcesFolder + "/" + name);

        public static List<DamageInput> ItemDamageInputs()
        {
            var inputs = new List<DamageInput>();

            // Item behaviors: the spawned prefab's data, times the item's scaling value
            foreach (var effect in BalanceTables.LoadAll<Effect>(BalanceTables.EffectsFolder))
            {
                foreach (var behavior in effect.Behaviors.Where(b => b != null && !(b is AbilityBehavior)))
                {
                    var prefabField = FieldAccess.FindField(behavior.GetType(), "prefabToSpawn");
                    var prefab = prefabField?.GetValue(behavior) as GameObject;
                    var source = prefab != null ? prefab.GetComponentInChildren<DamageSourceBase>(true) : null;
                    if (source == null || source.Data == null) continue;
                    var rules = (List<StackScalingRule>)FieldAccess.Get(behavior, "stackScaling");
                    inputs.Add(new DamageInput
                    {
                        source = "Item", name = effect.name + " / " + behavior.name, tier = 0,
                        baseDamage = source.Data.BaseDamage, damageStat = source.Data.WeaponClass,
                        statScale = source.Data.AttackScale * (effect.scalingValue > 0f ? effect.scalingValue : 1f),
                        twoStackMultiplier = new StackScaling(rules, 2).Get(StackScalingTarget.Damage),
                    });
                }
            }

            // Dashes and the Fire Tornado explosion: tier base damage, the prefab's stat and scale
            var trap = SourceData("Trap");
            var flame = SourceData("TrailFlame");
            var hitbox = SourceData("PlayerWarrior_DashHitbox");
            var explosion = SourceData("FireballExplotion");
            foreach (var ability in BalanceTables.LoadAll<AbilityBehavior>(BalanceTables.EffectBehaviorsFolder))
            {
                for (int tier = 0; tier < ability.TierCount; tier++)
                {
                    switch (ability)
                    {
                        case TrapStepAbility trapStep:
                            inputs.Add(Ability(ability, tier, trapStep.GetTier(tier).trapDamage, trap.WeaponClass, trap.AttackScale));
                            break;
                        case FlameDashAbility flameDash:
                            inputs.Add(Ability(ability, tier, flameDash.GetTier(tier).flameDamage, flame.WeaponClass, flame.AttackScale));
                            break;
                        case CrusaderChargeAbility charge:
                            var row = charge.GetTier(tier);
                            inputs.Add(Ability(ability, tier, row.damage, hitbox.WeaponClass, row.meleeDamageScale));
                            break;
                        case FireTornadoesAbility tornadoes:
                            inputs.Add(Ability(ability, tier, tornadoes.GetTier(tier).tornadoExplosionDamage, explosion.WeaponClass, explosion.AttackScale));
                            break;
                    }
                }
            }
            return inputs;
        }

        private static DamageInput Ability(AbilityBehavior ability, int tier, int baseDamage, WeaponClass stat, float scale) => new()
        {
            source = "Ability", name = ability.name, tier = tier, baseDamage = baseDamage, damageStat = stat, statScale = scale,
            twoStackMultiplier = 1f,
        };
    }
}
