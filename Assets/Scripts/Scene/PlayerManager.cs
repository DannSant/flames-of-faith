using Game.Common;
using Game.Utils;
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
        [SerializeField] private Transform spawnPoint;

        private GameObject currentPlayer;

        public GameObject CurrentPlayer => currentPlayer;

        public void SpawnSelectedPlayer(int selectedIndex)
        {
            if (currentPlayer != null)
            {
                Destroy(currentPlayer);
            }

            GameObject prefab = playerPrefabs[selectedIndex];
            Debug.Log($"Spawning player prefab: {prefab.name} at {spawnPoint.position}");
            currentPlayer = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
            
        }

        public void MovePlayerToScene(ManagedScene targetScene)
        {
            SceneManager.MoveGameObjectToScene(currentPlayer, targetScene);
        }

        public void LateInitializePlayer()
        {
            foreach (var comp in currentPlayer.GetComponentsInChildren<ILateInitializable>())
            {
                comp.LateInitialize();
            }
        }

        public T GetPlayerComponent<T>() where T : MonoBehaviour
        {
            if (currentPlayer == null)
            {
                Debug.LogError("Player not spawned yet.");
                return null;
            }
            return currentPlayer?.GetComponent<T>();
        }

        public void Clear()
        {
            currentPlayer = null;
        }
    }
}
