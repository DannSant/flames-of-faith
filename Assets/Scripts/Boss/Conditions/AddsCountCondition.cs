using UnityEngine;

namespace Game.Boss
{
    public enum ComparisonType
    {
        Equal,
        NotEqual,
        Greater,
        Less,
        GreaterOrEqual,
        LessOrEqual
    }
    [CreateAssetMenu(menuName = "Boss/AbilityCondition/AddsCountCondition")]
    public class AddsCountCondition : BossAbilityCondition
    {
        [SerializeField] private int count;
        [SerializeField] private ComparisonType comparisonType;
        public override bool Evaluate(BossAbilityContext context)
        {
            switch (comparisonType)
            {
                case ComparisonType.Equal:
                    return context.activeAddsCount == count;
                case ComparisonType.NotEqual:
                    return context.activeAddsCount != count;
                case ComparisonType.Greater:
                    return context.activeAddsCount > count;
                case ComparisonType.Less:
                    return context.activeAddsCount < count;
                case ComparisonType.GreaterOrEqual:
                    return context.activeAddsCount >= count;
                case ComparisonType.LessOrEqual:
                    return context.activeAddsCount <= count;
                default:
                    return false;
            }
        }
    }

}