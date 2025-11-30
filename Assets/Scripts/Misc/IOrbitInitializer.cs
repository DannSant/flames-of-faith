using UnityEngine;

namespace Game.Misc
{
    public interface IOrbitInitializer 
    {
        void InitializeOrbit(Transform target, float initialAngle);
    }
}
