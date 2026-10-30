using Game.Progression;
using Game.Utils;
using UnityEngine;

namespace Game.RunEncounters
{
    public abstract class EventOptionActionBase : ScriptableObject
    {
        [SerializeField] protected string resourceName = "";
        [SerializeField] protected float amount = 10f;

        [SerializeField] protected bool isPlainReward = true;
        [SerializeField] protected Vector2 amountRange = new Vector2(-10, 10);
        [SerializeField] protected float baseChance = 0f;
        [SerializeField] protected StatType biasStat = StatType.Luck;

        public string ResourceName => resourceName;
        public float Amount => amount;
        public bool IsPlainReward => isPlainReward;
        public Vector2 AmountRange => amountRange;
        public float BaseChance => baseChance;
        public StatType BiasStat => biasStat;

        /// <summary>The placeholder this action fills in a result text ({health}, {grace}...).</summary>
        public abstract EventResultKind Kind { get; }

        /// <summary>Applies the action and reports what it actually changed.</summary>
        public abstract EventActionResult Apply(EventContext context);

        /// <summary>The fixed amount, or a roll in the range that leans toward its high end with the bias stat.</summary>
        protected float RollAmount(EventContext context) => isPlainReward
            ? amount
            : StatBiasedRoll.Roll(amountRange, baseChance, context.playerProgression, biasStat);
    }

}
