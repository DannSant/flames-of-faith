using Game.Combat;
using Game.Combat.Projectiles;
using System;
using UnityEngine;

namespace Game.Effects{
   

    public abstract class Effect : ScriptableObject
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

        protected EffectStore ownerStore;

        //Properties
        public string EffectID => effectID;
        public Sprite EffectIcon => icon;
        public string EffectName => effectName;
        public float ScalingValue => scalingValue;
        public string Description => description;
        public int BuyPrice => buyPrice;
        public int SellPrice => sellPrice;

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