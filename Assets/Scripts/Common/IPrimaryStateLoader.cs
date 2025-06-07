using UnityEngine;

namespace Game.Common
{

    public interface IPrimaryStateLoader
    {
        void LoadState();
        void SaveState();
        void ResetState();
    }

    public interface IDependentStateLoader
    {
        void LoadState();
        void SaveState();
        void ResetState();
    }

}