using UnityEngine;

namespace Game.Boss { 
    public abstract class SummonedObject : MonoBehaviour
    {
        public abstract bool shouldBeDestroyedOnBossDeath();
    }
}