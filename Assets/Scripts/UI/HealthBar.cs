using Game.Combat;
using Game.Scene;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI {
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private float fillSpeed = 5f;
        [SerializeField] private TextMeshProUGUI healthText;

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
            if (PlayerManager.Instance == null)
            {
                return;
            }
            var playerHealth = PlayerManager.Instance.GetPlayerComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.onHealthChanged += UpdateHealthBar;
                UpdateHealthBar(playerHealth.GetCurrentHealth(), playerHealth.GetMaxHealth());
            }
            
        }
        private void OnDisable()
        {
            if (PlayerManager.Instance == null)
            {
                return;
            }
            var playerHealth = PlayerManager.Instance.GetPlayerComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.onHealthChanged -= UpdateHealthBar;
            }
        }
        public void UpdateHealthBar(float current, float max)
        {
            
            UpdateHealthBar(current, max, instant: false);
        }

        private void UpdateHealthBar(float current, float max, bool instant)
        {
            if (healthSlider == null || max <= 0) return;

            if(healthText == null)
            {
                Debug.LogWarning("HealthText is not assigned in HealthBar.");
                return;
            }

            float targetValue = current;

            if (currentRoutine != null)
            {
                StopCoroutine(currentRoutine);
            }
           

            healthSlider.maxValue = max;

            if (instant)
            {
                healthSlider.value = targetValue;
                healthText.text = FormatHealth(current, max);
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
                healthText.text = FormatHealth(healthSlider.value, healthSlider.maxValue);
                yield return null;
            }

            healthSlider.value = target; // Final snap
            healthText.text = FormatHealth(target, healthSlider.maxValue);
        }

        // Health can be fractional; round up so the player never reads 0 while still alive.
        // The small tolerance keeps float error like 10.00001 from showing as 11.
        private static string FormatHealth(float current, float max)
        {
            return $"{Mathf.Max(0, Mathf.CeilToInt(current - 0.001f))}/{max:0}";
        }

    }
}
