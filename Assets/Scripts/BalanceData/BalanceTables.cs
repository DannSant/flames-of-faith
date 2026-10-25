using Game.AI.Behaviors;
using Game.Combat;
using Game.Combat.Elemental;
using Game.Control;
using Game.Effects;
using Game.Enemies;
using Game.Loot;
using Game.Progression;
using Game.Scene;
using Game.Waves;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.BalanceData
{
    /// <summary>
    /// The balance tables of the data-driven pilot: enemies, waves, XP and damage sources. Each becomes one CSV in
    /// BalanceData/ and one tab of the designer's sheet. Order matters on import: Waves runs before WaveEnemies, so the
    /// enemies of a wave the same import adds can be set. Every asset is read from Resources, so a build can apply the
    /// tables too (BalanceOverride).
    /// </summary>
    public static class BalanceTables
    {
        // Resources paths
        public const string EnemiesFolder = "Enemies";
        public const string BehaviorsFolder = "Behaviors";
        public const string WavesFolder = "Waves";
        public const string LevelsFolder = "Levels";
        public const string DamageSourcesFolder = "Combat/DamageSources";
        public const string ExperienceSettingsPath = "Progression/ExperienceSettings";
        public const string EnemyDamageSettingsPath = "Combat/EnemyDamageSettings";
        public const string DebuffsFolder = "Debuffs";
        public const string EffectsFolder = "Effects";
        public const string EffectBehaviorsFolder = "Effects/EffectBehaviors";
        public const string IconsFolder = "Icons";
        public const string ClassesFolder = "CharacterData";
        public const string CorruptionSettingsPath = "Combat/CorruptionSettings";
        public const string PierceSettingsPath = "Combat/PierceSettings";
        public const string PlayerStatSettingsPath = "Progression/PlayerStatSettings";
        public const string StatDatabasePath = "Progression/StatDatabase";

        // Technical switches on every enemy behavior, not balance numbers
        private static readonly HashSet<string> BehaviorFieldsSkipped = new() { "stopDuringGameplayFreeze", "avoidObstacles" };

        static BalanceTables()
        {
            CellCodec.ReferenceFolders[typeof(ElementalDebuffData)] = DebuffsFolder;
            CellCodec.ReferenceFolders[typeof(Sprite)] = IconsFolder;
            CellCodec.ReferenceFolders[typeof(EffectBehavior)] = EffectBehaviorsFolder;
        }

        public static IEnumerable<T> LoadAll<T>(string resourcesPath) where T : Object =>
            Resources.LoadAll<T>(resourcesPath).Where(a => a != null).Distinct();

        private static T Load<T>(string resourcesPath) where T : Object
        {
            var asset = Resources.Load<T>(resourcesPath);
            if (asset == null) throw new FileNotFoundException($"Missing Resources/{resourcesPath}");
            return asset;
        }

        public static List<BalanceTableDefinition> All()
        {
            var tables = new List<BalanceTableDefinition>
            {
                new AssetTable<EnemyData>("Enemies", "Enemy", () => LoadAll<EnemyData>(EnemiesFolder), e => e.enemyType.ToString(),
                    "healthBase", "healthPerWave", "healthPerLevel", "contactDamageTier", "projectileDamageTier",
                    "moveSpeed", "hopForce", "xpTier", "alwaysCorrupted", "overrideCorruptedHealthBonus", "corruptedHealthBonus"),
            };
            tables.AddRange(BehaviorTables());
            tables.AddRange(new BalanceTableDefinition[]
            {
                new AssetTable<LevelData>("Levels", "Level", () => LoadAll<LevelData>(LevelsFolder), l => l.name, "taintLevel"),
                new AssetTable<WaveDatabase>("WaveLevels", "Level", () => LoadAll<WaveDatabase>(WavesFolder), d => d.name,
                    "corruptorType", "lastWave"),
                new WaveTable(() => LoadAll<WaveDatabase>(WavesFolder)),
                new WaveEnemyTable(() => LoadAll<WaveDatabase>(WavesFolder)),
                new SettingsTable<ExperienceSettings>("Experience", () => Load<ExperienceSettings>(ExperienceSettingsPath),
                    "xpRequiredBase", "xpRequiredGrowth", "dropChance", "fallbackXp", "levelsPerWaveTarget",
                    "expectedKillRatio", "cooldownBlend", "xpBonusPerReductionPoint"),
                // Fixed size: each tier has a token sprite only Unity sets
                new ChildListTable<ExperienceSettings>("ExperienceTokenTiers", "Settings",
                    () => new[] { Load<ExperienceSettings>(ExperienceSettingsPath) }, s => s.name, "denominations", false,
                    ("Tier", "tier"), ("Promote Chance", "promoteChance"), ("Multiplier", "multiplier")),
                new SettingsTable<EnemyDamageSettings>("EnemyDamage", () => Load<EnemyDamageSettings>(EnemyDamageSettingsPath),
                    "waveProgressStep", "armorEffectivenessConstant", "minDamagePercent", "minDamage",
                    "evasionSoftCapPoints", "evasionPerPointBeforeSoftCap", "evasionPerPointAfterSoftCap", "evasionMaxChance",
                    "dodgeInvulnerabilityDuration"),
                new ChildListTable<EnemyDamageSettings>("EnemyDamageCurve", "Settings",
                    () => new[] { Load<EnemyDamageSettings>(EnemyDamageSettingsPath) }, s => s.name, "expectedCurve", true,
                    ("Levels Beaten", "levelsBeaten"), ("Max Health", "maxHealth"), ("Armor", "armor")),
                new ChildListTable<EnemyDamageSettings>("EnemyDamageTiers", "Settings",
                    () => new[] { Load<EnemyDamageSettings>(EnemyDamageSettingsPath) }, s => s.name, "tierHitsToKill", true,
                    ("Hits To Kill", "")),
                new AssetTable<DamageSourceData>("DamageSources", "Damage Source", () => LoadAll<DamageSourceData>(DamageSourcesFolder), d => d.name,
                    "baseDamage", "weaponClass", "attackScale", "useTriggerEnter", "useTriggerStay", "tickInterval", "shouldStackDamage",
                    "enablePierce", "pierceCount", "addPierceStat", "applyKnockback", "knockbackForce", "canTriggerLifesteal",
                    "directElementalDebuffData", "projectileSpeed", "lifetime", "lifetimePerSkillDuration"),

                // Step 3: debuffs, Grace / Corruption, player stats and formulas, class dashes
                new AssetTable<ElementalDebuffData>("Debuffs", "Debuff", () => LoadAll<ElementalDebuffData>(DebuffsFolder), d => d.name,
                    ("Base Duration", "baseDuration"), ("Duration Per Mastow", "durationStatScale"),
                    ("Base Strength", "baseStrength"), ("Strength Per Mastow", "strengthStatScale"),
                    ("Chance To Apply", "chanceToApply"), ("Extra Chance To Reapply", "additionalChanceToReapply"),
                    ("Tick Interval", "tickInterval"),
                    ("Spread Enabled", "contactSpread.enabled"), ("Spread Chance", "contactSpread.chance"),
                    ("Spread Cooldown", "contactSpread.cooldown"), ("Spread Max Generations", "contactSpread.maxGenerations"),
                    ("Spread Duration Falloff", "contactSpread.durationFalloff"), ("Spread Strength Falloff", "contactSpread.strengthFalloff"),
                    ("Stacking Enabled", "stacking.enabled"), ("Max Stacks", "stacking.maxStacks"),
                    ("Refresh Duration On Stack", "stacking.refreshDurationOnStack"), ("Burst Damage Per Stack", "stacking.burstDamagePerStack"),
                    ("Max Slow", "maxSlow"), ("Chain Radius", "chainRadius"), ("Max Chain Depth", "maxChainDepth")),
                new SettingsTable<CorruptionSettings>("Corruption", () => Load<CorruptionSettings>(CorruptionSettingsPath),
                    "minGrace", "maxGrace", "startingGrace", "damagePerGracePoint", "minDamageMultiplier",
                    "corruptedChancePerNegativeGrace", "corruptedChancePerTaintLevel", "maxCorruptedChance",
                    "corruptedDamageMultiplier", "corruptedHealthBonus",
                    "corruptedDamagePerCorruption", "maxCorruptionFromDamage",
                    "gracePickupBaseChance", "gracePickupChancePerAffinity", "gracePickupMaxChance",
                    "corruptorSpawnTimeout", "corruptorPhaseDuration", "corruptorFastKillTime", "corruptorBaseCorruption",
                    "corruptorSecondsPerExtraCorruption", "corruptorMaxCorruption", "corruptorEscapeCorruption"),
                new SettingsTable<PlayerStatSettings>("PlayerStats", () => Load<PlayerStatSettings>(PlayerStatSettingsPath),
                    ("Base Move Speed", "baseMoveSpeed"), ("Move Speed Per Point", "moveSpeedPerPoint"),
                    ("Invulnerability After Hit", "invulnerabilityAfterHit"),
                    ("Regen Base Interval", "regenBaseInterval"), ("Regen Interval Divisor", "regenIntervalDivisor"),
                    ("Lifesteal Base Chance", "lifestealBaseChance"), ("Lifesteal Chance Per Point", "lifestealChancePerPoint"),
                    ("Lifesteal Heal Per Point", "lifestealHealPerPoint"), ("Lifesteal Min Heal", "lifestealMinHeal"),
                    ("Lifesteal Max Heal", "lifestealMaxHeal"),
                    ("Dash Cooldown Scale Per Point", "dashCooldownScalePerPoint"),
                    ("Shop Discount Soft Cap", "shopDiscountSoftCap"), ("Shop Discount Hard Cap", "shopDiscountHardCap"),
                    ("Pickup Base Range", "pickupBaseRange"), ("Pickup Range Per Point", "pickupRangePerPoint"),
                    ("Level-up Tier 1 Weight", "upgradeTierBaseChances.Array.data[0]"),
                    ("Level-up Tier 2 Weight", "upgradeTierBaseChances.Array.data[1]"),
                    ("Level-up Tier 3 Weight", "upgradeTierBaseChances.Array.data[2]"),
                    ("Level-up Tier 4 Weight", "upgradeTierBaseChances.Array.data[3]"),
                    ("Retribution Damage Tier 1 Cap", "retribution.damageTier1Cap"),
                    ("Retribution Damage Tier 2 Cap", "retribution.damageTier2Cap"),
                    ("Retribution Damage Hard Cap", "retribution.damageHardCap"),
                    ("Retribution Damage Tier 1 Rate", "retribution.damageTier1Rate"),
                    ("Retribution Damage Tier 2 Rate", "retribution.damageTier2Rate"),
                    ("Retribution Damage Tier 3 Rate", "retribution.damageTier3Rate"),
                    ("Retribution Melee Damage Scaling", "retribution.meleeDamageScaling"),
                    ("Retribution Grace Tier 1 Cap", "retribution.graceTier1Cap"),
                    ("Retribution Grace Hard Cap", "retribution.graceHardCap"),
                    ("Retribution Grace Tier 1 Rate", "retribution.graceTier1Rate"),
                    ("Retribution Grace Tier 2 Rate", "retribution.graceTier2Rate"),
                    ("Retribution Grace Gained", "retribution.graceGeneratedAmount"),
                    ("Retribution Grace Cooldown", "retribution.graceGainCooldown")),
                new SettingsTable<PierceSettings>("Pierce", () => Load<PierceSettings>(PierceSettingsPath),
                    "extraPiercePerPoint", "splitChancePerPoint", "splitMaxChance", "splitDamageMultiplier", "splitMaxTravelDistance"),
                // Retired stats (Cleave) stay in the database, because stats are stored by index, but not in the sheet
                new KeyedListTable<StatDatabase>("Stats", "Stat", () => Load<StatDatabase>(StatDatabasePath), "stats", "statType",
                    entry => !((StatData)entry).Retired,
                    ("Initial Value", "initialValue"), ("Allow Level Upgrades", "allowLevelUpgrades"),
                    ("Tier 1", "tierValues.Array.data[0]"), ("Tier 2", "tierValues.Array.data[1]"),
                    ("Tier 3", "tierValues.Array.data[2]"), ("Tier 4", "tierValues.Array.data[3]"),
                    ("Category", "category"), ("Affected By Corruption", "affectedByCorruption"),
                    ("Corruption Reduce Factor", "corruptionReduceFactor")),
                new AssetTable<CharacterClassData>("Classes", "Class", () => LoadAll<CharacterClassData>(ClassesFolder), c => c.name,
                    "dashCooldown", "dashSpeed", "dashDuration", "dashMaxCharges"),

                // Items (step 5): what each item is. Structure (class kit tier, starting-only) stays in Unity
                new AssetTable<Effect>("Items", "Item", () => LoadAll<Effect>(EffectsFolder), e => e.name,
                    "description", "quality", "buyPrice", "sellPrice", "icon", "unlockedByDefault"),
                new ChildListTable<Effect>("ItemStats", "Item", () => LoadAll<Effect>(EffectsFolder), e => e.name, "effectStatModifiers", true,
                    ("Stat", "stat"), ("Value", "value"), ("Type", "type")).AllowingEmpty(),
                new ChildListTable<Effect>("ItemStatBonuses", "Item", () => LoadAll<Effect>(EffectsFolder), e => e.name, "statDependencies", true,
                    ("Source Stat", "sourceStat"), ("Per Points", "perPoints"), ("Target Stat", "targetStat"), ("Amount", "amount")).AllowingEmpty(),
                new ChildListTable<Effect>("ItemBehaviors", "Item", () => LoadAll<Effect>(EffectsFolder), e => e.name, "behaviors", true,
                    ("Behavior", "")).AllowingEmpty(),

                // Item drops (step 4): where and when each lootable item drops, and the odds per source
                new KeyedListTable<ItemDropTable>("ItemDrops", "Item", () => Load<ItemDropTable>(ItemDropTable.ResourcePath), "entries", "item", null,
                        ("Enemy Pickup", "enemyPickup"), ("Shop", "shop"), ("Treasure", "treasure"), ("Event", "events"),
                        ("Min Levels Beaten", "minLevelsBeaten"), ("Weight", "weight"))
                    .WithInfoColumns(
                        ("Quality", e => ((ItemDropEntry)e).item != null ? ((ItemDropEntry)e).item.Quality.ToString() : string.Empty),
                        ("Buy Price", e => ((ItemDropEntry)e).item != null ? ((ItemDropEntry)e).item.BuyPrice.ToString() : string.Empty),
                        ("Unlocked", e => ((ItemDropEntry)e).item != null && ((ItemDropEntry)e).item.UnlockedByDefault ? "TRUE" : "FALSE")),
                new KeyedListTable<LootSettings>("LootSources", "Source", () => Load<LootSettings>(LootSettings.ResourcePath), "sources", "source", null,
                    ("Common", "commonWeight"), ("Uncommon", "uncommonWeight"), ("Rare", "rareWeight"), ("Heroic", "heroicWeight"),
                    ("Epic", "epicWeight"), ("Legendary", "legendaryWeight"), ("Luck Scale", "luckScalePerStep")),
                new SettingsTable<LootSettings>("Loot", () => Load<LootSettings>(LootSettings.ResourcePath),
                    "shopItemCount", "shopItemsPerLuck", "shopMinItemCount", "shopMaxItemCount",
                    "enemyDropChance", "enemyItemShare", "treasureFallbackMinCoins", "treasureFallbackMaxCoins"),
            });
            return tables;
        }

        /// <summary>
        /// Plans an import of every table from <paramref name="tables"/> (by table name). A missing table is an error;
        /// nothing is applied until the caller calls <see cref="ImportPlan.Apply"/> on an error-free plan.
        /// </summary>
        public static ImportPlan Plan(IReadOnlyDictionary<string, BalanceTable> tables, IBalanceStore store)
        {
            var plan = new ImportPlan(store);
            CellCodec.ClearReferenceCache();
            foreach (var definition in All())
            {
                if (tables.TryGetValue(definition.Name, out var table))
                {
                    definition.Plan(table, plan);
                }
                else
                {
                    plan.Error(definition.Name, "the table is missing");
                }
            }
            return plan;
        }

        /// <summary>One table per enemy behavior type, with its numbers, toggles and enums (found from the assets).</summary>
        private static IEnumerable<BalanceTableDefinition> BehaviorTables()
        {
            foreach (var group in LoadAll<AIBehavior>(BehaviorsFolder).GroupBy(b => b.GetType()).OrderBy(g => g.Key.Name, StringComparer.Ordinal))
            {
                var fields = DesignFields(group.Key);
                if (fields.Count == 0) continue;
                var type = group.Key;
                yield return new AssetTable<AIBehavior>("EnemyBehaviors_" + ShortName(type.Name), "Behavior",
                    () => LoadAll<AIBehavior>(BehaviorsFolder).Where(b => b.GetType() == type), b => b.name, fields.ToArray());
            }
        }

        private static string ShortName(string typeName)
        {
            string name = typeName.Replace("AIBehavior", string.Empty).Replace("Behavior", string.Empty);
            return name.StartsWith("AI") ? name.Substring(2) : name;
        }

        /// <summary>
        /// The serialized numbers, toggles, enums and vector components a type shows in the Inspector, in Inspector
        /// order (base class first), without references or text. With <paramref name="nested"/>, a field holding a
        /// [Serializable] struct or class (an item behavior's damage profile) adds its own fields as "field.subField".
        /// </summary>
        public static List<string> DesignFields(Type type, bool nested = false)
        {
            var chain = new List<Type>();
            for (var t = type; t != null && t != typeof(ScriptableObject) && t != typeof(Object); t = t.BaseType) chain.Insert(0, t);

            var fields = new List<string>();
            foreach (var t in chain)
            {
                foreach (var field in t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    if (!IsShownInInspector(field) || BehaviorFieldsSkipped.Contains(field.Name)) continue;
                    var fieldType = field.FieldType;
                    if (fieldType == typeof(int) || fieldType == typeof(float) || fieldType == typeof(bool) || fieldType.IsEnum)
                    {
                        fields.Add(field.Name);
                    }
                    else if (fieldType == typeof(Vector2))
                    {
                        fields.Add(field.Name + ".x");
                        fields.Add(field.Name + ".y");
                    }
                    else if (nested && IsNestedRecord(fieldType))
                    {
                        fields.AddRange(DesignFields(fieldType).Select(sub => field.Name + "." + sub));
                    }
                }
            }
            return fields;
        }

        private static bool IsNestedRecord(Type type) =>
            type.GetCustomAttribute<SerializableAttribute>() != null && !typeof(Object).IsAssignableFrom(type)
            && !type.IsPrimitive && !type.IsEnum && type != typeof(string) && !typeof(System.Collections.IEnumerable).IsAssignableFrom(type);

        private static bool IsShownInInspector(FieldInfo field) =>
            !field.IsInitOnly && !field.IsNotSerialized
            && (field.IsPublic || field.GetCustomAttribute<SerializeField>() != null)
            && field.GetCustomAttribute<HideInInspector>() == null;
    }

    /// <summary>
    /// The waves of every level (wave database), numbered 1..n in their order. A row with the next number adds a wave:
    /// a new WaveData next to the level's others, appended to its list. Waves can't be removed here.
    /// </summary>
    public class WaveTable : BalanceTableDefinition
    {
        public static readonly string[] Fields =
        {
            "waveDuration", "longCooldown", "amountOfEnemiesWithLongCooldown", "regularCooldown",
            "expectedEnemyCountOverride", "corruptedChanceMultiplier", "spawnCorruptor", "overrideCorruptor", "corruptorOverride",
        };

        private readonly Func<IEnumerable<WaveDatabase>> databases;

        public WaveTable(Func<IEnumerable<WaveDatabase>> databases) => this.databases = databases;

        public override string Name => "Waves";
        public override IReadOnlyList<string> Columns => new[] { "Level", "Wave" }.Concat(Fields.Select(Header)).ToList();

        public override IReadOnlyList<string> ColumnNotes =>
            new[] { BalanceDocs.ColumnNote(Name, "Level", null), BalanceDocs.ColumnNote(Name, "Wave", null) }
                .Concat(FieldNotes(typeof(WaveData), Fields)).ToList();

        public static string WaveKey(string level, int wave) => $"{level} wave {wave}";

        public override BalanceTable Export()
        {
            var table = new BalanceTable(Name, Columns);
            foreach (var database in databases().OrderBy(d => d.name, StringComparer.Ordinal))
            {
                for (int i = 0; i < database.waves.Count; i++)
                {
                    if (database.waves[i] == null) continue;
                    var wave = database.waves[i];
                    table.AddRow(new[] { database.name, (i + 1).ToString() }.Concat(Fields.Select(f => FormatField(wave, f))).ToArray());
                }
            }
            return table;
        }

        public override void Plan(BalanceTable table, ImportPlan plan)
        {
            if (!CheckColumns(table, plan)) return;
            var byName = databases().ToDictionary(d => d.name, StringComparer.OrdinalIgnoreCase);
            var rowsByLevel = new Dictionary<string, Dictionary<int, int>>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < table.Rows.Count; i++)
            {
                var row = table.Rows[i];
                int line = BalanceTable.LineOf(i);
                string level = row[0].Trim();
                if (!byName.ContainsKey(level))
                {
                    plan.Error(Name, line, $"no level \"{level}\" (a wave database under Resources/{BalanceTables.WavesFolder})");
                    continue;
                }
                if (!int.TryParse(row[1].Trim(), out int wave) || wave < 1)
                {
                    plan.Error(Name, line, $"Wave must be a number from 1, not \"{row[1]}\"");
                    continue;
                }
                if (!rowsByLevel.TryGetValue(level, out var waves)) rowsByLevel[level] = waves = new Dictionary<int, int>();
                if (!waves.TryAdd(wave, i)) plan.Error(Name, line, $"{level} has two rows for wave {wave}");
            }

            foreach (var (level, database) in byName)
            {
                var waves = rowsByLevel.TryGetValue(level, out var w) ? w : new Dictionary<int, int>();
                int count = database.waves.Count;
                int rows = waves.Count;
                if (rows < count)
                {
                    plan.Error(Name, $"{level} has {count} waves but {rows} rows (waves can't be removed here)");
                    continue;
                }
                if (Enumerable.Range(1, rows).Any(n => !waves.ContainsKey(n)))
                {
                    plan.Error(Name, $"{level}'s waves must be numbered 1 to {rows} without gaps");
                    continue;
                }

                for (int n = 1; n <= rows; n++)
                {
                    var cells = table.Rows[waves[n]];
                    int line = BalanceTable.LineOf(waves[n]);
                    string label = WaveKey(level, n);
                    if (n <= count)
                    {
                        var wave = database.waves[n - 1];
                        if (wave == null)
                        {
                            plan.Error(Name, line, $"{label} is an empty slot in {level}'s list");
                            continue;
                        }
                        for (int f = 0; f < Fields.Length; f++) PlanCell(plan, line, label, wave, wave, Fields[f], cells[f + 2]);
                    }
                    else
                    {
                        PlanNewWave(plan, database, n, label, line, cells);
                    }
                }
            }
        }

        private void PlanNewWave(ImportPlan plan, WaveDatabase database, int number, string label, int line, string[] cells)
        {
            WaveData created = null;
            Func<Object> target = () => created;
            plan.Do($"{label}: new wave", () =>
            {
                created = plan.Store.CreateAsset<WaveData>(database, $"Wave{number}");
                plan.Store.BeforeChange(database);
                database.waves.Add(created);
                plan.Store.AfterChange(database);
            });
            plan.AddPendingAsset(label, target);

            // Cells are checked against a fresh wave, and every one is set (defaults included)
            var scratch = ScriptableObject.CreateInstance<WaveData>();
            try
            {
                for (int f = 0; f < Fields.Length; f++)
                {
                    FieldAccess.TryDescribe(typeof(WaveData), Fields[f], out var type, out var field);
                    if (!CellCodec.TryParse(cells[f + 2], type, field, out object value, out string error))
                    {
                        plan.Error(Name, line, $"{label}, {Header(Fields[f])}: {error}");
                        continue;
                    }
                    plan.SetField($"{label}.{Header(Fields[f])}: {cells[f + 2].Trim()}", target, Fields[f], value);
                }
            }
            finally
            {
                Object.DestroyImmediate(scratch);
            }
        }
    }

    /// <summary>
    /// The enemy pool of every wave: one row per enemy type with its spawn weight. Rows can be added or removed; a
    /// wave needs at least one. Planned after <see cref="WaveTable"/>, so a wave it adds can get its enemies.
    /// </summary>
    public class WaveEnemyTable : BalanceTableDefinition
    {
        private readonly Func<IEnumerable<WaveDatabase>> databases;
        private readonly ChildListTable<WaveData> pool;

        public WaveEnemyTable(Func<IEnumerable<WaveDatabase>> databases)
        {
            this.databases = databases;
            pool = new ChildListTable<WaveData>(Name, "Wave", Enumerable.Empty<WaveData>, w => w.name, "enemyPool", true,
                ("Enemy", "type"), ("Weight", "weight"));
        }

        public override string Name => "WaveEnemies";
        public override IReadOnlyList<string> Columns => new[] { "Level", "Wave", "Enemy", "Weight" };

        public override BalanceTable Export()
        {
            var table = new BalanceTable(Name, Columns);
            foreach (var database in databases().OrderBy(d => d.name, StringComparer.Ordinal))
            {
                for (int i = 0; i < database.waves.Count; i++)
                {
                    if (database.waves[i] == null) continue;
                    foreach (var entry in database.waves[i].enemyPool ?? new List<WaveEnemyPoolEntry>())
                    {
                        table.AddRow(database.name, (i + 1).ToString(), entry.type.ToString(), entry.weight.ToString());
                    }
                }
            }
            return table;
        }

        public override void Plan(BalanceTable table, ImportPlan plan)
        {
            if (!CheckColumns(table, plan)) return;

            var waves = new Dictionary<string, WaveData>(StringComparer.OrdinalIgnoreCase);
            foreach (var database in databases())
            {
                for (int i = 0; i < database.waves.Count; i++)
                {
                    if (database.waves[i] != null) waves[WaveTable.WaveKey(database.name, i + 1)] = database.waves[i];
                }
            }

            var rowsByWave = new Dictionary<string, List<(int line, string[] cells)>>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < table.Rows.Count; i++)
            {
                var row = table.Rows[i];
                string key = int.TryParse(row[1].Trim(), out int number) ? WaveTable.WaveKey(row[0].Trim(), number) : null;
                if (key == null || (!waves.ContainsKey(key) && !plan.TryGetPendingAsset(key, out _)))
                {
                    plan.Error(Name, BalanceTable.LineOf(i), $"no wave {row[1].Trim()} in level \"{row[0].Trim()}\" (add it in Waves first)");
                    continue;
                }
                if (!rowsByWave.TryGetValue(key, out var rows)) rowsByWave[key] = rows = new List<(int, string[])>();
                // The pool table's rows are key + item cells
                rows.Add((BalanceTable.LineOf(i), new[] { key, row[2], row[3] }));
            }

            foreach (var (key, wave) in waves)
            {
                var rows = rowsByWave.TryGetValue(key, out var r) ? r : new List<(int, string[])>();
                pool.PlanList(plan, key, wave, rows, () => wave);
            }

            foreach (string key in plan.PendingAssetKeys.Where(k => !rowsByWave.ContainsKey(k)))
            {
                plan.Error(Name, $"{key} is new but has no enemies (add its rows here)");
            }

            foreach (var (key, rows) in rowsByWave.Where(kv => !waves.ContainsKey(kv.Key)))
            {
                plan.TryGetPendingAsset(key, out var resolve);
                var scratch = ScriptableObject.CreateInstance<WaveData>();
                try
                {
                    pool.PlanList(plan, key, scratch, rows, resolve);
                }
                finally
                {
                    Object.DestroyImmediate(scratch);
                }
            }
        }
    }
}
