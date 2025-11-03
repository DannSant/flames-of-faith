using Game.Combat;
using Game.Scene;
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
            var playerGrace = PlayerManager.Instance.GetPlayerComponent<PlayerGrace>();
            if (playerGrace != null)
            {
                playerGrace.onGraceChanged += UpdateGraceBar;
                UpdateGraceBar(playerGrace.CurrentGrace, playerGrace.MaxGrace);
            }
            else
            {
                Debug.LogWarning("PlayerGrace singleton instance not found!");
            }
        }

        private void OnDisable()
        {
            var playerGrace = PlayerManager.Instance.GetPlayerComponent<PlayerGrace>();
            if (playerGrace != null)
            {
                playerGrace.onGraceChanged -= UpdateGraceBar;
            }
        }

        private void UpdateGraceBar(float current, float max)
        {
            UpdateGraceText(current, max);
            //UpdateGraceBar(current, max, instant: false);
        }

        private void UpdateGraceText(float current, float max) 
        {
            graceText.text = $"{current}/{max}";
        }

        /*private void UpdateGraceBar(int current, int max, bool instant)
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
        }*/
    }
}
