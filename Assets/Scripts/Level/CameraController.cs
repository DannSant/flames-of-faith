using Game.Common;
using Game.Control;
using Game.Scene;
using Unity.Cinemachine;
using UnityEngine;

namespace Game.Level
{
    public class CameraController : Singleton<CameraController>
    {
        private CinemachineCamera cinemachineCamera;

        private void OnEnable()
        {
            if (MainSceneController.Instance != null)
            {
                MainSceneController.Instance.OnGameplayResetRequested += SetPlayerCameraFollow;
            }
        }

        private void OnDisable()
        {
            if (MainSceneController.Instance != null)
            {
                MainSceneController.Instance.OnGameplayResetRequested -= SetPlayerCameraFollow;
            }
        }

        public void SetPlayerCameraFollow()
        {
            cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();
            cinemachineCamera.Follow = PlayerController.Instance.transform;
        }
    }

}