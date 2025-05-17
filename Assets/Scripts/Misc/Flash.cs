using System.Collections;
using UnityEngine;

namespace Game.Misc
{
    public class Flash : MonoBehaviour
    {
        [SerializeField] private Material flashMaterial;
        [SerializeField] private float flashDuration = 0.1f;

        private Material defaultMaterial;
        private SpriteRenderer spriteRenderer;
        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            defaultMaterial = spriteRenderer.material;

        }

        public void StartFlash()
        {
            // Start the flash coroutine
            StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            // Set the flash material
            spriteRenderer.material = flashMaterial; 
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.material = defaultMaterial;
        }
    }

}