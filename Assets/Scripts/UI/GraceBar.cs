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
        [Tooltip("Hide the pending Corruption label while it is 0. Keep it visible so its tooltip can always be reached.")]
        [SerializeField] private bool hidePendingCorruptionWhenZero = false;
        [Tooltip("Hide the pending Corruption label on the overworld map, where there are no waves.")]
        [SerializeField] private bool hidePendingCorruptionOnMap = true;

        [Header("Tooltips")]
        [SerializeField] private string graceTooltipTitle = "Grace";
        [Tooltip("{0} = damage bonus in percent.")]
        [TextArea]
        [SerializeField] private string graceTooltip = "Your Grace increases your total damage by {0}% and protects you from Corruption.";
        [Tooltip("{0} = armor lost, {1} = damage lost in percent, {2} = healing lost per heal.")]
        [TextArea]
        [SerializeField] private string corruptedTooltip = "You are Corrupted: armor -{0}, damage -{1}%, healing received -{2} per heal.";
        [SerializeField] private string corruptionTooltipTitle = "Corruption";
        [TextArea]
        [SerializeField] private string corruptionTooltip = "Corruption gathered this wave. When the wave ends it is subtracted from your Grace.";

        private Coroutine currentRoutine;
        private PlayerGrace playerGrace;
        private PlayerCorruption playerCorruption;
        private GraceStatus graceStatus;

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
                playerGrace.OnGraceStatusChanged += UpdateGraceStatus;
                UpdateGrace(playerGrace.CurrentGrace, playerGrace.MaxGrace);
                UpdateGraceStatus(playerGrace.GetStatus());
            }

            SetupTooltips();

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
                playerGrace.OnGraceStatusChanged -= UpdateGraceStatus;
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

            bool onMap = PlayerManager.Instance != null && PlayerManager.Instance.IsPlayerOnMap;
            if (onMap && hidePendingCorruptionOnMap)
            {
                pendingCorruptionText.gameObject.SetActive(false);
                return;
            }

            bool hasCorruption = corruption > 0f;
            pendingCorruptionText.gameObject.SetActive(hasCorruption || !hidePendingCorruptionWhenZero);
            pendingCorruptionText.text = hasCorruption ? $"-{corruption:0}" : "0";
        }

        private void UpdateGraceStatus(GraceStatus status)
        {
            graceStatus = status;
        }

        // Hover (mouse) or select (gamepad, from the pause menu) the labels to explain what they mean
        private void SetupTooltips()
        {
            if (graceText != null)
            {
                graceText.gameObject.AddComponent<TooltipTriggerUI>().Setup(graceTooltipTitle, BuildGraceTooltip);
            }
            if (pendingCorruptionText != null)
            {
                pendingCorruptionText.gameObject.AddComponent<TooltipTriggerUI>().Setup(corruptionTooltipTitle, () => corruptionTooltip);
            }
        }

        private string BuildGraceTooltip()
        {
            if (graceStatus.isCorrupted)
            {
                return string.Format(corruptedTooltip,
                    graceStatus.armorPenalty.ToString("0"),
                    (-graceStatus.damagePercent).ToString("0"),
                    graceStatus.healingPenalty.ToString("0"));
            }
            return string.Format(graceTooltip, graceStatus.damagePercent.ToString("0"));
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
