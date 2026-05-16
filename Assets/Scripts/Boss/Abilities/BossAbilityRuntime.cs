using System.Collections;
using UnityEngine;

namespace Game.Boss
{
    public class BossAbilityRuntime
    {
        private BossAbilityBase ability;
        private float lastUseTime = -999f;
        public bool IsRunning { get; private set; }        

        public BossAbilityRuntime(BossAbilityBase ability)
        {
            this.ability = ability;
            // Start on cooldown
            lastUseTime = Time.time;
        }

        public bool CanExecute(BossController boss, BossAbilityContext context)
        {
            if (IsRunning) return false;

            if (Time.time < lastUseTime + ability.cooldown)
                return false;

            if (boss.HasBlockFlag(AbilityBlockFlags.Abilities))
                return false;
            
            foreach (var condition in ability.Conditions)
            {
                if (!condition.Evaluate(context))
                    return false;
            }

            return true;
        }

        public IEnumerator Execute(BossController boss, BossAbilityContext context, System.Action<AbilityBlockFlags, BossAbilityRuntime> onStart, System.Action<BossAbilityRuntime> onEnd)
        {
            IsRunning = true;
            lastUseTime = Time.time;
            

            onStart?.Invoke(ability.blocksWhileActive, this);

            yield return ability.Execute(boss,this,context);

            IsRunning = false;
            onEnd?.Invoke(this);
        }

        public BossAbilityBase GetBossAbility() { return ability; }
    }
}