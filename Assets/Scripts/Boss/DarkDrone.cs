using UnityEngine;

namespace Game.Boss
{
    public class DarkDrone :  SummonedObject
    {
        [SerializeField] private bool shouldBeDestroyedOnBossDeathFlag = true;
        public override bool shouldBeDestroyedOnBossDeath()
        {
            return shouldBeDestroyedOnBossDeathFlag;
        }
    }

}