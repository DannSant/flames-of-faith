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
        [SerializeField] private Sprite icon;
        [SerializeField] public string effectName = "New Effect";
        [Tooltip("This value will be multiplied by the level of the item when calculating damage or other state")]
        [SerializeField] public float scalingValue = 1;
        [SerializeField] [TextArea] public string description;
        [SerializeField] private int buyPrice = 0;
        [SerializeField] private int sellPrice = 0;
        [Header("Stat Modifiers")]
        [SerializeField] private List<StatModifier> effectStatModifiers = new();
        [Header("Behaviors")]
        [SerializeField] private List<EffectBehavior> behaviors = new();
        [SerializeField] private bool unlockedByDefault = false;

        protected EffectStore ownerStore;

        //Properties
        public string EffectID { get => effectID; set => effectID = value; }
        public Sprite EffectIcon { get => icon; set => icon = value; }
        public string EffectName { get => effectName; set => effectName = value; }
        public float ScalingValue { get => scalingValue; set => scalingValue = value; }
        public string Description { get => description; set => description = value; }
        public int BuyPrice { get => buyPrice; set => buyPrice = value; }
        public int SellPrice { get => sellPrice; set => sellPrice = value; }
        public List<StatModifier> StatModifiers { get => effectStatModifiers; set => effectStatModifiers = value; }
        public List<EffectBehavior> Behaviors { get => behaviors; set => behaviors = value; }
        public bool UnlockedByDefault { get => unlockedByDefault; set => unlockedByDefault = value; }

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

        public void InitializeFromData(EffectRow row)
        {          

            EffectID = row.effectID;
            EffectName = row.name;
            Description = row.description;
            BuyPrice = row.priceBuy;
            SellPrice = row.priceSell;
            UnlockedByDefault = row.unlockedByDefault == 1 ? true : false;

            // Load icon
            if (!string.IsNullOrEmpty(row.iconKey))
                EffectIcon = Resources.Load<Sprite>($"Icons/{row.iconKey}");

            // Deserialize stat modifiers           
            StatModifiers = Newtonsoft.Json.JsonConvert.DeserializeObject<List<StatModifier>>(row.statModifiersJson);

            // Deserialize behaviors
            var behaviorIDs = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(row.behaviorsJson);

            Behaviors = behaviorIDs
                .Select(id => Resources.Load<EffectBehavior>($"Effects/EffectBehaviors/{id}"))
                .Where(b => b != null)
                .ToList();            
        }

        public override string ToString()
        {
            return $"EffectID: {effectID}, " +
                   $"EffectName: {effectName}, " +
                   $"ScalingValue: {scalingValue}, " +
                   $"Description: {description}, " +
                   $"OwnerStore: {ownerStore}, " +
                   $"EffectIcon: {(icon != null ? icon.name : "None")}";
        }




    }

}