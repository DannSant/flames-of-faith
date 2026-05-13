using UnityEngine;

namespace Game.Boss
{
    public abstract class BossAbilityCondition : ScriptableObject
    {
        public abstract bool Evaluate(BossAbilityContext context);
    }
}
