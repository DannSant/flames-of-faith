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
            if (IsRunning) {
                CheckExecutionConditions("IsRunning", IsRunning, context);
                return false; 
            }

            if (Time.time < lastUseTime + ability.cooldown)
            {
                CheckExecutionConditions("Cooldown", Time.time < lastUseTime + ability.cooldown, context);
                return false; 
            }

            if (boss.HasBlockFlag(AbilityBlockFlags.Abilities))
            {
                CheckExecutionConditions("BlockFlag", boss.HasBlockFlag(AbilityBlockFlags.Abilities), context);
                return false;
            }
            
            foreach (var condition in ability.Conditions)
            {
                if (!condition.Evaluate(context))
                {
                    CheckExecutionConditions("Condition: " + condition.name, !condition.Evaluate(context), context);
                    return false;
                }
            }

            return true;
        }

        private void CheckExecutionConditions(string conditionLabel, bool conditionResult, BossAbilityContext context)
        {
            if (!context.isPhaseTwo)
            {
                return;
            }

            //Debug.Log($"[BossAbilityRuntime] Phase 2 - Condition: ability: {ability.abilityName} label: '{conditionLabel}' result: {conditionResult}");

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

        public void ResetRuntimeState()
        {
            IsRunning = false;
            // Reset last use so ability is available immediately after reset
            lastUseTime = Time.time - ability.cooldown;
        }
    }
}