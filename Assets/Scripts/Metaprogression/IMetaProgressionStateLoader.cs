using UnityEngine;

namespace Game.Metaprogression
{
    public interface IMetaProgressionStateLoader
    {
        void Initialize();
        void LoadState(MetaState state);
        void SaveState();
        void ResetState();
    }
}