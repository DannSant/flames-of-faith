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
        [SerializeField] TextMeshProUGUI corruptionText;
        [SerializeField] private Image corruptionFillImage;

        private Coroutine currentRoutine;

        private PlayerCorruption playerCorruption;
        private PlayerGrace playerGrace;

        private void Start()
        {           

            playerGrace = PlayerManager.Instance.GetPlayerComponent<PlayerGrace>();
            if (playerGrace != null)
            {
                playerGrace.onGraceChanged += OnGraceUpdated;
            }

            playerCorruption = PlayerManager.Instance.GetPlayerComponent<PlayerCorruption>();
            if (playerCorruption != null)
            {
                playerCorruption.OnCorruptionChanged += UpdateCorruption;
                UpdateCorruption(playerCorruption.CorruptionValue, playerCorruption.CalculateCorruptionEffectLevel(), playerGrace != null ? playerGrace.MaxGrace : 1.0f);
            }
            else
            {
                Debug.LogWarning("PlayerCorruption component not found!");
            }
        }

        private void OnDisable()
        {
            if (playerCorruption != null)
            {
                playerCorruption.OnCorruptionChanged -= UpdateCorruption;
            }
           
            if (playerGrace != null)
            {
                playerGrace.onGraceChanged -= OnGraceUpdated;
            }
        }

        private void OnGraceUpdated (float currentGrace,float maxGrace)
        {
            if (playerCorruption != null)
            {
                UpdateCorruption(playerCorruption.CorruptionValue, playerCorruption.CalculateCorruptionEffectLevel(), maxGrace);
            }
        }

        private void UpdateCorruption(float corruptionLevel, float actualCorruption, float maxGrace)
        {
            UpdateCorruptionText(corruptionLevel, actualCorruption);
            float normalizedCorruption = Mathf.Min(corruptionLevel / maxGrace,1.0f);
            UpdateCorruptionBar(normalizedCorruption);


        }

        private void UpdateCorruptionText(float corruptionLevel, float actualCorruption)
        {
            corruptionText.text = $"Corruption: {actualCorruption} ({corruptionLevel})";
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