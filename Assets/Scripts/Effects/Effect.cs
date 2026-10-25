using Game.Combat;
using Game.Combat.Projectiles;
using Game.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

namespace Game.Effects{
   

    public class Effect : ScriptableObject
    {
        [Header("Effect data")]
        [SerializeField] private string effectID;
        [Tooltip("The item's icon (a sprite in Resources/Icons, by name).")]
        [SerializeField] private Sprite icon;
        [SerializeField] public string effectName = "New Effect";
        [HideInInspector]
        [System.Obsolete("Retired Oct 2026: folded into the item behaviors' damage profiles (Stat Scale). Not read; kept for the retired DB loader.")]
        [SerializeField] public float scalingValue = 1;
        [Tooltip("The text shown in the item's tooltip. _melee_damage_ and similar placeholders become stat names. Keep numbers out: tier lines and stat lines are added from the data.")]
        [SerializeField] [TextArea] public string description;
        [Tooltip("What the shop charges, before the Shop Item Discount stat.")]
        [Min(0)]
        [SerializeField] private int buyPrice = 0;
        [Tooltip("Coins the player gets for selling it from the item bag.")]
        [Min(0)]
        [SerializeField] private int sellPrice = 0;
        [Header("Stat Modifiers")]
        [SerializeField] private List<StatModifier> effectStatModifiers = new();
        [Header("Stat Bonuses")]
        [Tooltip("Stats that grow with another stat (e.g. every Retribution point: +1 Armor). Starting items only; items with rules never appear in shops or rewards.")]
        [SerializeField] private List<StatDependencyRule> statDependencies = new();
        [Header("Behaviors")]
        [SerializeField] private List<EffectBehavior> behaviors = new();
        [Tooltip("Available from the start. A locked item never drops until a meta-unlock unlocks it.")]
        [SerializeField] private bool unlockedByDefault = false;
        [Tooltip("The item's quality: its tooltip color and the drop odds it uses (LootSources).")]
        [SerializeField] private EffectQuality quality = EffectQuality.Common;
        [Header("Class Kit")]
        [Tooltip("Upgrade tier of a class kit item (0 = base). Selects the row of its ability behaviors' tier tables.")]
        [SerializeField] private int tier = 0;
        [Tooltip("Class kit item: never sold, dropped or rewarded.")]
        [SerializeField] private bool startingOnly = false;

        protected EffectStore ownerStore;

        //Properties
        public string EffectID { get => effectID; set => effectID = value; }
        public Sprite EffectIcon { get => icon; set => icon = value; }
        public string EffectName { get => effectName; set => effectName = value; }
        [System.Obsolete("Retired Oct 2026: folded into the item behaviors' damage profiles (Stat Scale). Not read; kept for the retired DB loader.")]
        public float ScalingValue { get => scalingValue; set => scalingValue = value; }
        public string Description { get => description; set => description = value; }
        public int BuyPrice { get => buyPrice; set => buyPrice = value; }
        public int SellPrice { get => sellPrice; set => sellPrice = value; }
        public List<StatModifier> StatModifiers { get => effectStatModifiers; set => effectStatModifiers = value; }
        public List<EffectBehavior> Behaviors { get => behaviors; set => behaviors = value; }
        public List<StatDependencyRule> StatDependencies { get => statDependencies; set => statDependencies = value ?? new List<StatDependencyRule>(); }
        /// <summary>Items with stat dependency rules are starting items only: never sold or rewarded.</summary>
        public bool HasStatDependencies => statDependencies != null && statDependencies.Count > 0;
        public bool UnlockedByDefault { get => unlockedByDefault; set => unlockedByDefault = value; }
        public EffectQuality Quality { get => quality; set => quality = value; }
        public int Tier { get => tier; set => tier = Mathf.Max(0, value); }
        public bool StartingOnly { get => startingOnly; set => startingOnly = value; }
        /// <summary>Starting items only: never sold, dropped or rewarded. Items with stat bonus rules count too.</summary>
        public bool IsStartingOnly => startingOnly || HasStatDependencies;

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Only assign a new ID if it's empty
            if (string.IsNullOrEmpty(effectID))
            {
                effectID = Guid.NewGuid().ToString();
                UnityEditor.EditorUtility.SetDirty(this); // Mark asset as modified so Unity saves it
            }
        }
#endif

        /// <summary>
        /// Apply this effect to the target GameObject.
        /// This is called when the effect is first added (e.g., item pickup).
        /// </summary>
        /// <param name="target">The GameObject this effect should affect (typically the player).</param>
        public virtual void Apply(GameObject config, EffectStore effectStore)
        {
            ownerStore = effectStore;
        }

        /// <summary>
        /// Cleanup or unsubscribe if necessary when the effect is removed or the game ends.
        /// </summary>
        public virtual void Cleanup() { }

        /// <summary>
        /// Optional tick behavior for update-based effects.
        /// Called from EffectStore.Update() if needed.
        /// </summary>
        /// <param name="target">The GameObject this effect is affecting.</param>
        public virtual void Tick(GameObject target) { }

        /// <summary>
        /// Re-Apply this effect to the target GameObject.
        /// This is called when the effect is added again, this method is optional and should only be called if additional action is needed
        /// when we re add an effect. If not, the effect store will stack up only the count of the effect.
        /// </summary>
        /// <param name="target">The GameObject this effect should affect (typically the player).</param>
        public virtual void UpdateEffect(GameObject config) { }

        [System.Obsolete("Retired Oct 2026 (data-driven step 5): items are edited through the balance sheet tables (Items, ItemStats, ItemStatBonuses, ItemBehaviors) and Tools/Flames of Faith/Items/New Item. Kept until a play-test round confirms the sheet path, then deleted with the SQLite library.")]
        public void InitializeFromData(EffectRow row)
        {          

            EffectID = row.effectID;
            EffectName = row.name;
            Description = row.description;
            BuyPrice = row.priceBuy;
            SellPrice = row.priceSell;
            // A row that predates this column reads back 0, and 0 would zero out the damage share
            // of everything this effect spawns - so treat it as "not authored" and keep the full share.
            ScalingValue = row.scalingValue > 0f ? row.scalingValue : 1f;
            UnlockedByDefault = row.unlockedByDefault == 1 ? true : false;
            Quality = (EffectQuality)row.quality;
            // null means the row predates these columns: a base-tier, regular item.
            Tier = row.tier ?? 0;
            StartingOnly = row.startingOnly.HasValue && row.startingOnly.Value == 1;

            // Load icon
            if (!string.IsNullOrEmpty(row.iconKey))
                EffectIcon = Resources.Load<Sprite>($"Icons/{row.iconKey}");

            // Deserialize stat modifiers           
            StatModifiers = Newtonsoft.Json.JsonConvert.DeserializeObject<List<StatModifier>>(row.statModifiersJson);

            // Null/empty: the row has no rules (or predates the column).
            StatDependencies = string.IsNullOrEmpty(row.statDependenciesJson)
                ? new List<StatDependencyRule>()
                : Newtonsoft.Json.JsonConvert.DeserializeObject<List<StatDependencyRule>>(row.statDependenciesJson);

            // Deserialize behaviors
            var behaviorIDs = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(row.behaviorsJson);

            Behaviors = behaviorIDs
                .Select(id => Resources.Load<EffectBehavior>($"Effects/EffectBehaviors/{id}"))
                .Where(b => b != null)
                .ToList();            
        }

        /// <summary>
        /// Description text with damage-type meta tags colorized, plus an auto-generated
        /// line per stat modifier (e.g. "+2 Melee Damage") appended after a line break.
        /// The tooltip shows the two parts in separate columns: GetDescriptionText and GetStatLines.
        /// </summary>
        public string GetFormattedDescription()
        {
            string statLines = GetStatLines();
            string text = GetDescriptionText();
            return string.IsNullOrEmpty(statLines) ? text : text + "\n" + statLines;
        }

        /// <summary>The description and the ability lines for this item's tier, without the stat lines.</summary>
        public string GetDescriptionText()
        {
            string result = DamageTypeColorHelper.ColorizeMetaTags(description);

            // Abilities describe the tier this item is (e.g. the Fireball's explosion from +1)
            if (behaviors != null)
            {
                var abilityLines = behaviors
                    .OfType<Game.Effects.Abilities.AbilityBehavior>()
                    .Select(a => a.DescribeTier(tier))
                    .Where(line => !string.IsNullOrEmpty(line))
                    .ToList();
                if (abilityLines.Count > 0)
                {
                    result += "\n" + DamageTypeColorHelper.ColorizeMetaTags(string.Join("\n", abilityLines));
                }
            }

            return result;
        }

        /// <summary>One line per stat modifier ("+2 Melee Damage"), then per stat bonus rule. Empty without any.</summary>
        public string GetStatLines()
        {
            var lines = new List<string>();
            if (effectStatModifiers != null)
            {
                lines.AddRange(effectStatModifiers.Select(BuildModifierLine));
            }
            if (HasStatDependencies)
            {
                lines.AddRange(statDependencies.Where(r => r != null).Select(BuildDependencyLine));
            }
            return string.Join("\n", lines);
        }

        // "+1 Armor per Retribution point" / "-1 Attack Speed per 5 Evasion"
        private static string BuildDependencyLine(StatDependencyRule rule)
        {
            string sign = rule.amount >= 0 ? "+" : "";
            string formattedValue = rule.amount % 1 == 0
                ? ((int)rule.amount).ToString()
                : rule.amount.ToString("0.##");

            string target = ColorizeStat(rule.targetStat);
            string source = ColorizeStat(rule.sourceStat);
            string per = rule.perPoints <= 1 ? $"per {source} point" : $"per {rule.perPoints} {source}";
            return $"{sign}{formattedValue} {target} {per}";
        }

        private static string ColorizeStat(Game.Progression.StatType stat)
        {
            string label = StatDisplayNameHelper.GetDisplayName(stat);
            if (DamageTypeColorHelper.TryGetWeaponClass(stat, out WeaponClass weaponClass))
            {
                label = $"<color=#{DamageTypeColorHelper.GetHex(weaponClass)}>{label}</color>";
            }
            return label;
        }

        private static string BuildModifierLine(StatModifier modifier)
        {
            string sign = modifier.value >= 0 ? "+" : "";
            string formattedValue = modifier.value % 1 == 0
                ? ((int)modifier.value).ToString()
                : modifier.value.ToString("0.##");
            string suffix = modifier.type == ModifierType.PercentAdd ? "%" : "";

            string label = StatDisplayNameHelper.GetDisplayName(modifier.stat);
            if (DamageTypeColorHelper.TryGetWeaponClass(modifier.stat, out WeaponClass weaponClass))
            {
                label = $"<color=#{DamageTypeColorHelper.GetHex(weaponClass)}>{label}</color>";
            }

            return $"{sign}{formattedValue}{suffix} {label}";
        }

        public override string ToString()
        {
            return $"EffectID: {effectID}, " +
                   $"EffectName: {effectName}, " +
                   $"Description: {description}, " +
                   $"OwnerStore: {ownerStore}, " +
                   $"EffectIcon: {(icon != null ? icon.name : "None")}";
        }




    }

}