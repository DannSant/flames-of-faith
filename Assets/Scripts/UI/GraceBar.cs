using Game.Combat;
using Game.Scene;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Single Grace bar centered on 0: the positive fill grows to the right while Grace is above 0, the negative
    /// (Corrupted) fill grows to the left while it is below 0. Also shows the Corruption that will be subtracted
    /// at the end of the current wave.
    /// </summary>
    public class GraceBar : MonoBehaviour
    {
        [SerializeField] private float fillSpeed = 5f;
        [SerializeField] TextMeshProUGUI graceText;
        [Tooltip("Filled image for positive Grace (fill origin on the center side).")]
        [SerializeField] private Image graceFillImage;
        [Tooltip("Filled image for negative Grace (fill origin on the center side).")]
        [SerializeField] private Image negativeGraceFillImage;
        [Tooltip("Optional. Shows the Corruption gathered this wave, subtracted from Grace when the wave ends.")]
        [SerializeField] private TextMeshProUGUI pendingCorruptionText;
        [SerializeField] private Color positiveTextColor = Color.white;
        [SerializeField] private Color negativeTextColor = new Color(0.75f, 0.45f, 1f);

        private Coroutine currentRoutine;
        private PlayerGrace playerGrace;
        private PlayerCorruption playerCorruption;

        private void Start()
        {
            if (PlayerManager.Instance == null)
            {
                return;
            }

            playerGrace = PlayerManager.Instance.GetPlayerComponent<PlayerGrace>();
            if (playerGrace != null)
            {
                playerGrace.onGraceChanged += UpdateGrace;
                UpdateGrace(playerGrace.CurrentGrace, playerGrace.MaxGrace);
            }

            playerCorruption = PlayerManager.Instance.GetPlayerComponent<PlayerCorruption>();
            if (playerCorruption != null)
            {
                playerCorruption.OnCorruptionChanged += UpdatePendingCorruption;
                UpdatePendingCorruption(playerCorruption.CorruptionValue);
            }
        }

        private void OnDisable()
        {
            if (playerGrace != null)
            {
                playerGrace.onGraceChanged -= UpdateGrace;
            }
            if (playerCorruption != null)
            {
                playerCorruption.OnCorruptionChanged -= UpdatePendingCorruption;
            }
        }

        // Grace limits are symmetric (-max..+max), so max is enough to scale both sides
        public void UpdateGrace(float current, float max)
        {
            if (graceText != null)
            {
                graceText.text = $"Grace {FormatSigned(current)}";
                graceText.color = current < 0f ? negativeTextColor : positiveTextColor;
            }

            if (max <= 0f) return;

            float positive = Mathf.Clamp01(current / max);
            float negative = Mathf.Clamp01(-current / max);

            if (currentRoutine != null)
            {
                StopCoroutine(currentRoutine);
            }
            currentRoutine = StartCoroutine(AnimateFills(positive, negative));
        }

        private void UpdatePendingCorruption(float corruption)
        {
            if (pendingCorruptionText == null) return;

            bool hasCorruption = corruption > 0f;
            pendingCorruptionText.gameObject.SetActive(hasCorruption);
            if (hasCorruption)
            {
                pendingCorruptionText.text = $"-{corruption:0}";
            }
        }

        private IEnumerator AnimateFills(float positiveTarget, float negativeTarget)
        {
            while (!IsAt(graceFillImage, positiveTarget) || !IsAt(negativeGraceFillImage, negativeTarget))
            {
                StepFill(graceFillImage, positiveTarget);
                StepFill(negativeGraceFillImage, negativeTarget);
                yield return null;
            }
            SetFill(graceFillImage, positiveTarget);
            SetFill(negativeGraceFillImage, negativeTarget);
        }

        private void StepFill(Image image, float target)
        {
            if (image == null) return;
            image.fillAmount = Mathf.Lerp(image.fillAmount, target, Time.deltaTime * fillSpeed);
        }

        private static bool IsAt(Image image, float target)
        {
            return image == null || Mathf.Abs(image.fillAmount - target) <= 0.01f;
        }

        private static void SetFill(Image image, float value)
        {
            if (image != null) image.fillAmount = value;
        }

        private static string FormatSigned(float value)
        {
            return value > 0f ? $"+{value:0}" : value.ToString("0");
        }
    }
}
