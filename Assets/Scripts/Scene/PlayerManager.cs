using Game.Common;
using Game.Control;
using Game.Misc;
using Game.Overworld;
using Game.Utils;
using Game.Waves;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using ManagedScene = UnityEngine.SceneManagement.Scene;

namespace Game.Scene
{
    public class PlayerManager : Singleton<PlayerManager>
    {
        protected override void Awake()
        {
            base.Awake();
        }
        [SerializeField] private List<GameObject> playerPrefabs;
        //[SerializeField] private Transform spawnPoint;

        private GameObject currentPlayer;

        public GameObject CurrentPlayer => currentPlayer;

        private bool isPlayerOnMap = false;
        public bool IsPlayerOnMap { get { return isPlayerOnMap; } set { isPlayerOnMap = value; } }

        public event Action OnPlayerDisabledOnMap;

        public void SpawnSelectedPlayer(int selectedIndex, bool isNewRun)
        {
            if (currentPlayer != null)
            {
                Destroy(currentPlayer);
            }

            GameObject prefab = playerPrefabs[selectedIndex];
            
            currentPlayer = Instantiate(prefab, transform.position, Quaternion.identity);

            /*if (isNewRun)
            {
                ResetAllPlayerComponentStates();
            }*/
            
        }       

        public void MovePlayerToScene(ManagedScene targetScene)
        {
            SceneManager.MoveGameObjectToScene(currentPlayer, targetScene);
            SceneManager.SetActiveScene(targetScene);
            var spawnPoint = FindAnyObjectByType<SpawnPoint>();           
            var spawnPosition = spawnPoint != null ? spawnPoint.transform.position : Vector3.zero;            
            currentPlayer.transform.position = spawnPosition;            
            var playerController = currentPlayer.GetComponent<PlayerController>();
            if(playerController != null)
            {
                playerController.DefaultPosition = spawnPosition;
            }
        }

        public void LateInitializePlayer()
        {
            foreach (var comp in currentPlayer.GetComponentsInChildren<IInitializeAfterStateReady>())
            {
                comp.InitializeAfterStateReady();
            }
        }

        public T GetPlayerComponent<T>() where T : MonoBehaviour
        {
            if (currentPlayer == null)
            {
                //Debug.LogError("Player not spawned yet.");
                return null;
            }
            return currentPlayer?.GetComponent<T>();
        }

        public T GetPlayerChildComponent<T>() where T : MonoBehaviour
        {
            if (currentPlayer == null)
            {
                //Debug.LogError("Player not spawned yet.");
                return null;
            }
            return currentPlayer?.GetComponentInChildren<T>();
        }

        public void ClearPlayer()
        {
            currentPlayer = null;
        }
        public void BindWaveEventsIfReady()
        {
            if (WaveSpawner.Instance != null)
            {             
                WaveSpawner.Instance.OnWaveGroupFinished += HandleWaveGroupFinished;
            }
        }

        // TODO: Unbind on scene unload
        public void UnbindWaveEventsIfReady()
        {
            if (WaveSpawner.Instance != null)
            {
                WaveSpawner.Instance.OnWaveGroupFinished -= HandleWaveGroupFinished;
            }
        }

        private void HandleWaveGroupFinished()
        {
            // Notify MapRunController about level cleared
            MapRunController.Instance.OnLevelCleared();

            SaveAllPlayerComponentStates();

            // Mark level as beaten to keep count of levels beaten in this run
            GameSession.Instance.MarkLevelBeaten(GameSession.Instance.currentLevel);            

            // Load Level Selector scene
            MainSceneController.Instance.LoadLevelSelectorScene(false);

           
        }

        public void DisableComponentsForMap()
        {
            foreach (var component in currentPlayer.GetComponentsInChildren<IMapComponentDisabler>())
            {
                component.DisableComponentsOnMap();
            }

            //Disable collider so we don't accidentally click on the player and prevent us to enter a level
            var colliders = currentPlayer.GetComponentsInChildren<Collider2D>();
            foreach (var collider in colliders)
            {
                collider.enabled = false;
            }

            //Call event for any additional logic that needs to happen when player is disabled on map
            OnPlayerDisabledOnMap?.Invoke();
        }


        public void SaveAllPlayerComponentStates()
        {
            foreach (var component in currentPlayer.GetComponentsInChildren<IDependentStateLoader>())
            {
                component.SaveState();
            }
            foreach (var component in currentPlayer.GetComponentsInChildren<IPrimaryStateLoader>())
            {
                component.SaveState();
            }
            
        }

        public void LoadAllPlayerComponentStates()
        {
            
            foreach (var component in currentPlayer.GetComponentsInChildren<IPrimaryStateLoader>())
            {
                component.LoadState();
            }

         
            foreach (var component in currentPlayer.GetComponentsInChildren<IDependentStateLoader>())
            {
                component.LoadState();
            }
        }

        public void ResetAllPlayerComponentStates()
        {
            foreach (var component in currentPlayer.GetComponentsInChildren<IPrimaryStateLoader>())
            {
                component.ResetState();
            }

            foreach (var component in currentPlayer.GetComponentsInChildren<IDependentStateLoader>())
            {
                component.ResetState();
            }
        }
    }
}
