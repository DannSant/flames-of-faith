using Game.Combat;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI {
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private float fillSpeed = 5f;

        private Slider healthSlider;
        private Coroutine currentRoutine;

        private void Awake()
        {
            healthSlider = GetComponent<Slider>();
            if (healthSlider == null)
            {
                Debug.LogError("HealthBar script requires an Slider component.");
                enabled = false; // Disable this script if no Image component is found
            }
        }
        private void Start()
        {
            if (PlayerHealth.Instance != null)
            {
                PlayerHealth.Instance.onHealthChanged += UpdateHealthBar;
                UpdateHealthBar(PlayerHealth.Instance.GetCurrentHealth(), PlayerHealth.Instance.GetMaxHealth());
            }
            else
            {
                Debug.LogWarning("PlayerHealth singleton instance not found!");
            }
        }
        private void UpdateHealthBar(int current, int max)
        {
            UpdateHealthBar(current, max, instant: false);
        }

        private void UpdateHealthBar(int current, int max, bool instant)
        {
            if (healthSlider == null || max <= 0) return;

            float targetValue = current;

            if (currentRoutine != null)
            {
                StopCoroutine(currentRoutine);
            }
           

            healthSlider.maxValue = max;

            if (instant)
            {
                healthSlider.value = targetValue;                
            }
            else
            {
                currentRoutine = StartCoroutine(AnimateSliderValue(targetValue));
            }
        }

        private IEnumerator AnimateSliderValue(float target)
        {
            while (Mathf.Abs(healthSlider.value - target) > 0.01f)
            {
                healthSlider.value = Mathf.Lerp(healthSlider.value, target, Time.deltaTime * fillSpeed);
                yield return null;
            }

            healthSlider.value = target; // Final snap
        }

    }
}
