using Game.Combat;
using Game.Scene;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class CorruptionBar : MonoBehaviour
    {
        [SerializeField] private float fillSpeed = 5f;
        [Tooltip("Corruption value that fills the bar completely.")]
        [SerializeField] private float maxDisplayedCorruption = 10f;
        [SerializeField] TextMeshProUGUI corruptionText;
        [SerializeField] private Image corruptionFillImage;

        private Coroutine currentRoutine;

        private PlayerCorruption playerCorruption;

        private void Start()
        {
            if(PlayerManager.Instance== null)
            {
                return;
            }

            playerCorruption = PlayerManager.Instance.GetPlayerComponent<PlayerCorruption>();
            if (playerCorruption != null)
            {
                playerCorruption.OnCorruptionChanged += UpdateCorruption;
                UpdateCorruption(playerCorruption.CorruptionValue);
            }
        }

        private void OnDisable()
        {
            if (playerCorruption != null)
            {
                playerCorruption.OnCorruptionChanged -= UpdateCorruption;
            }
        }

        public void UpdateCorruption(float corruptionLevel)
        {
            corruptionText.text = $"Corruption: {corruptionLevel}";
            float normalizedCorruption = maxDisplayedCorruption > 0f ? Mathf.Min(corruptionLevel / maxDisplayedCorruption, 1.0f) : 0f;
            UpdateCorruptionBar(normalizedCorruption);
        }

        private void UpdateCorruptionBar(float current)
        {
            if (corruptionFillImage == null) return;
            float targetValue = current;
            if (currentRoutine != null)
            {
                StopCoroutine(currentRoutine);
            }
            currentRoutine = StartCoroutine(FillCorruptionBarRoutine(targetValue));
        }

        private IEnumerator FillCorruptionBarRoutine(float targetValue)
        {
            while (Mathf.Abs(corruptionFillImage.fillAmount - targetValue) > 0.01f)
            {
                corruptionFillImage.fillAmount = Mathf.Lerp(corruptionFillImage.fillAmount, targetValue, Time.deltaTime * fillSpeed);
                yield return null;
            }
            corruptionFillImage.fillAmount = targetValue; // Final snap
        }
    }
}
