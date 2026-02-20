using UnityEngine;

namespace Game.Common
{
    /**
     * Disables a component or executes logic to disable a component when the player enters on the map scene. This is useful for components that should not be active when the player is on the map, such as player controls.
     * */
    public interface IMapComponentDisabler
    {
        void DisableComponentsOnMap();
    }
}
