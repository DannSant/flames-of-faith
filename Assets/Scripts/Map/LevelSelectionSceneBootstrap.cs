using Game.Scene;
using UnityEngine;


namespace Game.Map
{
    public class LevelSelectionSceneBootstrap : MonoBehaviour
    {
        private void Start()
        {
            // Resolve levelData for the current layer
            var layers = LevelSelectionController.Instance.BuildLevelDataOfCurrentLayer();

            // Get the current layer
            int currentLayer = LevelSelectionController.Instance.GetCurrentLayer();

            /*foreach (var layer in layers) { 
                Debug.Log(layer.ToString());
            }*/

            // Render map
            MapRenderer.Instance.RenderMap(layers, currentLayer);
        }
    }

}