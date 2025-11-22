using Game.Control;
using Game.Waves;
using System;
using UnityEngine;

namespace Game.Pickups
{
    public abstract class BasePickup : MonoBehaviour
    {
        public abstract void OnPickup(GameObject picker);

        private void Start()
        {
            var waveSpawner = WaveSpawner.Instance;
            if(waveSpawner != null)
            {
                waveSpawner.OnWaveComplete += DestroyPickupOnWaveCompleted;
            }
        }

        private void OnDisable()
        {
            var waveSpawner = WaveSpawner.Instance;
            if (waveSpawner != null)
            {
                waveSpawner.OnWaveComplete -= DestroyPickupOnWaveCompleted;
            }
        }

        private void DestroyPickupOnWaveCompleted()
        {
            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var playerController = other.GetComponent<PlayerController>();
            if(playerController == null) return;
            OnPickup(other.gameObject);
        }
    }
}