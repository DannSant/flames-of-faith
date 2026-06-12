using Game.AI;
using Game.Combat;
using Game.Control;
using Game.Scene;
using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

namespace Game.Boss
{
    [System.Serializable]
    public class BossAltar :  AITarget
    {
        [Header("Settings")]
        [SerializeField] private float midThreshold = 50f;
        [SerializeField] private float maxThreshold = 99f;
        [Header("References")]
        [SerializeField] private Animator animator;

        [Header("Camera")]
        [SerializeField] private CinemachineCamera virtualCamera;
        [SerializeField] private float zoomedInSize = 3f;       
        [SerializeField] private float zoomOutDuration = 1.5f;

        private BossWaveHandler waveHandler;
        private float originalZoom;

        private void Awake()
        {
            originalZoom = virtualCamera.Lens.OrthographicSize;
        }

        private void Start()
        {
            waveHandler = FindAnyObjectByType<BossWaveHandler>();
            if (waveHandler == null)
            {
                Debug.LogError("BossWaveHandler not found!");
            }

            waveHandler.OnFlameProgressChanged += HandleFlameProgressChanged;

            StartCoroutine(StartingAnimation());
        }

        private void OnDisable()
        {
            if(waveHandler != null)
            {
                waveHandler.OnFlameProgressChanged -= HandleFlameProgressChanged;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            var altarAttacker = collision.GetComponent<AltarAttacker>();
            if (altarAttacker != null && waveHandler != null)
            {
                waveHandler.ReduceFlameProgress(altarAttacker.DamageAmount);
            }

            var enemyHealth = collision.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(new DamageRequest(9999f, WeaponClass.Melee, false));
            }
        }

        private void HandleFlameProgressChanged(float current, float max)
        {
            if(current <= 0f)
            {
                //if progress is 0 or less, fight has not started yet, so we don't want to trigger any animation
                return;
            }

           float percentage = (current / max) * 100f;
            if(percentage < midThreshold)
            {
                animator.SetTrigger("Small");
            }else if (percentage >= midThreshold && percentage < maxThreshold)
            {
                animator.SetTrigger("Medium");
            }
            else if (percentage >= maxThreshold)
            {
                animator.SetTrigger("High");
            }
        }

        private IEnumerator StartingAnimation() {

            //Zoom in camera
            if (virtualCamera != null)
            {
                virtualCamera.Lens.OrthographicSize = zoomedInSize;
            }

            yield return new WaitForSeconds(0.5f); // Wait for components to be fully initialized

            //Play cleanse animation
            var characterVisual = PlayerManager.Instance.GetPlayerChildComponent<CharacterVisual>();
            characterVisual.PlayCleanseAnimation();

            //Disable player input
            var playerController = PlayerManager.Instance.GetPlayerComponent<PlayerController>();
            playerController.DisableComponentsOnMap();

            //Slowly zoom out camera while animation is playing
            if (virtualCamera != null)
            {
                yield return StartCoroutine(ZoomCamera(zoomedInSize, originalZoom, zoomOutDuration));
            }

            yield return new WaitForSeconds(.8f);

            //Re-enable player input
            playerController.EnableInput();

            //Start fight
            waveHandler.StartPhaseOne();

        }

        private IEnumerator ZoomCamera(float startSize, float endSize, float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(elapsed / duration);

                virtualCamera.Lens.OrthographicSize = Mathf.Lerp(startSize, endSize, t);

                yield return null;
            }

            virtualCamera.Lens.OrthographicSize = endSize;
        }
    }

}