using UnityEngine;

namespace Game.Boss
{
    public class BossAbilityContext
    {
        public Transform bossTransform;
        public Transform playerTransform;

        public MonoBehaviour coroutineRunner; // usually the BossController

        public float phaseTime;
        public int enrageLevel;

        // Optional helpers
        public System.Action<float> ModifyFlameProgress; // for altar interactions

        public int activeAddsCount;
        public BossAbilityBase currentAbility;
    }
}