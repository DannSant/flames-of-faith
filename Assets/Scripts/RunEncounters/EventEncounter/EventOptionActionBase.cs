using Game.Progression;
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

        public abstract string Apply(EventContext context);
    }

}