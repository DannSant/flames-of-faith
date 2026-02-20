using Game.Common;
using Game.Control;
using Game.Overworld;
using Game.Scene;
using Game.Waves;
using Unity.Cinemachine;
using UnityEngine;

namespace Game.Level
{
    public class CameraController : Singleton<CameraController>, ISceneCleanupHandler
    {
        private CinemachineCamera cinemachineCamera;

        private void OnEnable()
        {
            if (MainSceneController.Instance != null)
            {
                MainSceneController.Instance.OnGameplayInitialSetup += SetPlayerCameraFollow;
            }
        }

        private void OnDisable()
        {
            if (MainSceneController.Instance != null)
            {
                MainSceneController.Instance.OnGameplayInitialSetup -= SetPlayerCameraFollow;
            }
        }

        public void SetPlayerCameraFollow()
        {
            cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();
            if (PlayerManager.Instance.IsPlayerOnMap)
            {
                cinemachineCamera.Follow = FindFirstObjectByType<OverworldMapRenderer>().PlayerMarkerTransform;
            }
            else
            {                
                cinemachineCamera.Follow = PlayerManager.Instance.GetPlayerComponent<PlayerController>().transform;
            }
            
        }
        public void Cleanup()
        {
            if (MainSceneController.Instance != null)
            {
                MainSceneController.Instance.OnGameplayInitialSetup -= SetPlayerCameraFollow;
            }
            Destroy(gameObject);
        }
    }

}