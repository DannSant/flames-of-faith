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
        [SerializeField] private Image graceFillImage;
        //private Slider graceSlider;
        private Coroutine currentRoutine;
       
        private void Start()
        {
            var playerGrace = PlayerManager.Instance.GetPlayerComponent<PlayerGrace>();
            if (playerGrace != null)
            {
                playerGrace.onGraceChanged += UpdateGrace;
                UpdateGrace(playerGrace.CurrentGrace, playerGrace.MaxGrace);
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
                playerGrace.onGraceChanged -= UpdateGrace;
            }
        }

        private void UpdateGrace(float current, float max)
        {
            UpdateGraceText(current, max);
            UpdateGraceBar(current, max);
        }

        private void UpdateGraceText(float current, float max) 
        {
            graceText.text = $"Grace: {current}/{max}";
        }

        private void UpdateGraceBar(float current, float max)
        {
           
            if (graceFillImage == null || max <= 0) return;

            float targetValue = current;

            if (currentRoutine != null)
            {
                StopCoroutine(currentRoutine);
            }

            currentRoutine = StartCoroutine(AnimateSliderValue(targetValue / max));


        }

        private IEnumerator AnimateSliderValue(float target)
        {
            //Debug.Log("GraceBar: Animation started. Target value: " + target);
            while (Mathf.Abs(graceFillImage.fillAmount - target) > 0.01f)
            {
                graceFillImage.fillAmount = Mathf.Lerp(graceFillImage.fillAmount, target, Time.deltaTime * fillSpeed);
                yield return null;
            }
            //Debug.Log("GraceBar: Animation complete. Final value: " + target);
            graceFillImage.fillAmount = target; // Final snap
        }
    }
}
