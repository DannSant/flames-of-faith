using UnityEngine;

namespace Game.Boss
{
    [CreateAssetMenu(menuName = "Boss/AbilityCondition/BossAbilityStateCondition")]
    public class BossAbilityStateCondition : BossAbilityCondition
    {
        [SerializeField]
        private BossAbilityBase targetAbility;

        [SerializeField]
        private bool shouldBeActive = true;

        public override bool Evaluate(BossAbilityContext context)
        {
            bool isActive =
                context.currentAbility == targetAbility;

            return shouldBeActive
                ? isActive
                : !isActive;
        }
    }

}