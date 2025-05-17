using Game.Combat;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class GraceBar : MonoBehaviour
    {
        [SerializeField] private float fillSpeed = 5f;
        [SerializeField] TextMeshProUGUI graceText;
        private Slider graceSlider;
        private Coroutine currentRoutine;
        private void Awake()
        {
            graceSlider = GetComponent<Slider>();
            if (graceSlider == null)
            {
                Debug.LogError("GraceBar script requires an Slider component.");
                enabled = false; // Disable this script if no Image component is found
            }
        }
        private void Start()
        {
            if (PlayerGrace.Instance != null)
            {
                PlayerGrace.Instance.onGraceChanged += UpdateGraceBar;
                UpdateGraceBar(PlayerGrace.Instance.CurrentGrace, PlayerGrace.Instance.MaxGrace);
            }
            else
            {
                Debug.LogWarning("PlayerGrace singleton instance not found!");
            }
        }
        private void UpdateGraceBar(int current, int max)
        {
            UpdateGraceText(current, max);
            UpdateGraceBar(current, max, instant: false);
        }

        private void UpdateGraceText(int current, int max) 
        {
            graceText.text = $"{current}/{max}";
        }

        private void UpdateGraceBar(int current, int max, bool instant)
        {
           
            if (graceSlider == null || max <= 0) return;

            float targetValue = current;

            if (currentRoutine != null)
            {
                StopCoroutine(currentRoutine);
            }

            graceSlider.value = targetValue;
            graceSlider.maxValue = max;
        }

        private IEnumerator AnimateSliderValue(float target)
        {
            Debug.Log("GraceBar: Animation started. Target value: " + target);
            while (Mathf.Abs(graceSlider.value - target) > 0.01f)
            {
                graceSlider.value = Mathf.Lerp(graceSlider.value, target, Time.deltaTime * fillSpeed);
                yield return null;
            }
            Debug.Log("GraceBar: Animation complete. Final value: " + target);
            graceSlider.value = target; // Final snap
        }
    }
}
