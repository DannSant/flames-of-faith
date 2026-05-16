using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Boss
{
    public enum AbilityPriority
    {
        Low,
        Normal,
        High,
        Critical
    }

    [System.Flags]
    public enum AbilityBlockFlags
    {
        None = 0,
        Movement = 1 << 0,
        Abilities = 1 << 1,
        Everything = Movement | Abilities
    }

    public abstract class BossAbilityBase : ScriptableObject
    {
        [Header("Ability data")]
        public string abilityName;
        public string animationName;
        public string initialAnimationName; // optional animation to play at the start of the ability (e.g. a cast animation before the main one)
        public string endAnimationName; // optional animation to play at the end of the ability (e.g. a recovery animation)
        public bool hasInitialAnimation => !string.IsNullOrEmpty(initialAnimationName);
        public bool hasEndAnimation => !string.IsNullOrEmpty(endAnimationName);        

        [Header("Conditions")]
        [SerializeField] private List<BossAbilityCondition> conditions;

        [Header("Timing")]
        public float cooldown = 5f;
        public float delayToStart = 0f; // delay before the ability starts executing (e.g. for windup animations)
        public float delayToEnd = 0f;

        [Header("Execution")]
        public AbilityPriority priority = AbilityPriority.Normal;
        public AbilityBlockFlags blocksWhileActive = AbilityBlockFlags.None;

        [Header("Metadata")]
        public List<string> abilityMetadata = new List<string>();

        public abstract IEnumerator Execute(BossController boss, BossAbilityRuntime bossAbilityRuntime, BossAbilityContext context);

        public List<BossAbilityCondition> Conditions => conditions;

    }

}